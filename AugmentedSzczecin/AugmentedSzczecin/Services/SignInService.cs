using AugmentedSzczecin.Events;
using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Models;
using Caliburn.Micro;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace AugmentedSzczecin.Services
{
    public class SignInService : ISignInService
    {
        private readonly IEventAggregator _eventAggregator;

        public SignInService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public async void SignIn(string email, string password)
        {
            var baseAddress = new Uri("http://private-8596e-patronage2015.apiary-mock.com/user");
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
                                    _eventAggregator.PublishOnUIThread(new SignInFailedEvent()
                                    {
                                        SignInFailedException = new Exception("Back-end Error!")
                                    });
                                }
                                else
                                {
                                    _eventAggregator.PublishOnUIThread(new SignInSuccessEvent()
                                    {
                                        SuccessMessage = "Signed In successfully!"
                                    });
                                }
                            }
                            else
                            {
                                _eventAggregator.PublishOnUIThread(new SignInFailedEvent() { SignInFailedException = new Exception("200 error: Something went wrong!") });
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
