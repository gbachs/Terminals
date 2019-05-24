﻿using System;
using System.Collections.Generic;

namespace Terminals.Data
{
    /// <summary>
    ///     Favorites changed event arguments container, informing about changes in Favorites collection
    /// </summary>
    internal class FavoritesChangedEventArgs : EventArgs
    {
        internal FavoritesChangedEventArgs()
        {
            this.Added = new List<IFavorite>();
            this.Removed = new List<IFavorite>();
            this.Updated = new List<IFavorite>();
        }

        /// <summary>
        ///     Newly added favorites to the persisted database
        /// </summary>
        internal List<IFavorite> Added { get; }

        /// <summary>
        ///     All favorites actually no longer stored in favorites persisted database
        /// </summary>
        internal List<IFavorite> Removed { get; }

        /// <summary>
        ///     Already stored favorites, where at least one property was changed.
        /// </summary>
        internal List<IFavorite> Updated { get; }

        /// <summary>
        ///     Gets the value indicating if there are any added or removed items to report.
        /// </summary>
        internal bool IsEmpty =>
            this.Added.Count == 0 &&
            this.Removed.Count == 0 &&
            this.Updated.Count == 0;

        internal void AddFrom(FavoritesChangedEventArgs source)
        {
            var toAdd = ListsHelper.GetMissingSourcesInTarget(source.Added, this.Added);
            this.Added.AddRange(toAdd);
            var toUpdate = ListsHelper.GetMissingSourcesInTarget(source.Updated, this.Updated);
            this.Updated.AddRange(toUpdate);
            var toRemove = ListsHelper.GetMissingSourcesInTarget(source.Removed, this.Removed);
            this.Removed.AddRange(toRemove);
        }

        public override string ToString()
        {
            return string.Format("FavoritesChangedEventArgs:Added={0};Removed={1};Changed={2}",
                this.Added.Count, this.Removed.Count, this.Updated.Count);
        }
    }
}