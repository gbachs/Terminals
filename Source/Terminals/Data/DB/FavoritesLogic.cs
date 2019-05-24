using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Drawing;
using System.Linq;
using System.Transactions;
using Terminals.Connections;

namespace Terminals.Data.DB
{
    /// <summary>
    ///     SQL persisted favorites container
    /// </summary>
    internal class Favorites : IFavorites, IEnumerable<DbFavorite>
    {
        private readonly FavoritesBatchActions batchActions;

        private readonly EntitiesCache<DbFavorite> cache = new EntitiesCache<DbFavorite>();

        private readonly ConnectionManager connectionManager;

        private readonly StoredCredentials credentials;

        private readonly DataDispatcher dispatcher;

        private readonly DbFavoriteImagesStore favoriteIcons;

        private readonly Groups groups;

        private bool isLoaded;

        internal Favorites(SqlPersistence persistence, Groups groups, StoredCredentials credentials,
            ConnectionManager connectionManager, FavoriteIcons favoriteIcons)
        {
            this.groups = groups;
            this.credentials = credentials;
            this.dispatcher = persistence.Dispatcher;
            this.connectionManager = connectionManager;
            this.favoriteIcons = new DbFavoriteImagesStore(this.dispatcher, favoriteIcons);
            this.batchActions = new FavoritesBatchActions(this, this.cache, persistence);
        }

        private List<DbFavorite> Cached => this.cache.ToList();

        IFavorite IFavorites.this[Guid favoriteId]
        {
            get
            {
                this.EnsureCache();
                return this.cache.FirstOrDefault(favorite => favorite.Guid == favoriteId);
            }
        }

        IFavorite IFavorites.this[string favoriteName]
        {
            get
            {
                this.EnsureCache();
                return this.cache.FirstOrDefault(favorite =>
                    favorite.Name.Equals(favoriteName, StringComparison.CurrentCultureIgnoreCase));
            }
        }

        public void Add(IFavorite favorite)
        {
            var favoritesToAdd = new List<IFavorite> {favorite};
            this.Add(favoritesToAdd);
        }

        public void Add(List<IFavorite> favorites)
        {
            try
            {
                this.TryAdd(favorites);
            }
            catch (EntityException exception)
            {
                this.dispatcher.ReportActionError(this.Add, favorites, this, exception,
                    "Unable to add favorite to database.");
            }
        }

        public void Update(IFavorite favorite)
        {
            try
            {
                this.TryUpdateFavorite(favorite);
            }
            catch (EntityException exception)
            {
                this.dispatcher.ReportActionError(this.Update, favorite, this, exception,
                    "Unable to update favorite in database");
            }
        }

        public void UpdateFavorite(IFavorite favorite, List<IGroup> newGroups)
        {
            try
            {
                using (var transaction = new TransactionScope())
                {
                    // do it in transaction, because here we do more things at once
                    this.TryUpdateFavorite(favorite, newGroups);
                    transaction.Complete();
                }
            }
            catch (EntityException exception)
            {
                this.dispatcher.ReportActionError(this.UpdateFavorite, favorite, newGroups, this, exception,
                    "Unable to update favorite and its groups in database.");
            }
        }

        public void Delete(IFavorite favorite)
        {
            var favoritesToDelete = new List<IFavorite> {favorite};
            this.Delete(favoritesToDelete);
        }

        public void Delete(List<IFavorite> favorites)
        {
            try
            {
                this.TryDeleteInTransaction(favorites);
            }
            catch (DbUpdateException) // item already removed
            {
                var toRemove = favorites.Cast<DbFavorite>().ToList();
                this.FinishRemove(favorites, toRemove);
            }
            catch (EntityException exception)
            {
                this.dispatcher.ReportActionError(this.Delete, favorites, this, exception,
                    "Unable to delete favorites from database");
            }
        }

        public SortableList<IFavorite> ToListOrderedByDefaultSorting()
        {
            return Data.Favorites.OrderByDefaultSorting(this);
        }

        public void ApplyCredentialsToAllFavorites(List<IFavorite> selectedFavorites, ICredentialSet credential)
        {
            try
            {
                this.TryApplyCredentials(selectedFavorites, credential);
            }
            catch (EntityException exception)
            {
                this.dispatcher.ReportActionError(this.ApplyCredentialsToAllFavorites, selectedFavorites, credential,
                    this, exception, "Unable to set credentials on favorites.");
            }
        }

        public void SetPasswordToAllFavorites(List<IFavorite> selectedFavorites, string newPassword)
        {
            this.batchActions.SetPasswordToFavorites(selectedFavorites, newPassword);
        }

        public void ApplyDomainNameToAllFavorites(List<IFavorite> selectedFavorites, string newDomainName)
        {
            this.batchActions.ApplyDomainNameToFavorites(selectedFavorites, newDomainName);
        }

