using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Terminals.Configuration;
using Terminals.Connections;
using Terminals.Network;

namespace Terminals.Scanner
{
    internal class NetworkScanItem
    {
        private readonly ConnectionManager connectionManager;

        private bool cancelationPending;

        private readonly string iPAddress;

        private readonly List<int> ports;

        internal NetworkScanItem(ConnectionManager connectionManager, string iPAddress, List<int> ports)
        {
            this.iPAddress = iPAddress;
            this.ports = ports;
            this.connectionManager = connectionManager;
        }

        // dont use events, otherwise we have to unregister
        internal NetworkScanHandler OnScanHit { get; set; }

        internal NetworkScanHandler OnScanFinished { get; set; }

        internal string HostName { get; private set; }

        private bool CancelationPending
        {
            get
            {
                lock (this.ports)
                {
                    return this.cancelationPending;
                }
            }
        }

        public override string ToString()
        {
            var portsText = string.Empty;
            foreach (var port in this.ports)
                portsText += port + ",";
            return string.Format("NeworkScanItem:{0},{1}{{{2}}}", this.iPAddress, this.HostName, portsText);
        }

        internal void Scan(object data)
        {
            this.ResolveHostname();
            foreach (var port in this.ports)
            {
                if (this.CancelationPending)
                    return;

                this.ScanPort(port);
            }

            this.FireOnScanFinished();
        }

        internal void Stop()
        {
            lock (this.ports)
            {
                this.cancelationPending = true;
            }
        }

        private void ResolveHostname()
        {
            try
            {
                if (this.CancelationPending)
                    return;

                var entry = Dns.GetHostEntry(this.iPAddress);
                this.HostName = entry.HostName;
            }
            catch (Exception exc)
            {
                Logging.Error("Attempting to Resolve host named failed", exc);
            }
        }

        private void ScanPort(int port)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var connectionState = new ConnectionState(port, client);
                    client.BeginConnect(this.iPAddress, port, this.AttemptConnect, connectionState);
                    this.WaitUntilTimeOut(connectionState);
                    client.Client.Close();
                }
            }
            catch (Exception e)
            {
                Logging.Error("Scan Failed", e);
            }
        }

        private void FireOnScanFinished()
        {
            this.OnScanFinished?.Invoke(this.CreateNewEventArguments());
        }

        private void WaitUntilTimeOut(ConnectionState connectionState)
        {
            var timeout = 0;
            var maxTimeOut = Settings.Instance.PortScanTimeoutSeconds * 1000 / 50; // in seconds not in miliseconds
            while (timeout <= maxTimeOut && !this.CancelationPending && !connectionState.Done)
            {
                Thread.Sleep(50);
                timeout++;
            }
        }

        private void AttemptConnect(IAsyncResult result)
        {
            var connectionState = result.AsyncState as ConnectionState;
            var socket = connectionState.Client.Client;

            if (socket != null && socket.Connected)
            {
                var detector = new ServiceDetector(this.connectionManager);
                var serviceName = detector.ResolveServiceName(this.iPAddress, connectionState.Port);
                this.FireOnScanHit(connectionState.Port, serviceName);
            }
            else
            {
                Debug.WriteLine("Port {0} not openend at {1}", this.iPAddress, connectionState.Port);
            }

            connectionState.Done = true;
        }

        private void FireOnScanHit(int port, string serviceName)
        {
            if (this.OnScanHit != null)
            {
                var args = this.CreateNewEventArguments(port, serviceName);
                this.OnScanHit(args);
            }
        }

        private ScanItemEventArgs CreateNewEventArguments(int port = 0, string serviceName = "")
        {
            var args = new ScanItemEventArgs();
            args.DateTime = DateTime.Now;
            args.ScanResult = new NetworkScanResult
            {
                HostName = this.HostName,
                IPAddress = this.iPAddress,
                Port = port,
                ServiceName = serviceName
            };

            return args;
        }
    }
}