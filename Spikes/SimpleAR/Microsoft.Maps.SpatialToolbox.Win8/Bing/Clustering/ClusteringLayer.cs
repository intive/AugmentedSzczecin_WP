using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if WINDOWS_APP
using Bing.Maps;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
#elif WPF
using System.Windows.Controls;
using System.Windows;
using Microsoft.Maps.MapControl.WPF;
using System.Windows.Media;
#elif WINDOWS_PHONE_APP
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#elif WINDOWS_PHONE
using Microsoft.Phone.Maps.Controls;
using System.Device.Location;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
#endif

namespace Microsoft.Maps.SpatialToolbox.Bing.Clustering
{
    /// <summary>C:\Users\richbrun\Desktop\Code Samples\CodePlex37\Mirosoft.Maps.SpatialToolbox\Source\Microsoft.Maps.SpatialToolbox.Win8\Bing\HeatMapLayer.cs
    /// A MapLayer that clusters a set of data based on the current map view and zoom level. 
    /// This offers increased performance and a better user experience when rendering thousands of pushpins on a map.
    /// </summary>
    public class ClusteringLayer : Panel
    {
        #region Private Properties

        private int _currentZoomLevel;
        private const int _maxZoomLevel = 21;
        private int clusterRadius = 45;
        private ClusteringType clusterType = ClusteringType.Grid;

        private ItemLocationCollection _items;      
        private ClusteredPoint[] clusters;

#if WINDOWS_PHONE_APP
        internal MapControl _map;
        private IList<DependencyObject> _baseLayer;
        
        private Geopoint _center;
        private List<Geopoint> _allLocations;
#elif WINDOWS_PHONE
        internal Map _map;
        private MapLayer _baseLayer;
        
        private GeoCoordinate _center;
        private GeoCoordinateCollection _allLocations;
#else
        internal Map _map;
        private MapLayer _parentLayer;
        private MapLayer _baseLayer;
        
        private Location _center;
        private LocationCollection _allLocations;
#endif

        #endregion

        #region Constructor 

        /// <summary>
        /// A MapLayer that clusters a set of data based on the current map view and zoom level. 
        /// This offers increased performance and a better user experience when rendering thousands of pushpins on a map.
        /// </summary>
#if WINDOWS_PHONE_APP
        public ClusteringLayer(MapControl map)
        {
            _map = map;

            _items = new ItemLocationCollection();
            _items.CollectionChanged += () =>
            {
                _allLocations.Clear();

                foreach (var i in _items)
                {
                    _allLocations.Add(i.Location);
                }

                Cluster();
            };

            _allLocations = new List<Geopoint>();

            this.Unloaded += (s, e) =>
            {
                if (_map != null)
                {
                    _map.ZoomLevelChanged -= _map_ViewChangeEnded;
                    _map.CenterChanged -= _map_ViewChangeEnded;
                    _map.SizeChanged -= _map_SizeChanged;
                }
            };

            Init();
        }
#elif WINDOWS_PHONE
        public ClusteringLayer(Map map)
        {
            _map = map;

            _items = new ItemLocationCollection();
            _items.CollectionChanged += () =>
            {
                _allLocations.Clear();

                foreach (var i in _items)
                {
                    _allLocations.Add(i.Location);
                }

                Cluster();
            };

            _allLocations = new GeoCoordinateCollection();

            this.Unloaded += (s, e) =>
            {
                if (_map != null)
                {
                    _map.ViewChanged -= _map_ViewChangeEnded;
                    _map.SizeChanged -= _map_SizeChanged;
                }
            };
        }
#else
        public ClusteringLayer()
        {
            _items = new ItemLocationCollection();
            _items.CollectionChanged += () =>
            {
                _allLocations.Clear();

                foreach (var i in _items)
                {
                    _allLocations.Add(i.Location);
                }

                Cluster();
            };

            _allLocations = new LocationCollection();

            this.Loaded += (s, e) =>
            {
                DependencyObject parent = this;
                while (parent != null && !(parent is Map))
                {
                    parent = VisualTreeHelper.GetParent(parent);

                    if (parent is MapLayer)
                    {
                        _parentLayer = parent as MapLayer;
                    }
                }

                if (parent != null && _parentLayer != null)
                {
                    _map = parent as Map;
                    Init();
                }
            };

            this.Unloaded += (s, e) =>
            {
                if (_map != null)
                {
#if WINDOWS_APP
                    _map.ViewChangeEnded -= _map_ViewChangeEnded;
#elif WPF
                    _map.ViewChangeEnd -= _map_ViewChangeEnded;
#endif
                    _map.SizeChanged -= _map_SizeChanged;
                }
            };
        }
#endif