        public void ApplyUserNameToAllFavorites(List<IFavorite> selectedFavorites, string newUserName)
        {
            this.batchActions.ApplyUserNameToFavorites(selectedFavorites, newUserName);
        }

        public void UpdateFavoriteIcon(IFavorite favorite, string imageFilePath)
        {
            var toUpdate = favorite as DbFavorite;
            if (toUpdate == null)
                return;

            this.favoriteIcons.AssingNewIcon(toUpdate, imageFilePath);
        }

        public Image LoadFavoriteIcon(IFavorite favorite)
        {
            var toUpdate = favorite as DbFavorite;
            this.favoriteIcons.LoadImageFromDatabase(toUpdate);
            return toUpdate.ToolBarIconImage;
        }

        private void TryAdd(List<IFavorite> favorites)
        {
            using (var database = DatabaseConnections.CreateInstance())
            {
                var toAdd = favorites.Cast<DbFavorite>().ToList();
                database.AddAll(toAdd);
                this.UpdateIconInDatabase(database, toAdd);
                database.SaveImmediatelyIfRequested();
                database.Cache.DetachAll(toAdd);
                this.cache.Add(toAdd);
                this.dispatcher.ReportFavoritesAdded(favorites);
            }
        }

        private void UpdateIconInDatabase(Database database, List<DbFavorite> favorites)
        {
            foreach (var toUpdate in favorites)
                this.favoriteIcons.UpdateImageInDatabase(toUpdate, database);
        }

        private void TryUpdateFavorite(IFavorite favorite)
        {
            using (var database = DatabaseConnections.CreateInstance())
            {
                var toUpdate = favorite as DbFavorite;
                database.Cache.AttachFavorite(toUpdate);
                this.TrySaveAndReportFavoriteUpdate(toUpdate, database);
            }
        }

        private void TryUpdateFavorite(IFavorite favorite, List<IGroup> newGroups)
        {
            using (var database = DatabaseConnections.CreateInstance())
            {
                var toUpdate = favorite as DbFavorite;
                database.Cache.AttachFavorite(toUpdate);
                var addedGroups = database.AddToDatabase(newGroups);
                // commit newly created groups, otherwise we cant add into them
                database.SaveImmediatelyIfRequested();
                UpdateGroupsMembership(favorite, newGroups);
                database.SaveImmediatelyIfRequested();

                this.dispatcher.ReportGroupsAdded(addedGroups);
                this.TrySaveAndReportFavoriteUpdate(toUpdate, database);
            }
        }

        private static void UpdateGroupsMembership(IFavorite favorite, List<IGroup> newGroups)
        {
            var redundantGroups = ListsHelper.GetMissingSourcesInTarget(favorite.Groups, newGroups);
            var missingGroups = ListsHelper.GetMissingSourcesInTarget(newGroups, favorite.Groups);
            Data.Favorites.AddIntoMissingGroups(favorite, missingGroups);
            Data.Groups.RemoveFavoritesFromGroups(new List<IFavorite> {favorite}, redundantGroups);
        }

        private void TrySaveAndReportFavoriteUpdate(DbFavorite toUpdate, Database database)
        {
            try
            {
                this.SaveAndReportFavoriteUpdated(database, toUpdate);
            }
            catch (DbUpdateException)
            {
                this.TryToRefreshUpdatedFavorite(toUpdate, database);
            }
        }

        private void TryToRefreshUpdatedFavorite(DbFavorite toUpdate, Database database)
        {
            try
            {
                database.RefreshEntity(toUpdate);
                this.SaveAndReportFavoriteUpdated(database, toUpdate);
            }
            catch (InvalidOperationException)
            {
                this.cache.Delete(toUpdate);
                this.dispatcher.ReportFavoriteDeleted(toUpdate);
            }
        }

        private void SaveAndReportFavoriteUpdated(Database database, DbFavorite favorite)
        {
            database.Cache.MarkFavoriteAsModified(favorite);
            database.SaveImmediatelyIfRequested();
            this.favoriteIcons.UpdateImageInDatabase(favorite, database);
            database.Cache.DetachFavorite(favorite);
            this.cache.Update(favorite);
            this.dispatcher.ReportFavoriteUpdated(favorite);
        }

        private void TryDeleteInTransaction(List<IFavorite> favorites)
        {
            using (var transaction = new TransactionScope())
            {
                this.TryDelete(favorites);
                transaction.Complete();
            }
        }

        private void TryDelete(List<IFavorite> favorites)
        {
            using (var database = DatabaseConnections.CreateInstance())
            {
                var favoritesToDelete = favorites.Cast<DbFavorite>().ToList();
                var redundantCredentialBase = SelectRedundantCredentialBase(favoritesToDelete);
                this.DeleteFavoritesFromDatabase(database, favoritesToDelete);
                database.SaveImmediatelyIfRequested();
                database.RemoveRedundantCredentialBase(redundantCredentialBase);
                database.SaveImmediatelyIfRequested();
                this.groups.RefreshCache();
                this.FinishRemove(favorites, favoritesToDelete);
            }
        }

