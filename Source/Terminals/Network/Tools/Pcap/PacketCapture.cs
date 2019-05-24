using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Be.Windows.Forms;
using Tamir.IPLib;
using Tamir.IPLib.Packets;
using Terminals.Properties;
using Terminals.Services;

namespace Terminals
{
    public partial class PacketCapture : UserControl
    {
        private PcapDevice dev;

        private PcapDeviceList devices;

        private readonly string DumpFile = @"c:\Terminals.dump";

        private List<Packet> newpackets = new List<Packet>();

        private List<Packet> packets = new List<Packet>();

        private MethodInvoker stopUpdater;

        private MethodInvoker updater;

        public PacketCapture()
        {
            this.InitializeComponent();
        }

        private void PacketCapture_Load(object sender, EventArgs e)
        {
            try
            {
                this.promiscuousCheckbox.Enabled = true;
                this.DumpToFileCheckbox.Enabled = true;
                this.StopCaptureButton.Enabled = false;
                this.AmberPicture.Visible = true;
                this.GreenPicture.Visible = false;
                this.RedPicture.Visible = false;
                this.updater = this.UpdateUI;
                this.stopUpdater = this.PcapStopped;
                this.devices = SharpPcap.GetAllDevices();
                foreach (PcapDevice device in this.devices)
                    this.comboBox1.Items.Add(device.PcapDescription);
                if (this.devices.Count > 0)
                    this.comboBox1.SelectedIndex = 1;
                this.webBrowser1.DocumentStream = new MemoryStream(Encoding.Default.GetBytes(Resources.Filtering));
            }
            catch (Exception exc)
            {
                this.Enabled = false;
                if (exc is BadImageFormatException)
                {
                    Logging.Info(
                        "Terminals Packet Capture is not configured to work with this system (Bad Image Format Exception)",
                        exc);
                    MessageBox.Show(
                        "Terminals Packet Capture is not configured to work with this system (Bad Image Format Exception)");
                }
                else if (exc is DllNotFoundException)
                {
                    Logging.Info("WinpPcap was not installed", exc);
                    ExternalLinks.ShowWinPCapPage();
                }
                else
                {
                    Logging.Info("WinpPcap was not installed correctly", exc);
                }
            }

            this.PacketCapture_Resize(null, null);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var index = this.comboBox1.SelectedIndex;
            this.dev = this.devices[index];
            this.propertyGrid1.SelectedObject = this.dev as NetworkDevice;
        }

        private void StartCapture(object state)
        {
            var dev = (PcapDevice)state;
            dev.PcapOpen(this.promiscuousCheckbox.Checked);
            if (this.DumpToFileCheckbox.Checked)
                dev.PcapDumpOpen(this.DumpFile);
            try
            {
                dev.PcapSetFilter(this.FilterTextBox.Text);
            }
            catch (Exception exc)
            {
                MessageBox.Show("Failed to set the filter: " + this.FilterTextBox.Text);
                Logging.Info("Failed to set the filter: " + this.FilterTextBox.Text, exc);
            }

            dev.PcapStartCapture();
        }

        private void CaptureButton_Click(object sender, EventArgs e)
        {
            this.DumpToFileCheckbox.Enabled = false;
            this.promiscuousCheckbox.Enabled = false;
            this.CaptureButton.Enabled = false;
            this.StopCaptureButton.Enabled = true;
            this.AmberPicture.Visible = false;
            this.GreenPicture.Visible = true;
            this.RedPicture.Visible = false;
            this.listBox1.Items.Clear();
            lock (this.packets)
            {
                this.packets = new List<Packet>();
                this.newpackets = new List<Packet>();
                this.dev.PcapOnPacketArrival += this.dev_PcapOnPacketArrival;
                this.dev.PcapOnCaptureStopped += this.dev_PcapOnCaptureStopped;
            }

            ThreadPool.QueueUserWorkItem(this.StartCapture, this.dev);
        }

        private void dev_PcapOnCaptureStopped(object sender, bool error)
        {
            this.Invoke(this.stopUpdater);
        }

        private void PcapStopped()
        {
            this.CaptureButton.Enabled = true;
            this.StopCaptureButton.Enabled = false;
            this.RedPicture.Visible = false;
            this.GreenPicture.Visible = false;
            this.AmberPicture.Visible = true;
            this.DumpToFileCheckbox.Enabled = true;
            this.promiscuousCheckbox.Enabled = true;
        }

        private void UpdateUI()
        {
            lock (this.packets)
            {
                this.GreenPicture.Visible = false;
                Application.DoEvents();
                foreach (var packet in this.newpackets)
                {
                    this.listBox1.Items.Add(packet);
                    this.newpackets = new List<Packet>();
                }

                Application.DoEvents();
                this.GreenPicture.Visible = true;
            }
        }

        private void dev_PcapOnPacketArrival(object sender, Packet packet)
        {
            lock (this.packets)
            {
                this.packets.Add(packet);
                this.newpackets.Add(packet);
            }

            if (this.dev.PcapDumpOpened)
                this.dev.PcapDump(packet);
            this.Invoke(this.updater);
        }

        private void StopCapture(object state)
        {
            var dev = (PcapDevice)state;
            dev.PcapStopCapture();
            dev.PcapClose();
            dev.PcapDumpFlush();
            dev.PcapDumpClose();
        }

        private void StopCaptureButton_Click(object sender, EventArgs e)
        {
            this.CaptureButton.Enabled = false;
            this.StopCaptureButton.Enabled = false;
            this.RedPicture.Visible = true;
            this.GreenPicture.Visible = false;
            this.AmberPicture.Visible = false;
            this.DumpToFileCheckbox.Enabled = true;
            this.promiscuousCheckbox.Enabled = true;
            ThreadPool.QueueUserWorkItem(this.StopCapture, this.dev);
            if (this.DumpToFileCheckbox.Checked)
                ExternalLinks.OpenFileInNotepad(this.DumpFile);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedIndex > 0)
            {
                var packet = this.packets[this.listBox1.SelectedIndex];
                var provider = new DynamicByteProvider(packet.Data);
                this.hexBox1.ByteProvider = provider;
                this.textBox1.Text = Encoding.Default.GetString(packet.Data);
                this.treeView1.Nodes.Clear();
                var header = this.treeView1.Nodes.Add("Header");
                header.Nodes.Add(string.Format("Length:{0}", packet.Header.Length));
                var sb = new StringBuilder();
                foreach (var b in packet.Header)
                {
                    sb.Append(b.ToString("00"));
                    sb.Append(" ");
                }

                header.Nodes.Add(string.Format("Data:{0}", sb));
                header.Nodes.Add(string.Format("Capture Length:{0}", packet.PcapHeader.CaptureLength.ToString()));
                header.Nodes.Add(string.Format("Packet Length:{0}", packet.PcapHeader.PacketLength.ToString()));
                header.Nodes.Add(string.Format("Date:{0}", packet.PcapHeader.Date.ToString()));
                header.Nodes.Add(string.Format("Microseconds:{0}", packet.PcapHeader.MicroSeconds.ToString()));
                header.Nodes.Add(string.Format("Seconds:{0}", packet.PcapHeader.Seconds.ToString()));

                this.treeView1.ExpandAll();
            }
        }

        private void PacketCapture_Resize(object sender, EventArgs e)
        {
            if (this.hexBox1.Width > 640)
                this.hexBox1.BytesPerLine = 16;
            else
                this.hexBox1.BytesPerLine = 8;
        }
    }
}