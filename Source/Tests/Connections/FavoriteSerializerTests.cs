﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Terminals.Common.Connections;
using Terminals.Connections.VMRC;
using Terminals.Connections.VNC;
using Terminals.Data;
using Terminals.Data.FilePersisted;
using Terminals.Plugins.Putty;

namespace Tests.Connections
{
    [TestClass]
    public class FavoriteSerializerTests
    {
        private const string FILE_NAME = "SerializationTest.xml";

        private const string VNC_ELEMENT = @"
     <Favorite id=""d0a609d9-09a4-4f8d-8ed3-e58048a2369d"" xmlns=""http://Terminals.codeplex.com"">
      <Protocol>VNC</Protocol>
      <Port>3389</Port>
      <Security />
      <NewWindow>false</NewWindow>
      <ExecuteBeforeConnect>
        <Execute>false</Execute>
        <WaitForExit>false</WaitForExit>
      </ExecuteBeforeConnect>
      <Display>
        <Height>0</Height>
        <Width>0</Width>
        <DesktopSize>FitToWindow</DesktopSize>
        <Colors>Bits32</Colors>
      </Display>
      <VncOptions>
        <AutoScale>false</AutoScale>
        <ViewOnly>false</ViewOnly>
        <DisplayNumber>0</DisplayNumber>
      </VncOptions>
    </Favorite>";

        private const string GROUP_NAME = "innerGroup";

        private const string GROUP_ID = "3fde996d-bcf8-4f4a-b4ed-a7fab81f7967";

        private const string VNC_ID = "477025bd-a8dd-4d95-bc70-b25dc7dc6c87";

        private static readonly Guid GROUP_GUID = new Guid(GROUP_ID);

        private static readonly Guid VNC_GUID = new Guid(VNC_ID);

        private static readonly Guid RDP_GUID = new Guid("aea91f1f-c2d8-429d-a2ad-cc915b637881");

        private static readonly Favorite VNC_FAVORITE = ToFavorite(VncConnectionPlugin.VNC, VNC_GUID);
        private static readonly Favorite RDP_FAVORITE = ToFavorite(KnownConnectionConstants.RDP, RDP_GUID);

        private static readonly string UNKNOWN_VNC_GUID = String.Format("<guid>{0}</guid>", VNC_ID);

        private static readonly XElement UNKNOWN_VNC = XDocument.Parse(UNKNOWN_VNC_GUID).Root;

        private static readonly XElement vncCachedFavorite = XDocument.Parse(VNC_ELEMENT).Root;


        private static readonly Tuple<string, Type>[] testCases = new Tuple<string, Type>[]
        {
            new Tuple<string, Type>(KnownConnectionConstants.RDP, typeof(RdpOptions)),
            new Tuple<string, Type>(VncConnectionPlugin.VNC, typeof(VncOptions)),
            new Tuple<string, Type>(VmrcConnectionPlugin.VMRC, typeof(VMRCOptions)),
            new Tuple<string, Type>(TelnetConnectionPlugin.TELNET, typeof(TelnetOptions)),
            new Tuple<string, Type>(SshConnectionPlugin.SSH, typeof(SshOptions))
        };

        [TestMethod]
        public void RdpOnlyPluginAndCachedVncXml_Serialize_SavesUnknonwCachedFavorite()
        {
            string saved = SerializeAndLoadSavedContent();
            bool savedVnc = saved.Contains("<Protocol>VNC</Protocol>");
            Assert.IsTrue(savedVnc, "The saved content has to contain both known and unknown protocol elements.");
        }

        [TestMethod]
        public void RdpOnlyPluginAndCachedVncXml_Serialize_SavesUnknonwGroupMembership()
        {
            string saved = SerializeAndLoadSavedContent();
            bool savedVncGroupMembership = saved.Contains(VNC_ID + "</guid>");
            Assert.IsTrue(savedVncGroupMembership, "The saved content has to contain both known and unknown favorites group memberhips.");
        }

        private static string SerializeAndLoadSavedContent()
        {
            var rdpOnlyManager = TestConnectionManager.CreateRdpOnlyManager();
            FavoritesFile file = CreateTestFile(KnownConnectionConstants.RDP);
            var unknownElements = new UnknonwPluginElements();
            unknownElements.Favorites.Add(vncCachedFavorite);
            unknownElements.GroupMembership[GROUP_ID] = new List<XElement>() {UNKNOWN_VNC};
            var context = new SerializationContext(file, unknownElements);
            var limitedSerializer = new FavoritesFileSerializer(rdpOnlyManager);

            limitedSerializer.Serialize(context, FILE_NAME);
            string saved = File.ReadAllText(FILE_NAME);
            return saved;
        }

        [TestMethod]
        public void RdpOnlyPlugin_Deserialize_LoadsRdpGroupMembershipAsKnown()
        {
            SerializationContext loaded = SerializeRdpVncDeserializeRdpOnly();
            FavoritesFile favoritesFile = loaded.File;
            bool rdpInGroup = favoritesFile.FavoritesInGroups.Where(fg => fg.GroupId == GROUP_GUID)
                .Any(gm => gm.Favorites.Contains(RDP_GUID));
            Assert.IsTrue(rdpInGroup, "Known favorites membership should be identified.");
        }

