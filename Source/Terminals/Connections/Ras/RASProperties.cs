using System;
using System.Windows.Forms;
using FalafelSoftware.TransPort;
using Terminals.Data;
using Terminals.TerminalServices;

namespace Terminals.Connections
{
    internal partial class RASProperties : UserControl
    {
        private DateTime connectedTime = DateTime.MinValue;

        private string Entry = string.Empty;

        private readonly MethodInvoker logMiv;

        private RASConnection rASConnection;

        private readonly Timer timer;

        public RASProperties()
        {
            this.InitializeComponent();
            this.timer = new Timer();
            this.timer.Interval = 500;
            this.timer.Tick += this.timer_Tick;
            this.timer.Start();
            this.logMiv = this.UpdateLog;
        }

        public string LastError { get; set; }

        public RASConnection RASConnection
        {
            get => this.rASConnection;
            set
            {
                this.rASConnection = value;
                this.rASConnection.OnLog += this.rASConnection_OnLog;
            }
        }

        public TerminalServer Server => null;

        public bool IsTerminalServer { get => false; set { } }

        private void UpdateLog()
        {
            this.LogListBox.TopIndex = this.LogListBox.Items.Add(this.Entry);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            this.UpdateStats();
        }

        #region IConnection Members

        public IFavorite Favorite { get => this.RASConnection.Favorite; set => this.RASConnection.Favorite = value; }

        public bool Connect()
        {
            return this.RASConnection.Connect();
        }

        public void Disconnect()
        {
            this.timer.Stop();
            this.RASConnection.Disconnect();
        }

        public bool Connected => this.RASConnection.Connected;

        public void ChangeDesktopSize(DesktopSize size)
        {
        }

        private void UpdateStats()
        {
            this.lbDetails1.Items.Clear();
            this.BringToFront();
            if (this.Connected)
            {
                if (this.connectedTime == DateTime.MinValue)
                    this.connectedTime = DateTime.Now;

                var entry = new RASENTRY();
                this.rASConnection.ras.GetEntry(this.rASConnection.ras.EntryName, ref entry);
                this.AddDetailsText("Connection Status", "Connected");
                this.AddDetailsText("Host", entry.LocalPhoneNumber);
                this.AddDetailsText("IP Address", this.rASConnection.ras.IPAddress());
                var ts = new TimeSpan(DateTime.Now.Ticks - this.connectedTime.Ticks);
                this.AddDetailsText("Connection Duration:",
                    string.Format("{0} Days, {1} Hours, {2} Minutes, {3} Seconds", ts.Days, ts.Hours, ts.Minutes,
                        ts.Seconds));
            }
            else
            {
                this.AddDetailsText("Connection Status", "Not Connected");
            }
        }

        private void AddDetailsText(string caption, object item)
        {
            if (item != null)
                this.lbDetails1.TopIndex = this.lbDetails1.Items.Add(caption + ":  " + item);
        }

        private void rASConnection_OnLog(string Entry)
        {
            this.Entry = Entry;
            lock (this.logMiv)
            {
                this.Invoke(this.logMiv);
            }
        }

        #endregion
    }
}