using System.Configuration;

namespace Terminals
{
    public class GroupConfigurationElementCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType =>
            ConfigurationElementCollectionType.AddRemoveClearMap;

        public new string AddElementName { get => base.AddElementName; set => base.AddElementName = value; }

        public new string ClearElementName { get => base.ClearElementName; set => base.AddElementName = value; }

        public new string RemoveElementName => base.RemoveElementName;

        public new int Count => base.Count;

        public GroupConfigurationElement this[int index]
        {
            get => (GroupConfigurationElement)this.BaseGet(index);
            set
            {
                if (this.BaseGet(index) != null)
                    this.BaseRemoveAt(index);
                this.BaseAdd(index, value);
            }
        }

        public new GroupConfigurationElement this[string Name] => (GroupConfigurationElement)this.BaseGet(Name);

        protected override ConfigurationElement CreateNewElement()
        {
            return new GroupConfigurationElement();
        }

        protected override ConfigurationElement CreateNewElement(string elementName)
        {
            return new GroupConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((GroupConfigurationElement)element).Name;
        }

        public int IndexOf(GroupConfigurationElement item)
        {
            return this.BaseIndexOf(item);
        }

        public GroupConfigurationElement ItemByName(string name)
        {
            return (GroupConfigurationElement)this.BaseGet(name);
        }

        public void Add(GroupConfigurationElement item)
        {
            this.BaseAdd(item);
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            this.BaseAdd(element, false);
        }

        public void Remove(GroupConfigurationElement item)
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