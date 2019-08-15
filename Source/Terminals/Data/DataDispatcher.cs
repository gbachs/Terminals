using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace Terminals.Data
{
    /// <summary>
    ///     Informs about changes in Tags collection
    /// </summary>
    /// <param name="args">Not null container reporting removed and added Tags</param>
    internal delegate void GroupsChangedEventHandler(GroupsChangedArgs args);

    /// <summary>
    ///     Informs about changes in favorites collection.
    /// </summary>
    /// <param name="args">Not null container, reporting Added, removed, and updated favorites</param>
    internal delegate void FavoritesChangedEventHandler(FavoritesChangedEventArgs args);

    /// <summary>
    ///     Central point, which distributes information about changes in Tags and Favorites collections
    /// </summary>
    internal sealed class DataDispatcher
    {
        private int callStackCounter;

        private FavoritesChangedEventArgs favorites;

        private GroupsChangedArgs groups;

        private bool DelayedUpdate => this.groups != null;

        internal event GroupsChangedEventHandler GroupsChanged;

        internal event FavoritesChangedEventHandler FavoritesChanged;

        internal event EventHandler<DataErrorEventArgs> ErrorOccurred;

        internal void StartDelayedUpdate()
        {
            if (this.DelayedUpdate)
                return;
            this.groups = new GroupsChangedArgs();
            this.favorites = new FavoritesChangedEventArgs();
        }

        internal void EndDelayedUpdate()
        {
            this.FireGroupsChangedEvent(this.groups);
            this.groups = null;
            this.FireFavoritesChangedEvent(this.favorites);
            this.favorites = null;
        }

        internal static List<IFavorite> GetMissingFavorites(List<IFavorite> newFavorites, List<IFavorite> oldFavorites)
        {
            return newFavorites.Where(
                    newFavorite => oldFavorites.FirstOrDefault(oldFavorite => oldFavorite.Name == newFavorite.Name) ==
                                   null)
                .ToList();
        }

        internal void ReportFavoriteAdded(IFavorite addedFavorite)
        {
            var args = new FavoritesChangedEventArgs();
            args.Added.Add(addedFavorite);
            this.FireFavoriteChanges(args);
        }

        internal void ReportFavoritesAdded(List<IFavorite> addedFavorites)
        {
            var args = new FavoritesChangedEventArgs();
            args.Added.AddRange(addedFavorites);
            this.FireFavoriteChanges(args);
        }

        internal void ReportFavoriteUpdated(IFavorite changedFavorite)
        {
            var args = new FavoritesChangedEventArgs();
            args.Updated.Add(changedFavorite);
            this.FireFavoriteChanges(args);
        }

        internal void ReportFavoritesUpdated(List<IFavorite> changedFavorites)
        {
            var args = new FavoritesChangedEventArgs();
            args.Updated.AddRange(changedFavorites);
            this.FireFavoriteChanges(args);
        }

        internal void ReportFavoriteDeleted(IFavorite deletedFavorite)
        {
            var args = new FavoritesChangedEventArgs();
            args.Removed.Add(deletedFavorite);
            this.FireFavoriteChanges(args);
        }

        internal void ReportFavoritesDeleted(List<IFavorite> deletedFavorites)
        {
            var args = new FavoritesChangedEventArgs();
            args.Removed.AddRange(deletedFavorites);
            this.FireFavoriteChanges(args);
        }

        private void FireFavoriteChanges(FavoritesChangedEventArgs args)
        {
            if (args.IsEmpty)
                return;

            this.DeliverFavoriteChanges(args);
        }

        private void DeliverFavoriteChanges(FavoritesChangedEventArgs args)
        {
            if (this.DelayedUpdate)
                this.favorites.AddFrom(args);
            else
                this.FireFavoritesChangedEvent(args);
        }

        private void FireFavoritesChangedEvent(FavoritesChangedEventArgs args)
        {
            // prevent persistence dummy call without start update
            if (args == null)
                return;

            Debug.WriteLine(args.ToString());

            this.FavoritesChanged?.Invoke(args);
        }

        internal void ReportGroupsAdded(List<IGroup> addedGroups)
        {
            var args = new GroupsChangedArgs();
            args.Added.AddRange(addedGroups);
            this.FireGroupsChanged(args);
        }

        internal void ReportGroupsUpdated(IList<IGroup> updatedGroups)
        {
            var args = new GroupsChangedArgs();
            args.Updated.AddRange(updatedGroups);
            this.FireGroupsChanged(args);
        }

        internal void ReportGroupsDeleted(List<IGroup> deletedGroups)
        {
            var args = new GroupsChangedArgs();
            args.Removed.AddRange(deletedGroups);
            this.FireGroupsChanged(args);
        }

        internal void ReportGroupsRecreated(List<IGroup> addedGroups, List<IGroup> deletedGroups)
        {
            var args = new GroupsChangedArgs(addedGroups, deletedGroups);
            this.FireGroupsChanged(args);
        }

        private void FireGroupsChanged(GroupsChangedArgs args)
        {
            if (args.IsEmpty)
                return;

            this.DeliverGroupsChanges(args);
        }

        private void DeliverGroupsChanges(GroupsChangedArgs args)
        {
            if (this.DelayedUpdate)
                this.groups.AddFrom(args);
            else
                this.FireGroupsChangedEvent(args);
        }

        private void FireGroupsChangedEvent(GroupsChangedArgs args)
        {
            // prevent persistence dummy call without start update
            if (args == null)
                return;

            Debug.WriteLine(args.ToString());

            this.GroupsChanged?.Invoke(args);
        }

        internal void ReportActionError<TActionParams1, TActionParams2>(Action<TActionParams1, TActionParams2> action,
            TActionParams1 actionParams1, TActionParams2 actionParams2, object sender, EntityException exception,
            string message)
        {
            this.ReportDataError(sender, exception, message);
            action(actionParams1, actionParams2);
            this.callStackCounter = 0;
        }

        internal void ReportActionError<TActionParams>(Action<TActionParams> action, TActionParams actionParams,
            object sender, EntityException exception, string message)
        {
            this.ReportDataError(sender, exception, message);
            action(actionParams);
            this.callStackCounter = 0;
        }

        internal void ReportActionError(Action action, object sender, EntityException exception, string message)
        {
            this.ReportDataError(sender, exception, message);
            action();
            this.callStackCounter = 0;
        }

        internal TFuncReturnValue ReportFunctionError<TActionParams, TFuncReturnValue>(
            Func<TActionParams, TFuncReturnValue> function,
            TActionParams actionParams, object sender, EntityException exception, string message)
        {
            this.ReportDataError(sender, exception, message);
            var returnValue = function(actionParams);
            this.callStackCounter = 0;
            return returnValue;
        }

        internal TFuncReturnValue ReportFunctionError<TFuncReturnValue>(Func<TFuncReturnValue> function,
            object sender, EntityException exception, string message)
        {
            this.ReportDataError(sender, exception, message);
            var returnValue = function();
            this.callStackCounter = 0;
            return returnValue;
        }

        private void ReportDataError(object sender, EntityException exception, string message)
        {
            this.callStackCounter++;
            // don't log the action params, because they may contain sensitive value (passwords etc.)
            Logging.Error(message, exception);
            this.FireDataErrorOccured(sender, message);
        }

        private void FireDataErrorOccured(object sender, string message)
        {
            // prevent not only not unregistered event handler, but also wrong handing resulting in call stack overflow
            if (this.ErrorOccurred == null || this.callStackCounter > 20)
                throw new ApplicationException("Terminals was not recover from previous data exception");

            var arguments = new DataErrorEventArgs
            {
                Message = message,
                CallStackFull = this.callStackCounter == 20
            };
            this.ErrorOccurred(sender, arguments);
        }
    }
}