using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Terminals.Connections;

namespace Terminals.Scanner
{
    internal delegate void NetworkScanHandler(ScanItemEventArgs args);

    internal class NetworkScanManager
    {
        private readonly ConnectionManager connectionManager;

        private int doneAddressScans;

        private readonly object doneItems = new object();

        private bool scanIsRunning;

        private readonly List<NetworkScanItem> scanItems = new List<NetworkScanItem>();

        public NetworkScanManager(ConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;
        }

        /// <summary>
        ///     Gets count of pending ip addresses to scan during last or actualy running scan.
        /// </summary>
        internal int PendingAddressesToScan => this.AllAddressesToScan - this.DoneAddressScans;

        /// <summary>
        ///     Gets count of all ip addresses to scan during last or actualy running scan.
        /// </summary>
        internal int AllAddressesToScan { get; private set; }

        /// <summary>
        ///     Gets or sets count of already finished ipaddress scans during last or actualy running scan.
        /// </summary>
        internal int DoneAddressScans
        {
            get
            {
                lock (this.doneItems)
                {
                    return this.doneAddressScans;
                }
            }
            private set
            {
                lock (this.doneItems)
                {
                    this.doneAddressScans = value;
                }
            }
        }

        internal bool ScanIsRunning
        {
            get
            {
                lock (this.doneItems)
                {
                    return this.scanIsRunning;
                }
            }
            set
            {
                lock (this.doneItems)
                {
                    this.scanIsRunning = value;
                }
            }
        }

        internal event NetworkScanHandler OnAddressScanHit;

        internal event NetworkScanHandler OnAddressScanFinished;

        public override string ToString()
        {
            return string.Format("NetworkScanManager:{0}{1}/{2}",
                this.ScanIsRunning, this.DoneAddressScans, this.AllAddressesToScan);
        }

        internal void StartScan(string A, string B, string C, string D, string E, List<int> portList)
        {
            Debug.WriteLine("Starting scan with previous state" + this.ScanIsRunning);
            if (this.ScanIsRunning)
                return;

            this.ScanIsRunning = true;
            this.DoneAddressScans = 0;
            this.PrepareItemsToScan(A, B, C, D, E, portList);
            this.QueueBackgroundScans();
        }

        private void PrepareItemsToScan(string A, string B, string C, string D, string E, List<int> portList)
        {
            var ipBody = string.Format("{0}.{1}.{2}.", A, B, C);
            var start = 0;
            var end = 0;
            int.TryParse(D, out start);
            int.TryParse(E, out end);

            this.scanItems.Clear();
            this.AllAddressesToScan = end - start + 1;

            for (var ipSuffix = start; ipSuffix <= end; ipSuffix++)
            {
                if (!this.ScanIsRunning)
                    break;
                var ipAdddress = string.Format("{0}{1}", ipBody, ipSuffix);
                this.AddItemToScan(portList, ipAdddress);
            }
        }

        private void AddItemToScan(List<int> portList, string ipAdddress)
        {
            var item = new NetworkScanItem(this.connectionManager, ipAdddress, portList);
            this.scanItems.Add(item);
        }

        internal void StopScan()
        {
            Debug.WriteLine("Canceling scan with previous state" + this.ScanIsRunning);
            foreach (var scanItem in this.scanItems)
                scanItem.Stop();
            this.ScanIsRunning = false;
            Debug.WriteLine("Scan stoped.");
        }

        private void QueueBackgroundScans()
        {
            foreach (var item in this.scanItems)
            {
                if (!this.ScanIsRunning)
                    break;
                this.QueueBackgroundScan(item);
            }
        }

        private void QueueBackgroundScan(NetworkScanItem item)
        {
            // dont use events, otherwise we have to unregister
            item.OnScanHit = this.item_OnScanHit;
            item.OnScanFinished = this.item_OnScanFinished;
            ThreadPool.QueueUserWorkItem(item.Scan);
        }

        private void item_OnScanFinished(ScanItemEventArgs args)
        {
            Debug.WriteLine("Scan finished: {0}", args.ScanResult);

            this.DoneAddressScans++;

            if (this.OnAddressScanFinished != null && this.ScanIsRunning)
                this.OnAddressScanFinished(args);

            if (this.PendingAddressesToScan <= 0)
                this.StopScan();
        }

        private void item_OnScanHit(ScanItemEventArgs args)
        {
            if (this.OnAddressScanHit != null && this.ScanIsRunning)
                this.OnAddressScanHit(args);
        }
    }
}