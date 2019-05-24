using System;
using System.Windows.Forms;

namespace Terminals.Network.WhoIs
{
    internal partial class WhoIs : UserControl
    {
        public WhoIs()
        {
            this.InitializeComponent();
        }

        private void whoisButton_Click(object sender, EventArgs e)
        {
            var server = this.hostTextbox.Text.Trim();
            if (server != string.Empty)
            {
                if (!server.StartsWith("=") && !server.ToLower().EndsWith(".ca"))
                    server = "=" + server;

                var result = WhoisResolver.Whois(server);
                result = result.Replace("\n", Environment.NewLine);
                var pos = result.IndexOf("Whois Server:");
                if (pos > 0)
                {
                    var newServer = result.Substring(pos + 13, result.IndexOf("\r\n", pos) - pos - 13);
                    if (server.StartsWith("="))
                        server = this.hostTextbox.Text.Trim();

                    var newResults = WhoisResolver.Whois(server, newServer.Trim());
                    if (!string.IsNullOrEmpty(newResults))
                        newResults = newResults.Replace("\n", Environment.NewLine);
                    ;

                    result = string.Format(
                        "{0}\r\n----------------------Sub Query:{1}--------------------------\r\n{2}", result,
                        newServer, newResults);
                }

                this.textBox2.Text = result;
            }
        }

        private void hostTextbox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                this.whoisButton_Click(null, null);
        }
    }
}