using System;
using System.Xml.Serialization;

namespace Terminals.Data
{
    /// <summary>
    ///     Represents xml file persisted favorites data and their groups
    /// </summary>
    [Serializable]
    [XmlRoot(Namespace = SERIALIZATION_DEFAULT_NAMESPACE)]
    public class FavoritesFile
    {
        public const string SERIALIZATION_DEFAULT_NAMESPACE = "http://Terminals.codeplex.com";

        public FavoritesFile()
        {
            this.FileVerion = "2.0";
            this.Favorites = new Favorite[0];
            this.Groups = new Group[0];
            this.FavoritesInGroups = new FavoritesInGroup[0];
        }

        [XmlAttribute] public string FileVerion { get; set; }

        public Favorite[] Favorites { get; set; }

        public Group[] Groups { get; set; }

        public FavoritesInGroup[] FavoritesInGroups { get; set; }

        public override string ToString()
        {
            var favoritesCount = this.Favorites.Length;
            var groupsCount = this.Groups.Length;
            return string.Format("FavoritesFile:Favorites='{0}',Groups='{1}'", favoritesCount, groupsCount);
        }
    }
}