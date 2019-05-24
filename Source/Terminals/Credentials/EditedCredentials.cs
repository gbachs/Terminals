using Terminals.Data;

namespace Terminals.Credentials
{
    internal class EditedCredentials
    {
        public EditedCredentials(ICredentialSet credentialSet, string userName, string domain)
        {
            this.UserName = userName;
            this.Domain = domain;
            this.Edited = credentialSet;
        }

        public string Name => this.Edited.Name;

        public string UserName { get; }

        public string Domain { get; }

        public ICredentialSet Edited { get; }
    }
}