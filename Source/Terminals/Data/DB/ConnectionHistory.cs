﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Terminals.Data.History;
using Terminals.History;
using Terminals.Network;

namespace Terminals.Data.DB
{
    /// <summary>
    ///     SQl implementation of connections history.
    ///     This doesn't cache the history table in entity framework, because of performance.
    /// </summary>
    internal class ConnectionHistory : IConnectionHistory
    {
        /// <summary>
        ///     Cache older items than today only, because the history cant change.
        ///     The reason is to don't reload these items from database.
        /// </summary>
        private readonly Dictionary<string, SortableList<IFavorite>> cache =
            new Dictionary<string, SortableList<IFavorite>>();

        /// <summary>
        ///     Access the cached items, instead of retrieving them from database
        /// </summary>
        private readonly Favorites favorites;

        private readonly DataDispatcher dispatcher;

        internal ConnectionHistory(Favorites favorites, DataDispatcher dispatcher)
        {
            this.favorites = favorites;
            this.dispatcher = dispatcher;
        }

        public event HistoryRecorded HistoryRecorded;

        public event Action HistoryClear;

        public SortableList<IFavorite> GetDateItems(string historyDateKey)
        {
            // cache older groups only
            if (historyDateKey == HistoryIntervals.TODAY)
                return this.LoadFromDatabaseByDate(historyDateKey);

            return this.LoadFromCache(historyDateKey);
        }

        public void RecordHistoryItem(IFavorite favorite)
        {
            var historyTarget = favorite as DbFavorite;
            if (historyTarget == null)
                return;

            // here we don't cache today's items, we always load the current state from database
            this.AddToDatabase(historyTarget);
        }

        public void Clear()
        {
            try
            {
                TryClearHistory();
                this.HistoryClear?.Invoke();
            }
            catch (EntityException exception)
            {
                this.dispatcher.ReportActionError(this.Clear, this, exception,
                    "Unable to clear history.\r\nDatabase connection lost.");
            }
        }

        private SortableList<IFavorite> LoadFromCache(string historyDateKey)
        {
            if (!this.cache.ContainsKey(historyDateKey))
            {
                var loaded = this.LoadFromDatabaseByDate(historyDateKey);
                this.cache.Add(historyDateKey, loaded);
            }

            return this.cache[historyDateKey];
        }

        private SortableList<IFavorite> LoadFromDatabaseByDate(string historyDateKey)
        {
            try
            {
                return this.TryLodFromDatabase(historyDateKey);
            }
            catch (EntityException exception)
            {
                return this.dispatcher.ReportFunctionError(this.LoadFromDatabaseByDate, historyDateKey, this,
                    exception, "Unable to load history part form database.\r\nDatabase connection lost.");
            }
        }

        private SortableList<IFavorite> TryLodFromDatabase(string historyDateKey)
        {
            using (var database = DatabaseConnections.CreateInstance())
            {
                var interval = HistoryIntervals.GetIntervalByName(historyDateKey);
                // store holds dates in UTC
                var favoriteIds = database.GetFavoritesHistoryByDate(interval.From, interval.To).ToList();
                var intervalFavorites =
                    this.favorites.Cast<DbFavorite>().Where(favorite => favoriteIds.Contains(favorite.Id));
                return Data.Favorites.OrderByDefaultSorting(intervalFavorites);
            }
        }

        private void AddToDatabase(DbFavorite historyTarget)
        {
            try
            {
                TryAddToDatabase(historyTarget);
                // don't report, if it wasn't successfully added
                this.FireOnHistoryRecorded(historyTarget);
            }
            catch (EntityException exception)
            {
                this.dispatcher.ReportActionError(this.AddToDatabase, historyTarget, this, exception,
                    "Unable to save connection history.\r\nDatabase connection lost.");
            }
        }

        private static void TryAddToDatabase(DbFavorite historyTarget)
        {
            using (var database = DatabaseConnections.CreateInstance())
            {
                var userSid = WindowsUserIdentifiers.GetCurrentUserSid();
                // store holds dates in UTC
                database.InsertHistory(historyTarget.Id, Moment.Now, userSid);
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

        private static void TryClearHistory()
        {
            using (var database = DatabaseConnections.CreateInstance())
            {
                database.ClearHistory();
            }
        }

        public override string ToString()
        {
            return string.Format("ConnectionHistory:Cached={0}", this.cache.Count());
        }
    }
}