using Terminals.Data;
using Terminals.Data.Credentials;

namespace Terminals.Configuration
{
    internal class FavoriteConfigurationSecurity
    {
        private readonly ICredentials credentials;

        private readonly FavoriteConfigurationElement favorite;

        private readonly PersistenceSecurity security;

        internal FavoriteConfigurationSecurity(IPersistence persistence, FavoriteConfigurationElement favorite)
        {
            this.security = persistence.Security;
            this.credentials = persistence.Credentials;
            this.favorite = favorite;
        }

        internal string TsgwPassword
        {
            get => this.security.DecryptPassword(this.favorite.TsgwEncryptedPassword);
            set => this.favorite.TsgwEncryptedPassword = this.security.EncryptPassword(value);
        }

        /// <summary>
        ///     Gets or sets the password String in not encrypted form
        /// </summary>
        internal string Password
        {
            get
            {
                var cred = this.credentials[this.favorite.Credential];
                if (cred != null)
                {
                    var guarded = new GuardedCredential(cred, this.security);
                    return guarded.Password;
                }

                return this.security.DecryptPassword(this.favorite.EncryptedPassword);
            }
            set => this.favorite.EncryptedPassword = this.security.EncryptPassword(value);
        }

        internal string ResolveDomainName()
        {
            var cred = this.credentials[this.favorite.Credential];

            if (cred != null)
            {
                var guarded = new GuardedCredential(cred, this.security);
                return guarded.Domain;
            }

            return this.favorite.DomainName;
        }

        public string ResolveUserName()
        {
            if (!string.IsNullOrEmpty(this.favorite.Credential))
            {
                var cred = this.credentials[this.favorite.Credential];

                if (cred != null)
                {
                    var guarded = new GuardedCredential(cred, this.security);
                    return guarded.UserName;
                }
            }

            return this.favorite.UserName;
        }
    }
}