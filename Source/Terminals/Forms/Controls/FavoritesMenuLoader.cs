﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Terminals.Configuration;
using Terminals.Connections;
using Terminals.Data;
using Terminals.Forms.Controls;
using Terminals.Properties;
using Settings = Terminals.Configuration.Settings;

namespace Terminals
{
    internal partial class MainForm : IConnectionMainView
    {
        /// <summary>
        /// Fills menu, tool strip menu and tool bar with favorite buttons
        /// </summary>
        private class FavoritesMenuLoader
        {
            private readonly Settings settings = Settings.Instance;
            private readonly IPersistence persistence;

            private ToolStripMenuItem favoritesToolStripMenuItem;
            private GroupMenuItem untaggedToolStripMenuItem;
            private ToolStripComboBox tscConnectTo;
            private EventHandler serverToolStripMenuItemClick;
            private ToolStrip favoriteToolBar;
            private ContextMenuStrip quickContextMenu;
            private ToolStripItemClickedEventHandler quickContextMenuItemClicked;
            private ToolStripMenuItem groupsToolStripMenuItem;
            private ToolStripSeparator groupsSeparator;
            private ToolStripMenuItem addTerminalToGroupToolStripMenuItem;
            private ToolStripMenuItem saveTerminalsAsGroupToolStripMenuItem;
            private EventHandler groupToolStripMenuItemClick;
            private EventHandler groupAddToolStripMenuItemClick;

            internal const String COMMAND_EXIT = "Exit";
            internal const String QUICK_CONNECT = "QuickConnect";
            internal const String COMMAND_SPECIAL = "SpecialCommands";
            internal const String COMMAND_RESTORESCREEN = "RestoreScreen";
            internal const String COMMAND_FULLSCREEN = "FullScreen";
            internal const String COMMAND_SHOWMENU = "ShowMenu";
            internal const String COMMAND_OPTIONS = "Options";
            internal const String COMMAND_CAPTUREMANAGER = "ScreenCaptureManager";
            internal const String COMMAND_NETTOOLS = "NetworkingTools";
            internal const String COMMAND_CREDENTIALMANAGER = "CredentialsManager";
            internal const String COMMAND_ORGANIZEFAVORITES = "OrganizeFavorites";
            private const String COMMAND_ALPHABETICAL = "Alphabetical";

            private ToolStripMenuItem special;
            private ToolStripMenuItem mgmt;
            private ToolStripMenuItem cpl;
            private ToolStripMenuItem other;
            private GroupMenuItem unTaggedQuickMenuItem;
            private ToolStripMenuItem alphabeticalMenu;
            private ToolStripItem restoreScreenMenuItem;
            private ToolStripItem fullScreenMenuItem;

            private readonly ToolTipBuilder toolTipBuilder;

            /// <summary>
            /// Stored in context menu Tag to identify favorite context menu items
            /// </summary>
            internal const String FAVORITE = "favorite";

            private int AlphabeticalMenuItemIndex => this.quickContextMenu.Items.IndexOf(this.alphabeticalMenu);

            private IOrderedEnumerable<IGroup>  PersistedGroups
            {
                get { return this.persistence.Groups.OrderBy(group => group.Name); }
            }

            private IFavorites PersistedFavorites => this.persistence.Favorites;

            internal FavoritesMenuLoader(MainForm mainForm, IPersistence persistence)
            {
                this.persistence = persistence;
                this.toolTipBuilder = new ToolTipBuilder(this.persistence.Security);
                AssignMainFormFields(mainForm);
                this.favoritesToolStripMenuItem.DropDownItems.Add("-");
                CreateUntaggedItem();
                CreateTrayMenuItems();
                UpdateMenuAndContextMenu();
                RegisterEventHandlers();
            }

            private void RegisterEventHandlers()
            {
                var dispatcher = this.persistence.Dispatcher;
                dispatcher.GroupsChanged += new GroupsChangedEventHandler(this.OnDataChanged);
                dispatcher.FavoritesChanged += new FavoritesChangedEventHandler(this.OnDataChanged);
                settings.ConfigurationChanged += new ConfigurationChangedHandler(this.OnSettingsConfigurationChanged);
            }

