using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Models;
using AugmentedSzczecin.Services;
using AugmentedSzczecin.ViewModels;
using Caliburn.Micro;
using AugmentedSzczecin.Models;
using AugmentedSzczecin.Services;
using AugmentedSzczecin.ViewModels;
using Caliburn.Micro;
using System;
using System.Collections.ObjectModel;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace AugmentedSzczecin.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CurrentMapView : Page
    {

        private ObservableCollection<PointOfInterest> _mapLocations;
        public CurrentMapView()
        {
            this.InitializeComponent();
            _mapLocations = new ObservableCollection<PointOfInterest>();
            LoadMap();
        }

        private void LoadMap()
        {
            object serivesFromCurrentMapVieModel;
            serivesFromCurrentMapVieModel = IoC.GetInstance(typeof(CurrentMapViewModel), null);
            ((CurrentMapViewModel)serivesFromCurrentMapVieModel).UpdateInternetConnection();
            bool InternetConnection = ((CurrentMapViewModel)serivesFromCurrentMapVieModel).InternetConnection;

            if (InternetConnection)
                AddPins();
            else
                ((CurrentMapViewModel)serivesFromCurrentMapVieModel).InternetConnectionDisabledMsg();
        }

        private async void AddPins()
        {
            object servicesFromLocationListViewModel;
            servicesFromLocationListViewModel = IoC.GetInstance(typeof(LocationListViewModel), null);

            _mapLocations = await ((LocationListViewModel)servicesFromLocationListViewModel).LoadData();
            
            if (_mapLocations != null)
                foreach (PointOfInterest pointOfInterest in _mapLocations)
                {
                    var newPin = CreatePin();
                    BingMap.Children.Add(newPin);
                    Geopoint geopoint = new Geopoint(new BasicGeoposition
                            {
                                Longitude = pointOfInterest.Longitude,
                                Latitude = pointOfInterest.Latitude
                            });
                    MapControl.SetLocation(newPin, geopoint);
                    MapControl.SetNormalizedAnchorPoint(newPin, new Point(0.5, 1));
                }
            else
            {
                object serivesFromCurrentMapVieModel = IoC.GetInstance(typeof(CurrentMapViewModel), null);
                ((CurrentMapViewModel)serivesFromCurrentMapVieModel).InternetConnectionDisabledMsg();
            }
        }

        private DependencyObject CreatePin()
        {
            var myGrid = new Grid();
            myGrid.RowDefinitions.Add(new RowDefinition());
            myGrid.RowDefinitions.Add(new RowDefinition());
            myGrid.Background = new SolidColorBrush(Colors.Transparent);

            Uri uri = new Uri("ms-appx:///Assets/Locationpoint.png", UriKind.Absolute);

            var image = new Image()
            {
                Source = new BitmapImage(uri),
                Width = 10,
                Height = 10
            };

            myGrid.Children.Add(image);

            return myGrid;
        }


        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }
    }
}
