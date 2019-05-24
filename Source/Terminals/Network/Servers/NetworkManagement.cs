using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
// standard
// for DllImport, MarshalAs, etc
// for IEnumerator, IEnumerable

// for ComboBox

namespace NetworkManagement
{
    /// <summary>
    ///     Wrapper class for all Win32 API calls and structures
    /// </summary>
    internal class Win32API
    {
        #region Win32 API Interfaces

        [DllImport("netapi32.dll", EntryPoint = "NetApiBufferFree")]
        internal static extern void NetApiBufferFree(IntPtr bufptr);

        [DllImport("netapi32.dll", EntryPoint = "NetServerEnum")]
        internal static extern uint NetServerEnum(
            IntPtr ServerName,
            uint level,
            ref IntPtr siPtr,
            uint prefmaxlen,
            ref uint entriesread,
            ref uint totalentries,
            uint servertype,
            [MarshalAs(UnmanagedType.LPWStr)] string domain,
            IntPtr resumeHandle);

        /// <summary>
        ///     Windows NT/2000/XP Only
        /// </summary>
        [DllImport("netapi32.dll", EntryPoint = "NetServerGetInfo")]
        internal static extern uint NetServerGetInfo(
            [MarshalAs(UnmanagedType.LPWStr)] string ServerName,
            int level,
            ref IntPtr buffPtr);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct SERVER_INFO_101
        {
            public int dwPlatformID;

            public IntPtr lpszServerName;

            public int dwVersionMajor;

            public int dwVersionMinor;

            public int dwType;

            public IntPtr lpszComment;
        }

        #endregion
    }

    /// <summary>
    ///     The possible flag values for Server Type (see lmserver.h).
    /// </summary>
    [Flags]
    public enum ServerType : long
    {
        /// <summary>
        ///     Opposite of All.  No servers will be returned.
        /// </summary>
        None = 0x00000000,

        /// <summary>
        ///     All workstations
        /// </summary>
        Workstation = 0x00000001,

        /// <summary>
        ///     All servers
        /// </summary>
        Server = 0x00000002,

        /// <summary>
        ///     Any server running with Microsoft SQL Server
        /// </summary>
        SQLServer = 0x00000004,

        /// <summary>
        ///     Primary domain controller
        /// </summary>
        DomainController = 0x00000008,

        /// <summary>
        ///     Backup domain controller
        /// </summary>
        DomainBackupController = 0x00000010,

        /// <summary>
        ///     Server running the Timesource service
        /// </summary>
        TimeSource = 0x00000020,

        /// <summary>
        ///     Apple File Protocol servers
        /// </summary>
        AFP = 0x00000040,

        /// <summary>
        ///     Novell servers
        /// </summary>
        Novell = 0x00000080,

        /// <summary>
        ///     LAN Manager 2.x domain member
        /// </summary>
        DomainMember = 0x00000100,

        /// <summary>
        ///     Server sharing print queue
        /// </summary>
        PrintQueue = 0x00000200,

        /// <summary>
        ///     Server running dial-in service
        /// </summary>
        Dialin = 0x00000400,

        /// <summary>
        ///     Xenix server
        /// </summary>
        Xenix = 0x00000800,

        /// <summary>
        ///     Unix servers?
        /// </summary>
        Unix = Xenix,

        /// <summary>
        ///     Windows NT workstation or server
        /// </summary>
        NT = 0x00001000,

        /// <summary>
        ///     Server running Windows for Workgroups
        /// </summary>
        WFW = 0x00002000,

        /// <summary>
        ///     Microsoft File and Print for NetWare
        /// </summary>
        MFPN = 0x00004000,

        /// <summary>
        ///     Server that is not a domain controller
        /// </summary>
        NTServer = 0x00008000,

        /// <summary>
        ///     Server that can run the browser service
        /// </summary>
        PotentialBrowser = 0x00010000,

        /// <summary>
        ///     Server running a browser service as backup
        /// </summary>
        BackupBrowser = 0x00020000,

        /// <summary>
        ///     Server running the master browser service
        /// </summary>
        MasterBrowser = 0x00040000,

