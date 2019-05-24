using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Terminals.Configuration;
using Terminals.Connections;
using Terminals.Data.FilePersisted;
using Terminals.History;

namespace Terminals.Data
{
    internal class FilePersistence : IPersistence
    {
        /// <summary>
        ///     Gets unique id of the persistence to be stored in settings (0)
        /// </summary>
        internal const int TYPE_ID = 0;

        private readonly ConnectionHistory connectionHistory;

        private readonly SerializationContextBuilder contextBuilder;

        private readonly Factory factory;

        private readonly Favorites favorites;

        private readonly FileLocations fileLocations;

        private readonly Mutex fileLock = new Mutex(false, "Terminals.CodePlex.com.FilePersistence");

        private readonly FavoritesFileSerializer serializer;

        private readonly StoredCredentials storedCredentials;

        private UnknonwPluginElements cachedUnknown = new UnknonwPluginElements();

        private bool delaySave;

        private IDataFileWatcher fileWatcher;

        /// <summary>
        ///     Try to reuse current security in case of changing persistence, because user is already authenticated
        /// </summary>
        internal FilePersistence(PersistenceSecurity security, FavoriteIcons favoriteIcons,
            ConnectionManager connectionManager)
            : this(security, new DataFileWatcher(Settings.Instance.FileLocations.Favorites),
                favoriteIcons, connectionManager)
        {
        }

        /// <summary>
        ///     For testing purpose allowes to inject internaly used services
        /// </summary>
        internal FilePersistence(PersistenceSecurity security, IDataFileWatcher fileWatcher,
            FavoriteIcons favoriteIcons, ConnectionManager connectionManager)
        {
            this.fileLocations = Settings.Instance.FileLocations;
            this.serializer = new FavoritesFileSerializer(connectionManager);
            this.Security = security;
            this.Dispatcher = new DataDispatcher();
            this.storedCredentials = new StoredCredentials(security);
            this.GroupsStore = new Groups(this);
            this.favorites = new Favorites(this, favoriteIcons, connectionManager);
            this.connectionHistory = new ConnectionHistory(this.favorites);
            this.factory = new Factory(this.GroupsStore, this.Dispatcher, connectionManager);
            this.contextBuilder = new SerializationContextBuilder(this.GroupsStore, this.favorites, this.Dispatcher);
            this.InitializeFileWatch(fileWatcher);
        }

        internal Groups GroupsStore { get; }

        public int TypeId => TYPE_ID;

        public string Name => "Files";

        public IFavorites Favorites => this.favorites;

        public IGroups Groups => this.GroupsStore;

        public IFactory Factory => this.factory;

        public ICredentials Credentials => this.storedCredentials;

        public IConnectionHistory ConnectionHistory => this.connectionHistory;

        public DataDispatcher Dispatcher { get; }

        public PersistenceSecurity Security { get; }

        public void AssignSynchronizationObject(ISynchronizeInvoke synchronizer)
        {
            Settings.Instance.AssignSynchronizationObject(synchronizer);
            this.connectionHistory.AssignSynchronizationObject(synchronizer);
            this.storedCredentials.AssignSynchronizationObject(synchronizer);
            this.fileWatcher.AssignSynchronizer(synchronizer);
        }

        public void StartDelayedUpdate()
        {
            this.delaySave = true;
            this.Dispatcher.StartDelayedUpdate();
        }

        public void SaveAndFinishDelayedUpdate()
        {
            this.delaySave = false;
            this.SaveImmediatelyIfRequested();
            this.Dispatcher.EndDelayedUpdate();
        }

        public bool Initialize()
        {
            this.storedCredentials.Initialize();
            var file = this.LoadFile();
            this.GroupsStore.AddAllToCache(file.Groups.Cast<IGroup>().ToList());
            this.favorites.AddAllToCache(file.Favorites.Cast<IFavorite>().ToList());
            this.UpdateFavoritesInGroups(file.FavoritesInGroups);
            this.Security.OnUpdatePasswordsByNewMasterPassword += this.UpdatePasswordsByNewMasterPassword;
            return true;
        }