            private void AssignMainFormFields(MainForm mainForm)
            {
                this.favoritesToolStripMenuItem = mainForm.favoritesToolStripMenuItem;
                this.tscConnectTo = mainForm.tscConnectTo;
                this.serverToolStripMenuItemClick = mainForm.ServerToolStripMenuItem_Click;
                this.favoriteToolBar = mainForm.favoriteToolBar;
                this.quickContextMenu = mainForm.QuickContextMenu;
                this.quickContextMenuItemClicked = mainForm.QuickContextMenu_ItemClicked;
                this.groupsToolStripMenuItem = mainForm.groupsToolStripMenuItem;
                this.groupsSeparator = mainForm.groupsSeparator;
                this.addTerminalToGroupToolStripMenuItem = mainForm.addTerminalToGroupToolStripMenuItem;
                this.saveTerminalsAsGroupToolStripMenuItem = mainForm.saveTerminalsAsGroupToolStripMenuItem;
                this.groupToolStripMenuItemClick = mainForm.GroupToolStripMenuItem_Click;
                this.groupAddToolStripMenuItemClick = mainForm.GroupAddToolStripMenuItem_Click;
            }

            private void OnDataChanged(EventArgs args)
            {
                // performance hit when importing large number of favorites => improve the refresh
                UpdateMenuAndContextMenu();
            }

            private void OnSettingsConfigurationChanged(ConfigurationChangedEventArgs args)
            {
                LoadFavoritesToolbar();
            }

            /// <summary>
            /// Simple refresh, which removes all menu items and recreates new, when necessary using lazy loading.
            /// </summary>
            private void UpdateMenuAndContextMenu()
            {
                this.FillMainMenu();
                this.FillTrayContextMenu();
            }

            private void CreateUntaggedItem()
            {
                this.untaggedToolStripMenuItem = new UntagedMenuItem(this.persistence);
                this.untaggedToolStripMenuItem.DropDownOpening += new EventHandler(this.OnTagMenuDropDownOpening);
                this.favoritesToolStripMenuItem.DropDownItems.Add(this.untaggedToolStripMenuItem);
            }

            /// <summary>
            /// Creates skeleton for system tray menu items. No tags or favorites are added here.
            /// </summary>
            private void CreateTrayMenuItems()
            {
                this.AddGeneralTrayContextMenu();
                this.quickContextMenu.Items.Add("-");
                AddUntaggedQuickContextMenu();

                // here favorite Tags will be placed

                this.AddAlphabeticalContextMenu();

                var quickConnect = this.quickContextMenu.Items.Add(Program.Resources.GetString(QUICK_CONNECT));
                quickConnect.Name = QUICK_CONNECT;

                this.quickContextMenu.Items.Add("-");

                var exitMenu = this.quickContextMenu.Items.Add(Program.Resources.GetString(COMMAND_EXIT));
                exitMenu.Name = COMMAND_EXIT;
            }

            private void AddUntaggedQuickContextMenu()
            {
                this.unTaggedQuickMenuItem = new UntagedMenuItem(this.persistence);
                unTaggedQuickMenuItem.DropDownItemClicked += new ToolStripItemClickedEventHandler(this.quickContextMenuItemClicked);
                unTaggedQuickMenuItem.DropDownOpening += new EventHandler(OnTagTrayMenuItemDropDownOpening);
                this.quickContextMenu.Items.Add(unTaggedQuickMenuItem);
            }

            private void FillMainMenu()
            {
                ReFreshConnectionsComboBox();
                this.untaggedToolStripMenuItem.ClearDropDownsToEmpty();
                this.ClearFavoritesToolStripmenuItems();
                this.CreateGroupsToolStripMenuItems();
                this.LoadFavoritesToolbar();
            }

            private void ReFreshConnectionsComboBox()
            {
                this.tscConnectTo.Items.Clear();
                var connectionNames = PersistedFavorites.Select(favorite => favorite.Name).ToArray();
                this.tscConnectTo.Items.AddRange(connectionNames);
            }

            #region Menu toolstrips

