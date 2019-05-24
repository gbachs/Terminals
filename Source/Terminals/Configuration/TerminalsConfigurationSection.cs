using System;
using System.Configuration;

namespace Terminals
{
    public class TerminalsConfigurationSection : ConfigurationSection
    {
        private const string DISABLED_PLUGINS_NAME = "disabledPlugins";

        #region Terminals Version

        [ConfigurationProperty("ConfigVersion")]
        public string ConfigVersion { get => (string)this["ConfigVersion"]; set => this["ConfigVersion"] = value; }

        #endregion

        #region Flickr section

        [ConfigurationProperty("flickrToken", DefaultValue = "")]
        public string FlickrToken { get => Convert.ToString(this["flickrToken"]); set => this["flickrToken"] = value; }

        #endregion

        [ConfigurationProperty("specialCommands")]
        [ConfigurationCollection(typeof(SpecialCommandConfigurationElement))]
        public SpecialCommandConfigurationElementCollection SpecialCommands
        {
            get => (SpecialCommandConfigurationElementCollection)this["specialCommands"];
            set => this["specialCommands"] = value;
        }

        [ConfigurationProperty("savedConnectionsList")]
        [ConfigurationCollection(typeof(MRUItemConfigurationElementCollection))]
        public MRUItemConfigurationElementCollection SavedConnections
        {
            get => (MRUItemConfigurationElementCollection)this["savedConnectionsList"];
            set => this["savedConnectionsList"] = value;
        }

        [ConfigurationProperty(DISABLED_PLUGINS_NAME)]
        [ConfigurationCollection(typeof(MRUItemConfigurationElementCollection))]
        public MRUItemConfigurationElementCollection DisabledPlugins
        {
            get => (MRUItemConfigurationElementCollection)this[DISABLED_PLUGINS_NAME];
            set => this[DISABLED_PLUGINS_NAME] = value;
        }

        [ConfigurationProperty("savedCredentials", DefaultValue = "")]
        public string SavedCredentialsLocation
        {
            get => (string)this["savedCredentials"];
            set => this["savedCredentials"] = value;
        }

        [ConfigurationProperty("favoritesFile", DefaultValue = "")]
        public string SavedFavoritesFileLocation
        {
            get => (string)this["favoritesFile"];
            set => this["favoritesFile"] = value;
        }

        #region General section

        [ConfigurationProperty("NeverShowTerminalsWindow")]
        public bool NeverShowTerminalsWindow
        {
            get => (bool)this["NeverShowTerminalsWindow"];
            set => this["NeverShowTerminalsWindow"] = value;
        }

        [ConfigurationProperty("showUserNameInTitle")]
        public bool ShowUserNameInTitle
        {
            get => (bool)this["showUserNameInTitle"];
            set => this["showUserNameInTitle"] = value;
        }

        [ConfigurationProperty("showInformationToolTips")]
        public bool ShowInformationToolTips
        {
            get => (bool)this["showInformationToolTips"];
            set => this["showInformationToolTips"] = value;
        }

        [ConfigurationProperty("showFullInformationToolTips")]
        public bool ShowFullInformationToolTips
        {
            get => (bool)this["showFullInformationToolTips"];
            set => this["showFullInformationToolTips"] = value;
        }

        [ConfigurationProperty("singleInstance")]
        public bool SingleInstance { get => (bool)this["singleInstance"]; set => this["singleInstance"] = value; }

        [ConfigurationProperty("showConfirmDialog")]
        public bool ShowConfirmDialog
        {
            get => (bool)this["showConfirmDialog"];
            set => this["showConfirmDialog"] = value;
        }

        [ConfigurationProperty("askToReconnect", DefaultValue = true)]
        public bool AskToReconnect { get => (bool)this["askToReconnect"]; set => this["askToReconnect"] = value; }

        [ConfigurationProperty("saveConnectionsOnClose")]
        public bool SaveConnectionsOnClose
        {
            get => (bool)this["saveConnectionsOnClose"];
            set => this["saveConnectionsOnClose"] = value;
        }

