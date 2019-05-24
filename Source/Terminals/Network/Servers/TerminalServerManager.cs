using System;
using System.Windows.Forms;
using Terminals.Common.Connections;
using Terminals.Data;
using Terminals.TerminalServices;

namespace Terminals.Network.Servers
{
    internal partial class TerminalServerManager : UserControl
    {
        private string hostName;

        private IPersistence persistence;

        private Session selectedSession;

        private TerminalServer server;

        public TerminalServerManager()
        {
            this.InitializeComponent();
        }

        public string HostName
        {
            get => this.hostName;
            set
            {
                this.hostName = value;
                this.ServerNameComboBox.Text = this.hostName;
            }
        }

        internal void AssignPersistence(IPersistence persistence)
        {
            this.persistence = persistence;
        }

        public void ForceTSAdmin(string host)
        {
            this.ServerNameComboBox.Text = host;
            this.ConnectButton_Click(null, null);
        }

        public void Connect(string server, bool headless)
        {
            try
            {
                this.splitContainer1.Panel1Collapsed = headless;
                if (server != string.Empty)
                {
                    this.ServerNameComboBox.Text = server;
                    this.ConnectButton_Click(null, null);
                }
            }
            catch (Exception exc)
            {
                Logging.Error("Connection Failure.", exc);
            }
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (this.ParentForm != null)
                this.ParentForm.Cursor = Cursors.WaitCursor;

            this.selectedSession = null;
            this.dataGridView1.DataSource = null;
            this.dataGridView2.DataSource = null;
            this.propertyGrid1.SelectedObject = null;
            Application.DoEvents();
            this.server = TerminalServer.LoadServer(this.ServerNameComboBox.Text);

            try
            {
                if (this.server.IsATerminalServer)
                {
                    this.dataGridView1.DataSource = this.server.Sessions;
                    this.dataGridView1.Columns[1].Visible = false;
                }
                else
                {
                    MessageBox.Show("This machine does not appear to be a Terminal Server");
                }
            }
            catch (Exception)
            {
                // Do nothing when error
            }

            if (this.ParentForm != null)
                this.ParentForm.Cursor = Cursors.Default;
        }

        private void DataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (this.server.IsATerminalServer)
                if (this.dataGridView1.DataSource != null)
                {
                    this.selectedSession = this.server.Sessions[e.RowIndex];
                    this.propertyGrid1.SelectedObject = this.selectedSession.Client;
                    this.dataGridView2.DataSource = this.selectedSession.Processes;
                }
        }

        private void TerminalServerManager_Load(object sender, EventArgs e)
        {
            this.ServerNameComboBox.Items.Clear();
            foreach (var favorite in this.persistence.Favorites)
                if (favorite.Protocol == KnownConnectionConstants.RDP)
                    this.ServerNameComboBox.Items.Add(favorite.ServerName);
        }

        private void SendMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TerminalServer.SendMessageToSession(this.selectedSession);
        }

        private void LogoffSessionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.selectedSession != null)
                if (MessageBox.Show("Are you sure you want to log off the selected session?", "Confirmation Required",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
                    TerminalServicesAPI.LogOffSession(this.selectedSession, false);
        }

        private void RebootServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.server.IsATerminalServer)
                if (MessageBox.Show("Are you sure you want to reboot this server?", "Confirmation Required",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
                    TerminalServicesAPI.ShutdownSystem(this.server, true);
        }

        private void ShutdownServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.server.IsATerminalServer)
                if (MessageBox.Show("Are you sure you want to shutdown this server?", "Confirmation Required",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
                    TerminalServicesAPI.ShutdownSystem(this.server, false);
        }

        private void ServerNameComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                this.ConnectButton_Click(null, null);
        }
    }
}