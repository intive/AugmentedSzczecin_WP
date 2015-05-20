﻿using Windows.Devices.Geolocation;
using Caliburn.Micro;

namespace AugmentedSzczecin.ViewModels
{
    public class CreatePointOfInterestViewModel : Screen
    {
        private readonly INavigationService _navigationService;

        public CreatePointOfInterestViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }


        public Geopoint Parameter { get; set; }

        private double _latitude;
        public double Latitude
        {
            get { return _latitude; }
            set
            {
                if (_latitude != value)
                {
                    _latitude = value;
                    NotifyOfPropertyChange(() => Latitude);
                }
            }
        }

        private double _longitude;
        public double Longitude
        {
            get { return _longitude; }
            set
            {
                if (_longitude != value)
                {
                    _longitude = value;
                    NotifyOfPropertyChange(() => Longitude);
                }
            }
        }

        protected override void OnActivate()
        {
            Latitude = Parameter.Position.Latitude;
            Longitude = Parameter.Position.Longitude;
        }

        public void CancelNewPointOfInterestClick()
        {
            _navigationService.NavigateToViewModel<AddPointOfInterestViewModel>(Parameter);
        }

    }
}