        [ConfigurationProperty("minimizeToTray", DefaultValue = true)]
        public bool MinimizeToTray
        {
            get
            {
                if (this["minimizeToTray"] == null || this["minimizeToTray"].ToString() == string.Empty) return true;
                var min = true;
                bool.TryParse(this["minimizeToTray"].ToString(), out min);
                return min;
            }
            set => this["minimizeToTray"] = value;
        }

        [ConfigurationProperty("forceComputerNamesAsURI", DefaultValue = true)]
        public bool ForceComputerNamesAsURI
        {
            get
            {
                if (this["forceComputerNamesAsURI"] == null ||
                    this["forceComputerNamesAsURI"].ToString() == string.Empty) return true;
                var min = true;
                bool.TryParse(this["forceComputerNamesAsURI"].ToString(), out min);
                return min;
            }
            set => this["forceComputerNamesAsURI"] = value;
        }

        [ConfigurationProperty("warnOnConnectionClose", DefaultValue = true)]
        public bool WarnOnConnectionClose
        {
            get
            {
                if (this["warnOnConnectionClose"] == null ||
                    this["warnOnConnectionClose"].ToString() == string.Empty) return true;
                var min = true;
                bool.TryParse(this["warnOnConnectionClose"].ToString(), out min);
                return min;
            }
            set => this["warnOnConnectionClose"] = value;
        }

        [ConfigurationProperty("autoCaseTags")]
        public bool AutoCaseTags { get => (bool)this["autoCaseTags"]; set => this["autoCaseTags"] = value; }

        [ConfigurationProperty("defaultDesktopShare")]
        public string DefaultDesktopShare
        {
            get => (string)this["defaultDesktopShare"];
            set => this["defaultDesktopShare"] = value;
        }

        [ConfigurationProperty("portScanTimeoutSeconds", DefaultValue = 5)]
        public int PortScanTimeoutSeconds
        {
            get
            {
                var timeout = 5;
                if (this["portScanTimeoutSeconds"] != null && this["portScanTimeoutSeconds"].ToString() != string.Empty)
                    int.TryParse(this["portScanTimeoutSeconds"].ToString(), out timeout);
                return timeout;
            }
            set => this["portScanTimeoutSeconds"] = value;
        }

        #endregion

        #region Execute Before Connect section

        [ConfigurationProperty("executeBeforeConnect")]
        public bool ExecuteBeforeConnect
        {
            get => (bool)this["executeBeforeConnect"];
            set => this["executeBeforeConnect"] = value;
        }

        [ConfigurationProperty("executeBeforeConnectCommand")]
        public string ExecuteBeforeConnectCommand
        {
            get => (string)this["executeBeforeConnectCommand"];
            set => this["executeBeforeConnectCommand"] = value;
        }

        [ConfigurationProperty("executeBeforeConnectArgs")]
        public string ExecuteBeforeConnectArgs
        {
            get => (string)this["executeBeforeConnectArgs"];
            set => this["executeBeforeConnectArgs"] = value;
        }

        [ConfigurationProperty("executeBeforeConnectInitialDirectory")]
        public string ExecuteBeforeConnectInitialDirectory
        {
            get => (string)this["executeBeforeConnectInitialDirectory"];
            set => this["executeBeforeConnectInitialDirectory"] = value;
        }

        [ConfigurationProperty("executeBeforeConnectWaitForExit")]
        public bool ExecuteBeforeConnectWaitForExit
        {
            get => (bool)this["executeBeforeConnectWaitForExit"];
            set => this["executeBeforeConnectWaitForExit"] = value;
        }

        #endregion

        #region Security section

        [ConfigurationProperty("terminalsPassword", DefaultValue = "")]
        public string TerminalsPassword
        {
            get => Convert.ToString(this["terminalsPassword"]);
            set => this["terminalsPassword"] = value;
        }

        [ConfigurationProperty("defaultDomain", IsRequired = false)]
        public string DefaultDomain { get => (string)this["defaultDomain"]; set => this["defaultDomain"] = value; }

        [ConfigurationProperty("defaultUsername", IsRequired = false)]
        public string DefaultUsername
        {
            get => (string)this["defaultUsername"];
            set => this["defaultUsername"] = value;
        }

