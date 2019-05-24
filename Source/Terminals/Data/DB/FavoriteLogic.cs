using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Terminals.Common.Connections;
using Terminals.Converters;

namespace Terminals.Data.DB
{
    internal partial class DbFavorite : IFavorite, IIntegerKeyEnityty
    {
        private StoredCredentials credentials;

        private Groups groups;

        // for backward compatibility with the file persistence only
        private Guid guid;

        private int id;

        /// <summary>
        ///     cant be set in constructor, because the constructor is used by EF when loading the entities
        /// </summary>
        private bool isNewlyCreated;

        /// <summary>
        ///     Should be never null to prevent access violations
        /// </summary>
        private ProtocolOptions protocolProperties;

        // because of the disposable image, favorite should implement IDisposable

        /// <summary>
        ///     Initializes new instance of a favorite and sets its properties to default values,
        ///     which aren't defined by database.
        /// </summary>
        public DbFavorite()
        {
            this.Groups = new HashSet<DbGroup>();
            this.Port = KnownConnectionConstants.RDPPort;
            this.ChangeProtocol(KnownConnectionConstants.RDP, new EmptyOptions());
            this.Details = new FavoriteDetails(this);
        }

        internal FavoriteDetails Details { get; }

        internal Guid Guid
        {
            get
            {
                if (this.guid == Guid.Empty)
                    this.guid = GuidConverter.ToGuid(this.Id);

                return this.guid;
            }
        }

        /// <summary>
        ///     Gets empty string. Set loads the image from file and updates the icon reference in database.
        ///     The string get/set image file path to import/export favorite icon isn't supported in database persistence.
        /// </summary>
        public string ToolBarIconFile { get => string.Empty; set { } }

        public Image ToolBarIconImage { get; set; }

        Guid IFavorite.Id => this.Guid;

        IBeforeConnectExecuteOptions IFavorite.ExecuteBeforeConnect
        {
            get
            {
                this.Details.Load();
                return this.Details.ExecuteBeforeConnect;
            }
        }

        IDisplayOptions IFavorite.Display
        {
            get
            {
                this.Details.Load();
                return this.Details.Display;
            }
        }

        ISecurityOptions IFavorite.Security => this.GetSecurity();

        List<IGroup> IFavorite.Groups => this.GetInvariantGroups();

        /// <summary>
        ///     Gets or sets the protocol specific container. This isn't a part of an entity,
        ///     because we are using lazy loading of this property and we don't want to cache
        ///     its xml persisted content.
        /// </summary>
        public ProtocolOptions ProtocolProperties
        {
            get
            {
                this.Details.LoadProtocolProperties();
                return this.protocolProperties;
            }
        }

        public string GroupNames
        {
            get
            {
                var loadedGroups = this.GetInvariantGroups();
                return Favorite.GroupsListToString(loadedGroups);
            }
        }

        public string Protocol { get; set; }

        IFavorite IFavorite.Copy()
        {
            var copy = Factory.CreateFavorite(this.groups, this.credentials, this.Details.Dispatcher);
            copy.UpdateFrom(this);
            return copy;
        }

        void IFavorite.UpdateFrom(IFavorite source)
        {
            var sourceFavorite = source as DbFavorite;
            if (sourceFavorite == null)
                return;
            this.UpdateFrom(sourceFavorite);
        }

        bool IStoreIdEquals<IFavorite>.StoreIdEquals(IFavorite oponent)
        {
            var oponentFavorite = oponent as DbFavorite;
            if (oponentFavorite == null)
                return false;

            return oponentFavorite.Id == this.Id;
        }

        public int GetStoreIdHash()
        {
            return this.Id.GetHashCode();
        }

        public int Id
        {
            get => this.id;
            set
            {
                this.id = value;
                this.guid = GuidConverter.ToGuid(value);
            }
        }

        private DbSecurityOptions GetSecurity()
        {
            this.Details.Load();
            // returns null, if the favorite details loading failed.
            // the same for all other detail properties
            return this.Details.Security;
        }

        internal void MarkAsNewlyCreated()
        {
            this.isNewlyCreated = true;
            this.Details.LoadFieldsFromReferences();
        }

        private void UpdateFrom(DbFavorite source)
        {
            // force load first to fill the content, otherwise we don't have to able to copy
            this.Details.Load();
            source.Details.Load();
            // force the protocol to be loaded, in case the source wasnt accessed yet.
            source.Details.LoadProtocolProperties();

            this.DesktopShare = source.DesktopShare;
            // we cant copy the fields, because they are also dependent on the favorite Id
            this.Details.UpdateFrom(source.Details);
            this.Name = source.Name;
            this.NewWindow = source.NewWindow;
            this.Notes = source.Notes;
            this.Port = source.Port;
            this.ServerName = source.ServerName;
            this.ToolBarIconImage = source.ToolBarIconImage;
            // protocolProperties don't have a favorite Id reference, so we can overwrite complete content
            var sourceProperties = source.protocolProperties.Copy();
            this.ChangeProtocol(source.Protocol, sourceProperties);
            this.AssignStores(source.groups, source.credentials, source.Details.Dispatcher);
        }

        private List<IGroup> GetInvariantGroups()
        {
            // see also the Group.Favorites
            // prefer to select cached items, instead of selecting from database directly
            return this.groups.GetGroupsContainingFavorite(this.Id)
                .Cast<IGroup>()
                .ToList();
        }

        internal void AssignStores(Groups groups, StoredCredentials credentials, DataDispatcher dispatcher)
        {
            this.groups = groups;
            this.credentials = credentials;
            this.Details.Dispatcher = dispatcher;
        }

        internal void SaveDetails(Database database)
        {
            this.Details.Save(database);
        }

        internal void ReleaseLoadedDetails()
        {
            this.Details.ReleaseLoadedDetails();
        }

        public override string ToString()
        {
            return Favorite.ToString(this);
        }
    }
}