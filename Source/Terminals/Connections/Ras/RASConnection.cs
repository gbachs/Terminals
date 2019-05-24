using System;
using System.Windows.Forms;
using FalafelSoftware.TransPort;

namespace Terminals.Connections
{
    internal class RASConnection : Connection
    {
        public override bool Connected => this.ras.Connected;

        public Ras ras { get; set; }

        public override bool Connect()
        {
            try
            {
                this.ras = new Ras();
                var p = new RASProperties();
                p.RASConnection = this;
                p.Dock = DockStyle.Fill;
                this.Dock = DockStyle.Fill;
                this.Controls.Add(p);
                p.BringToFront();
                this.BringToFront();
                p.Parent = this.Parent;

                this.ras.SetModemSpeaker = false;
                this.ras.SetSoftwareCompression = false;
                this.ras.UsePrefixSuffix = false;
                this.ras.HangUpOnDestroy = true;

                this.ras.DialError += this.ras_DialError;
                this.ras.DialStatus += this.ras_DialStatus;
                this.ras.ConnectionChanged += this.ras_ConnectionChanged;
                this.ras.EntryName = this.Favorite.ServerName;

                var security = this.ResolveFavoriteCredentials();
                RasError error;
                if (!string.IsNullOrEmpty(security.UserName) && !string.IsNullOrEmpty(security.Password))
                {
                    this.Log("Using Terminals Credentials, Dialing...");
                    this.ras.UserName = security.UserName;
                    this.ras.Password = security.Password;
                    this.ras.Domain = security.Domain;
                    error = this.ras.Dial();
                }
                else
                {
                    this.Log("Terminals has no credentials, Showing Dial Dialog...");
                    error = this.ras.DialDialog();
                }

                this.Log("Dial Result:" + error);
                return error == RasError.Success;
            }
            catch (Exception exc)
            {
                Logging.Fatal("Connecting to RAS", exc);
                return false;
            }
        }

        private void ras_DialStatus(object sender, DialStatusEventArgs e)
        {
            this.Log("Status:" + e.ConnectionState);
        }

        private void ras_DialError(object sender, DialErrorEventArgs e)
        {
            if (e.RasError != RasError.Success)
            {
                this.Log("Error:" + e.RasError);
                MessageBox.Show("Could not connect to the server. Reason:" + e.RasError);
            }
        }

        private void ras_ConnectionChanged(object sender, ConnectionChangedEventArgs e)
        {
            this.Log("Connected:" + e.Connected);

            if (!e.Connected)
                this.FireDisconnected();
        }

        public void Disconnect()
        {
            this.Log("Hanging Up:" + this.ras.HangUp());
        }
    }
}