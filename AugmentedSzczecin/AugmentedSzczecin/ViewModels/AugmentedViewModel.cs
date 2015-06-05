using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Models;
using Caliburn.Micro;
using Windows.Devices.Geolocation;
namespace AugmentedSzczecin.ViewModels
{
    public class AugmentedViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IPointOfInterestService _pointOfInterestService;
        private readonly ILocationService _locationService;

        private int _radius;
        public int Radius
        {
            get
            {
                return _radius;
            }
            set
            {
                _radius = value;
                NotifyOfPropertyChange(() => Radius);
                LoadPoIs();
            }
        }

        public AugmentedViewModel(IEventAggregator eventAggregator, ILocationService locationService, IPointOfInterestService pointOfInterestService, INavigationService navigationService)
        {
            _eventAggregator = eventAggregator;
            _pointOfInterestService = pointOfInterestService;
            _locationService = locationService;
            Radius = 300;
        }
        protected override void OnActivate()
        {
            base.OnActivate();
            _eventAggregator.Subscribe(this);
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            _eventAggregator.Unsubscribe(this);    
        }

        public async void LoadPoIs()
        {
            Geopoint point = await _locationService.GetGeolocation();
            _pointOfInterestService.LoadPoIs(point.Position.Latitude, point.Position.Longitude, Radius, CategoryType.ALL);
        }
    }
}
