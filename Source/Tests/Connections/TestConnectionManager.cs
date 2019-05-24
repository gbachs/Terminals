using System.Collections.Generic;
using Moq;
using Terminals.Connections;
using Terminals.Connections.Rdp;
using Terminals.Connections.VMRC;
using Terminals.Connections.VNC;
using Terminals.Data;
using Terminals.Plugins.Putty;

namespace Tests.Connections
{
    internal class TestConnectionManager
    {
        private static readonly List<IConnectionPlugin> connectionPlugins = new List<IConnectionPlugin>()
        {
            new RdpConnectionPlugin(),
            new VncConnectionPlugin(),
            new VmrcConnectionPlugin(),
            new TelnetConnectionPlugin(),
            new SshConnectionPlugin()
        };

        private static readonly ConnectionManager instance = CreateConnectionManager(connectionPlugins);

        /// <summary>
        /// Gets instance of manager configured by staticaly loaded plugins.
        /// Otherwise we would need to deploy the plugins into test directory.
        /// </summary>
        public static ConnectionManager Instance { get { return instance; } }

        internal static ConnectionManager CreateRdpOnlyManager()
        {
            var plugins = new List<IConnectionPlugin>()
            {
                new RdpConnectionPlugin(),
            };

            return CreateConnectionManager(plugins);
        }

        internal static ConnectionManager CreateConnectionManager(List<IConnectionPlugin> connectionPlugins)
        {
            var mockLoader = CreateMockLoader(connectionPlugins);
            return new ConnectionManager(mockLoader.Object);
        }

        private static Mock<IPluginsLoader> CreateMockLoader(List<IConnectionPlugin> connectionPlugins)
        {
            var mockLoader = new Mock<IPluginsLoader>();
            mockLoader.Setup(l => l.Load())
                .Returns(() => connectionPlugins);
            return mockLoader;
        }

        internal static FavoriteIcons CreateTestFavoriteIcons()
        {
            return new FavoriteIcons(Instance);
        }
    }
}