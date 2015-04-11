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
                _userDataStorageService.AddUserData(email, password);
                _userDataStorageService.AddUserData(email, tokenResponseData.TokenString);
                _eventAggregator.PublishOnUIThread(new SignInSuccessEvent() { SuccessMessage = "Signed In successfully!" });
            }
            else
            {
                _eventAggregator.PublishOnUIThread(new SignInFailedEvent() { FailMessage = tokenResponseData.ErrorCode });
            }
        }
    }
}

