namespace Terminals.Data.DB
{
    internal partial class DbDisplayOptions : IDisplayOptions
    {
        int IDisplayOptions.Height
        {
            get => this.Height == null ? 0 : (int)this.Height;
            set => this.Height = value == 0 ? (int?)null : value;
        }

        int IDisplayOptions.Width
        {
            get => this.Width == null ? 0 : (int)this.Width;
            set => this.Width = value == 0 ? (int?)null : value;
        }

        public DesktopSize DesktopSize
        {
            get => this.Size == null ? DesktopSize.FitToWindow : (DesktopSize)this.Size;
            set => this.Size = value == DesktopSize.FitToWindow ? null : (byte?)value;
        }

        Colors IDisplayOptions.Colors
        {
            get => this.Colors == null ? Terminals.Colors.Bits32 : (Colors)this.Colors;
            set => this.Colors = value == Terminals.Colors.Bits32 ? null : (byte?)value;
        }

        internal void UpdateFrom(DbDisplayOptions source)
        {
            this.Height = source.Height;
            this.Width = source.Width;
            this.DesktopSize = source.DesktopSize;
            this.Colors = source.Colors;
        }
    }
}