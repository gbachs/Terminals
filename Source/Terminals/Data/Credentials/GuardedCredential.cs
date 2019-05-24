using Terminals.Security;

namespace Terminals.Data.Credentials
{
    internal class GuardedCredential : IGuardedCredential
    {
        private readonly ICredentialBase credential;

        internal GuardedCredential(ICredentialBase credential, PersistenceSecurity persistenceSecurity)
        {
            this.credential = credential;
            this.PersistenceSecurity = persistenceSecurity;
        }

        protected PersistenceSecurity PersistenceSecurity { get; }

        /// <summary>
        ///     Gets or sets the user name in not encrypted form. This value isn't stored.
        ///     this property needs to be public, because it is required by the validation.
        /// </summary>
        public string UserName
        {
            get => this.GetDecryptedUserName();
            set
            {
                if (string.IsNullOrEmpty(value))
                    this.credential.EncryptedUserName = string.Empty;
                else
                    this.credential.EncryptedUserName = this.PersistenceSecurity.EncryptPersistencePassword(value);
            }
        }

        public string Domain
        {
            get => this.GetDecryptedDomain();
            set
            {
                if (string.IsNullOrEmpty(value))
                    this.credential.EncryptedDomain = string.Empty;
                else
                    this.credential.EncryptedDomain = this.PersistenceSecurity.EncryptPersistencePassword(value);
            }
        }

        public string Password
        {
            get => this.GetDecryptedPassword();
            set
            {
                if (string.IsNullOrEmpty(value))
                    this.credential.EncryptedPassword = string.Empty;
                else
                    this.credential.EncryptedPassword = this.PersistenceSecurity.EncryptPersistencePassword(value);
            }
        }

        public string EncryptedPassword
        {
            get => this.credential.EncryptedPassword;
            set => this.credential.EncryptedPassword = value;
        }

        /// <summary>
        ///     Replaces stored encrypted password by new one using newKeymaterial
        /// </summary>
        /// <param name="newKeymaterial">key created from master password hash</param>
        internal void UpdatePasswordByNewKeyMaterial(string newKeymaterial)
        {
            var userName = this.GetDecryptedUserName();
            if (!string.IsNullOrEmpty(userName))
                this.credential.EncryptedUserName = PasswordFunctions2.EncryptPassword(userName, newKeymaterial);

            var domain = this.GetDecryptedDomain();
            if (!string.IsNullOrEmpty(domain))
                this.credential.EncryptedDomain = PasswordFunctions2.EncryptPassword(domain, newKeymaterial);

            var secret = this.GetDecryptedPassword();
            if (!string.IsNullOrEmpty(secret))
                this.credential.EncryptedPassword = PasswordFunctions2.EncryptPassword(secret, newKeymaterial);
        }

        private string GetDecryptedUserName()
        {
            if (!string.IsNullOrEmpty(this.credential.EncryptedUserName))
                return this.PersistenceSecurity.DecryptPersistencePassword(this.credential.EncryptedUserName);

            return string.Empty;
        }

        private string GetDecryptedDomain()
        {
            if (!string.IsNullOrEmpty(this.credential.EncryptedDomain))
                return this.PersistenceSecurity.DecryptPersistencePassword(this.credential.EncryptedDomain);

            return string.Empty;
        }

        private string GetDecryptedPassword()
        {
            if (!string.IsNullOrEmpty(this.credential.EncryptedPassword))
                return this.PersistenceSecurity.DecryptPersistencePassword(this.credential.EncryptedPassword);

            return string.Empty;
        }
    }
}