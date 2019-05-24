using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using Terminals.Common.Connections;

namespace Terminals.Data.DB
{
    internal partial class Database : DbContext
    {
        internal Database(DbConnection connection)
            : base(connection, true)
        {
            // todo disable change tracking, we use the context disconnected, but implementation has to be fixed
            //this.Configuration.ProxyCreationEnabled = false;
            //this.Configuration.AutoDetectChangesEnabled = false;

            this.BeforeConnectExecute = this.Set<DbBeforeConnectExecute>();
            this.CredentialBase = this.Set<DbCredentialBase>();
            this.DisplayOptions = this.Set<DbDisplayOptions>();
            this.Favorites = this.Set<DbFavorite>();
            this.Groups = this.Set<DbGroup>();
            this.Security = this.Set<DbSecurityOptions>();

            this.Cache = new CacheConnector(this);
        }

        /// <summary>
        ///     Gets this instance connector for cached entities
        /// </summary>
        public CacheConnector Cache { get; }

        internal void SaveImmediatelyIfRequested()
        {
            // don't ask, save immediately. Here is no benefit to save in batch like in FilePersistence
            this.SaveChanges();
        }

        public override int SaveChanges()
        {
            var changedFavorites = this.GetChangedOrAddedFavorites();
            // add to database first, otherwise the favorite properties cant be committed.
            var returnValue = base.SaveChanges();
            this.SaveFavoriteDetails(changedFavorites);
            return returnValue;
        }

        private void SaveFavoriteDetails(IEnumerable<DbFavorite> changedFavorites)
        {
            foreach (var favorite in changedFavorites)
                favorite.SaveDetails(this);
        }

        private IEnumerable<DbFavorite> GetChangedOrAddedFavorites()
        {
            return this.ChangeTracker.Entries<DbFavorite>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(change => change.Entity)
                .ToList();
        }

        internal byte[] GetFavoriteIcon(int favoriteId)
        {
            var obtained = this.GetFavoriteIcon((int?)favoriteId).FirstOrDefault();
            if (obtained != null)
                return obtained;

            return FavoriteIcons.EmptyImageData;
        }

        internal string GetProtocolPropertiesByFavorite(int favoriteId)
        {
            return this.GetFavoriteProtocolProperties(favoriteId).FirstOrDefault();
        }

        internal string GetMasterPasswordHash()
        {
            var obtained = this.GetMasterPasswordKey().FirstOrDefault();
            if (obtained != null)
                return obtained;

            return string.Empty;
        }

        internal void UpdateMasterPassword(string newMasterPasswordKey)
        {
            this.UpdateMasterPasswordKey(newMasterPasswordKey);
        }

        internal List<int> GetRdpFavoriteIds()
        {
            return this.Favorites.Where(candidate => candidate.Protocol == KnownConnectionConstants.RDP)
                .Select(rdpFavorite => rdpFavorite.Id).ToList();
        }

        internal void AddAll(IEnumerable<DbFavorite> favorites)
        {
            foreach (var favorite in favorites)
                this.Favorites.Add(favorite);
        }

        /// <summary>
        ///     we have to delete the credentials base manually, this property uses lazy creation
        ///     and therefore there is no database constraint
        /// </summary>
        internal void RemoveRedundantCredentialBase(List<DbCredentialBase> redundantCredentialBase)
        {
            foreach (var credentialBase in redundantCredentialBase)
                this.CredentialBase.Remove(credentialBase);
        }

        internal void DeleteAll(IEnumerable<DbFavorite> favorites)
        {
            foreach (var favorite in favorites)
                this.Favorites.Remove(favorite);
        }

        internal void AddToGroups(DbGroup toAdd)
        {
            toAdd.FieldsToReferences();
            if (toAdd.ParentGroup != null)
                this.Cache.Attach(toAdd.ParentGroup);
            this.Groups.Add(toAdd);
        }

        internal List<IGroup> AddToDatabase(List<IGroup> groups)
        {
            // not added groups don't have an identifier obtained from database
            var added = groups.Where(candidate => ((DbGroup)candidate).Id == 0).ToList();
            this.AddAll(added);
            var toAttach = groups.Where(candidate => ((DbGroup)candidate).Id != 0).Cast<DbGroup>().ToList();
            this.Cache.AttachAll(toAttach);
            return added;
        }

        private void AddAll(List<IGroup> added)
        {
            foreach (DbGroup group in added)
                this.Groups.Add(group);
        }

        internal void DeleteAll(IEnumerable<DbGroup> groups)
        {
            foreach (var group in groups)
                this.Groups.Remove(group);
        }

        internal void RefreshEntity<TEntity>(TEntity toUpdate)
            where TEntity : class
        {
            this.Entry(toUpdate).Reload();
            ((IObjectContextAdapter)this).ObjectContext.Refresh(RefreshMode.ClientWins, toUpdate);
        }
    }
}