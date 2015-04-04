namespace AugmentedSzczecin.Interfaces
{
    public interface IUserDataStorageService
    {
        void AddUserData(string email, string password);

        bool IsUserSignedIn();

        void SignOut();
    }
}
