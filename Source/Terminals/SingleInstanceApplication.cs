using System.Threading;
using Terminals.CommandLine;
using Terminals.Network;

namespace Terminals
{
    /// <summary>
    ///     Allowes to run only one instance of this application per user.
    ///     This is necessary to allow run multiple times on Terminal server
    /// </summary>
    internal class SingleInstanceApplication
    {
        private const string INSTANCELOCK_NAME = "Terminals.codeplex.com.SingleInstance";

        private const string STARTUPLOCK_NAME = "Terminals.codeplex.com.CommandServerStartUp";

        private readonly bool firstInstance;

        /// <summary>
        ///     This is a machine wide application instances counter.
        ///     when the last application is shutdown the mutex will be released automatically
        /// </summary>
        private readonly Mutex instanceLock;

        private CommandLineServer server;

        /// <summary>
        ///     Prevent asking for window notifications, when the server is in startup or shutdown procedure
        /// </summary>
        private readonly Mutex startupLock;

        /// <summary>
        ///     generates mutexee per user names with global prefix to control also sessions on Terminal server
        /// </summary>
        private static string GetMutexName(string generalName)
        {
            return $"Global\\{generalName}.{WindowsUserIdentifiers.GetCurrentUserSid()}";
        }

        internal void Initialize(MainForm mainForm, CommandLineArgs commandLine)
        {
            if (!this.firstInstance)
                return;

            this.StartServer(mainForm, commandLine);
            // startupLock obtained in constructor, the server is now available to notifications
            this.startupLock.ReleaseMutex();
        }

        private void StartServer(MainForm mainForm, CommandLineArgs commandLine)
        {
            if (!commandLine.SingleInstance)
                return;

            this.server = new CommandLineServer(mainForm);
            this.server.Open();
        }

        /// <summary>
        ///     close the server as soon as, when closing the main form,
        ///     because othewise the form can be already dead and it cant process notification requests
        /// </summary>
        internal void Close()
        {
            if (!this.firstInstance)
                return;
            try
            {
                this.startupLock.WaitOne();
                this.server?.Close();
            }
            finally
            {
                this.startupLock.Close();
                this.instanceLock.Close();
            }
        }

        /// <summary>
        ///     If other instance is runing, then forwards the command line to it and returns true;
        ///     otherwise returns false.
        /// </summary>
        internal bool NotifyExisting(CommandLineArgs args)
        {
            if (this.firstInstance)
                return false;

            return this.ForwardCommand(args);
        }

        private bool ForwardCommand(CommandLineArgs args)
        {
            try
            {
                // wait until the main instance startup/shutdown ends
                this.startupLock.WaitOne();
                var client = CommandLineServer.CreateClient();
                client.ForwardCommand(args);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                this.startupLock.ReleaseMutex();
            }
        }

        #region Thread safe singleton

        private static class Nested
        {
            internal static readonly SingleInstanceApplication instance = new SingleInstanceApplication();
        }

        internal static SingleInstanceApplication Instance => Nested.instance;

        private SingleInstanceApplication()
        {
            var instanceLockName = GetMutexName(INSTANCELOCK_NAME);
            this.instanceLock = new Mutex(true, instanceLockName, out this.firstInstance);
            // we are sure, that we own the previous one, so lock the applicatin startup immediately
            var startupLockName = GetMutexName(STARTUPLOCK_NAME);
            this.startupLock = new Mutex(this.firstInstance, startupLockName);
        }

        #endregion
    }
}