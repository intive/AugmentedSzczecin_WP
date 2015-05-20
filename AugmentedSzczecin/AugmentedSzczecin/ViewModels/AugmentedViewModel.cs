using AugmentedSzczecin.Interfaces;
using Caliburn.Micro;
using Windows.Devices.Geolocation;
namespace AugmentedSzczecin.ViewModels
{
    public class AugmentedViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IPointOfInterestService _pointOfInterestService;
        private readonly ILocationService _locationService;

        private int _radius = 200;
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
            }
        }

        public AugmentedViewModel(IEventAggregator eventAggregator, ILocationService locationService, IPointOfInterestService pointOfInterestService, INavigationService navigationService)
        {
            _eventAggregator = eventAggregator;
            _pointOfInterestService = pointOfInterestService;
            _locationService = locationService;
        }
        protected override async void OnActivate()
        {
            base.OnActivate();
            _eventAggregator.Subscribe(this);
            Geopoint point = await _locationService.GetGeolocation();
            _pointOfInterestService.LoadPoIs(point.Position.Latitude, point.Position.Longitude, Radius);
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            _eventAggregator.Unsubscribe(this);    
        }
    }
}
