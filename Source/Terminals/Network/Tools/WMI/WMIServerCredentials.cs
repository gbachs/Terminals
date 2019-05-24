using System;
using System.Windows.Forms;

namespace Terminals.Network.WMI
{
    internal partial class WMIServerCredentials : UserControl
    {
        public WMIServerCredentials()
        {
            this.InitializeComponent();
        }

        public string SelectedServer { get => this.comboBox1.Text; set => this.comboBox1.Text = value; }

        public string Username { get => this.UsernameTextbox.Text; set => this.UsernameTextbox.Text = value; }

        public string Password { get => this.PasswordTextbox.Text; set => this.PasswordTextbox.Text = value; }

        private void WMIServerCredentials_Load(object sender, EventArgs e)
        {
            if (Environment.UserDomainName != null && Environment.UserDomainName != "")
                this.UsernameTextbox.Text = string.Format(@"{0}\{1}", Environment.UserDomainName, Environment.UserName);
            else
                this.UsernameTextbox.Text = Environment.UserName;

            //try {
            //    foreach(FavoriteConfigurationElement elm in Settings.GetFavorites()) {
            //        this.comboBox1.Items.Add(elm.ServerName);
            //    }
            //} catch(Exception exc) {
            //    Terminals.Logging.Log.Error("WMI Server Credentials Favorite Query Failed", exc);
            //}
        }
    }
}