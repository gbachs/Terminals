using System.ServiceModel;
using Terminals.CommandLine;

namespace Terminals.Network
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    internal class CommandLineService : ICommandLineService
    {
        private readonly MainForm mainForm;

        internal CommandLineService(MainForm mainForm)
        {
            this.mainForm = mainForm;
        }

        public void ForwardCommand(CommandLineArgs args)
        {
            this.mainForm.HandleCommandLineActions(args);
            this.mainForm.BringToFront();
            this.mainForm.Focus();
        }
    }
}