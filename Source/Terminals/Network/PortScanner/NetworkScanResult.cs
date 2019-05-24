using System;

namespace Terminals.Scanner
{
    internal class NetworkScanResult
    {
        public string IPAddress { get; set; }

        public string HostName { get; set; }

        internal int Port { get; set; }

        public string ServiceName { get; set; }

        public override string ToString()
        {
            return string.Format("NetworkScanResult:{0},{1},{2}",
                this.IPAddress, this.ServiceName, this.HostName);
        }

        internal FavoriteConfigurationElement ToFavorite(string tags)
        {
            var favorite = new FavoriteConfigurationElement();
            favorite.ServerName = this.IPAddress;
            favorite.Port = this.Port;
            favorite.Protocol = this.ServiceName;
            if (tags != string.Empty)
                favorite.Tags = tags;
            favorite.Name = string.Format("{0}_{1}", this.HostName, favorite.Protocol);
            favorite.DomainName = Environment.UserDomainName;
            favorite.UserName = Environment.UserName;
            return favorite;
        }
    }
}