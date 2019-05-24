using System.Collections.Generic;
using System.Xml.Linq;

namespace Terminals.Data.FilePersisted
{
    internal class UnknonwPluginElements
    {
        public UnknonwPluginElements()
            : this(new List<XElement>(), new Dictionary<string, List<XElement>>())
        {
        }

        public UnknonwPluginElements(List<XElement> favorites, Dictionary<string, List<XElement>> groupMembership)
        {
            this.Favorites = favorites;
            this.GroupMembership = groupMembership;
        }

        public List<XElement> Favorites { get; }

        public Dictionary<string, List<XElement>> GroupMembership { get; }
    }
}