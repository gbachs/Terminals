using Terminals.Common.Connections;
using Terminals.Connections;
using Terminals.Data.Credentials;

namespace Terminals.Data
{
    internal class ModelConvertersTemplate
    {
        protected ModelConvertersTemplate(IPersistence persistence, ConnectionManager connectionManager)
        {
            this.Persistence = persistence;
            this.CredentialFactory = new GuardedCredentialFactory(this.Persistence);
            this.ConnectionManager = connectionManager;
        }

        protected ConnectionManager ConnectionManager { get; }

        protected IPersistence Persistence { get; }

        protected IGuardedCredentialFactory CredentialFactory { get; }

        protected IOptionsConverter CreateOptionsConverter(string protocolName)
        {
            var converterFactory = this.ConnectionManager.GetOptionsConverterFactory(protocolName);
            return converterFactory.CreatOptionsConverter();
        }
    }
}