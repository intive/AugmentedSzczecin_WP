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
        }

        public async void Register(string email, string password)
        {
            var newUser = new User() { Email = email, ErrorCode = "", Password = password };

            User userResponseData = await _httpService.CreateAccount(newUser);

            if(userResponseData.ErrorCode == null)
            {
                _eventAggregator.PublishOnUIThread(new RegisterSuccessEvent() { SuccessMessage = "Registration Successful!" });
            }
            else
            {
                _eventAggregator.PublishOnUIThread(new RegisterFailedEvent() { FailMessage = userResponseData.ErrorCode });
            }
        }

        public async void SignIn(string email, string password)
        {
            var newUser = new User() { Email = email, ErrorCode = "", Password = password };

            Token tokenResponseData = await _httpService.SignIn(newUser);

            if (tokenResponseData.ErrorCode == null)
            {
                _userDataStorageService.AddUserData("ASPassword", email, password);
                _userDataStorageService.AddUserData("ASToken", email, tokenResponseData.TokenString);
                _eventAggregator.PublishOnUIThread(new SignInSuccessEvent() { SuccessMessage = "Signed In successfully!" });
            }
            else
            {
                /******TYLKO DO TESTU**********/
                _userDataStorageService.AddUserData("ASPassword", email, password);
                _userDataStorageService.AddUserData("ASToken", email, "test");
                /******************************/
                _eventAggregator.PublishOnUIThread(new SignInFailedEvent() { FailMessage = tokenResponseData.ErrorCode });
            }
        }
    
        public bool IsUserSignedIn()
        {
            bool isUserSignedIn = _userDataStorageService.IsUserSignedIn();
            
            return isUserSignedIn;
        }

        public void SignOut()
        {
            _userDataStorageService.SignOut();
        }
    
        public string GetUserEmail()
        {
            string email = _userDataStorageService.GetUserEmail();

            return email;
        }

        public string GetUserToken()
        {
            string token = _userDataStorageService.GetUserToken();

            return token;
        }

        public async void ResetPassword(string email)
        {
            var newUser = new User() { Email = email, ErrorCode = "", Password = "" };

            User userResponseData = await _httpService.ResetPassword(newUser);

            if (userResponseData.ErrorCode == null)
            {
                _eventAggregator.PublishOnUIThread(new ResetPasswordSuccessEvent() { SuccessMessage = "New password sent on the email address!" });
            }
            else
            {
                _eventAggregator.PublishOnUIThread(new ResetPasswordFailedEvent() { FailMessage = userResponseData.ErrorCode });
            }
        }
    }
}
