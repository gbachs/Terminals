using System;
using System.DirectoryServices;
using System.Threading;

namespace Terminals.Network
{
    internal delegate void ListComputersDoneDelegate(bool success);

    internal delegate void ComputerFoundDelegate(ActiveDirectoryComputer computer);

    internal class ActiveDirectoryClient
    {
        private readonly object runLock = new object();

        private bool cancelationPending;

        private bool isRunning;

        internal bool IsRunning
        {
            get
            {
                lock (this.runLock)
                {
                    return this.isRunning;
                }
            }
            private set
            {
                lock (this.runLock)
                {
                    this.isRunning = value;
                }
            }
        }

        private bool CancelationPending
        {
            get
            {
                lock (this.runLock)
                {
                    return this.cancelationPending;
                }
            }
        }

        internal event ListComputersDoneDelegate ListComputersDone;

        internal event ComputerFoundDelegate ComputerFound;

        internal void FindComputers(ActiveDirectorySearchParams searchParams)
        {
            if (!this.IsRunning) // nothing is running
            {
                this.cancelationPending = false;
                this.IsRunning = true;
                ThreadPool.QueueUserWorkItem(this.StartScan, searchParams);
            }
        }

        internal void Stop()
        {
            lock (this.runLock)
            {
                if (this.isRunning)
                    this.cancelationPending = true;
            }
        }

        private void StartScan(object state)
        {
            try
            {
                var searchParams = state as ActiveDirectorySearchParams;
                this.SearchComputers(searchParams);
                this.FireListComputersDone(true);
            }
            catch (Exception exc)
            {
                this.FireListComputersDone(false);
                Logging.Error("Could not list the computers on the domain: " + state, exc);
            }
            finally
            {
                this.IsRunning = false;
            }
        }

        private void SearchComputers(ActiveDirectorySearchParams searchParams)
        {
            using (var entry = new DirectoryEntry(string.Format("LDAP://{0}", searchParams.Domain)))
            {
                using (var searcher = CreateSearcher(entry, searchParams))
                {
                    using (var found = searcher.FindAll())
                    {
                        this.ImportResults(searchParams, found);
                    }
                }
            }
        }

        private void ImportResults(ActiveDirectorySearchParams searchParams, SearchResultCollection found)
        {
            foreach (SearchResult result in found)
            {
                if (this.CancelationPending)
                    return;
                var computer = result.GetDirectoryEntry();
                var comp = ActiveDirectoryComputer.FromDirectoryEntry(searchParams.Domain, computer);
                this.FireComputerFound(comp);
            }
        }

        private static DirectorySearcher CreateSearcher(DirectoryEntry entry, ActiveDirectorySearchParams searchParams)
        {
            var searcher = new DirectorySearcher(entry);
            searcher.Asynchronous = true;
            searcher.Filter = searchParams.Filter;
            searcher.SearchRoot = new DirectoryEntry("LDAP://" + searchParams.Searchbase);
            searcher.PageSize = searchParams.PageSize;
            return searcher;
        }

        private void FireListComputersDone(bool success)
        {
            if (this.ListComputersDone != null)
                this.ListComputersDone(success);
        }

        private void FireComputerFound(ActiveDirectoryComputer computer)
        {
            if (this.ComputerFound != null)
                this.ComputerFound(computer);
        }
    }
}