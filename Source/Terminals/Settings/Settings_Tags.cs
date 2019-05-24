using System.Collections.Generic;
using System.Linq;
using Terminals.Converters;
using SysConfig = System.Configuration;

namespace Terminals.Configuration
{
    internal partial class Settings
    {
        /// <summary>
        ///     Gets alphabeticaly sorted array of tags resolved from Tags store.
        ///     Since version 2. only for updates. Use new persistence instead.
        /// </summary>
        internal string[] Tags
        {
            get
            {
                var tags = this.GetSection().Tags.ToList();
                tags.Sort();
                return tags.ToArray();
            }
        }

        private void AddTagsToSettings(List<string> tags)
        {
            foreach (var tag in tags)
            {
                if (string.IsNullOrEmpty(tag))
                    continue;

                this.AddTagToSettings(tag);
            }
        }

        /// <summary>
        ///     Adds tag to the tags collection if it already isnt there.
        ///     If the tag is in database, than it returns empty string, otherwise the commited tag.
        /// </summary>
        private void AddTagToSettings(string tag)
        {
            if (this.AutoCaseTags)
                tag = TextConverter.ToTitleCase(tag);
            if (this.Tags.Contains(tag))
                return;

            this.GetSection().Tags.AddByName(tag);
            this.SaveImmediatelyIfRequested();
        }

        private void DeleteTags()
        {
            var tagsToDelete = this.Tags.ToList();
            this.DeleteTagsFromSettings(tagsToDelete);
        }

        private List<string> DeleteTagsFromSettings(List<string> tagsToDelete)
        {
            var deletedTags = new List<string>();
            foreach (var tagTodelete in tagsToDelete)
            {
                if (this.GetNumberOfFavoritesUsingTag(tagTodelete) > 0)
                    continue;

                deletedTags.Add(this.DeleteTagFromSettings(tagTodelete));
            }

            return deletedTags;
        }

        /// <summary>
        ///     Removes the tag from settings, but doesnt send the Tag removed event
        /// </summary>
        private string DeleteTagFromSettings(string tagToDelete)
        {
            if (this.AutoCaseTags)
                tagToDelete = TextConverter.ToTitleCase(tagToDelete);
            this.GetSection().Tags.DeleteByName(tagToDelete);
            this.SaveImmediatelyIfRequested();
            return tagToDelete;
        }

        private int GetNumberOfFavoritesUsingTag(string tagToRemove)
        {
            var favorites = this.GetFavorites().ToList();
            return favorites.Count(favorite => this.tagsConverter.ResolveTagsList(favorite).Contains(tagToRemove));
        }
    }
}