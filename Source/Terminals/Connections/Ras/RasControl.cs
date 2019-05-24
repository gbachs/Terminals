using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using FalafelSoftware.TransPort;
using Terminals.Common.Connections;

namespace Terminals.Forms.EditFavorite
{
    // todo feature disabled, because it is broken
    internal partial class RasControl : UserControl
    {
        private Dictionary<string, RASENTRY> dialupList = new Dictionary<string, RASENTRY>();

        internal RasControl()
        {
            this.InitializeComponent();

            this.ConnectionNames = new List<string>();
        }

        internal List<string> ConnectionNames { get; }

        internal void OnServerNameChanged(string protocolName, string serverName)
        {
            if (protocolName == KnownConnectionConstants.RAS)
                this.FillRasControls(serverName);
        }

        private void FillRasControls(string serverName)
        {
            this.LoadDialupConnections();
            this.RASDetailsListBox.Items.Clear();
            if (this.dialupList != null && this.dialupList.ContainsKey(serverName))
            {
                var selectedRAS = this.dialupList[serverName];
                this.RASDetailsListBox.Items.Add(string.Format("{0}:{1}", "Connection", serverName));
                this.RASDetailsListBox.Items.Add(string.Format("{0}:{1}", "Area Code", selectedRAS.AreaCode));
                this.RASDetailsListBox.Items.Add(string.Format("{0}:{1}", "Country Code", selectedRAS.CountryCode));
                this.RASDetailsListBox.Items.Add(string.Format("{0}:{1}", "Device Name", selectedRAS.DeviceName));
                this.RASDetailsListBox.Items.Add(string.Format("{0}:{1}", "Device Type", selectedRAS.DeviceType));
                this.RASDetailsListBox.Items.Add(string.Format("{0}:{1}", "Local Phone Number",
                    selectedRAS.LocalPhoneNumber));
            }
        }

        public void SetControls()
        {
            this.LoadDialupConnections();
            this.RASDetailsListBox.Items.Clear();
        }

        private void LoadDialupConnections()
        {
            this.dialupList = new Dictionary<string, RASENTRY>();
            this.ConnectionNames.Clear();
            var rasEntries = new ArrayList();
            this.ras1.ListEntries(ref rasEntries);
            foreach (string item in rasEntries)
            {
                var details = new RASENTRY();
                this.ras1.GetEntry(item, ref details);
                this.dialupList.Add(item, details);
                this.ConnectionNames.Add(item);
            }
        }
    }
}