            private void ClearFavoritesToolStripmenuItems()
            {
                var dropDowns = this.favoritesToolStripMenuItem.DropDownItems;
                var seperatorIndex = dropDowns.IndexOf(this.untaggedToolStripMenuItem);
                for (var index = dropDowns.Count - 1; index > seperatorIndex; index--)
                {
                    var tagMenuItem = dropDowns[index] as ToolStripMenuItem;
                    UnregisterTagMenuItemEventHandlers(tagMenuItem);
                    dropDowns.RemoveAt(index);
                }
            }

            private void UnregisterTagMenuItemEventHandlers(ToolStripMenuItem tagMenuItem)
            {
                tagMenuItem.DropDownOpening -= OnTagMenuDropDownOpening;
                foreach (ToolStripMenuItem favoriteItem in tagMenuItem.DropDownItems)
                {
                    favoriteItem.Click -= this.serverToolStripMenuItemClick;
                }
            }

            /// <summary>
            /// Fills the main window "favorites" menu, after separator places all tags
            /// and their favorites as dropdown items
            /// </summary>
            private void CreateGroupsToolStripMenuItems()
            {
                foreach (var group in PersistedGroups)
                {
                    ToolStripMenuItem tagMenu = new GroupMenuItem(group);
                    tagMenu.DropDownOpening += new EventHandler(this.OnTagMenuDropDownOpening);
                    this.favoritesToolStripMenuItem.DropDownItems.Add(tagMenu);
                }
            }

            /// <summary>
            /// Lazy loading for favorites dropdown menu items in Tag menu item
            /// </summary>
            private void OnTagMenuDropDownOpening(object sender, EventArgs e)
            {
                var groupMenu = (GroupMenuItem)sender;
                if (groupMenu.IsEmpty)
                {
                    groupMenu.DropDown.Items.Clear();
                    List<IFavorite> tagFavorites = Favorites.OrderByDefaultSorting(groupMenu.Favorites);
                    foreach (var favorite in tagFavorites)
                    {
                        var item = this.CreateToolStripItemByFavorite(favorite);
                        groupMenu.DropDown.Items.Add(item);
                    }
                }
            }

            private ToolStripMenuItem CreateToolStripItemByFavorite(IFavorite favorite)
            {
                var item = CreateFavoriteMenuItem(favorite);
                item.Click += this.serverToolStripMenuItemClick;
                return item;
            }

            #endregion

            #region Fill tool bar by user defined items

            private void LoadFavoritesToolbar()
            {
                try
                {
                    favoriteToolBar.SuspendLayout();
                    ClearFavoriteButtons();
                    CreateFavoriteButtons();
                    favoriteToolBar.ResumeLayout();
                }
                catch (Exception exc)
                {
                    Logging.Error("Error Loading Favorites Toolbar", exc);
                }
            }

            private void ClearFavoriteButtons()
            {
                foreach (ToolStripButton favoriteButton in this.favoriteToolBar.Items)
                {
                    favoriteButton.Click -= this.serverToolStripMenuItemClick;
                }
                this.favoriteToolBar.Items.Clear();
            }

            private void CreateFavoriteButtons()
            {
                foreach (var favoriteId in settings.FavoritesToolbarButtons)
                {
                    this.CreateFavoriteButton(favoriteId);
                }
            }

            private void CreateFavoriteButton(Guid favoriteId)
            {
                var favorite = PersistedFavorites[favoriteId];
                if (favorite != null)
                {
                    var favoriteBtn = this.CreateFavoriteButton(favorite);
                    this.favoriteToolBar.Items.Add(favoriteBtn);
                }
            }

            private ToolStripButton CreateFavoriteButton(IFavorite favorite)
            {
                var buttonImage = this.PersistedFavorites.LoadFavoriteIcon(favorite);
                var favoriteBtn = new ToolStripButton(favorite.Name, buttonImage, this.serverToolStripMenuItemClick);
                favoriteBtn.ToolTipText = this.toolTipBuilder.BuildTooTip(favorite);
                favoriteBtn.Tag = favorite;
                favoriteBtn.Overflow = ToolStripItemOverflow.AsNeeded;
                return favoriteBtn;
            }

