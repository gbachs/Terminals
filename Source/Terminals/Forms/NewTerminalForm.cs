using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Windows.Forms;
using Terminals.Configuration;
using Terminals.Connections;
using Terminals.Data;
using Terminals.Forms;

namespace Terminals
{
    /// <summary>
    /// Copy of related dialog, from which user controls will be extracted
    /// </summary>
    internal partial class NewTerminalForm : Form, INewTerminalForm
    {
        private readonly Settings settings = Settings.Instance;

        private NewTerminalFormValidator validator;

        private readonly ConnectionManager connectionManager;

        public Guid EditedId { get; private set; }

        public string ProtocolText => this.favoritePropertiesControl1.ProtocolText;

        public string ServerNameText => this.favoritePropertiesControl1.ServerNameText;

        public string PortText => this.favoritePropertiesControl1.PortText;

        public bool EditingNew => this.EditedId == Guid.Empty;

        private readonly IPersistence persistence;

        private readonly FavoriteIcons favoriteIcons;

        private IFavorites PersistedFavorites => this.persistence.Favorites;

        private new TerminalFormDialogResult DialogResult { get; set; }

        internal IFavorite Favorite { get; private set; }

        public NewTerminalForm(IPersistence persistence, ConnectionManager connectionManager, FavoriteIcons favoriteIcons, string serverName)
            : this(persistence, connectionManager, favoriteIcons)
        {
            this.Init(serverName);
        }

        public NewTerminalForm(IPersistence persistence, ConnectionManager connectionManager, FavoriteIcons favoriteIcons, IFavorite favorite)
            : this(persistence, connectionManager, favoriteIcons)
        {
            this.Init(favorite);
        }

        private NewTerminalForm(IPersistence persistence, ConnectionManager connectionManager, FavoriteIcons favoriteIcons)
        {
            this.persistence = persistence;
            this.connectionManager = connectionManager;
            this.favoriteIcons = favoriteIcons;
            this.InitializeComponent();
            this.InitializeFavoritePropertiesControl();

        }

        private void InitializeFavoritePropertiesControl()
        {
            this.validator = new NewTerminalFormValidator(this.persistence, this.connectionManager, this);
            this.favoritePropertiesControl1.AssignServices(this.persistence, this.connectionManager, this.favoriteIcons);
            this.favoritePropertiesControl1.SetOkButtonRequested += this.GeneralProperties_SetOkButtonRequested;
            this.favoritePropertiesControl1.RegisterValidations(this.validator);
            this.favoritePropertiesControl1.SetErrorProviderIconsAlignment(this.errorProvider);
            this.favoritePropertiesControl1.LoadContent();
        }

        private void GeneralProperties_SetOkButtonRequested(object sender, EventArgs e)
        {
            this.SetOkButtonState();
        }

        private void NewTerminalForm_Shown(object sender, EventArgs e)
        {
            this.favoritePropertiesControl1.FocusServers();
        }

        /// <summary>
        /// Overload ShowDialog and return custom result.
        /// </summary>
        /// <returns>Returns custom dialogresult.</returns>
        public new TerminalFormDialogResult ShowDialog()
        {
            base.ShowDialog();

            return this.DialogResult;
        }

        /// <summary>
        /// Save favorite and close form. If the form isnt valid the form control is focused.
        /// </summary>
        private void BtnSave_Click(object sender, EventArgs e)
        {
            this.favoritePropertiesControl1.SaveMRUs();
            if (this.FillFavorite(false))
            {
                this.DialogResult = TerminalFormDialogResult.SaveAndClose;
                this.Close();
            }
        }

        private void BtnSaveDefault_Click(object sender, EventArgs e)
        {
            this.contextMenuStripDefaults.Show(this.btnSaveDefault, 0, this.btnSaveDefault.Height);
        }

        private void RemoveSavedDefaultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settings.RemoveDefaultFavorite();
        }

        /// <summary>
        /// Save favorite, close form and immediatly connect to the favorite.
        /// </summary>
        private void SaveConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.favoritePropertiesControl1.SaveMRUs();

            if (this.FillFavorite(false))
                this.DialogResult = TerminalFormDialogResult.SaveAndConnect;

