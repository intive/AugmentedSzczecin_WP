using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AugmentedSzczecin.Events;
using Caliburn.Micro;

namespace AugmentedSzczecin.Services
{
    public class PointOfInterestService : IPointOfInterestService
    {
        private EventAggregator sampleEventAggregator;

        public ObservableCollection<PointOfInterest> GetPointOfInterest(string jsonString)
        {
            var model = JsonConvert.DeserializeObject<ObservableCollection<PointOfInterest>>(jsonString);
            return model;
        }

        private readonly string _page = "https://augmented-szczecin-test.azure-mobile.net/tables/PointOfInterest";

        public async Task<string> HttpGetAsync()
        {
            HttpClient client = new HttpClient();

            HttpResponseMessage response = await client.GetAsync(_page);
            response.EnsureSuccessStatusCode();
            string jsonString = await response.Content.ReadAsStringAsync();


            return jsonString;
        }

        public async void Refresh()
        {
            try
            {
                var jsonString = await HttpGetAsync();
                PointOfInterestList = GetPointOfInterest(jsonString);
                sampleEventAggregator.Publish(new PointOfInterestLoadedEvent() { PointOfInterestList = PointOfInterestList }, action => { Task.Factory.StartNew(action); });
            }
            catch (Exception e)
            {
                EventAggregatorExtensions.PublishOnUIThread(null, new PointOfInterestLoadFailedEvent() { PointOfInterestLoadException = e });
            }
        }

        private ObservableCollection<PointOfInterest> _pointOfInterestList = new ObservableCollection<PointOfInterest>();
        public ObservableCollection<PointOfInterest> PointOfInterestList
        {
            get { return _pointOfInterestList; }
            set
            {
                if (_pointOfInterestList != null && _pointOfInterestList != value)
                {
                    _pointOfInterestList = value;
                }
            }
        }
    }
}
