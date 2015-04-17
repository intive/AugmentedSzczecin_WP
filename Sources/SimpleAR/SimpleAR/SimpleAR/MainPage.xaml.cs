using Microsoft.Maps.SpatialToolbox;
using Microsoft.Maps.SpatialToolbox.Bing.NavteqPoiSchema;
using System;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Geolocation;
using Windows.Devices.Sensors;
using Windows.Media.Capture;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Maps;
using System.Net.Http;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Point = Microsoft.Maps.SpatialToolbox.Point;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace SimpleAR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SimpleOrientationSensor _orientationSensor;
        private Compass _compass;
        private Geolocator _gps;

        private uint _movementThreshold = 100;
        private double _defaultSeatchRadius = 1;

        private string _sessionKey;
        private MapControl _pinLayer;
        private UserPushpin _gpsPin;

        private Geopoint _currentLocation = null;
        private double _currentHeading = 0;
        private Result[] _poiLocations = null;
        public MainPage()
        {
            this.InitializeComponent();
            _sessionKey = MyMap.MapServiceToken;
            MyMap.Loaded += async (s, e) =>
            {
                try
                {
                    _pinLayer = new MapControl();
                    MyMap.Children.Add(_pinLayer);

                    _gpsPin = new UserPushpin();
                    MyMap.Children.Add(_gpsPin);
                }
                catch
                {
                    // ignored
                }
            };

            //this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private async Task StartCamera()
        {
            var source = new Windows.Media.Capture.MediaCapture();
            try
            {
                //Use a specific camera
                var cameraId = await FindRearFacingCamera();
                if (!string.IsNullOrEmpty(cameraId))
                {
                    var settings = new MediaCaptureInitializationSettings();
                    settings.VideoDeviceId = cameraId;
                    settings.StreamingCaptureMode = StreamingCaptureMode.Video;

                    await source.InitializeAsync(settings);

                    PreviewScreen.Source = source;

                    //Start video preview
                    await source.StartPreviewAsync();
                }
            }
            catch
            {
            }
        }

        private async void StopCamera()
        {
            try
            {
                if (PreviewScreen.Source != null)
                {
                    //Stop camera if the video source is not null
                    await PreviewScreen.Source.StopPreviewAsync();
                    PreviewScreen.Source = null;
                }
            }
            catch { }
        }

        private async Task<string> FindRearFacingCamera()
        {
            //Get all video capture devices
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            //First look using panel, this is the best approach
            var device = (from d in devices
                          where d.EnclosureLocation != null &&
                                d.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Back
                          select d).FirstOrDefault();


            if (device == null)
            {
                device = (from d in devices
                          where d.Name.ToLower().Contains("back") ||
                                d.Id.ToLower().Contains("back") ||
                                d.Name.ToLower().Contains("rear") ||
                                d.Id.ToLower().Contains("rear")
                          select d).FirstOrDefault();
            }

            if (device != null)
            {
                return device.Id;
            }

            return null;
        }

        private async Task<Response> NavteqPoiSearch(Point center)
        {
            string baseUrl =
                "http://spatial.virtualearth.net/REST/v1/data/c2ae584bbccc4916a0acf75d1e6947b4/NavteqEU/NavteqPOIs";

            string query =
                string.Format("{0}?spatialFilter=nearby({1:N5},{2:N5},{3})&$format=json&key={4}",
                    baseUrl, center.Coordinate.Latitude, center.Coordinate.Longitude, _defaultSeatchRadius, _sessionKey);

            return await GetResponse<Response>(new Uri(query));
        }

        private async Task<T> GetResponse<T>(Uri uri)
        {
            System.Net.Http.HttpClient client = new HttpClient();
            var response = await client.GetAsync(uri);

            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                return (T)ser.ReadObject(stream);
            }
        }

        private void UpdateARView()
        {
            if (_currentLocation != null)
            {
                ItemCanvas.Children.Clear();

                if (_poiLocations != null && _poiLocations.Length > 0)
                {
                    var center = new Coordinate(_currentLocation.Position.Latitude,
                        _currentLocation.Position.Longitude);

                    foreach (var poi in _poiLocations)
                    {

                        var c = new Coordinate(poi.Latitude, poi.Longitude);
                        var poiHeading = SpatialTools.CalculateHeading(center, c);
                        var diff = _currentHeading - poiHeading;

                        if (diff > 180)
                        {
                            diff = _currentHeading - (poiHeading + 360);
                        }
                        else if (diff < -180)
                        {
                            diff = _currentHeading + 360 - poiHeading;
                        }

                        if (Math.Abs(diff) <= 22.5)
                        {
                            var distance = SpatialTools.HaversineDistance(center, c, DistanceUnits.KM);

                            double left = 0;

                            if (diff > 0)
                            {
                                left = ItemCanvas.ActualWidth / 2 * ((22.5 - diff) / 22.5);
                            }
                            else
                            {
                                left = ItemCanvas.ActualWidth / 2 * (1 + -diff / 22.5);
                            }

                            double top = ItemCanvas.ActualHeight * (1 - distance / _defaultSeatchRadius);

                            var tb = new TextBlock()
                            {
                                Text = poi.Name,
                                FontSize = 24,
                                TextAlignment = TextAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Center
                            };

                            Canvas.SetLeft(tb, left);
                            Canvas.SetTop(tb, top);
                            ItemCanvas.Children.Add(tb);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                await StartCamera();

                _orientationSensor = SimpleOrientationSensor.GetDefault();
                if (_orientationSensor != null)
                {
                    _orientationSensor.OrientationChanged += SimpleOrientationSensorReadingChanged;
                    UpdateOrientation(_orientationSensor.GetCurrentOrientation());
                }

                _compass = Compass.GetDefault();
                if (_compass != null)
                {
                    _compass.ReadingChanged += CompassReadingChanged;
                    CompassChanged(_compass.GetCurrentReading());
                }

                _gps = new Geolocator();
                _gps.MovementThreshold = _movementThreshold;
                _gps.PositionChanged += GpsPositionChanged;

                if (_gps.LocationStatus == PositionStatus.Ready)
                {
                    var pos = await _gps.GetGeopositionAsync();
                    if (pos != null && pos.Coordinate != null && pos.Coordinate.Point != null)
                    {
                        GpsChanged(pos.Coordinate.Point.Position);
                    }
                }
            }
            catch { }
        }

        private void SimpleOrientationSensorReadingChanged(SimpleOrientationSensor sender, SimpleOrientationSensorOrientationChangedEventArgs args)
        {
            UpdateOrientation(args.Orientation);
        }

        private async void UpdateOrientation(SimpleOrientation orientation)
        {
            try
            {
                VideoRotation videoRotation = VideoRotation.None;
                bool showMap = false;

                switch (orientation)
                {
                    case SimpleOrientation.NotRotated:
                        videoRotation = VideoRotation.None;
                        break;
                    case SimpleOrientation.Rotated90DegreesCounterclockwise:
                        videoRotation = VideoRotation.Clockwise90Degrees;
                        break;
                    case SimpleOrientation.Rotated180DegreesCounterclockwise:
                        videoRotation = VideoRotation.Clockwise180Degrees;
                        break;
                    case SimpleOrientation.Rotated270DegreesCounterclockwise:
                        videoRotation = VideoRotation.Clockwise270Degrees;
                        break;
                    case SimpleOrientation.Facedown:
                    case SimpleOrientation.Faceup:
                        showMap = true;
                        break;
                    default:
                        break;
                }

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    if (PreviewScreen.Source != null)
                    {
                        PreviewScreen.Source.SetPreviewRotation(videoRotation);
                    }

                    if (showMap)
                    {
                        MapView.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MapView.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch { }
        }

        private void CompassReadingChanged(Compass sender, CompassReadingChangedEventArgs args)
        {
            var reading = sender.GetCurrentReading();
            CompassChanged(reading);
        }

        private async void CompassChanged(CompassReading reading)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CompassReading.Text = reading.HeadingMagneticNorth + "";
                    _currentHeading = reading.HeadingMagneticNorth;

                    if (_gpsPin != null)
                    {
                        _gpsPin.DataContext = _currentHeading;
                    }

                    UpdateARView();
                });
            }
            catch { }
        }

        private void GpsPositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            if (args.Position != null && args.Position.Coordinate != null &&
                args.Position.Coordinate.Point != null)
            {
                GpsChanged(args.Position.Coordinate.Point.Position);
            }
        }

        private async void GpsChanged(BasicGeoposition position)
        {
            try
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        _currentLocation = new Geopoint(
                            new BasicGeoposition()
                            {
                                Latitude = position.Latitude,
                                Longitude = position.Longitude
                            }
                            );
                        _poiLocations = null;

                        if (_pinLayer != null)
                        {
                            _pinLayer.Children.Clear();

                            var poi =
                                await
                                    NavteqPoiSearch(new Point(_currentLocation.Position.Latitude,
                                        _currentLocation.Position.Longitude));
                            if (poi != null && poi.ResultSet != null &&
                                poi.ResultSet.Results != null &&
                                poi.ResultSet.Results.Length > 0)
                            {
                                _poiLocations = poi.ResultSet.Results;

                                foreach (var r in _poiLocations)
                                {
                                    var loc = new Geopoint(
                                        new BasicGeoposition()
                                        {
                                            Latitude = r.Latitude,
                                            Longitude = r.Latitude
                                        }
                                        );

                                    var pin = new UserPushpin();
                                    pin.Tag = r;
                                    MapControl.SetLocation(pin, loc);
                                    _pinLayer.Children.Add(pin);
                                }

                                MyMap.TrySetViewAsync(_currentLocation, 16);
                            }

                            MapControl.SetLocation(_gpsPin, _currentLocation);

                            UpdateARView();
                        }
                    }
                    catch { }
                });
            }
            catch { }
        }









    }
}