            this.Close();
        }

        /// <summary>
        /// Save favorite and copy the current favorite settings, except favorite and connection name.
        /// </summary>
        private void SaveCopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.favoritePropertiesControl1.SaveMRUs();
            if (this.FillFavorite(false))
            {
                this.EditedId = Guid.Empty;
                this.favoritePropertiesControl1.ResetServerNameControls(this.Favorite.Name);
            }
        }

        private void SaveCurrentSettingsAsDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.FillFavorite(true);
        }

        /// <summary>
        /// Save favorite and clear form for a new favorite.
        /// </summary>
        private void SaveNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.favoritePropertiesControl1.SaveMRUs();
            if (this.FillFavorite(false))
            {
                this.EditedId = Guid.Empty;
                this.Init(String.Empty);
                this.favoritePropertiesControl1.FocusServers();
            }
        }

        private void Init(IFavorite favorite)
        {
            this.InitMruAndButtons();
            this.EditedId = favorite.Id;
            this.Text = "Edit Connection";
            this.favoritePropertiesControl1.LoadFrom(favorite);
        }

        private void Init(string serverName)
        {
            this.InitMruAndButtons();
            this.favoritePropertiesControl1.FillCredentialsCombobox(Guid.Empty);

            var defaultSavedFavorite = this.settings.GetDefaultFavorite();
            if (defaultSavedFavorite != null)
            {
                var defaultFavorite = ModelConverterV1ToV2.ConvertToFavorite(defaultSavedFavorite, this.persistence, this.connectionManager);
                this.favoritePropertiesControl1.LoadFrom(defaultFavorite);
            }

            this.favoritePropertiesControl1.FillServerName(serverName);
        }

        private void InitMruAndButtons()
        {
            this.favoritePropertiesControl1.LoadMRUs();
            this.SetOkButtonState();
        }

        internal void AssingSelectedGroup(IGroup group)
        {
            this.favoritePropertiesControl1.AssingSelectedGroup(group);
        }

        private Boolean FillFavorite(Boolean defaultFav)
        {
            try
            {
                var isValid = this.validator.Validate();
                if (!isValid)
                    return false;

                var favorite = ResolveFavortie();
                this.FillFavoriteFromControls(favorite);

                if (defaultFav)
                    SaveDefaultFavorite(favorite);
                else
                    this.CommitFavoriteChanges(favorite);

                return true;
            }
            catch (DbEntityValidationException entityValidation)
            {
                EntityLogValidationErrors(entityValidation);
                this.ShowErrorMessageBox("Unable to save favorite, because database constrains are not satisfied");
                return false;
            }
            catch (Exception e)
            {
                Logging.Info("Fill Favorite Failed", e);
                this.ShowErrorMessageBox(e.Message);
                return false;
            }
        }

        private static void EntityLogValidationErrors(DbEntityValidationException entityValidation)
        {
            Logging.Error("Entity exception", entityValidation);
            foreach (var validationResult in entityValidation.EntityValidationErrors)
            {
                foreach (var propertyError in validationResult.ValidationErrors)
                {
                    Logging.Error(string.Format("Validation error '{0}': {1}", propertyError.PropertyName, propertyError.ErrorMessage));
                }
            }
        }

        /// <summary>
        /// Overwrites favortie property by favorite stored in persistence
        /// or newly created one
        /// </summary>
        private IFavorite ResolveFavortie()
        {
            IFavorite favorite = null; // force favorite property reset
            if (!this.EditedId.Equals(Guid.Empty))
                favorite = PersistedFavorites[this.EditedId];
            if (favorite == null)
                favorite = this.persistence.Factory.CreateFavorite();
            this.Favorite = favorite;
            return favorite;
        }

        public void FillFavoriteFromControls(IFavorite favorite)
        {
            this.favoritePropertiesControl1.SaveTo(favorite);
        }

        private void SaveDefaultFavorite(IFavorite favorite)
        {
            favorite.Name = String.Empty;
            favorite.ServerName = String.Empty;
            favorite.Notes = String.Empty;
            // to reset we dont need to go through encryption
            favorite.Security.EncryptedDomain = String.Empty;
            favorite.Security.EncryptedUserName = String.Empty;
            favorite.Security.EncryptedPassword = String.Empty;

            var defaultFavorite = ModelConverterV2ToV1.ConvertToFavorite(favorite, this.persistence, this.connectionManager);
            defaultFavorite.EnableSecuritySettings = false;
            defaultFavorite.SecurityWorkingFolder = string.Empty;
            defaultFavorite.SecurityStartProgram = string.Empty;
            defaultFavorite.SecurityFullScreen = false;

            settings.SaveDefaultFavorite(defaultFavorite);
        }

        private void CommitFavoriteChanges(IFavorite favorite)
        {
            settings.StartDelayedUpdate();
            this.persistence.StartDelayedUpdate();

            if (this.EditingNew)
                this.AddToPersistence(favorite);
            else
                settings.EditFavoriteButton(this.EditedId, favorite.Id, this.favoritePropertiesControl1.ShowOnToolbar);

            var updatedGroups = this.favoritePropertiesControl1.GetNewlySelectedGroups();
            PersistedFavorites.UpdateFavorite(favorite, updatedGroups);
            this.persistence.SaveAndFinishDelayedUpdate();
            settings.SaveAndFinishDelayedUpdate();
        }

        private void AddToPersistence(IFavorite favorite)
        {
            this.PersistedFavorites.Add(favorite);
            if (this.favoritePropertiesControl1.ShowOnToolbar)
                settings.AddFavoriteButton(favorite.Id);
        }

        private void ShowErrorMessageBox(string message)
        {
            MessageBox.Show(this, message, Program.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void SetErrorInfo(Control target, string message)
        {
            if (target == null)
                return;

            this.errorProvider.SetError(target, message);
        }

        private void SetOkButtonState()
        {
            if (this.favoritePropertiesControl1.UrlVisible)
            {
                this.btnSave.Enabled = this.validator.IsUrlValid();
            }
            else
            {
                this.btnSave.Enabled = !this.validator.IsServerNameEmpty();
            }
        }

        public Uri GetFullUrlFromHttpTextBox()
        {
            return this.favoritePropertiesControl1.GetFullUrlFromHttpTextBox();
        }
    }
}