        [ConfigurationProperty("encryptedDefaultPassword", IsRequired = false)]
        public string EncryptedDefaultPassword
        {
            get => (string)this["encryptedDefaultPassword"];
            set => this["encryptedDefaultPassword"] = value;
        }

        [ConfigurationProperty("useAmazon")]
        public bool UseAmazon { get => (bool)this["useAmazon"]; set => this["useAmazon"] = value; }

        [ConfigurationProperty("encryptedAmazonAccessKey", IsRequired = false)]
        public string EncryptedAmazonAccessKey
        {
            get => (string)this["encryptedAmazonAccessKey"];
            set => this["encryptedAmazonAccessKey"] = value;
        }

        [ConfigurationProperty("encryptedAmazonSecretKey", IsRequired = false)]
        public string EncryptedAmazonSecretKey
        {
            get => (string)this["encryptedAmazonSecretKey"];
            set => this["encryptedAmazonSecretKey"] = value;
        }

        [ConfigurationProperty("AmazonBucketName", IsRequired = false)]
        public string AmazonBucketName
        {
            get => (string)this["AmazonBucketName"];
            set => this["AmazonBucketName"] = value;
        }

        #endregion

        #region Proxy section

        [ConfigurationProperty("useProxy")]
        public bool UseProxy { get => (bool)this["useProxy"]; set => this["useProxy"] = value; }

        [ConfigurationProperty("proxyAddress")]
        public string ProxyAddress { get => (string)this["proxyAddress"]; set => this["proxyAddress"] = value; }

        [ConfigurationProperty("proxyPort")]
        public int ProxyPort { get => (int)this["proxyPort"]; set => this["proxyPort"] = value; }

        #endregion

        #region Screen capture section

        [ConfigurationProperty("enableCaptureToClipboard")]
        public bool EnableCaptureToClipboard
        {
            get => (bool)this["enableCaptureToClipboard"];
            set => this["enableCaptureToClipboard"] = value;
        }

        [ConfigurationProperty("enableCaptureToFolder")]
        public bool EnableCaptureToFolder
        {
            get => (bool)this["enableCaptureToFolder"];
            set => this["enableCaptureToFolder"] = value;
        }

        [ConfigurationProperty("autoSwitchOnCapture", DefaultValue = true)]
        public bool AutoSwitchOnCapture
        {
            get => (bool)this["autoSwitchOnCapture"];
            set => this["autoSwitchOnCapture"] = value;
        }

        [ConfigurationProperty("captureRoot")]
        public string CaptureRoot { get => (string)this["captureRoot"]; set => this["captureRoot"] = value; }

        #endregion

        #region More section

        [ConfigurationProperty("restoreWindowOnLastTerminalDisconnect", DefaultValue = true)]
        public bool RestoreWindowOnLastTerminalDisconnect
        {
            get
            {
                if (this["restoreWindowOnLastTerminalDisconnect"] == null ||
                    this["restoreWindowOnLastTerminalDisconnect"].ToString() == string.Empty)
                    return true;
                var min = true;
                bool.TryParse(this["restoreWindowOnLastTerminalDisconnect"].ToString(), out min);
                return min;
            }
            set => this["restoreWindowOnLastTerminalDisconnect"] = value;
        }

        [ConfigurationProperty("enableFavoritesPanel", DefaultValue = true)]
        public bool EnableFavoritesPanel
        {
            get
            {
                if (this["enableFavoritesPanel"] == null ||
                    this["enableFavoritesPanel"].ToString() == string.Empty) return true;
                var min = true;
                bool.TryParse(this["enableFavoritesPanel"].ToString(), out min);
                return min;
            }
            set => this["enableFavoritesPanel"] = value;
        }

        [ConfigurationProperty("enableGroupsMenu", DefaultValue = true)]
        public bool EnableGroupsMenu
        {
            get
            {
                if (this["enableGroupsMenu"] == null || this["enableGroupsMenu"].ToString() == string.Empty)
                    return true;
                var min = true;
                bool.TryParse(this["enableGroupsMenu"].ToString(), out min);
                return min;
            }
            set => this["enableGroupsMenu"] = value;
        }

