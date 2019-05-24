using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Terminals.Data;
using Terminals.Properties;
using SysConfig = System.Configuration;

namespace Terminals.Configuration
{
    internal delegate void ConfigurationChangedHandler(ConfigurationChangedEventArgs args);

    internal partial class Settings
    {
        /// <summary>
        ///     Prevent concurent updates on config file by another program
        /// </summary>
        private static readonly Mutex fileLock = new Mutex(false, "Terminals.CodePlex.com.Settings");

        private SysConfig.Configuration _config;

        /// <summary>
        ///     Flag informing, that configuration shouldnt be saved imediately, but after explicit call
        ///     This increases performance for
        /// </summary>
        private bool delayConfigurationSave;

        private IDataFileWatcher fileWatcher;

        private readonly Func<string, IDataFileWatcher> initializeFileWatcher;

        internal Settings()
            : this(p => new DataFileWatcher(p))
        {
        }

        internal Settings(IDataFileWatcher fileWatcher)
            : this(p => fileWatcher)
        {
        }

        private Settings(Func<string, IDataFileWatcher> initializeFileWatcher)
        {
            this.FileLocations = new FileLocations(this);
            this.initializeFileWatcher = initializeFileWatcher;
            this.FileLocations.ConfigFileChanged += this.ConfigFilePathChanged;
        }

        internal FileLocations FileLocations { get; }

        private SysConfig.Configuration Config
        {
            get
            {
                if (this._config == null)
                    this._config = this.GetConfiguration();

                return this._config;
            }
        }

        /// <summary>
        ///     Informs lisseners, that configuration file was changed by another application
        ///     or another Terminals instance. In this case all cached not saved data are lost.
        /// </summary>
        internal event ConfigurationChangedHandler ConfigurationChanged;

        /// <summary>
        ///     Real filewatch works, only, if the path is correctly configured.
        ///     Changing the path after the watch is created, doesnt raise any event.
        /// </summary>
        private void ConfigFilePathChanged(object sender, FileChangedEventArgs e)
        {
            this.fileWatcher = this.initializeFileWatcher(e.NewPath);
            this.fileWatcher.FileChanged += this.ConfigFileChanged;
        }

        private void ConfigFileChanged(object sender, EventArgs e)
        {
            var old = this.GetSection();
            this.ForceReload();
            var args = ConfigurationChangedEventArgs.CreateFromSettings(old, this.GetSection());
            this.FireConfigurationChanged(args);
        }

        private void FireConfigurationChanged(ConfigurationChangedEventArgs args)
        {
            if (this.ConfigurationChanged != null)
                this.ConfigurationChanged(args);
        }

        /// <summary>
        ///     Because filewatcher is created before the main form in GUI thread.
        ///     This lets to fire the file system watcher events in GUI thread.
        /// </summary>
        internal void AssignSynchronizationObject(ISynchronizeInvoke synchronizer)
        {
            this.fileWatcher.AssignSynchronizer(synchronizer);
        }

        internal void ForceReload()
        {
            this._config = this.GetConfiguration();
        }

        /// <summary>
        ///     Prevents save configuration after each change. After this call, no settings are saved
        ///     into config file, until you call SaveAndFinishDelayedUpdate.
        ///     This dramatically increases performance. Use this method for batch updates.
        /// </summary>
        internal void StartDelayedUpdate()
        {
            this.delayConfigurationSave = true;
        }

        /// <summary>
        ///     Stops prevent write changes into config file and immediately writes last state.
        ///     Usually the changes are saved immediately
        /// </summary>
        internal void SaveAndFinishDelayedUpdate()
        {
            this.delayConfigurationSave = false;
            this.SaveImmediatelyIfRequested();
        }

        private void SaveImmediatelyIfRequested()
        {
            if (!this.delayConfigurationSave)
                try
                {
                    fileLock.WaitOne(); // lock the file for changes by other application instance
                    this.Save();
                }
                catch (Exception exception)
                {
                    Logging.Error("Config file access failed by save.", exception);
                }
                finally
                {
                    fileLock.ReleaseMutex();
                }
        }

        private void Save()
        {
            this.fileWatcher.StopObservation();
            this.Config.Save();
            this.fileWatcher.StartObservation();
            Debug.WriteLine("Terminals.config file saved.");
        }

        private SysConfig.Configuration GetConfiguration()
        {
            try
            {
                this.CreateConfigFileIfNotExist();
                return this.OpenConfiguration();
            }
            catch (Exception exc) // try to recover the file
            {
                Logging.Error("Get Configuration", exc);
                this.BackUpConfigFile();
                this.SaveDefaultConfigFile();
                return this.OpenConfiguration();
            }
        }

        private void CreateConfigFileIfNotExist()
        {
            if (!File.Exists(this.FileLocations.Configuration))
                this.SaveDefaultConfigFile();
        }

        private SysConfig.ExeConfigurationFileMap CreateConfigFileMap()
        {
            var configFileMap = new SysConfig.ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = this.FileLocations.Configuration;
            return configFileMap;
        }

        private SysConfig.Configuration OpenConfiguration()
        {
            var configFileMap = this.CreateConfigFileMap();
            fileLock.WaitOne();
            var config =
                SysConfig.ConfigurationManager.OpenMappedExeConfiguration(configFileMap,
                    SysConfig.ConfigurationUserLevel.None);
            fileLock.ReleaseMutex();
            return config;
        }

