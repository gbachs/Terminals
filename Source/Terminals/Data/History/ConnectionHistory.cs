using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Terminals.Configuration;
using Terminals.Data;
using Unified;

namespace Terminals.History
{
    internal delegate void HistoryRecorded(HistoryRecordedEventArgs args);

    internal sealed class ConnectionHistory : IConnectionHistory
    {
        private readonly Favorites favorites;

        /// <summary>
        ///     Prevent concurrent updates on History file by another program
        /// </summary>
        private readonly Mutex fileLock = new Mutex(false, "Terminals.CodePlex.com.History");

        private readonly DataFileWatcher fileWatcher;

        private readonly ManualResetEvent loadingGate = new ManualResetEvent(false);

        private HistoryByFavorite currentHistory;

        internal ConnectionHistory(Favorites favorites)
        {
            this.favorites = favorites;
            this.fileWatcher = new DataFileWatcher(FileLocations.HistoryFullFileName);
            this.fileWatcher.FileChanged += this.OnFileChanged;
            this.fileWatcher.StartObservation();
            ThreadPool.QueueUserWorkItem(this.LoadHistory);
        }

        public event HistoryRecorded HistoryRecorded;

        public event Action HistoryClear;

        public SortableList<IFavorite> GetDateItems(string historyDateKey)
        {
            this.loadingGate.WaitOne();
            var historyGroupItems = this.GetGroupedByDate()[historyDateKey];
            var groupFavorites = SelectFavoritesFromHistoryItems(historyGroupItems);
            return Favorites.OrderByDefaultSorting(groupFavorites);
        }

        public void RecordHistoryItem(IFavorite favorite)
        {
            this.loadingGate.WaitOne();
            if (this.currentHistory == null || favorite == null)
                return;

            var favoriteHistoryList = this.GetFavoriteHistoryList(favorite.Id);
            favoriteHistoryList.Add(new HistoryItem());
            this.SaveHistory();
            this.FireOnHistoryRecorded(favorite);
        }

        public void Clear()
        {
            this.currentHistory.Clear();
            this.SaveHistory();
            if (this.HistoryClear != null)
                this.HistoryClear();
        }

        private void OnFileChanged(object sender, EventArgs e)
        {
            // don't need locking here, because only today is updated adding new items
            var oldTodays = this.GetOldTodaysHistory();
            this.LoadHistory(null);
            var newTodays = this.MergeWithNewTodays(oldTodays);
            foreach (var favorite in newTodays)
                this.FireOnHistoryRecorded(favorite);
        }

        private List<IFavorite> MergeWithNewTodays(SortableList<IFavorite> oldTodays)
        {
            List<IFavorite> newTodays = this.GetDateItems(HistoryIntervals.TODAY);
            if (oldTodays != null)
                newTodays = DataDispatcher.GetMissingFavorites(newTodays, oldTodays);
            return newTodays;
        }

        private SortableList<IFavorite> GetOldTodaysHistory()
        {
            SortableList<IFavorite> oldTodays = null;
            if (this.currentHistory != null)
                oldTodays = this.GetDateItems(HistoryIntervals.TODAY);
            return oldTodays;
        }

        /// <summary>
        ///     Because file watcher is created before the main form in GUI thread.
        ///     This lets to fire the file system watcher events in GUI thread.
        /// </summary>
        internal void AssignSynchronizationObject(ISynchronizeInvoke synchronizer)
        {
            this.fileWatcher.AssignSynchronizer(synchronizer);
        }

        private static SortableList<IFavorite> SelectFavoritesFromHistoryItems(
            SortableList<IHistoryItem> groupedByDate)
        {
            var selection = new SortableList<IFavorite>();
            foreach (var favoriteTouch in groupedByDate)
            {
                var favorite = favoriteTouch.Favorite;
                if (favorite != null && !selection.Contains(favorite)) // add each favorite only once
                    selection.Add(favorite);
            }

            return selection;
        }

        private SerializableDictionary<string, SortableList<IHistoryItem>> GetGroupedByDate()
        {
            return this.currentHistory.GroupByDate();
        }

        /// <summary>
        ///     Load or re-load history from HistoryLocation
        /// </summary>
        private void LoadHistory(object threadState)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                this.TryLoadFile();
                Debug.WriteLine("History Loaded. Duration:{0}ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception exc)
            {
                Logging.Error("Error Loading History", exc);
            }
            finally
            {
                this.loadingGate.Set();
            }
        }

        private void TryLoadFile()
        {
            var fileName = FileLocations.HistoryFullFileName;
            if (!string.IsNullOrEmpty(fileName))
            {
                Logging.InfoFormat("Loading History from: {0}", fileName);
                if (File.Exists(fileName))
                {
                    this.LoadFile();
                    return;
                }
            }

            this.currentHistory = new HistoryByFavorite {Favorites = this.favorites};
        }

        private void LoadFile()
        {
            try
            {
                this.fileLock.WaitOne();
                var loadedHistory =
                    Serialize.DeserializeXMLFromDisk(FileLocations.HistoryFullFileName, typeof(HistoryByFavorite)) as
                        HistoryByFavorite;
                loadedHistory.Favorites = this.favorites;
                this.currentHistory = loadedHistory;
            }
            catch // fails also in case of history upgrade, we don't upgrade history file
            {
                this.currentHistory = new HistoryByFavorite {Favorites = this.favorites};
            }
            finally
            {
                this.fileLock.ReleaseMutex();
            }
        }

        private void SaveHistory()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                this.fileLock.WaitOne();
                this.fileWatcher.StopObservation();
                Serialize.SerializeXMLToDisk(this.currentHistory, FileLocations.HistoryFullFileName);
                Logging.Info(string.Format("History saved. Duration:{0} ms", stopwatch.ElapsedMilliseconds));
            }
            catch (Exception exc)
            {
                Logging.Error("Error Saving History", exc);
            }
            finally
            {
                this.fileLock.ReleaseMutex();
                this.fileWatcher.StartObservation();
            }
        }

        private void FireOnHistoryRecorded(IFavorite favorite)
        {
            if (this.HistoryRecorded != null)
            {
                var args = new HistoryRecordedEventArgs(favorite);
                this.HistoryRecorded(args);
            }
        }

        private List<HistoryItem> GetFavoriteHistoryList(Guid favoriteId)
        {
            if (!this.currentHistory.ContainsKey(favoriteId))
                this.currentHistory.Add(favoriteId, new List<HistoryItem>());

            return this.currentHistory[favoriteId];
        }
    }
}