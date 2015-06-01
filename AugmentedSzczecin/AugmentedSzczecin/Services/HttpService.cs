﻿using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AugmentedSzczecin.Services
{
    public class HttpService : IHttpService
    {
        private HttpClient _client = new HttpClient() { BaseAddress = new Uri("http://78.133.154.62:1080/") };

        public async Task<ObservableCollection<PointOfInterest>> GetPointOfInterestList()
        {
            HttpResponseMessage response = await _client.GetAsync("places");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            if (json == "{}")
            {
                return new ObservableCollection<PointOfInterest>();
        }
            return JsonConvert.DeserializeObject<ObservableCollection<PointOfInterest>>(json);
        }

        public async Task<ObservableCollection<PointOfInterest>> GetPointOfInterestList(double latitude, double longitude, int radius, CategoryType category)
        {
            HttpResponseMessage response = null;
            if (category == CategoryType.ALL)
            {
                response = await _client.GetAsync(string.Format("q?lt={0}&lg={1}&radius={2}", latitude, longitude, radius));
            }
            else
            {
                response = await _client.GetAsync(string.Format("q?lt={0}&lg={1}&radius={2}&cat={3}", latitude, longitude, radius, category));
            }
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            if (json == "{}")
            {
                return new ObservableCollection<PointOfInterest>();
            }
            ObservableCollection<PointOfInterest> pois = new ObservableCollection<PointOfInterest>();
            IList<JToken> results = JObject.Parse(json)["places"].Children().ToList();
            foreach (var result in results)
            {
                pois.Add(JsonConvert.DeserializeObject<PointOfInterest>(result.ToString()));
            }
            return pois;
        }

        public async Task<int> CreateAccount(User user)
        {
            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.Unicode, "application/json");
            var response = await _client.PostAsync("users", content);
            return (int)response.StatusCode;
        }

        public async Task<int> SignIn(User user)
        {
            SetAuthenticationHeader(user.Email, user.Password);
            var response = await _client.GetAsync("users/whoami");
            return (int)response.StatusCode;
        }

        public async Task<bool> ResetPassword(User user)
        {
            var response = await _client.GetAsync(string.Format("users/{0}/resetpassword", user.Email));
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> AddPointOfInterest(PointOfInterest poi, string email, string password)
        {
            SetAuthenticationHeader(email, password);
            var json = JsonConvert.SerializeObject(poi, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, });
            var content = new StringContent(json, Encoding.GetEncoding("iso-8859-1"), "application/json");
            var response = await _client.PostAsync("places", content);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

        public void SetAuthenticationHeader(string email, string password)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(string.Format("{0}:{1}", email, password))));
        }

        public void SignOut()
        {
            _client.DefaultRequestHeaders.Authorization = null;
        }
    }
}
