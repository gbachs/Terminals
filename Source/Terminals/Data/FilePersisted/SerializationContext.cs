namespace Terminals.Data.FilePersisted
{
    internal class SerializationContext
    {
        public SerializationContext()
            : this(new FavoritesFile(), new UnknonwPluginElements())
        {
        }

        public SerializationContext(FavoritesFile file, UnknonwPluginElements unknown)
        {
            this.File = file;
            this.Unknown = unknown;
        }

        internal FavoritesFile File { get; }

        internal UnknonwPluginElements Unknown { get; }

        public override string ToString()
        {
            var favoritesCount = this.File.Favorites.Length;
            var unknownCount = this.Unknown.Favorites.Count;
            return string.Format("SerializationContext:Unknowns='{0}',Known='{1}'", unknownCount, favoritesCount);
        }
    }
}