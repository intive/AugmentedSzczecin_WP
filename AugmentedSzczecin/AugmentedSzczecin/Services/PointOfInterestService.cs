using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AugmentedSzczecin.Services
{
    public class PointOfInterestService : IPointOfInterestService
    {

        private static string _page = "https://augmented-szczecin-test.azure-mobile.net/tables/PointOfInterest";
        public async Task<string> HttpGetAsync()
        {
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync(_page);
            response.EnsureSuccessStatusCode();
            string jsonString = await response.Content.ReadAsStringAsync();


            return jsonString;
        }
        async void Refresh()
        {
            try
            {

            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public ObservableCollection<PointOfInterest> GetPointOfInterest(string jsonString)
        {
            ObservableCollection<PointOfInterest> model;
            model = JsonConvert.DeserializeObject<ObservableCollection<PointOfInterest>>(jsonString);

            return model;
        }
    }
}
