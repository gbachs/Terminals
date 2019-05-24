using System;

namespace Terminals.Data
{
    [Serializable]
    public class BeforeConnectExecuteOptions : IBeforeConnectExecuteOptions
    {
        public bool Execute { get; set; }

        public string Command { get; set; }

        public string CommandArguments { get; set; }

        public string InitialDirectory { get; set; }

        public bool WaitForExit { get; set; }

        internal BeforeConnectExecuteOptions Copy()
        {
            return new BeforeConnectExecuteOptions
            {
                Execute = this.Execute,
                Command = this.Command,
                CommandArguments = this.CommandArguments,
                InitialDirectory = this.InitialDirectory,
                WaitForExit = this.WaitForExit
            };
        }
    }
}