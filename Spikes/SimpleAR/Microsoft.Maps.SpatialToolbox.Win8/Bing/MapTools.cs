using System;
using Microsoft.Maps.SpatialToolbox.IO;

#if WINDOWS_APP
using Bing.Maps;
using Windows.UI.Xaml.Input;
#elif WPF
using Microsoft.Maps.MapControl.WPF;
using System.Windows;
using System.Windows.Input;
#elif WINDOWS_PHONE
using Microsoft.Phone.Maps.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows;
#elif WINDOWS_PHONE_APP
using Windows.UI.Xaml.Controls.Maps;
using System.Collections.Generic;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
#endif

namespace Microsoft.Maps.SpatialToolbox.Bing
{
    /// <summary>
    /// A set of tools that make it easy to work with shapes in Bing Maps. 
    /// </summary>
    public static class MapTools
    {
        #region Public Methods

        /// <summary>
        /// Loads a SpatialDataSet to a specified MapLayer and MapShapeLayer in Bing Maps. 
        /// </summary>
        /// <param name="data">SpatialDataSet to add to map.</param>
        /// <param name="pinLayer">MapLayer used for rendering pushpins.</param>
        /// <param name="shapeLayer">Shape layer used to render polylines and polygons.</param>
        /// <param name="defaultStyle">Default styl eto use for shapes that do not have a style defined.</param>
        /// <param name="tapEvent">A tap event handler to be added to the shapes.</param>
#if WINDOWS_APP
        public static void LoadGeometries(SpatialDataSet data, MapLayer pinLayer, MapShapeLayer shapeLayer, ShapeStyle defaultStyle, TappedEventHandler tapEvent)
#elif WPF
        public static void LoadGeometries(SpatialDataSet data, MapLayer pinLayer, MapLayer shapeLayer, ShapeStyle defaultStyle, MouseButtonEventHandler tapEvent)
#elif WINDOWS_PHONE
        public static void LoadGeometries(SpatialDataSet data, MapLayer pinLayer, Collection<MapElement> shapeLayer, ShapeStyle defaultStyle, MouseButtonEventHandler tapEvent)
#elif WINDOWS_PHONE_APP
        public static void LoadGeometries(SpatialDataSet data, IList<DependencyObject> pinLayer, IList<MapElement> shapeLayer, ShapeStyle defaultStyle, TappedEventHandler tapEvent)
#endif
        {
            if (data != null && data.Geometries != null && data.Geometries.Count > 0)
            {
                ShapeStyle style;

                foreach (var g in data.Geometries)
                {
                    style = defaultStyle;

                    if (!string.IsNullOrEmpty(g.StyleKey)
                        && data.Styles != null
                        && data.Styles.ContainsKey(g.StyleKey))
                    {
                        style = data.Styles[g.StyleKey];
                    }

                    LoadGeometry(g, pinLayer, shapeLayer, style, tapEvent);
                }
            }
        }

