using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
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
        private readonly IHttpService _httpService;
        private readonly IAccountService _accountService;

        public PointOfInterestService(IEventAggregator eventAggregator, IHttpService httpService, IAccountService accountService)
        {
            _eventAggregator = eventAggregator;
            _httpService = httpService;
            _accountService = accountService;
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

        public async void LoadPlaces()
        {
            try
            {
                PointOfInterestList = await _httpService.GetPointOfInterestList();
                _eventAggregator.PublishOnUIThread(new PointOfInterestLoadedEvent() { PointOfInterestList = PointOfInterestList });
            }
            catch (Exception e)
            {
                _eventAggregator.PublishOnUIThread(new PointOfInterestLoadFailedEvent() { PointOfInterestLoadException = e });
            }
        }

        public async void LoadPoIs(double latitude, double longitude, int radius, CategoryType category)
        {
            try
            {
                PointOfInterestList = await _httpService.GetPointOfInterestList(latitude, longitude, radius, category);
                _eventAggregator.PublishOnUIThread(new PointOfInterestLoadedEvent() { PointOfInterestList = PointOfInterestList });
            }
            catch (Exception e)
            {
                _eventAggregator.PublishOnUIThread(new PointOfInterestLoadFailedEvent() { PointOfInterestLoadException = e });
            }
        }

        public async void AddPointOfInterest(PointOfInterest poi)
        {
            var loader = new ResourceLoader();
            var isPointOfInterestAdded = await _httpService.AddPointOfInterest(poi, _accountService.GetUserEmail(), _accountService.GetUserPassword());

            if (isPointOfInterestAdded)
            {
                _eventAggregator.PublishOnUIThread(new CreatePointOfInterestSuccessEvent() { SuccessMessage = loader.GetString("CreatePointOfInterestSuccessMessage") });
            }
            else
            {
                _eventAggregator.PublishOnUIThread(new CreatePointOfInterestFailedEvent() { FailureMessage = loader.GetString("CreatePointOfInterestFailureMessage") });
            }
        }
    }
}
