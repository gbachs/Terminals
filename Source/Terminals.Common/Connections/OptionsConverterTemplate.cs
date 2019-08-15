namespace Terminals.Common.Connections
{
    public abstract class OptionsConverterTemplate<TOptions> : IOptionsConverter
        where TOptions : class
    {
        public void FromConfigFavorite(OptionsConversionContext context)
        {
            if (context.Favorite.ProtocolProperties is TOptions options)
            {
                this.FromConfigFavorite(context.ConfigFavorite, options);
            }
        }

        protected abstract void FromConfigFavorite(FavoriteConfigurationElement source, TOptions options);

        public void ToConfigFavorite(OptionsConversionContext context)
        {
            if (context.Favorite.ProtocolProperties is TOptions options)
            {
                this.ToConfigFavorite(context.ConfigFavorite, options);
            }
        }

        protected abstract void ToConfigFavorite(FavoriteConfigurationElement destination, TOptions options);
    }
}