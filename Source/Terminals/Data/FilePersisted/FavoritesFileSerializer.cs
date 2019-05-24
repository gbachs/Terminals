using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using Terminals.Connections;

namespace Terminals.Data.FilePersisted
{
    internal class FavoritesFileSerializer
    {
        private readonly ConnectionManager connectinManager;

        internal FavoritesFileSerializer(ConnectionManager connectinManager)
        {
            this.connectinManager = connectinManager;
        }

        internal void Serialize(SerializationContext context, string fileName)
        {
            var document = new XDocument();
            this.Serialize(context, document);
            var file = FavoritesXmlFile.CreateDocument(document);
            file.AppenUnknownContent(context.Unknown);
            document.Save(fileName);
        }

        private void Serialize(SerializationContext context, XDocument document)
        {
            using (var writer = document.CreateWriter())
            {
                var serializer = this.CreateSerializer();
                serializer.Serialize(writer, context.File);
            }
        }

        internal SerializationContext Deserialize(string fileLocation)
        {
            if (!File.Exists(fileLocation))
                return new SerializationContext();

            return this.TryDeserialize(fileLocation);
        }

        private SerializationContext TryDeserialize(string fileLocation)
        {
            var availableProtocols = this.connectinManager.GetAvailableProtocols();
            var document = FavoritesXmlFile.LoadXmlDocument(fileLocation);
            var unknown = document.RemoveUnknownFavorites(availableProtocols);
            var serializer = this.CreateSerializer();
            var loaded = DeSerialize(document, serializer);

            if (loaded != null)
                return new SerializationContext(loaded, unknown);

            return new SerializationContext();
        }

        private static FavoritesFile DeSerialize(FavoritesXmlFile document, XmlSerializer serializer)
        {
            using (var xmlReader = document.CreateReader())
            {
                return serializer.Deserialize(xmlReader) as FavoritesFile;
            }
        }

        private XmlSerializer CreateSerializer()
        {
            var attributes = this.CreateAttributes();
            return new XmlSerializer(typeof(FavoritesFile), attributes);
        }

        private XmlAttributeOverrides CreateAttributes()
        {
            var extraTypes = this.connectinManager.GetAllKnownProtocolOptionTypes();
            var attributeOverrides = new XmlAttributeOverrides();
            var listAttribs = new XmlAttributes();

            foreach (var extraType in extraTypes)
                listAttribs.XmlElements.Add(new XmlElementAttribute(extraType.Name, extraType));

            attributeOverrides.Add(typeof(Favorite), "ProtocolProperties", listAttribs);
            return attributeOverrides;
        }
    }
}