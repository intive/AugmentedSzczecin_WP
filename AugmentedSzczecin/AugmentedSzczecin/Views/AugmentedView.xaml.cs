using AugmentedSzczecin.Models;
using Microsoft.Maps.SpatialToolbox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace AugmentedSzczecin.Views
{
    public sealed partial class CameraView : Page
    {
        private SimpleOrientationSensor _orientationSensor;
        private Compass _compass;
        private Geolocator _gps;

        private uint _movementThreshold = 100;
        private double _defaultSeatchRadius = 1;

        private Geopoint _currentLocation = null;
        private double _currentHeading = 0;

        private PointOfInterest[] _poiLocations = { 
                                                    new PointOfInterest("1") {Name = "Point 1", Latitude = 53.429236, Longitude = 14.556504},
                                                    new PointOfInterest("2") {Name = "Point 2", Latitude = 53.428716, Longitude = 14.556604},
                                                    new PointOfInterest("3") {Name = "Point 3", Latitude = 53.428419, Longitude = 14.556035},
                                                    new PointOfInterest("4") {Name = "Point 4", Latitude = 53.428566, Longitude = 14.555177},
                                                    new PointOfInterest("5") {Name = "Point 5", Latitude = 53.429045, Longitude = 14.555270},
                                                    new PointOfInterest("6") {Name = "Point 6", Latitude = 53.397248, Longitude = 14.525335},
                                                    new PointOfInterest("7") {Name = "Point 7", Latitude = 53.397200, Longitude = 14.524005},
                                                    new PointOfInterest("8") {Name = "Point 8", Latitude = 53.396524, Longitude = 14.523997},
                                                    new PointOfInterest("9") {Name = "Point 9", Latitude = 53.396329, Longitude = 14.525113},
                                                    new PointOfInterest("10") {Name = "Point 10", Latitude = 53.396708, Longitude = 14.526167}
                                                  };

        private async Task StartCamera()
        {
            var mediaCapture = new MediaCapture();
            mediaCapture.SetPreviewRotation(VideoRotation.Clockwise270Degrees);
            mediaCapture.SetRecordRotation(VideoRotation.Clockwise270Degrees);
            try
            {
                PreviewScreen.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();
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
                    await PreviewScreen.Source.StopPreviewAsync();
                    PreviewScreen.Source.Dispose();
                    PreviewScreen.Source = null;
                }
            }
            catch { }
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
            //UpdateOrientation(args.Orientation);
        }

        private async void UpdateOrientation(SimpleOrientation orientation)
        {
            try
            {
                VideoRotation videoRotation = VideoRotation.None;

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
                    default:
                        break;
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (PreviewScreen.Source != null)
                    {
                        PreviewScreen.Source.SetPreviewRotation(videoRotation);
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
                    _currentHeading = reading.HeadingMagneticNorth;

                    UpdateARView();
                });
            }
            catch { }
        }

        private void GpsPositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            if (args.Position != null && args.Position.Coordinate != null && args.Position.Coordinate.Point != null)
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
                        _currentLocation = new Geopoint(new BasicGeoposition() { Latitude = position.Latitude, Longitude = position.Longitude });
                        UpdateARView();
                    }
                    catch { }
                });
            }
            catch { }
        }
    }
}
