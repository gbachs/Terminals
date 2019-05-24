using System.Configuration;

namespace Terminals
{
    public class SpecialCommandConfigurationElementCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType =>
            ConfigurationElementCollectionType.AddRemoveClearMap;

        public new string AddElementName { get => base.AddElementName; set => base.AddElementName = value; }

        public new string ClearElementName { get => base.ClearElementName; set => base.AddElementName = value; }

        public new string RemoveElementName => base.RemoveElementName;

        public new int Count => base.Count;

        public SpecialCommandConfigurationElement this[int index]
        {
            get => (SpecialCommandConfigurationElement)this.BaseGet(index);
            set
            {
                if (this.BaseGet(index) != null)
                    this.BaseRemoveAt(index);
                this.BaseAdd(index, value);
            }
        }

        public new SpecialCommandConfigurationElement this[string Name] =>
            (SpecialCommandConfigurationElement)this.BaseGet(Name);

        protected override ConfigurationElement CreateNewElement()
        {
            return new SpecialCommandConfigurationElement();
        }

        protected override ConfigurationElement CreateNewElement(string elementName)
        {
            return new SpecialCommandConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SpecialCommandConfigurationElement)element).Name;
        }

        public int IndexOf(SpecialCommandConfigurationElement item)
        {
            return this.BaseIndexOf(item);
        }

        public SpecialCommandConfigurationElement ItemByName(string name)
        {
            return (SpecialCommandConfigurationElement)this.BaseGet(name);
        }

        public void Add(SpecialCommandConfigurationElement item)
        {
            this.BaseAdd(item);
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            this.BaseAdd(element, false);
        }

        public void Remove(SpecialCommandConfigurationElement item)
        {
            if (this.BaseIndexOf(item) >= 0)
                this.BaseRemove(item.Name);
        }

        public void RemoveAt(int index)
        {
            this.BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            this.BaseRemove(name);
        }

        public void Clear()
        {
            this.BaseClear();
        }
    }
}