        private void BackUpConfigFile()
        {
            if (File.Exists(this.FileLocations.Configuration))
            {
                var backupFileName = GetBackupFileName();
                // back it up before we do anything
                File.Copy(this.FileLocations.Configuration, backupFileName);
                // now delete it
                File.Delete(this.FileLocations.Configuration);
            }
        }

        private static string GetBackupFileName()
        {
            var newGUID = Guid.NewGuid().ToString();
            var fileDate = DateTime.Now.ToFileTime();
            var backupFile = string.Format("Terminals-{1}-{0}.config", newGUID, fileDate);
            return FileLocations.GetFullPath(backupFile);
        }

        internal void SaveDefaultConfigFile()
        {
            var templateConfigFile = Resources.Terminals;
            File.WriteAllText(this.FileLocations.Configuration, templateConfigFile);
        }

        private static void MoveAndDeleteFile(string fileName, string tempFileName)
        {
            // delete the zerobyte file which is created by default
            if (File.Exists(tempFileName))
                File.Delete(tempFileName);

            // move the error file to the temp file
            File.Move(fileName, tempFileName);

            // if its still hanging around, kill it
            if (File.Exists(fileName))
                File.Delete(fileName);
        }

        private SysConfig.Configuration ImportConfiguration()
        {
            // get a temp filename to hold the current settings which are failing
            var tempFile = Path.GetTempFileName();

            this.fileWatcher.StopObservation();
            MoveAndDeleteFile(this.FileLocations.Configuration, tempFile);
            this.SaveDefaultConfigFile();
            this.fileWatcher.StartObservation();
            var c = this.OpenConfiguration();

            // get a list of the properties on the Settings object (static props)
            var propList = typeof(Settings).GetProperties();

            // read all the xml from the erroring file
            var doc = new XmlDocument();
            doc.LoadXml(File.ReadAllText(tempFile));

            // get the settings root
            var root = doc.SelectSingleNode("/configuration/settings");
            try
            {
                // for each setting's attribute
                foreach (XmlAttribute att in root.Attributes)
                    // scan for the related property if any
                    try
                    {
                        foreach (var info in propList)
                            try
                            {
                                if (info.Name.ToLower() == att.Name.ToLower())
                                {
                                    // found a matching property, try to set it
                                    var val = att.Value;
                                    info.SetValue(null, Convert.ChangeType(val, info.PropertyType), null);
                                    break;
                                }
                            }
                            catch (Exception exc)
                            {
                                // ignore the error
                                Logging.Error("Remapping Settings Inner", exc);
                            }
                    }
                    catch (Exception exc) // ignore the error
                    {
                        Logging.Error("Remapping Settings Outer", exc);
                    }
            }
            catch (Exception exc) // ignore the error
            {
                Logging.Error("Remapping Settings Outer Try", exc);
            }

            var favs = doc.SelectNodes("/configuration/settings/favorites/add");
            try
            {
                foreach (XmlNode fav in favs)
                    try
                    {
                        var newFav = new FavoriteConfigurationElement();
                        foreach (XmlAttribute att in fav.Attributes)
                            try
                            {
                                foreach (var info in newFav.GetType().GetProperties())
                                    try
                                    {
                                        if (info.Name.ToLower() == att.Name.ToLower())
                                        {
                                            // found a matching property, try to set it
                                            var val = att.Value;
                                            if (info.PropertyType.IsEnum)
                                                info.SetValue(newFav, Enum.Parse(info.PropertyType, val), null);
                                            else
                                                info.SetValue(newFav, Convert.ChangeType(val, info.PropertyType), null);

                                            break;
                                        }
                                    }
                                    catch (Exception exc) // ignore the error
                                    {
                                        Logging.Error("Remapping Favorites 1", exc);
                                    }
                            }
                            catch (Exception exc) // ignore the error
                            {
                                Logging.Error("Remapping Favorites 2", exc);
                            }

                        this.AddFavorite(newFav);
                    }
                    catch (Exception exc) // ignore the error
                    {
                        Logging.Error("Remapping Favorites 3", exc);
                    }
            }
            catch (Exception exc) // ignore the error
            {
                Logging.Error("Remapping Favorites 4", exc);
            }

            return c;
        }

        private TerminalsConfigurationSection GetSection()
        {
            try
            {
                return this.Config.GetSection("settings") as TerminalsConfigurationSection;
            }
            catch (Exception exc)
            {
                if (exc.Message.Contains("telnet"))
                {
                    MessageBox.Show("You need to replace telnetrows, telnetcols, telnetfont, telnetbackcolor, "
                                    + "telnettextcolor, telnetcursorcolor with consolerows, consolecols, consolefont, consolebackcolor, "
                                    + "consoletextcolor, consolecursorcolor");
                    return null;
                }

                Logging.Error("Telnet Section Failed", exc);

                try
                {
                    // kick into the import routine
                    var configuration = this.ImportConfiguration();
                    configuration = this.GetConfiguration();
                    if (configuration == null)
                        MessageBox.Show("Terminals was able to automatically upgrade your existing connections.");
                    return configuration.GetSection("settings") as TerminalsConfigurationSection;
                }
                catch (Exception importException)
                {
                    Logging.Error("Trying to import connections failed", importException);
#if !DEBUG
                    string message =
 string.Format("Terminals was NOT able to automatically upgrade your existing connections.\r\nError:{0}",
                        importException.Message);
                    MessageBox.Show(message);
#endif
                    return new TerminalsConfigurationSection();
                }
            }
        }
    }
}