        #endregion

        #region Public Properties

        /// <summary>
        /// A collection of ItemLocation objects to cluster.
        /// </summary>
        public ItemLocationCollection Items
        {
            get
            {
                return _items;
            }
        }
   
        /// <summary>
        /// Radius of a cluster. 
        /// </summary>
        public int ClusterRadius
        {
            get
            {
                return clusterRadius;
            }
            set
            {
                if (value > 0)
                {
                    clusterRadius = value;
                    Cluster();
                }
            }
        }

        /// <summary>
        /// The type of clustering algorithm to use to cluster data.
        /// </summary>
        public ClusteringType ClusterType
        {
            get
            {
                return clusterType;
            }
            set
            {
                clusterType = value;
                Cluster();
            }
        }

        #endregion

        #region Public Events

#if WINDOWS_PHONE
        public delegate MapOverlay ItemRenderCallback(object item, ClusteredPoint clusterInfo);
        public delegate MapOverlay ClusteredItemRenderCallback(ClusteredPoint clusterInfo);
#else
        public delegate UIElement ItemRenderCallback(object item, ClusteredPoint clusterInfo);
        public delegate UIElement ClusteredItemRenderCallback(ClusteredPoint clusterInfo);
#endif
        /// <summary>
        /// A callback method used to create a single Pushpin icon.
        /// </summary>
        public event ItemRenderCallback CreateItemPushpin;  

        /// <summary>
        /// A callback method used to create a clustered Pushpin icon.
        /// </summary>
        public event ClusteredItemRenderCallback CreateClusteredItemPushpin;

        #endregion

        #region Private Methods

        private void Init()
        {
#if WINDOWS_PHONE_APP
            _baseLayer = _map.Children;
#elif WINDOWS_PHONE
            _baseLayer = new MapLayer();
            _map.Layers.Add(_baseLayer);
#else
            _baseLayer = new MapLayer();
            _parentLayer.Children.Add(_baseLayer);
#endif
            _currentZoomLevel = (int)Math.Round(_map.ZoomLevel);

#if WINDOWS_APP
            _map.ViewChangeEnded += _map_ViewChangeEnded;
#elif WPF
            _map.ViewChangeEnd += _map_ViewChangeEnded;
#elif WINDOWS_PHONE_APP
            _map.ZoomLevelChanged += _map_ViewChangeEnded;
            _map.CenterChanged += _map_ViewChangeEnded;
#endif
            _map.SizeChanged += _map_SizeChanged;

            Cluster();
        }

        private void _map_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Cluster();
        }

        private void _map_ViewChangeEnded(object sender, object e)
        {
            int zoom = (int)Math.Round(_map.ZoomLevel);

            //Only recluster if the map has moved
            if (_center != _map.Center || _currentZoomLevel != zoom)
            {
                _currentZoomLevel = zoom;
                _center = _map.Center;
                Cluster();
            }
        }

        private async void Cluster()
        {
            try
            {
                if (_map != null && _items != null && _items.Count > 0 && _currentZoomLevel <= _maxZoomLevel)
                {
#if WINDOWS_APP
                    var pixels = new List<Windows.Foundation.Point>();
                    _map.TryLocationsToPixels(_allLocations, pixels);

                    Windows.Foundation.Point centerPixel;
                    _map.TryLocationToPixel(new Location(0, 0), out centerPixel);
#elif WPF
                    var pixels = new List<System.Windows.Point>(_allLocations.Count);

                    foreach(var l in _allLocations){
                        pixels.Add(_map.LocationToViewportPoint(l));
                    };

                    System.Windows.Point centerPixel;
                    centerPixel = _map.LocationToViewportPoint(new Location(0, 0));
#elif WINDOWS_PHONE_APP
                    var pixels = new List<Windows.Foundation.Point>(_allLocations.Count);
                    Windows.Foundation.Point p;

                    foreach(var l in _allLocations){
                        _map.GetOffsetFromLocation(l, out p);
                        pixels.Add(p);
                    }

                    Windows.Foundation.Point centerPixel;
                    _map.GetOffsetFromLocation(new Geopoint(new BasicGeoposition(){ Latitude = 0, Longitude = 0}), out centerPixel);
#elif WINDOWS_PHONE
                    var pixels = new List<System.Windows.Point>(_allLocations.Count);

                    foreach(var l in _allLocations){
                        pixels.Add(_map.ConvertGeoCoordinateToViewportPoint(l));
                    };

                    System.Windows.Point centerPixel;
                    centerPixel = _map.ConvertGeoCoordinateToViewportPoint(new GeoCoordinate(0, 0));
#endif
                    switch (ClusterType)
                    {
                        case ClusteringType.Grid:
                            await CalculateGridClusters(pixels, centerPixel);
                            break;
                        case ClusteringType.Point:
                            await CalculatePointClusters(pixels);
                            break;
                        default:
                            break;
                    }
                }

                Render();
            }
            catch { }
        }

