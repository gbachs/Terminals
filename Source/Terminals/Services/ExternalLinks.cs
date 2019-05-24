using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Terminals.Configuration;
using Terminals.Connections;
using Terminals.Data;
using Terminals.Forms;
using Terminals.Updates;

namespace Terminals.Services
{
    internal class ExternalLinks : Common.Forms.ExternalLinks
    {
        internal static string TerminalsReleasesUrl => Program.Resources.GetString("TerminalsURL");

        internal static void ShowWinPCapPage()
        {
            const string MESSAGE =
                "It appears that WinPcap is not installed.  In order to use this feature within Terminals you must first install that product.  Do you wish to visit the download location right now?";
            if (MessageBox.Show(MESSAGE, "Download WinPcap?", MessageBoxButtons.OKCancel) == DialogResult.OK)
                OpenPath("http://www.winpcap.org/install/default.htm");
        }

        internal static void AskIfShowReleasePage(Settings settings, ReleaseInfo releaseInfo)
        {
            var message =
                $"Version:{releaseInfo.Version}\r\nPublished:{releaseInfo.Published}\r\nDo you want to show the Terminals home page?";
            var answer = YesNoDisableForm.ShowDialog("New release is available", message);
            if (answer.Result == DialogResult.Yes)
                ShowReleasePage();

            if (answer.Disable)
                settings.NeverShowTerminalsWindow = true;
        }

        internal static void ShowReleasePage()
        {
            OpenPath(TerminalsReleasesUrl);
        }

        internal static void OpenAuthorPage()
        {
            OpenPath("http://weblogs.asp.net/rchartier/");
        }

        internal static void OpenLogsFolder()
        {
            OpenPath(FileLocations.LogDirectory);
        }

        internal static void CallExecuteBeforeConnected(IBeforeConnectExecuteOptions executeOptions)
        {
            if (executeOptions.Execute && !string.IsNullOrEmpty(executeOptions.Command))
            {
                var processStartInfo = new ProcessStartInfo(executeOptions.Command, executeOptions.CommandArguments);
                processStartInfo.WorkingDirectory = executeOptions.InitialDirectory;
                var process = Process.Start(processStartInfo);
                if (executeOptions.WaitForExit)
                    process.WaitForExit();
            }
        }

        internal static void Launch(SpecialCommandConfigurationElement command)
        {
            try
            {
                TryLaunch(command);
            }
            catch (Exception ex)
            {
                var message = $"Could not Launch the shortcut application: '{command.Name}'";
                MessageBox.Show(message);
                Logging.Error(message, ex);
            }
        }

        private static void TryLaunch(SpecialCommandConfigurationElement command)
        {
            var exe = command.Executable;
            if (exe.Contains("%"))
                exe = exe.Replace("%systemroot%", Environment.GetEnvironmentVariable("systemroot"));

            var startInfo = new ProcessStartInfo(exe, command.Arguments);
            startInfo.WorkingDirectory = command.WorkingFolder;
            Process.Start(startInfo);
        }

        internal static void StartMsManagementConsole(IFavorite favorite)
        {
            if (favorite != null)
                Process.Start("mmc.exe", "compmgmt.msc /a /computer=" + favorite.ServerName);
        }

        internal static void OpenLocalComputerManagement()
        {
            Process.Start("mmc.exe", "compmgmt.msc /a /computer=.");
        }

        internal static void StartMsInfo32(IFavorite favorite)
        {
            if (favorite != null)
            {
                var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                //if(programFiles.Contains("(x86)")) programFiles = programFiles.Replace(" (x86)","");
                var path = $@"{programFiles}\common files\Microsoft Shared\MSInfo\msinfo32.exe";
                if (File.Exists(path))
                    Process.Start($"\"{path}\"", $"/computer {favorite.ServerName}");
            }
        }

        internal static void OpenFileInNotepad(string file)
        {
            if (MessageBox.Show("Open dump file in notepad?") == DialogResult.OK)
                Process.Start("notepad.exe", file);
        }

        internal static void OpenTerminalServiceCommandPrompt(IConnectionExtra terminal, string psexecLocation)
        {
            var sessionId = string.Empty;
            if (!terminal.ConnectToConsole)
                sessionId = TSManager.GetCurrentSession(terminal.Server,
                    terminal.UserName,
                    terminal.Domain,
                    Environment.MachineName).Id.ToString();

            var process = new Process();
            var args = $" \\\\{terminal.Server} -i {sessionId} -d cmd";
            var startInfo = new ProcessStartInfo(psexecLocation, args);
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}