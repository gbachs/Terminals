// Note the VB example will give you the first entry of the array n times where n is the size of the array

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Terminals
{
    internal class TSManager
    {
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

            WTSClientProtocolType
        }

        public const int WTS_CURRENT_SESSION = -1;

        [DllImport("wtsapi32.dll")]
        private static extern IntPtr WTSOpenServer([MarshalAs(UnmanagedType.LPStr)] string pServerName);

        [DllImport("wtsapi32.dll")]
        private static extern void WTSCloseServer(IntPtr hServer);

        [DllImport("wtsapi32.dll")]
        private static extern int WTSEnumerateSessions(
            IntPtr hServer,
            [MarshalAs(UnmanagedType.U4)] int Reserved,
            [MarshalAs(UnmanagedType.U4)] int Version,
            ref IntPtr ppSessionInfo,
            [MarshalAs(UnmanagedType.U4)] ref int pCount);

        [DllImport("wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pMemory);

        [DllImport("Wtsapi32.dll")]
        public static extern bool WTSQuerySessionInformation(
            IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out IntPtr ppBuffer, out uint pBytesReturned);

        public static IntPtr OpenServer(string name)
        {
            var server = WTSOpenServer(name);
            return server;
        }

        public static void CloseServer(IntPtr serverHandle)
        {
            WTSCloseServer(serverHandle);
        }

        public static string QuerySessionInfo(IntPtr server, int sessionId, WTS_INFO_CLASS infoClass)
        {
            var buffer = IntPtr.Zero;
            uint bytesReturned;
            try
            {
                WTSQuerySessionInformation(server, sessionId, infoClass, out buffer, out bytesReturned);
                return Marshal.PtrToStringAnsi(buffer);
            }
            catch (Exception exc)
            {
                Logging.Info(exc);
                return string.Empty;
            }
            finally
            {
                WTSFreeMemory(buffer);
                buffer = IntPtr.Zero;
            }
        }

        public static List<SessionInfo> ListSessions(string serverName)
        {
            return ListSessions(serverName, null, null, null, null);
        }

        public static List<SessionInfo> ListSessions(string serverName, string userName, string domainName,
            string clientName, WTS_CONNECTSTATE_CLASS? state)
        {
            var server = IntPtr.Zero;
            var sessions = new List<SessionInfo>();
            server = OpenServer(serverName);
            try
            {
                var ppSessionInfo = IntPtr.Zero;
                var count = 0;
                var retval = WTSEnumerateSessions(server, 0, 1, ref ppSessionInfo, ref count);
                var dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
                var current = (int)ppSessionInfo;
                if (retval != 0)
                {
                    for (var i = 0; i < count; i++)
                    {
                        var sessionInfo = new SessionInfo();
                        var si = (WTS_SESSION_INFO)Marshal.PtrToStructure((IntPtr)current, typeof(WTS_SESSION_INFO));
                        current += dataSize;

                        sessionInfo.Id = si.SessionID;
                        sessionInfo.UserName = QuerySessionInfo(server, sessionInfo.Id, WTS_INFO_CLASS.WTSUserName);
                        sessionInfo.DomainName = QuerySessionInfo(server, sessionInfo.Id, WTS_INFO_CLASS.WTSDomainName);
                        sessionInfo.ClientName = QuerySessionInfo(server, sessionInfo.Id, WTS_INFO_CLASS.WTSClientName);
                        sessionInfo.State = si.State;

                        if (userName != null || domainName != null || clientName != null || state != null
                        ) //In this case, the caller is asking to return only matching sessions
                        {
                            if (userName != null && !string.Equals(userName, sessionInfo.UserName,
                                    StringComparison.CurrentCultureIgnoreCase))
                                continue; //Not matching
                            if (clientName != null && !string.Equals(clientName, sessionInfo.ClientName,
                                    StringComparison.CurrentCultureIgnoreCase))
                                continue; //Not matching
                            if (domainName != null && !string.Equals(domainName, sessionInfo.DomainName,
                                    StringComparison.CurrentCultureIgnoreCase))
                                continue; //Not matching
                            if (state != null && sessionInfo.State != state.Value)
                                continue;
                        }

                        sessions.Add(sessionInfo);
                    }

                    WTSFreeMemory(ppSessionInfo);
                }
            }
            finally
            {
                CloseServer(server);
            }

            return sessions;
        }

        public static SessionInfo GetCurrentSession(string serverName, string userName, string domainName,
            string clientName)
        {
            var sessions = ListSessions(serverName, userName, domainName, clientName, WTS_CONNECTSTATE_CLASS.WTSActive);
            if (sessions.Count == 0)
                return null;
            if (sessions.Count > 1)
                throw new Exception("Duplicate sessions found for user");
            return sessions[0];
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_SESSION_INFO
        {
            public readonly int SessionID;

            [MarshalAs(UnmanagedType.LPStr)]
            public readonly string pWinStationName;

            public readonly WTS_CONNECTSTATE_CLASS State;
        }
    }

    public class SessionInfo
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string DomainName { get; set; }

        public string ClientName { get; set; }

        internal TSManager.WTS_CONNECTSTATE_CLASS State { get; set; }
    }
}