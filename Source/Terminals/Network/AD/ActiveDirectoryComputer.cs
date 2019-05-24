using System;
using System.DirectoryServices;
using Terminals.Common.Connections;
using Terminals.Connections;

namespace Terminals.Network
{
    internal class ActiveDirectoryComputer
    {
        private const string NAME = "name";

        private const string OS = "operatingSystem";

        private const string DN = "distinguishedName";

        internal ActiveDirectoryComputer()
        {
            this.Protocol = KnownConnectionConstants.RDP;
            this.ComputerName = string.Empty;
            this.OperatingSystem = string.Empty;
            this.Tags = string.Empty;
            this.Notes = string.Empty;
        }

        // public required by databinding
        public string ComputerName { get; set; }

        public string OperatingSystem { get; set; }

        internal string Protocol { get; set; }

        internal string Tags { get; set; }

        internal string Notes { get; set; }

        internal static ActiveDirectoryComputer FromDirectoryEntry(string domain, DirectoryEntry computer)
        {
            var comp = new ActiveDirectoryComputer();
            comp.Tags = domain;

            if (computer.Properties != null)
            {
                comp.NameFromEntry(computer);
                comp.OperationSystemFromEntry(computer);
                comp.DistinquishedNameFromEntry(computer);
            }

            return comp;
        }

        private void NameFromEntry(DirectoryEntry computer)
        {
            var nameValues = computer.Properties[NAME];
            var name = computer.Name.Replace("CN=", "");
            if (nameValues != null && nameValues.Count > 0)
                name = nameValues[0].ToString();
            this.ComputerName = name;
        }

        private void OperationSystemFromEntry(DirectoryEntry computer)
        {
            var osValues = computer.Properties[OS];
            if (osValues != null && osValues.Count > 0)
            {
                this.Tags += "," + osValues[0];
                this.OperatingSystem = osValues[0].ToString();
            }
        }

        private void DistinquishedNameFromEntry(DirectoryEntry computer)
        {
            var dnameValues = computer.Properties[DN];
            if (dnameValues != null && dnameValues.Count > 0)
            {
                var distinguishedName = dnameValues[0].ToString();
                if (distinguishedName.Contains("OU=Domain Controllers"))
                    this.Tags += ",Domain Controllers";
            }
        }

        internal FavoriteConfigurationElement ToFavorite(ConnectionManager connectionManager, string domain)
        {
            var favorite = new FavoriteConfigurationElement(this.ComputerName);
            favorite.Name = this.ComputerName;
            favorite.ServerName = this.ComputerName;
            favorite.UserName = Environment.UserName;
            favorite.DomainName = domain;
            favorite.Tags = this.Tags;
            favorite.Port = connectionManager.GetPort(this.Protocol);
            favorite.Protocol = this.Protocol;
            favorite.Notes = this.Notes;
            return favorite;
        }
    }
}