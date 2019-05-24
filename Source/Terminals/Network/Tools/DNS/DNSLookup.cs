using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;
using Bdev.Net.Dns;
using Terminals.Network.DNS;

namespace Terminals.Network
{
    internal partial class DNSLookup : UserControl
    {
        public DNSLookup()
        {
            this.InitializeComponent();
        }

        public void ForceDNS(string Host)
        {
            this.hostnameTextBox.Text = Host;
            this.lookupButton_Click(null, null);
        }

        private void lookupButton_Click(object sender, EventArgs e)
        {
            var serverIP = this.serverComboBox.Text.Trim();
            if (serverIP == "") serverIP = "128.8.10.90";
            this.serverComboBox.Text = serverIP.Trim();
            var domain = this.hostnameTextBox.Text.Trim();
            if (domain == "") domain = "codeplex.com";
            this.hostnameTextBox.Text = domain.Trim();

            try
            {
                var responses = new List<Answer>();

                var dnsServer = IPAddress.Parse(serverIP);
                // create a DNS request
                var request = new Request();
                request.AddQuestion(new Question(domain, DnsType.ANAME, DnsClass.IN));
                responses.Add(Resolver.Lookup(request, dnsServer).Answers[0]);

                request = new Request();
                request.AddQuestion(new Question(domain, DnsType.MX, DnsClass.IN));
                responses.Add(Resolver.Lookup(request, dnsServer).Answers[0]);

                request = new Request();
                request.AddQuestion(new Question(domain, DnsType.NS, DnsClass.IN));
                responses.Add(Resolver.Lookup(request, dnsServer).Answers[0]);

                request = new Request();
                request.AddQuestion(new Question(domain, DnsType.SOA, DnsClass.IN));
                responses.Add(Resolver.Lookup(request, dnsServer).Answers[0]);

                this.dataGridView1.DataSource = responses;
                //this.propertyGrid1.SelectedObject = records;
                // send it to the DNS server and get the response
                //
                //this.dataGridView1.DataSource = response.Answers;
            }
            catch (Exception exc)
            {
                Logging.Info("Could not resolve host.", exc);
                MessageBox.Show("Could not resolve host.");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void DNSLookup_Load(object sender, EventArgs e)
        {
            this.serverComboBox.DataSource = AdapterInfo.DNSServers;
        }

        private void hostnameTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                this.lookupButton_Click(null, null);
        }
    }

    public class IPRender
    {
        public IPAddress address;

        public string Address => this.address.ToString();

        public string AddressFamily => this.address.AddressFamily.ToString();

        public bool IsIPv6LinkLocal => this.address.IsIPv6LinkLocal;

        public bool IsIPv6Multicast => this.address.IsIPv6Multicast;

        public bool IsIPv6SiteLocal => this.address.IsIPv6SiteLocal;
    }

    public class KnownAlias
    {
        public string Alias { get; set; }
    }
}