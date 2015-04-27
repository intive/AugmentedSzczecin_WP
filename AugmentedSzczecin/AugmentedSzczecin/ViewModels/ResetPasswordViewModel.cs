using AugmentedSzczecin.Events;
using AugmentedSzczecin.Interfaces;
using Caliburn.Micro;
using System;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;

namespace AugmentedSzczecin.ViewModels
{
    public class ResetPasswordViewModel : Screen, IHandle<ResetPasswordSuccessEvent>, IHandle<ResetPasswordFailedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IAccountService _accountService;

        public ResetPasswordViewModel(IEventAggregator eventAggregator, IAccountService accountService)
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
                    NotifyOfPropertyChange(() =>IsProgressRingVisible);
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
                    ValidateEmailEmpty();
                    ValidateEmailMatch();
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

        public void Reset()
        {
            CheckValidation();
            if (ValidationCheck)
            {
                AreControlsEnabled = false;
                IsProgressRingVisible = true;
                _accountService.ResetPassword(Email);
            }
            else
            {
                WrongValidationMessageDialog();
            }

        }

        public void Handle(ResetPasswordSuccessEvent e)
        {
            AreControlsEnabled = true;
            IsProgressRingVisible = false;
            var msg = new MessageDialog(e.SuccessMessage);
            msg.ShowAsync();
        }

        public void Handle(ResetPasswordFailedEvent e)
        {
            AreControlsEnabled = true;
            IsProgressRingVisible = false;
            var msg = new MessageDialog(e.FailMessage);
            msg.ShowAsync();
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
            ValidationCheck = IsEmailEmptyValid && IsEmailMatchValid;
        }

        private async void WrongValidationMessageDialog()
        {
            var loader = new ResourceLoader();
            string message = "";

            if (!IsEmailEmptyValid)
            {
                message += loader.GetString("SignUpEmailEmpty") + "\n";
            }
            if (!IsEmailMatchValid && IsEmailEmptyValid)
            {
                message += loader.GetString("SignUpEmailMatch") + "\n";
            }

            var msg = new MessageDialog(message);
            await msg.ShowAsync();
        }
    }
}
