using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using AugmentedSzczecin.Events;
using AugmentedSzczecin.Interfaces;
using Caliburn.Micro;

namespace AugmentedSzczecin.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly IEventAggregator _eventAggregator;
        public RegisterService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public async void Register(string email, string password)
        {
            Uri baseAddress = new Uri("http://private-8596e-patronage2015.apiary-mock.com/user");

            try
            {
                using (var httpClient = new HttpClient { BaseAddress = baseAddress })
                {
                    using (
                        var content = new StringContent("{ \"email\": \"" + email + ", \"password\": \"" + password + " }",
                            System.Text.Encoding.Unicode, "application/json"))
                    {
                        using (var response = await httpClient.PostAsync("user", content))
                        {
                            string responseData = await response.Content.ReadAsStringAsync();
                        }
                    }
                }

            }
            catch (Exception e)
            {
                _eventAggregator.PublishOnUIThread(new RegisterFailedEvent() { RegisterFailedException = e });
            }
           

        }
    }
}
