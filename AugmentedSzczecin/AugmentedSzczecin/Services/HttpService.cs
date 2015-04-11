using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AugmentedSzczecin.Services
{
    public class HttpService : IHttpService
    {
        private string _uriMock = "http://private-8596e-patronage2015.apiary-mock.com/user";

        public async Task<User> CreateAccount(User user)
        {
            var userResponseData = new User();
            var baseAddress = new Uri(_uriMock);
            try
            {
                using (var httpClient = new HttpClient { BaseAddress = baseAddress })
                {
                    var json = JsonConvert.SerializeObject(user);

                    using (var content = new StringContent(json, Encoding.Unicode, "application/json"))
                    {
                        using (var response = await httpClient.PostAsync("user", content))
                        {
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                var responseData = await response.Content.ReadAsStringAsync();
                                userResponseData = JsonConvert.DeserializeObject<User>(responseData);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return userResponseData;
        }

        public async Task<Token> SignIn(User user)
        {
            var tokenResponseData = new Token();
            var baseAddress = new Uri(_uriMock);
            try
            {
                using (var httpClient = new HttpClient { BaseAddress = baseAddress })
                {
                    var json = JsonConvert.SerializeObject(user);

                    using (var content = new StringContent(json, Encoding.Unicode, "application/json"))
                    {
                        using (var response = await httpClient.PostAsync("user", content))
                        {
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                var responseData = await response.Content.ReadAsStringAsync();
                                tokenResponseData = JsonConvert.DeserializeObject<Token>(responseData);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }

            return tokenResponseData;
        }
    }
}
