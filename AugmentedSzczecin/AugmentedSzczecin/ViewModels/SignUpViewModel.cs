using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Caliburn.Micro;
using Windows.UI.Xaml.Controls;
using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Views;

namespace AugmentedSzczecin.ViewModels
{
    public class SignUpViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IRegisterService _registerService;
        public SignUpViewModel(IEventAggregator eventAggregator, IRegisterService registerService)
        {
            _eventAggregator = eventAggregator;
            _registerService = registerService;
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
                if (value != _password)
                {
                    _password = value;
                    ValidatePasswordLength();
                    ValidatePasswordEmpty();
                    CheckValidation();
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
                    CheckValidation();
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

        private bool _validationCheck;

        public bool ValidationCheck
        {
            get 
            { 
                return _validationCheck; 
            }
            set 
            { 
                _validationCheck = value;
                NotifyOfPropertyChange(() => ValidationCheck);
            }
        }

        private void CheckValidation()
        {
            ValidationCheck = IsEmailEmptyValid && IsEmailMatchValid && IsPasswordLengthValid && IsPasswordEmptyValid;
        }

        
        public void Register()
        {
            if (ValidationCheck)
            {
                _registerService.Register(Email, Password);
            }
            else
                WrongValidationMessageDialog();
        }

        private async void WrongValidationMessageDialog()
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            string Message = "";
            if (!IsEmailEmptyValid)
                Message += loader.GetString("SignUpEmailEmpty") + "\n";

            if (!IsEmailMatchValid && IsEmailEmptyValid)
                Message += loader.GetString("SignUpEmailMatch") + "\n";

            if (!IsPasswordLengthValid && IsPasswordEmptyValid)
                Message += loader.GetString("SignUpPasswordLength") + "\n";

            if (!IsPasswordEmptyValid)
                Message += loader.GetString("SignUpPasswordEmpty");

            var Msg = new MessageDialog(Message);
            await Msg.ShowAsync();
            
        }
    }
}
