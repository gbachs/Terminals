using System.Configuration;

namespace Terminals
{
    /// <summary>
    ///     Collection of Windows Form states
    /// </summary>
    [ConfigurationCollection(typeof(FormStateConfigElement), CollectionType =
        ConfigurationElementCollectionType.AddRemoveClearMap)]
    public class FormsCollection : ConfigurationElementCollection
    {
        public FormStateConfigElement this[int index]
        {
            get => (FormStateConfigElement)this.BaseGet(index);
            set
            {
                if (this.BaseGet(index) != null)
                    this.BaseRemoveAt(index);
                this.BaseAdd(index, value);
            }
        }

        public new FormStateConfigElement this[string name] => (FormStateConfigElement)this.BaseGet(name);

        protected override ConfigurationElement CreateNewElement()
        {
            return new FormStateConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FormStateConfigElement)element).Name;
        }

        public void Add(FormStateConfigElement formState)
        {
            this.BaseAdd(formState, false);
        }
    }
}