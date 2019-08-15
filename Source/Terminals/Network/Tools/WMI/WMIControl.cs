using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Terminals;
using Terminals.Properties;

namespace WMITestClient
{
    /// <summary>
    ///     Summary description for Form1.
    /// </summary>
    internal class WMIControl : UserControl
    {
        private MenuItem BasicTreemenuItem;

        private MenuItem ClearmenuItem;

        private IContainer components;

        private Button ConnectButton;

        private Label ConnectionLabel;

        private MenuItem ExitmenuItem;

        private ArrayList history = new ArrayList();

        private MenuItem LoadmenuItem;

        private MenuItem LoginMenuItem;

        private MainMenu mainMenu1;

        private MenuItem menuItem1;

        private MenuItem menuItem2;

        private OpenFileDialog openFileDialog1;

        private Panel panel1;

        private ProgressBar progressBar1;

        private Button QueryButton;

        private ComboBox QueryTextBox;

        private MenuItem SavemenuItem;

        private SplitContainer splitContainer1;

        private SplitContainer splitContainer2;

        private bool StilRunning = true;

        private Button StopButton;

        private TreeView treeView1;

        private TreeView treeView2;

        public WMIControl()
        {
            this.InitializeComponent();
            this.Form1_Resize(null, null);
        }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Computer { get; set; }

        private string HistoryPath => Application.StartupPath + "\\WMITestClientHistory.txt";

        public static DataTable WMIToDataTable(string query, string computer, string username, string password)
        {
            var qry = query;
            ManagementObjectSearcher searcher;
            var queryObj = new ObjectQuery(qry);

            if (username != string.Empty && password != string.Empty && computer != string.Empty &&
                !computer.StartsWith(@"\\localhost"))
            {
                var oConn = new ConnectionOptions();
                oConn.Username = username;
                oConn.Password = password;
                if (!computer.StartsWith(@"\\"))
                    computer = @"\\" + computer;

                if (!computer.ToLower().EndsWith(@"\root\cimv2"))
                    computer += @"\root\cimv2";

                var oMs = new ManagementScope(computer, oConn);
                searcher = new ManagementObjectSearcher(oMs, queryObj);
            }
            else
            {
                searcher = new ManagementObjectSearcher(queryObj);
            }

            var dt = new DataTable();
            var needsSchema = true;
            var length = 0;
            object[] values = null;
            foreach (ManagementObject share in searcher.Get())
            {
                if (needsSchema)
                {
                    foreach (var p in share.Properties)
                    {
                        var col = new DataColumn(p.Name, ConvertCimType(p.Type));
                        dt.Columns.Add(col);
                    }

                    length = share.Properties.Count;
                    needsSchema = false;
                }

                if (values == null)
                    values = new object[length];

                var x = 0;
                foreach (var p in share.Properties)
                {
                    if (p.Type == CimType.DateTime)
                        values[x] = ManagementDateTimeConverter.ToDateTime(p.Value.ToString());
                    else
                        values[x] = p.Value;

                    x++;
                }

                dt.Rows.Add(values);
                values = null;
            }

            return dt;
        }

