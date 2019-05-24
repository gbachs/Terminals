using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;

namespace Terminals.Data.DB
{
    /// <summary>
    ///     SQL persisted groups container
    /// </summary>
    internal class Groups : IGroups
    {
        private readonly EntitiesCache<DbGroup> cache;

        private DataDispatcher dispatcher;

        private Favorites favorites;

        private bool isLoaded;

        internal Groups()
        {
            this.cache = new EntitiesCache<DbGroup>();
        }

        private List<DbGroup> Cached
        {
            get
            {
                this.CheckCache();
                return this.cache.ToList();
            }
        }

        /// <summary>
        ///     Gets cached item by its database unique identifier
        /// </summary>
        internal DbGroup this[int id]
        {
            get
            {
                this.CheckCache();
                return this.cache.FirstOrDefault(candidate => candidate.Id == id);
            }
        }

        IGroup IGroups.this[string groupName]
        {
            get
            {
                this.CheckCache();
                return this.cache.FirstOrDefault(group =>
                    group.Name.Equals(groupName, StringComparison.CurrentCultureIgnoreCase));
            }
        }

        public void Add(IGroup group)
        {
            try
            {
                this.TryAdd(group);
            }
            catch (EntityException exception)
            {
                this.dispatcher.ReportActionError(this.Add, group, this, exception, "Unable to add group to database.");
            }
        }

        public void Update(IGroup group)
        {
            try
            {
                this.TryUpdateGroup(group);
            }
            catch (EntityException exception)
            {
                this.dispatcher.ReportActionError(this.Update, group, this, exception,
                    "Unable to update group in database");
            }
        }

        public void Delete(IGroup group)
        {
            try
            {
                this.TryDelete(group);
            }
            catch (DbUpdateException) // item already removed
            {
                this.FinishGroupRemove(group);
            }
            catch (EntityException exception)
            {
                this.dispatcher.ReportActionError(this.Delete, group, this, exception,
                    "Unable to remove group from database.");
            }
        }

        public void Rebuild()
        {
            try
            {
                this.TryRebuild();
            }
            catch (DbUpdateException)
            {
                // merge or update is not critical simply force refresh
                this.RefreshCache();
            }
            catch (EntityException exception)
            {
                this.dispatcher.ReportActionError(this.Rebuild, this, exception, "Unable to rebuild groups.");
            }
        }

        public void AssignStores(DataDispatcher dispatcher, Favorites favorites)
        {
            this.dispatcher = dispatcher;
            this.favorites = favorites;
        }

        private void TryAdd(IGroup group)
        {
            using (var database = DatabaseConnections.CreateInstance())
            {
                var toAdd = group as DbGroup;
                database.AddToGroups(toAdd);
                database.SaveImmediatelyIfRequested();
                database.Cache.DetachGoup(toAdd);
                this.cache.Add(toAdd);
                this.dispatcher.ReportGroupsAdded(new List<IGroup> {toAdd});
            }
        }

        private void TryUpdateGroup(IGroup group)
        {
            using (var database = DatabaseConnections.CreateInstance())
            {
                var toUpdate = group as DbGroup;
                database.Cache.Attach(toUpdate);
                this.TrySaveAndReport(toUpdate, database);
            }
        }

        private void TrySaveAndReport(DbGroup toUpdate, Database database)
        {
            try
            {
                this.SaveAndReportUpdated(database, toUpdate);
            }
            catch (DbUpdateException)
            {
                this.TryToRefreshUpdated(toUpdate, database);
            }
        }

        private void TryToRefreshUpdated(DbGroup toUpdate, Database database)
        {
            try
            {
                database.RefreshEntity(toUpdate);
                this.SaveAndReportUpdated(database, toUpdate);
            }
            catch (InvalidOperationException)
            {
                this.cache.Delete(toUpdate);
                this.dispatcher.ReportGroupsDeleted(new List<IGroup> {toUpdate});
            }
        }

        private void SaveAndReportUpdated(Database database, DbGroup toUpdate)
        {
            database.Cache.MarkAsModified(toUpdate);
            database.SaveImmediatelyIfRequested();
            database.Cache.Detach(toUpdate);
            this.cache.Update(toUpdate);
            this.dispatcher.ReportGroupsUpdated(new List<IGroup> {toUpdate});
        }

        private void TryDelete(IGroup group)
        {
            using (var database = DatabaseConnections.CreateInstance())
            {
                var toDelete = group as DbGroup;
                var changedFavorites = group.Favorites;
                this.SetChildsToRoot(database, toDelete);
                database.Groups.Attach(toDelete);
                database.Groups.Remove(toDelete);
                database.SaveImmediatelyIfRequested();
                this.FinishGroupRemove(group);
                this.dispatcher.ReportFavoritesUpdated(changedFavorites);
            }
        }

        private void SetChildsToRoot(Database database, DbGroup group)
        {
            foreach (var child in this.CachedChilds(group))
            {
                child.Parent = null;
                database.Cache.Attach(child);
                this.TrySaveAndReport(child, database);
                this.cache.Delete(child);
            }
        }

