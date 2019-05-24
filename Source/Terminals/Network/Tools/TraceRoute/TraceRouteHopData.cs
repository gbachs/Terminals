using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Terminals.Network
{
    /// <summary>
    ///     Contains data of a trace routing hop.
    /// </summary>
    internal class TraceRouteHopData
    {
        /// <summary>
        ///     Constructs a new object from the IPAddress of the node and the round trip time taken
        /// </summary>
        /// <param name="hopCount">Number of hops to the destination</param>
        /// <param name="address">The IP address of the hop</param>
        /// <param name="roundTripTime">The roundtriptime it takes to the hop</param>
        /// <param name="status">The hop IP status</param>
        public TraceRouteHopData(byte hopCount, IPAddress address, long roundTripTime, IPStatus status,
            bool resolveNames)
        {
            this.Count = hopCount;
            this.Address = address;
            this.RoundTripTime = roundTripTime;
            this.Status = status;
            this.TryToResolveHostName(resolveNames);
        }

        /// <summary>
        ///     Gets or sets the hop count.
        /// </summary>
        public byte Count { get; set; }

        /// <summary>
        ///     Gets or sets the IP address for the hop.
        /// </summary>
        public IPAddress Address { get; set; }

        /// <summary>
        ///     Gets or sets the time taken to go to the hop and come back to the originating node in milliseconds.
        /// </summary>
        public long RoundTripTime { get; set; }

        /// <summary>
        ///     Gets or sets the IPStatus of request send to the hope.
        /// </summary>
        public IPStatus Status { get; set; }

        /// <summary>
        ///     Gets or sets the resolved hostname for the IP address of the hop.
        /// </summary>
        public string HostName { get; set; }

        private void TryToResolveHostName(bool resolveNames)
        {
            try
            {
                if (this.Status != IPStatus.Success || !resolveNames)
                    return;
                var entry = Dns.GetHostEntry(this.Address);
                this.HostName = entry.HostName;
            }
            catch (SocketException)
            {
                // No such host is known error.
                this.HostName = string.Empty;
            }
        }
    }
}