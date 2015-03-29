using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AugmentedSzczecin.Interfaces
{
    public interface IRegisterService
    {
        void Register(string email, string password);
    }
}
