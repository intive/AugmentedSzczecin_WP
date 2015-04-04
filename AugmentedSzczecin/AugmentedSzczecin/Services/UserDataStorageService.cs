
using AugmentedSzczecin.Interfaces;
using System.Collections.Generic;
using Windows.Security.Credentials;
namespace AugmentedSzczecin.Services
{
    public class UserDataStorageService : IUserDataStorageService
    {
        public void AddUserData(string email, string password)
        {
            var vault = new PasswordVault();
            vault.Add(new PasswordCredential("AugmentedSzczecinUserData", email, password));
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
            vault.Remove((vault.RetrieveAll())[0]);
        }
    }
}
