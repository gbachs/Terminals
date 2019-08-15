using System;
using System.Management;
using System.Text;

namespace Terminals.Network.DNS
{
    internal class Adapter
    {
        #region Public Properties

        public ManagementObject PropertyData { get; set; }

        public bool ArpAlwaysSourceRoute => this.ToBoolean("ArpAlwaysSourceRoute");

        public bool ArpUseEtherSNAP => this.ToBoolean("ArpUseEtherSNAP");

        public string Caption => this.ToString("Caption");

        public string DatabasePath => this.ToString("DatabasePath");

        public bool DeadGWDetectEnabled => this.ToBoolean("DeadGWDetectEnabled");

        public string[] DefaultIPGateway => this.ToStringArray("DefaultIPGateway");

        public string DefaultIPGatewayList => ToStringList(this.DefaultIPGateway);

        public byte DefaultTOS => this.ToByte("DefaultTOS");

        public byte DefaultTTL => this.ToByte("DefaultTTL");

        public string Description => this.ToString("Description");

        public bool DHCPEnabled => this.ToBoolean("DHCPEnabled");

        public DateTime DHCPLeaseExpires => this.ToDateTime("DHCPLeaseExpires");

        public DateTime DHCPLeaseObtained => this.ToDateTime("DHCPLeaseObtained");

        public string DHCPServer => this.ToString("DHCPServer");

        public string DNSDomain => this.ToString("DNSDomain");

        public string DNSDomainSuffixSearchOrder
        {
            get
            {
                try
                {
                    var dns = PropertyData.Properties["DNSDomainSuffixSearchOrder"]?.Value;
                    if (dns == null)
                        return string.Empty;

                    var dnsList = dns as Array;
                    if (dnsList == null)
                        return dns.ToString();

                    var sb = new StringBuilder();
                    foreach (var o in dnsList)
                    {
                        sb.Append(o);
                        sb.Append(",");
                    }

                    return sb.ToString().TrimEnd(',');
                }
                catch (Exception exc)
                {
                    Logging.Error("see: http://terminals.codeplex.com/workitem/20748", exc);
                    return string.Empty;
                }
            }
        }

        public bool DNSEnabledForWINSResolution => this.ToBoolean("DNSEnabledForWINSResolution");

        public string DNSHostName => this.ToString("DNSHostName");

        public string[] DNSServerSearchOrder => this.ToStringArray("DNSServerSearchOrder");

        public string DNSServerSearchOrderList => ToStringList(this.DNSServerSearchOrder);

        public bool DomainDNSRegistrationEnabled => this.ToBoolean("DomainDNSRegistrationEnabled");

        public uint ForwardBufferMemory => this.ToUInt32("ForwardBufferMemory");

        public bool FullDNSRegistrationEnabled => this.ToBoolean("FullDNSRegistrationEnabled");

        public ushort[] GatewayCostMetric => this.ToUInt16Array("GatewayCostMetric");

        public string GatewayCostMetricList => ToStringList(this.GatewayCostMetric);

        public uint IGMPLevel => this.ToUInt32("IGMPLevel");

        public uint Index => this.ToUInt32("Index");

        public string[] IPAddress => this.ToStringArray("IPAddress");

        public string IPAddressList => ToStringList(this.IPAddress);

        public uint IPConnectionMetric => this.ToUInt32("IPConnectionMetric");

        public bool IPEnabled => this.ToBoolean("IPEnabled");

        public bool IPFilterSecurityEnabled => this.ToBoolean("IPFilterSecurityEnabled");

        public bool IPPortSecurityEnabled => this.ToBoolean("IPPortSecurityEnabled");

        public string[] IPSecPermitIPProtocols => this.ToStringArray("IPSecPermitIPProtocols");

        public string IPSecPermitIPProtocolsList => ToStringList(this.IPSecPermitIPProtocols);