        [TestMethod]
        public void RdpOnlyPlugin_Deserialize_LoadsVncGroupMembershipAsUnKnown()
        {
            SerializationContext loaded = SerializeRdpVncDeserializeRdpOnly();
            List<XElement> groupMembers = loaded.Unknown.GroupMembership[GROUP_ID];
            bool rdpInGroup = groupMembers.Any(fg => fg.Value.Contains(VNC_ID));
            Assert.IsTrue(rdpInGroup, "Unknown favorites membership should be protected for next save.");
        }

        [TestMethod]
        public void RdpOnlyPlugin_Deserialize_LoadsVncAsUnknown()
        {
            AssertDeserializedWithRdpOnlyPlugin(VncConnectionPlugin.VNC, true, false);
        }

        [TestMethod]
        public void RdpOnlyPlugin_Deserialize_LoadsRdpAsKnown()
        {
            AssertDeserializedWithRdpOnlyPlugin(KnownConnectionConstants.RDP, false, true);
        }

        private static void AssertDeserializedWithRdpOnlyPlugin(string expectedProtocol, bool expectedUnknown, bool expectedKnown)
        {
            SerializationContext loaded = SerializeRdpVncDeserializeRdpOnly();
            AssertDeserializedFavorite(loaded, expectedProtocol, expectedUnknown, expectedKnown);
        }

        private static SerializationContext SerializeRdpVncDeserializeRdpOnly()
        {
            var fullSerializer = new FavoritesFileSerializer(TestConnectionManager.Instance);
            FavoritesFile file = CreateTestFile(RDP_FAVORITE, VNC_FAVORITE);
            var context = new SerializationContext(file, new UnknonwPluginElements());
            fullSerializer.Serialize(context, FILE_NAME);
            var rdpOnlyManager = TestConnectionManager.CreateRdpOnlyManager();
            var limitedSerializer = new FavoritesFileSerializer(rdpOnlyManager);
            return limitedSerializer.Deserialize(FILE_NAME);
        }

        private static void AssertDeserializedFavorite(SerializationContext loaded, string expectedProtocol,
            bool expectedUnknown, bool expectedKnown)
        {
            bool hasUnknown = loaded.Unknown.Favorites.Any(e => e.Value.Contains(expectedProtocol));
            bool hasKnown = loaded.File.Favorites.Any(f => f.Protocol == expectedProtocol);
            const string MESSAGE = "Deserialized '{0}' as Unknown = '{1}' (expected '{2}') and Known = '{3}' (expected '{4}').";
            Console.WriteLine(MESSAGE, expectedProtocol, hasUnknown, expectedUnknown, hasKnown, expectedKnown);
            bool passed = hasUnknown == expectedUnknown && hasKnown == expectedKnown;
            Assert.IsTrue(passed, "The not identified favorites cant be lost.");
        }

        [TestMethod]
        public void AllProtocols_SerializeDeserializeToXml_RestoresValues()
        {
            bool allValid = testCases.All(RestoreXmlSerializedFavorite);
            const string MESSAGE = "Without working roundtrip of data serialization we are not able to persist the options.";
            Assert.IsTrue(allValid, MESSAGE);
        }
        
        private static bool RestoreXmlSerializedFavorite(Tuple<string, Type> testCase)
        {
            var serializer = new FavoritesFileSerializer(TestConnectionManager.Instance);
            FavoritesFile file = CreateTestFile(testCase.Item1);
            var context = new SerializationContext(file, new UnknonwPluginElements());
            serializer.Serialize(context, FILE_NAME);
            SerializationContext loaded = serializer.Deserialize(FILE_NAME);
            Favorite target = loaded.File.Favorites[0];
            return target.ProtocolProperties.GetType().FullName == testCase.Item2.FullName;
        }

        private static FavoritesFile CreateTestFile(params string[] protocols)
        {
            Favorite[] favorites = protocols.Select(ToFavorite).ToArray();
            return CreateTestFile(favorites);
        }

        private static FavoritesFile CreateTestFile(params Favorite[] favorites)
        {
            var file = new FavoritesFile();
            file.Favorites = favorites;
            file.Groups = CreateGroups();
            file.FavoritesInGroups = SelectFavoriteIds(favorites);
            return file;
        }

        private static FavoritesInGroup[] SelectFavoriteIds(Favorite[] favorites)
        {
            var groupMembers = new FavoritesInGroup()
            {
                GroupId = GROUP_GUID,
                Favorites = favorites.Select(f => f.Id).ToArray()
            };

            return new FavoritesInGroup[] { groupMembers };
        }

        private static Group[] CreateGroups()
        {
            var group = new Group(GROUP_NAME)
            {
                Id = GROUP_GUID
            };

            return new Group[] {group};
        }

        private static Favorite ToFavorite(string protocol, Guid id)
        {
            var favorite = ToFavorite(protocol);
            favorite.Id = id;
            return favorite;
        }

        private static Favorite ToFavorite(string protocol)
        {
            var favorite = new Favorite();
            TestConnectionManager.Instance.ChangeProtocol(favorite, protocol);
            return favorite;
        }
    }
}