            #endregion

            #region System tray context menu

            private void FillTrayContextMenu()
            {
                this.unTaggedQuickMenuItem.ClearDropDownsToEmpty();
                this.ClearTrayFavoritesMenu();
                this.AddTagTrayMenuItems();
                this.alphabeticalMenu.DropDownItems.Clear();
            }

            private void ClearTrayFavoritesMenu()
            {
                var startIndex = this.quickContextMenu.Items.IndexOf(this.unTaggedQuickMenuItem) + 1;
                while (startIndex < this.AlphabeticalMenuItemIndex)
                {
                    // unregister event handler to release the menu item
                    var tagItem = (ToolStripMenuItem)this.quickContextMenu.Items[startIndex];
                    tagItem.DropDownItemClicked -= this.quickContextMenuItemClicked;
                    tagItem.DropDownOpening -= OnTagTrayMenuItemDropDownOpening;
                    this.quickContextMenu.Items.RemoveAt(startIndex);
                }
            }

            private void AddTagTrayMenuItems()
            {
                foreach (var group in PersistedGroups)
                {
                    ToolStripMenuItem tagMenuItem = new GroupMenuItem(group);
                    tagMenuItem.DropDownItemClicked += new ToolStripItemClickedEventHandler(this.quickContextMenuItemClicked);
                    tagMenuItem.DropDownOpening += new EventHandler(OnTagTrayMenuItemDropDownOpening);
                    this.quickContextMenu.Items.Insert(this.AlphabeticalMenuItemIndex, tagMenuItem);
                }
            }

            private void OnTagTrayMenuItemDropDownOpening(object sender, EventArgs e)
            {
                var groupMenu = (GroupMenuItem)sender;
                if (groupMenu.IsEmpty)
                {
                    groupMenu.DropDown.Items.Clear();
                    List<IFavorite> tagFavorites = Favorites.OrderByDefaultSorting(groupMenu.Favorites);
                    foreach (var favorite in tagFavorites)
                    {
                        var item = CreateFavoriteMenuItem(favorite);
                        groupMenu.DropDown.Items.Add(item);
                    }
                }
            }

            private ToolStripMenuItem CreateFavoriteMenuItem(IFavorite favorite)
            {
                var item = new ToolStripMenuItem(favorite.Name);
                item.Tag = FAVORITE;
                item.Image = this.PersistedFavorites.LoadFavoriteIcon(favorite);
                item.ToolTipText = this.toolTipBuilder.BuildTooTip(favorite);
                return item;
            }

            private void AddAlphabeticalContextMenu()
            {
                this.alphabeticalMenu = new ToolStripMenuItem(Program.Resources.GetString(COMMAND_ALPHABETICAL));
                this.alphabeticalMenu.Name = COMMAND_ALPHABETICAL;
                this.alphabeticalMenu.DropDownItemClicked += new ToolStripItemClickedEventHandler(this.quickContextMenuItemClicked);
                this.alphabeticalMenu.DropDownOpening += new EventHandler(this.OnAlphabeticalMenuDropDownOpening);
                this.alphabeticalMenu.Image = Resources.atoz;
                this.quickContextMenu.Items.Add(this.alphabeticalMenu);
            }

            private void OnAlphabeticalMenuDropDownOpening(object sender, EventArgs e)
            {
                if (!this.alphabeticalMenu.HasDropDownItems)
                {
                    List<IFavorite> favorites = new SortableList<IFavorite>(PersistedFavorites)
                        .SortByProperty("Name", SortOrder.Ascending);
                    CreateAlphabeticalFavoriteMenuItems(favorites);
                    var alphaMenuVisible = this.alphabeticalMenu.DropDownItems.Count > 0;
                    this.alphabeticalMenu.Visible = alphaMenuVisible;
                }
            }

            private void CreateAlphabeticalFavoriteMenuItems(List<IFavorite> favorites)
            {
                foreach (var favorite in favorites)
                {
                    var sortedItem = CreateFavoriteMenuItem(favorite);
                    this.alphabeticalMenu.DropDownItems.Add(sortedItem);
                }
            }

