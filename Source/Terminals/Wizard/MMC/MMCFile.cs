using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using IconHandler;

namespace Terminals.Wizard.MMC
{
    internal class MMCFile
    {
        private readonly FileInfo mmcFileInfo;

        public string Name;

        public bool Parsed;

        private string rawContents;

        public Icon SmallIcon;

        public MMCFile(FileInfo MMCFile)
        {
            this.mmcFileInfo = MMCFile;
            this.Parse();
        }

        public MMCFile(string MMCFile)
        {
            if (File.Exists(MMCFile))
            {
                this.mmcFileInfo = new FileInfo(MMCFile);
                this.Parse();
            }
        }

        protected void Parse()
        {
            try
            {
                this.rawContents = File.ReadAllText(this.mmcFileInfo.FullName, Encoding.Default);
                if (this.rawContents != null && this.rawContents.Trim() != "" && this.rawContents.StartsWith("<?xml"))
                {
                    var xDoc = new XmlDocument();
                    xDoc.LoadXml(this.rawContents);
                    var node = xDoc.SelectSingleNode("/MMC_ConsoleFile/StringTables/StringTable/Strings");
                    foreach (XmlNode cNode in node.ChildNodes)
                    {
                        var name = cNode.InnerText;
                        if (name != "Favorites" && name != "Console Root")
                        {
                            this.Name = name;
                            this.Parsed = true;
                            break;
                        }
                    }

                    //System.Xml.XmlNode binarynode = xDoc.SelectSingleNode("/MMC_ConsoleFile/BinaryStorage");
                    //foreach (System.Xml.XmlNode child in binarynode.ChildNodes)
                    //{
                    //    string childname = child.Attributes["Name"].Value;
                    //    if (childname.ToLower().Contains("small"))
                    //    {
                    //        string image = child.InnerText;
                    //        byte[] buff = System.Convert.FromBase64String(child.InnerText.Trim());
                    //        System.IO.MemoryStream stm = new System.IO.MemoryStream(buff);
                    //        if (stm.Position > 0 && stm.CanSeek) stm.Seek(0, System.IO.SeekOrigin.Begin);
                    //        System.IO.File.WriteAllBytes(@"C:\Users\Administrator\Desktop\foo.ico", buff);
                    //        System.Drawing.Icon ico = new System.Drawing.Icon(stm);

                    //    }
                    //}

                    var visual = xDoc.SelectSingleNode("/MMC_ConsoleFile/VisualAttributes/Icon");
                    if (visual != null)
                    {
                        var iconFile = visual.Attributes["File"].Value;
                        var index = Convert.ToInt32(visual.Attributes["Index"].Value);
                        var icons = IconHandler.IconHandler.IconsFromFile(iconFile, IconSize.Small);
                        if (icons != null && icons.Length > 0)
                        {
                            if (icons.Length > index)
                                this.SmallIcon = icons[index];
                            else
                                this.SmallIcon = icons[0];
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Logging.Error("Error parsing MMC File", exc);
            }
        }
    }
}