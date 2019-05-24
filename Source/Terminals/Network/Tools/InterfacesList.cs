using System;
using System.Windows.Forms;
using Metro;
using Terminals.Network.DNS;

namespace Terminals.Network
{
    internal partial class InterfacesList : UserControl
    {
        public InterfacesList()
        {
            this.InitializeComponent();
        }

        private void InterfacesList_Load(object sender, EventArgs e)
        {
            this.dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            var nil = new NetworkInterfaceList();
            this.dataGridView1.DataSource = null;
            this.dataGridView1.DataSource = nil.Interfaces;
            this.dataGridView2.DataSource = null;
            this.dataGridView2.DataSource = AdapterInfo.GetAdapters();
        }
    }
}