        /// <summary>
        /// Loads a Geometry object to a specified MapLayer and MapShapeLayer in Bing Maps. 
        /// </summary>
        /// <param name="geometry">Geometry object to add to map.</param>
        /// <param name="pinLayer">MapLayer used for rendering pushpins.</param>
        /// <param name="shapeLayer">Shape layer used to render polylines and polygons.</param>
        /// <param name="defaultStyle">Default styl eto use for shapes that do not have a style defined.</param>
        /// <param name="tapEvent">A tap event handler to be added to the shapes.</param>
#if WINDOWS_APP
        public static void LoadGeometry(Geometry geometry, MapLayer pinLayer, MapShapeLayer shapeLayer, ShapeStyle style, TappedEventHandler tapEvent)
#elif WPF
        public static void LoadGeometry(Geometry geometry, MapLayer pinLayer, MapLayer shapeLayer, ShapeStyle style, MouseButtonEventHandler tapEvent)
#elif WINDOWS_PHONE
        public static void LoadGeometry(Geometry geometry, MapLayer pinLayer, Collection<MapElement> shapeLayer, ShapeStyle style, MouseButtonEventHandler tapEvent)
#elif WINDOWS_PHONE_APP
        public static void LoadGeometry(Geometry geometry, IList<DependencyObject> pinLayer, IList<MapElement> shapeLayer, ShapeStyle style, TappedEventHandler tapEvent)
#endif
        {
            if (geometry is Point)
            {
                var p = StyleTools.GenerateMapShape(geometry as Point, style);
                if (p != null)
                {
                    if (tapEvent != null)
                    {
                        #if WINDOWS_APP ||  WINDOWS_PHONE_APP
                        p.Tapped += tapEvent;
                        #elif WPF
                        p.MouseLeftButtonDown += tapEvent;
                        #elif WINDOWS_PHONE
                        (p.Content as FrameworkElement).MouseLeftButtonDown += tapEvent;
                        #endif
                    }

                    #if WINDOWS_PHONE || WINDOWS_PHONE_APP
                    pinLayer.Add(p);
                    #else
                    pinLayer.Children.Add(p);
                    #endif
                }
            }
            else if (geometry is LineString)
            {
                var l = StyleTools.GenerateMapShape(geometry as LineString, style);
                if (l != null)
                {
                    #if WINDOWS_APP
                    if (tapEvent != null)
                    {
                        l.Tapped += tapEvent;
                    }

                    shapeLayer.Shapes.Add(l);
                    #elif WPF
                    if (tapEvent != null)
                    {
                        l.MouseLeftButtonDown += tapEvent;
                    }

                    shapeLayer.Children.Add(l);
                    #elif WINDOWS_PHONE || WINDOWS_PHONE_APP
                    shapeLayer.Add(l);
                    #endif
                }
            }
            else if (geometry is Polygon)
            {
                var polys = StyleTools.GenerateMapShape(geometry as Polygon, style);
                if (polys != null)
                {
                    #if WINDOWS_PHONE || WINDOWS_PHONE_APP
                    shapeLayer.Add(polys);
                    #else
                    foreach (var p in polys)
                    {                     
                        #if WINDOWS_APP
                        if (tapEvent != null)
                        {
                            p.Tapped += tapEvent;
                        }

                        shapeLayer.Shapes.Add(p);
                        #elif WPF
                        if (tapEvent != null)
                        {
                            p.MouseLeftButtonDown += tapEvent;
                        }

                        shapeLayer.Children.Add(p);
                        #endif
                    }
                    #endif
                }
            }
            else if (geometry is MultiPoint)
            {
                //Break MultiPoint objects into individual pushpins
                var mp = geometry as MultiPoint;

                foreach (var p in mp)
                {
                    if (p.Metadata == null || !p.Metadata.HasMetadata())
                    {
                        p.Metadata = mp.Metadata;
                    }

                    var pin = StyleTools.GenerateMapShape(p, style);
                    if (pin != null)
                    {
                        if (tapEvent != null)
                        {
                            #if WINDOWS_APP ||  WINDOWS_PHONE_APP
                            pin.Tapped += tapEvent;
                            #elif WPF
                            pin.MouseLeftButtonDown += tapEvent;
                            #elif WINDOWS_PHONE
                            (pin.Content as FrameworkElement).MouseLeftButtonDown += tapEvent;
                            #endif
                        }

                        #if WINDOWS_PHONE || WINDOWS_PHONE_APP
                        pinLayer.Add(pin);
                        #else
                        pinLayer.Children.Add(pin);
                        #endif                        
                    }
                }
            }
            else if (geometry is MultiLineString)
            {
                //Break MultiPoint bjects into individual pushpins
                var ml = geometry as MultiLineString;

                foreach (var l in ml)
                {
                    if (l.Metadata == null || !l.Metadata.HasMetadata())
                    {
                        l.Metadata = ml.Metadata;
                    }
                    LoadGeometry(l, pinLayer, shapeLayer, style, tapEvent);
                }
            }
            else if (geometry is MultiPolygon)
            {
                //Break MultiPoint bjects into individual pushpins
                var mp = geometry as MultiPolygon;

                foreach (var p in mp)
                {
                    if (p.Metadata == null || !p.Metadata.HasMetadata())
                    {
                        p.Metadata = mp.Metadata;
                    }
                    LoadGeometry(p, pinLayer, shapeLayer, style, tapEvent);
                }
            }
            else if (geometry is GeometryCollection)
            {
                //Break MultiPoint bjects into individual pushpins
                var gc = geometry as GeometryCollection;

                foreach (var g in gc)
                {
                    if (g.Metadata == null || !g.Metadata.HasMetadata())
                    {
                        g.Metadata = gc.Metadata;
                    }
                    LoadGeometry(g, pinLayer, shapeLayer, style, tapEvent);
                }
            }
        }

