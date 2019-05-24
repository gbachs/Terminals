using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Terminals.Connections;
using Terminals.Data;
using Terminals.Forms.Controls;
using Terminals.Network;
using Terminals.Scanner;

namespace Terminals
{
    internal partial class NetworkScanner : Form
    {
        private readonly ConnectionManager connectionManager;

        private readonly IPersistence persistence;

        private readonly Server server;

        private NetworkScanManager manager;

        private bool validation;

        internal NetworkScanner(IPersistence persistence, ConnectionManager connectionManager)
        {
            this.InitializeComponent();

            this.persistence = persistence;
            this.connectionManager = connectionManager;
            this.checkListPorts.DataSource = this.connectionManager.GetAvailableProtocols();
            this.CheckAllPorts();
            this.server = new Server(persistence, this.connectionManager);
            this.FillTextBoxesFromLocalIp();
            this.InitScanManager();
            this.gridScanResults.AutoGenerateColumns = false;
            Client.OnServerConnection += this.Client_OnServerConnection;
            this.bsScanResults.DataSource = new SortableList<NetworkScanResult>();
        }

        private void FillTextBoxesFromLocalIp()
        {
            var localIP = NetworkAdapters.TryGetIPv4LocalAddress();
            var ipList = localIP.Split('.');
            this.ATextbox.Text = ipList[0];
            this.BTextbox.Text = ipList[1];
            this.CTextbox.Text = ipList[2];
            this.DTextbox.Text = "1";
            this.ETextbox.Text = "255";
            this.ServerAddressLabel.Text = localIP;
        }

        private void InitScanManager()
        {
            this.manager = new NetworkScanManager(this.connectionManager);
            this.manager.OnAddressScanHit += this.manager_OnScanHit;
            this.manager.OnAddressScanFinished += this.manager_OnAddresScanFinished;
        }

        private void ScanButton_Click(object sender, EventArgs e)
        {
            this.scanProgressBar.Value = 0;

            if (this.ScanButton.Text == "&Scan")
                this.StartScan();
            else
                this.StopScan();
        }

        private void StopScan()
        {
            this.manager.StopScan();
            this.ScanStatusLabel.Text = "Scan Stopped.";
            this.ScanButton.Text = "&Scan";
        }

        private void StartScan()
        {
            this.bsScanResults.Clear();
            this.ScanStatusLabel.Text = "Initiating Scan...";
            this.ScanButton.Text = "Stop";
            var ports = this.GetSelectedPorts();
            this.manager.StartScan(this.ATextbox.Text, this.BTextbox.Text, this.CTextbox.Text,
                this.DTextbox.Text, this.ETextbox.Text, ports);
        }

        private List<int> GetSelectedPorts()
        {
            return this.checkListPorts.CheckedItems.OfType<string>()
                .Select(this.connectionManager.GetPort)
                .ToList();
        }

        private void manager_OnAddresScanFinished(ScanItemEventArgs args)
        {
            this.Invoke(new MethodInvoker(this.UpdateScanStatus));
        }

        /// <summary>
        ///     Updates the status bar, button state and progress bar.
        ///     The last who sends "is done" autoamticaly informs about the compleated state.
        /// </summary>
        private void UpdateScanStatus()
        {
            this.scanProgressBar.Maximum = this.manager.AllAddressesToScan;
            this.scanProgressBar.Value = this.manager.DoneAddressScans;
            var pendingAddresses = this.manager.AllAddressesToScan - this.scanProgressBar.Value;
            Debug.WriteLine("updating status with pending ({0}): {1}",
                this.manager.ScanIsRunning, pendingAddresses);

            this.ScanStatusLabel.Text = string.Format("Pending items:{0}", pendingAddresses);
            if (this.scanProgressBar.Value >= this.scanProgressBar.Maximum)
                this.scanProgressBar.Value = 0;

            if (pendingAddresses == 0)
            {
                this.ScanButton.Text = "&Scan";
                this.ScanStatusLabel.Text =
                    string.Format("Completed scan, found: {0} items.", this.bsScanResults.Count);
                this.scanProgressBar.Value = 0;
            }
        }

