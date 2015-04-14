using System;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using AugmentedSzczecin.Events;
using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Views;
using Caliburn.Micro;
using Windows.System;
using Windows.UI.Xaml;

namespace AugmentedSzczecin.ViewModels
{
    public class SignUpViewModel : Screen, IHandle<RegisterSuccessEvent>, IHandle<RegisterFailedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IAccountService _accountService;
        private PasswordBox _passwordBox;

        public SignUpViewModel(IEventAggregator eventAggregator, IAccountService accountService)
        {
            _eventAggregator = eventAggregator;
            _accountService = accountService;
        }

        private bool _isProgressRingVisible;
        public bool IsProgressRingVisible
        {
            get
            {
                return _isProgressRingVisible;
            }
            set
            {
                if (value != _isProgressRingVisible)
                {
                    _isProgressRingVisible = value;
                    NotifyOfPropertyChange(() => IsProgressRingVisible);
                }
            }
        }

        private bool _isProgressRingActive;
        public bool IsProgressRingActive
        {
            get
            {
                return _isProgressRingActive;
            }
            set
            {
                if (value != _isProgressRingActive)
                {
                    _isProgressRingActive = value;
                    NotifyOfPropertyChange(() => IsProgressRingActive);
                }
            }
        }

        private bool _areControlsEnabled = true;
        public bool AreControlsEnabled
        {
            get
            {
                return _areControlsEnabled;
            }
            set
            {
                if (value != _areControlsEnabled)
                {
                    _areControlsEnabled = value;
                    NotifyOfPropertyChange(() => AreControlsEnabled);
                }
            }
        }

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

        private string _email;
        public string Email
        {
            get
            {
                return _email;
            }
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
            get
            {
                return _isEmailEmptyValid;
            }
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
            get
            {
                return _isEmailMatchValid;
            }
            set
            {
                if (_isEmailMatchValid != value)
                {
                    _isEmailMatchValid = value;
                    NotifyOfPropertyChange(() => IsEmailMatchValid);
                }
            }
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

        protected override void OnActivate()
        {
            _eventAggregator.Subscribe(this);
            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            _eventAggregator.Unsubscribe(this);
            base.OnDeactivate(close);
        }

        protected override void OnViewAttached(object view, object context)
        {
            _passwordBox = ((SignUpView)view).Password;
            base.OnViewAttached(view, context);
        }

        public void Register()
        {
            if (ValidationCheck)
            {
                AreControlsEnabled = false;
                IsProgressRingVisible = true;
                IsProgressRingActive = true;
                _accountService.Register(Email, Password);
            }
            else
            {
                WrongValidationMessageDialog();
            }
        }

        public void Handle(RegisterFailedEvent e)
        {
            AreControlsEnabled = true;
            IsProgressRingVisible = false;
            IsProgressRingActive = false;
            var msg = new MessageDialog(e.FailMessage);
            msg.ShowAsync();
        }

        public void Handle(RegisterSuccessEvent e)
        {
            AreControlsEnabled = true;
            IsProgressRingVisible = false;
            IsProgressRingActive = false;
            var msg = new MessageDialog(e.SuccessMessage);
            msg.ShowAsync();
        }

        public void EmailTextBoxChangeFocusToPasswordTextBox(ActionExecutionContext context)
        {
            if (((Windows.UI.Xaml.Input.KeyRoutedEventArgs)context.EventArgs).Key == VirtualKey.Enter)
            {
                _passwordBox.Focus(FocusState.Keyboard);
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

        private void CheckValidation()
        {
            ValidationCheck = IsEmailEmptyValid && IsEmailMatchValid && IsPasswordLengthValid && IsPasswordEmptyValid;
        }

        private async void WrongValidationMessageDialog()
        {
            var loader = new ResourceLoader();
            string message = "";

            if (!IsPasswordEmptyValid && !IsEmailEmptyValid)
            {
                message += loader.GetString("SignUpEmptyForm");
            }
            else
            {
                if (!IsEmailEmptyValid)
                {
                    message += loader.GetString("SignUpEmailEmpty") + "\n";
                }
                if (!IsEmailMatchValid && IsEmailEmptyValid)
                {
                    message += loader.GetString("SignUpEmailMatch") + "\n";
                }
                if (!IsPasswordLengthValid && IsPasswordEmptyValid)
                {
                    message += loader.GetString("SignUpPasswordLength") + "\n";
                }

                if (!IsPasswordEmptyValid)
                {
                    message += loader.GetString("SignUpPasswordEmpty");
                }
            }

            var msg = new MessageDialog(message);
            await msg.ShowAsync();
        }
    }
}
