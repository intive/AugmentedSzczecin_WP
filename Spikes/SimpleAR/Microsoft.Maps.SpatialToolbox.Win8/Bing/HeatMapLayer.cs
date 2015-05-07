using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;

#if WINDOWS_APP
using Bing.Maps;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml;
#elif WINDOWS_PHONE
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using System.Windows.Media;
using System.Device.Location;
using System.Windows.Media.Imaging;
using System.Windows;
#elif WINDOWS_PHONE_APP
using Windows.UI.Xaml.Controls;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
#endif

namespace Microsoft.Maps.SpatialToolbox.Bing
{
    /// <summary>
    /// A map layer that represents data as a heat map.
    /// </summary>
    public class HeatMapLayer : Grid, INotifyPropertyChanged
    {
        #region Private Properties

        #if WINDOWS_APP
        private WebView _baseWebView; 
        private LocationCollection _locations;
        private Location _center = new Location(0,0);
        private Uri pageUri = new Uri("ms-appx-web:///Microsoft.Maps.SpatialToolbox.Win8/Bing/Assets/HeatMap.html", UriKind.Absolute);
        #elif WINDOWS_PHONE
        private WebBrowser _baseWebView;
        private GeoCoordinateCollection _locations;
        private GeoCoordinate _center = new GeoCoordinate(0, 0);
        private Uri pageUri = new Uri("/Bing/Assets/HeatMap.html", UriKind.Relative);
        #elif WINDOWS_PHONE_APP
        private WebView _baseWebView;
        private Geopath _locations;
        private Geopoint _center = new Geopoint(new BasicGeoposition(){ Latitude = 0, Longitude = 0});
        private Uri pageUri = new Uri("ms-appx-web:///Microsoft.Maps.SpatialToolbox.WP8.1/Bing/Assets/HeatMap.html", UriKind.Absolute);        
        #endif

        #if WINDOWS_PHONE_APP
        private MapControl _map;
        #else
        private Map _map;
        #endif
        
        private GradientStopCollection _heatGradient;

        private int _viewChangeEndWaitTime = 100;

        private double _radius = 10000;
        private double _intensity = 0.5;
        private int _minRadius = 2;
        private bool _enableHardEdge;

        private bool _isLoaded, _isMoving, _skipStartEventOnce;
        private bool _zoomed, _panned;
        
        private double _zoom = 0;

        private DateTime _lastMovement;
        private Timer _updateTimer;

        private Image _img;

        #endregion

        #region Constructor

