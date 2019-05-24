using System;
using System.Drawing;
using System.Windows.Forms;
using Terminals.Common.Connections;
using Terminals.Data;

namespace Terminals.Connections
{
    /// <summary>
    ///     Implementation of default values in case required plugin is not available.
    /// </summary>
    internal class DummyPlugin : IConnectionPlugin, IOptionsConverterFactory
    {
        public int Port => 0;

        public string PortName => KnownConnectionConstants.RDP;

        public Image GetIcon()
        {
            return Connection.Terminalsicon;
        }

        public Connection CreateConnection()
        {
            return new Connection();
        }

        public Control[] CreateOptionsControls()
        {
            return new Control[0];
        }

        public Type GetOptionsType()
        {
            return typeof(EmptyOptions);
        }

        public ProtocolOptions CreateOptions()
        {
            return new EmptyOptions();
        }

        public IOptionsConverter CreatOptionsConverter()
        {
            return new EmptyOptionsConverter();
        }
    }
}