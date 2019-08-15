using System.Windows.Forms;

namespace Terminals.Forms.Controls
{
    /// <summary>
    /// Encaupsulation of Windows Forms Control.ModifierKeys static members.
    /// </summary>
    internal class KeyModifiers : IKeyModifiers
    {
        public bool WithShift => Control.ModifierKeys.HasFlag(Keys.Shift);

        public bool WithControl => Control.ModifierKeys.HasFlag(Keys.Control);
    }
}