        public static Type ConvertCimType(CimType ctValue)
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

        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.components?.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("To load-> Double Click");
            this.QueryButton = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.StopButton = new System.Windows.Forms.Button();
            this.QueryTextBox = new System.Windows.Forms.ComboBox();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.LoginMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.SavemenuItem = new System.Windows.Forms.MenuItem();
            this.LoadmenuItem = new System.Windows.Forms.MenuItem();
            this.ClearmenuItem = new System.Windows.Forms.MenuItem();
            this.BasicTreemenuItem = new System.Windows.Forms.MenuItem();
            this.ExitmenuItem = new System.Windows.Forms.MenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.treeView2 = new System.Windows.Forms.TreeView();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.ConnectionLabel = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // QueryButton
            // 
            this.QueryButton.Anchor =
                ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top |
                                                      System.Windows.Forms.AnchorStyles.Right)));
            this.QueryButton.Location = new System.Drawing.Point(592, 7);
            this.QueryButton.Name = "QueryButton";
            this.QueryButton.Size = new System.Drawing.Size(67, 24);
            this.QueryButton.TabIndex = 1;
            this.QueryButton.Text = "&Query";
            this.QueryButton.Click += new System.EventHandler(this.QueryButton_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar1.Location = new System.Drawing.Point(0, 81);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(753, 10);
            this.progressBar1.TabIndex = 5;
            // 
            // StopButton
            // 
            this.StopButton.Anchor =
                ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top |
                                                      System.Windows.Forms.AnchorStyles.Right)));
            this.StopButton.Enabled = false;
            this.StopButton.Location = new System.Drawing.Point(667, 7);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(67, 24);
            this.StopButton.TabIndex = 2;
            this.StopButton.Text = "Stop!";
            this.StopButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // QueryTextBox
            // 
            this.QueryTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.QueryTextBox.Location = new System.Drawing.Point(0, 0);
            this.QueryTextBox.Name = "QueryTextBox";
            this.QueryTextBox.Size = new System.Drawing.Size(753, 24);
            this.QueryTextBox.TabIndex = 0;
            this.QueryTextBox.Text = "select * from CIM_System";
            this.QueryTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.QueryTextBox_KeyUp);
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[]
            {
                this.menuItem1
            });
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[]
            {
                this.LoginMenuItem,
                this.menuItem2,
                this.BasicTreemenuItem,
                this.ExitmenuItem
            });
            this.menuItem1.Text = "&Options";
            // 
            // LoginMenuItem
            // 
            this.LoginMenuItem.Index = 0;
            this.LoginMenuItem.Text = "&Login";
            this.LoginMenuItem.Click += new System.EventHandler(this.LoginMenuItem_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 1;
            this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[]
            {
                this.SavemenuItem,
                this.LoadmenuItem,
                this.ClearmenuItem
            });
            this.menuItem2.Text = "&History";
            // 
            // SavemenuItem
            // 
            this.SavemenuItem.Index = 0;
            this.SavemenuItem.Text = "&Save";
            this.SavemenuItem.Click += new System.EventHandler(this.SavemenuItem_Click);
            // 
            // LoadmenuItem
            // 
            this.LoadmenuItem.Index = 1;
            this.LoadmenuItem.Text = "L&oad";
            this.LoadmenuItem.Click += new System.EventHandler(this.LoadmenuItem_Click);
            // 
            // ClearmenuItem
            // 
            this.ClearmenuItem.Index = 2;
            this.ClearmenuItem.Text = "&Clear";
            this.ClearmenuItem.Click += new System.EventHandler(this.ClearmenuItem_Click);
            // 
            // BasicTreemenuItem
            // 
            this.BasicTreemenuItem.Index = 2;
            this.BasicTreemenuItem.Text = "Load Static Class Tree";
            this.BasicTreemenuItem.Click += new System.EventHandler(this.BasicTreemenuItem_Click);
            // 
            // ExitmenuItem
            // 
            this.ExitmenuItem.Index = 3;
            this.ExitmenuItem.Text = "E&xit";
            this.ExitmenuItem.Click += new System.EventHandler(this.ExitmenuItem_Click);
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(399, 374);
            this.treeView1.TabIndex = 3;
            this.treeView1.DoubleClick += new System.EventHandler(this.treeView1_DoubleClick);
            // 
            // treeView2
            // 
            this.treeView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView2.Location = new System.Drawing.Point(0, 0);
            this.treeView2.Name = "treeView2";
            treeNode1.Name = "";
            treeNode1.Text = "To load-> Double Click";
            this.treeView2.Nodes.AddRange(new System.Windows.Forms.TreeNode[]
            {
                treeNode1
            });
            this.treeView2.Size = new System.Drawing.Size(350, 374);
            this.treeView2.TabIndex = 4;
            this.treeView2.Click += new System.EventHandler(this.treeView2_Click);
            this.treeView2.DoubleClick += new System.EventHandler(this.treeView2_DoubleClick);
            // 
            // ConnectButton
            // 
            this.ConnectButton.Location = new System.Drawing.Point(4, 7);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(67, 24);
            this.ConnectButton.TabIndex = 3;
            this.ConnectButton.Text = "Connect...";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            this.splitContainer1.Panel1.Controls.Add(this.QueryTextBox);
            this.splitContainer1.Panel1.Controls.Add(this.progressBar1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(753, 469);
            this.splitContainer1.SplitterDistance = 91;
            this.splitContainer1.TabIndex = 7;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ConnectionLabel);
            this.panel1.Controls.Add(this.QueryButton);
            this.panel1.Controls.Add(this.StopButton);
            this.panel1.Controls.Add(this.ConnectButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(753, 57);
            this.panel1.TabIndex = 7;
            // 
            // ConnectionLabel
            // 
            this.ConnectionLabel.AutoSize = true;
            this.ConnectionLabel.Location = new System.Drawing.Point(88, 13);
            this.ConnectionLabel.Name = "ConnectionLabel";
            this.ConnectionLabel.Size = new System.Drawing.Size(141, 17);
            this.ConnectionLabel.TabIndex = 7;
            this.ConnectionLabel.Text = "\\\\localhost\\root\\cimv2";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.treeView2);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.treeView1);
            this.splitContainer2.Size = new System.Drawing.Size(753, 374);
            this.splitContainer2.SplitterDistance = 350;
            this.splitContainer2.TabIndex = 5;
            // 
            // WMIControl
            // 
            this.Controls.Add(this.splitContainer1);
            this.Name = "WMIControl";
            this.Size = new System.Drawing.Size(753, 469);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        public void Form1_Resize(object sender, EventArgs e)
        {
            //this.tabControl1.Height = this.Height - 50;
            //this.StopButton.Left = this.Width - QueryButton.Width - 15;
            //QueryButton.Left = StopButton.Left - QueryButton.Width - 15;
            //this.ConnectButton.Left = QueryButton.Left - StopButton.Left - QueryButton.Width - StopButton.Width - 15;

            //this.QueryTextBox.Width = QueryButton.Left - 15;
            this.progressBar1.Width = this.Width - 10;
            //progressBar1.Left=this.Width-progressBar1.Width-25;
            //progressBar1.Top=this.Height-progressBar1.Height-33;
        }

        private void IncrementBar()
        {
            if (this.progressBar1.Value >= this.progressBar1.Maximum)
                this.progressBar1.Value = 0;

            this.progressBar1.Value++;
        }

        private void AddToHistory()
        {
            if (!this.history.Contains(this.QueryTextBox.Text))
            {
                this.QueryTextBox.Items.Add(this.QueryTextBox.Text);
                this.history.Add(this.QueryTextBox.Text);
            }
        }

        private void SaveHistory()
        {
            var sb = new StringBuilder();
            foreach (string historyItem in this.history)
                if (historyItem != "")
                    sb.Append(historyItem + "\r\n");

            var sw = new StreamWriter(this.HistoryPath, false);
            sw.Write(sb.ToString());
            sw.Close();
        }

        private void LoadHistory(string path)
        {
            var realPath = path;
            if (realPath == null || realPath == string.Empty)
                realPath = this.HistoryPath;

            if (File.Exists(realPath))
            {
                var sr = new StreamReader(realPath);
                var contents = sr.ReadToEnd();
                sr.Close();
                var items = Regex.Split(contents, "\r\n");
                foreach (var item in items)
                    if (item != string.Empty)
                    {
                        this.history.Add(item);
                        this.QueryTextBox.Items.Add(item);
                    }
            }
        }

        private void QueryButton_Click(object sender, EventArgs e)
        {
            this.AddToHistory();
            this.StilRunning = true;
            this.QueryButton.Enabled = false;
            this.StopButton.Enabled = true;
            try
            {
                this.treeView1.Nodes.Clear();
                if (this.QueryTextBox.Text != string.Empty)
                {
                    var qry = this.QueryTextBox.Text;
                    ManagementObjectSearcher searcher;
                    var query = new ObjectQuery(qry);

                    if (!string.IsNullOrEmpty(this.Username) && !string.IsNullOrEmpty(this.Password) &&
                        !string.IsNullOrEmpty(this.Computer) && !this.Computer.StartsWith(@"\\localhost"))
                    {
                        var oConn = new ConnectionOptions();
                        oConn.Username = this.Username;
                        oConn.Password = this.Password;

                        var oMs = new ManagementScope(this.Computer, oConn);
                        searcher = new ManagementObjectSearcher(oMs, query);
                    }
                    else
                    {
                        searcher = new ManagementObjectSearcher(query);
                    }

                    var root = new TreeNode(qry);
                    root.Tag = "RootNode";
                    root.Expand();
                    this.treeView1.Nodes.Add(root);
                    var path = "";
                    foreach (ManagementObject share in searcher.Get())
                    {
                        var item = new TreeNode(share.ClassPath.ClassName);
                        item.Tag = "ClassNode";
                        root.Nodes.Add(item);
                        path = "Enumerating:" + share.ClassPath.ClassName;
                        foreach (var p in share.Properties)
                        {
                            var isLocal = p.IsLocal;
                            var type = p.Type.ToString();
                            var origin = p.Origin;
                            var name = p.Name;
                            path = "Enumerating:" + share.ClassPath.ClassName + "," + name;
                            Application.DoEvents();

                            var IsArray = p.IsArray;
                            var val = "NULL";
                            if (p.Value != null)
                                val = p.Value.ToString();

                            var node = new TreeNode(name);
                            node.Tag = "PropertyNode";
                            var display = "";
                            if (type.ToLower() == "string")
                                display = "Value='" + val + "'";
                            else
                                display = "Value=" + val;
                            var ValueNode = new TreeNode(display);
                            ValueNode.Tag = "ValueNode";
                            var TypeNode = new TreeNode("Type='" + type + "'");
                            TypeNode.Tag = "ValueNode";
                            var localNode = new TreeNode("IsLocal=" + isLocal);
                            localNode.Tag = "ValueNode";
                            var OriginNode = new TreeNode("Origin='" + origin + "'");
                            OriginNode.Tag = "ValueNode";
                            var IsArrayNode = new TreeNode("IsArray=" + IsArray);
                            IsArrayNode.Tag = "ValueNode";

                            node.Nodes.Add(ValueNode);
                            node.Nodes.Add(TypeNode);
                            node.Nodes.Add(localNode);
                            node.Nodes.Add(OriginNode);
                            node.Nodes.Add(IsArrayNode);

                            if (IsArray && p.Value != null)
                            {
                                var a = (Array)p.Value;
                                for (var x = 0; x < a.Length; x++)
                                {
                                    var v = "";
                                    if (a.GetValue(x) != null)
                                        v = a.GetValue(x).ToString();

                                    var arrayNode = new TreeNode(name + "[" + x + "]=" + v);
                                    arrayNode.Tag = "ArrayNode";
                                    IsArrayNode.Nodes.Add(arrayNode);
                                    this.IncrementBar();
                                }
                            }

                            this.IncrementBar();

                            item.Nodes.Add(node);
                            Application.DoEvents();
                            if (!this.StilRunning)
                                break;
                        }

                        if (!this.StilRunning)
                            break;
                    }
                }
            }
            catch (Exception exc)
            {
                Logging.Info("Query Button Failed", exc);
                MessageBox.Show("Error Thrown:" + exc.Message);
            }

            this.progressBar1.Value = 0;
            this.QueryButton.Enabled = true;
            this.StopButton.Enabled = false;
        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            //
            // select * from win32_process where caption like 'k%'

            // work our way up the tree till the root
            if (sender != null)
            {
                var queryString = string.Empty;
                var n = ((TreeView)sender).SelectedNode;
                if (n != null)
                {
                    var nodeType = string.Empty;
                    if (n.Tag != null) nodeType = n.Tag.ToString();
                    var ntext = n.Text;
                    if (ntext.IndexOf("(") > 0) ntext = ntext.Substring(0, ntext.IndexOf("(") - 1).Trim();

                    switch (nodeType)
                    {
                        case "ClassNode":
                            queryString = "select * from " + ntext;
                            break;

                        case "PropertyNode":
                            var p = n.Parent;
                            var pnPText = p.Text;
                            if (pnPText.IndexOf("(") > 0)
                                pnPText = pnPText.Substring(0, pnPText.IndexOf("(") - 1).Trim();

                            queryString = "select " + ntext + " from " + pnPText;
                            break;

                        case "ValueNode":
                            var p1 = n.Parent;
                            var pp = p1.Parent;
                            var ppText = pp.Text;
                            var p1Text = p1.Text;
                            if (ppText.IndexOf("(") > 0)
                                ppText = ppText.Substring(0, ppText.IndexOf("(") - 1).Trim();

                            if (p1Text.IndexOf("(") > 0)
                                p1Text = p1Text.Substring(0, p1Text.IndexOf("(") - 1).Trim();

                            if (ntext.Substring(0, 5).ToLower() == "value")
                                queryString = "select * from " + ppText + " where " + p1Text + "=" +
                                              ntext.Replace("Value=", string.Empty);
                            else
                                queryString = "select " + p1Text + " from " + ppText; // + " where " + n.Text;

                            break;

                        case "RootNode":
                            if (this.treeView1.Nodes == null || this.treeView1.Nodes.Count == 0)
                                this.treeView1.Nodes.Add("Query");

                            var root = this.treeView1.Nodes[0];
                            queryString = root.Text;
                            break;

                        default:
                            if (nodeType != null && nodeType != string.Empty)
                                MessageBox.Show(nodeType);

                            break;
                    }

                    this.QueryTextBox.Text = queryString;

                    ////Clipboard.SetDataObject(queryString);
                    ////System.Windows.Forms.MessageBox.Show("\""+queryString + "\" was copied to the clipboard.");
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.StilRunning = false;
            this.QueryButton.Enabled = true;
            this.StopButton.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.QueryTextBox.Items.Clear();
            this.QueryTextBox.Text = "select * from CIM_System";
            this.LoadHistory(null);
            this.BasicTreemenuItem_Click(null, null);
        }

        private void Form1_Closing(object sender, CancelEventArgs e)
        {
            this.SaveHistory();
        }

        private void ExitmenuItem_Click(object sender, EventArgs e)
        {
            this.SaveHistory();
            Application.Exit();
        }

        private void SavemenuItem_Click(object sender, EventArgs e)
        {
            this.SaveHistory();
            MessageBox.Show("History Saved.");
        }

        private void ClearmenuItem_Click(object sender, EventArgs e)
        {
            this.QueryTextBox.Items.Clear();
            this.history = new ArrayList();
            File.Delete(this.HistoryPath);
            MessageBox.Show("History Cleared.");
        }

        private void LoadmenuItem_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.CheckFileExists = true;
            this.openFileDialog1.CheckPathExists = true;
            this.openFileDialog1.DefaultExt = "*.txt";
            this.openFileDialog1.InitialDirectory = Application.StartupPath;
            this.openFileDialog1.Multiselect = false;
            this.openFileDialog1.Title = "Locate History File...";
            var result = this.openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
                this.LoadHistory(this.openFileDialog1.FileName);
        }

        private void LoginMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new LoginForm();
            frm.UserName = this.Username;
            frm.Password = this.Password;
            frm.MachineName = this.Computer;

            frm.ShowDialog(this);
            if (!frm.Cancelled)
            {
                this.Username = frm.UserName;
                this.Password = frm.Password;
                this.Computer = frm.MachineName;
            }
        }

        private void LoadNode(XmlNode n, TreeNode tn)
        {
            if (n != null)
            {
                string nText = null;
                string nAlt = null;
                string nType = null;
                try
                {
                    nText = n.Attributes["Text"].Value;
                    this.IncrementBar();
                    Application.DoEvents();

                    try
                    {
                        if (n.Attributes["Alt"] != null)
                            nAlt = n.Attributes["Alt"].Value;
                    }
                    catch (Exception exc)
                    {
                        Logging.Error("Alt Attribute", exc);
                        nAlt = nText;
                    }

                    try
                    {
                        if (n.Attributes["Type"] != null)
                            nType = n.Attributes["Type"].Value;
                    }
                    catch (Exception exc)
                    {
                        Logging.Error("Type Attributes", exc);
                        nType = nAlt;
                    }

                    if (nAlt == string.Empty || nAlt == null)
                        tn.Text = nText;
                    else
                        tn.Text = nText + " (" + nAlt + ")";

                    tn.Tag = nType;
                    if (n.HasChildNodes)
                        foreach (XmlNode newXmlNode in n.ChildNodes)
                        {
                            var child = new TreeNode();
                            this.LoadNode(newXmlNode, child);
                            tn.Nodes.Add(child);
                        }
                }
                catch (Exception ee)
                {
                    Logging.Error("Load Node Failed", ee);
                }
            }
        }

        private void BasicTreemenuItem_Click(object sender, EventArgs e)
        {
            //string path = Application.StartupPath + "\\BasicTree.xml";
            //System.IO.StreamReader rdr = new System.IO.StreamReader(path);
            //string xml = rdr.ReadToEnd();
            //rdr.Close();
            this.LoadBasicTree(Resources.BasicTree);
        }

        private void LoadBasicTree(string xml)
        {
            this.progressBar1.Value = 0;

            this.IncrementBar();
            this.treeView2.Nodes.Clear();
            Application.DoEvents();
            this.IncrementBar();
            Application.DoEvents();

            this.IncrementBar();
            Application.DoEvents();
            var x = new XmlDocument();
            try
            {
                x.LoadXml(xml);
            }
            catch (Exception xexc)
            {
                Logging.Error("Load Basic Tree Failed", xexc);
            }

            var n = x.SelectSingleNode("/tree");
            var root = new TreeNode();
            this.treeView2.Nodes.Add(root);
            Application.DoEvents();

            this.LoadNode(n, root);

            root.Expand();
            if (root.Nodes != null && root.Nodes.Count > 0)
                root.Nodes[0].Expand();

            this.progressBar1.Value = 0;
        }

        private void treeView2_DoubleClick(object sender, EventArgs e)
        {
            if (this.treeView2.Nodes[0].Text == "To load-> Double Click")
            {
                this.BasicTreemenuItem_Click(null, null);
            }
            else
            {
                this.treeView1.Nodes.Clear();
                Application.DoEvents();
                this.treeView1_DoubleClick(this.treeView2, null);
                Application.DoEvents();
                this.QueryButton_Click(null, null);
                Application.DoEvents();
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*
                        if(tabControl1.SelectedTab.Name==processesTab.Name) {
                System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcesses();
                treeView3.Nodes.Clear();
                foreach(System.Diagnostics.Process p in procs) {
                    treeView3.Nodes.Add(new System.Windows.Forms.TreeNode(p.ProcessName));
                    Application.DoEvents();          
                }
            }
        */
        }

        private void treeView2_Click(object sender, EventArgs e)
        {
            //System.Windows.Forms.TreeNode rootNode = treeView2.SelectedNode;
            //if (rootNode != null)
            //{
            //    if (rootNode.Nodes == null || rootNode.Nodes.Count <= 0)
            //    {
            //        if (rootNode.Tag != null && Convert.ToString(rootNode.Tag) == "ClassNode")
            //        {
            //            string ntext = rootNode.Text;
            //            if (ntext.IndexOf("(") > 0) ntext = ntext.Substring(0, ntext.IndexOf("(") - 1).Trim();
            //            string qry = "select * from " + ntext;
            //            Application.DoEvents();
            //            System.Management.ManagementObjectSearcher searcher;
            //            System.Management.ObjectQuery query = new System.Management.ObjectQuery(qry);

            //            if (Username != "" && Password != "" && Computer != "")
            //            {
            //                System.Management.ConnectionOptions oConn = new System.Management.ConnectionOptions();
            //                oConn.Username = Username;
            //                oConn.Password = Password;
            //                System.Management.ManagementScope oMs = new System.Management.ManagementScope(Computer, oConn);
            //                searcher = new System.Management.ManagementObjectSearcher(oMs, query);
            //            }
            //            else
            //            {
            //                searcher = new System.Management.ManagementObjectSearcher(query);
            //            }
            //            System.Windows.Forms.TreeNode root = new TreeNode(qry);
            //            root.Tag = "RootNode";
            //            rootNode.Nodes.Add(root);
            //            root.Expand();
            //            rootNode.Expand();
            //            string path = "";
            //            Application.DoEvents();

            //            foreach (System.Management.ManagementObject share in searcher.Get())
            //            {
            //                System.Windows.Forms.TreeNode item = new TreeNode(share.ClassPath.ClassName);
            //                item.Tag = "ClassNode";
            //                root.Nodes.Add(item);
            //                path = "Enumerating:" + share.ClassPath.ClassName;
            //                foreach (System.Management.PropertyData p in share.Properties)
            //                {
            //                    bool isLocal = p.IsLocal;
            //                    string type = p.Type.ToString();
            //                    string origin = p.Origin;
            //                    string name = p.Name;
            //                    path = "Enumerating:" + share.ClassPath.ClassName + "," + name;
            //                    Application.DoEvents();

            //                    bool IsArray = p.IsArray;
            //                    string val = "NULL";
            //                    if (p.Value != null) val = p.Value.ToString();
            //                    System.Windows.Forms.TreeNode node = new TreeNode(name);
            //                    node.Tag = "PropertyNode";
            //                    string display = "";
            //                    if (type.ToLower() == "string")
            //                    {
            //                        display = "Value='" + val + "'";
            //                    }
            //                    else
            //                    {
            //                        display = "Value=" + val;
            //                    }
            //                    System.Windows.Forms.TreeNode ValueNode = new TreeNode(display);
            //                    ValueNode.Tag = "ValueNode";
            //                    System.Windows.Forms.TreeNode TypeNode = new TreeNode("Type='" + type + "'");
            //                    TypeNode.Tag = "ValueNode";
            //                    System.Windows.Forms.TreeNode localNode = new TreeNode("IsLocal=" + isLocal);
            //                    localNode.Tag = "ValueNode";
            //                    System.Windows.Forms.TreeNode OriginNode = new TreeNode("Origin='" + origin + "'");
            //                    OriginNode.Tag = "ValueNode";
            //                    System.Windows.Forms.TreeNode IsArrayNode = new TreeNode("IsArray=" + IsArray);
            //                    IsArrayNode.Tag = "ValueNode";

            //                    node.Nodes.Add(ValueNode);
            //                    node.Nodes.Add(TypeNode);
            //                    node.Nodes.Add(localNode);
            //                    node.Nodes.Add(OriginNode);
            //                    node.Nodes.Add(IsArrayNode);
            //                    Application.DoEvents();
            //                    if (IsArray && p.Value != null)
            //                    {
            //                        System.Array a = (System.Array)p.Value;
            //                        for (int x = 0; x < a.Length; x++)
            //                        {
            //                            string v = "";
            //                            if (a.GetValue(x) != null) v = a.GetValue(x).ToString();
            //                            System.Windows.Forms.TreeNode arrayNode = new TreeNode(name + "[" + x + "]=" + v);
            //                            arrayNode.Tag = "ArrayNode";
            //                            IsArrayNode.Nodes.Add(arrayNode);
            //                            IncrementBar();
            //                        }
            //                    }
            //                    IncrementBar();

            //                    item.Nodes.Add(node);
            //                    Application.DoEvents();
            //                    if (!this.StilRunning) break;

            //                }
            //                if (!this.StilRunning) break;
            //            }
            //        }
            //    }
            //}
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            this.LoginMenuItem_Click(null, null);
            this.ConnectionLabel.Text = this.Computer;
        }

        private void QueryTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                this.QueryButton_Click(null, null);
        }
    }
}