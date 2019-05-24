using System;
using System.ComponentModel;
using System.IO;

namespace Terminals.Data
{
    /// <summary>
    ///     Detects data or configuration file changes done
    ///     by another application or Terminals instance and reports them.
    ///     Raises events in GUI thread, so no Invoke is required.
    /// </summary>
    internal class DataFileWatcher : IDataFileWatcher
    {
        private readonly FileSystemWatcher fileWatcher;

        private readonly string fullFileName;

        internal DataFileWatcher(string fullFileName)
        {
            this.fullFileName = fullFileName;
            this.fileWatcher = new FileSystemWatcher();
            this.fileWatcher.Path = Path.GetDirectoryName(fullFileName);
            this.fileWatcher.Filter = Path.GetFileName(fullFileName);
            this.fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName |
                                            NotifyFilters.CreationTime | NotifyFilters.Size;
            this.fileWatcher.Changed += this.ConfigFileChanged;
        }

        public event EventHandler FileChanged;

        /// <summary>
        ///     Because file watcher is created before the main form,
        ///     the synchronization object has to be assigned later.
        ///     This lets to fire the file system watcher events in GUI thread.
        /// </summary>
        public void AssignSynchronizer(ISynchronizeInvoke synchronizer)
        {
            this.fileWatcher.SynchronizingObject = synchronizer;
        }

        public void StopObservation()
        {
            this.fileWatcher.EnableRaisingEvents = false;
        }

        public void StartObservation()
        {
            this.fileWatcher.EnableRaisingEvents = true;
        }

        private void ConfigFileChanged(object sender, FileSystemEventArgs e)
        {
            Logging.DebugFormat("{0} file change by another application (or Terminals instance) detected!",
                this.fullFileName);
            if (this.FileChanged != null && this.fileWatcher.SynchronizingObject != null)
                this.FileChanged(this.fullFileName, new EventArgs());
        }
    }
}