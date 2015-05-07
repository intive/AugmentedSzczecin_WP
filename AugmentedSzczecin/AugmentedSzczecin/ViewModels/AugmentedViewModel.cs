using AugmentedSzczecin.Interfaces;
using Caliburn.Micro;
namespace AugmentedSzczecin.ViewModels
{
    public class AugmentedViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IPointOfInterestService _pointOfInterestService;

        public AugmentedViewModel(IEventAggregator eventAggregator, ILocationService locationService, IPointOfInterestService pointOfInterestService, INavigationService navigationService)
        {
            _eventAggregator = eventAggregator;
            _pointOfInterestService = pointOfInterestService;
        }
        protected override void OnActivate()
        {
            base.OnActivate();
            _eventAggregator.Subscribe(this);
            _pointOfInterestService.Refresh();
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            _eventAggregator.Unsubscribe(this);    
        }
    }
}
