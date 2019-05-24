using System;

namespace Terminals.Data
{
    [Serializable]
    public class DisplayOptions : IDisplayOptions
    {
        private Colors colors = Colors.Bits32;

        private DesktopSize desktopSize = DesktopSize.FitToWindow;

        public int Height { get; set; }

        public int Width { get; set; }

        public DesktopSize DesktopSize { get => this.desktopSize; set => this.desktopSize = value; }

        public Colors Colors { get => this.colors; set => this.colors = value; }

        internal DisplayOptions Copy()
        {
            return new DisplayOptions
            {
                Height = this.Height,
                Width = this.Width,
                DesktopSize = this.DesktopSize,
                Colors = this.Colors
            };
        }
    }
}