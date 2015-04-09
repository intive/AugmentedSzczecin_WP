﻿using AugmentedSzczecin.Events;
using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Views;
using Caliburn.Micro;
using System;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using Windows.Security.Credentials;
using Windows.UI.Popups;

namespace AugmentedSzczecin.ViewModels
{
    public class SignInViewModel : Screen, IHandle<SignInSuccessEvent>, IHandle<SignInFailedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ISignInService _signInService;
        private readonly IUserDataStorageService _userDataStorageService;
        private Windows.UI.Xaml.Controls.PasswordBox _passwordBox;

        public SignInViewModel(IEventAggregator eventAggregator, ISignInService signInService, IUserDataStorageService userDataStorageService)
        {
            _eventAggregator = eventAggregator;
            _signInService = signInService;
            _userDataStorageService = userDataStorageService;
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
            _passwordBox = ((SignInView)view).Password;
            base.OnViewAttached(view, context);
        }

        private bool _isProgressRingVisible;
        public bool IsProgressRingVisible
        {
            get { return _isProgressRingVisible; }
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
            get { return _isProgressRingActive; }
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
            get { return _areControlsEnabled; }
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
            get { return _password; }
            set
            {
                if (value != _password)
                {
                    _password = value;
                    ValidatePasswordEmpty();
                    NotifyOfPropertyChange(() => Password);
                }
            }
        }

        private bool _isPasswordEmptyValid;
        public bool IsPasswordEmptyValid
        {
            get { return _isPasswordEmptyValid; }
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
            get { return _email; }
            set
            {
                if (_email != value)
                {
                    _email = value;
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
                }
            }
        }

        private bool _validationCheck;
        public bool ValidationCheck
        {
            get { return _validationCheck; }
            set
            {
                _validationCheck = value;
                NotifyOfPropertyChange(() => ValidationCheck);
            }
        }

        private void ValidatePasswordEmpty()
        {
            IsPasswordEmptyValid = !String.IsNullOrEmpty(_passwordBox.Password);
        }

        private void ValidateEmailEmpty()
        {
            IsEmailEmptyValid = !String.IsNullOrEmpty(Email);
        }

        private void CheckValidation()
        {
            ValidationCheck = IsEmailEmptyValid && IsPasswordEmptyValid;
        }

        public void SignIn()
        {
            CheckValidation();
            if (ValidationCheck)
            {
                AreControlsEnabled = false;
                IsProgressRingVisible = true;
                IsProgressRingActive = true;
                _signInService.SignIn(Email, Password);
            }
            else
            {
                WrongValidationMessageDialog();
            }
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
                if (!IsPasswordEmptyValid)
                {
                    message += loader.GetString("SignUpPasswordEmpty");
                }
            }

            var msg = new MessageDialog(message);
            await msg.ShowAsync();
        }

        public void Handle(SignInSuccessEvent e)
        {
            AreControlsEnabled = true;
            IsProgressRingVisible = false;
            IsProgressRingActive = false;
            _userDataStorageService.AddUserData(Email, Password);
            var msg = new MessageDialog(e.SuccessMessage);
            msg.ShowAsync();
        }

        public void Handle(SignInFailedEvent e)
        {
            AreControlsEnabled = true;
            IsProgressRingVisible = false;
            IsProgressRingActive = false;
            /******TYLKO DO TESTU****/
            _userDataStorageService.AddUserData(Email, Password);
            /************************/
            var msg = new MessageDialog(e.SignInFailedException.Message);
            msg.ShowAsync();
        }
    }
}
