using AugmentedSzczecin.Interfaces;
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
            json = "{\"places\":[{\"name\":\"osmy Budynek pierwszego na ziemiach polskich kolegium salwatoriaĹskiego oraz grota Matki BoĹźej z Lourdes\",\"location\":{\"longitude\":14.5580,\"latitude\":53.4299},\"address\":{\"street\":\"GĹowackiego 3 w dzielnicy Salwator\"},\"subcategory\":\"MONUMENT\",\"category\":\"PLACE\"}, {\"name\":\"piaty Budynek pierwszego na ziemiach polskich kolegium salwatoriaĹskiego oraz grota Matki BoĹźej z Lourdes\",\"location\":{\"longitude\":14.5590,\"latitude\":53.4390},\"address\":{\"street\":\"GĹowackiego 3 w dzielnicy Salwator\"},\"subcategory\":\"MONUMENT\",\"category\":\"PLACE\"}], \"events\":[{\"name\":\"drugi Budynek pierwszego na ziemiach polskich kolegium salwatoriaĹskiego oraz grota Matki BoĹźej z Lourdes\",\"location\":{\"longitude\":14.5588,\"latitude\":53.4299},\"address\":{\"street\":\"GĹowackiego 3 w dzielnicy Salwator\"},\"subcategory\":\"MONUMENT\",\"category\":\"EVENT\"}, {\"name\":\"czwarty Budynek pierwszego na ziemiach polskich kolegium salwatoriaĹskiego oraz grota Matki BoĹźej z Lourdes\",\"location\":{\"longitude\":14.5590,\"latitude\":53.4390},\"address\":{\"street\":\"GĹowackiego 3 w dzielnicy Salwator\"},\"subcategory\":\"MONUMENT\",\"category\":\"EVENT\"}], \"commercial\":[{\"name\":\"dziewiaty Budynek pierwszego na ziemiach polskich kolegium salwatoriaĹskiego oraz grota Matki BoĹźej z Lourdes\",\"location\":{\"longitude\":14.5570,\"latitude\":53.4300},\"address\":{\"street\":\"GĹowackiego 3 w dzielnicy Salwator\"},\"subcategory\":\"MONUMENT\",\"category\":\"COMMERCIAL\"}, {\"name\":\"jedenasty Budynek pierwszego na ziemiach polskich kolegium salwatoriaĹskiego oraz grota Matki BoĹźej z Lourdes\",\"location\":{\"longitude\":14.5510,\"latitude\":53.4400},\"address\":{\"street\":\"GĹowackiego 3 w dzielnicy Salwator\"},\"subcategory\":\"MONUMENT\",\"category\":\"COMMERCIAL\"}]}";

            if (json == "{}")
            {
                return new ObservableCollection<PointOfInterest>();
            }

            IList<JToken> results = null;
            switch(category)
            {
                case CategoryType.PLACE:
                    results = PoiQueryResponse.ParsePoiPlacesQuery(json);
                    break;
                case CategoryType.EVENT:
                    results = PoiQueryResponse.ParsePoiEventsQuery(json);
                    break;
                case CategoryType.PERSON:
                    results = PoiQueryResponse.ParsePoiPeopleQuery(json);
                    break;
                case CategoryType.COMMERCIAL:
                    results = PoiQueryResponse.ParsePoiCommercialQuery(json);
                    break;
                case CategoryType.ALL:
                    results = PoiQueryResponse.ParsePoiAllQuery(json);
                    break;
            }

            ObservableCollection<PointOfInterest> pois = new ObservableCollection<PointOfInterest>();
            
            if(results != null)
            {
                foreach (var result in results)
                {
                    pois.Add(JsonConvert.DeserializeObject<PointOfInterest>(result.ToString()));
                }
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

        public async Task<bool> AddPointOfInterest(PointOfInterest poi)
        {
            var json = JsonConvert.SerializeObject(poi);
            var content = new StringContent(json, Encoding.Unicode, "application/json");
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
