using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Terminals
{
    public class FavoriteAliasConfigurationElementCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType =>
            ConfigurationElementCollectionType.AddRemoveClearMap;

        public new string AddElementName { get => base.AddElementName; set => base.AddElementName = value; }

        public new string ClearElementName { get => base.ClearElementName; set => base.AddElementName = value; }

        public new string RemoveElementName => base.RemoveElementName;

        public new int Count => base.Count;

        public FavoriteAliasConfigurationElement this[int index]
        {
            get => (FavoriteAliasConfigurationElement)this.BaseGet(index);
            set
            {
                if (this.BaseGet(index) != null)
                    this.BaseRemoveAt(index);
                this.BaseAdd(index, value);
            }
        }

        public new FavoriteAliasConfigurationElement this[string Name] =>
            (FavoriteAliasConfigurationElement)this.BaseGet(Name);

        protected override ConfigurationElement CreateNewElement()
        {
            return new FavoriteAliasConfigurationElement();
        }

        protected override ConfigurationElement CreateNewElement(string elementName)
        {
            return new FavoriteAliasConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FavoriteAliasConfigurationElement)element).Name;
        }

        public int IndexOf(FavoriteAliasConfigurationElement item)
        {
            return this.BaseIndexOf(item);
        }

        public FavoriteAliasConfigurationElement ItemByName(string name)
        {
            return (FavoriteAliasConfigurationElement)this.BaseGet(name);
        }

        public void Add(FavoriteAliasConfigurationElement item)
        {
            this.BaseAdd(item);
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            this.BaseAdd(element, false);
        }

        public void Remove(FavoriteAliasConfigurationElement item)
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

        internal List<string> GetFavoriteNames()
        {
            return this.Cast<FavoriteAliasConfigurationElement>()
                .Select(favoriteAlias => favoriteAlias.Name)
                .ToList();
        }
    }
}