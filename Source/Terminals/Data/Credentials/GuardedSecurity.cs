using System;
using Terminals.Configuration;

namespace Terminals.Data.Credentials
{
    internal class GuardedSecurity : GuardedCredential, IGuardedSecurity
    {
        private readonly ICredentials credentials;

        private readonly IPersistence persistence;

        private readonly ISecurityOptions securityOptions;

        internal GuardedSecurity(IPersistence persistence, ISecurityOptions securityOptions)
            : base(securityOptions, persistence.Security)
        {
            this.persistence = persistence;
            this.credentials = persistence.Credentials;
            this.securityOptions = securityOptions;
        }

        public IGuardedSecurity GetResolvedCredentials()
        {
            var resolved = this.securityOptions.Copy();
            IGuardedSecurity result = new GuardedSecurity(this.persistence, resolved);
            this.ResolveCredentials(resolved, this.securityOptions.Credential);
            return result;
        }

        public void UpdateFromCredential(ICredentialSet credentials)
        {
            this.UpdateFromCredential(credentials, this.securityOptions);
        }

        public void ResolveCredentials(ISecurityOptions result, Guid credentialId)
        {
            var source = this.credentials[credentialId];
            this.UpdateFromCredential(source, result);
            this.UpdateFromDefaultValues(result);
        }

        private void UpdateFromDefaultValues(ICredentialBase target)
        {
            var settings = Settings.Instance;
            var guarded = new GuardedCredential(target, this.PersistenceSecurity);

            if (string.IsNullOrEmpty(guarded.Domain))
                guarded.Domain = settings.DefaultDomain;

            if (string.IsNullOrEmpty(guarded.UserName))
                guarded.UserName = settings.DefaultUsername;

            if (string.IsNullOrEmpty(guarded.Password))
                guarded.Password = settings.DefaultPassword;
        }

        private void UpdateFromCredential(ICredentialSet source, ISecurityOptions target)
        {
            if (source != null)
            {
                target.Credential = source.Id;
                var guardedSource = new GuardedCredential(source, this.PersistenceSecurity);
                var guardedTarget = new GuardedCredential(target, this.PersistenceSecurity);
                guardedTarget.Domain = guardedSource.Domain;
                guardedTarget.UserName = guardedSource.UserName;
                target.EncryptedPassword = source.EncryptedPassword;
            }
        }
    }
}