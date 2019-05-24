using System.Windows.Forms;

namespace Terminals.Forms
{
    /// <summary>
    ///     General purpose password change dialog,
    ///     where user enters old and two times new password. Checks, if the new password confirm matches
    /// </summary>
    internal partial class ChangePassword : Form
    {
        public ChangePassword()
        {
            this.InitializeComponent();
        }

        internal string OldPassword => this.txtOldPassword.Text;

        internal string NewPassword => this.txtNewPassword.Text;

        private bool ConfirmedPasswordMatch =>
            this.DialogResult == DialogResult.OK &&
            this.txtConfirmPassword.Text == this.txtNewPassword.Text;

        private void ChangePassword_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.Cancel)
                return;

            if (!this.ConfirmedPasswordMatch)
            {
                e.Cancel = true;
                MessageBox.Show("New password doesn't match", "Database password change",
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }
    }
}