using System;
using System.Collections.Generic;
using Terminals.Configuration;
using Terminals.Converters;
using Terminals.Data;

namespace Terminals.Network
{
    [Serializable]
    public class SharedFavorite
    {
        public Colors Colors;

        public bool ConnectToConsole;

        public string ConsoleBackColor;

        public int ConsoleCols;

        public string ConsoleCursorColor;

        public string ConsoleFont;

        public int ConsoleRows;

        public string ConsoleTextColor;

        public string DesktopShare;

        public DesktopSize DesktopSize;

        public bool DisableWallPaper;

        public string DomainName;

        public string Name;

        public int Port;

        public string Protocol;

        public bool RedirectClipboard;

        public bool RedirectDevices;

        public bool RedirectDrives;

        public List<string> RedirectedDrives;

        public bool RedirectPorts;

        public bool RedirectPrinters;

        public bool RedirectSmartCards;

        public string ServerName;

        public RemoteSounds Sounds;

        public string Tags;

        public bool Telnet;

        public bool VMRCAdministratorMode;

        public bool VMRCReducedColorsMode;

        public static FavoriteConfigurationElement ConvertFromFavorite(SharedFavorite Favorite)
        {
            var fav = new FavoriteConfigurationElement();
            fav.Colors = Favorite.Colors;
            fav.ConnectToConsole = Favorite.ConnectToConsole;
            fav.DesktopShare = Favorite.DesktopShare;
            fav.DesktopSize = Favorite.DesktopSize;
            fav.DomainName = Favorite.DomainName;
            fav.Name = Favorite.Name;
            fav.Port = Favorite.Port;
            fav.Protocol = Favorite.Protocol;
            fav.RedirectClipboard = Favorite.RedirectClipboard;
            fav.RedirectDevices = Favorite.RedirectDevices;
            fav.RedirectedDrives = Favorite.RedirectedDrives;
            fav.RedirectPorts = Favorite.RedirectPorts;
            fav.RedirectPrinters = Favorite.RedirectPrinters;
            fav.RedirectSmartCards = Favorite.RedirectSmartCards;
            fav.ServerName = Favorite.ServerName;
            fav.DisableWallPaper = Favorite.DisableWallPaper;
            fav.Sounds = Favorite.Sounds;
            fav.Tags = Favorite.Tags;
            fav.ConsoleBackColor = Favorite.ConsoleBackColor;
            fav.ConsoleCols = Favorite.ConsoleCols;
            fav.ConsoleCursorColor = Favorite.ConsoleCursorColor;
            fav.ConsoleFont = Favorite.ConsoleFont;
            fav.ConsoleRows = Favorite.ConsoleRows;
            fav.ConsoleTextColor = Favorite.ConsoleTextColor;
            fav.VMRCAdministratorMode = Favorite.VMRCAdministratorMode;
            fav.VMRCReducedColorsMode = Favorite.VMRCReducedColorsMode;

            return fav;
        }

        internal static SharedFavorite ConvertFromFavorite(IPersistence persistence,
            FavoriteConfigurationElement Favorite)
        {
            var favoriteSecurity = new FavoriteConfigurationSecurity(persistence, Favorite);
            var fav = new SharedFavorite();
            fav.Colors = Favorite.Colors;
            fav.ConnectToConsole = Favorite.ConnectToConsole;
            fav.DesktopShare = Favorite.DesktopShare;
            fav.DesktopSize = Favorite.DesktopSize;
            fav.DomainName = favoriteSecurity.ResolveDomainName();
            fav.Name = Favorite.Name;
            fav.Port = Favorite.Port;
            fav.Protocol = Favorite.Protocol;
            fav.RedirectClipboard = Favorite.RedirectClipboard;
            fav.RedirectDevices = Favorite.RedirectDevices;
            fav.RedirectedDrives = Favorite.RedirectedDrives;
            fav.RedirectPorts = Favorite.RedirectPorts;
            fav.RedirectPrinters = Favorite.RedirectPrinters;
            fav.RedirectSmartCards = Favorite.RedirectSmartCards;
            fav.ServerName = Favorite.ServerName;
            fav.DisableWallPaper = Favorite.DisableWallPaper;
            fav.Sounds = Favorite.Sounds;
            var tagsConverter = new TagsConverter();
            fav.Tags = tagsConverter.ResolveTags(Favorite);
            fav.ConsoleBackColor = Favorite.ConsoleBackColor;
            fav.ConsoleCols = Favorite.ConsoleCols;
            fav.ConsoleCursorColor = Favorite.ConsoleCursorColor;
            fav.ConsoleFont = Favorite.ConsoleFont;
            fav.ConsoleRows = Favorite.ConsoleRows;
            fav.ConsoleTextColor = Favorite.ConsoleTextColor;
            fav.VMRCAdministratorMode = Favorite.VMRCAdministratorMode;
            fav.VMRCReducedColorsMode = Favorite.VMRCReducedColorsMode;
            return fav;
        }
    }
}