using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Terminals.Connections;
using Terminals.Data;
using Unified;

namespace Terminals.Network
{
    internal class Server
    {
        internal const int SERVER_PORT = 1216;

        private readonly ConnectionManager connectionManager;

        private readonly IPersistence persistence;

        private readonly TcpListener server = new TcpListener(IPAddress.Any, SERVER_PORT);

        public Server(IPersistence persistence, ConnectionManager connectionManager)
        {
            this.persistence = persistence;
            this.connectionManager = connectionManager;
        }

        public bool ServerOnline { get; private set; }

        public void Stop()
        {
            this.ServerOnline = false;
        }

        public void Start()
        {
            this.ServerOnline = true;
            ThreadPool.QueueUserWorkItem(this.StartServer, null);
        }

        private static void FinishDisconnect(Socket incomingSocket)
        {
            incomingSocket.Disconnect(true);
        }

        private void StartServer(object data)
        {
            try
            {
                while (this.ServerOnline)
                {
                    this.server.Start();
                    var incomingSocket = this.server.AcceptSocket();
                    var received = new byte[512];
                    incomingSocket.Receive(received, received.Length, 0);
                    var userName = Encoding.Default.GetString(received);
                    this.SendFavorites(incomingSocket);
                }

                this.server.Stop();
            }
            catch (Exception exc)
            {
                Logging.Error("StartServer", exc);
            }
        }

        private void SendFavorites(Socket incomingSocket)
        {
            var list = this.FavoritesToSharedList();
            var data = SharedListToBinaryData(list);
            incomingSocket.Send(data);
            FinishDisconnect(incomingSocket);
        }

        private ArrayList FavoritesToSharedList()
        {
            var favoritesToShare = this.persistence.Favorites;
            var list = new ArrayList();

            foreach (var favorite in favoritesToShare)
            {
                var configFavorite =
                    ModelConverterV2ToV1.ConvertToFavorite(favorite, this.persistence, this.connectionManager);
                list.Add(SharedFavorite.ConvertFromFavorite(this.persistence, configFavorite));
            }

            return list;
        }

        private static byte[] SharedListToBinaryData(ArrayList favorites)
        {
            var favs = Serialize.SerializeBinary(favorites);

            if (favs != null && favs.Length > 0)
            {
                if (favs.CanRead && favs.Position > 0)
                    favs.Position = 0;
                var data = favs.ToArray();
                favs.Close();
                favs.Dispose();
                return data;
            }

            return new byte[0];
        }
    }
}