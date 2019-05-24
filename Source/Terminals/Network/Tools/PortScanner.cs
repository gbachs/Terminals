using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Metro;
using Metro.Scanning;

namespace Terminals.Network
{
    internal partial class PortScanner : UserControl
    {
        private readonly object resultsLock = new object();

        private readonly MethodInvoker updateConnections;

        private int counter;

        private IPAddress endPointAddress;

        private int portCount;

        private List<ScanResult> results;

        private List<TcpSynScanner> scanners;

        public PortScanner()
        {
            this.InitializeComponent();

            this.updateConnections = this.UpdateConnections;
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            this.scanners = new List<TcpSynScanner>();
            this.results = new List<ScanResult>();
            this.StartButton.Enabled = false;
            //System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(ScanSubnet), null);
            this.ScanSubnet();
        }

        private void ScanSubnet()
        {
            var startPort = this.pa.Text;
            var endPort = this.pb.Text;
            var iStartPort = 0;
            var iEndPort = 0;
            if (int.TryParse(startPort, out iStartPort) && int.TryParse(endPort, out iEndPort))
            {
                if (iStartPort > iEndPort)
                {
                    var iPortTemp = iStartPort;
                    iStartPort = iEndPort;
                    iEndPort = iPortTemp;
                }

                var ports = new ushort[iEndPort - iStartPort + 1];
                var portsCounter = 0;
                for (var y = iStartPort; y <= iEndPort; y++)
                {
                    ports[portsCounter] = (ushort)y;
                    portsCounter++;
                }

                this.portCount = ports.Length;
                var initial = string.Format("{0}.{1}.{2}.", this.a.Text, this.b.Text, this.c.Text);
                var start = this.d.Text;
                var end = this.e.Text;
                var iStart = 0;
                var iEnd = 0;
                if (int.TryParse(start, out iStart) && int.TryParse(end, out iEnd))
                {
                    if (iStart > iEnd)
                    {
                        var iTemp = iStart;
                        iStart = iEnd;
                        iEnd = iTemp;
                    }

                    for (var x = iStart; x <= iEnd; x++)
                    {
                        IPAddress finalAddress;
                        if (IPAddress.TryParse(initial + x, out finalAddress))
                            try
                            {
                                ThreadPool.QueueUserWorkItem(this.ScanMachine, new object[] {finalAddress, ports});
                            }
                            catch (Exception exc)
                            {
                                Logging.Error("Threaded Scan Machine Call", exc);
                            }
                    }
                }
            }
        }

        private void ScanMachine(object state)
        {
            try
            {
                var states = (object[])state;
                var address = (IPAddress)states[0];
                var ports = (ushort[])states[1];

                var scanner = new TcpSynScanner(new IPEndPoint(this.endPointAddress, 0));
                scanner.PortReply += this.Scanner_PortReply;
                scanner.ScanComplete += this.Scanner_ScanComplete;
                this.scanners.Add(scanner);
                scanner.StartScan(address, ports, 1000, 100, true);
                this.counter = this.counter + ports.Length;
            }
            catch (NotSupportedException) // thrown by constructor of packet sniffer
            {
                MessageBox.Show("Port scanner requires administrative priviledges to run!", "Terminals - port scanner",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (Exception exception)
            {
                Logging.Info("Scanner caught an exception", exception);
            }

            if (!this.IsDisposed) this.Invoke(this.updateConnections);
            Application.DoEvents();
        }

        private void PortScanner_Load(object sender, EventArgs eargs)
        {
            var interfaceList = new NetworkInterfaceList();
            try
            {
                foreach (var face in interfaceList.Interfaces)
                    if (face.IsEnabled && !face.isLoopback)
                    {
                        this.endPointAddress = face.Address;
                        var parts = this.endPointAddress.ToString().Split('.');
                        this.a.Text = parts[0];
                        this.b.Text = parts[1];
                        this.c.Text = parts[2];
                        this.d.Text = parts[3];
                        this.e.Text = parts[3];
                        break;
                    }
            }
            catch (Exception exc)
            {
                Logging.Error("Connecting to the network interfaces", exc);
            }
        }

        private void Scanner_ScanComplete()
        {
            if (this.counter > 0) this.counter = this.counter - this.portCount;
            this.Invoke(this.updateConnections);
            this.StartButton.Enabled = true;
        }

        private void Scanner_PortReply(IPEndPoint remoteEndPoint, TcpPortState state)
        {
            this.counter--;
            var r = new ScanResult();
            r.RemoteEndPoint = new IPEndPoint(remoteEndPoint.Address, remoteEndPoint.Port);
            r.State = state;
            lock (this.resultsLock)
            {
                this.results.Add(r);
            }

            this.Invoke(this.updateConnections);
        }

        private void UpdateConnections()
        {
            this.resultsGridView.Rows.Clear();
            if (this.resultsGridView.Columns.Count == 0)
            {
                this.resultsGridView.Columns.Add("EndPoint", "End Point");
                this.resultsGridView.Columns.Add("State", "State");
            }

            lock (this.resultsLock)
            {
                foreach (var result in this.results)
                    if (result.State == TcpPortState.Opened)
                        this.resultsGridView.Rows.Add(result.RemoteEndPoint, result.State);
            }

            if (this.counter <= 0)
            {
                this.StopButton.Enabled = false;
                this.StartButton.Enabled = true;
                this.counter = 0;
            }

            this.ScanResultsLabel.Text = string.Format("Outsanding Requests:{0}", this.counter);
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            foreach (var scanner in this.scanners)
                if (scanner.Running)
                    scanner.CancelScan();
            this.Invoke(this.updateConnections);
        }

        private void A_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.OemPeriod)
                this.b.Focus();
        }

        private void B_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.OemPeriod)
                this.c.Focus();
        }

        private void C_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.OemPeriod)
                this.d.Focus();
        }

        private void D_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.OemPeriod)
                this.e.Focus();
        }

        private void E_KeyUp(object sender, KeyEventArgs earg)
        {
            if (earg.KeyCode == Keys.Enter || earg.KeyCode == Keys.OemPeriod)
                this.pa.Focus();
        }

        private void Pa_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.OemPeriod)
                this.pb.Focus();
        }

        private void Pb_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                this.StartButton_Click(null, null);
        }

        private void CopyRemoteAddressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.resultsGridView.SelectedCells.Count > 0 &&
                this.resultsGridView.SelectedCells[0].RowIndex <= this.resultsGridView.Rows.Count)
            {
                var ip = this.resultsGridView.Rows[this.resultsGridView.SelectedCells[0].RowIndex].Cells[0].Value
                    .ToString();
                if (ip != null && ip.IndexOf(":") > 0)
                {
                    ip = ip.Substring(0, ip.IndexOf(":"));
                    Clipboard.SetText(ip, TextDataFormat.Text);
                }
            }
        }
    }

    internal class ScanResult
    {
        public IPEndPoint RemoteEndPoint { get; set; }

        public TcpPortState State { get; set; }
    }
}