        /// <summary>
        ///     Server running the domain master browser
        /// </summary>
        DomainMaster = 0x00080000,

        /// <summary>
        ///     Not documented on MSDN? Help Microsoft!
        /// </summary>
        OSF = 0x00100000,

        /// <summary>
        ///     Running VMS
        /// </summary>
        VMS = 0x00200000,

        /// <summary>
        ///     Windows 95 or later
        /// </summary>
        Windows = 0x00400000,

        /// <summary>
        ///     Distributed File System??
        /// </summary>
        DFS = 0x00800000,

        /// <summary>
        ///     Not documented on MSDN? Help Microsoft!
        /// </summary>
        ClusterNT = 0x01000000,

        /// <summary>
        ///     Terminal Server
        /// </summary>
        TerminalServer = 0x02000000,

        /// <summary>
        ///     Not documented on MSDN? Help Microsoft!
        /// </summary>
        DCE = 0x10000000,

        /// <summary>
        ///     Not documented on MSDN? Help Microsoft!
        /// </summary>
        AlternateXPort = 0x20000000,

        /// <summary>
        ///     Servers maintained by the browser
        /// </summary>
        ListOnly = 0x40000000,

        /// <summary>
        ///     List Domains
        /// </summary>
        DomainEnum = 0x80000000,

        /// <summary>
        ///     All servers
        /// </summary>
        All = 0xFFFFFFFF
    }

    /// <summary>
    ///     Enumerates over a set of servers returning the server's name.
    /// </summary>
    public class ServerEnumerator : IEnumerator
    {
        static ServerEnumerator()
        {
            SERVER_INFO_101_SIZE = Marshal.SizeOf(typeof(Win32API.SERVER_INFO_101));
        }

        /// <summary>
        /// </summary>
        /// <param name="serverType"></param>
        protected internal ServerEnumerator(ServerType serverType)
            : this(serverType, null)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="serverType"></param>
        /// <param name="domainName"></param>
        protected internal ServerEnumerator(ServerType serverType, string domainName)
        {
            uint level = 101, prefmaxlen = 0xFFFFFFFF, entriesread = 0, totalentries = 0;

            this.Reset();
            this.serverInfoPtr = IntPtr.Zero;

            var nRes = Win32API.NetServerEnum(
                IntPtr.Zero, // Server Name: Reserved; must be NULL. 
                level, // Return server names, types, and associated software. The bufptr parameter points to an array of SERVER_INFO_101 structures.
                ref this.serverInfoPtr, // Pointer to the buffer that receives the data.
                prefmaxlen, // Specifies the preferred maximum length of returned data, in bytes.
                ref entriesread, // count of elements actually enumerated.
                ref totalentries, // total number of visible servers and workstations on the network
                (uint)serverType, // value that filters the server entries to return from the enumeration
                domainName, // Pointer to a constant string that specifies the name of the domain for which a list of servers is to be returned.
                IntPtr.Zero); // Reserved; must be set to zero. 

            this.itemCount = entriesread;
        }

        /// <summary>
        ///     Returns the current server/machine/domain name
        /// </summary>
        public object Current => this.currentServerName;

        /// <summary>
        ///     Moves to the next server/machine/domain
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            var result = false;

            if (++this.currentItem < this.itemCount)
            {
                var newOffset = this.serverInfoPtr.ToInt32() + SERVER_INFO_101_SIZE * this.currentItem;
                var si = (Win32API.SERVER_INFO_101)Marshal.PtrToStructure(new IntPtr(newOffset),
                    typeof(Win32API.SERVER_INFO_101));
                this.currentServerName = Marshal.PtrToStringAuto(si.lpszServerName);
                result = true;
            }

            return result;
        }

        /// <summary>
        ///     Resets the enumeration back to the beginning.
        /// </summary>
        public void Reset()
        {
            this.currentItem = -1;
            this.currentServerName = null;
        }

        /// <summary>
        /// </summary>
        ~ServerEnumerator()
        {
            if (!this.serverInfoPtr.Equals(IntPtr.Zero))
            {
                Win32API.NetApiBufferFree(this.serverInfoPtr);
                this.serverInfoPtr = IntPtr.Zero;
            }
        }