        [ConfigurationProperty("autoExapandTagsPanel", DefaultValue = false)]
        public bool AutoExapandTagsPanel
        {
            get => (bool)this["autoExapandTagsPanel"];
            set => this["autoExapandTagsPanel"] = value;
        }

        [ConfigurationProperty("defaultSortProperty", DefaultValue = "ConnectionName")]
        public string DefaultSortProperty
        {
            get
            {
                if (this["defaultSortProperty"] == null || this["defaultSortProperty"].ToString() == string.Empty)
                    return "ConnectionName";
                return this["defaultSortProperty"].ToString();
            }
            set => this["defaultSortProperty"] = value;
        }

        [ConfigurationProperty("office2007BlueFeel", DefaultValue = false)]
        public bool Office2007BlueFeel
        {
            get => (bool)this["office2007BlueFeel"];
            set => this["office2007BlueFeel"] = value;
        }

        [ConfigurationProperty("office2007BlackFeel", DefaultValue = false)]
        public bool Office2007BlackFeel
        {
            get => (bool)this["office2007BlackFeel"];
            set => this["office2007BlackFeel"] = value;
        }

        #endregion

        #region Vnc section

        [ConfigurationProperty("vncAutoScale", IsRequired = false)]
        public bool VncAutoScale { get => (bool)this["vncAutoScale"]; set => this["vncAutoScale"] = value; }

        [ConfigurationProperty("vncDisplayNumber", IsRequired = false)]
        public int VncDisplayNumber { get => (int)this["vncDisplayNumber"]; set => this["vncDisplayNumber"] = value; }

        [ConfigurationProperty("vncViewOnly", IsRequired = false)]
        public bool VncViewOnly { get => (bool)this["vncViewOnly"]; set => this["vncViewOnly"] = value; }

        #endregion

        #region Mainform controls section

        [ConfigurationProperty("favoritePanelWidth", DefaultValue = 300)]
        public int FavoritePanelWidth
        {
            get
            {
                if (this["favoritePanelWidth"] != null)
                    return (int)this["favoritePanelWidth"];
                return 300;
            }
            set => this["favoritePanelWidth"] = value;
        }

        [ConfigurationProperty("showFavoritePanel", DefaultValue = true)]
        public bool ShowFavoritePanel
        {
            get
            {
                if (this["showFavoritePanel"] != null)
                    return (bool)this["showFavoritePanel"];
                return true;
            }
            set => this["showFavoritePanel"] = value;
        }

        [ConfigurationProperty("toolbarsLocked", DefaultValue = true)]
        public bool ToolbarsLocked { get => (bool)this["toolbarsLocked"]; set => this["toolbarsLocked"] = value; }

        /// <summary>
        ///     Gets or set ordered collection of favorite names to show in GUI as toolstrip buttons
        /// </summary>
        [ConfigurationProperty("favoritesButtonsList")]
        [ConfigurationCollection(typeof(MRUItemConfigurationElementCollection))]
        public MRUItemConfigurationElementCollection FavoritesButtons
        {
            get => (MRUItemConfigurationElementCollection)this["favoritesButtonsList"];
            set => this["favoritesButtonsList"] = value;
        }

        #endregion

        #region Startup section

        [ConfigurationProperty("updateSource", IsRequired = false)]
        public string UpdateSource
        {
            get
            {
                if (this["updateSource"] == null || this["updateSource"] as string == "")
                    this["updateSource"] = @"http://tools.mscorlib.com/Terminals/TerminalsUpdates.xml";
                return (string)this["updateSource"];
            }
            set => this["updateSource"] = value;
        }

        [ConfigurationProperty("showWizard", DefaultValue = true)]
        public bool ShowWizard { get => (bool)this["showWizard"]; set => this["showWizard"] = value; }

        [ConfigurationProperty("psexecLocation")]
        public string PsexecLocation { get => (string)this["psexecLocation"]; set => this["psexecLocation"] = value; }

        #endregion

        #region Favorites & groups section

