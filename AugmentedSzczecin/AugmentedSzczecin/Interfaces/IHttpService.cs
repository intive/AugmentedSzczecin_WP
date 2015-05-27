using AugmentedSzczecin.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AugmentedSzczecin.Interfaces
{
    public interface IHttpService
    {
        Task<ObservableCollection<PointOfInterest>> GetPointOfInterestList();

        Task<ObservableCollection<PointOfInterest>> GetPointOfInterestList(double latitude, double longitude, int radius, CategoryType category);

        Task<bool> CreateAccount(User user);

        Task<bool> SignIn(User user);

        void SignOut();

        void SetAuthenticationHeader(string email, string password);

        Task<bool> ResetPassword(User user);

        Task<bool> AddPointOfInterest(PointOfInterest poi);
    }
}
