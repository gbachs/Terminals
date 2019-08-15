using System;
using System.Drawing;

namespace TabControl
{
    public class TabControlItemClosedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the currently closed tab.
        /// </summary>
        public TabControlItem Item { get; set; }
    }

    #region TabControlItemClosingEventArgs

    public class TabControlItemClosingEventArgs : EventArgs
    {
        public TabControlItemClosingEventArgs(TabControlItem item)
        {
            _item = item;
        }

        private bool _cancel = false;
        private TabControlItem _item;

        public TabControlItem Item
        {
            get => _item;
            set => _item = value;
        }

        public bool Cancel
        {
            get => _cancel;
            set => _cancel = value;
        }
    }

    #endregion

    #region TabControlItemChangedEventArgs

    public class TabControlItemChangedEventArgs : EventArgs
    {
        TabControlItem itm;
        TabControlItemChangeTypes changeType;

        public TabControlItemChangedEventArgs(TabControlItem item, TabControlItemChangeTypes type)
        {
            changeType = type;
            itm = item;
        }

        public TabControlItemChangeTypes ChangeType => changeType;

        public TabControlItem Item => itm;
    }

    #endregion

    #region TabControlItemChangedEventArgs

    public class TabControlMouseOnTitleEventArgs : EventArgs
    {
        TabControlItem item;
        Point location;

        public TabControlMouseOnTitleEventArgs(TabControlItem item, Point location)
        {
            this.location = location;
            this.item = item;
        }

        public Point Location => location;

        public TabControlItem Item => item;
    }

    #endregion

    public delegate void TabControlItemChangedHandler(TabControlItemChangedEventArgs e);
    public delegate void TabControlItemClosingHandler(TabControlItemClosingEventArgs e);
    public delegate void TabControlItemClosedHandler(object sender, TabControlItemClosedEventArgs e);
    public delegate void TabControlMouseEnteredTitleHandler(TabControlMouseOnTitleEventArgs e);
    public delegate void TabControlMouseOnTitleHandler(TabControlMouseOnTitleEventArgs e);
    public delegate void TabControlMouseLeftTitleHandler(TabControlMouseOnTitleEventArgs e);

}
