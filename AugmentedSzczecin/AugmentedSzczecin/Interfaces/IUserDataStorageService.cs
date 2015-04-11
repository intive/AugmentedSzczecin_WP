namespace AugmentedSzczecin.Interfaces
{
    public interface IUserDataStorageService
    {
        void AddUserData(string resource, string email, string token);

        bool IsUserSignedIn();

        void SignOut();

        string GetUserEmail();

        string GetUserToken();
    }
}
