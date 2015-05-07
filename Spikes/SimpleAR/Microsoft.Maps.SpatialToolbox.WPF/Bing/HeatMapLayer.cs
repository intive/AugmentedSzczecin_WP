using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using Microsoft.Maps.SpatialToolbox.Imaging;
using System.Windows.Media;

namespace Microsoft.Maps.SpatialToolbox.Bing
{
    public class HeatMapLayer : Grid, INotifyPropertyChanged
    {
        #region Private Properties

        private LocationCollection _locations;
        private List<System.Drawing.PointF> _points;

        private HeatMapStyle _style;
        private HeatMapRenderEngine _renderEngine;

        private Map _map;

        private GradientStopCollection _heatGradient;

        private int _viewChangeEndWaitTime = 100;

        private double _radius = 10000;
        private double _intensity = 0.5;
        private int _minRadius = 2;
        private bool _enableHardEdge;

        private bool _isMoving, _skipStartEventOnce;
        private bool _zoomed, _panned;
        private Location _center;

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

            _img = new Image();
            this.Children.Add(_img);

            _heatGradient = new GradientStopCollection(){
                new GradientStop(Colors.Navy, 0),
                new GradientStop(Colors.Blue, 0.25),
                new GradientStop(Colors.Green, 0.5),
                new GradientStop(Colors.Yellow, 0.75),
                new GradientStop(Colors.Red, 1)
            };

            _renderEngine = new HeatMapRenderEngine();
            _style = new HeatMapStyle()
            {
                Intensity = _intensity,
                HeatGradient = _heatGradient
            };

            this.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case "Locations":
                        Render();
                        break;
                    case "Radius":
                        RefreshLayer();
                        break;
                    case "Intensity":
                        _style.Intensity = _intensity;
                        RefreshLayer();
                        break;
                    case "HeatGradient":
                    case "EnableHardEdge":
                        _style.EnableHardEdge = _enableHardEdge;
                        _style.HeatGradient = _heatGradient;
                        RefreshLayer();
                        break;
                    case "ParentMap":
                        AttachMapEvents();
                        break;
                }
            };
        }

        #endregion

        #region Public Properties

         ///<summary>
         ///Heat gradient to use to render heat map.
         ///</summary>
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
                _intensity = Math.Min(1, Math.Max(0, value));
                OnPropertyChanged("Intensity");
            }
        }


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
            _map.ViewChangeStart -= _map_ViewChangeStart;
            _map.ViewChangeEnd -= _map_ViewChangeEnd;
            _map.SizeChanged -= _map_SizeChanged;
        }

        private void AttachMapEvents()
        {
            this.Width = _map.ActualWidth;
            this.Height = _map.ActualHeight;

            //remove any old event handlers
            DetachMapEvents();

            _center = _map.Center;
            _zoom = _map.ZoomLevel;

            //Add new map events
            _map.ViewChangeStart += _map_ViewChangeStart;
            _map.ViewChangeEnd += _map_ViewChangeEnd;

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
                    await Dispatcher.BeginInvoke(new Action(() =>
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
                            //_img.Visibility = Visibility.Visible;
                        }
                    }), null);
                }
                catch { }
            }, null, 0, 100);
        }

        private void _map_ViewChangeEnd(object sender, object e)
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

        private void _map_ViewChangeStart(object sender, object e)
        {         
            //if (!_skipStartEventOnce)
            //{
                //Clear Heat Map
                _img.Visibility = Visibility.Collapsed;
                _img.Source = null;
                _isMoving = true;
            //}
            //else
            //{
            //    _skipStartEventOnce = false;
            //}
        }

        private void _map_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Width = _map.ActualWidth;
            this.Height = _map.ActualHeight;

            Render();
        }

        #endregion

        #region Heat Map Methods

        private void Render()
        {
            try
            {
                if (_map != null && Locations != null)
                {
                    _points = new List<System.Drawing.PointF>(_locations.Count);

                    System.Windows.Point p;
                    foreach (var l in _locations)
                    {
                        _map.TryLocationToViewportPoint(l, out p);
                        _points.Add(new System.Drawing.PointF((float)p.X, (float)p.Y));
                    }

                    RefreshLayer();
                }
                else
                {
                    _img.Source = null;
                }
            }
            catch { }
        }

        /// <summary>
        /// Redraws the layer using the last calculated coordinates.
        /// </summary>
        private async void RefreshLayer()
        {
            try
            {
                //Ground resolution at equator
                var groudResolution = (2 * Math.PI * 6378135) / Math.Round(256 * Math.Pow(2, _map.ZoomLevel));
                var radiusInPixels = _radius / groudResolution;

                if (radiusInPixels < _minRadius)
                {
                    radiusInPixels = _minRadius;
                }

                using (var ms = await _renderEngine.RenderData(_points, _style, (int)radiusInPixels, (int)Math.Ceiling(this.Width), (int)Math.Ceiling(this.Height))) 
                {
                    if (ms != null)
                    {
                        ms.Position = 0;
                        var bmp = new System.Windows.Media.Imaging.BitmapImage();
                        bmp.BeginInit();
                        bmp.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                        bmp.StreamSource = ms;
                        bmp.EndInit();

                        _img.Source = bmp;
                        _img.Visibility = Visibility.Visible;
                    }
                }
            }
            catch { }
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

        #endregion
    }
}
