using System;
using AugmentedSzczecin.Events;
using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Models;
using Caliburn.Micro;

namespace AugmentedSzczecin.Services
{
    public class AccountService : IAccountService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IHttpService _httpService;
        private readonly IUserDataStorageService _userDataStorageService;

        public AccountService(IEventAggregator eventAggregator, IHttpService httpService, IUserDataStorageService userDataStorageService)
        {
            _eventAggregator = eventAggregator;
            _httpService = httpService;
            _userDataStorageService = userDataStorageService;

            if (_userDataStorageService.IsUserSignedIn())
            {
                _httpService.SetAuthenticationHeader(_userDataStorageService.GetUserEmail(), _userDataStorageService.GetUserPassword());
            }
        }

        public async void Register(string email, string password)
        {
            var newUser = new User() { Email = email, Password = password };
            var status = await _httpService.CreateAccount(newUser);

            switch (status)
            {
                case 204:
                    _userDataStorageService.AddUserData("ASPassword", email, password);
                    _httpService.SetAuthenticationHeader(email, password);
                    _eventAggregator.PublishOnUIThread(new RegisterSuccessEvent() { SuccessMessage = "Registration Successful!" });
                    break;
                case 422:
                    _eventAggregator.PublishOnUIThread(new RegisterFailedEvent() { FailMessage = "Ten email jest już używany." });
                    break;
                default:
                    _eventAggregator.PublishOnUIThread(new RegisterFailedEvent() { FailMessage = "Backend error" });
                    break;
            }
        }

        public async void SignIn(string email, string password)
        {
            var newUser = new User() { Email = email, Password = password };
            var status = await _httpService.SignIn(newUser);

            switch (status)
            {
                case 200:
                    _userDataStorageService.AddUserData("ASPassword", email, password);
                    _httpService.SetAuthenticationHeader(email, password);
                    _eventAggregator.PublishOnUIThread(new SignInSuccessEvent() { SuccessMessage = "SignIn Successful!" });
                    break;
                case 401:
                    _eventAggregator.PublishOnUIThread(new SignInFailedEvent() { FailMessage = "Niepoprawne dane." });
                    break;
                default:
                    _eventAggregator.PublishOnUIThread(new SignInFailedEvent() { FailMessage = "Backend error" });
                    break;
            }
        }
    
        public bool IsUserSignedIn()
        {
            return _userDataStorageService.IsUserSignedIn();
        }

        public void SignOut()
        {
            _userDataStorageService.SignOut();
            _httpService.SignOut();
        }
    
        public string GetUserEmail()
        {
            return _userDataStorageService.GetUserEmail();
        }

        public string GetUserPassword()
        {
            return _userDataStorageService.GetUserPassword();
        }

        public async void ResetPassword(string email)
        {
            var newUser = new User() { Email = email, Password = "" };

            var status = await _httpService.ResetPassword(newUser);

            if (status)
            {
                _eventAggregator.PublishOnUIThread(new ResetPasswordSuccessEvent() { SuccessMessage = "New password sent on the email address!" });
            }
            else
            {
                _eventAggregator.PublishOnUIThread(new ResetPasswordFailedEvent() { FailMessage = "Backend error" });
            }
        }
    }
}
