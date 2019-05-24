using System;
using System.Windows.Forms;
using Metro.TransportLayer.Tcp;

namespace Terminals.Network
{
    internal partial class LocalConnections : UserControl
    {
        public LocalConnections()
        {
            this.InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var connections = TcpConnectionManager.GetCurrentTcpConnections();
            //this.dataGridView1.DataSource = null;
            this.dataGridView1.DataSource = connections;
        }

        private void LocalConnections_Load(object sender, EventArgs e)
        {
            this.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            this.timer1_Tick(null, null);
            this.timer1.Enabled = true;
            this.timer1.Start();
        }
    }
}