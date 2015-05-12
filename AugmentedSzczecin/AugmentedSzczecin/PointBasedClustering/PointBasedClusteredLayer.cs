using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI.Xaml;
using Newtonsoft.Json;

namespace AugmentedSzczecin.PointBasedClustering
{
    public class PointBasedClusteredLayer : BaseClusteredLayer
    {
        #region Private Properties

        private IList<ClusteredPoint> _clusteredData;

        #endregion

        #region Constructor

        //todo: czy dobrze
        public PointBasedClusteredLayer()
            : base()
        {

        }

        #endregion

        #region Private Methods

        internal override async void Cluster()
        {
            if (Map != null && CurrentZoomLevel <= MaxZoomLevel)
            {
                var pixels = new List<Point>();

                //todo: Map.TryLocationsToPixels(AllLocations, pixels);

                int maxX = (int)Math.Ceiling(Map.ActualWidth + ClusterRadius);
                int maxY = (int)Math.Ceiling(Map.ActualHeight + ClusterRadius);

                await Task.Run(() =>
                {
                    var clusteredData = new List<ClusteredPoint>();

                    if (Items != null && Items.Count > 0)
                    {
                        double tileZoomRatio = 256 * Math.Pow(2, CurrentZoomLevel);
                        Point pixel;
                        bool IsInCluster;

                        for (int i = 0; i < Items.Count; i++)
                        {
                            var entity = Items[i];
                            pixel = pixels[i];
                            IsInCluster = false;

                            if (pixel.X < -ClusterRadius)
                            {
                                pixel.X += tileZoomRatio;
                            }
                            else if (pixel.X > maxX + ClusterRadius)
                            {
                                pixel.X -= tileZoomRatio;
                            }

                            if (pixel != null && pixel.X <= maxX && pixel.Y <= maxY && pixel.X >= -ClusterRadius &&
                                pixel.Y >= -ClusterRadius)
                            {
                                foreach (var cluster in _clusteredData)
                                {
                                    if (pixel.Y >= cluster.Top && pixel.Y <= cluster.Bottom &&
                                        ((cluster.Left <= cluster.Right && pixel.X >= cluster.Left &&
                                          pixel.X <= cluster.Right) ||
                                         (cluster.Left >= cluster.Right &&
                                          (pixel.X >= cluster.Left || pixel.X <= cluster.Right))))
                                    {
                                        cluster.ItemIndices.Add(i);
                                        IsInCluster = true;
                                        break;
                                    }
                                }

                                if (!IsInCluster)
                                {
                                    ClusteredPoint cluster = new ClusteredPoint()
                                    {
                                        Location = AllLocations[i],
                                        Left = pixel.X - ClusterRadius,
                                        Right = pixel.X + ClusterRadius,
                                        Top = pixel.Y - ClusterRadius,
                                        Bottom = pixel.Y + ClusterRadius,
                                        Zoom = CurrentZoomLevel,
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
                            }
                        }
                    }

                    _clusteredData = clusteredData;
                });
            }

            Render();
        }



        internal override void Render()
        {
            if (Map != null)
            {

                //todo: BaseLayer.Children.Clear(); 

                UIElement pin;

                foreach (var c in _clusteredData)
                {
                    if (c != null)
                    {
                        if (c.ItemIndices.Count == 1)
                        {
                            var item = _items[c.ItemIndices[0]];
                            pin = GetPin(item);
                        }
                        else
                        {
                            pin = GetClustedPin(c);
                        }

                        //todo: Map.SetPosition(pin, c.Location)
                        //todo: BaseLayer.Children.Add(pin);
                    }
                }
            }
        }

        #endregion
    }
}
