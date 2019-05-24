using System;
using System.IO;

namespace Terminals.Configuration
{
    /// <summary>
    ///     Data file locations resolution under Data subdirectory
    /// </summary>
    internal sealed class FileLocations
    {
        private const string PROFILE_PATH = @"Robert_Chartier\Terminals\";

        /// <summary>
        ///     Gets the directory name of data directory,
        ///     where all files changed by user should be stored
        /// </summary>
        private const string DATA_DIRECTORY = "Data";

        /// <summary>
        ///     Gets directory name of the commands thumb images location ("Thumbs").
        /// </summary>
        internal const string THUMBS_DIRECTORY = "Thumbs";

        /// <summary>
        ///     Gets default name of the credentials file ("Credentials.xml").
        /// </summary>
        internal const string CREDENTIALS_FILENAME = "Credentials.xml";

        /// <summary>
        ///     Gets default name of the favorites file ("Favorites.xml").
        /// </summary>
        internal const string FAVORITES_FILENAME = "Favorites.xml";

        /// <summary>
        ///     Gets the file name of stored history values ("History.xml").
        /// </summary>
        internal const string HISTORY_FILENAME = "History.xml";

        /// <summary>
        ///     Gets the name of custom user options configuration file ("Terminals.config").
        /// </summary>
        internal const string CONFIG_FILENAME = "Terminals.config";

        /// <summary>
        ///     Gets the file name of xml config file, where toolbar positions are stored ("ToolStrip.settings.config").
        /// </summary>
        internal const string TOOLSTRIPS_FILENAME = "ToolStrip.settings.config";

        private const string SQL_MIGRATIONS = "Migrations";

        private static readonly string PROFILE_DATA_DIRECTORY =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        private readonly Settings settings;

        private string configuration;

        internal FileLocations(Settings settings)
        {
            this.settings = settings;
        }

        internal static string ControlPanelImage => Path.Combine(ThumbsDirectoryFullPath, @"ControlPanel.png");

        internal static string LastUpdateCheck => GetFullPath("LastUpdateCheck.txt");

        internal static string WriteAccessLock => GetFullPath("WriteAccessCheck.txt");

        internal static string LogDirectory => Logging.LogDirectory;

        internal static string ThumbsDirectoryFullPath => GetFullPath(THUMBS_DIRECTORY);

        internal static string ToolStripsFullFileName => GetFullPath(TOOLSTRIPS_FILENAME);

        internal static string HistoryFullFileName => GetFullPath(HISTORY_FILENAME);

        internal static string DefaultCaptureRootDirectory
        {
            get
            {
                var rootDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                return Path.Combine(rootDirectory, "Terminals Captures");
            }
        }

        /// <summary>
        ///     Gets full path to the Sql migration scripts directory in application install directory
        /// </summary>
        internal static string SqlMigrations => Path.Combine(Program.Info.Location, SQL_MIGRATIONS);

        internal string Configuration
        {
            get => this.configuration;
            private set
            {
                this.configuration = value;
                this.FireConfigPathChanged(this.configuration);
            }
        }

        internal string Favorites { get; private set; }

        internal string Credentials { get; private set; }

        internal event EventHandler<FileChangedEventArgs> ConfigFileChanged;

        /// <summary>
        ///     Sets custom file locations for general data files.
        ///     All paths have to be set to absolute file path,  otherwise are ignored.
        ///     You have to call this method only once at startup before files are loaded,
        ///     otherwise their usage isn't consistent.
        /// </summary>
        internal void AssignCustomFileLocations(string configurationFullPath,
            string favoritesFullPath, string credentialsFullPath)
        {
            // we don't have to assign to file file watchers, they aren't initialized yet
            // we don't have to check if files exist, we recreate them
            this.AssignConfigurationFile(configurationFullPath);
            this.AssignFavoritesFile(favoritesFullPath);
            this.AssignCredentialsFile(credentialsFullPath);
        }

        private void AssignConfigurationFile(string configurationFullPath)
        {
            if (string.IsNullOrEmpty(configurationFullPath))
                this.Configuration = GetFullPath(CONFIG_FILENAME);
            else
                this.Configuration = configurationFullPath;
        }

        private void AssignFavoritesFile(string favoritesFullPath)
        {
            if (string.IsNullOrEmpty(favoritesFullPath))
            {
                this.Favorites = this.settings.SavedFavoritesFileLocation;
                if (string.IsNullOrEmpty(this.Favorites))
                    this.Favorites = GetFullPath(FAVORITES_FILENAME);
            }
            else
            {
                this.Favorites = favoritesFullPath;
            }
        }

        private void AssignCredentialsFile(string credentialsFullPath)
        {
            if (string.IsNullOrEmpty(credentialsFullPath))
                this.AssignDefaultCredentialsFile();
            else
                this.Credentials = credentialsFullPath;
        }

        private void AssignDefaultCredentialsFile()
        {
            this.Credentials = this.settings.SavedCredentialsLocation;
            if (string.IsNullOrEmpty(this.Credentials) || this.Credentials == CREDENTIALS_FILENAME)
                this.Credentials = GetFullPath(CREDENTIALS_FILENAME);
        }

        /// <summary>
        ///     Gets the full file path to the required file or directory in application data directory.
        /// </summary>
        /// <param name="relativePath">The relative path to the file from data directory.</param>
        internal static string GetFullPath(string relativePath)
        {
            var root = GetDataRootDirectoryFullPath();
            return Path.Combine(root, relativePath);
        }

        private static string GetDataRootDirectoryFullPath()
        {
            var root = Path.Combine(Program.Info.Location, DATA_DIRECTORY);
            var localInstallation = !Properties.Settings.Default.Portable;
            if (localInstallation)
                root = GetProfileDataDirectoryPath();

            EnsureDataDirectory(root);
            return root;
        }

        private static string GetProfileDataDirectoryPath()
        {
            const string relativeDataPath = PROFILE_PATH + DATA_DIRECTORY;
            return Path.Combine(PROFILE_DATA_DIRECTORY, relativeDataPath);
        }

        private static void EnsureDataDirectory(string root)
        {
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
        }

        internal static string FormatThumbFileName(string fileName)
        {
            return string.Format(@"{0}\{1}.jpg", ThumbsDirectoryFullPath, fileName);
        }

        internal static void EnsureImagesDirectory()
        {
            EnsureDataDirectory(ThumbsDirectoryFullPath);
        }

        internal static bool UserHasAccessToDataDirectory()
        {
            try
            {
                // Test to make sure that the current user has write access to the current directory.
                using (var sw = File.AppendText(WriteAccessLock))
                {
                }

                return true;
            }
            catch (Exception ex)
            {
                Logging.FatalFormat("Access Denied {0}", ex.Message);
                return false;
            }
        }

        private void FireConfigPathChanged(string newPath)
        {
            if (this.ConfigFileChanged != null)
            {
                var args = new FileChangedEventArgs {NewPath = newPath};
                this.ConfigFileChanged(this, args);
            }
        }
    }
}