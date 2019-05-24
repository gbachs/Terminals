using System;
using System.Collections.Generic;
using System.Data;
using System.Management;
using System.Text;
using System.Windows.Forms;

namespace Terminals.Network.WMI
{
    internal partial class Services : UserControl
    {
        private readonly List<ManagementObject> list = new List<ManagementObject>();

        public Services()
        {
            this.InitializeComponent();
        }

        private void LoadServices(string Username, string Password, string Computer)
        {
            var sb = new StringBuilder();
            var qry =
                "select AcceptPause, AcceptStop, Caption, CheckPoint, CreationClassName, Description, DesktopInteract, DisplayName, ErrorControl, ExitCode, InstallDate, Name, PathName, ProcessId,ServiceSpecificExitCode, ServiceType, Started, StartMode, StartName, State, Status, SystemCreationClassName, SystemName, TagId, WaitHint from win32_service";
            ManagementObjectSearcher searcher;
            var query = new ObjectQuery(qry);

            if (Username != "" && Password != "" && Computer != "" && !Computer.StartsWith(@"\\localhost"))
            {
                var oConn = new ConnectionOptions();
                oConn.Username = Username;
                oConn.Password = Password;
                if (!Computer.StartsWith(@"\\")) Computer = @"\\" + Computer;
                if (!Computer.ToLower().EndsWith(@"\root\cimv2")) Computer = Computer + @"\root\cimv2";
                var oMs = new ManagementScope(Computer, oConn);

                searcher = new ManagementObjectSearcher(oMs, query);
            }
            else
            {
                searcher = new ManagementObjectSearcher(query);
            }

            var dt = new DataTable();
            var needsSchema = true;
            var length = 0;
            object[] values = null;
            this.list.Clear();
            foreach (ManagementObject share in searcher.Get())
            {
                var s = new Share();
                this.list.Add(share);
                if (needsSchema)
                {
                    foreach (var p in share.Properties)
                    {
                        var col = new DataColumn(p.Name, this.ConvertCimType(p.Type));
                        dt.Columns.Add(col);
                    }

                    length = share.Properties.Count;
                    needsSchema = false;
                }

                if (values == null) values = new object[length];
                var x = 0;
                foreach (var p in share.Properties)
                    if (p != null && x < length)
                    {
                        values[x] = p.Value;
                        x++;
                    }

                dt.Rows.Add(values);
                values = null;
            }

            this.dataGridView1.DataSource = dt;
        }

        private void Services_Load(object sender, EventArgs e)
        {
            this.LoadServices("", "", "");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.LoadServices(this.wmiServerCredentials1.Username, this.wmiServerCredentials1.Password,
                this.wmiServerCredentials1.SelectedServer);
        }

        private Type ConvertCimType(CimType ctValue)
        {
            Type tReturnVal = null;
            switch (ctValue)
            {
                case CimType.Boolean:
                    tReturnVal = typeof(bool);
                    break;
                case CimType.Char16:
                    tReturnVal = typeof(string);
                    break;
                case CimType.DateTime:
                    tReturnVal = typeof(DateTime);
                    break;
                case CimType.Object:
                    tReturnVal = typeof(object);
                    break;
                case CimType.Real32:
                    tReturnVal = typeof(decimal);
                    break;
                case CimType.Real64:
                    tReturnVal = typeof(decimal);
                    break;
                case CimType.Reference:
                    tReturnVal = typeof(object);
                    break;
                case CimType.SInt16:
                    tReturnVal = typeof(short);
                    break;
                case CimType.SInt32:
                    tReturnVal = typeof(int);
                    break;
                case CimType.SInt8:
                    tReturnVal = typeof(short);
                    break;
                case CimType.String:
                    tReturnVal = typeof(string);
                    break;
                case CimType.UInt16:
                    tReturnVal = typeof(ushort);
                    break;
                case CimType.UInt32:
                    tReturnVal = typeof(uint);
                    break;
                case CimType.UInt64:
                    tReturnVal = typeof(ulong);
                    break;
                case CimType.UInt8:
                    tReturnVal = typeof(ushort);
                    break;
            }

            return tReturnVal;
        }

        private ManagementObject FindWMIObject(string name, string propname)
        {
            ManagementObject foundObj = null;
            foreach (var obj in this.list)
                if (obj.Properties[propname].Value.ToString() == name)
                {
                    foundObj = obj;
                    break;
                }

            return foundObj;
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = this.dataGridView1.Rows[this.dataGridView1.SelectedCells[0].RowIndex].Cells["Name"].Value
                .ToString();
            if (name != null && name != "")
            {
                var obj = this.FindWMIObject(name, "Name");
                if (obj != null)
                {
                    obj.InvokeMethod("PauseService", null);
                    this.LoadServices(this.wmiServerCredentials1.Username, this.wmiServerCredentials1.Password,
                        this.wmiServerCredentials1.SelectedServer);
                }
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = this.dataGridView1.Rows[this.dataGridView1.SelectedCells[0].RowIndex].Cells["Name"].Value
                .ToString();
            if (name != null && name != "")
            {
                var obj = this.FindWMIObject(name, "Name");
                if (obj != null)
                {
                    obj.InvokeMethod("StopService", null);
                    this.LoadServices(this.wmiServerCredentials1.Username, this.wmiServerCredentials1.Password,
                        this.wmiServerCredentials1.SelectedServer);
                }
            }
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = this.dataGridView1.Rows[this.dataGridView1.SelectedCells[0].RowIndex].Cells["Name"].Value
                .ToString();
            if (name != null && name != "")
            {
                var obj = this.FindWMIObject(name, "Name");
                if (obj != null)
                {
                    obj.InvokeMethod("StartService", null);
                    this.LoadServices(this.wmiServerCredentials1.Username, this.wmiServerCredentials1.Password,
                        this.wmiServerCredentials1.SelectedServer);
                }
            }
        }

        private void wmiServerCredentials1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                this.button1_Click(null, null);
        }
    }
}