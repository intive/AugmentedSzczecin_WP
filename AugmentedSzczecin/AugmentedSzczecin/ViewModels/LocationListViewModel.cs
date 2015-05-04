using System.Collections.ObjectModel;
using Windows.UI.Popups;
using AugmentedSzczecin.Events;
using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Models;
using Caliburn.Micro;

namespace AugmentedSzczecin.ViewModels
{
    public class LocationListViewModel : Screen, IHandle<PointOfInterestLoadedEvent>,
        IHandle<PointOfInterestLoadFailedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IPointOfInterestService _pointOfInterestService;

        public LocationListViewModel(IEventAggregator eventAggregator, IPointOfInterestService pointOfInterestService)
        {
            _eventAggregator = eventAggregator;
            _pointOfInterestService = pointOfInterestService;
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
                    NotifyOfPropertyChange(() => PointOfInterestList);
                }
            }
        }

        protected override void OnActivate()
        {
            _eventAggregator.Subscribe(this);
            base.OnActivate();
            RefreshPointOfInterestService();
        }

        protected override void OnDeactivate(bool close)
        {
            _eventAggregator.Unsubscribe(this);
            base.OnDeactivate(close);
        }

        public void RefreshPointOfInterestService()
        {
            _pointOfInterestService.Refresh();
        }

        public void Handle(PointOfInterestLoadedEvent e)
        {
            PointOfInterestList = e.PointOfInterestList;
        }

        public void Handle(PointOfInterestLoadFailedEvent e)
        {
            var msg = new MessageDialog(e.PointOfInterestLoadException.Message);
            msg.ShowAsync();
        }
    }
}