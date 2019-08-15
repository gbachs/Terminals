using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Terminals.Configuration;
using Terminals.Forms;

namespace Terminals.CaptureManager
{
    internal partial class CaptureManagerLayout : UserControl
    {
        public const string ControlName = "layout1";
        private TreeNode root = new TreeNode("Capture Root Folder");
        public CaptureManagerLayout()
        {
            this.InitializeComponent();
        }
        
        private void CaptureManagerLayout_Load(object sender, EventArgs e)
        {
            this.LoadRoot();
            this.viewComboBox.SelectedIndex = 0;
        }

 
        public void RefreshView()
        {
            if (this.treeViewFolders.SelectedNode != null)
            {
                var folder = (this.treeViewFolders.SelectedNode.Tag as DirectoryInfo);
                this.LoadFolder(folder.FullName, this.treeViewFolders.SelectedNode);
            }
        }

        private void LoadRoot()
        {
            CaptureManager.EnsureRoot();
            this.root.Tag = new DirectoryInfo(CaptureManager.CaptureRoot);
            AssignImageIndexes(this.root);
            
            this.treeViewFolders.Nodes.Add(this.root);
            this.treeViewFolders.SelectedNode = this.root;
            
            this.LoadFolder(CaptureManager.CaptureRoot, this.root);
            this.root.Expand();
        }

        private static void AssignImageIndexes(TreeNode treeNodeToConfigure)
        {
            treeNodeToConfigure.ImageIndex = 0;
            treeNodeToConfigure.SelectedImageIndex = 1;           
        }

        private void LoadFolder(string Path, TreeNode Parent)
        {
            this.listViewFiles.Items.Clear();
            Parent.Nodes.Clear();
            var directories = CaptureManager.LoadCaptureFolder(Path);
            foreach (var folder in directories)
            {
                AddNewDirectoryTreeNode(Parent, folder);
            }

            var c = CaptureManager.LoadCaptures(Path);
            foreach (var cap in c)
            {
                AddNewCaptureListViewItem(cap);
            }
        }

        private static void AddNewDirectoryTreeNode(TreeNode Parent, DirectoryInfo folder)
        {
            var child = new TreeNode(folder.Name);
            AssignImageIndexes(child);
            child.Tag = folder;
            Parent.Nodes.Add(child);
        }

        private void AddNewCaptureListViewItem(Capture cap)
        {
            var item = new ListViewItem();
            item.Tag = cap;
            item.Text = cap.Name;
            item.ToolTipText = cap.FilePath;
            var index = this.imageList1.Images.Add(cap.Image, Color.HotPink);
            item.ImageIndex = index;
            this.listViewFiles.Items.Add(item);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.pictureBox1.Image = null;
            this.pictureCommentsTextBox.Text = string.Empty;
            this.saveButton.Enabled = false;
            this.deleteButton.Enabled = false;

            var dir = (e.Node.Tag as DirectoryInfo);
            this.LoadFolder(dir.FullName, e.Node);
        }

