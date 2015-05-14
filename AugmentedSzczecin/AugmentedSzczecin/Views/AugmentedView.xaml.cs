﻿using AugmentedSzczecin.Events;
using AugmentedSzczecin.Models;
using Caliburn.Micro;
using Microsoft.Maps.SpatialToolbox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace AugmentedSzczecin.Views
{
    public sealed partial class AugmentedView : Page, IHandle<PointOfInterestLoadedEvent>, IHandle<PointOfInterestLoadFailedEvent>
    {
        private SimpleOrientationSensor _orientationSensor;
        private Compass _compass;
        private Geolocator _gps;

        private uint _movementThreshold = 100;
        private double _defaultSearchRadius = 1;

        private Geopoint _currentLocation = null;
        private double _currentHeading = 0;

        private ObservableCollection<PointOfInterest> _poiLocations;
        readonly object _eventAgg;
        public AugmentedView()
        {
            InitializeComponent();

            _eventAgg = IoC.GetInstance(typeof(IEventAggregator), null);
            ((EventAggregator)_eventAgg).Subscribe(this);
        }

        ~AugmentedView()
        {
            ((EventAggregator)_eventAgg).Unsubscribe(this);
        }
        private async Task StartCamera()
        {
            var mediaCapture = new MediaCapture();
            (App.Current as App)._mediaCapture = mediaCapture;
            await mediaCapture.InitializeAsync();
            PreviewScreen.Source = mediaCapture;
            await mediaCapture.StartPreviewAsync();
        }

        private async void StopCamera()
        {
            if (PreviewScreen.Source != null)
            {
                await PreviewScreen.Source.StopPreviewAsync();
                PreviewScreen.Source.Dispose();
                PreviewScreen.Source = null;
            }
        }

        private void UpdateARView()
        {
            if (_currentLocation != null)
            {
                ItemCanvas.Children.Clear();

                if (_poiLocations != null && _poiLocations.Count > 0)
                {
                    var center = new Coordinate(_currentLocation.Position.Latitude,
                        _currentLocation.Position.Longitude);

                    foreach (var poi in _poiLocations)
                    {

                        var c = new Coordinate(poi.Location.Latitude, poi.Location.Longitude);
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

                            double top = ItemCanvas.ActualHeight * (1 - distance / _defaultSearchRadius);

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

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            _orientationSensor.OrientationChanged -= SimpleOrientationSensorReadingChanged;
            _compass.ReadingChanged -= CompassReadingChanged;
            _gps.PositionChanged -= GpsPositionChanged;
            StopCamera();
        }

        private void SimpleOrientationSensorReadingChanged(SimpleOrientationSensor sender, SimpleOrientationSensorOrientationChangedEventArgs args)
        {
            UpdateOrientation(args.Orientation);
        }

        private async void UpdateOrientation(SimpleOrientation orientation)
        {
            VideoRotation videoRotation = VideoRotation.None;

            switch (orientation)
            {
                case SimpleOrientation.NotRotated:
                    videoRotation = VideoRotation.Clockwise90Degrees;
                    break;
                case SimpleOrientation.Rotated90DegreesCounterclockwise:
                    videoRotation = VideoRotation.None;
                    break;
                case SimpleOrientation.Rotated180DegreesCounterclockwise:
                    videoRotation = VideoRotation.Clockwise270Degrees;
                    break;
                case SimpleOrientation.Rotated270DegreesCounterclockwise:
                    videoRotation = VideoRotation.Clockwise180Degrees;
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

        private void CompassReadingChanged(Compass sender, CompassReadingChangedEventArgs args)
        {
            var reading = sender.GetCurrentReading();
            CompassChanged(reading);
        }

        private async void CompassChanged(CompassReading reading)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _currentHeading = reading.HeadingMagneticNorth;

                UpdateARView();
            });
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
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {

                _currentLocation = new Geopoint(new BasicGeoposition() { Latitude = position.Latitude, Longitude = position.Longitude });
                UpdateARView();

            });
        }

        public void Handle(PointOfInterestLoadedEvent e)
        {
            _poiLocations = e.PointOfInterestList;
        }

        public void Handle(PointOfInterestLoadFailedEvent e)
        {
            var msg = new MessageDialog(e.PointOfInterestLoadException.Message);
            msg.ShowAsync();
        }
    }
}
