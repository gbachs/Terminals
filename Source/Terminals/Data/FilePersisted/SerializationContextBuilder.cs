using System.Collections.Generic;
using System.Linq;
using Terminals.Data.FilePersisted;

namespace Terminals.Data
{
    internal class SerializationContextBuilder
    {
        private readonly DataDispatcher dispatcher;

        private readonly Favorites favorites;

        private readonly Groups groups;

        public SerializationContextBuilder(Groups groups, Favorites favorites, DataDispatcher dispatcher)
        {
            this.groups = groups;
            this.favorites = favorites;
            this.dispatcher = dispatcher;
        }

        internal SerializationContext CreateDataFromCache(UnknonwPluginElements cachedUnknown)
        {
            var file = new FavoritesFile
            {
                Favorites = this.favorites.Cast<Favorite>().ToArray(),
                Groups = this.groups.Cast<Group>().ToArray(),
                FavoritesInGroups = this.GetFavoriteInGroups()
            };
            return new SerializationContext(file, cachedUnknown);
        }

        private FavoritesInGroup[] GetFavoriteInGroups()
        {
            var references = new List<FavoritesInGroup>();
            foreach (Group group in this.groups)
            {
                var groupReferences = group.GetGroupReferences();
                references.Add(groupReferences);
            }

            return references.ToArray();
        }

        internal void AssignServices(FavoritesFile file)
        {
            this.AssignServices(file.Favorites);
            this.AssignServices(file.Groups);
        }

        private void AssignServices(Group[] fileGroups)
        {
            foreach (var group in fileGroups)
                group.AssignStores(this.groups, this.dispatcher);
        }

        private void AssignServices(Favorite[] fileFavorites)
        {
            foreach (var favorite in fileFavorites)
                favorite.AssignStores(this.groups);
        }
    }
}