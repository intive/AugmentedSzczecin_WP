using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Models;
using AugmentedSzczecin.Services;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace AugmentedSzczecin.ViewModels
{
    public class LocationListViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IHttpRequestService _httpRequestService;
        private readonly IPointOfInterestService _pointOfInterestHandlingService;

        public LocationListViewModel(IEventAggregator eventAggregator, IHttpRequestService httpRequestService, IPointOfInterestService pointOfInterestHandlingService)
        {
            _eventAggregator = eventAggregator;
            _httpRequestService = httpRequestService;
            _pointOfInterestHandlingService = pointOfInterestHandlingService;
        }

        protected override void OnActivate()
        {
            //EventAggr
            base.OnActivate();
            LoadData();
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
        }

        public async Task<ObservableCollection<PointOfInterest>> LoadData()
        {
            string jsonString = null;
            try
            {
                jsonString = await _httpRequestService.HttpGetAsync();
            }
            catch (Exception)
            {
                return PointOfInterestList = null;
            }
            return PointOfInterestList = _pointOfInterestHandlingService.GetPointOfInterest(jsonString);
        }

        private ObservableCollection<PointOfInterest> _pointOfInterestList = new ObservableCollection<PointOfInterest>();
        public ObservableCollection<PointOfInterest> PointOfInterestList
        {
            get { return _pointOfInterestList; }
            set
            {
                if (_pointOfInterestList != value)
                {
                    _pointOfInterestList = value;
                    NotifyOfPropertyChange(() => PointOfInterestList);
                }
            }
        }
    }
}
