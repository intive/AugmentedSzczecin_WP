using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Caliburn.Micro;

namespace AugmentedSzczecin.ViewModels
{
    public class SignUpViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator;
        public SignUpViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        private void ValidateEmail()
        {
            IsEmailValid = (!String.IsNullOrEmpty(Email) &&
                            Regex.IsMatch(Email,
                                @"^(?("")(""[^""]+?""@)|(([0-9a-zA-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-z])@))" +
                                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-z][-\w]*[0-9a-zA-z]*\.)+[a-zA-z0-9]{2,24}))$"));
        }

        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                if (_email != value)
                {
                    _email = value;
                    ValidateEmail();
                    NotifyOfPropertyChange(() => Email);
                }
            }
        }

        private bool _isEmailValid;
        public bool IsEmailValid
        {
            get { return _isEmailValid; }
            set
            {
                _isEmailValid = value;
                NotifyOfPropertyChange(() => IsEmailValid);
                NotifyOfPropertyChange(() => CanRegister);
            }
        }

        public bool CanRegister
        {
            get { return IsEmailValid; }
        }

        public void Register()
        {
        }
    }
}