        public string[] IPSecPermitTCPPorts => this.ToStringArray("IPSecPermitTCPPorts");

        public string IPSecPermitTCPPortsList => ToStringList(this.IPSecPermitTCPPorts);

        public string[] IPSecPermitUDPPorts => this.ToStringArray("IPSecPermitUDPPorts");

        public string IPSecPermitUDPPortsList => ToStringList(this.IPSecPermitUDPPorts);

        public string[] IPSubnet => this.ToStringArray("IPSubnet");

        public string IPSubnetList => ToStringList(this.IPSubnet);

        public bool IPUseZeroBroadcast => this.ToBoolean("IPUseZeroBroadcast");

        public string IPXAddress => this.ToString("IPXAddress");

        public bool IPXEnabled => this.ToBoolean("IPXEnabled");

        public uint IPXFrameType => this.ToUInt32("IPXFrameType");

        public uint IPXMediaType => this.ToUInt32("IPXMediaType");

        public string IPXNetworkNumber => this.ToString("IPXNetworkNumber");

        public string IPXVirtualNetNumber => this.ToString("IPXVirtualNetNumber");

        public uint KeepAliveInterval => this.ToUInt32("KeepAliveInterval");

        public uint KeepAliveTime => this.ToUInt32("KeepAliveTime");

        public string MACAddress => this.ToString("MACAddress");

        public uint MTU => this.ToUInt32("MTU");

        public uint NumForwardPackets => this.ToUInt32("NumForwardPackets");

        public bool PMTUBHDetectEnabled => this.ToBoolean("PMTUBHDetectEnabled");

        public bool PMTUDiscoveryEnabled => this.ToBoolean("PMTUDiscoveryEnabled");

        public string ServiceName => this.ToString("ServiceName");

        public string SettingID => this.ToString("SettingID");

        public uint TcpipNetbiosOptions => this.ToUInt32("TcpipNetbiosOptions");

        public uint TcpMaxConnectRetransmissions => this.ToUInt32("TcpMaxConnectRetransmissions");

        public uint TcpMaxDataRetransmissions => this.ToUInt32("TcpMaxDataRetransmissions");

        public uint TcpNumConnections => this.ToUInt32("TcpNumConnections");

        public bool TcpUseRFC1122UrgentPointer => this.ToBoolean("TcpUseRFC1122UrgentPointer");

        public ushort TcpWindowSize => this.ToUInt16("TcpWindowSize");

        public bool WINSEnableLMHostsLookup => this.ToBoolean("WINSEnableLMHostsLookup");

        public string WINSHostLookupFile => this.ToString("WINSHostLookupFile");

        public string WINSPrimaryServer => this.ToString("WINSPrimaryServer");

        public string WINSScopeID => this.ToString("WINSScopeID");

        public string WINSSecondaryServer => this.ToString("WINSSecondaryServer");

        #endregion

        #region Private methods developer made

        private string ToString(string property)
        {
            try
            {
                var value = this.PropertyData.Properties[property].Value;
                return value == null ? string.Empty : value.ToString();
            }
            catch (Exception ex)
            {
                LogPropertyConversion(ex, property);
                return string.Empty;
            }
        }

        private bool ToBoolean(string property)
        {
            try
            {
                var value = this.PropertyData.Properties[property].Value;
                return value == null ? false : Convert.ToBoolean(value);
            }
            catch (Exception ex)
            {
                LogPropertyConversion(ex, property);
                return false;
            }
        }

        private string[] ToStringArray(string property)
        {
            try
            {
                var value = this.PropertyData.Properties[property].Value;
                return value == null ? new string[] { } : (string[])value;
            }
            catch (Exception ex)
            {
                LogPropertyConversion(ex, property);
                return new string[] { };
            }
        }

        private byte ToByte(string property)
        {
            try
            {
                var value = this.PropertyData.Properties[property].Value;
                return value == null ? Convert.ToByte(0) : Convert.ToByte(value);
            }
            catch (Exception ex)
            {
                LogPropertyConversion(ex, property);
                return Convert.ToByte(0);
            }
        }

