using System.Drawing;
using System.Windows.Forms;
using Terminals.Common.Properties;
using Terminals.Data;

namespace Terminals.Connections
{
    public class Connection : Control, IConnection
    {
        // cached images, bad performace, but simplifies check, if the image is known
        public static readonly Image Terminalsicon = Resources.terminalsicon;

        public delegate void LogHandler(string entry);

        public event LogHandler OnLog;

        public delegate void Disconnected(Connection connection);

        public event Disconnected OnDisconnected;

        public string LastError { get; protected set; }

        public virtual bool Connected 
        {
            get; private set;
        }

        /// <summary>
        /// Gets or sets the associated favorite.
        /// If the connection is virtual, it doesnt have any favorite, so it can be null.
        /// </summary>
        public IFavorite Favorite { get; set; }

        /// <summary>
        /// Gets or sets the original Favorite used create this connection.
        /// It can be null in case Adhoc connection, which user doesnt want to save.
        /// </summary>
        public IFavorite OriginFavorite { get; set; }


        public IConnectionMainView ParentForm { get; set; }

        public IGuardedCredentialFactory CredentialFactory { get; set; }
        
        /// <summary>
        /// Create this control doesnt mean to open the connection.
        /// Use explicit call instead. Because there may be related resources, 
        /// call Dispose to close the connection to prevent memory leak.
        /// </summary>
        public virtual bool Connect()
        {
            return true;
        }

        protected void Log(string text)
        {
            this.OnLog?.Invoke(text);
        }

        /// <summary>
        /// Default empty implementation to be overriden by connection
        /// </summary>
        public virtual void ChangeDesktopSize(DesktopSize size)
        {
        }

        /// <summary>
        /// Avoid to call remove the tab control from connection.
        /// Instead we are going to fire event to the MainForm, which will do for us:
        /// - remove the tab
        /// - Close the connection, when necessary
        /// - and finally the expected Dispose of Disposable resources 
        /// After that the connection wouldnt need reference to the TabControl and MainForm.
        /// </summary>
        protected void FireDisconnected()
        {
            this.OnDisconnected?.Invoke(this);
        }

        protected IGuardedSecurity ResolveFavoriteCredentials()
        {
            var security = this.CredentialFactory.CreateSecurityOptoins(this.Favorite.Security);
            return security.GetResolvedCredentials();
        }
    }
}
