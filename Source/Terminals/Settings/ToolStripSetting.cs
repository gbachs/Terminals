namespace Terminals
{
    /// <summary>
    ///     One tool strip configuration. Used to backup and restore the window layout.
    /// </summary>
    public class ToolStripSetting
    {
        public string Name { get; set; }

        public bool Visible { get; set; }

        public int Row { get; set; }

        public string Dock { get; set; }

        public int Left { get; set; }

        public int Top { get; set; }

        public override string ToString()
        {
            return string.Format("ToolStripSetting:Name={0},Visible={1},Row={2},Position=[{3},{4}],Dock={5}",
                this.Name, this.Visible, this.Row, this.Left, this.Top, this.Dock);
        }
    }
}