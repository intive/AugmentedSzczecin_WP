using System;
using Windows.ApplicationModel.Resources;
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
            var loader = new ResourceLoader();
            var newUser = new User() { Email = email, Password = password };
            var status = await _httpService.CreateAccount(newUser);

            switch (status)
            {
                case 204:
                    _userDataStorageService.AddUserData("ASPassword", email, password);
                    _httpService.SetAuthenticationHeader(email, password);
                    _eventAggregator.PublishOnUIThread(new RegisterSuccessEvent() { SuccessMessage = loader.GetString("RegisterSuccessMessage") });
                    break;
                case 422:
                    _eventAggregator.PublishOnUIThread(new RegisterFailedEvent() { FailMessage = loader.GetString("RegisterEmailFailMessage") });
                    break;
                default:
                    _eventAggregator.PublishOnUIThread(new RegisterFailedEvent() { FailMessage = loader.GetString("ServerError") });
                    break;
            }
        }

        public async void SignIn(string email, string password)
        {
            var loader = new ResourceLoader();
            var newUser = new User() { Email = email, Password = password };
            var status = await _httpService.SignIn(newUser);

            switch (status)
            {
                case 200:
                    _userDataStorageService.AddUserData("ASPassword", email, password);
                    _httpService.SetAuthenticationHeader(email, password);
                    _eventAggregator.PublishOnUIThread(new SignInSuccessEvent() { SuccessMessage = loader.GetString("SignInSuccessMessage") });
                    break;
                case 401:
                    _eventAggregator.PublishOnUIThread(new SignInFailedEvent() { FailMessage = loader.GetString("SignInDataFailMessage") });
                    break;
                default:
                    _eventAggregator.PublishOnUIThread(new SignInFailedEvent() { FailMessage = loader.GetString("ServerError") });
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
            var loader = new ResourceLoader();
            var newUser = new User() { Email = email, Password = "" };

            var status = await _httpService.ResetPassword(newUser);

            if (status)
            {
                _eventAggregator.PublishOnUIThread(new ResetPasswordSuccessEvent() { SuccessMessage = loader.GetString("ResetPasswordSuccessMessage") });
            }
            else
            {
                _eventAggregator.PublishOnUIThread(new ResetPasswordFailedEvent() { FailMessage = loader.GetString("ServerError") });
            }
        }
    }
}
