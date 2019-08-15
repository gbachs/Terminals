using Terminals.Data;
using Terminals.Data.Credentials;

namespace Terminals.Forms
{
    /// <summary>
    /// Favorite view model 1:1 wrapper around IFavorite. To be bound in Organize favorites form, 
    /// because we need flatern complex properties like UserName obtained from Security.
    /// WinForms grid is not able to bound "Security.UserName" as DataProperty.
    /// </summary>
    internal class FavoriteViewModel
    {
        private readonly ICredentials storedCredentials;

        private readonly GuardedSecurity guarded;

        internal IFavorite Favorite { get; private set; }
        
        public string Name
        {
            get => this.Favorite.Name;
            set => this.Favorite.Name = value;
        }

        public string ServerName => this.Favorite.ServerName;

        public string Protocol => this.Favorite.Protocol;

        public string UserName => this.guarded.UserName;

        public string Credential
        {
            get
            {
                var credentialSet = this.storedCredentials[this.Favorite.Security.Credential];
                if (credentialSet != null)
                    return credentialSet.Name;
                
                return string.Empty;
            }
        }

        public string GroupNames => this.Favorite.GroupNames;

        public string Notes => this.Favorite.Notes;

        internal FavoriteViewModel(IFavorite favorite, IPersistence persistence)
        {
            this.Favorite = favorite;
            this.storedCredentials = persistence.Credentials;
            this.guarded = new GuardedSecurity(persistence, favorite.Security);
        }
    }
}
