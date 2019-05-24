using System.Collections.Generic;
using Terminals.Configuration;

namespace Terminals.Converters
{
    internal class TagsConverter
    {
        internal List<string> ResolveTagsList(FavoriteConfigurationElement favorite)
        {
            var tagList = new List<string>();
            var tags = this.ResolveTags(favorite);
            var splittedTags = tags.Split(',');

            if (!(splittedTags.Length == 1 && string.IsNullOrEmpty(splittedTags[0])))
                tagList.AddRange(splittedTags);

            return tagList;
        }

        internal string ResolveTags(FavoriteConfigurationElement favorite)
        {
            if (Settings.Instance.AutoCaseTags)
                return TextConverter.ToTitleCase(favorite.Tags);

            return favorite.Tags;
        }
    }
}