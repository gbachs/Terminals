using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Terminals.Configuration;
using Terminals.Connections;
using Terminals.Properties;

namespace Terminals.Data
{
    /// <summary>
    ///     Loading of icons from files
    /// </summary>
    internal class FavoriteIcons
    {
        private const string DEFAULT_ICONKEY = "terminalsicon.png";

        private const string ICON_PREFIX = "treeIcon_";

        private readonly Dictionary<string, Image> pluginIcons;

        internal FavoriteIcons(ConnectionManager connectionManager)
        {
            this.pluginIcons = connectionManager.GetPluginIcons();
        }

        /// <summary>
        ///     Gets empty bytes array used as empty not assigned image data.
        /// </summary>
        internal static byte[] EmptyImageData => new byte[0];

        internal IDictionary<string, Image> GetProtocolIcons()
        {
            var uiIcons = this.pluginIcons.ToDictionary(k => CreateIconKey(k.Key), v => v.Value);
            uiIcons.Add(DEFAULT_ICONKEY, Connection.Terminalsicon);
            return uiIcons;
        }

        private static string CreateIconKey(string protocol)
        {
            return ICON_PREFIX + protocol;
        }

        /// <summary>
        ///     Gets the icon file name by icons defined in FavoritesTreeView imageListIcons
        /// </summary>
        internal string GetTreeviewImageListKey(string protocol)
        {
            if (this.pluginIcons.ContainsKey(protocol))
                return CreateIconKey(protocol);

            return DEFAULT_ICONKEY;
        }

        /// <summary>
        ///     Gets the icon indexes by icons defined in FavoritesTreeView imageListIcons
        /// </summary>
        private Image GetProtocolImage(IFavorite favorite)
        {
            if (this.pluginIcons.ContainsKey(favorite.Protocol))
                return this.pluginIcons[favorite.Protocol];

            return Connection.Terminalsicon;
        }

        internal Image GetFavoriteIcon(Favorite favorite)
        {
            if (string.IsNullOrEmpty(favorite.ToolBarIconFile))
                return this.GetProtocolImage(favorite);

            return LoadImage(favorite.ToolBarIconFile, Resources.smallterm);
        }

        internal static Image LoadImage(string imageFilePath, Image defaultIcon)
        {
            try
            {
                if (!string.IsNullOrEmpty(imageFilePath) && File.Exists(imageFilePath))
                    return Image.FromFile(imageFilePath);
            }
            catch (Exception ex)
            {
                Logging.Error("Error Loading menu item image", ex);
            }

            return defaultIcon;
        }

        internal Image LoadImage(string value, IFavorite favorite)
        {
            var imageData = LoadImage(value);
            return this.LoadImage(imageData, favorite);
        }

        internal Image LoadImage(byte[] imageData, IFavorite favorite)
        {
            try
            {
                // empty or not assign icon replace by default icon
                if (imageData.Length == 0)
                    return this.GetProtocolImage(favorite);

                return LoadFromBinary(imageData);
            }
            catch
            {
                return this.GetProtocolImage(favorite);
            }
        }

        private static Image LoadFromBinary(byte[] imageData)
        {
            using (var memoryStream = new MemoryStream(imageData))
            {
                return Image.FromStream(memoryStream, true);
            }
        }

        private static byte[] LoadImage(string imageFilePath)
        {
            try
            {
                return TryLoadImage(imageFilePath);
            }
            catch
            {
                return EmptyImageData;
            }
        }

        private static byte[] TryLoadImage(string imageFilePath)
        {
            using (var file = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                var fileInfo = new FileInfo(imageFilePath);
                var imageLength = (int)fileInfo.Length;
                var imageData = new byte[imageLength];
                file.Read(imageData, 0, imageLength);
                return imageData;
            }
        }

        internal static string CopySelectedImageToThumbsDir(string newImagefilePath)
        {
            var newFileName = Path.GetFileName(newImagefilePath);
            var newFileInThumbsDir = Path.Combine(FileLocations.ThumbsDirectoryFullPath, newFileName);

            // the file wasn't selected directly from Thumbs dir, otherwise we don't need to copy it
            if (newFileInThumbsDir != newImagefilePath && !File.Exists(newFileInThumbsDir))
                File.Copy(newImagefilePath, newFileInThumbsDir);

            return newFileInThumbsDir;
        }

        internal byte[] ImageToBinary(Image image)
        {
            if (this.IsDefaultProtocolImage(image))
                return EmptyImageData;

            using (var memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, image.RawFormat);
                return memoryStream.ToArray();
            }
        }

        internal bool IsDefaultProtocolImage(Image image)
        {
            return Connection.Terminalsicon == image || this.pluginIcons.ContainsValue(image);
        }
    }
}