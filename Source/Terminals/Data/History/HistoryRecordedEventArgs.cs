using System;
using Terminals.Data;

namespace Terminals.History
{
    /// <summary>
    ///     Event arguments informing about favorite added to the history
    /// </summary>
    internal class HistoryRecordedEventArgs : EventArgs
    {
        internal HistoryRecordedEventArgs(IFavorite favorite)
        {
            this.Favorite = favorite;
        }

        /// <summary>
        ///     Gets or sets the favorite name added to the history
        /// </summary>
        internal IFavorite Favorite { get; }
    }
}