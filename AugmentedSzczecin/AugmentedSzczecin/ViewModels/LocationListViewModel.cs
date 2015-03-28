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
using AugmentedSzczecin.Events;

namespace AugmentedSzczecin.ViewModels
{
    public class LocationListViewModel : Screen, IHandle<PointOfInterestLoadedEvent>, IHandle<PointOfInterestLoadFailedEvent> 
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IPointOfInterestService _pointOfInterestService;

        public LocationListViewModel(IEventAggregator eventAggregator, IPointOfInterestService pointOfInterestService)
        {
            _eventAggregator = eventAggregator;
            _pointOfInterestService = pointOfInterestService;
        }

        protected override void OnActivate()
        {
            //EventAggr
            _pointOfInterestService.Refresh();
            _eventAggregator.Subscribe(this);
            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            _eventAggregator.Unsubscribe(this);
            base.OnDeactivate(close);
        }

        public void Handle(PointOfInterestLoadedEvent e)
        {
            PointOfInterestList = e.PointOfInterestList;
        }

        public void Handle(PointOfInterestLoadFailedEvent e)
        {

        }

        private ObservableCollection<PointOfInterest> _pointOfInterestList  = new ObservableCollection<PointOfInterest>();
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
    }
}
