using System;
using System.Configuration;

namespace Terminals
{
    public class FavoriteConfigurationElementCollection : ConfigurationElementCollection
    {
        public FavoriteConfigurationElementCollection()
            : base(StringComparer.CurrentCultureIgnoreCase)
        {
        }

        public override ConfigurationElementCollectionType CollectionType =>
            ConfigurationElementCollectionType.AddRemoveClearMap;

        public FavoriteConfigurationElement this[int index]
        {
            get => (FavoriteConfigurationElement)this.BaseGet(index);
            set
            {
                if (this.BaseGet(index) != null)
                    this.BaseRemoveAt(index);
                this.BaseAdd(index, value);
            }
        }

        public new FavoriteConfigurationElement this[string name]
        {
            get => (FavoriteConfigurationElement)this.BaseGet(name);
            set
            {
                if (this.BaseGet(name) != null)
                    this.BaseRemove(name);
                this.BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new FavoriteConfigurationElement();
        }

        protected override ConfigurationElement CreateNewElement(string elementName)
        {
            return new FavoriteConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FavoriteConfigurationElement)element).Name;
        }

        public int IndexOf(FavoriteConfigurationElement item)
        {
            return this.BaseIndexOf(item);
        }

        public void Add(FavoriteConfigurationElement item)
        {
            this.BaseAdd(item);
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            this.BaseAdd(element, false);
        }

        public void Remove(FavoriteConfigurationElement item)
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

        internal SortableList<FavoriteConfigurationElement> ToList()
        {
            var favorites = new SortableList<FavoriteConfigurationElement>();
            foreach (FavoriteConfigurationElement favorite in this)
                favorites.Add(favorite);

            return favorites;
        }
    }
}