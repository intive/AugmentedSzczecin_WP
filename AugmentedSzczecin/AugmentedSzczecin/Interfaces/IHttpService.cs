using AugmentedSzczecin.Models;
using System.Threading.Tasks;

namespace AugmentedSzczecin.Interfaces
{
    public interface IHttpService
    {
        public Task<User> CreateAccount(User user);

        public Task<Token> SignIn(User user);
    }
}
