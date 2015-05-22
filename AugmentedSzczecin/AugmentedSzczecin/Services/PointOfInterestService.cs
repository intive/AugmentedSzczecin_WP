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
        private readonly IHttpService _httpService;

        public PointOfInterestService(IEventAggregator eventAggregator, IHttpService httpService)
        {
            _eventAggregator = eventAggregator;
            _httpService = httpService;
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

        public async void Refresh()
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

        public async void Refresh(string latitude, string longitude, string radius, string category)
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
    }
}
