using AugmentedSzczecin.Interfaces;
using System.Collections.Generic;
using Windows.Security.Credentials;
using Windows.UI.Popups;

namespace AugmentedSzczecin.Services
{
    public class UserDataStorageService : IUserDataStorageService
    {
        public void AddUserData(string resource, string email, string token)
        {
            var vault = new PasswordVault();
            vault.Add(new PasswordCredential(resource, email, token));
        }

        public bool IsUserSignedIn()
        {
            var vault = new PasswordVault();
            var UserList = vault.RetrieveAll();
            if (UserList.Count != 0)
            {
                return true;
            }

            return false;
        }

        public void SignOut()
        {
            var vault = new PasswordVault();
            IReadOnlyList<PasswordCredential> userDatalist = vault.RetrieveAll();

            if (userDatalist.Count != 0)
            {
                for (int i = userDatalist.Count - 1; i >= 0; i--)
                {
                    vault.Remove((vault.RetrieveAll())[i]);
                }
            }
        }

        public string GetUserEmail()
        {
            var vault = new PasswordVault();
            IReadOnlyList<PasswordCredential> userDatalist = vault.RetrieveAll();

            string email = userDatalist[0].UserName;

            return email;
        }

        public string GetUserToken()
        {
            var vault = new PasswordVault();
            IReadOnlyList<PasswordCredential> userDatalist = vault.RetrieveAll();

            userDatalist[1].RetrievePassword();
            string token = userDatalist[1].Password;

            return token;
        }
    }
}