        private void Render()
        {
            if (_map != null)
            {
#if WINDOWS_PHONE_APP
                _baseLayer.Clear();
                UIElement pin;
#elif WINDOWS_PHONE
                _baseLayer.Clear();
                MapOverlay pin;
#else
                _baseLayer.Children.Clear();
                UIElement pin;
#endif
                
                foreach (var c in clusters)
                {
                    if (c != null)
                    {
                        pin = null;

                        if (c.ItemIndices.Count == 1)
                        {
                            var item = _items[c.ItemIndices[0]];

                            if (CreateItemPushpin != null)
                            {
                                pin = CreateItemPushpin(item, c);
                            }
                        }
                        else if(CreateClusteredItemPushpin != null)
                        {

                            pin = CreateClusteredItemPushpin(c);
                        }

                        if (pin != null)
                        {
#if WINDOWS_PHONE_APP || WINDOWS_PHONE
                            _baseLayer.Add(pin);
#else
                            _baseLayer.Children.Add(pin);
#endif
                        }
                    }
                }
            }
        }

        #endregion

        #region Clustering Logic

        /*
         * This class uses the Grid Based clustering algorithm to sort data into clusters based on the current map view 
         * and is derived from this code base: http://bingmapsv7modules.codeplex.com/wikipage?title=Client%20Side%20Clustering 
         * 
         * This algorithm is very fast and clusters the data that is only in the current map view. This algorithm 
         * recalculates the clusters every time the map moves (both when zooming and panning). As a result this causes 
         * the grid cells that are used for calculating clusters to move. This algorithm has a chance of clusters overlapping 
         * if pins are near the edge of a grid cell. This makes for a less ideal user experience however this is a trade off 
         * for performance. This algorithm is recommended for data sets of 50,000 data points or less.
         */
#if WINDOWS_APP || WINDOWS_PHONE_APP
        private async Task CalculateGridClusters(List<Windows.Foundation.Point> pixels, Windows.Foundation.Point centerPixel)
#elif WPF || WINDOWS_PHONE
        private async Task CalculateGridClusters(List<System.Windows.Point> pixels, System.Windows.Point centerPixel)
#endif
        {
            int gridSize = ClusterRadius * 2;

            int numXCells = (int)Math.Ceiling(_map.ActualWidth / gridSize);
            int numYCells = (int)Math.Ceiling(_map.ActualHeight / gridSize);

            int maxX = (int)Math.Ceiling(_map.ActualWidth + ClusterRadius);
            int maxY = (int)Math.Ceiling(_map.ActualHeight + ClusterRadius);

            //grid should be relative to 0,0 (lat,lon), not top-left corner of map, so that panning
            //doesn't cause items to move to different clusters and therefore cluster markers move
            //if placement is GridCenter
            var gridBaseX = centerPixel.X % gridSize;
            var gridBaseY = centerPixel.Y % gridSize;

            clusters = await Task.Run<ClusteredPoint[]>(() =>
            {
                int numCells = numXCells * numYCells;

                ClusteredPoint[] clusteredData = new ClusteredPoint[numCells];

#if WINDOWS_APP || WINDOWS_PHONE_APP
                Windows.Foundation.Point pixel;
#elif WPF || WINDOWS_PHONE
                System.Windows.Point pixel;
#endif
                int k, j, key;

                //Itirate through the data
                for (int i = 0; i < _items.Count; i++)
                {
                    var entity = _items[i];
                    pixel = pixels[i];

                    //Check to see if the pin is within the bounds of the viewable map
                    if (pixel != null && pixel.X <= maxX && pixel.Y <= maxY && pixel.X >= -ClusterRadius && pixel.Y >= -ClusterRadius)
                    {
                        //calculate the grid position on the map of where the location is located
                        k = (int)Math.Floor((pixel.X - gridBaseX) / gridSize);
                        j = (int)Math.Floor((pixel.Y - gridBaseY) / gridSize);

                        //calculates the grid location in the array
                        key = k + j * numXCells;

                        if (key >= 0 && key < numCells)
                        {
                            if (clusteredData[key] == null)
                            {
                                clusteredData[key] = new ClusteredPoint()
                                {
                                    Location = _allLocations[i],
                                    ItemIndices = new List<int>() { i },
                                    Zoom = _currentZoomLevel,
                                    Left = k * gridSize,
                                    Top = j * gridSize,
                                    Bottom = (j + 1) * gridSize ,
                                    Right = (k+1)*gridSize
                                };
                            }
                            else
                            {
                                clusteredData[key].ItemIndices.Add(i);
                            }
                        }
                    }
                }

                return clusteredData;
            });
        }

