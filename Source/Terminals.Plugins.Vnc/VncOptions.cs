using System;

namespace Terminals.Data
{
    [Serializable]
    internal class VncOptions : ProtocolOptions
    {
        public bool AutoScale { get; set; }
        public bool ViewOnly { get; set; }
        public int DisplayNumber { get; set; }

        public override ProtocolOptions Copy()
        {
            return new VncOptions
                {
                    AutoScale = this.AutoScale,
                    ViewOnly = this.ViewOnly,
                    DisplayNumber = this.DisplayNumber
                };
        }
    }
}