        /// <summary>
        /// Loops through all shapes in a MapLayer and a MapShapeLayer and counts the number of shapes and coordinates.
        /// </summary>
        /// <param name="pinLayer">MapLayer to analyize.</param>
        /// <param name="polyLayer">MapShapeLayer to anaylize.</param>
        /// <param name="pinCount">Number of pins in the MapLayer.</param>
        /// <param name="lineCount">Number of polylines in the MapShapeLayer.</param>
        /// <param name="polygonCount">Number of polygons in the MapShapeLayer.</param>
        /// <param name="coordCount">Number of coordinates in all the layers.</param>
#if WINDOWS_APP
        public static void AnalizeLayer(MapLayer pinLayer, MapShapeLayer polyLayer, out int pinCount, out int lineCount, out int polygonCount, out int coordCount)
#elif WPF
        public static void AnalizeLayer(MapLayer pinLayer, MapLayer polyLayer, out int pinCount, out int lineCount, out int polygonCount, out int coordCount)
#elif WINDOWS_PHONE
        public static void AnalizeLayer(MapLayer pinLayer, Collection<MapElement> polyLayer, out int pinCount, out int lineCount, out int polygonCount, out int coordCount)
#elif WINDOWS_PHONE_APP
        public static void AnalizeLayer(IList<UIElement> pinLayer, IList<MapElement> polyLayer, out int pinCount, out int lineCount, out int polygonCount, out int coordCount)
#endif
        {
            pinCount = 0;
            lineCount = 0;
            polygonCount = 0;
            coordCount = 0;

            if (pinLayer != null)
            {
                pinCount = CountPins(pinLayer);
                coordCount += pinCount;
            }

            if (polyLayer != null)
            {
#if WINDOWS_PHONE
                foreach (var c in polyLayer)
                {
                    if (c is MapPolyline)
                    {
                        lineCount++;
                        coordCount += (c as MapPolyline).Path.Count;
                    }
                    else if (c is MapPolygon)
                    {
                        polygonCount++;
                        coordCount += (c as MapPolygon).Path.Count;
                    }
                }
#elif WINDOWS_PHONE_APP
                foreach (var c in polyLayer)
                {
                    if (c is MapPolyline)
                    {
                        lineCount++;
                        coordCount += (c as MapPolyline).Path.Positions.Count;
                    }
                    else if (c is MapPolygon)
                    {
                        polygonCount++;
                        coordCount += (c as MapPolygon).Path.Positions.Count;
                    }
                }
#else
                #if WINDOWS_APP
                foreach (var c in polyLayer.Shapes)
                #elif WPF
                foreach (var c in polyLayer.Children)
                #endif
                {
                    if (c is MapPolyline)
                    {
                        lineCount++;
                        coordCount += (c as MapPolyline).Locations.Count;
                    }
                    else if (c is MapPolygon)
                    {
                        polygonCount++;
                        coordCount += (c as MapPolygon).Locations.Count;
                    }
                }
#endif
            }
        }

        #endregion

        #region Private Methods

#if WINDOWS_PHONE_APP
        private static int CountPins(IList<UIElement> pinLayer)
#else
        private static int CountPins(MapLayer pinLayer)
#endif
        {
            int cnt = 0;
#if WINDOWS_PHONE || WINDOWS_PHONE_APP
            cnt = pinLayer.Count;
#else
            foreach (var c in pinLayer.Children)
            {
                if (c is MapLayer)
                {
                    cnt += CountPins(c as MapLayer);
                }
                else
                {
                    cnt++;
                }
            }
#endif
            return cnt;
        }

        #endregion
    }
}
