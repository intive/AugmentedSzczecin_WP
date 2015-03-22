using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AugmentedSzczecin.ViewModels
{
    class SignUpViewModel
    {
        public SignUpViewModel()
        {
            var Loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            var Str = Loader.GetString("SignUpEmail");
            Str = Loader.GetString("SignUpPassword");
            Str = Loader.GetString("SignUpButtonSignUp");
        }
    }
}
