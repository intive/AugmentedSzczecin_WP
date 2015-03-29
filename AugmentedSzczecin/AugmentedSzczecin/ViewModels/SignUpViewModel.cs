using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        
        public bool CanRegister
        {
            get
            {
                return IsPasswordLengthValid && IsPasswordEmptyValid;
            }
        }

        public void Register()
        {

        }
    }
}
