using SysConfig = System.Configuration;

namespace Terminals.Configuration
{
    internal partial class Settings
    {
        internal GroupConfigurationElementCollection GetGroups()
        {
            return this.GetSection().Groups;
        }

        /// <summary>
        ///     "Since version 2. only for updates. Use new persistence instead."
        /// </summary>
        internal void ClearGroups()
        {
            var configGroups = this.GetGroups();
            configGroups.Clear();
        }
    }
}