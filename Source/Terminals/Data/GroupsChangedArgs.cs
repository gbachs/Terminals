using System;
using System.Collections.Generic;

namespace Terminals.Data
{
    /// <summary>
    ///     Tags changed event arguments container, informing about changes in Tags collection
    /// </summary>
    internal class GroupsChangedArgs : EventArgs
    {
        internal GroupsChangedArgs()
        {
            this.Added = new List<IGroup>();
            this.Updated = new List<IGroup>();
            this.Removed = new List<IGroup>();
        }

        internal GroupsChangedArgs(List<IGroup> addedGroups, List<IGroup> deletedGroups)
            : this()
        {
            // merge collections to report only differences
            MergeChangeLists(addedGroups, deletedGroups);
            this.Added.AddRange(addedGroups);
            this.Removed.AddRange(deletedGroups);
        }

        /// <summary>
        ///     Newly added IGroups, currently used at least by one connection
        /// </summary>
        internal List<IGroup> Added { get; }

        internal List<IGroup> Updated { get; }

        /// <summary>
        ///     All IGroups actually no longer used by any favorite
        /// </summary>
        internal List<IGroup> Removed { get; }

        /// <summary>
        ///     Gets the value indicating if there are any added or removed items to report.
        /// </summary>
        internal bool IsEmpty =>
            this.Added.Count == 0 &&
            this.Removed.Count == 0 &&
            this.Updated.Count == 0;

        private static void MergeChangeLists(List<IGroup> addedGroups, List<IGroup> deletedGroups)
        {
            var index = 0;
            while (index < deletedGroups.Count)
            {
                var deletedIGroup = deletedGroups[index];
                if (addedGroups.Contains(deletedIGroup))
                {
                    addedGroups.Remove(deletedIGroup);
                    deletedGroups.Remove(deletedIGroup);
                }
                else
                {
                    index++;
                }
            }
        }

        internal void AddFrom(GroupsChangedArgs source)
        {
            var toAdd = ListsHelper.GetMissingSourcesInTarget(source.Added, this.Added);
            this.Added.AddRange(toAdd);
            var toUpdate = ListsHelper.GetMissingSourcesInTarget(source.Updated, this.Updated);
            this.Updated.AddRange(toUpdate);
            var toRemove = ListsHelper.GetMissingSourcesInTarget(source.Removed, this.Removed);
            this.Removed.AddRange(toRemove);
        }

        public override string ToString()
        {
            return string.Format("GroupsChangedArgs:Added={0};Updated {1};Removed={2}",
                this.Added.Count, this.Updated.Count, this.Removed.Count);
        }
    }
}