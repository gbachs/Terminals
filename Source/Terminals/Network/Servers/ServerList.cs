using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NetworkManagement;

namespace Terminals.Network.Servers
{
    internal partial class ServerList : UserControl
    {
        public ServerList()
        {
            this.InitializeComponent();
        }

        private void ServerList_Load(object sender, EventArgs e)
        {
            this.dataGridView1.DataSource = null;
            Application.DoEvents();
            var list = new List<KnownServers>();
            var servers = new NetworkManagement.Servers(ServerType.All);
            foreach (string name in servers)
            {
                var type = NetworkManagement.Servers.GetServerType(name);
                var s = new KnownServers();
                s.Name = name;
                s.Type = type;
                list.Add(s);
            }

            this.dataGridView1.DataSource = list;
        }
    }

    internal class KnownServers
    {
        public string Name { get; set; }

        public ServerType Type { get; set; }
    }
}