        public HeatMapLayer()
            : base()
        {
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;

#if WINDOWS_APP || WINDOWS_PHONE_APP
            _baseWebView = new WebView()
            {
                Name = "BaseWebView",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                DefaultBackgroundColor = Windows.UI.Colors.Transparent
            };

             _baseWebView.NavigationCompleted += (s,e) => {                
                _baseWebView.Visibility = Visibility.Collapsed;
                _isLoaded = true;
                LoadHeatMap();
                ResizeHeatMap();
                Render();
            };

            _baseWebView.ScriptNotify += _baseWebView_ScriptNotify;
            _baseWebView.Navigate(pageUri);
#elif WINDOWS_PHONE
            _baseWebView = new WebBrowser()
            {
                Name = "BaseWebView",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsScriptEnabled = true,
                UseOptimizedManipulationRouting = true
            };

            _baseWebView.Navigated += (s, e) =>
            {
                _baseWebView.Visibility = Visibility.Collapsed;
                _isLoaded = true;
                LoadHeatMap();
                ResizeHeatMap();
                Render();
            };

            _baseWebView.ScriptNotify += _baseWebView_ScriptNotify;            
            _baseWebView.Navigate(pageUri);
#endif

            this.Children.Add(_baseWebView);
            
            _img = new Image();
            this.Children.Add(_img);

            this.PropertyChanged += (s, e) => {
                switch (e.PropertyName)
                {
                    case "Radius":
                    case "Locations":
                        Render();
                        break;
                    case "Intensity":
                    case "HeatGradient":
                    case "EnableHardEdge":
                        SetOptions();
                        break;
                    case "ParentMap":
                        AttachMapEvents();
                        break;
                }                
            };
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Heat gradient to use to render heat map.
        /// </summary>
        public GradientStopCollection HeatGradient
        {
            get { return _heatGradient; }
            set
            {
                _heatGradient = value;
                OnPropertyChanged("HeatGradient");
            }
        }

        /// <summary>
        /// Insenity of the heat map. A value between 0 and 1.
        /// </summary>
        public double Intensity
        {
            get { return _intensity; }
            set
            {
                _intensity = Math.Min(1,Math.Max(0,value));
                OnPropertyChanged("Intensity");
            }
        }
       
#if WINDOWS_APP
        /// <summary>
        /// Collection of locations to plot on the heat map.
        /// </summary>
        public LocationCollection Locations
        {
            get { return _locations; }
            set
            {
                _locations = value;
                OnPropertyChanged("Locations");
            }
        }
#elif WINDOWS_PHONE
        /// <summary>
        /// Collection of locations to plot on the heat map.
        /// </summary>
        public GeoCoordinateCollection Locations
        {
            get { return _locations; }
            set
            {
                _locations = value;
                OnPropertyChanged("Locations");
            }
        }
#elif WINDOWS_PHONE_APP
        /// <summary>
        /// Collection of locations to plot on the heat map.
        /// </summary>
        public Geopath Locations
        {
            get { return _locations; }
            set
            {
                _locations = value;
                OnPropertyChanged("Locations");
            }
        }
#endif

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Reference to the Map control
        /// </summary>
        public MapControl ParentMap
        {
            get { return _map; }
            set
            {
                _map = value;
                OnPropertyChanged("ParentMap");
            }
        }
#else
        /// <summary>
        /// Reference to Bing Maps control
        /// </summary>
        public Map ParentMap
        {
            get { return _map; }
            set
            {
                _map = value;
                OnPropertyChanged("ParentMap");
            }
        }
#endif

        /// <summary>
        /// Radius of data point in meters.
        /// </summary>
        public double Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                OnPropertyChanged("Radius");
            }
        }

        /// <summary>
        /// Gives all values the same opacity to create a hard edge on each data point. 
        /// When set to false (default) the data points will use a fading opacity towards the edges.
        /// </summary>
        public bool EnableHardEdge
        {
            get { return _enableHardEdge; }
            set
            {
                _enableHardEdge = value;
                OnPropertyChanged("EnableHardEdge");
            }
        }

        #endregion

        #region Private Methods

        #region Map Event Handlers

        private void DetachMapEvents()
        {
#if WINDOWS_APP
            _map.ViewChangeStarted -= _map_ViewChangeStarted;
            _map.ViewChangeEnded -= _map_ViewChangeEnded;
            _map.SizeChanged -= _map_SizeChanged;
#elif WINDOWS_PHONE || WINDOWS_PHONE_APP
            _map.ZoomLevelChanged -= _map_ZoomLevelChanged;
            _map.CenterChanged -= _map_CenterChanged;
            _map.SizeChanged -= _map_SizeChanged;
#endif
        }

