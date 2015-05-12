using System;
using System.Collections.ObjectModel;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;

namespace AugmentedSzczecin.PointBasedClustering
{
    public abstract class BaseClusteredLayer : Panel
    {
        #region Internal Properties

        internal MapControl Map;
        internal MapTileSource ParentLayer;
        internal MapTileSource BaseLayer;
        internal int CurrentZoomLevel;
        internal Geopoint Center;

        internal const int MaxZoomLevel = 21;

        internal ItemLocationCollection _items;
        internal ObservableCollection<Geopoint> AllLocations;

        #endregion

        #region Constructor

        public BaseClusteredLayer()
        {
            _items = new ItemLocationCollection();
            _items.CollectionChanged += () =>
            {
                AllLocations.Clear();

                foreach (var i in _items)
                {
                    AllLocations.Add(i.Location);
                }

                Cluster();
            };

            AllLocations = new ObservableCollection<Geopoint>();

            this.Loaded += (s, e) =>
            {
                DependencyObject parent = this;
                while (parent != null)
                {
                    parent = VisualTreeHelper.GetParent(parent);

                    if (parent != null)
                    {
                        ParentLayer = parent as MapTileSource;
                    }
                }

                if (parent != null)
                {
                    Map = Parent as MapControl;
                    Init();
                }
            };

            this.Unloaded += (s, e) =>
            {
                if (Map != null)
                {
                    Map.CenterChanged -= _map_ViewChangeEnded;
                    Map.SizeChanged -= _map_SizeChanged;
                }
            };
        }

        #endregion

        #region Public Properties

        public ItemLocationCollection Items
        {
            get { return _items; }
        }

        private int clusterRadius = 45;

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

        #endregion

        #region Internal Methods

        internal UIElement GetPin(object item)
        {
            //todo !!!
            //if (CreateItemPushpin != null)
            //{
            //    return CreateItemPushpin(item);
            //}

            return null;
        }

        internal UIElement GetClustedPin(ClusteredPoint clusterInfo)
        {
            //todo !!!
            //if (CreateItemPushpin != null)
            //{
            //    return CreateClusteredItemPushpin(clusterInfo);
            //}

            return null;
        }

        #endregion

        #region Abstract Methods

        internal abstract void Cluster();
        internal abstract void Render();

        #endregion

        #region Private Methods

        private void Init()
        {
            var baseLayer = new MapTileSource();
            Map.TileSources.Add(baseLayer);

            CurrentZoomLevel = (int)Math.Round(Map.ZoomLevel);
            Map.CenterChanged += _map_ViewChangeEnded;
            Map.SizeChanged += _map_SizeChanged;

            Cluster();
        }

        private void _map_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Cluster();
        }

        private void _map_ViewChangeEnded(MapControl sender, object args)
        {
            int zoom = (int)Math.Round(Map.ZoomLevel);

            if ((Center != Map.Center || CurrentZoomLevel != zoom))
            {
                CurrentZoomLevel = zoom;
                Center = Map.Center;
                Cluster();
            }
        }

        #endregion
    }
}