        private List<DbGroup> CachedChilds(IGroup parent)
        {
            return this.cache.Where(candidate => parent.StoreIdEquals(candidate.Parent))
                .ToList();
        }

        private void FinishGroupRemove(IGroup group)
        {
            this.cache.Delete((DbGroup)group);
            this.dispatcher.ReportGroupsDeleted(new List<IGroup> {group});
        }

        private void TryRebuild()
        {
            using (var database = DatabaseConnections.CreateInstance())
            {
                var emptyGroups = this.DeleteEmptyGroupsFromDatabase(database);
                database.SaveImmediatelyIfRequested();
                var toReport = this.DeleteFromCache(emptyGroups);
                this.dispatcher.ReportGroupsDeleted(toReport);
            }
        }

        /// <summary>
        ///     Call this method after the changes were committed into database,
        ///     to let the cache in last state as long as possible and ensure, that the commit didn't fail.
        /// </summary>
        private List<IGroup> DeleteFromCache(List<DbGroup> emptyGroups)
        {
            this.cache.Delete(emptyGroups);
            return emptyGroups.Cast<IGroup>().ToList();
        }

        /// <summary>
        ///     Doesn't remove them from cache, only from database
        /// </summary>
        private List<DbGroup> DeleteEmptyGroupsFromDatabase(Database database)
        {
            var emptyGroups = this.GetEmptyGroups();
            database.Cache.AttachAll(emptyGroups);
            database.DeleteAll(emptyGroups);
            return emptyGroups;
        }

        private List<DbGroup> GetEmptyGroups()
        {
            return this.cache.Where(group => ((IGroup)group).Favorites.Count == 0)
                .ToList();
        }

        internal void RefreshCache()
        {
            var newlyLoaded = this.Load(this.Cached);
            var oldGroups = this.Cached;

            var missing = ListsHelper.GetMissingSourcesInTarget(newlyLoaded, oldGroups);
            var redundant = ListsHelper.GetMissingSourcesInTarget(oldGroups, newlyLoaded);
            this.cache.Add(missing);
            this.cache.Delete(redundant);
            this.RefreshLoaded();

            var missingToReport = missing.Cast<IGroup>().ToList();
            var redundantToReport = redundant.Cast<IGroup>().ToList();
            this.dispatcher.ReportGroupsRecreated(missingToReport, redundantToReport);
        }

        private void RefreshLoaded()
        {
            foreach (var group in this.cache)
                group.ReleaseFavoriteIds();
        }

        private void CheckCache()
        {
            if (this.isLoaded)
                return;

            var loaded = this.LoadFromDatabase();
            this.AssignGroupsContainer(loaded);
            this.cache.Add(loaded);
            this.isLoaded = true;
        }

        private List<DbGroup> Load(List<DbGroup> toRefresh)
        {
            var loaded = this.LoadFromDatabase(toRefresh);
            this.AssignGroupsContainer(loaded);
            return loaded;
        }

        private void AssignGroupsContainer(List<DbGroup> groups)
        {
            foreach (var group in groups)
                group.AssignStores(this, this.dispatcher, this.favorites);
        }

        private List<DbGroup> LoadFromDatabase()
        {
            try
            {
                return TryLoadFromDatabase();
            }
            catch (EntityException exception)
            {
                return this.dispatcher.ReportFunctionError(this.LoadFromDatabase, this, exception,
                    "Unable to load groups from database");
            }
        }

        private static List<DbGroup> TryLoadFromDatabase()
        {
            using (var database = DatabaseConnections.CreateInstance())
            {
                var groups = database.Groups.ToList();
                database.Cache.DetachAll(groups);
                return groups;
            }
        }

        private List<DbGroup> LoadFromDatabase(List<DbGroup> toRefresh)
        {
            try
            {
                return TryLoadFromDatabase(toRefresh);
            }
            catch (EntityException exception)
            {
                return this.dispatcher.ReportFunctionError(this.LoadFromDatabase, toRefresh, this, exception,
                    "Unable to refresh groups from database");
            }
        }

        private static List<DbGroup> TryLoadFromDatabase(List<DbGroup> toRefresh)
        {
            using (var database = DatabaseConnections.CreateInstance())
            {
                if (toRefresh != null)
                    database.Cache.AttachAll(toRefresh);

                ((IObjectContextAdapter)database).ObjectContext.Refresh(RefreshMode.StoreWins, database.Groups);
                var groups = database.Groups.Include("ParentGroup").ToList();
                database.Cache.DetachAll(groups);
                return groups;
            }
        }

        internal List<DbGroup> GetGroupsContainingFavorite(int favoriteId)
        {
            this.CheckCache();
            return this.cache.Where(candidate => candidate.ContainsFavorite(favoriteId))
                .ToList();
        }

        public override string ToString()
        {
            return string.Format("Groups:Cached={0}", this.cache.Count());
        }

        #region IEnumerable members

        public IEnumerator<IGroup> GetEnumerator()
        {
            this.CheckCache();
            return this.cache.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}