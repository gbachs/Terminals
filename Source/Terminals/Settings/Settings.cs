using System;
using System.Linq;
using Terminals.Common.Configuration;
using Terminals.Data;
using Terminals.Security;

namespace Terminals.Configuration
{
    internal partial class Settings : IConnectionSettings, IMRUSettings, IPluginSettings, IBeforeConnectExecuteOptions
    {
        public Version ConfigVersion
        {
            get
            {
                var configVersion = this.GetSection().ConfigVersion;
                if (configVersion != string.Empty)
                    return new Version(configVersion);

                return null;
            }
            set
            {
                this.GetSection().ConfigVersion = value.ToString();
                this.SaveImmediatelyIfRequested();
            }
        }

        #region Flickr tab settings

        public string FlickrToken
        {
            get => this.GetSection().FlickrToken;
            set
            {
                this.GetSection().FlickrToken = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        #endregion

        public bool AskToReconnect
        {
            get => this.GetSection().AskToReconnect;
            set
            {
                this.GetSection().AskToReconnect = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public string[] DisabledPlugins
        {
            get => this.GetSection().DisabledPlugins.ToSortedArray();
            set => this.UpdateEnabledPlugins(value);
        }

        public void UpdateEnabledPlugins(string[] disabledPlugins)
        {
            var pluginsSection = this.GetSection().DisabledPlugins;
            pluginsSection.Clear();

            foreach (var disabledPlugin in disabledPlugins)
                pluginsSection.AddByName(disabledPlugin);

            this.SaveImmediatelyIfRequested();
        }

        #region General tab settings

        public bool NeverShowTerminalsWindow
        {
            get => this.GetSection().NeverShowTerminalsWindow;
            set
            {
                this.GetSection().NeverShowTerminalsWindow = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool ShowUserNameInTitle
        {
            get => this.GetSection().ShowUserNameInTitle;
            set
            {
                this.GetSection().ShowUserNameInTitle = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool ShowInformationToolTips
        {
            get => this.GetSection().ShowInformationToolTips;
            set
            {
                this.GetSection().ShowInformationToolTips = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool ShowFullInformationToolTips
        {
            get => this.GetSection().ShowFullInformationToolTips;
            set
            {
                this.GetSection().ShowFullInformationToolTips = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool SingleInstance
        {
            get => this.GetSection().SingleInstance;
            set
            {
                this.GetSection().SingleInstance = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool ShowConfirmDialog
        {
            get => this.GetSection().ShowConfirmDialog;
            set
            {
                this.GetSection().ShowConfirmDialog = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool SaveConnectionsOnClose
        {
            get => this.GetSection().SaveConnectionsOnClose;
            set
            {
                this.GetSection().SaveConnectionsOnClose = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool MinimizeToTray
        {
            get => this.GetSection().MinimizeToTray;
            set
            {
                this.GetSection().MinimizeToTray = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        // Validate server names
        public bool ForceComputerNamesAsURI
        {
            get => this.GetSection().ForceComputerNamesAsURI;
            set
            {
                this.GetSection().ForceComputerNamesAsURI = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool WarnOnConnectionClose
        {
            get => this.GetSection().WarnOnConnectionClose;
            set
            {
                this.GetSection().WarnOnConnectionClose = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool AutoCaseTags
        {
            get => this.GetSection().AutoCaseTags;
            set
            {
                this.GetSection().AutoCaseTags = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public string DefaultDesktopShare
        {
            get => this.GetSection().DefaultDesktopShare;
            set
            {
                this.GetSection().DefaultDesktopShare = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public int PortScanTimeoutSeconds
        {
            get => this.GetSection().PortScanTimeoutSeconds;
            set
            {
                this.GetSection().PortScanTimeoutSeconds = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        #endregion

        #region Execute Before Connect tab settings

        public bool Execute
        {
            get => this.GetSection().ExecuteBeforeConnect;
            set
            {
                this.GetSection().ExecuteBeforeConnect = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public string Command
        {
            get => this.GetSection().ExecuteBeforeConnectCommand;
            set
            {
                this.GetSection().ExecuteBeforeConnectCommand = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public string CommandArguments
        {
            get => this.GetSection().ExecuteBeforeConnectArgs;
            set
            {
                this.GetSection().ExecuteBeforeConnectArgs = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public string InitialDirectory
        {
            get => this.GetSection().ExecuteBeforeConnectInitialDirectory;
            set
            {
                this.GetSection().ExecuteBeforeConnectInitialDirectory = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool WaitForExit
        {
            get => this.GetSection().ExecuteBeforeConnectWaitForExit;
            set
            {
                this.GetSection().ExecuteBeforeConnectWaitForExit = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        #endregion

        #region Security

        /// <summary>
        ///     Gets or sets the stored master password key, getter and setter don't make any encryption.
        /// </summary>
        internal string MasterPasswordHash
        {
            get => this.GetSection().TerminalsPassword;
            private set => this.GetSection().TerminalsPassword = value;
        }

        /// <summary>
        ///     This updates all stored passwords and assigns new key material in config section.
        /// </summary>
        internal void UpdateConfigurationPasswords(string newMasterKey, string newStoredMasterKey)
        {
            this.MasterPasswordHash = newStoredMasterKey;
            this.UpdateStoredPasswords(newMasterKey);
            this.SaveImmediatelyIfRequested();
        }

        private void UpdateStoredPasswords(string newKeyMaterial)
        {
            var configSection = this.GetSection();
            configSection.EncryptedDefaultPassword =
                PasswordFunctions2.EncryptPassword(this.DefaultPassword, newKeyMaterial);
            configSection.EncryptedAmazonAccessKey =
                PasswordFunctions2.EncryptPassword(this.AmazonAccessKey, newKeyMaterial);
            configSection.EncryptedAmazonSecretKey =
                PasswordFunctions2.EncryptPassword(this.AmazonSecretKey, newKeyMaterial);
            configSection.EncryptedConnectionString =
                PasswordFunctions2.EncryptPassword(this.ConnectionString, newKeyMaterial);
            configSection.DatabaseMasterPasswordHash =
                PasswordFunctions2.EncryptPassword(this.DatabaseMasterPassword, newKeyMaterial);
        }

        #endregion

        #region Security tab settings

        public string DefaultDomain
        {
            get
            {
                var encryptedDefaultDomain = this.GetSection().DefaultDomain;
                return this.PersistenceSecurity.DecryptPassword(encryptedDefaultDomain);
            }
            set
            {
                this.GetSection().DefaultDomain = this.PersistenceSecurity.EncryptPassword(value);
                this.SaveImmediatelyIfRequested();
            }
        }

        public string DefaultUsername
        {
            get
            {
                var encryptedDefaultUserName = this.GetSection().DefaultUsername;
                return this.PersistenceSecurity.DecryptPassword(encryptedDefaultUserName);
            }
            set
            {
                this.GetSection().DefaultUsername = this.PersistenceSecurity.EncryptPassword(value);
                this.SaveImmediatelyIfRequested();
            }
        }

        internal string DefaultPassword
        {
            get
            {
                var encryptedDefaultPassword = this.GetSection().EncryptedDefaultPassword;
                return this.PersistenceSecurity.DecryptPassword(encryptedDefaultPassword);
            }
            set
            {
                this.GetSection().EncryptedDefaultPassword = this.PersistenceSecurity.EncryptPassword(value);
                this.SaveImmediatelyIfRequested();
            }
        }

        /// <summary>
        ///     Gets or sets authentication instance used to encrypt and decrypt secured settings.
        ///     Set only initialized and authenticated instance before access to any data.
        /// </summary>
        internal PersistenceSecurity PersistenceSecurity { get; set; }

        internal bool UseAmazon
        {
            get => this.GetSection().UseAmazon;
            set
            {
                this.GetSection().UseAmazon = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        internal string AmazonAccessKey
        {
            get
            {
                var encryptedAmazonAccessKey = this.GetSection().EncryptedAmazonAccessKey;
                return this.PersistenceSecurity.DecryptPassword(encryptedAmazonAccessKey);
            }
            set
            {
                this.GetSection().EncryptedAmazonAccessKey = this.PersistenceSecurity.EncryptPassword(value);
                this.SaveImmediatelyIfRequested();
            }
        }

        internal string AmazonSecretKey
        {
            get
            {
                var encryptedAmazonSecretKey = this.GetSection().EncryptedAmazonSecretKey;
                return this.PersistenceSecurity.DecryptPassword(encryptedAmazonSecretKey);
            }
            set
            {
                this.GetSection().EncryptedAmazonSecretKey = this.PersistenceSecurity.EncryptPassword(value);
                this.SaveImmediatelyIfRequested();
            }
        }

        internal string AmazonBucketName
        {
            get => this.GetSection().AmazonBucketName;
            set
            {
                this.GetSection().AmazonBucketName = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        #endregion

        #region Proxy tab settings

        public bool UseProxy
        {
            get => this.GetSection().UseProxy;
            set
            {
                this.GetSection().UseProxy = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public string ProxyAddress
        {
            get => this.GetSection().ProxyAddress;
            set
            {
                this.GetSection().ProxyAddress = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public int ProxyPort
        {
            get => this.GetSection().ProxyPort;
            set
            {
                this.GetSection().ProxyPort = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        #endregion

        #region Screen capture tab settings

        public bool EnableCaptureToClipboard
        {
            get => this.GetSection().EnableCaptureToClipboard;
            set
            {
                this.GetSection().EnableCaptureToClipboard = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool EnableCaptureToFolder
        {
            get => this.GetSection().EnableCaptureToFolder;
            set
            {
                this.GetSection().EnableCaptureToFolder = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        internal bool EnabledCaptureToFolderAndClipBoard => this.EnableCaptureToClipboard || this.EnableCaptureToFolder;

        public bool AutoSwitchOnCapture
        {
            get => this.GetSection().AutoSwitchOnCapture;
            set
            {
                this.GetSection().AutoSwitchOnCapture = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public string CaptureRoot
        {
            get
            {
                var root = this.GetSection().CaptureRoot;
                if (string.IsNullOrEmpty(root))
                    root = FileLocations.DefaultCaptureRootDirectory;

                return root;
            }
            set
            {
                this.GetSection().CaptureRoot = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        #endregion

        #region More tab settings

        public bool RestoreWindowOnLastTerminalDisconnect
        {
            get => this.GetSection().RestoreWindowOnLastTerminalDisconnect;
            set
            {
                this.GetSection().RestoreWindowOnLastTerminalDisconnect = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool EnableFavoritesPanel
        {
            get => this.GetSection().EnableFavoritesPanel;
            set
            {
                this.GetSection().EnableFavoritesPanel = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool EnableGroupsMenu
        {
            get => this.GetSection().EnableGroupsMenu;
            set
            {
                this.GetSection().EnableGroupsMenu = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool AutoExapandTagsPanel
        {
            get => this.GetSection().AutoExapandTagsPanel;
            set
            {
                this.GetSection().AutoExapandTagsPanel = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public SortProperties DefaultSortProperty
        {
            get
            {
                var config = this.GetSection();
                if (config != null)
                {
                    var dsp = config.DefaultSortProperty;
                    var prop = (SortProperties)Enum.Parse(typeof(SortProperties), dsp);
                    return prop;
                }

                return SortProperties.ConnectionName;
            }
            set
            {
                this.GetSection().DefaultSortProperty = value.ToString();
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool Office2007BlackFeel
        {
            get => this.GetSection().Office2007BlackFeel;
            set
            {
                this.GetSection().Office2007BlackFeel = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool Office2007BlueFeel
        {
            get => this.GetSection().Office2007BlueFeel;
            set
            {
                this.GetSection().Office2007BlueFeel = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        #endregion

        #region Vnc settings

        public bool VncAutoScale
        {
            get => this.GetSection().VncAutoScale;
            set
            {
                this.GetSection().VncAutoScale = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool VncViewOnly
        {
            get => this.GetSection().VncViewOnly;
            set
            {
                this.GetSection().VncViewOnly = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public int VncDisplayNumber
        {
            get => this.GetSection().VncDisplayNumber;
            set
            {
                this.GetSection().VncDisplayNumber = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        #endregion

        #region Mainform control settings

        public int FavoritePanelWidth
        {
            get => this.GetSection().FavoritePanelWidth;
            set
            {
                this.GetSection().FavoritePanelWidth = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool ShowFavoritePanel
        {
            get => this.GetSection().ShowFavoritePanel;
            set
            {
                this.GetSection().ShowFavoritePanel = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool ToolbarsLocked
        {
            get => this.GetSection().ToolbarsLocked;
            set
            {
                this.GetSection().ToolbarsLocked = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        #endregion

        #region Startup settings

        public string UpdateSource
        {
            get => this.GetSection().UpdateSource;
            set
            {
                this.GetSection().UpdateSource = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public bool ShowWizard
        {
            get => this.GetSection().ShowWizard;
            set
            {
                this.GetSection().ShowWizard = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public string PsexecLocation
        {
            get => this.GetSection().PsexecLocation;
            set
            {
                this.GetSection().PsexecLocation = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public string SavedCredentialsLocation
        {
            get => this.GetSection().SavedCredentialsLocation;
            set
            {
                this.GetSection().SavedCredentialsLocation = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public string SavedFavoritesFileLocation
        {
            get => this.GetSection().SavedFavoritesFileLocation;
            set
            {
                this.GetSection().SavedFavoritesFileLocation = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        #endregion

        #region MRU lists

        public string[] MRUServerNames => this.GetSection().ServersMRU.ToSortedArray();

        public string[] MRUDomainNames => this.GetSection().DomainsMRU.ToSortedArray();

        public string[] MRUUserNames => this.GetSection().UsersMRU.ToSortedArray();

        public string[] SavedSearches
        {
            get => this.GetSection().SearchesMRU.ToSortedArray();
            set
            {
                var newSearches = new MRUItemConfigurationElementCollection(value.Distinct());
                this.GetSection().SearchesMRU = newSearches;
            }
        }

        #endregion

        #region Tags/Favorite lists Settings

        public string ExpandedFavoriteNodes
        {
            get => this.GetSection().ExpandedFavoriteNodes;
            set
            {
                this.GetSection().ExpandedFavoriteNodes = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public string ExpandedHistoryNodes
        {
            get => this.GetSection().ExpandedHistoryNodes;
            set
            {
                this.GetSection().ExpandedHistoryNodes = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        #endregion

        #region Persistence File/Sql database

        /// <summary>
        ///     Gets or sets encrypted entity framework connection string
        /// </summary>
        internal string ConnectionString
        {
            get
            {
                var encryptedConnectionString = this.GetSection().EncryptedConnectionString;
                return this.PersistenceSecurity.DecryptPassword(encryptedConnectionString);
            }
            set
            {
                this.GetSection().EncryptedConnectionString = this.PersistenceSecurity.EncryptPassword(value);
                this.SaveImmediatelyIfRequested();
            }
        }

        /// <summary>
        ///     Gets or sets bidirectional encrypted database password. We need it in unencrypted form
        ///     to be able authenticate against the database and don't prompt user for it.
        /// </summary>
        internal string DatabaseMasterPassword
        {
            get
            {
                var databaseMasterPasswordHash = this.GetSection().DatabaseMasterPasswordHash;
                return this.PersistenceSecurity.DecryptPassword(databaseMasterPasswordHash);
            }
            set
            {
                this.GetSection().DatabaseMasterPasswordHash = this.PersistenceSecurity.EncryptPassword(value);
                this.SaveImmediatelyIfRequested();
            }
        }

        /// <summary>
        ///     Gets or sets the value identifying the persistence.
        ///     0 by default - file persisted data, 1 - SQL database
        /// </summary>
        internal byte PersistenceType
        {
            get => this.GetSection().PersistenceType;
            set
            {
                this.GetSection().PersistenceType = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        #endregion

        #region Public

        public void AddServerMRUItem(string name)
        {
            this.GetSection().ServersMRU.AddByName(name);
            this.SaveImmediatelyIfRequested();
        }

        public void AddDomainMRUItem(string name)
        {
            this.GetSection().DomainsMRU.AddByName(name);
            this.SaveImmediatelyIfRequested();
        }

        public void AddUserMRUItem(string name)
        {
            this.GetSection().UsersMRU.AddByName(name);
            this.SaveImmediatelyIfRequested();
        }

        public void AddConnection(string name)
        {
            this.GetSection().SavedConnections.AddByName(name);
            this.SaveImmediatelyIfRequested();
        }

        public SpecialCommandConfigurationElementCollection SpecialCommands
        {
            get => this.GetSection().SpecialCommands;
            set
            {
                this.GetSection().SpecialCommands = value;
                this.SaveImmediatelyIfRequested();
            }
        }

        public void CreateSavedConnectionsList(string[] names)
        {
            this.GetSection().SavedConnections.Clear();
            this.SaveImmediatelyIfRequested();
            foreach (var name in names)
                this.AddConnection(name);
        }

        public void ClearSavedConnectionsList()
        {
            this.GetSection().SavedConnections.Clear();
            this.SaveImmediatelyIfRequested();
        }

        public string[] SavedConnections => this.GetSection().SavedConnections.ToList().ToArray();

        public KeysSection SSHKeys
        {
            get
            {
                var keys = this.Config.Sections["SSH"] as KeysSection;
                if (keys == null)
                {
                    // The section wasn't found, so add it.
                    keys = new KeysSection();
                    this.Config.Sections.Add("SSH", keys);
                }

                return keys;
            }
        }

        internal FormsSection Forms
        {
            get
            {
                var formsSection = this.Config.Sections[FormsSection.FORMS] as FormsSection;
                if (formsSection == null)
                {
                    // The section wasn't found, so add it.
                    formsSection = new FormsSection();
                    this.Config.Sections.Add(FormsSection.FORMS, formsSection);
                }

                return formsSection;
            }
        }

        #endregion
    }
}