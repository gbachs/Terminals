using System.Net.Sockets;

namespace Terminals.Scanner
{
    internal class ConnectionState
    {
        internal ConnectionState(int port, TcpClient client)
        {
            this.Port = port;
            this.Client = client;
        }

        internal int Port { get; }

        internal TcpClient Client { get; }

        internal bool Done { get; set; }
    }
}