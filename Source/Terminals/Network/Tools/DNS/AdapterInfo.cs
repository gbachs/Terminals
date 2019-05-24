using System;
using System.Collections.Generic;
using System.Management;

namespace Terminals.Network.DNS
{
    internal class AdapterInfo
    {
        public static List<string> DNSServers
        {
            get
            {
                var servers = new List<string>();
                try
                {
                    var adapters = GetAdapters();
                    foreach (var a in adapters)
                        if (a.IPEnabled)
                            if (a.DNSServerSearchOrder != null)
                                foreach (var server in a.DNSServerSearchOrder)
                                    servers.Add(server);
                }
                catch (Exception exc)
                {
                    Logging.Error("DNS Server Lookup Failed (WMI)", exc);
                }

                return servers;
            }
        }

        public static List<Adapter> GetAdapters()
        {
            var adapterList = new List<Adapter>();

            ManagementObjectSearcher searcher;
            var q = new ObjectQuery("SELECT * FROM Win32_NetworkAdapterConfiguration");
            searcher = new ManagementObjectSearcher(q);
            foreach (ManagementObject share in searcher.Get())
            {
                var ad = new Adapter();
                ad.PropertyData = share;
                adapterList.Add(ad);
            }

            return adapterList;
        }
    }
}