        private DateTime ToDateTime(string property)
        {
            try
            {
                var value = this.PropertyData.Properties[property].Value;
                return value == null ? DateTime.MinValue : this.GetDateTime(value.ToString());
            }
            catch (Exception ex)
            {
                LogPropertyConversion(ex, property);
                return DateTime.MinValue;
            }
        }

        private ushort ToUInt16(string property)
        {
            try
            {
                var value = this.PropertyData.Properties[property].Value;
                return value == null ? Convert.ToUInt16(0) : Convert.ToUInt16(value);
            }
            catch (Exception ex)
            {
                LogPropertyConversion(ex, property);
                return Convert.ToUInt16(0);
            }
        }

        private ushort[] ToUInt16Array(string property)
        {
            try
            {
                var value = this.PropertyData.Properties[property].Value;
                return value == null ? new ushort[] { } : (ushort[])value;
            }
            catch (Exception ex)
            {
                LogPropertyConversion(ex, property);
                return new ushort[] { };
            }
        }

        private uint ToUInt32(string property)
        {
            try
            {
                var value = this.PropertyData.Properties[property].Value;
                return value == null ? 0 : Convert.ToUInt32(value);
            }
            catch (Exception ex)
            {
                LogPropertyConversion(ex, property);
                return 0;
            }
        }

        private static void LogPropertyConversion(Exception ex, string propertyName)
        {
            var message = string.Format("Unable to get property '{0}' value in DNS Adapter", propertyName);
            Logging.Error(message, ex);
        }

        private static string ToStringList(string[] stringArray)
        {
            var x = 1;
            var sb = new StringBuilder();
            foreach (var s in stringArray)
            {
                sb.Append(s);
                if (x < stringArray.Length)
                    sb.Append(",");

                x++;
            }

            return sb.ToString();
        }

        private static string ToStringList(ushort[] stringArray)
        {
            var x = 1;
            var sb = new StringBuilder();
            foreach (var s in stringArray)
            {
                sb.Append(s);
                if (x < stringArray.Length)
                    sb.Append(",");

                x++;
            }

            return sb.ToString();
        }

        // There is a utility called mgmtclassgen that ships with the .NET SDK that
        // will generate managed code for existing WMI classes. It also generates
        // datetime conversion routines like this one.
        // Thanks to Chetan Parmar and dotnet247.com for the help.
        private DateTime GetDateTime(string dmtfDate)
        {
            var year = DateTime.Now.Year;
            var month = 1;
            var day = 1;
            var hour = 0;
            var minute = 0;
            var second = 0;
            var millisec = 0;
            var dmtf = dmtfDate;
            var tempString = string.Empty;

            if (string.IsNullOrEmpty(dmtf))
                return DateTime.MinValue;

            if (dmtf.Length != 25)
                return DateTime.MinValue;

            tempString = dmtf.Substring(0, 4);
            if ("****" != tempString)
                year = int.Parse(tempString);

            tempString = dmtf.Substring(4, 2);
            if ("**" != tempString)
                month = int.Parse(tempString);

            tempString = dmtf.Substring(6, 2);
            if ("**" != tempString)
                day = int.Parse(tempString);

            tempString = dmtf.Substring(8, 2);
            if ("**" != tempString)
                hour = int.Parse(tempString);

            tempString = dmtf.Substring(10, 2);
            if ("**" != tempString)
                minute = int.Parse(tempString);

            tempString = dmtf.Substring(12, 2);
            if ("**" != tempString)
                second = int.Parse(tempString);

            tempString = dmtf.Substring(15, 3);
            if ("***" != tempString)
                millisec = int.Parse(tempString);

            var dateRet = new DateTime(year, month, day, hour, minute, second, millisec);
            return dateRet;
        }

        #endregion
    }
}