        private void manager_OnScanHit(ScanItemEventArgs args)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new NetworkScanHandler(this.manager_OnScanHit), args);
            }
            else
            {
                this.bsScanResults.Add(args.ScanResult);
                this.gridScanResults.Refresh();
            }
        }

        private void CheckListPorts_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Unchecked)
                this.checkBoxAll.Checked = false;
        }

        private void CheckBoxAll_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBoxAll.Checked)
                this.CheckAllPorts();
        }

        private void CheckAllPorts()
        {
            for (var index = 0; index < this.checkListPorts.Items.Count; index++)
                this.checkListPorts.SetItemChecked(index, true);
        }

        private void AddAllButton_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            var tags = this.GetTagsToApply();
            var favoritesToImport = this.GetFavoritesFromBindingSource(tags);
            this.ImportSelectedItems(favoritesToImport);
        }

        private List<FavoriteConfigurationElement> GetFavoritesFromBindingSource(string tags)
        {
            var favoritesToImport = new List<FavoriteConfigurationElement>();
            foreach (DataGridViewRow scanResultRow in this.gridScanResults.SelectedRows)
            {
                var computer = scanResultRow.DataBoundItem as NetworkScanResult;
                var favorite = computer.ToFavorite(tags);
                favoritesToImport.Add(favorite);
            }

            return favoritesToImport;
        }

        private string GetTagsToApply()
        {
            var tags = this.TagsTextbox.Text;
            tags = tags.Replace("Groups...", string.Empty).Trim();
            return tags;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (this.server.ServerOnline)
            {
                this.button1.Text = "Start Server";
                this.server.Stop();
            }
            else
            {
                this.button1.Text = "Stop Server";
                this.server.Start();
            }

            if (this.server.ServerOnline)
                this.ServerStatusLabel.Text = "Server is ONLINE";
            else
                this.ServerStatusLabel.Text = "Server is OFFLINE";
        }

        private void ImportSelectedItems(List<FavoriteConfigurationElement> favoritesToImport)
        {
            var managedImport = new ImportWithDialogs(this, this.persistence, this.connectionManager);
            managedImport.Import(favoritesToImport);
        }

        private void Client_OnServerConnection(ShareFavoritesEventArgs args)
        {
            this.ImportSelectedItems(args.Favorites);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Client.Start(this.ServerAddressTextbox.Text);
        }

        private void NetworkScanner_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Client.OnServerConnection -= this.Client_OnServerConnection;
                this.manager.StopScan();
                this.server.Stop();
                Client.Stop();
            }
            catch (Exception exc)
            {
                Logging.Info("Network Scanner failed to stop server and client at close", exc);
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        ///     Validate text boxes to allow inser only byte.
        /// </summary>
        private void IPTextbox_TextChanged(object sender, EventArgs e)
        {
            if (this.validation)
                return; // prevent stack overflow

            byte testValue;
            this.validation = true;
            var textBox = sender as TextBox;
            var isValid = byte.TryParse(textBox.Text, NumberStyles.None, null, out testValue);

            if (!isValid && this.validation)
                textBox.Text = textBox.Tag.ToString();
            else
                textBox.Tag = textBox.Text;

            this.validation = false;
        }

        private void GridScanResults_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var lastSortedColumn = this.gridScanResults.FindLastSortedColumn();
            var column = this.gridScanResults.Columns[e.ColumnIndex];

            var newSortDirection = SortableUnboundGrid.GetNewSortDirection(lastSortedColumn, column);
            var data = this.bsScanResults.DataSource as SortableList<NetworkScanResult>;
            this.bsScanResults.DataSource = data.SortByProperty(column.DataPropertyName, newSortDirection);
            column.HeaderCell.SortGlyphDirection = newSortDirection;
        }
    }
}