        /*
         * This class uses the Point Based clustering algorithm to sort data into clusters based on zoom level 
         * and is derived from this code is based on: 
         * http://bingmapsv7modules.codeplex.com/wikipage?title=Point%20Based%20Clustering
         * 
         * This algorithm sorts all data into clusters based on zoom level and the current map view. It takes 
         * the first item in the list of items and groups all nearby items into it's location. The next 
         * ungrouped item is then used to do the same until all the items have been alocated. This results in 
         * clusters always being the same at a specific zoom level which means better performance when panning. 
         * This algorithm also has a much lower chance of having overlapping clusters.
         * 
         * Tests have found that this clustering algorithm works well for up to 30,000 data points or less.
         */
#if WINDOWS_APP || WINDOWS_PHONE_APP
        private async Task CalculatePointClusters(List<Windows.Foundation.Point> pixels)
#elif WPF || WINDOWS_PHONE
        private async Task CalculatePointClusters(List<System.Windows.Point> pixels)
#endif
        {
            int maxX = (int)Math.Ceiling(_map.ActualWidth + ClusterRadius);
            int maxY = (int)Math.Ceiling(_map.ActualHeight + ClusterRadius);

            clusters = await Task.Run<ClusteredPoint[]>(() =>
            {
                var clusteredData = new List<ClusteredPoint>();

                if (_items != null && _items.Count > 0)
                {
                    double tileZoomRatio = 256 * Math.Pow(2, _currentZoomLevel);

#if WINDOWS_APP || WINDOWS_PHONE_APP
                    Windows.Foundation.Point pixel;
#elif WPF || WINDOWS_PHONE
                    System.Windows.Point pixel;
#endif

                    //Itirate through the data
                    for (int i = 0; i < _items.Count; i++)
                    {
                        var entity = _items[i];
                        pixel = pixels[i];

                        if (pixel.X < -ClusterRadius)
                        {
                            pixel.X += tileZoomRatio;
                        }
                        else if (pixel.X > maxX + ClusterRadius)
                        {
                            pixel.X -= tileZoomRatio;
                        }

                        //Check to see if the pin is within the bounds of the viewable map
                        if (pixel != null && pixel.X <= maxX && pixel.Y <= maxY && pixel.X >= -ClusterRadius && pixel.Y >= -ClusterRadius)
                        {
                            var cluster = (from c in clusteredData
                                           where (pixel.Y >= c.Top && pixel.Y <= c.Bottom &&
                                                 ((c.Left <= c.Right && pixel.X >= c.Left && pixel.X <= c.Right) ||
                                                 (c.Left >= c.Right && (pixel.X >= c.Left || pixel.X <= c.Right))))
                                           select c).FirstOrDefault();

                            //If entity is not in a cluster then it does not fit an existing cluster
                            if (cluster == null)//!IsInCluster)
                            {
                                cluster = new ClusteredPoint()
                                {
                                    Location = _allLocations[i],
                                    Left = pixel.X - ClusterRadius,
                                    Right = pixel.X + ClusterRadius,
                                    Top = pixel.Y - ClusterRadius,
                                    Bottom = pixel.Y + ClusterRadius,
                                    Zoom = _currentZoomLevel,
                                    ItemIndices = new List<int>() { i }
                                };

                                if (cluster.Left < 0)
                                {
                                    cluster.Left += tileZoomRatio;
                                }

                                if (cluster.Right > tileZoomRatio)
                                {
                                    cluster.Right -= tileZoomRatio;
                                }

                                clusteredData.Add(cluster);
                            }
                            else
                            {
                                cluster.ItemIndices.Add(i);
                            }
                        }
                    }
                }

                return clusteredData.ToArray();
            });
        }

        #endregion
    }
}