        private void InitializeFileWatch(IDataFileWatcher fileWatcher)
        {
            this.fileWatcher = fileWatcher;
            this.fileWatcher.FileChanged += this.FavoritesFileChanged;
            this.fileWatcher.StartObservation();
        }

        private void FavoritesFileChanged(object sender, EventArgs e)
        {
            var file = this.LoadFile();
            // dont report changes immediately, we have to wait till memberships are refreshed properly
            this.Dispatcher.StartDelayedUpdate();
            var addedGroups = this.GroupsStore.Merge(file.Groups.Cast<IGroup>().ToList());
            this.favorites.Merge(file.Favorites.Cast<IFavorite>().ToList());
            // first update also present groups assignment,
            // than send the favorite update also for present favorites
            var updated = this.UpdateFavoritesInGroups(file.FavoritesInGroups);
            updated = ListsHelper.GetMissingSourcesInTarget(updated, addedGroups);
            this.Dispatcher.ReportGroupsUpdated(updated);
            this.Dispatcher.EndDelayedUpdate();
        }

        internal void UpdatePasswordsByNewMasterPassword(string newMasterKey)
        {
            this.storedCredentials.UpdatePasswordsByNewKeyMaterial(newMasterKey);
            this.favorites.UpdatePasswordsByNewMasterPassword(newMasterKey);
            this.SaveImmediatelyIfRequested();
        }

        internal void SaveImmediatelyIfRequested()
        {
            if (!this.delaySave)
                this.Save();
        }

        private FavoritesFile LoadFile()
        {
            var loaded = this.TryLoadFile();
            this.cachedUnknown = loaded.Unknown;
            this.contextBuilder.AssignServices(loaded.File);
            return loaded.File;
        }

        private SerializationContext TryLoadFile()
        {
            try
            {
                this.fileLock.WaitOne();
                var fileLocation = this.fileLocations.Favorites;
                return this.serializer.Deserialize(fileLocation);
            }
            catch (Exception exception)
            {
                Logging.Error("File persistence was unable to load Favorites.xml", exception);
                return new SerializationContext();
            }
            finally
            {
                this.fileLock.ReleaseMutex();
                Debug.WriteLine("Favorite file was loaded.");
            }
        }

        private List<IGroup> UpdateFavoritesInGroups(FavoritesInGroup[] favoritesInGroups)
        {
            var updatedGroups = new List<IGroup>();

            foreach (var favoritesInGroup in favoritesInGroups)
            {
                var group = this.GroupsStore[favoritesInGroup.GroupId] as Group;
                var groupUpdated = this.UpdateFavoritesInGroup(group, favoritesInGroup.Favorites);
                if (groupUpdated)
                    updatedGroups.Add(group);
            }

            return updatedGroups;
        }

        private bool UpdateFavoritesInGroup(Group group, Guid[] favoritesInGroup)
        {
            if (group != null)
            {
                var newFavorites = this.GetFavoritesInGroup(favoritesInGroup);
                return group.UpdateFavorites(newFavorites);
            }

            return false;
        }

        private List<IFavorite> GetFavoritesInGroup(Guid[] favoritesInGroup)
        {
            return this.Favorites
                .Where(favorite => favoritesInGroup.Contains(favorite.Id))
                .ToList();
        }

        private void Save()
        {
            var context = this.contextBuilder.CreateDataFromCache(this.cachedUnknown);
            this.TrySave(context);
        }

        private void TrySave(SerializationContext context)
        {
            try
            {
                this.fileLock.WaitOne();
                this.fileWatcher.StopObservation();
                var fileLocation = this.fileLocations.Favorites;
                this.serializer.Serialize(context, fileLocation);
            }
            catch (Exception exception)
            {
                Logging.Error("File persistence was unable to save Favorites.xml", exception);
            }
            finally
            {
                this.fileWatcher.StartObservation();
                this.fileLock.ReleaseMutex();
                Debug.WriteLine("Favorite file was saved.");
            }
        }
    }
}