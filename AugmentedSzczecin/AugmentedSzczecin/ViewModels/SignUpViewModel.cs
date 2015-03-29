using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Caliburn.Micro;
using Windows.UI.Xaml.Controls;
using AugmentedSzczecin.Views;

namespace AugmentedSzczecin.ViewModels
{
    public class SignUpViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator;
        public SignUpViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
        }

        protected override void OnViewAttached(object view, object context)
        {
            _passwordBox = ((SignUpView)view).Password;
            base.OnViewAttached(view, context);
        }

        private PasswordBox _passwordBox;

        private string _password;

        public string Password
        {
            get 
            { 
                return _password; 
            }
            set 
            { 
                if(value != _password)
                {
                    _password = value;
                    ValidatePasswordLength();
                    ValidatePasswordEmpty();
                    NotifyOfPropertyChange(() => Password);
                }
            }
        }

        private bool _isPasswordLengthValid;

        public bool IsPasswordLengthValid
        {
            get
            {
                return _isPasswordLengthValid; 
            }
            set 
            {
                if (value != _isPasswordLengthValid)
                {
                    _isPasswordLengthValid = value;
                    NotifyOfPropertyChange(() => IsPasswordLengthValid);
                    NotifyOfPropertyChange(() => CanRegister);
                } 
            }
        }

        private bool _isPasswordEmptyValid;

        public bool IsPasswordEmptyValid
        {
            get
            {
                return _isPasswordEmptyValid;
            }
            set
            {
                if (value != _isPasswordEmptyValid)
                {
                    _isPasswordEmptyValid = value;
                    NotifyOfPropertyChange(() => IsPasswordEmptyValid);
                    NotifyOfPropertyChange(() => CanRegister);
                }
            }
        }

        private void ValidatePasswordLength()
        {
            IsPasswordLengthValid = _passwordBox.Password.Length >= 6;
        }

        private void ValidatePasswordEmpty()
        {
            IsPasswordEmptyValid = !String.IsNullOrEmpty(_passwordBox.Password);
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
                    ValidateEmailMatch();
                    ValidateEmailEmpty();
                    NotifyOfPropertyChange(() => Email);
                }
            }
        }

        private bool _isEmailEmptyValid;
        public bool IsEmailEmptyValid
        {
            get { return _isEmailEmptyValid; }
            set
            {
                if (_isEmailEmptyValid != value)
                {
                    _isEmailEmptyValid = value;
                    NotifyOfPropertyChange(() => IsEmailEmptyValid);
                    NotifyOfPropertyChange(() => CanRegister);
                }
            }
        }

        private bool _isEmailMatchValid;
        public bool IsEmailMatchValid
        {
            get { return _isEmailMatchValid; }
            set
            {
                if (_isEmailMatchValid != value)
                {
                    _isEmailMatchValid = value;
                    NotifyOfPropertyChange(() => IsEmailMatchValid);
                    NotifyOfPropertyChange(() => CanRegister);
                }
            }
        }

        private void ValidateEmailEmpty()
        {
            IsEmailEmptyValid = !String.IsNullOrEmpty(Email);
        }

        private void ValidateEmailMatch()
        {
            IsEmailMatchValid = Regex.IsMatch(Email,
                @"^(?("")(""[^""]+?""@)|(([0-9a-zA-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-z][-\w]*[0-9a-zA-z]*\.)+[a-zA-z0-9]{2,24}))$");
        }

        public bool CanRegister
        {
            get 
            { 
                return IsEmailEmptyValid && IsEmailMatchValid && IsPasswordLengthValid && IsPasswordEmptyValid; 
            }
        }

        public void Register()
        {
        }
    }
}
