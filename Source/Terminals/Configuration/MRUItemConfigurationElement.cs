using System.Configuration;

namespace Terminals
{
    public class MRUItemConfigurationElement : ConfigurationElement
    {
        public MRUItemConfigurationElement()
        {
        }

        public MRUItemConfigurationElement(string name)
        {
            this.Name = name;
        }

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name { get => (string)this["name"]; set => this["name"] = value; }
    }
}