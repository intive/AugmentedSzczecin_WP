using AugmentedSzczecin.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AugmentedSzczecin.Interfaces
{
    public interface IHttpService
    {
        Task<ObservableCollection<PointOfInterest>> GetPointOfInterestsList();

        Task<User> CreateAccount(User user);

        Task<Token> SignIn(User user);

        Task<User> ResetPassword(User user);
    }
}