using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace TabControl
{
    [ToolboxItem(false)]
    public class BaseStyledPanel : Panel
    {
        private static readonly ToolStripProfessionalRenderer renderer;

        public event EventHandler ThemeChanged;

        static BaseStyledPanel()
        {
            renderer = new ToolStripProfessionalRenderer();
        }

        protected BaseStyledPanel()
        {
            // Set painting style for better performance.
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnSystemColorsChanged(EventArgs e)
        {
            base.OnSystemColorsChanged(e);
            UpdateRenderer();
            Invalidate();
        }

        protected virtual void OnThemeChanged(EventArgs e)
        {
            this.ThemeChanged?.Invoke(this, e);
        }

        private void UpdateRenderer()
        {
            if (!UseThemes)
            {
                renderer.ColorTable.UseSystemColors = true;
            }
            else
            {
                renderer.ColorTable.UseSystemColors = false;
            }
        }

        [Browsable(false)] public ToolStripProfessionalRenderer ToolStripRenderer => renderer;

        [DefaultValue(true)]
        [Browsable(false)]
        public bool UseThemes =>
            VisualStyleRenderer.IsSupported && VisualStyleInformation.IsSupportedByOS &&
            Application.RenderWithVisualStyles;
    }
}