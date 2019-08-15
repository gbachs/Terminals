using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Terminals.Configuration;
using Terminals.Data.Credentials;
using Unified;

namespace Terminals.Data
{
    internal sealed class StoredCredentials : ICredentials
    {
        private readonly List<ICredentialSet> cache;

        private readonly Mutex fileLock = new Mutex(false, "Terminals.CodePlex.com.Credentials");

        private readonly PersistenceSecurity persistenceSecurity;

        private DataFileWatcher fileWatcher;

        internal StoredCredentials(PersistenceSecurity persistenceSecurity)
        {
            this.persistenceSecurity = persistenceSecurity;
            this.cache = new List<ICredentialSet>();
            this.InitializeFileWatch();
        }

        private string FileFullName => Settings.Instance.FileLocations.Credentials;

        private void InitializeFileWatch()
        {
            this.fileWatcher = new DataFileWatcher(this.FileFullName);
            this.fileWatcher.FileChanged += this.CredentialsFileChanged;
            this.fileWatcher.StartObservation();
        }

        /// <summary>
        ///     Don't load the file in constructor, we wait until persistence is authenticated.
        ///     This is needed specially by upgrades from previous version,
        ///     to let it upgrade the file, before it is loaded into persistence.
        /// </summary>
        internal void Initialize()
        {
            var configFileName = this.FileFullName;
            if (File.Exists(configFileName))
                this.LoadStoredCredentials(configFileName);
            else
                this.Save();
        }

        private void CredentialsFileChanged(object sender, EventArgs e)
        {
            this.LoadStoredCredentials(this.FileFullName);
            this.CredentialsChanged?.Invoke(this, new EventArgs());
        }

        internal void AssignSynchronizationObject(ISynchronizeInvoke synchronizer)
        {
            this.fileWatcher.AssignSynchronizer(synchronizer);
        }

        private void LoadStoredCredentials(string configFileName)
        {
            var loaded = this.LoadFile(configFileName);
            if (loaded != null)
            {
                this.cache.Clear();
                this.cache.AddRange(loaded);
            }
        }

        private List<ICredentialSet> LoadFile(string configFileName)
        {
            try
            {
                this.fileLock.WaitOne();
                return this.DeserializeFileContent(configFileName);
            }
            catch (Exception exception)
            {
                var errorMessage = string.Format("Load credentials from {0} failed.", configFileName);
                Logging.Error(errorMessage, exception);
                return new List<ICredentialSet>();
            }
            finally
            {
                this.fileLock.ReleaseMutex();
                Debug.WriteLine("Credentials file Loaded.");
            }
        }

        private List<ICredentialSet> DeserializeFileContent(string configFileName)
        {
            var loadedObj = Serialize.DeserializeXMLFromDisk(configFileName, typeof(List<CredentialSet>));
            var loadedItems = loadedObj as List<CredentialSet>;
            return loadedItems.Cast<ICredentialSet>().ToList();
        }

        #region ICredentials

        public event EventHandler CredentialsChanged;

        public ICredentialSet this[Guid id]
        {
            get { return this.cache.FirstOrDefault(candidate => candidate.Id.Equals(id)); }
        }

        /// <summary>
        ///     Gets a credential by its name from cached credentials.
        ///     This method isn't case sensitive. If no item matches, returns null.
        /// </summary>
        /// <param name="name">name of an item to search</param>
        public ICredentialSet this[string name]
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                    return null;

                return this.cache.FirstOrDefault(candidate => candidate.Name
                    .Equals(name, StringComparison.CurrentCultureIgnoreCase));
            }
        }

        public void Add(ICredentialSet toAdd)
        {
            if (string.IsNullOrEmpty(toAdd.Name))
                return;

            this.cache.Add(toAdd);
            this.Save();
        }

        public void Remove(ICredentialSet toRemove)
        {
            this.cache.Remove(toRemove);
            this.Save();
        }

        public void Update(ICredentialSet toUpdate)
        {
            var oldItem = this[toUpdate.Id];
            if (oldItem != null)
                this.cache.Remove(oldItem);
            this.cache.Add(toUpdate);
            this.Save();
        }

        public void UpdatePasswordsByNewKeyMaterial(string newKeyMaterial)
        {
            foreach (var credentials in this.cache)
            {
                var guarded = new GuardedCredential(credentials, this.persistenceSecurity);
                guarded.UpdatePasswordByNewKeyMaterial(newKeyMaterial);
            }

            this.Save();
        }

        /// <summary>
        ///     Called automatically after each <see cref="ICredentials" /> method.
        ///     Not necessary to call manually.
        /// </summary>
        internal void Save()
        {
            try
            {
                this.fileLock.WaitOne();
                this.fileWatcher.StopObservation();
                this.SaveToFile();
            }
            catch (Exception exception)
            {
                var errorMessage = string.Format("Save credentials to {0} failed.", this.FileFullName);
                Logging.Error(errorMessage, exception);
            }
            finally
            {
                this.fileWatcher.StartObservation();
                this.fileLock.ReleaseMutex();
            }
        }

        private void SaveToFile()
        {
            var fileContent = this.cache.Cast<CredentialSet>().ToList();
            Serialize.SerializeXMLToDisk(fileContent, this.FileFullName);
            Debug.WriteLine("Credentials file saved.");
        }

        #endregion

        #region IEnumerable members

        public IEnumerator<ICredentialSet> GetEnumerator()
        {
            return this.cache.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}