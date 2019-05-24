using System;
using System.Collections.Generic;
using System.Linq;
using SysConfig = System.Configuration;

namespace Terminals.Configuration
{
    internal partial class Settings
    {
        internal Guid[] FavoritesToolbarButtons
        {
            get
            {
                return this.GetSection().FavoritesButtons.ToList()
                    .Select(id => new Guid(id))
                    .ToArray();
            }
        }

        /// <summary>
        ///     For backward compatibility with older version than 2.0 for imports.
        /// </summary>
        public string[] FavoriteNamesToolbarButtons => this.GetSection().FavoritesButtons.ToList().ToArray();

        private MRUItemConfigurationElementCollection ButtonsCollection => this.GetSection().FavoritesButtons;

        internal void AddFavoriteButton(Guid favoriteId)
        {
            this.AddButtonToInternCollection(favoriteId);
            this.FireButtonsChangedEvent();
        }

        private void AddButtonToInternCollection(Guid favoriteId)
        {
            this.ButtonsCollection.AddByName(favoriteId.ToString());
            this.SaveImmediatelyIfRequested();
        }

        internal void UpdateFavoritesToolbarButtons(List<Guid> newFavoriteIds)
        {
            this.StartDelayedUpdate();
            this.ButtonsCollection.Clear();
            foreach (var favoriteId in newFavoriteIds)
                this.AddButtonToInternCollection(favoriteId);
            this.SaveAndFinishDelayedUpdate();
            this.FireButtonsChangedEvent();
        }

        internal void EditFavoriteButton(Guid oldFavoriteId, Guid newFavoriteId, bool showOnToolbar)
        {
            this.DeleteFavoriteButton(oldFavoriteId);

            var hasToolbarButton = this.HasToolbarButton(newFavoriteId);
            if (hasToolbarButton && !showOnToolbar)
                this.DeleteFavoriteButton(newFavoriteId);
            else if (showOnToolbar)
                this.AddFavoriteButton(newFavoriteId);
        }

        private void DeleteFavoriteButton(Guid favoriteId)
        {
            this.ButtonsCollection.DeleteByName(favoriteId.ToString());
            this.SaveImmediatelyIfRequested();
            this.FireButtonsChangedEvent();
        }

        internal bool HasToolbarButton(Guid favoriteId)
        {
            return this.FavoritesToolbarButtons.Contains(favoriteId);
        }

        private void FireButtonsChangedEvent()
        {
            var args = ConfigurationChangedEventArgs.CreateFromButtons();
            this.FireConfigurationChanged(args);
        }
    }
}