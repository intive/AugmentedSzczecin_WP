using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private const string TemporaryPointOfInterestDatabaseUri = "https://augmented-szczecin-test.azure-mobile.net/tables/PointOfInterest";

        public async Task<ObservableCollection<PointOfInterest>> GetPointOfInterestsList()
        {
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync(TemporaryPointOfInterestDatabaseUri);
            response.EnsureSuccessStatusCode();
            string jsonString = await response.Content.ReadAsStringAsync();
            ObservableCollection<PointOfInterest> PointOfInterestList = JsonConvert.DeserializeObject<ObservableCollection<PointOfInterest>>(jsonString);

            return PointOfInterestList;
        }

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
                            else
                            {
                                userResponseData.ErrorCode = "Back-end Error!";
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                userResponseData.ErrorCode = e.Message;
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
                            else
                            {
                                tokenResponseData.ErrorCode = "Back-end Error!";
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