        private static List<DbCredentialBase> SelectRedundantCredentialBase(IEnumerable<DbFavorite> favorites)
        {
            return favorites.Where(f => f.Details.CredentialBaseToRemove != null)
                .Select(f => f.Details.CredentialBaseToRemove)
                .ToList();
        }

        private void FinishRemove(List<IFavorite> favorites, List<DbFavorite> favoritesToDelete)
        {
            this.cache.Delete(favoritesToDelete);
            this.dispatcher.ReportFavoritesDeleted(favorites);
        }

        private void DeleteFavoritesFromDatabase(Database database, List<DbFavorite> favorites)
        {
            // we don't have to attach the details, because they will be deleted by reference constraints
            database.Cache.AttachAll(favorites);
            database.DeleteAll(favorites);
        }

        private void TryApplyCredentials(List<IFavorite> selectedFavorites, ICredentialSet credential)
        {
            using (var database = DatabaseConnections.CreateInstance())
            {
                var dbFavorites = selectedFavorites.Cast<DbFavorite>().ToList();
                Data.Favorites.ApplyCredentialsToFavorites(selectedFavorites, credential);
                database.Cache.AttachAll(dbFavorites);
                // here we have to mark it modified, because caching detail properties
                // sets proper credential set reference
                database.Cache.MarkAsModified(dbFavorites);
                this.batchActions.SaveAndReportFavoritesUpdated(database, dbFavorites, selectedFavorites);
            }
        }

        private void EnsureCache()
        {
            if (this.isLoaded)
                return;

            var loaded = this.LoadFromDatabase();
            this.cache.Add(loaded);
            this.isLoaded = true;
        }

        internal void RefreshCache()
        {
            var newlyLoaded = this.LoadFromDatabase();
            var oldFavorites = this.Cached;
            var missing =
                ListsHelper.GetMissingSourcesInTarget(newlyLoaded, oldFavorites, new ByIdComparer<DbFavorite>());
            var redundant =
                ListsHelper.GetMissingSourcesInTarget(oldFavorites, newlyLoaded, new ByIdComparer<DbFavorite>());
            var toUpdate = newlyLoaded.Intersect(oldFavorites, new ChangedVersionComparer()).ToList();

            this.cache.Add(missing);
            this.cache.Delete(redundant);
            this.cache.Update(toUpdate);
            this.RefreshCachedItems();

            var missingToReport = missing.Cast<IFavorite>().ToList();
            var redundantToReport = redundant.Cast<IFavorite>().ToList();
            var updatedToReport = toUpdate.Cast<IFavorite>().ToList();

            this.dispatcher.ReportFavoritesAdded(missingToReport);
            this.dispatcher.ReportFavoritesDeleted(redundantToReport);
            this.dispatcher.ReportFavoritesUpdated(updatedToReport);
        }

        private void RefreshCachedItems()
        {
            foreach (var favorite in this.cache)
                favorite.ReleaseLoadedDetails();
        }

        private List<DbFavorite> LoadFromDatabase()
        {
            try
            {
                return this.TryLoadFromDatabase();
            }
            catch (EntityException exception)
            {
                return this.dispatcher.ReportFunctionError(this.LoadFromDatabase, this, exception,
                    "Unable to load favorites from database.");
            }
        }

        private List<DbFavorite> TryLoadFromDatabase()
        {
            var knownProtocols = this.connectionManager.GetAvailableProtocols();
            using (var database = DatabaseConnections.CreateInstance())
            {
                // to list because Linq to entities allows only cast to primitive types.
                // cant use connectionManager.IsKnownProtocol because it cant be translated to serverside query.
                var favorites = database.Favorites
                    .Where(f => knownProtocols.Contains(f.Protocol))
                    .ToList();
                database.Cache.DetachAll(favorites);
                favorites.ForEach(this.PrepareFavorite);
                return favorites;
            }
        }

        private void PrepareFavorite(DbFavorite favorite)
        {
            favorite.AssignStores(this.groups, this.credentials, this.dispatcher);
            // not real change, but synchronizing loaded properties to empty state, before details are loaded from DB.
            var correctOptions =
                this.connectionManager.UpdateProtocolPropertiesByProtocol(favorite.Protocol, new EmptyOptions());
            favorite.ChangeProtocol(favorite.Protocol, correctOptions);
        }

        public override string ToString()
        {
            return string.Format("Favorites:Cached={0}", this.cache.Count());
        }

        #region IEnumerable members

        IEnumerator<IFavorite> IEnumerable<IFavorite>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<DbFavorite> GetEnumerator()
        {
            this.EnsureCache();
            return this.cache.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}