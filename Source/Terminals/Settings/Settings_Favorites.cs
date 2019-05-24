using System.Collections.Generic;
using Terminals.Converters;

namespace Terminals.Configuration
{
    internal partial class Settings
    {
        private readonly TagsConverter tagsConverter = new TagsConverter();

        /// <summary>
        ///     "Since version 2. only for updates. Use new persistence instead."
        /// </summary>
        internal void RemoveAllFavoritesAndTags()
        {
            this.DeleteFavorites();
            this.DeleteTags();
        }

        private void DeleteFavorites()
        {
            List<FavoriteConfigurationElement> favorites = this.GetFavorites().ToList();
            var deletedTags = new List<string>();

            this.StartDelayedUpdate();

            foreach (var favorite in favorites)
            {
                this.DeleteFavoriteFromSettings(favorite.Name);
                var tagList = this.tagsConverter.ResolveTagsList(favorite);
                var difference = this.DeleteTagsFromSettings(tagList);
                deletedTags.AddRange(difference);
            }

            this.SaveAndFinishDelayedUpdate();
        }

        private void DeleteFavoriteFromSettings(string name)
        {
            this.GetSection().Favorites.Remove(name);
            this.SaveImmediatelyIfRequested();
        }

        private void AddFavorite(FavoriteConfigurationElement favorite)
        {
            this.AddFavoriteToSettings(favorite);
            this.AddTagsToSettings(this.tagsConverter.ResolveTagsList(favorite));
        }

        /// <summary>
        ///     Adds favorite to the database, but doesn't fire the changed event
        /// </summary>
        private void AddFavoriteToSettings(FavoriteConfigurationElement favorite)
        {
            this.GetSection().Favorites.Add(favorite);
            this.SaveImmediatelyIfRequested();
        }

        internal FavoriteConfigurationElement GetDefaultFavorite()
        {
            var section = this.GetSection();
            if (section != null && section.DefaultFavorite.Count > 0)
                return section.DefaultFavorite[0];
            return null;
        }

        internal void SaveDefaultFavorite(FavoriteConfigurationElement favorite)
        {
            var defaultFav = this.GetSection().DefaultFavorite;
            defaultFav.Clear();
            defaultFav.Add(favorite);
            this.SaveImmediatelyIfRequested();
        }

        internal void RemoveDefaultFavorite()
        {
            var defaultFav = this.GetSection().DefaultFavorite;
            defaultFav.Clear();
            this.SaveImmediatelyIfRequested();
        }

        internal FavoriteConfigurationElementCollection GetFavorites()
        {
            var section = this.GetSection();
            if (section != null)
                return section.Favorites;
            return null;
        }
    }
}