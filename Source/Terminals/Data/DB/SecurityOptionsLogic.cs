﻿using System;

namespace Terminals.Data.DB
{
    /// <summary>
    ///     Sql implementation of user credentials directly used on favorites.
    ///     Remember, that this type isn't used inside protocol options.
    ///     We don't use table per type mapping here, because the credential base doesn't have to be defined,
    ///     if user has selected StoredCredential. CredentialBase is implemented with lazy loading,
    ///     e.g. this item has in database its CredentialBase only
    ///     if some of its values to assigned has not null or empty value
    /// </summary>
    internal partial class DbSecurityOptions : ISecurityOptions
    {
        /// <summary>
        ///     reference to the associated credentials by its ID.
        ///     Is resolved by request from storedCredentials field.
        /// </summary>
        private int? credentialId;

        private StoredCredentials storedCredentials;

        /// <summary>
        ///     Distinguish between newly created CachedCredentials or loaded
        /// </summary>
        internal bool NewCachedCredentials { get; private set; }

        /// <summary>
        ///     Cached base properties. Loaded from reference or assigned by creation only.
        /// </summary>
        internal DbCredentialBase CachedCredentials { get; private set; }

        public string EncryptedUserName
        {
            get
            {
                return this.CachedCredentials?.EncryptedUserName;
            }
            set
            {
                if (this.CanCommitSecuredValue(value))
                {
                    this.EnsureCredentialBase();
                    this.CachedCredentials.EncryptedUserName = value;
                }
            }
        }

        public string EncryptedDomain
        {
            get
            {
                return this.CachedCredentials?.EncryptedDomain;
            }
            set
            {
                if (this.CanCommitSecuredValue(value))
                {
                    this.EnsureCredentialBase();
                    this.CachedCredentials.EncryptedDomain = value;
                }
            }
        }

        public string EncryptedPassword
        {
            get
            {
                return this.CachedCredentials?.EncryptedPassword;
            }
            set
            {
                if (this.CanCommitSecuredValue(value))
                {
                    this.EnsureCredentialBase();
                    this.CachedCredentials.EncryptedPassword = value;
                }
            }
        }

        public Guid Credential { get => this.GetCredential(); set => this.SetCredential(value); }

        public ISecurityOptions Copy()
        {
            var copy = new DbSecurityOptions();
            // newCachedCredentials doesn't have to be assigned here, because it depends on the values copied
            copy.UpdateFrom(this);
            return copy;
        }

        private bool CanCommitSecuredValue(string newValue)
        {
            // don't force store of empty value, if Security is not present in database
            // but allow clear, if security was already used
            return !string.IsNullOrEmpty(newValue) ||
                   string.IsNullOrEmpty(newValue) && this.CachedCredentials != null;
        }

        internal void AssignStores(StoredCredentials storedCredentials)
        {
            this.storedCredentials = storedCredentials;
        }

        private Guid GetCredential()
        {
            var resolved = this.ResolveCredentailFromStore();
            if (resolved != null)
                return resolved.Guid;

            return Guid.Empty;
        }

        internal DbCredentialSet ResolveCredentailFromStore()
        {
            if (this.credentialId != null)
                return this.storedCredentials[this.credentialId.Value];

            return null;
        }

        private void SetCredential(Guid value)
        {
            if (value != Guid.Empty)
                this.SetCredentialByStoreId(value);
            else
                this.credentialId = null;
        }

        private void SetCredentialByStoreId(Guid value)
        {
            var credentialToAssign = this.storedCredentials[value];
            if (credentialToAssign != null)
                this.credentialId = credentialToAssign.Id;
        }

        /// <summary>
        ///     LazyLoading of CredentialBase for favorites, where security wasn't touched until now.
        ///     The credential base doesn't have to be initialized, if used doesn't configure its properties.
        /// </summary>
        private void EnsureCredentialBase()
        {
            if (this.CachedCredentials == null)
            {
                this.CachedCredentials = new DbCredentialBase();
                this.CredentialBase = this.CachedCredentials;
                this.NewCachedCredentials = true;
            }
        }

        internal void LoadReferences(Database database)
        {
            var securityEntry = database.Entry(this);
            securityEntry.Reference(so => so.CredentialBase).Load();
            securityEntry.Reference(so => so.CredentialSet).Load();
        }

        internal void LoadFieldsFromReferences()
        {
            this.CachedCredentials = this.CredentialBase;
            this.LoadFromCredentialSetReference();
        }

        private void LoadFromCredentialSetReference()
        {
            if (this.CredentialSet != null)
                this.credentialId = this.CredentialSet.Id;
            else
                this.credentialId = null;
        }

        internal void UpdateCredentialSetReference()
        {
            if (this.credentialId != null)
                this.CredentialSet = this.storedCredentials[this.credentialId.Value];
            else
                this.CredentialSet = null;
        }

        internal void UpdateFrom(DbSecurityOptions source)
        {
            this.EncryptedUserName = source.EncryptedUserName;
            this.EncryptedDomain = source.EncryptedDomain;
            this.EncryptedPassword = source.EncryptedPassword;
            this.credentialId = source.credentialId;
            this.storedCredentials = source.storedCredentials;
        }

        public override string ToString()
        {
            if (this.CachedCredentials == null)
                return "SecurityOptions:Empty";

            return string.Format("SecurityOptions:Credential={0}", this.Credential);
        }

        /// <summary>
        ///     Because of CachedCredentialBase lazy loading, we have to mark the property as initially saved.
        /// </summary>
        internal void Save()
        {
            this.NewCachedCredentials = false;
        }
    }
}