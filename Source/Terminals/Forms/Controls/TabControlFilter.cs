using System.Collections.Generic;
using System.Linq;
using TabControl;
using Terminals.Connections;
using Terminals.Data;

namespace Terminals
{
    internal class TabControlFilter
    {
        internal bool HasSelected => this.tabControl.SelectedItem != null;

        internal IFavorite SelectedOriginFavorite
        {
            get
            {
                if (this.HasSelected)
                    return this.Selected.OriginFavorite;

                return null;
            }
        }

        internal IFavorite SelectedFavorite
        {
            get
            {
                if (this.HasSelected)
                    return this.Selected.Favorite;

                return null;
            }
        }

        internal IConnection SelectedConnection
        {
            get
            {
                if (this.HasSelected)
                    return this.Selected.Connection;

                return null;
            }
        }

        internal TerminalTabControlItem Selected => this.tabControl.SelectedItem as TerminalTabControlItem;

        private readonly TabControl.TabControl tabControl;

        public TabControlFilter(TabControl.TabControl tabControl)
        {
            this.tabControl = tabControl;
        }

        internal TabControlItem FindAttachedTab(IFavorite updated)
        {
            return this.tabControl.Items.Cast<TerminalTabControlItem>()
                .FirstOrDefault(tab => tab.OriginFavorite != null && tab.OriginFavorite.StoreIdEquals(updated));
        }

        internal IEnumerable<IFavorite> SelectTabsWithFavorite()
        {
            return this.tabControl.Items.OfType<TerminalTabControlItem>()
                .Where(ti => ti.OriginFavorite != null)
                .Select(ti => ti.OriginFavorite);
        }

        internal IFavorite FindFavoriteByTabTitle(string tabTitle)
        {
            var tab = this.tabControl.Items.OfType<TerminalTabControlItem>()
                .FirstOrDefault(candidate => candidate.Title == tabTitle);

            return tab?.Favorite;
        }

        internal TerminalTabControlItem FindCaptureManagerTab()
        {
            var captureManagerTitle = Program.Resources.GetString("CaptureManager");

            return this.tabControl.Items
                .OfType<TerminalTabControlItem>()
                .FirstOrDefault(ti => ti.Title == captureManagerTitle);
        }

        internal TerminalTabControlItem FindTabToClose(Connection connection)
        {
            return this.tabControl.Items
                .OfType<TerminalTabControlItem>()
                .FirstOrDefault(tab => tab.Connection == connection);
        }
    }
}