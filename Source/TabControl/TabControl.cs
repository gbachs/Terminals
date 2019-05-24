using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TabControl
{
    /// <summary>
    ///     http://www.codeproject.com/Articles/13902/TabStrips-A-TabControl-in-the-Visual-Studio-2005-w
    /// </summary>
    [Designer(typeof(TabControlDesigner))]
    [DefaultEvent("TabControlItemSelectionChanged")]
    [DefaultProperty("Items")]
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(TabControl))]
    public class TabControl : BaseStyledPanel, ISupportInitialize
    {
        #region Ctor

        public TabControl()
        {
            this.BeginInit();

            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.Selectable, true);

            this.Items = new TabControlItemCollection(this);
            this.Items.CollectionChanged += this.OnCollectionChanged;
            base.Size = new Size(350, 200);

            this.Menu = new ContextMenuStrip();
            this.Menu.Renderer = this.ToolStripRenderer;
            this.Menu.ItemClicked += this.OnMenuItemClicked;
            this.Menu.VisibleChanged += this.OnMenuVisibleChanged;

            this.menuGlyph = new TabControlMenuGlyph(this.ToolStripRenderer);
            this.closeButton = new TabControlCloseButton(this.ToolStripRenderer);
            this.Font = new Font("Tahoma", 8.25f, FontStyle.Regular);
            this.sf = new StringFormat();
            this.movePreview = new TabPreview(this);

            this.EndInit();

            this.UpdateLayout();
        }

        #endregion

        private static int CompareSortText(ToolStripMenuItem x, ToolStripMenuItem y)
        {
            return x.Text.CompareTo(y.Text);
        }

        #region Static Fields

        internal static int PreferredWidth = 350;

        internal static int PreferredHeight = 200;

        #endregion

        #region Constants

        private const int DEF_HEADER_HEIGHT = 19;

        //private int DEF_GLYPH_INDENT = 10;
        private int DEF_START_POS = 10;

        private const int DEF_GLYPH_WIDTH = 40;

        #endregion

        #region Fields

        private Rectangle stripButtonRect = Rectangle.Empty;

        private TabControlItem selectedItem;

        private readonly TabControlMenuGlyph menuGlyph;

        private readonly TabControlCloseButton closeButton;

        private readonly StringFormat sf;

        private TabControlItem tabAtMouseDown;

        private bool mouseEnteredTitle;

        private bool alwaysShowClose = true;

        private bool isIniting;

        private bool alwaysShowMenuGlyph = true;

        private bool showTabs = true;

        private bool showBorder = true;

        public event TabControlItemClosingHandler TabControlItemClosing;

        public event TabControlItemChangedHandler TabControlItemSelectionChanged;

        public event TabControlMouseOnTitleHandler TabControlMouseOnTitle;

        public event TabControlMouseLeftTitleHandler TabControlMouseLeftTitle;

        public event TabControlMouseEnteredTitleHandler TabControlMouseEnteredTitle;

        public event HandledEventHandler MenuItemsLoading;

        public event EventHandler MenuItemsLoaded;

        public event TabControlItemClosedHandler TabControlItemClosed;

        public event TabControlItemChangedHandler TabControlItemDetach;

        #endregion

        #region Methods

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            this.UpdateLayout();
            this.Invalidate();
        }

        private bool AllowDraw(TabControlItem item)
        {
            var result = true;

            if (this.RightToLeft == RightToLeft.No)
            {
                if (item.StripRect.Right >= this.stripButtonRect.Width)
                    result = false;
            }
            else
            {
                if (item.StripRect.Left <= this.stripButtonRect.Left)
                    return false;
            }

            return result;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            this.SetDefaultSelected();
            if (this.showBorder)
            {
                var borderRc = this.ClientRectangle;
                borderRc.Width--;
                borderRc.Height--;
                e.Graphics.DrawRectangle(SystemPens.ControlDark, borderRc);
            }

            if (this.RightToLeft == RightToLeft.No)
                this.DEF_START_POS = 10;
            else
                this.DEF_START_POS = this.stripButtonRect.Right;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF selectedButton = Rectangle.Empty;

            #region Draw Pages

            if (this.showTabs)
            {
                var selectedTabItem = this.SelectedItem;

                // todo bug 32353 - first calculate all, then identify ragne of tabs to draw, and finally paint them 
                for (var i = 0; i < this.Items.Count; i++)
                {
                    var currentItem = this.Items[i];
                    if (!currentItem.Visible && !this.DesignMode)
                        continue;

                    this.OnCalcTabPage(e.Graphics, currentItem);
                    currentItem.IsDrawn = false;

                    if (currentItem == selectedTabItem) //delay drawing active item to the end
                        continue;

                    if (!this.AllowDraw(currentItem))
                        continue;

                    this.OnDrawTabPage(e.Graphics, currentItem);
                }

                if (selectedTabItem != null && this.AllowDraw(selectedTabItem))
                    try
                    {
                        this.OnDrawTabPage(e.Graphics, selectedTabItem);
                    }
                    catch (Exception)
                    {
                        //black hole this for now
                    }
            }

            #endregion

            #region Draw UnderPage Line

            if (this.showTabs && this.showBorder)
                using (var pen = new Pen(this.ToolStripRenderer.ColorTable.MenuStripGradientBegin))
                {
                    if (this.RightToLeft == RightToLeft.No)
                    {
                        if (this.Items.DrawnCount == 0 || this.Items.VisibleCount == 0)
                        {
                            e.Graphics.DrawLine(pen, new Point(0, DEF_HEADER_HEIGHT),
                                new Point(this.ClientRectangle.Width, DEF_HEADER_HEIGHT));
                        }
                        else if (this.SelectedItem != null && this.SelectedItem.IsDrawn)
                        {
                            var end = new Point((int)this.SelectedItem.StripRect.Left - 9, DEF_HEADER_HEIGHT);
                            e.Graphics.DrawLine(pen, new Point(0, DEF_HEADER_HEIGHT), end);
                            end.X += (int)this.SelectedItem.StripRect.Width + 10;
                            e.Graphics.DrawLine(pen, end, new Point(this.ClientRectangle.Width, DEF_HEADER_HEIGHT));
                        }
                    }
                    else
                    {
                        if (this.Items.DrawnCount == 0 || this.Items.VisibleCount == 0)
                        {
                            e.Graphics.DrawLine(SystemPens.ControlDark, new Point(0, DEF_HEADER_HEIGHT),
                                new Point(this.ClientRectangle.Width, DEF_HEADER_HEIGHT));
                        }
                        else if (this.SelectedItem != null && this.SelectedItem.IsDrawn)
                        {
                            var end = new Point((int)this.SelectedItem.StripRect.Left, DEF_HEADER_HEIGHT);
                            e.Graphics.DrawLine(pen, new Point(0, DEF_HEADER_HEIGHT), end);
                            end.X += (int)this.SelectedItem.StripRect.Width + 20;
                            e.Graphics.DrawLine(pen, end, new Point(this.ClientRectangle.Width, DEF_HEADER_HEIGHT));
                        }
                    }
                }

            #endregion

            #region Draw Menu and Close Glyphs

            if (this.showTabs)
            {
                if (this.AlwaysShowMenuGlyph && this.Items.VisibleCount > 0 ||
                    this.Items.DrawnCount > this.Items.VisibleCount)
                    this.menuGlyph.DrawGlyph(e.Graphics);

                if (this.AlwaysShowClose || this.SelectedItem != null && this.SelectedItem.CanClose)
                    this.closeButton.DrawCross(e.Graphics);
            }

            #endregion
        }

        public void AddTab(TabControlItem tabItem)
        {
            this.Items.Add(tabItem);
            tabItem.Dock = DockStyle.Fill;
        }

        public void RemoveTab(TabControlItem tabItem)
        {
            var tabIndex = this.Items.IndexOf(tabItem);
            var wasSelected = tabItem.Selected;

            if (tabIndex >= 0)
            {
                this.UnSelectItem(tabItem);
                this.Items.Remove(tabItem);
            }

            if (wasSelected)
            {
                if (this.Items.Count > 0)
                {
                    if (this.RightToLeft == RightToLeft.No)
                    {
                        if (this.Items[tabIndex - 1] != null)
                            this.SelectedItem = this.Items[tabIndex - 1];
                        else
                            this.SelectedItem = this.Items.FirstVisible;
                    }
                    else
                    {
                        if (this.Items[tabIndex + 1] != null)
                            this.SelectedItem = this.Items[tabIndex + 1];
                        else
                            this.SelectedItem = this.Items.LastVisible;
                    }
                }
                else
                {
                    this.SelectedItem = null;
                }
            }
        }

        public void ForceCloseTab(TabControlItem tabItem)
        {
            this.RemoveTab(tabItem);
            this.OnTabControlItemClosed(tabItem);
        }

        public void CloseTab(TabControlItem tabItem)
        {
            if (tabItem != null)
            {
                this.SelectedItem = tabItem;
                var args = new TabControlItemClosingEventArgs(this.SelectedItem);
                this.OnTabControlItemClosing(args);
                if (this.SelectedItem != null && !args.Cancel && this.SelectedItem.CanClose)
                {
                    this.RemoveTab(this.SelectedItem);
                    this.OnTabControlItemClosed(tabItem);
                }
            }
        }

        protected override void OnClick(EventArgs e)
        {
            if (this.IsMouseModdleClick(e))
                this.CloseTabAtCurrentCursor();

            base.OnClick(e);
        }

        private bool IsMouseModdleClick(EventArgs e)
        {
            var mouse = e as MouseEventArgs;
            return mouse != null && mouse.Button == MouseButtons.Middle;
        }

        private void CloseTabAtCurrentCursor()
        {
            var selectedTab = this.GetTabItemByPoint(this.PointToClient(Cursor.Position));
            this.CloseTab(selectedTab);
        }

        public bool ShowToolTipOnTitle { get; set; }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            try
            {
                this.HandleTablItemMouseUpActions(e);
                var handled = this.HandleMenuGlipMouseUp(e);
                handled |= this.HandleCloseButtonMouseUp(e);
                handled |= this.HandleTabDetach(e);

                if (!handled)
                    base.OnMouseUp(e);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
            finally
            {
                this.tabAtMouseDown = null;
                this.mouseDownAtMenuGliph = false;
                this.mouseDownAtCloseGliph = false;
                this.movePreview.Hide();
            }
        }

        private bool HandleTabDetach(MouseEventArgs e)
        {
            var outside = this.IsMouseOutsideHeader(e.Location);
            if (outside && this.tabAtMouseDown != null)
            {
                this.FireTabItemDetach();
                return true;
            }

            return false;
        }

        private void FireTabItemDetach()
        {
            if (this.TabControlItemDetach != null)
            {
                var args = new TabControlItemChangedEventArgs(this.tabAtMouseDown, TabControlItemChangeTypes.Changed);
                this.TabControlItemDetach(args);
            }
        }

        private const int MOVE_TOLERANCE = 5;

        private bool IsMouseOutsideHeader(Point location)
        {
            var outsideY = location.Y < -MOVE_TOLERANCE || DEF_HEADER_HEIGHT + MOVE_TOLERANCE < location.Y;
            var outsideX = location.X < -MOVE_TOLERANCE || this.Width + MOVE_TOLERANCE < location.X;
            return outsideY || outsideX;
        }

        private bool HandleCloseButtonMouseUp(MouseEventArgs e)
        {
            if (this.mouseDownAtCloseGliph && this.MouseIsOnCloseButton(e))
            {
                this.CloseTab(this.SelectedItem);
                return true;
            }

            return false;
        }

        private bool HandleMenuGlipMouseUp(MouseEventArgs e)
        {
            if (this.mouseDownAtMenuGliph && this.MouseIsOnMenuGliph(e))
            {
                this.ShowTabsMenu();
                return true;
            }

            return false;
        }

        private void HandleTablItemMouseUpActions(MouseEventArgs e)
        {
            if (this.tabAtMouseDown != null)
            {
                var upItem = this.GetTabItemByPoint(e.Location);
                if (upItem != null && upItem == this.tabAtMouseDown)
                    this.SelectedItem = upItem;
                else
                    this.SwapTabItems(e.X, upItem);
            }
        }

        private void SwapTabItems(int mouseX, TabControlItem upItem)
        {
            var downIndex = this.Items.IndexOf(this.tabAtMouseDown);
            var newIndex = this.Items.IndexOf(upItem);

            var upCentre = 48 + newIndex * 87;
            if (downIndex < newIndex)
            {
                newIndex--;
                upCentre += 10;
            }

            if (mouseX >= upCentre)
                newIndex++;

            if (newIndex > this.Items.Count - 1)
                newIndex = this.Items.Count - 1;
            if (newIndex <= 0)
                newIndex = 0;
            this.Items.Remove(this.tabAtMouseDown);
            this.Items.Insert(newIndex, this.tabAtMouseDown);
        }

        private bool mouseDownAtMenuGliph;

        private bool mouseDownAtCloseGliph;

        private Point mouseDownPoint;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.mouseDownPoint = e.Location;
                this.tabAtMouseDown = this.GetTabItemByPoint(this.mouseDownPoint);

                if (this.MouseIsOnMenuGliph(e)) // Show Tabs menu
                    this.mouseDownAtMenuGliph = true;

                if (this.MouseIsOnCloseButton(e)) // close by click on close button
                    this.mouseDownAtCloseGliph = true;
            }

            if (!this.mouseDownAtCloseGliph && !this.mouseDownAtMenuGliph && this.tabAtMouseDown == null)
                base.OnMouseDown(e); // custom handling

            this.Invalidate();
        }

        private bool MouseIsOnMenuGliph(MouseEventArgs e)
        {
            return this.menuGlyph.Rect.Contains(e.Location);
        }

        private bool MouseIsOnCloseButton(MouseEventArgs e)
        {
            return this.closeButton.Rect.Contains(e.Location);
        }

        private void ShowTabsMenu()
        {
            var args = new HandledEventArgs(false);
            this.OnMenuItemsLoading(args);
            if (!args.Handled)
                this.OnMenuItemsLoad(EventArgs.Empty);

            this.OnMenuShow();
        }

        public TabControlItem GetTabItemByPoint(Point pt)
        {
            TabControlItem item = null;

            for (var i = 0; i < this.Items.Count; i++)
            {
                var current = this.Items[i];
                if (current.StripRect.Contains(pt))
                    item = current;
            }

            return item;
        }

        protected internal virtual void OnTabControlMouseOnTitle(TabControlMouseOnTitleEventArgs e)
        {
            if (this.TabControlMouseOnTitle != null)
                this.TabControlMouseOnTitle(e);
        }

        protected internal virtual void OnTabControlMouseEnteredTitle(TabControlMouseOnTitleEventArgs e)
        {
            if (this.TabControlMouseEnteredTitle != null)
                this.TabControlMouseEnteredTitle(e);
        }

        protected internal virtual void OnTabControlMouseLeftTitle(TabControlMouseOnTitleEventArgs e)
        {
            if (this.TabControlMouseLeftTitle != null)
                this.TabControlMouseLeftTitle(e);
        }

        protected internal virtual void OnTabControlItemClosing(TabControlItemClosingEventArgs e)
        {
            if (this.TabControlItemClosing != null)
                this.TabControlItemClosing(e);
        }

        protected internal virtual void OnTabControlItemClosed(TabControlItem tabItem)
        {
            if (this.TabControlItemClosed != null)
            {
                var args = new TabControlItemClosedEventArgs {Item = tabItem};
                this.TabControlItemClosed(this, args);
            }
        }

        private void SetDefaultSelected()
        {
            if (this.selectedItem == null && this.Items.Count > 0)
                this.SelectedItem = this.Items[0];

            for (var i = 0; i < this.Items.Count; i++)
            {
                var itm = this.Items[i];
                itm.Dock = DockStyle.Fill;
            }
        }

        private void UnSelectAll()
        {
            for (var i = 0; i < this.Items.Count; i++)
            {
                var item = this.Items[i];
                this.UnSelectItem(item);
            }
        }

        internal void UnDrawAll()
        {
            for (var i = 0; i < this.Items.Count; i++)
                this.Items[i].IsDrawn = false;
        }

        internal void SelectItem(TabControlItem tabItem)
        {
            tabItem.Dock = DockStyle.Fill;
            tabItem.Visible = true;
            tabItem.Selected = true;
        }

        internal void UnSelectItem(TabControlItem tabItem)
        {
            tabItem.Selected = false;
        }

        protected virtual void OnMenuItemsLoading(HandledEventArgs e)
        {
            if (this.MenuItemsLoading != null)
                this.MenuItemsLoading(this, e);
        }

        protected virtual void OnMenuShow()
        {
            if (this.Menu.Visible == false && this.Menu.Items.Count > 0)
            {
                if (this.RightToLeft == RightToLeft.No)
                    this.Menu.Show(this, new Point(this.menuGlyph.Rect.Left, this.menuGlyph.Rect.Bottom + 2));
                else
                    this.Menu.Show(this, new Point(this.menuGlyph.Rect.Right, this.menuGlyph.Rect.Bottom + 2));

                this.MenuOpen = true;
            }
        }

        protected virtual void OnMenuItemsLoaded(EventArgs e)
        {
            if (this.MenuItemsLoaded != null)
                this.MenuItemsLoaded(this, e);
        }

        protected virtual void OnMenuItemsLoad(EventArgs e)
        {
            this.Menu.RightToLeft = this.RightToLeft;
            this.Menu.Items.Clear();
            var list = new List<ToolStripMenuItem>();
            var nr = this.Items.Count;
            for (var i = 0; i < nr; i++)
            {
                var item = this.Items[i];
                if (!item.Visible)
                    continue;

                var tItem = new ToolStripMenuItem(item.Title);
                tItem.Tag = item;
                if (item.Selected)
                    tItem.Select();
                list.Add(tItem);
            }

            list.Sort(CompareSortText);
            this.Menu.Items.AddRange(list.ToArray());
            this.OnMenuItemsLoaded(EventArgs.Empty);
        }

        private void OnMenuItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var clickedItem = (TabControlItem)e.ClickedItem.Tag;
            if (clickedItem != null)
                this.SelectedItem = clickedItem;
        }

        private void OnMenuVisibleChanged(object sender, EventArgs e)
        {
            if (this.Menu.Visible == false)
                this.MenuOpen = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            this.HandleDrawMenuGliph(e);
            this.HandleDrawCloseButton(e);
            this.HandleMouseInTitle(e);
            this.HandlePreviewMove(e);
        }

        private readonly TabPreview movePreview;

        private void HandlePreviewMove(MouseEventArgs e)
        {
            if (this.tabAtMouseDown != null)
            {
                this.UpdateMovePreviewLocation(e);
                this.ShowTabPreview(e);
            }
        }

        private void UpdateMovePreviewLocation(MouseEventArgs e)
        {
            var newLocation = this.PointToScreen(e.Location);
            newLocation.X -= this.mouseDownPoint.X;
            newLocation.Y -= this.mouseDownPoint.Y;
            this.movePreview.Location = newLocation;
        }

        private void ShowTabPreview(MouseEventArgs e)
        {
            var movedOutOfTolerance = this.MovedOutOfTolerance(e);
            if (movedOutOfTolerance)
            {
                var toDetach = this.IsMouseOutsideHeader(e.Location);
                this.movePreview.UpdateDetachState(toDetach);
                this.movePreview.Show(this.tabAtMouseDown);
            }
        }

        private bool MovedOutOfTolerance(MouseEventArgs e)
        {
            var xDelta = Math.Abs(this.mouseDownPoint.X - e.Location.X);
            var yDelta = Math.Abs(this.mouseDownPoint.Y - e.Location.Y);
            var movedOutOfTolerance = xDelta > MOVE_TOLERANCE || yDelta > MOVE_TOLERANCE;
            return movedOutOfTolerance;
        }

        private void HandleMouseInTitle(MouseEventArgs e)
        {
            var item = this.GetTabItemByPoint(e.Location);
            if (item != null)
            {
                var inTitle = item.LocationIsInTitle(e.Location);
                var args = new TabControlMouseOnTitleEventArgs(item, e.Location);
                if (inTitle)
                {
                    //mouseWasOnTitle = true;
                    this.OnTabControlMouseOnTitle(args);
                    if (!this.mouseEnteredTitle)
                    {
                        this.mouseEnteredTitle = true;
                        this.OnTabControlMouseEnteredTitle(args);
                    }
                }
                else if (this.mouseEnteredTitle) // if (mouseWasOnTitle)
                {
                    //mouseWasOnTitle = false;
                    this.mouseEnteredTitle = false;
                    this.OnTabControlMouseLeftTitle(args);
                }
            }
        }

        private void HandleDrawMenuGliph(MouseEventArgs e)
        {
            if (this.MouseIsOnMenuGliph(e))
            {
                this.menuGlyph.IsMouseOver = true;
                this.Invalidate(this.menuGlyph.Rect);
            }
            else
            {
                if (this.menuGlyph.IsMouseOver && !this.MenuOpen)
                {
                    this.menuGlyph.IsMouseOver = false;
                    this.Invalidate(this.menuGlyph.Rect);
                }
            }
        }

        private void HandleDrawCloseButton(MouseEventArgs e)
        {
            if (this.MouseIsOnCloseButton(e))
            {
                this.closeButton.IsMouseOver = true;
                this.Invalidate(this.closeButton.Rect);
            }
            else
            {
                if (this.closeButton.IsMouseOver)
                {
                    this.closeButton.IsMouseOver = false;
                    this.Invalidate(this.closeButton.Rect);
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.menuGlyph.IsMouseOver = false;
            this.Invalidate(this.menuGlyph.Rect);

            this.closeButton.IsMouseOver = false;
            this.Invalidate(this.closeButton.Rect);
        }

        private void OnCalcTabPage(Graphics g, TabControlItem currentItem)
        {
            var currentFont = this.Font;
            if (currentItem == this.SelectedItem)
                currentFont = new Font(this.Font, FontStyle.Bold);

            var textSize = g.MeasureString(currentItem.Title, currentFont, new SizeF(200, 10), this.sf);
            textSize.Width += 20;

            if (this.RightToLeft == RightToLeft.No)
            {
                var buttonRect = new RectangleF(this.DEF_START_POS, 3, textSize.Width, 17);
                currentItem.StripRect = buttonRect;
                this.DEF_START_POS += (int)textSize.Width;
            }
            else
            {
                var buttonRect = new RectangleF(this.DEF_START_POS - textSize.Width + 1, 3, textSize.Width - 1, 17);
                currentItem.StripRect = buttonRect;
                this.DEF_START_POS -= (int)textSize.Width;
            }
        }

        internal void OnDrawTabPage(Graphics g, TabControlItem currentItem)
        {
            var isFirstTab = this.Items.IndexOf(currentItem) == 0;
            var currentFont = this.Font;

            if (currentItem == this.SelectedItem)
                currentFont = new Font(this.Font, FontStyle.Bold);

            var textSize = g.MeasureString(currentItem.Title, currentFont, new SizeF(200, 10), this.sf);
            textSize.Width += 20;
            var buttonRect = currentItem.StripRect;

            var path = new GraphicsPath();
            LinearGradientBrush brush = null;
            var mtop = 3;

            #region Draw Not Right-To-Left Tab

            if (this.RightToLeft == RightToLeft.No)
            {
                if (currentItem == this.SelectedItem || isFirstTab)
                {
                    path.AddLine(buttonRect.Left - 10, buttonRect.Bottom - 1,
                        buttonRect.Left + buttonRect.Height / 2 - 4, mtop + 4);
                }
                else
                {
                    path.AddLine(buttonRect.Left, buttonRect.Bottom - 1, buttonRect.Left,
                        buttonRect.Bottom - buttonRect.Height / 2 - 2);
                    path.AddLine(buttonRect.Left, buttonRect.Bottom - buttonRect.Height / 2 - 3,
                        buttonRect.Left + buttonRect.Height / 2 - 4, mtop + 3);
                }

                path.AddLine(buttonRect.Left + buttonRect.Height / 2 + 2, mtop, buttonRect.Right - 3, mtop);
                path.AddLine(buttonRect.Right, mtop + 2, buttonRect.Right, buttonRect.Bottom - 1);
                path.AddLine(buttonRect.Right - 4, buttonRect.Bottom - 1, buttonRect.Left, buttonRect.Bottom - 1);
                path.CloseFigure();
                try
                {
                    if (currentItem == this.SelectedItem)
                        brush = new LinearGradientBrush(buttonRect, SystemColors.ControlLightLight, SystemColors.Window,
                            LinearGradientMode.Vertical);
                    else
                        brush = new LinearGradientBrush(buttonRect, SystemColors.ControlLightLight,
                            SystemColors.Control, LinearGradientMode.Vertical);
                }
                catch (Exception)
                {
                }

                g.FillPath(brush, path);
                var pen = SystemPens.ControlDark;
                if (currentItem == this.SelectedItem)
                    pen = new Pen(this.ToolStripRenderer.ColorTable.MenuStripGradientBegin);
                g.DrawPath(pen, path);
                if (currentItem == this.SelectedItem)
                    pen.Dispose();

                if (currentItem == this.SelectedItem)
                    g.DrawLine(new Pen(brush), buttonRect.Left - 9, buttonRect.Height + 2,
                        buttonRect.Left + buttonRect.Width - 1, buttonRect.Height + 2);

                var textLoc = new PointF(buttonRect.Left + buttonRect.Height - 4,
                    buttonRect.Top + buttonRect.Height / 2 - textSize.Height / 2 - 3);
                var textRect = buttonRect;
                textRect.Location = textLoc;
                textRect.Width = buttonRect.Width - (textRect.Left - buttonRect.Left) - 4;
                textRect.Height = textSize.Height + currentFont.Size / 2;

                if (currentItem == this.SelectedItem)
                    //textRect.Y -= 2;
                    g.DrawString(currentItem.Title, currentFont, new SolidBrush(this.ForeColor), textRect, this.sf);
                else
                    g.DrawString(currentItem.Title, currentFont, new SolidBrush(this.ForeColor), textRect, this.sf);
            }

            #endregion

            #region Draw Right-To-Left Tab

            if (this.RightToLeft == RightToLeft.Yes)
            {
                if (currentItem == this.SelectedItem || isFirstTab)
                {
                    path.AddLine(buttonRect.Right + 10, buttonRect.Bottom - 1,
                        buttonRect.Right - buttonRect.Height / 2 + 4, mtop + 4);
                }
                else
                {
                    path.AddLine(buttonRect.Right, buttonRect.Bottom - 1, buttonRect.Right,
                        buttonRect.Bottom - buttonRect.Height / 2 - 2);
                    path.AddLine(buttonRect.Right, buttonRect.Bottom - buttonRect.Height / 2 - 3,
                        buttonRect.Right - buttonRect.Height / 2 + 4, mtop + 3);
                }

                path.AddLine(buttonRect.Right - buttonRect.Height / 2 - 2, mtop, buttonRect.Left + 3, mtop);
                path.AddLine(buttonRect.Left, mtop + 2, buttonRect.Left, buttonRect.Bottom - 1);
                path.AddLine(buttonRect.Left + 4, buttonRect.Bottom - 1, buttonRect.Right, buttonRect.Bottom - 1);
                path.CloseFigure();

                if (currentItem == this.SelectedItem)
                    brush = new LinearGradientBrush(buttonRect, SystemColors.ControlLightLight, SystemColors.Window,
                        LinearGradientMode.Vertical);
                else
                    brush = new LinearGradientBrush(buttonRect, SystemColors.ControlLightLight, SystemColors.Control,
                        LinearGradientMode.Vertical);

                g.FillPath(brush, path);
                g.DrawPath(SystemPens.ControlDark, path);

                if (currentItem == this.SelectedItem)
                    g.DrawLine(new Pen(brush), buttonRect.Right + 9, buttonRect.Height + 2,
                        buttonRect.Right - buttonRect.Width + 1, buttonRect.Height + 2);

                var textLoc = new PointF(buttonRect.Left + 2,
                    buttonRect.Top + buttonRect.Height / 2 - textSize.Height / 2 - 2);
                var textRect = buttonRect;
                textRect.Location = textLoc;
                textRect.Width = buttonRect.Width - (textRect.Left - buttonRect.Left) - 10;
                textRect.Height = textSize.Height + currentFont.Size / 2;

                if (currentItem == this.SelectedItem)
                {
                    textRect.Y -= 1;
                    g.DrawString(currentItem.Title, currentFont, new SolidBrush(this.ForeColor), textRect, this.sf);
                }
                else
                {
                    g.DrawString(currentItem.Title, currentFont, new SolidBrush(this.ForeColor), textRect, this.sf);
                }

                //g.FillRectangle(Brushes.Red, textRect);
            }

            #endregion

            currentItem.IsDrawn = true;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (this.isIniting)
                return;

            this.UpdateLayout();
        }

        private void UpdateLayout()
        {
            if (this.RightToLeft == RightToLeft.No)
            {
                this.sf.Trimming = StringTrimming.EllipsisCharacter;
                this.sf.FormatFlags |= StringFormatFlags.NoWrap;
                this.sf.FormatFlags &= StringFormatFlags.DirectionRightToLeft;

                this.stripButtonRect = new Rectangle(0, 0, this.ClientSize.Width - DEF_GLYPH_WIDTH - 2, 10);
                this.menuGlyph.Rect = new Rectangle(this.ClientSize.Width - DEF_GLYPH_WIDTH, 2, 16, 16);
                this.closeButton.Rect = new Rectangle(this.ClientSize.Width - 20, 2, 16, 15);
            }
            else
            {
                this.sf.Trimming = StringTrimming.EllipsisCharacter;
                this.sf.FormatFlags |= StringFormatFlags.NoWrap;
                this.sf.FormatFlags |= StringFormatFlags.DirectionRightToLeft;

                this.stripButtonRect =
                    new Rectangle(DEF_GLYPH_WIDTH + 2, 0, this.ClientSize.Width - DEF_GLYPH_WIDTH - 15, 10);
                this.menuGlyph.Rect = new Rectangle(20 + 4, 2, 16, 16); //this.ClientSize.Width - 20, 2, 16, 16);
                this.closeButton.Rect = new Rectangle(4, 2, 16, 15); //ClientSize.Width - DEF_GLYPH_WIDTH, 2, 16, 16);
            }

            var borderWidth = this.showBorder ? 1 : 0;
            var headerWidth = this.showTabs ? DEF_HEADER_HEIGHT + 1 : 1;

            this.DockPadding.Top = headerWidth;
            this.DockPadding.Bottom = borderWidth;
            this.DockPadding.Right = borderWidth;
            this.DockPadding.Left = borderWidth;
        }

        protected virtual void OnTabControlItemChanged(TabControlItemChangedEventArgs e)
        {
            if (this.TabControlItemSelectionChanged != null)
                this.TabControlItemSelectionChanged(e);
        }

        private void OnCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            var itm = (TabControlItem)e.Element;

            if (e.Action == CollectionChangeAction.Add)
            {
                this.Controls.Add(itm);
                this.OnTabControlItemChanged(new TabControlItemChangedEventArgs(itm, TabControlItemChangeTypes.Added));
            }
            else if (e.Action == CollectionChangeAction.Remove)
            {
                this.Controls.Remove(itm);
                this.OnTabControlItemChanged(
                    new TabControlItemChangedEventArgs(itm, TabControlItemChangeTypes.Removed));
            }
            else
            {
                this.OnTabControlItemChanged(
                    new TabControlItemChangedEventArgs(itm, TabControlItemChangeTypes.Changed));
            }

            this.UpdateLayout();
            this.Invalidate();
        }

        #endregion

        #region Props

        [DefaultValue(null)]
        [RefreshProperties(RefreshProperties.All)]
        public TabControlItem SelectedItem
        {
            get => this.selectedItem;
            set
            {
                if (this.selectedItem == value)
                    return;

                if (value == null && this.Items.Count > 0)
                {
                    var itm = this.Items[0];
                    if (itm.Visible)
                    {
                        this.selectedItem = itm;
                        this.selectedItem.Selected = true;
                        this.selectedItem.Dock = DockStyle.Fill;
                    }
                }
                else
                {
                    this.selectedItem = value;
                }

                foreach (TabControlItem itm in this.Items)
                    if (itm == this.selectedItem)
                    {
                        this.SelectItem(itm);
                        itm.Dock = DockStyle.Fill;
                        itm.Show();
                    }
                    else
                    {
                        this.UnSelectItem(itm);
                        itm.Hide();
                    }

                if (this.selectedItem != null)
                    this.SelectItem(this.selectedItem);
                this.Invalidate();

                /*if (selectedItem != null && !selectedItem.IsDrawn)
                {
                    Items.MoveTo(0, selectedItem);
                    Invalidate();
                }*/

                this.OnTabControlItemChanged(new TabControlItemChangedEventArgs(this.selectedItem,
                    TabControlItemChangeTypes.SelectionChanged));
            }
        }

        [DefaultValue(typeof(Size), "350,200")]
        public new Size Size
        {
            get => base.Size;
            set
            {
                if (base.Size == value)
                    return;

                base.Size = value;
                this.UpdateLayout();
            }
        }

        [DefaultValue(true)]
        public bool AlwaysShowMenuGlyph
        {
            get => this.alwaysShowMenuGlyph;
            set
            {
                if (this.alwaysShowMenuGlyph == value)
                    return;

                this.alwaysShowMenuGlyph = value;
                this.Invalidate();
            }
        }

        [DefaultValue(true)]
        public bool AlwaysShowClose
        {
            get => this.alwaysShowClose;
            set
            {
                if (this.alwaysShowClose == value)
                    return;

                this.alwaysShowClose = value;
                this.Invalidate();
            }
        }

        [DefaultValue(true)]
        public bool ShowTabs
        {
            get => this.showTabs;
            set
            {
                if (this.showTabs != value)
                {
                    this.showTabs = value;
                    this.Invalidate();
                    this.UpdateLayout();
                }
            }
        }

        [DefaultValue(true)]
        public bool ShowBorder
        {
            get => this.showBorder;
            set
            {
                if (this.showBorder != value)
                {
                    this.showBorder = value;
                    this.Invalidate();
                    this.UpdateLayout();
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TabControlItemCollection Items { get; }

        /// <summary>
        ///     DesignerSerializationVisibility
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ControlCollection Controls => base.Controls;

        [Browsable(false)] public ContextMenuStrip Menu { get; }

        [Browsable(false)] public bool MenuOpen { get; private set; }

        #endregion

        #region ShouldSerialize

        public bool ShouldSerializeSelectedItem()
        {
            return true;
        }

        public bool ShouldSerializeItems()
        {
            return this.Items.Count > 0;
        }

        public bool ShouldSerializeFont()
        {
            return this.Font.Name != "Tahoma" && this.Font.Size != 8.25f && this.Font.Style != FontStyle.Regular;
        }

        public new void ResetFont()
        {
            this.Font = new Font("Tahoma", 8.25f, FontStyle.Regular);
        }

        #endregion

        #region ISupportInitialize Members

        public void BeginInit()
        {
            this.isIniting = true;
        }

        public void EndInit()
        {
            this.isIniting = false;
        }

        #endregion
    }
}