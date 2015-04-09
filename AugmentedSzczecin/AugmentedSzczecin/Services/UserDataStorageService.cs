
using AugmentedSzczecin.Interfaces;
using System.Collections.Generic;
using Windows.Security.Credentials;
namespace AugmentedSzczecin.Services
{
    public class UserDataStorageService : IUserDataStorageService
    {
        public void AddUserData(string email, string token)
        {
            var vault = new PasswordVault();
            var nr = vault.RetrieveAll().Count;
            vault.Add(new PasswordCredential("AugmentedSzczecinUserData", email, token));
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
            if(vault.RetrieveAll().Count != 0)
            {
                vault.Remove((vault.RetrieveAll())[0]);
            }
        }
    }
}
