using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Terminals
{
    public class MRUItemConfigurationElementCollection : ConfigurationElementCollection
    {
        public MRUItemConfigurationElementCollection()
        {
        }

        internal MRUItemConfigurationElementCollection(IEnumerable<string> values)
        {
            foreach (var newItem in values)
                this.AddByName(newItem);
        }

        public override ConfigurationElementCollectionType CollectionType =>
            ConfigurationElementCollectionType.AddRemoveClearMap;

        public new string AddElementName { get => base.AddElementName; set => base.AddElementName = value; }

        public new string ClearElementName { get => base.ClearElementName; set => base.AddElementName = value; }

        public new string RemoveElementName => base.RemoveElementName;

        public new int Count => base.Count;

        public MRUItemConfigurationElement this[int index]
        {
            get => (MRUItemConfigurationElement)this.BaseGet(index);
            set
            {
                if (this.BaseGet(index) != null)
                    this.BaseRemoveAt(index);
                this.BaseAdd(index, value);
            }
        }

        public new MRUItemConfigurationElement this[string Name] => (MRUItemConfigurationElement)this.BaseGet(Name);

        protected override ConfigurationElement CreateNewElement()
        {
            return new MRUItemConfigurationElement();
        }

        protected override ConfigurationElement CreateNewElement(string elementName)
        {
            return new MRUItemConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MRUItemConfigurationElement)element).Name;
        }

        public int IndexOf(MRUItemConfigurationElement item)
        {
            return this.BaseIndexOf(item);
        }

        public MRUItemConfigurationElement ItemByName(string name)
        {
            return (MRUItemConfigurationElement)this.BaseGet(name);
        }

        public void Add(MRUItemConfigurationElement item)
        {
            this.BaseAdd(item);
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            this.BaseAdd(element, false);
        }

        public void Remove(MRUItemConfigurationElement item)
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

        internal List<string> ToList()
        {
            return this.Cast<MRUItemConfigurationElement>()
                .Select(configurationElement => configurationElement.Name)
                .ToList();
        }

        internal string[] ToSortedArray()
        {
            var domainNames = this.ToList();
            domainNames.Sort();
            return domainNames.ToArray();
        }

        internal void AddByName(string name)
        {
            var configurationElement = this.ItemByName(name);
            if (configurationElement == null)
                this.Add(new MRUItemConfigurationElement(name));
        }

        internal void DeleteByName(string name)
        {
            var configurationElement = this.ItemByName(name);
            if (configurationElement != null)
                this.Remove(name);
        }

        internal void EditByName(string oldName, string newName)
        {
            var configurationElement = this.ItemByName(oldName);
            if (configurationElement != null)
                this[oldName].Name = newName;
        }
    }
}