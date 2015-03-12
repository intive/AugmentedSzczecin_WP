using AugmentedSzczecin.Model;
using System;
using System.Collections.ObjectModel;
using Windows.Devices.Geolocation;
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

        private ObservableCollection<LocationForMap> _mapLocations;

        public CurrentMapView()
        {
            this.InitializeComponent();

            _mapLocations = new ObservableCollection<LocationForMap>()
                {
                    new LocationForMap
                    {
                        Geopoint = new Geopoint(new BasicGeoposition
                        {
                            Longitude = -122.1311156,
                            Latitude = 47.6785619
                        }),
                        Name = "Redmond"
                    },

                    new LocationForMap
                    {
                        Geopoint = new Geopoint(new BasicGeoposition
                        {
                            Longitude = -122.1381556,
                            Latitude = 47.6796119
                        }),
                        Name = "Moja ulubiona cukiernia"
                    },

                    new LocationForMap
                    {
                        Geopoint = new Geopoint(new BasicGeoposition
                        {
                            Longitude = 14.536886131390929,
                            Latitude = 53.469053423032165
                        }),
                        Name = "Moja ulubiona cukiernia"
           
                    }
                };
            AddPins();
        }

        private void AddPins()
        {
            foreach (LocationForMap locationForMap in _mapLocations)
            {
                var newPin = CreatePin();
                BingMap.Children.Add(newPin);
                MapControl.SetLocation(newPin, locationForMap.Geopoint);
                MapControl.SetNormalizedAnchorPoint(newPin, locationForMap.Anchor);
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
                Width=10,
                Height=10
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