        #if WINDOWS_APP
        private void AttachMapEvents()
        {
            this.Width = _map.ActualWidth;
            this.Height = _map.ActualHeight;

            //remove any old event handlers
            DetachMapEvents();

            //Add new map events
            _map.ViewChangeStarted += _map_ViewChangeStarted;
            _map.ViewChangeEnded += _map_ViewChangeEnded;

            _map.SizeChanged += _map_SizeChanged;

            if (_updateTimer != null)
            {
                _updateTimer.Dispose();
            }

            //Use a timer to throttle the updating due to moving the map
            _updateTimer = new Timer(async (s) =>
            {
                try
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                    {
                        if (_panned && _zoomed)
                        {
                            _skipStartEventOnce = true;
                        }

                        if ((_panned || _zoomed) && !_isMoving && (DateTime.Now - _lastMovement).Milliseconds < _viewChangeEndWaitTime)
                        {
                            //Update Heat Map
                            Render();

                            _panned = false;
                            _zoomed = false;
                        }
                        else if (!_panned && !_zoomed && !_isMoving)
                        {
                            _img.Visibility = Visibility.Visible;
                        }
                    });
                }
                catch { }
            }, null, 0, 100);
        }

        private void _map_ViewChangeEnded(object sender, object e)
        {
            if (_zoom != _map.ZoomLevel)
            {
                _zoomed = true;
                _zoom = _map.ZoomLevel;
            }
            else
            {
                _zoomed = false;
            }

            if (_center.Latitude != _map.Center.Latitude || _center.Longitude != _map.Center.Longitude)
            {
                _panned = true;
                _center = _map.Center;
            }
            else
            {
                _panned = false;
            }

            _lastMovement = DateTime.Now;
            _isMoving = false;
        }

        private void _map_ViewChangeStarted(object sender, object e)
        {         
            if (!_skipStartEventOnce)
            {
                //Clear Heat Map
                _img.Visibility = Visibility.Collapsed;
                _img.Source = null;
                _isMoving = true;
            }
            else
            {
                _skipStartEventOnce = false;
            }
        }
#elif WINDOWS_PHONE || WINDOWS_PHONE_APP
        private void AttachMapEvents()
        {
            this.Width = _map.ActualWidth;
            this.Height = _map.ActualHeight;

            //remove any old event handlers
            DetachMapEvents();

            //Add new map events
            _map.ZoomLevelChanged += _map_ZoomLevelChanged;
            _map.CenterChanged += _map_CenterChanged;
            _map.SizeChanged += _map_SizeChanged;

            if (_updateTimer != null)
            {
                _updateTimer.Dispose();
            }

            //Use a timer to throttle the updating due to moving the map
            _updateTimer = new Timer(async (s) =>
            {
                try
                {
                    
            #if WINDOWS_PHONE
                    Dispatcher.BeginInvoke(() =>
            #elif WINDOWS_PHONE_APP
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            #endif
                    
                    {
                        if (_panned && _zoomed)
                        {
                            _skipStartEventOnce = true;
                        }

                        if ((_panned || _zoomed) && !_isMoving && (DateTime.Now - _lastMovement).Milliseconds < _viewChangeEndWaitTime)
                        {
                            //Update Heat Map
                            Render();

                            _panned = false;
                            _zoomed = false;
                        }
                    });
                }
                catch { }
            }, null, 0, 100);
        }

        private void _map_CenterChanged(object sender, object e)
        {
            _img.Visibility = Visibility.Collapsed;

#if WINDOWS_PHONE
            if (_center.Latitude != _map.Center.Latitude || _center.Longitude != _map.Center.Longitude)
#elif WINDOWS_PHONE_APP
            if (_center.Position.Latitude != _map.Center.Position.Latitude || _center.Position.Longitude != _map.Center.Position.Longitude)
#endif
            {
                _panned = true;
                _center = _map.Center;
            }
            else
            {
                _panned = false;
            }

            _lastMovement = DateTime.Now;
            _isMoving = false;
        }

        private void _map_ZoomLevelChanged(object sender, object e)
        {
            _img.Visibility = Visibility.Collapsed;

            if (_zoom != _map.ZoomLevel)
            {
                _zoomed = true;
                _zoom = _map.ZoomLevel;
            }
            else
            {
                _zoomed = false;
            }

            _lastMovement = DateTime.Now;
            _isMoving = false;
        }
