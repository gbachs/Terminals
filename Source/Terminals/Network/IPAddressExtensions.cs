using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;

namespace Terminals.Network
{
    internal static class IPAddressExtensions
    {
        #region Public methods

        public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
        {
            var ipAdressBytes = address.GetAddressBytes();
            var subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            var broadcastAddress = new byte[ipAdressBytes.Length];
            for (var i = 0; i < broadcastAddress.Length; i++)
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));

            return new IPAddress(broadcastAddress);
        }

        public static IPAddress GetNetworkAddress(this IPAddress address, IPAddress subnetMask)
        {
            var ipAdressBytes = address.GetAddressBytes();
            var subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            var broadcastAddress = new byte[ipAdressBytes.Length];
            for (var i = 0; i < broadcastAddress.Length; i++)
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & subnetMaskBytes[i]);

            return new IPAddress(broadcastAddress);
        }

        public static bool IsInSameSubnet(this IPAddress address2, IPAddress address, IPAddress subnetMask)
        {
            var network1 = address.GetNetworkAddress(subnetMask);
            var network2 = address2.GetNetworkAddress(subnetMask);

            return network1.Equals(network2);
        }

        /// <summary>
        ///     Get all IP addresses between a start and an end IP address
        /// </summary>
        /// <param name="startIP">Starting IP address.</param>
        /// <param name="endIP">Ending IP address</param>
        /// <returns>IP address collection.</returns>
        /// <example>
        ///     foreach (String address in GetIPRange(startIP, endIP))
        ///     Console.WriteLine(address);
        /// </example>
        public static IEnumerable<IPAddress> GetIPRange(IPAddress startIP, IPAddress endIP)
        {
            var sIP = IpToUint(startIP.GetAddressBytes());
            var eIP = IpToUint(endIP.GetAddressBytes());

            while (sIP <= eIP)
            {
                yield return new IPAddress(ReverseBytesArray(sIP));
                sIP++;
            }
        }

        /// <summary>
        ///     Get the next IP address, adding one hop.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static IPAddress GetNextIPAddress(IPAddress address)
        {
            var sIP = IpToUint(address.GetAddressBytes());
            sIP++;
            return new IPAddress(ReverseBytesArray(sIP));
        }

        /// <summary>
        ///     Get the previous IP address, substracting one hop.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static IPAddress GetPreviousIPAddress(IPAddress address)
        {
            var sIP = IpToUint(address.GetAddressBytes());
            sIP--;
            return new IPAddress(ReverseBytesArray(sIP));
        }

        #endregion

        #region Private methods

        /// <summary>
        ///     Reverse byte order in array
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private static uint ReverseBytesArray(uint ip)
        {
            var bytes = BitConverter.GetBytes(ip);
            bytes = bytes.Reverse().ToArray();
            return (uint)BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        ///     Convert bytes array to 32 bit long value
        /// </summary>
        /// <param name="ipBytes"></param>
        /// <returns></returns>
        private static uint IpToUint(IEnumerable<byte> ipBytes)
        {
            var bConvert = new ByteConverter();
            uint ipUint = 0;

            var shift = 24; // indicates number of bits left for shifting
            foreach (var b in ipBytes)
            {
                if (ipUint == 0)
                {
                    var convertTo = bConvert.ConvertTo(b, typeof(uint));
                    if (convertTo != null)
                        ipUint = (uint)convertTo << shift;

                    shift -= 8;
                    continue;
                }

                if (shift >= 8)
                {
                    var convertTo = bConvert.ConvertTo(b, typeof(uint));
                    if (convertTo != null)
                        ipUint += (uint)convertTo << shift;
                }
                else
                {
                    var to = bConvert.ConvertTo(b, typeof(uint));
                    if (to != null)
                        ipUint += (uint)to;
                }

                shift -= 8;
            }

            return ipUint;
        }

        #endregion
    }
}