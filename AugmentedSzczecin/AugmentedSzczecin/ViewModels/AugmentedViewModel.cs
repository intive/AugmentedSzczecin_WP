using AugmentedSzczecin.AbstractClasses;
using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Models;
using Caliburn.Micro;
using Windows.Devices.Geolocation;
namespace AugmentedSzczecin.ViewModels
{
    public class AugmentedViewModel : FilteredPOIViewBase
    {
        public AugmentedViewModel( IEventAggregator eventAggregator, 
                                    ILocationService locationService, 
                                    IPointOfInterestService pointOfInterestService, 
                                    INavigationService navigationService) 
                                    : base(eventAggregator, locationService, pointOfInterestService, navigationService) { }

        protected override void OnActivate()
        {
            base.OnActivate();
            _eventAggregator.Subscribe(this);
        }

        protected override void OnDeactivate(bool close)
        {
            _eventAggregator.Unsubscribe(this);
            base.OnDeactivate(close);
        }

    }
}
