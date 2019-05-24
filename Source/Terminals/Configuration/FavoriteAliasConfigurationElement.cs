using System.Configuration;

namespace Terminals
{
    public class FavoriteAliasConfigurationElement : ConfigurationElement
    {
        public FavoriteAliasConfigurationElement()
        {
        }

        public FavoriteAliasConfigurationElement(string name)
        {
            this.Name = name;
        }

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name { get => (string)this["name"]; set => this["name"] = value; }
    }
}