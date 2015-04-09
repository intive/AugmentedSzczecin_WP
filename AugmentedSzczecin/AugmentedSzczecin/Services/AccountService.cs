﻿using System;
using System.Net;
using System.Net.Http;
using System.Text;
using AugmentedSzczecin.Events;
using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Models;
using Caliburn.Micro;
using Newtonsoft.Json;

namespace AugmentedSzczecin.Services
{
    public class AccountService : IAccountService
    {
        private string _uriMock = "http://private-8596e-patronage2015.apiary-mock.com/user";
        private readonly IEventAggregator _eventAggregator;

        public AccountService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public async void Register(string email, string password)
        {
            var baseAddress = new Uri(_uriMock);
            try
            {
                using (var httpClient = new HttpClient { BaseAddress = baseAddress })
                {
                    var newUser = new User() { Email = email, ErrorCode = "", Password = password };
                    var json = JsonConvert.SerializeObject(newUser);

                    using (var content = new StringContent(json, Encoding.Unicode, "application/json"))
                    {
                        using (var response = await httpClient.PostAsync("user", content))
                        {
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                var responseData = await response.Content.ReadAsStringAsync();
                                var user = JsonConvert.DeserializeObject<User>(responseData);

                                if (user.ErrorCode != null)
                                {
                                    _eventAggregator.PublishOnUIThread(new RegisterFailedEvent() { RegisterFailedException = new Exception("Back-end Error!") });
                                }
                                else
                                {
                                    _eventAggregator.PublishOnUIThread(new RegisterSuccessEvent() { SuccessMessage = "Registration Successful!" });
                                }
                            }
                            else
                            {
                                _eventAggregator.PublishOnUIThread(new RegisterFailedEvent() { RegisterFailedException = new Exception("200 error: Something went wrong!") });
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _eventAggregator.PublishOnUIThread(new RegisterFailedEvent() { RegisterFailedException = e });
            }
        }

        public async void SignIn(string email, string password)
        {
            var baseAddress = new Uri(_uriMock);
            try
            {
                using (var httpClient = new HttpClient { BaseAddress = baseAddress })
                {
                    var newUser = new User() { Email = email, ErrorCode = "", Password = password };
                    var json = JsonConvert.SerializeObject(newUser);

                    using (var content = new StringContent(json, Encoding.Unicode, "application/json"))
                    {
                        using (var response = await httpClient.PostAsync("user", content))
                        {
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                var responseData = await response.Content.ReadAsStringAsync();
                                var token = JsonConvert.DeserializeObject<Token>(responseData);

                                if (token.ErrorCode != null)
                                {
                                    _eventAggregator.PublishOnUIThread(new SignInFailedEvent() { SignInFailedException = new Exception("Back-end Error!") });
                                }
                                else
                                {
                                    var userDataStorageService = IoC.GetInstance(typeof(IUserDataStorageService), null);
                                    ((IUserDataStorageService)userDataStorageService).AddUserData(email, token.TokenString);
                                    _eventAggregator.PublishOnUIThread(new SignInSuccessEvent() { SuccessMessage = "Signed In successfully!" });
                                }
                            }
                            else
                            {
                                /**********TYLKO DO TESTU********/
                                var userDataStorageService = IoC.GetInstance(typeof(IUserDataStorageService), null);
                                ((IUserDataStorageService)userDataStorageService).AddUserData(email, "test");
                                /********************************/
                                _eventAggregator.PublishOnUIThread(new SignInFailedEvent() { SignInFailedException = new Exception("Http status code not OK!") });
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _eventAggregator.PublishOnUIThread(new SignInFailedEvent() { SignInFailedException = e });
            }
        }
    }
}
