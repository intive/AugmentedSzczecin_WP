using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using AugmentedSzczecin.Events;
using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Models;
using Caliburn.Micro;
using Newtonsoft.Json;

namespace AugmentedSzczecin.Services
{
    public class PointOfInterestService : IPointOfInterestService
    {
        private readonly IEventAggregator _eventAggregator;
        public PointOfInterestService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public ObservableCollection<PointOfInterest> GetPointOfInterest(string jsonString)
        {
            var model = JsonConvert.DeserializeObject<ObservableCollection<PointOfInterest>>(jsonString);
            return model;
        }

        private readonly string _page = "https://augmented-szczecin-test.azure-mobile.net/tables/PointOfInterest";

        public async Task<string> GetPointOfInterestsJsonString()
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
                var jsonString = await GetPointOfInterestsJsonString();
                PointOfInterestList = GetPointOfInterest(jsonString);
                _eventAggregator.PublishOnUIThread(new PointOfInterestLoadedEvent() { PointOfInterestList = PointOfInterestList });
            }
            catch (Exception e)
            {
                _eventAggregator.PublishOnUIThread(new PointOfInterestLoadFailedEvent() { PointOfInterestLoadException = e });
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
