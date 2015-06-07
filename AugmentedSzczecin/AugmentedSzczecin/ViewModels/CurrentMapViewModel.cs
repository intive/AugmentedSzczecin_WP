using System;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Geolocation;
using Windows.Networking.Connectivity;
using Windows.Phone.UI.Input;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using AugmentedSzczecin.Events;
using AugmentedSzczecin.Helpers;
using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.AbstractClasses;
using Caliburn.Micro;
using AugmentedSzczecin.Models;

namespace AugmentedSzczecin.ViewModels
{
    public class CurrentMapViewModel : FilteredPOIViewBase, IHandle<PointOfInterestLoadedEvent>, IHandle<PointOfInterestLoadFailedEvent>
    {
        #region Private & Public Fields

        private readonly string _bingKey = "AsaWb7fdBJmcC1YW6uC1UPb57wfLh9cmeX6Zq_r9s0k49tFScWa3o3Z0Sk7ZUo3I";


        #endregion

        #region Constructors

        public CurrentMapViewModel( IEventAggregator eventAggregator, 
                                    ILocationService locationService, 
                                    IPointOfInterestService pointOfInterestService, 
                                    INavigationService navigationService) 
                                    : base(eventAggregator, locationService, pointOfInterestService, navigationService) { }

        #endregion

        #region Properties

        private ObservableCollection<PointOfInterest> _mapLocations;
        public ObservableCollection<PointOfInterest> MapLocations
        {
            get 
            {
                return _mapLocations;
            }
            set
            {
                if (value != _mapLocations)
                {
                    _mapLocations = value;
                    NotifyOfPropertyChange(() => MapLocations);
                }
            }
        }

        

        public string BingKey
        {
            get { return _bingKey; }
        }

        private double _zoomLevel;
        public double ZoomLevel
        {
            get { return _zoomLevel; }
            set
            {
                if (_zoomLevel != value)
                {
                    _zoomLevel = value;
                    NotifyOfPropertyChange(() => ZoomLevel);
                }
            }
        }

        private readonly bool _landmarksVisible = true;
        public bool LandmarksVisible
        {
            get { return _landmarksVisible; }
        }

        private Geopoint _centerOfTheMap;
        public Geopoint CenterOfTheMap
        {
            get { return _centerOfTheMap; }
            set
            {
                if (_centerOfTheMap != value)
                {
                    _centerOfTheMap = value;
                    ChangeScaleBar(null);
                    MyLocationPointVisibility = Visibility.Visible;
                    NotifyOfPropertyChange(() => CenterOfTheMap);
                }
            }
        }

        private Visibility _myLocationPointVisibility = Visibility.Collapsed;
        public Visibility MyLocationPointVisibility
        {
            get { return _myLocationPointVisibility; }
            set
            {
                if (value != _myLocationPointVisibility)
                {
                    _myLocationPointVisibility = value;
                    NotifyOfPropertyChange(() => MyLocationPointVisibility);
                }
            }
        }

        private string _scaleText = "";
        public string ScaleText
        {
            get { return _scaleText; }
            set
            {
                if (_scaleText != value)
                {
                    _scaleText = value;
                    NotifyOfPropertyChange(() => ScaleText);
                }
            }
        }

        


        
        
        
        #endregion

        #region Override Methods

        protected override async void OnActivate()
        {
            base.OnActivate();
            CenterOfTheMap = await _locationService.GetGeolocation();
            CountZoomLevel();
            _eventAggregator.Subscribe(this);           
        }

        protected override void OnDeactivate(bool close)
        {
            _eventAggregator.Unsubscribe(this);
            base.OnDeactivate(close);
        }

        #endregion

        #region Public Methods

        public void ChangeScaleBar(MapControl temporaryMap)
        {
            double tempZoomLevel = ZoomLevel;
            if (temporaryMap != null)
            {
                tempZoomLevel = temporaryMap.ZoomLevel;
            }

            double tempLatitude = 0;
            if (CenterOfTheMap != null)
            {
                tempLatitude = CenterOfTheMap.Position.Latitude;
            }

            tempLatitude = tempLatitude * (Math.PI / 180);

            const double bingMapConstant = 156543.04;

            double metersPerPixel = bingMapConstant * Math.Cos(tempLatitude) / Math.Pow(2, tempZoomLevel);

            double scaleDistance = Math.Round(100 * metersPerPixel / 10) * 10;

            ScaleText = scaleDistance.ToString() + " m";
        }

        

        public void Handle(PointOfInterestLoadedEvent e)
        {
            MapLocations = e.PointOfInterestList;
        }

        public async void Handle(PointOfInterestLoadFailedEvent e)
        {
            var message = new MessageDialog(e.PointOfInterestLoadException.Message);
            await message.ShowAsync();
        }

        #endregion

        #region Private Methods

        private void CountZoomLevel()
        {
            ZoomLevel = ResolutionHelper.CountZoomLevel();
        }

        

        #endregion
    }
}
