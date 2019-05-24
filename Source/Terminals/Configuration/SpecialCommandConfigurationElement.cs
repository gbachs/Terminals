using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using Terminals.Properties;

namespace Terminals
{
    public class SpecialCommandConfigurationElement : ConfigurationElement
    {
        public SpecialCommandConfigurationElement()
        {
        }

        public SpecialCommandConfigurationElement(string name)
        {
            this.Name = name;
        }

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name { get => (string)this["name"]; set => this["name"] = value; }

        [ConfigurationProperty("executable", IsRequired = false)]
        public string Executable { get => (string)this["executable"]; set => this["executable"] = value; }

        [ConfigurationProperty("arguments", IsRequired = false)]
        public string Arguments { get => (string)this["arguments"]; set => this["arguments"] = value; }

        [ConfigurationProperty("workingFolder", IsRequired = false)]
        public string WorkingFolder { get => (string)this["workingFolder"]; set => this["workingFolder"] = value; }

        [ConfigurationProperty("thumbnail", IsRequired = false)]
        public string Thumbnail { get => (string)this["thumbnail"]; set => this["thumbnail"] = value; }

        public Image LoadThumbnail()
        {
            Image img = Resources.application_xp_terminal;
            try
            {
                if (!string.IsNullOrEmpty(this.Thumbnail))
                    if (File.Exists(this.Thumbnail))
                        img = Image.FromFile(this.Thumbnail);
            }
            catch (Exception exc)
            {
                Logging.Error("Could not LoadThumbnail for file:" + this.Thumbnail, exc);
            }

            return img;
        }
    }
}