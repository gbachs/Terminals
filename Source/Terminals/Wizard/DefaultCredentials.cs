using System;
using System.Windows.Forms;
using Terminals.Configuration;

namespace Terminals.Wizard
{
    internal partial class DefaultCredentials : UserControl
    {
        public DefaultCredentials()
        {
            InitializeComponent();

            var settings = Settings.Instance;
            this.domainTextbox.Text = settings.DefaultDomain;
            this.passwordTextbox.Text = settings.DefaultPassword;
            this.usernameTextbox.Text = settings.DefaultUsername;

            if(Environment.UserDomainName != Environment.MachineName) {
                if(this.domainTextbox.Text == "") this.domainTextbox.Text = Environment.UserDomainName;
            }
            if(this.usernameTextbox.Text == "") this.usernameTextbox.Text = Environment.UserName;
        }

        public string DefaultDomain => this.domainTextbox.Text;

        public string DefaultPassword => this.passwordTextbox.Text;

        public string DefaultUsername => this.usernameTextbox.Text;
    }
}
