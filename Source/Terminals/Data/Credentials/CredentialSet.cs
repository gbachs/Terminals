using System;
using System.Xml.Serialization;

namespace Terminals.Data
{
    /// <summary>
    ///     Container of stored user authentication.
    /// </summary>
    [Serializable]
    public class CredentialSet : CredentialBase, ICredentialSet
    {
        private Guid id = Guid.NewGuid();

        private string name;

        [XmlAttribute("id")] public Guid Id { get => this.id; set => this.id = value; }

        public string Name
        {
            get => this.name;
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                this.name = value;
            }
        }

        public override string ToString()
        {
            return string.Format(@"{0}:{1}\{2}", this.Name, "", "");
        }
    }
}