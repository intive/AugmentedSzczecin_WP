namespace AugmentedSzczecin.Interfaces
{
    public interface IAccountService
    {
        void Register(string email, string password);

        void SignIn(string email, string password);

        bool IsUserSignedIn();

        void SignOut();

        string GetUserEmail();

        string GetUserToken();
    }
}
