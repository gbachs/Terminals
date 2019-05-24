using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Terminals.TerminalServices
{
    public class TerminalServicesAPI
    {
        public enum SID_NAME_USE
        {
            User = 1,

            Group,

            Domain,

            Alias,

            WellKnownGroup,

            DeletedAccount,

            Invalid,

            Unknown,

            Computer
        }

        public enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,

            WTSConnected,

            WTSConnectQuery,

            WTSShadow,

            WTSDisconnected,

            WTSIdle,

            WTSListen,

            WTSReset,

            WTSDown,

            WTSInit
        }

        public enum WTS_INFO_CLASS
        {
            WTSInitialProgram,

            WTSApplicationName,

            WTSWorkingDirectory,

            WTSOEMId,

            WTSSessionId,

            WTSUserName,

            WTSWinStationName,

            WTSDomainName,

            WTSConnectState,

            WTSClientBuildNumber,

            WTSClientName,

            WTSClientDirectory,

            WTSClientProductId,

            WTSClientHardwareId,

            WTSClientAddress,

            WTSClientDisplay,

            WTSClientProtocolType,

            WTSIdleTime,

            WTSLogonTime,

            WTSIncomingBytes,

            WTSOutgoingBytes,

            WTSIncomingFrames,

            WTSOutgoingFrames
        }

        private const long WTS_WSD_REBOOT = 0x00000004;

        private const long WTS_WSD_SHUTDOWN = 0x00000002;

        [DllImport("WtsApi32.dll", EntryPoint = "WTSQuerySessionInformationW", CharSet = CharSet.Ansi,
            SetLastError = true, ExactSpelling = true)]
        public static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass,
            ref IntPtr ppBuffer, ref int pBytesReturned);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern bool WTSEnumerateProcesses(
            IntPtr serverHandle, // Handle to a terminal server. 
            int reserved, // must be 0
            int version, // must be 1
            ref IntPtr ppProcessInfo, // pointer to array of WTS_PROCESS_INFO
            ref int pCount); // pointer to number of processes

        [DllImport("WtsApi32.dll", EntryPoint = "WTSQuerySessionInformationW", CharSet = CharSet.Ansi,
            SetLastError = true, ExactSpelling = true)]
        private static extern bool WTSQuerySessionInformation2(IntPtr hServer, int SessionId,
            WTS_INFO_CLASS WTSInfoClass, ref IntPtr ppBuffer, ref int pCount);

        [DllImport("Kernel32.dll", EntryPoint = "GetCurrentProcessId", CharSet = CharSet.Ansi, SetLastError = true,
            ExactSpelling = true)]
        private static extern int GetCurrentProcessId();

        [DllImport("Kernel32.dll", EntryPoint = "ProcessIdToSessionId", CharSet = CharSet.Ansi, SetLastError = true,
            ExactSpelling = true)]
        private static extern bool ProcessIdToSessionId(int processID, ref int sessionID);

        [DllImport("Kernel32.dll", EntryPoint = "WTSGetActiveConsoleSessionId", CharSet = CharSet.Ansi,
            SetLastError = true, ExactSpelling = true)]
        private static extern int WTSGetActiveConsoleSessionId();

        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern bool WTSSendMessage(
            IntPtr hServer,
            [MarshalAs(UnmanagedType.I4)] int SessionId,
            string pTitle,
            [MarshalAs(UnmanagedType.U4)] int TitleLength,
            string pMessage,
            [MarshalAs(UnmanagedType.U4)] int MessageLength,
            [MarshalAs(UnmanagedType.U4)] int Style,
            [MarshalAs(UnmanagedType.U4)] int Timeout,
            [MarshalAs(UnmanagedType.U4)] out int pResponse,
            bool bWait);

        //Function for TS Client IP Address 

        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern bool WTSLogoffSession(IntPtr hServer, int SessionId, bool bWait);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool LookupAccountSid(
            string lpSystemName,
            [MarshalAs(UnmanagedType.LPArray)] byte[] Sid,
            StringBuilder lpName,
            ref uint cchName,
            StringBuilder ReferencedDomainName,
            ref uint cchReferencedDomainName,
            out SID_NAME_USE peUse);

        [DllImport("wtsapi32.dll", BestFitMapping = true, CallingConvention = CallingConvention.StdCall,
            CharSet = CharSet.Auto, EntryPoint = "WTSEnumerateSessions", SetLastError = true,
            ThrowOnUnmappableChar = true)]
        private static extern int WTSEnumerateSessions(
            [MarshalAs(UnmanagedType.SysInt)] IntPtr hServer,
            [MarshalAs(UnmanagedType.U4)] int Reserved,
            [MarshalAs(UnmanagedType.U4)] int Vesrion,
            [MarshalAs(UnmanagedType.SysInt)] ref IntPtr ppSessionInfo,
            [MarshalAs(UnmanagedType.U4)] ref int pCount);

        [DllImport("wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pMemory);

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr WTSOpenServer(string pServerName);

        [DllImport("wtsapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void WTSCloseServer(IntPtr hServer);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        public static extern int WTSShutdownSystem(IntPtr ServerHandle, long ShutdownFlags);

        public static void ShutdownSystem(TerminalServer Server, bool Reboot)
        {
            var action = WTS_WSD_REBOOT;
            if (!Reboot) action = WTS_WSD_SHUTDOWN;
            var server = WTSOpenServer(Server.ServerName);
            if (server != IntPtr.Zero) WTSShutdownSystem(server, action);
        }

        public static bool SendMessage(Session Session, string Title, string Message, int Style, int Timeout, bool Wait)
        {
            var server = WTSOpenServer(Session.ServerName);
            if (server != IntPtr.Zero)
            {
                var respose = 0;
                return WTSSendMessage(server, Session.SessionID, Title, Title.Length, Message, Message.Length, Style,
                    Timeout, out respose, Wait);
            }

            return false;
        }

        public static bool LogOffSession(Session Session, bool Wait)
        {
            var server = WTSOpenServer(Session.ServerName);
            if (server != IntPtr.Zero)
                return WTSLogoffSession(server, Session.SessionID, Wait);
            return false;
        }

        public static TerminalServer GetSessions(string ServerName)
        {
            var Data = new TerminalServer();
            Data.ServerName = ServerName;

            var ptrOpenedServer = IntPtr.Zero;
            try
            {
                ptrOpenedServer = WTSOpenServer(ServerName);
                if (ptrOpenedServer == IntPtr.Zero)
                {
                    Data.IsATerminalServer = false;
                    return Data;
                }

                Data.ServerPointer = ptrOpenedServer;
                Data.IsATerminalServer = true;

                int FRetVal;
                var ppSessionInfo = IntPtr.Zero;
                var Count = 0;
                try
                {
                    FRetVal = WTSEnumerateSessions(ptrOpenedServer, 0, 1, ref ppSessionInfo, ref Count);

                    if (FRetVal != 0)
                    {
                        Data.Sessions = new List<Session>();
                        var sessionInfo = new WTS_SESSION_INFO[Count + 1];
                        int i;
                        IntPtr session_ptr;
                        for (i = 0; i <= Count - 1; i++)
                        {
                            session_ptr = new IntPtr(ppSessionInfo.ToInt32() + i * Marshal.SizeOf(sessionInfo[i]));
                            sessionInfo[i] =
                                (WTS_SESSION_INFO)Marshal.PtrToStructure(session_ptr, typeof(WTS_SESSION_INFO));
                            var s = new Session();
                            s.SessionID = sessionInfo[i].SessionID;
                            s.State = (ConnectionStates)(int)sessionInfo[i].State;
                            s.WindowsStationName = sessionInfo[i].pWinStationName;
                            s.ServerName = ServerName;
                            Data.Sessions.Add(s);
                        }

                        WTSFreeMemory(ppSessionInfo);
                        var tmpArr = new strSessionsInfo[sessionInfo.GetUpperBound(0) + 1];
                        for (i = 0; i <= tmpArr.GetUpperBound(0); i++)
                        {
                            tmpArr[i].SessionID = sessionInfo[i].SessionID;
                            tmpArr[i].StationName = sessionInfo[i].pWinStationName;
                            tmpArr[i].ConnectionState = GetConnectionState(sessionInfo[i].State);
                            //MessageBox.Show(tmpArr(i).StationName & " " & tmpArr(i).SessionID & " " & tmpArr(i).ConnectionState) 
                        }

                        // ERROR: Not supported in C#: ReDimStatement 
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error("Get Sessions Inner", ex);
                    Data.Errors.Add(ex.Message + "\r\n" + Marshal.GetLastWin32Error());
                }
            }
            catch (Exception ex)
            {
                Logging.Info("Get Sessions Outer", ex);
                Data.Errors.Add(ex.Message + "\r\n" + Marshal.GetLastWin32Error());
            }

            var plist = WTSEnumerateProcesses(ptrOpenedServer, Data);

            //Get ProcessID of TS Session that executed this TS Session 
            var active_process = GetCurrentProcessId();
            var active_session = 0;
            var success1 = ProcessIdToSessionId(active_process, ref active_session);
            if (active_session <= 0) success1 = false;
            if (Data != null && Data.Sessions != null)
                foreach (var s in Data.Sessions)
                {
                    if (s.Client == null) s.Client = new Client();

                    var ClientInfo = LoadClientInfoForSession(Data.ServerPointer, s.SessionID);
                    s.Client.Address = ClientInfo.Address;
                    s.Client.AddressFamily = ClientInfo.AddressFamily;
                    s.Client.ClientName = ClientInfo.WTSClientName;
                    s.Client.DomianName = ClientInfo.WTSDomainName;
                    s.Client.StationName = ClientInfo.WTSStationName;
                    s.Client.Status = ClientInfo.WTSStatus;
                    s.Client.UserName = ClientInfo.WTSUserName;
                    s.IsTheActiveSession = false;
                    if (success1 && s.SessionID == active_session) s.IsTheActiveSession = true;
                }

            WTSCloseServer(ptrOpenedServer);
            return Data;
        }

        public static WTS_PROCESS_INFO[] WTSEnumerateProcesses(IntPtr WTS_CURRENT_SERVER_HANDLE, TerminalServer Data)
        {
            var pProcessInfo = IntPtr.Zero;
            var processCount = 0;

            if (!WTSEnumerateProcesses(WTS_CURRENT_SERVER_HANDLE, 0, 1, ref pProcessInfo, ref processCount))
                return null;

            const int NO_ERROR = 0;
            const int ERROR_INSUFFICIENT_BUFFER = 122;
            var err = NO_ERROR;
            var pMemory = pProcessInfo;
            var processInfos = new WTS_PROCESS_INFO[processCount];
            for (var i = 0; i < processCount; i++)
            {
                processInfos[i] = (WTS_PROCESS_INFO)Marshal.PtrToStructure(pProcessInfo, typeof(WTS_PROCESS_INFO));
                pProcessInfo = (IntPtr)((int)pProcessInfo + Marshal.SizeOf(processInfos[i]));

                var p = new SessionProcess();
                p.ProcessID = processInfos[i].ProcessID;
                p.ProcessName = Marshal.PtrToStringAnsi(processInfos[i].ProcessName);

                if (processInfos[i].UserSid != IntPtr.Zero)
                {
                    byte[] Sid = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
                    Marshal.Copy(processInfos[i].UserSid, Sid, 0, 14);
                    var name = new StringBuilder();
                    var cchName = (uint)name.Capacity;
                    SID_NAME_USE sidUse;
                    var referencedDomainName = new StringBuilder();
                    var cchReferencedDomainName = (uint)referencedDomainName.Capacity;
                    if (LookupAccountSid(Data.ServerName, Sid, name, ref cchName, referencedDomainName,
                        ref cchReferencedDomainName, out sidUse))
                    {
                        err = Marshal.GetLastWin32Error();
                        if (err == ERROR_INSUFFICIENT_BUFFER)
                        {
                            name.EnsureCapacity((int)cchName);
                            referencedDomainName.EnsureCapacity((int)cchReferencedDomainName);
                            err = NO_ERROR;
                            if (!LookupAccountSid(null, Sid, name, ref cchName, referencedDomainName,
                                ref cchReferencedDomainName, out sidUse))
                                err = Marshal.GetLastWin32Error();
                        }

                        p.UserType = sidUse.ToString();
                        p.User = name.ToString();
                    }
                }

                //string userSID = Marshal.PtrToStringAuto(processInfos[i].UserSid);
                p.SessionID = processInfos[i].SessionID;

                //LookupAccountSid(Data.ServerName, 
                //p.User = Marshal.PtrToStringAnsi(processInfos[i].UserSid);
                foreach (var s in Data.Sessions)
                    if (s.SessionID == p.SessionID)
                    {
                        if (s.Processes == null) s.Processes = new List<SessionProcess>();
                        s.Processes.Add(p);
                        break;
                    }
            }

            WTSFreeMemory(pMemory);
            return processInfos;
        }

        private static WTS_CLIENT_INFO LoadClientInfoForSession(IntPtr ptrOpenedServer, int active_session)
        {
            var returned = 0;
            var str = IntPtr.Zero;

            var ClientInfo = new WTS_CLIENT_INFO();
            ClientInfo.WTSStationName = "";
            ClientInfo.WTSClientName = "";
            ClientInfo.Address = new byte[6];
            ClientInfo.Address[2] = 0;
            ClientInfo.Address[3] = 0;
            ClientInfo.Address[4] = 0;
            ClientInfo.Address[5] = 0;

            ClientInfo.WTSClientName = GetString(ptrOpenedServer, active_session, WTS_INFO_CLASS.WTSClientName);
            ClientInfo.WTSStationName = GetString(ptrOpenedServer, active_session, WTS_INFO_CLASS.WTSWinStationName);
            ClientInfo.WTSDomainName = GetString(ptrOpenedServer, active_session, WTS_INFO_CLASS.WTSDomainName);

            //Get client IP address 
            var addr = IntPtr.Zero;
            if (WTSQuerySessionInformation2(ptrOpenedServer, active_session, WTS_INFO_CLASS.WTSClientAddress, ref addr,
                ref returned))
            {
                var obj = new _WTS_CLIENT_ADDRESS();
                obj = (_WTS_CLIENT_ADDRESS)Marshal.PtrToStructure(addr, obj.GetType());
                ClientInfo.Address[2] = obj.Address[2];
                ClientInfo.Address[3] = obj.Address[3];
                ClientInfo.Address[4] = obj.Address[4];
                ClientInfo.Address[5] = obj.Address[5];
            }

            return ClientInfo;
        }

        private static string GetString(IntPtr ptrOpenedServer, int active_session, WTS_INFO_CLASS whichOne)
        {
            var str = IntPtr.Zero;
            var returned = 0;
            if (WTSQuerySessionInformation(ptrOpenedServer, active_session, whichOne, ref str, ref returned))
                return Marshal.PtrToStringAuto(str);
            return "";
        }

        private static string GetConnectionState(WTS_CONNECTSTATE_CLASS State)
        {
            string RetVal;
            switch (State)
            {
                case WTS_CONNECTSTATE_CLASS.WTSActive:
                    RetVal = "Active";
                    break;
                case WTS_CONNECTSTATE_CLASS.WTSConnected:
                    RetVal = "Connected";
                    break;
                case WTS_CONNECTSTATE_CLASS.WTSConnectQuery:
                    RetVal = "Query";
                    break;
                case WTS_CONNECTSTATE_CLASS.WTSDisconnected:
                    RetVal = "Disconnected";
                    break;
                case WTS_CONNECTSTATE_CLASS.WTSDown:
                    RetVal = "Down";
                    break;
                case WTS_CONNECTSTATE_CLASS.WTSIdle:
                    RetVal = "Idle";
                    break;
                case WTS_CONNECTSTATE_CLASS.WTSInit:
                    RetVal = "Initializing.";
                    break;
                case WTS_CONNECTSTATE_CLASS.WTSListen:
                    RetVal = "Listen";
                    break;
                case WTS_CONNECTSTATE_CLASS.WTSReset:
                    RetVal = "reset";
                    break;
                case WTS_CONNECTSTATE_CLASS.WTSShadow:
                    RetVal = "Shadowing";
                    break;
                default:
                    RetVal = "Unknown connect state";
                    break;
            }

            return RetVal;
        }

        public struct WTS_PROCESS_INFO
        {
            public int SessionID;

            public int ProcessID;

            //This is a pointer to string...
            public IntPtr ProcessName;

            public IntPtr UserSid;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WTS_SESSION_INFO
        {
            //DWORD integer 
            public int SessionID;

            // integer LPTSTR - Pointer to a null-terminated string containing the name of the WinStation for this session 
            public string pWinStationName;

            public WTS_CONNECTSTATE_CLASS State;
        }

        internal struct strSessionsInfo
        {
            public int SessionID;

            public string StationName;

            public string ConnectionState;
        }

        //Structure for TS Client IP Address 
        [StructLayout(LayoutKind.Sequential)]
        public struct _WTS_CLIENT_ADDRESS
        {
            public int AddressFamily;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public byte[] Address;
        }

        //Structure for TS Client Information 
        public struct WTS_CLIENT_INFO
        {
            [MarshalAs(UnmanagedType.Bool)]
            public bool WTSStatus;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string WTSUserName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string WTSStationName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string WTSDomainName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string WTSClientName;

            [MarshalAs(UnmanagedType.LPWStr)]
            public int AddressFamily;

            [MarshalAs(UnmanagedType.ByValArray)]
            public byte[] Address;
        }
    }
}