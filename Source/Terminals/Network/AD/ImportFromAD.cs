using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Terminals.Configuration;
using Terminals.Connections;
using Terminals.Data;
using Terminals.Forms.Controls;

namespace Terminals.Network
{
    internal partial class ImportFromAD : Form
    {
        private readonly ActiveDirectoryClient adClient;

        private readonly ConnectionManager connectionManager;

        private readonly IPersistence persistence;

        private readonly Settings settings = Settings.Instance;

        private string defautlDomainName;

        public ImportFromAD(IPersistence persistence, ConnectionManager connectionManager)
        {
            this.InitializeComponent();

            this.persistence = persistence;
            this.connectionManager = connectionManager;
            this.gridComputers.AutoGenerateColumns = false;

            this.adClient = new ActiveDirectoryClient();
            this.adClient.ListComputersDone += this.AdClient_OnListComputersDone;
            this.adClient.ComputerFound += this.OnClientComputerFound;

            var computers = new SortableList<ActiveDirectoryComputer>();
            this.bsComputers.DataSource = computers;
        }

        private void ImportFromAD_Load(object sender, EventArgs e)
        {
            this.progressBar1.Visible = false;
            this.lblProgressStatus.Text = string.Empty;
            this.defautlDomainName = this.ResolveDomainName();
            this.domainTextbox.Text = this.defautlDomainName;
        }

        private string ResolveDomainName()
        {
            if (!string.IsNullOrEmpty(this.settings.DefaultDomain))
                return this.settings.DefaultDomain;

            return Environment.UserDomainName;
        }

        private void ScanADButton_Click(object sender, EventArgs e)
        {
            if (!this.adClient.IsRunning)
            {
                this.bsComputers.Clear();
                var searchParams = new ActiveDirectorySearchParams(this.domainTextbox.Text,
                    this.ldapFilterTextbox.Text, this.searchbaseTextbox.Text);
                this.adClient.FindComputers(searchParams);
                this.lblProgressStatus.Text = "Contacting domain...";
                this.SwitchToRunningMode();
            }
            else
            {
                this.adClient.Stop();
                this.lblProgressStatus.Text = "Canceling scan...";
            }
        }

        private void SwitchToRunningMode()
        {
            this.progressBar1.Visible = true;
            this.ButtonScanAD.Text = "Stop";
            this.btnSelectAll.Enabled = false;
            this.btnSelectNone.Enabled = false;
            this.ButtonImport.Enabled = false;
        }

        private void SwitchToStoppedMode()
        {
            this.progressBar1.Visible = false;
            this.ButtonScanAD.Text = "Scan";
            this.btnSelectAll.Enabled = true;
            this.btnSelectNone.Enabled = true;
            this.ButtonImport.Enabled = true;
        }

        private void OnClientComputerFound(ActiveDirectoryComputer computer)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new ComputerFoundDelegate(this.OnClientComputerFound), computer);
            }
            else
            {
                this.bsComputers.Add(computer);
                this.lblProgressStatus.Text = string.Format("Scaning... {0} computers found.", this.bsComputers.Count);
                this.gridComputers.Refresh();
            }
        }

        private void AdClient_OnListComputersDone(bool success)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new ListComputersDoneDelegate(this.AdClient_OnListComputersDone), success);
            }
            else
            {
                if (success)
                    this.lblProgressStatus.Text =
                        string.Format("Scan complete, {0} computers found.", this.bsComputers.Count);
                else
                    this.lblProgressStatus.Text = "Scan canceled.";

                this.SwitchToStoppedMode();
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnButtonImportClick(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            var favoritesToImport = this.GetFavoritesFromBindingSource(this.domainTextbox.Text);
            var managedImport = new ImportWithDialogs(this, this.persistence, this.connectionManager);
            managedImport.Import(favoritesToImport);
        }

        private List<FavoriteConfigurationElement> GetFavoritesFromBindingSource(string domain)
        {
            var favoritesToImport = new List<FavoriteConfigurationElement>();
            foreach (DataGridViewRow computerRow in this.gridComputers.SelectedRows)
            {
                var computer = computerRow.DataBoundItem as ActiveDirectoryComputer;
                var newFavorite = computer.ToFavorite(this.connectionManager, domain);
                favoritesToImport.Add(newFavorite);
            }

            return favoritesToImport;
        }

        private void OnBtnSelectAllClick(object sender, EventArgs e)
        {
            this.gridComputers.SelectAll();
        }

        private void OnBtnSelectNoneClick(object sender, EventArgs e)
        {
            this.gridComputers.ClearSelection();
        }

        private void ImportFromAD_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.adClient.ListComputersDone -= this.AdClient_OnListComputersDone;
            this.adClient.ComputerFound -= this.OnClientComputerFound;
            this.adClient.Stop();
        }

        private void GridComputers_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var lastSortedColumn = this.gridComputers.FindLastSortedColumn();
            var column = this.gridComputers.Columns[e.ColumnIndex];

            var newSortDirection = SortableUnboundGrid.GetNewSortDirection(lastSortedColumn, column);
            var data = this.bsComputers.DataSource as SortableList<ActiveDirectoryComputer>;
            this.bsComputers.DataSource = data.SortByProperty(column.DataPropertyName, newSortDirection);
            column.HeaderCell.SortGlyphDirection = newSortDirection;
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            this.ldapFilterTextbox.Text = ActiveDirectorySearchParams.DEFAULT_FILTER;
            this.searchbaseTextbox.Text = string.Empty;
            this.domainTextbox.Text = this.defautlDomainName;
        }
    }
}