            private void AddGeneralTrayContextMenu()
            {
                restoreScreenMenuItem = this.CreateGeneralTrayContextMenuItem(COMMAND_RESTORESCREEN, Resources.arrow_in);
                fullScreenMenuItem = this.CreateGeneralTrayContextMenuItem(COMMAND_FULLSCREEN, Resources.arrow_out);

                this.quickContextMenu.Items.Add("-");
                var showMenu = this.quickContextMenu.Items.Add(Program.Resources.GetString(COMMAND_SHOWMENU));
                showMenu.Name = COMMAND_SHOWMENU;
                this.quickContextMenu.Items.Add("-");
                CreateGeneralTrayContextMenuItem(COMMAND_CAPTUREMANAGER, Resources.screen_capture_box);
                CreateGeneralTrayContextMenuItem(COMMAND_NETTOOLS, Resources.computer_link);
                this.quickContextMenu.Items.Add("-");
                CreateGeneralTrayContextMenuItem(COMMAND_CREDENTIALMANAGER, Resources.computer_security);
                CreateGeneralTrayContextMenuItem(COMMAND_ORGANIZEFAVORITES, Resources.star);
                CreateGeneralTrayContextMenuItem(COMMAND_OPTIONS, Resources.options);
                this.quickContextMenu.Items.Add("-");
            }

            internal void UpdateSwitchFullScreenMenuItemsVisibility(Boolean fullScreen)
            {
                restoreScreenMenuItem.Visible = fullScreen;
                fullScreenMenuItem.Visible = !fullScreen;
            }

            private ToolStripItem CreateGeneralTrayContextMenuItem(String commnadName, Image icon)
            {
                var menuText = Program.Resources.GetString(commnadName);
                var menuItem = this.quickContextMenu.Items.Add(menuText, icon);
                menuItem.Name = commnadName;
                return menuItem;
            }

            #endregion

            internal void LoadGroups()
            {
                RemoveGroupsFromGroupsMenu();
                ClearAddToGroupMenuItem();
                AddGroupMenuItems();
                addTerminalToGroupToolStripMenuItem.Enabled = false;
                saveTerminalsAsGroupToolStripMenuItem.Enabled = false;
            }

            private void ClearAddToGroupMenuItem()
            {
                var dropDowns = this.addTerminalToGroupToolStripMenuItem.DropDownItems;
                for (var index = dropDowns.Count -1; 0 <= index; index--)
                {
                    var menuItem = (ToolStripMenuItem)dropDowns[index];
                    menuItem.Click -= this.groupAddToolStripMenuItemClick;
                    this.addTerminalToGroupToolStripMenuItem.DropDownItems.Remove(menuItem);
                }
            }

            private void AddGroupMenuItems()
            {
                foreach (var group in PersistedGroups)
                {
                    this.AddGroupMenuItems(group);
                }
            }

            private void RemoveGroupsFromGroupsMenu()
            {
                var seperatorIndex = this.groupsToolStripMenuItem.DropDownItems.IndexOf(this.groupsSeparator);
                for (var index = this.groupsToolStripMenuItem.DropDownItems.Count - 1; index > seperatorIndex; index--)
                {
                    var menuItem = this.groupsToolStripMenuItem.DropDownItems[index];
                    menuItem.Click -= this.groupToolStripMenuItemClick;
                    this.groupsToolStripMenuItem.DropDownItems.Remove(menuItem);
                }
            }

            private void AddGroupMenuItems(IGroup group)
            {
                ToolStripMenuItem groupToolStripMenuItem = new GroupMenuItem(group, false);
                groupToolStripMenuItem.Click += new EventHandler(this.groupToolStripMenuItemClick);
                groupsToolStripMenuItem.DropDownItems.Add(groupToolStripMenuItem);

                ToolStripMenuItem groupAddToolStripMenuItem = new GroupMenuItem(group, false);
                groupAddToolStripMenuItem.Click += new EventHandler(this.groupAddToolStripMenuItemClick);
                addTerminalToGroupToolStripMenuItem.DropDownItems.Add(groupAddToolStripMenuItem);
            }
        }
    }
}