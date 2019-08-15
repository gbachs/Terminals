using System;
using System.Drawing;
using System.Windows.Forms;
using Terminals.Data;

namespace Terminals.Connections
{
    internal partial class TabbedTools : UserControl
    {
        public delegate void TabChanged(object sender, TabControlEventArgs e);

        private readonly PacketCapture packetCapture1;

        public TabbedTools()
        {
            this.InitializeComponent();

            try
            {
                this.packetCapture1 = new PacketCapture();

                // 
                // PcapTabPage
                // 
                this.PcapTabPage.Controls.Add(this.packetCapture1);
                this.PcapTabPage.Location = new Point(4, 22);
                this.PcapTabPage.Name = "PcapTabPage";
                this.PcapTabPage.Padding = new Padding(3);
                this.PcapTabPage.Size = new Size(886, 309);
                this.PcapTabPage.TabIndex = 15;
                this.PcapTabPage.Text = "Packets";
                this.PcapTabPage.UseVisualStyleBackColor = true;
                // 
                // packetCapture1
                // 
                this.packetCapture1.Dock = DockStyle.Fill;
                this.packetCapture1.Location = new Point(3, 3);
                this.packetCapture1.Name = "packetCapture1";
                this.packetCapture1.Size = new Size(880, 303);
                this.packetCapture1.TabIndex = 0;
            }
            catch (Exception e)
            {
                this.PcapTabPage.Controls.Clear();
                var l = new Label();
                this.PcapTabPage.Controls.Add(l);
                l.Text = "Packet Capture is either not install or not supported on this version of windows.";
                l.Dock = DockStyle.Top;
                Logging.Info(l.Text, e);
            }
        }

        public event TabChanged OnTabChanged;

        private void TabControl1_Selected(object sender, TabControlEventArgs e)
        {
            this.OnTabChanged?.Invoke(sender, e);
        }

        public void HideTab(int index)
        {
            if (this.tabControl1.TabCount > index)
                this.tabControl1.TabPages[index].Hide();
        }

        public void Execute(NettworkingTools action, string host, IPersistence persistence)
        {
            this.terminalServerManager1.AssignPersistence(persistence);
            this.ExecuteAction(action, host);
        }

        private void ExecuteAction(NettworkingTools action, string host)
        {
            switch (action)
            {
                case NettworkingTools.Ping:
                    this.tabControl1.SelectedTab = this.tabControl1.TabPages[0];
                    this.ping1.ForcePing(host);

                    break;
                case NettworkingTools.Dns:
                    this.tabControl1.SelectedTab = this.tabControl1.TabPages[6];
                    this.dnsLookup1.ForceDNS(host);
                    break;

                case NettworkingTools.Trace:
                    this.tabControl1.SelectedTab = this.tabControl1.TabPages[1];
                    this.traceRoute1.ForceTrace(host);
                    break;

                case NettworkingTools.TsAdmin:
                    this.tabControl1.SelectedTab = this.tabControl1.TabPages[10];
                    this.terminalServerManager1.ForceTSAdmin(host);
                    break;
            }
        }
    }
}