        #region Instance Variables

        /// <summary>
        ///     Memory buffer pointer returned by NetServerEnum
        /// </summary>
        protected IntPtr serverInfoPtr;

        /// <summary>
        ///     The current item number
        /// </summary>
        protected int currentItem;

        /// <summary>
        ///     Number of items in the collection
        /// </summary>
        protected uint itemCount;

        /// <summary>
        ///     The name of the machine returned by Current
        /// </summary>
        protected string currentServerName;

        /// <summary>
        ///     Save the size of the SERVER_INFO_101 structure.
        ///     This allows us to only have a single time we need
        ///     to use 'unsafe' code.
        /// </summary>
        protected static int SERVER_INFO_101_SIZE;

        #endregion
    }

    /// <summary>
    ///     Class that encapsulates the Win32 API call of NetServerEnum
    /// </summary>
    public class Servers : IEnumerable
    {
        /// <summary>
        /// </summary>
        public Servers()
        {
            this.Type = ServerType.None;
        }

        /// <summary>
        ///     Specifies a value that filters the server entries to return from the enumeration
        /// </summary>
        /// <param name="aServerType"></param>
        public Servers(ServerType aServerType)
        {
            this.Type = aServerType;
        }

        /// <summary>
        /// </summary>
        /// <returns>IEnumerator</returns>
        public IEnumerator GetEnumerator()
        {
            return new ServerEnumerator(this.Type, this.DomainName);
        }

        /// <summary>
        ///     Returns the server type of the named server.
        /// </summary>
        /// <param name="serverName"></param>
        /// <returns></returns>
        public static ServerType GetServerType(string serverName)
        {
            var result = ServerType.None;

            var serverInfoPtr = IntPtr.Zero;
            var rc = Win32API.NetServerGetInfo(serverName, 101, ref serverInfoPtr);
            if (rc != 0 && serverInfoPtr != IntPtr.Zero)
            {
                var si = (Win32API.SERVER_INFO_101)Marshal.PtrToStructure(serverInfoPtr,
                    typeof(Win32API.SERVER_INFO_101));
                result = (ServerType)si.dwType;

                Win32API.NetApiBufferFree(serverInfoPtr);
                serverInfoPtr = IntPtr.Zero;
            }

            return result;
        }

        #region Win32 API Interfaces

        #endregion

        #region Properties

        /// <summary>
        ///     Gets/Sets the server type
        /// </summary>
        public ServerType Type { get; set; }

        /// <summary>
        /// </summary>
        public string DomainName { get; set; }

        #endregion
    }

    /// <summary>
    ///     A ComboBox that uses Servers object to populate itself
    ///     with a list of servers.
    /// </summary>
    public class ServerComboBox : ComboBox
    {
        private readonly Servers servers;

        /// <summary>
        /// </summary>
        public ServerComboBox()
        {
            this.InitializeComponent();
            this.servers = new Servers();
            this.AutoRefresh = false;
        }

        private void InitializeComponent()
        {
            this.Name = "ServerComboBox";
            this.Size = new Size(168, 24);
        }

        /// <summary>
        ///     Refreshes the ComboBox's data by enumerating the server list
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();

            this.BeginUpdate();
            this.Items.Clear();
            foreach (string name in this.servers)
                this.Items.Add(name);
            this.EndUpdate();
        }

        #region Properties

        /// <summary>
        ///     Server Type to search.  Can be one or more.
        /// </summary>
        public ServerType ServerType
        {
            get => this.servers.Type;
            set
            {
                this.servers.Type = value;
                if (this.AutoRefresh)
                    this.Refresh();
            }
        }

        /// <summary>
        ///     Domain name to search.  Set to <code>null</code> for all.
        /// </summary>
        public string DomainName
        {
            get => this.servers.DomainName;
            set
            {
                this.servers.DomainName = value;
                if (this.AutoRefresh)
                    this.Refresh();
            }
        }

        /// <summary>
        ///     If true, any changes to DomainName or ServerType will
        ///     cause the combobox to refresh it's data (default = false);
        /// </summary>
        public bool AutoRefresh { get; set; }

        #endregion
    }
}