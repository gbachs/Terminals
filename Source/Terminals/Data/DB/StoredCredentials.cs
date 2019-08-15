using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Terminals.Data.DB
{
    /// <summary>
    ///     SQL database implementation of managing credentials
    /// </summary>
    internal class StoredCredentials : ICredentials
    {
        private readonly EntitiesCache<DbCredentialSet> cache = new EntitiesCache<DbCredentialSet>();

        private readonly DataDispatcher dispatcher;

        private bool isLoaded;

        public StoredCredentials(DataDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        internal DbCredentialSet this[Guid id]
        {
            get
            {
                this.EnsureCache();
                return this.cache.FirstOrDefault(candidate => candidate.Guid == id);
            }
        }

        internal DbCredentialSet this[int storeId]
        {
            get
            {
                this.EnsureCache();
                return this.cache.FirstOrDefault(candidate => candidate.Id == storeId);
            }
        }

        public event EventHandler CredentialsChanged;

        ICredentialSet ICredentials.this[Guid id] => this[id];

        ICredentialSet ICredentials.this[string name]
        {
            get
            {
                this.EnsureCache();
                return this.cache.FirstOrDefault(candidate => candidate.Name
                    .Equals(name, StringComparison.CurrentCultureIgnoreCase));
            }
        }

        public void Add(ICredentialSet toAdd)
        {
            try // no concurrency here, because there are no dependences on other tables
            {
                this.TryAdd(toAdd);
            }
            catch (EntityException exception)
            {
                this.dispatcher.ReportActionError(this.Add, toAdd, this, exception,
                    "Unable to add credential to database");
            }
        }

        public void Remove(ICredentialSet toRemove)
        {
            try
            {
                this.TryRemove(toRemove);
            }
            catch (DbUpdateException)
            {
                this.cache.Delete((DbCredentialSet)toRemove);
            }
            catch (EntityException exception)
            {
                this.dispatcher.ReportActionError(this.Remove, toRemove, this, exception,
                    "Unable to remove credential from database.");
            }
        }

        public void Update(ICredentialSet toUpdate)
        {
            try
            {
                this.TryUpdate(toUpdate);
            }
            catch (DbUpdateException) // item already removed
            {
                this.cache.Delete((DbCredentialSet)toUpdate);
            }
            catch (EntityException exception)
            {
                this.dispatcher.ReportActionError(this.Update, toUpdate, this, exception,
                    "Unable to update credential set.");
            }
        }

        public void UpdatePasswordsByNewKeyMaterial(string newKeyMaterial)
        {
            this.RefreshCache();
        }

        private void TryAdd(ICredentialSet toAdd)
        {
            var credentialToAdd = toAdd as DbCredentialSet;
            AddToDatabase(credentialToAdd);
            this.cache.Add(credentialToAdd);
        }

        private static void AddToDatabase(DbCredentialSet credentialToAdd)
        {
            using (var database = DatabaseConnections.CreateInstance())
            {
                database.CredentialBase.Add(credentialToAdd);
                database.SaveImmediatelyIfRequested();
                database.Cache.Detach(credentialToAdd);
            }
        }

        private void TryRemove(ICredentialSet toRemove)
        {
            var credentailToRemove = toRemove as DbCredentialSet;
            DeleteFromDatabase(credentailToRemove);
            this.cache.Delete(credentailToRemove);
        }

        private static void DeleteFromDatabase(DbCredentialSet credentailToRemove)
        {
            using (var database = DatabaseConnections.CreateInstance())
            {
                database.CredentialBase.Attach(credentailToRemove);
                database.CredentialBase.Remove(credentailToRemove);
                database.SaveImmediatelyIfRequested();
            }
        }

        private void TryUpdate(ICredentialSet toUpdate)
        {
            using (var database = DatabaseConnections.CreateInstance())
            {
                var credentialToUpdate = toUpdate as DbCredentialSet;
                database.CredentialBase.Attach(credentialToUpdate);
                database.Cache.MarkAsModified(credentialToUpdate);
                database.SaveImmediatelyIfRequested();
                database.Cache.Detach(credentialToUpdate);
                this.cache.Update(credentialToUpdate);
            }
        }

        private void EnsureCache()
        {
            if (this.isLoaded)
                return;

            this.ReloadCache();
            this.isLoaded = true;
        }

        internal void RefreshCache()
        {
            this.cache.Clear();
            this.ReloadCache();
            this.CredentialsChanged?.Invoke(this, new EventArgs());
        }

        private void ReloadCache()
        {
            var loaded = this.LoadFromDatabase();
            this.cache.Add(loaded);
        }

        private List<DbCredentialSet> LoadFromDatabase()
        {
            try
            {
                return TryLoadFromDatabase();
            }
            catch (EntityException exception)
            {
                return this.dispatcher.ReportFunctionError(this.LoadFromDatabase, this, exception,
                    "Unable to load credentials from database.");
            }
        }

        private static List<DbCredentialSet> TryLoadFromDatabase()
        {
            using (var database = DatabaseConnections.CreateInstance())
            {
                return database.CredentialBase.OfType<DbCredentialSet>().ToList();
            }
        }

        public override string ToString()
        {
            return string.Format("StoredCredentials:Cached={0}", this.cache.Count());
        }

        #region IEnumerable members

        public IEnumerator<ICredentialSet> GetEnumerator()
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