#endif

        private void _map_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Width = _map.ActualWidth;
            this.Height = _map.ActualHeight;
            ResizeHeatMap();
        }

        #endregion

        private async void _baseWebView_ScriptNotify(object sender, NotifyEventArgs e)
        {
            try
            {
                var bmp = new BitmapImage();
                var base64 = e.Value.Substring(e.Value.IndexOf(",") + 1);
                var imageBytes = Convert.FromBase64String(base64);

#if WINDOWS_APP || WINDOWS_PHONE_APP
                using (var ms = new Windows.Storage.Streams.InMemoryRandomAccessStream())
                {   
                    using (var writer = new Windows.Storage.Streams.DataWriter(ms.GetOutputStreamAt(0)))
                    {
                        writer.WriteBytes(imageBytes);
                        await writer.StoreAsync();
                    }

                    bmp.SetSource(ms);
                }
#elif WINDOWS_PHONE
                using (var ms = new System.IO.MemoryStream(imageBytes))
                {
                    ms.Position = 0;
                    bmp.SetSource(ms);
                }
#endif                

                imageBytes = null;
                _img.Source = bmp;
                _img.Visibility = Visibility.Visible;
            }
            catch { }
        }

        #region Heat Map Methods

        private void LoadHeatMap()
        {
            InvokeJS("LoadHeatMap", new string[]{GenerateOptionJson()});
        }

        private void ResizeHeatMap()
        {
            InvokeJS("Resize", new string[] { this.Width + "", this.Height + "" });
        }

        private async void Render()
        {
            try
            {
                if (_isLoaded && _map != null)
                {
                    StringBuilder sb = new StringBuilder();

                    if (Locations != null)
                    {
#if WINDOWS_APP
                        var px = new List<Windows.Foundation.Point>();

                        _map.TryLocationsToPixels(Locations, px);

                        await System.Threading.Tasks.Task.Run(() =>
                        {
                            foreach (var p in px)
                            {
                                sb.AppendFormat("{0},{1}|", Math.Round(p.X), Math.Round(p.Y));
                            }
                        });
#elif WINDOWS_PHONE
                        foreach (var l in Locations)
                        {
                            var p = _map.ConvertGeoCoordinateToViewportPoint(l);
                            sb.AppendFormat("{0},{1}|", Math.Round(p.X), Math.Round(p.Y));
                        }                        
#elif WINDOWS_PHONE_APP
                        await System.Threading.Tasks.Task.Run(() =>
                        {
                            Windows.Foundation.Point p;

                            foreach (var l in Locations.Positions)
                            {
                                _map.GetOffsetFromLocation(new Geopoint(l), out p);
                                sb.AppendFormat("{0},{1}|", Math.Round(p.X), Math.Round(p.Y));
                            }                         
                        });
#endif
                    }

                    if (sb.Length > 0)
                    {
                        sb.Remove(sb.Length - 1, 1);
                    }

                    //Ground resolution at equator
                    var groudResolution = (2 * Math.PI * 6378135) / Math.Round(256 * Math.Pow(2, _map.ZoomLevel));
                    var radiusInPixels = _radius / groudResolution;

                    if (radiusInPixels < _minRadius)
                    {
                        radiusInPixels = _minRadius;
                    }

                    string[] args = { sb.ToString(), radiusInPixels.ToString(), _map.ZoomLevel.ToString() };

                    InvokeJS("Render", args);
                }
            }
            catch { }
        }

        private string GenerateOptionJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");

            if (_heatGradient != null && _heatGradient.Count > 0)
            {
                sb.Append("'colorgradient':{");
                foreach (var hg in _heatGradient)
                {
                    sb.AppendFormat("'{0}': 'rgba({1},{2},{3},{4})',", hg.Offset, hg.Color.R, hg.Color.G, hg.Color.B, hg.Color.A);
                }

                sb.Remove(sb.Length - 1, 1);

                sb.Append("},");
            }

            sb.AppendFormat("'enableHardEdge': {0},", (_enableHardEdge) ? "true" : "false");

            sb.AppendFormat("'intensity': {0}}}", _intensity);
            return sb.ToString();
        }

        private void SetOptions()
        {
            InvokeJS("SetOptions", new string[] { GenerateOptionJson(), "true" });
        }

        #endregion

        private async void InvokeJS(string methodName, string[] args)
        {
            try
            {
                if (_isLoaded)
                {
                    #if WINDOWS_APP|| WINDOWS_PHONE_APP
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
                    {
                        await _baseWebView.InvokeScriptAsync(methodName, args);
                    });
                    #elif WINDOWS_PHONE
                    Dispatcher.BeginInvoke(() =>
                    {
                        _baseWebView.InvokeScript(methodName, args);
                    });
                    #endif
                }
            }
            catch (Exception ex)
            {
                var t = "";
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
