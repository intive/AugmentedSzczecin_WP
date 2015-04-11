using AugmentedSzczecin.Models;
using System.Threading.Tasks;

namespace AugmentedSzczecin.Interfaces
{
    public interface IHttpService
    {
        Task<User> CreateAccount(User user);

        Task<Token> SignIn(User user);
    }
}
