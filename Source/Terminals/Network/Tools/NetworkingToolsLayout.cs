using System;
using System.Windows.Forms;
using Terminals.Data;

namespace Terminals.Connections
{
    internal partial class NetworkingToolsLayout : UserControl
    {
        public delegate void TabChanged(object sender, TabControlEventArgs e);

        public NetworkingToolsLayout()
        {
            this.InitializeComponent();
        }

        public event TabChanged OnTabChanged;

        private void TabbedTools1_Load(object sender, EventArgs e)
        {
            this.tabbedTools1.OnTabChanged += this.tabbedTools1_OnTabChanged;
        }

        public void Execute(NettworkingTools action, string host, IPersistence persistence)
        {
            this.tabbedTools1.Execute(action, host, persistence);
        }

        private void tabbedTools1_OnTabChanged(object sender, TabControlEventArgs e)
        {
            this.OnTabChanged?.Invoke(sender, e);
        }
    }
}