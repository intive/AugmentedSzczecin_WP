namespace AugmentedSzczecin.Interfaces
{
    public interface IUserDataStorageService
    {
        void AddUserData(string email, string token);

        bool IsUserSignedIn();

        void SignOut();
    }
}
