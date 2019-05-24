namespace Terminals.Network
{
    /// <summary>
    ///     Represents data from ping reply.
    /// </summary>
    internal class PingReplyData
    {
        public PingReplyData(long count, string status, string hostname, string destination, int bytes, int ttl,
            long roundTripTime)
        {
            this.Count = count;
            this.Status = status;
            this.Hostname = hostname;
            this.Destination = destination;
            this.Bytes = bytes;
            this.TimeToLive = ttl;
            this.RoundTripTime = roundTripTime;
        }

        public long Count { get; set; }

        public string Status { get; set; }

        public string Hostname { get; set; }

        public string Destination { get; set; }

        public int Bytes { get; set; }

        public int TimeToLive { get; set; }

        public long RoundTripTime { get; set; }
    }
}