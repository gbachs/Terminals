using System;
using System.Resources;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using Terminals.CommandLine;
using Terminals.Configuration;
using Terminals.Connections;
using Terminals.Data;
using Terminals.Forms;
using Terminals.Native;
using Terminals.Updates;

namespace Terminals
{
    internal static partial class Program
    {
        public static ResourceManager Resources = new ResourceManager("Terminals.Localization.LocalizedValues",
            typeof(MainForm).Assembly);

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        [ComVisible(true)]
        internal static void Main()
        {
            SetUnhandledExceptions();
            Info.SetApplicationVersion();

            Logging.Info(
                $"-------------------------------Title: {Info.TitleVersion} started Version:{Info.DLLVersion} Date:{Info.BuildDate}-------------------------------");
            Logging.Info("Start state 1 Complete: Unhandled exceptions");

            LogGeneralProperties();
            Logging.Info("Start state 2 Complete: Log General properties");

            SetApplicationProperties();
            Logging.Info("Start state 3 Complete: Set application properties");

            var settings = Settings.Instance;
            var commandLine = ParseCommandline(settings);
            Logging.Info("Start state 4 Complete: Parse command line");

            if (!EnsureDataAreWriteAble())
                return;
            Logging.Info("Start state 5 Complete: User account control");

            if (commandLine.SingleInstance && SingleInstanceApplication.Instance.NotifyExisting(commandLine))
                return;

            Logging.Info("Start state 6 Complete: Set Single instance mode");

            var connectionManager = new ConnectionManager(new PluginsLoader(settings));
            var favoriteIcons = new FavoriteIcons(connectionManager);
            var persistenceFactory = new PersistenceFactory(settings, connectionManager, favoriteIcons);
            // do it before config update, because it may import favorites from previous version
            var persistence = persistenceFactory.CreatePersistence();
            Logging.Info("Start state 7 Complete: Initilizing Persistence");

            TryUpdateConfig(settings, persistence, connectionManager);
            Logging.Info("Start state 8 Complete: Configuration upgrade");

            ShowFirstRunWizard(settings, persistence, connectionManager);
            var startupUi = new StartupUi();
            persistence = persistenceFactory.AuthenticateByMasterPassword(persistence, startupUi);
            PersistenceErrorForm.RegisterDataEventHandler(persistence.Dispatcher);

            RunMainForm(persistence, connectionManager, favoriteIcons, commandLine);

            Logging.Info($"-------------------------------{Info.TitleVersion} Stopped-------------------------------");
        }

        private static void TryUpdateConfig(Settings settings, IPersistence persistence,
            ConnectionManager connectionManager)
        {
            var updateConfig = new UpdateConfig(settings, persistence, connectionManager);
            updateConfig.CheckConfigVersionUpdate();
        }

        private static void SetUnhandledExceptions()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            Application.ThreadException += ApplicationThreadException;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowApplicationExit(e.ExceptionObject);
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private static void ApplicationThreadException(object sender, ThreadExceptionEventArgs e)
        {
            ShowApplicationExit(e.Exception);
        }

        private static void ShowApplicationExit(object messageToLog)
        {
            Logging.Fatal(messageToLog);
            Logging.Fatal("Application has to be terminated.");
            UnhandledTerminationForm.ShowRipDialog();
            Environment.Exit(-1);
        }

        private static bool EnsureDataAreWriteAble()
        {
            var hasDataAccess = FileLocations.UserHasAccessToDataDirectory();
            if (!hasDataAccess)
            {
                var message = $"Write Access is denied to:\r\n{FileLocations.WriteAccessLock}\r\n" +
                              "Please make sure you have write permissions to the data directory";
                MessageBox.Show(message, "Terminals", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return hasDataAccess;
        }

        private static void ShowFirstRunWizard(Settings settings, IPersistence persistence,
            ConnectionManager connectionManager)
        {
            if (settings.ShowWizard)
                //settings file doesn't exist
                using (var wzrd = new FirstRunWizard(persistence, connectionManager))
                {
                    wzrd.ShowDialog();
                }
        }

        private static void RunMainForm(IPersistence persistence, ConnectionManager connectionManager,
            FavoriteIcons favoriteIcons, CommandLineArgs commandLine)
        {
            var mainForm = new MainForm(persistence, connectionManager, favoriteIcons);
            SingleInstanceApplication.Instance.Initialize(mainForm, commandLine);
            mainForm.HandleCommandLineActions(commandLine);
            Application.Run(mainForm);
        }

        /// <summary>
        ///     dump out common/useful debugging data at app start
        /// </summary>
        private static void LogGeneralProperties()
        {
            Logging.Info($"CommandLine:{Environment.CommandLine}");
            Logging.Info($"CurrentDirectory:{Environment.CurrentDirectory}");
            Logging.Info($"MachineName:{Environment.MachineName}");
            Logging.Info($"OSVersion:{Environment.OSVersion}");
            Logging.Info($"ProcessorCount:{Environment.ProcessorCount}");
            Logging.Info($"UserInteractive:{Environment.UserInteractive}");
            Logging.Info($"Version:{Environment.Version}");
            Logging.Info($"WorkingSet:{Environment.WorkingSet}");
            Logging.Info($"Is64BitOperatingSystem:{Wow.Is64BitOperatingSystem}");
            Logging.Info($"Is64BitProcess:{Wow.Is64BitProcess}");
        }

        private static void SetApplicationProperties()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }

        private static CommandLineArgs ParseCommandline(Settings settings)
        {
            var commandline = new CommandLineArgs();
            var cmdLineArgs = Environment.GetCommandLineArgs();
            Parser.ParseArguments(cmdLineArgs, commandline);
            settings.FileLocations.AssignCustomFileLocations(commandline.configFile,
                commandline.favoritesFile, commandline.credentialsFile);
            return commandline;
        }
    }
}