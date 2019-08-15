using TabControl;
using Terminals.Connections;
using Terminals.Data;

namespace Terminals
{
    internal class TerminalTabControlItem : TabControlItem
    {
        /// <summary>
        /// Gets or sets the associated favorite.
        /// Not all tabs are using it, so it is optional.
        /// </summary>
        public IConnection Connection { get; set; }

        /// <summary>
        /// Gets or sets the associated configured favorite.
        /// Not all tabs are using it, so it is optional.
        /// Differs from the original connection used to open the connection.
        /// </summary>
        public IFavorite Favorite
        {
            get
            {
                return Connection?.Favorite;
            }
        }

        public IFavorite OriginFavorite
        {
            get
            {
                return Connection?.OriginFavorite;
            }
        }

        public TerminalTabControlItem(string caption)
            : base(caption, null)
        {
        }
    }
}