        private void newFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.treeViewFolders.SelectedNode != null)
            {
                var dir = (this.treeViewFolders.SelectedNode.Tag as DirectoryInfo);
                var result = InputBox.Show("Enter folder Name:", "Create new folder");
                if (result.ReturnCode == DialogResult.OK)
                {
                    var rootFolder = dir.FullName;
                    var fullNewName = Path.Combine(rootFolder, result.Text);
                    if (!Directory.Exists(fullNewName))
                    {
                        var info = Directory.CreateDirectory(fullNewName);
                        var node = new TreeNode(result.Text);
                        node.Tag = info;
                        this.treeViewFolders.SelectedNode.Nodes.Add(node);
                        this.treeViewFolders.SelectedNode.Expand();
                    }
                }
            }
        }

        private void deleteFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.treeViewFolders.SelectedNode != null && this.treeViewFolders.SelectedNode != this.root)
            {
                var dir = (this.treeViewFolders.SelectedNode.Tag as DirectoryInfo);
                if (Directory.Exists(dir.FullName))
                {
                    var files = dir.GetFiles();
                    var dirs = dir.GetDirectories();
                    var msg = string.Format("{0}\r\n\r\n", Program.Resources.GetString("ConfirmDeleteSingleFolder"));
                    if (files.Length > 0)
                    {
                        msg += string.Format("The folder \"{0}\" contains {1} files.", this.treeViewFolders.SelectedNode.Text, files.Length);
                    }

                    if (dirs.Length > 0)
                    {
                        msg += string.Format("The folder \"{0}\" contains {1} directories.", this.treeViewFolders.SelectedNode.Text, dirs.Length);
                    }

                    var result = MessageBox.Show(msg, Program.Resources.GetString("ConfirmCaptionDeleteSingleFolder"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (result == DialogResult.OK)
                    {
                        var rootFolder = dir.FullName;
                        Directory.Delete(rootFolder, true);
                        this.treeViewFolders.SelectedNode.Remove();
                    }
                }
                else
                {
                    this.treeViewFolders.SelectedNode.Remove();
                }
            }
        }

        private void listView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
        }

        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            ListViewItem NewNode;

            if (e.Data.GetDataPresent("System.Windows.Forms.ListViewItem", false))
            {
                var pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
                var DestinationNode = ((TreeView)sender).GetNodeAt(pt);
                NewNode = (ListViewItem)e.Data.GetData("System.Windows.Forms.ListViewItem");
                var c = (NewNode.Tag as Capture);

                if (DestinationNode != null)
                {
                    var destInfo = (DestinationNode.Tag as DirectoryInfo);
                    var dest = Path.Combine(destInfo.FullName, Path.GetFileName(c.FilePath));
                    c.Move(dest);

                    this.treeView1_AfterSelect(null, new TreeViewEventArgs(this.treeViewFolders.SelectedNode));
                }
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.listViewFiles.SelectedItems != null && this.listViewFiles.SelectedItems.Count > 0)
            {
                foreach (ListViewItem lvi in this.listViewFiles.SelectedItems)
                {
                    var cap = (lvi.Tag as Capture);
                    cap.Show();
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.pictureBox1.Image = null;
            this.pictureCommentsTextBox.Text = string.Empty;
            this.saveButton.Enabled = false;
            this.deleteButton.Enabled = false;
            if (this.listViewFiles.SelectedItems != null && this.listViewFiles.SelectedItems.Count > 0)
            {
                var cap = (this.listViewFiles.SelectedItems[0].Tag as Capture);
                this.pictureBox1.Image = cap.Image;
                this.pictureCommentsTextBox.Text = cap.Comments;
                this.saveButton.Enabled = true;
                this.deleteButton.Enabled = true;
            }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && this.listViewFiles.SelectedItems != null && this.listViewFiles.SelectedItems.Count > 0)
            {
                this.listViewFiles.ContextMenuStrip.Show();
            }
        }

        /// <summary>
        /// Delete selected listview item on delete key press.
        /// </summary>
        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                this.deleteFileToolStripMenuItem_Click(null, null);
            }
        }

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && this.treeViewFolders.SelectedNode != null)
            {
                this.treeViewFolders.SelectedNode = this.treeViewFolders.HitTest(e.Location).Node;
            }
        }

        private void treeView1_DragOver(object sender, DragEventArgs e)
        {
            var pos = this.treeViewFolders.PointToClient(new Point(e.X, e.Y));
            var hit = this.treeViewFolders.HitTest(pos);

            if (hit.Node != null)
            {
                hit.Node.Expand();
                this.treeViewFolders.SelectedNode = hit.Node;
                e.Effect = DragDropEffects.Move;
            }
        }

        private void treeView1_KeyDown(object sender, KeyEventArgs e)
        {
            this.deleteFolderToolStripMenuItem_Click(null, null);
        }

        private void deleteFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listViewFiles.SelectedItems != null && this.listViewFiles.SelectedItems.Count > 0)
            {
                var cnt = this.listViewFiles.SelectedItems.Count;
                var msg = string.Empty;
                var cpt = string.Empty;

                if (this.listViewFiles.SelectedItems.Count == 1)
                {
                    msg = Program.Resources.GetString("ConfirmDeleteSingleFile");
                    cpt = Program.Resources.GetString("ConfirmCaptionDeleteSingleItem");
                }
                else
                {
                    msg = string.Format(Program.Resources.GetString("ConfirmDeleteMultipleFiles"), cnt);
                    cpt = Program.Resources.GetString("ConfirmCaptionDeleteMultipleItems");
                }

                if (MessageBox.Show(msg, cpt, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    foreach (ListViewItem lvi in this.listViewFiles.SelectedItems)
                    {
                        var cap = (lvi.Tag as Capture);
                        cap.Delete();
                        this.listViewFiles.Items.Remove(lvi);
                    }
                }
            }
        }

        private void copyImageToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listViewFiles.SelectedItems != null && this.listViewFiles.SelectedItems.Count > 0)
            {
                var cap = (this.listViewFiles.SelectedItems[0].Tag as Capture);
                Clipboard.SetImage(cap.Image);
            }
        }

        private void copyImagePathToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listViewFiles.SelectedItems != null && this.listViewFiles.SelectedItems.Count > 0)
            {
                var cap = (this.listViewFiles.SelectedItems[0].Tag as Capture);
                Clipboard.SetText(cap.FilePath);
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            this.deleteFileToolStripMenuItem_Click(null, null);
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (this.listViewFiles.SelectedItems != null && this.listViewFiles.SelectedItems.Count > 0)
            {
                var cap = (this.listViewFiles.SelectedItems[0].Tag as Capture);
                cap.Comments = this.pictureCommentsTextBox.Text;
                cap.Save();
            }
        }

        private void View_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.viewComboBox.Text)
            {
                case "Large Icons":
                    this.imageList1.ImageSize = new Size(150, 150);
                    this.listViewFiles.View = View.LargeIcon;
                    break;

                case "Small Icons":
                    this.imageList1.ImageSize = new Size(50, 50);
                    this.listViewFiles.View = View.LargeIcon;
                    break;

                case "List":
                    this.imageList1.ImageSize = new Size(150, 150);
                    this.listViewFiles.View = View.List;
                    break;

                case "Tile":
                    this.imageList1.ImageSize = new Size(150, 150);
                    this.listViewFiles.View = View.Tile;
                    break;

                default:
                    break;
            }

            this.RefreshView();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            this.imageList1.ImageSize = new Size(this.trackBarZoom.Value, this.trackBarZoom.Value);
            this.listViewFiles.View = View.LargeIcon;
            this.RefreshView();
        }
    }
}