        [ConfigurationProperty("expandedFavoriteNodes", IsRequired = false)]
        public string ExpandedFavoriteNodes
        {
            get
            {
                if (this["expandedFavoriteNodes"] == null || this["expandedFavoriteNodes"] as string == "")
                    this["expandedFavoriteNodes"] = @"Untagged";
                return (string)this["expandedFavoriteNodes"];
            }
            set => this["expandedFavoriteNodes"] = value;
        }

        private const string EXPANDED_HISTORY_SEETINGS = "expandedHistoryNodes";

        [ConfigurationProperty(EXPANDED_HISTORY_SEETINGS, IsRequired = false)]
        public string ExpandedHistoryNodes
        {
            get => (string)this[EXPANDED_HISTORY_SEETINGS];
            set => this[EXPANDED_HISTORY_SEETINGS] = value;
        }

        [ConfigurationProperty("favorites")]
        [ConfigurationCollection(typeof(FavoriteConfigurationElementCollection))]
        public FavoriteConfigurationElementCollection Favorites
        {
            get => (FavoriteConfigurationElementCollection)this["favorites"];
            set => this["favorites"] = value;
        }

        [ConfigurationProperty("defaultFavorite")]
        [ConfigurationCollection(typeof(FavoriteConfigurationElementCollection))]
        public FavoriteConfigurationElementCollection DefaultFavorite
        {
            get => (FavoriteConfigurationElementCollection)this["defaultFavorite"];
            set => this["defaultFavorite"] = value;
        }

        [ConfigurationProperty("groups")]
        [ConfigurationCollection(typeof(GroupConfigurationElementCollection))]
        public GroupConfigurationElementCollection Groups
        {
            get => (GroupConfigurationElementCollection)this["groups"];
            set => this["groups"] = value;
        }

        [ConfigurationProperty("tags")]
        [ConfigurationCollection(typeof(MRUItemConfigurationElementCollection))]
        public MRUItemConfigurationElementCollection Tags
        {
            get => (MRUItemConfigurationElementCollection)this["tags"];
            set => this["tags"] = value;
        }

        #endregion

        #region MRU section

        [ConfigurationProperty("serversMRUList")]
        [ConfigurationCollection(typeof(MRUItemConfigurationElementCollection))]
        public MRUItemConfigurationElementCollection ServersMRU
        {
            get => (MRUItemConfigurationElementCollection)this["serversMRUList"];
            set => this["serversMRUList"] = value;
        }

        [ConfigurationProperty("domainsMRUList")]
        [ConfigurationCollection(typeof(MRUItemConfigurationElementCollection))]
        public MRUItemConfigurationElementCollection DomainsMRU
        {
            get => (MRUItemConfigurationElementCollection)this["domainsMRUList"];
            set => this["domainsMRUList"] = value;
        }

        [ConfigurationProperty("usersMRUList")]
        [ConfigurationCollection(typeof(MRUItemConfigurationElementCollection))]
        public MRUItemConfigurationElementCollection UsersMRU
        {
            get => (MRUItemConfigurationElementCollection)this["usersMRUList"];
            set => this["usersMRUList"] = value;
        }

        [ConfigurationProperty("searchesMRU")]
        [ConfigurationCollection(typeof(MRUItemConfigurationElementCollection))]
        public MRUItemConfigurationElementCollection SearchesMRU
        {
            get => (MRUItemConfigurationElementCollection)this["searchesMRU"];
            set => this["searchesMRU"] = value;
        }

        #endregion

        #region Database persistence

        [ConfigurationProperty("persistenceType", DefaultValue = (byte)0)]
        public byte PersistenceType { get => (byte)this["persistenceType"]; set => this["persistenceType"] = value; }

        [ConfigurationProperty("encryptedConnectionString", DefaultValue = "")]
        public string EncryptedConnectionString
        {
            get => (string)this["encryptedConnectionString"];
            set => this["encryptedConnectionString"] = value;
        }

        [ConfigurationProperty("databaseMasterPassword", DefaultValue = "")]
        public string DatabaseMasterPasswordHash
        {
            get => (string)this["databaseMasterPassword"];
            set => this["databaseMasterPassword"] = value;
        }

        #endregion
    }
}