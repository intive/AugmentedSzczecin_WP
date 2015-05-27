using AugmentedSzczecin.Interfaces;
using System.Collections.Generic;
using Windows.Security.Credentials;
using Windows.UI.Popups;
namespace AugmentedSzczecin.Services
{
    public class UserDataStorageService : IUserDataStorageService
    {
        public void AddUserData(string resource, string email, string password)
        {
            var vault = new PasswordVault();
            vault.Add(new PasswordCredential(resource, email, password));
        }

        public bool IsUserSignedIn()
        {
            var vault = new PasswordVault();
            var credentials = vault.RetrieveAll();
            if (credentials.Count != 0)
            {
                return true;
            }
            return false;
        }

        public void SignOut()
        {
            var vault = new PasswordVault();
            var credentials = vault.RetrieveAll();

            if (credentials.Count != 0)
            {
                for (int i = 0; i < credentials.Count; i++)
                {
                    vault.Remove((vault.RetrieveAll())[i]);
                }
            }
        }
    
        public string GetUserEmail()
        {
            var vault = new PasswordVault();
            var credentials = vault.RetrieveAll();
            return credentials[0].UserName;
        }

        public string GetUserPassword()
        {
            var vault = new PasswordVault();
            var credentials = vault.RetrieveAll();
            credentials[0].RetrievePassword();
            return credentials[0].Password;
        }
    }
}
