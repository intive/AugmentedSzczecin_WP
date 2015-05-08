using Microsoft.Phone.Maps.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Microsoft.Maps.SpatialToolbox.Bing
{
    public static class StyleTools
    {
        #region Public Properties

        public static Brush DefaultIconColorBrush = new SolidColorBrush(Colors.Blue);
        public static Color DefaultPolylineColor = Color.FromArgb(150, 0, 255, 0);
        public static Color DefaultPolygonFillColor = Color.FromArgb(150, 0, 0, 255);
        public static double DefaultPolylineThickness = 4;

        #endregion

        #region Apply Style Methods

        /// <summary>
        /// Apply a single style to all shapes in a MapLayer.
        /// </summary>
        /// <param name="pinLayer">MapLayer to apply style to.</param>        
        /// <param name="style">Style to apply to layers.</param>
        public static void ApplyStyle(MapLayer pinLayer, ShapeStyle style)
        {
            if (pinLayer != null)
            {
                foreach (var p in pinLayer)
                {
                    ApplyStyle(p, style);
                }
            }
        }

        /// <summary>
        /// Apply a single style to all shapes in a Collection<MapElement>.
        /// </summary>
        /// <param name="polyLayer">Collection<MapElement> to apply style to.</param>
        /// <param name="style">Style to apply to layers.</param>
        public static void ApplyStyle(Collection<MapElement> polyLayer, ShapeStyle style)
        {
            if (polyLayer != null)
            {
                foreach (var p in polyLayer)
                {
                    ApplyStyle(p, style);
                }
            }
        }

        /// <summary>
        /// Apply a style to a MapOverlay.
        /// </summary>
        /// <param name="element">MapOverlay to apply style to.</param>
        /// <param name="style">Style to apply to MapOverlay.</param>
        public static void ApplyStyle(MapOverlay element, ShapeStyle style)
        {
            if (element != null && style != null)
            {
                double scale = 1;

                if (!double.IsNaN(style.IconScale) && style.IconScale != 0 && style.IconScale != 1)
                {
                    scale = style.IconScale;
                }

                if (style.IconUrl != null)
                {
                    var img = new Image()
                    {
                        Source = new BitmapImage(style.IconUrl)
                    };

                    //If the image fails then load a default pushpin
                    img.ImageFailed += (s, a) =>
                    {
                        var c = DefaultIconColorBrush;
                        if (style.IconColor.HasValue)
                        {
                            c = new SolidColorBrush(style.IconColor.Value.ToColor());
                        }
                        element.Content = new Ellipse()
                        {
                            Width = 30,
                            Height = 30,
                            Stroke = new SolidColorBrush(Colors.White),
                            StrokeThickness = 5,
                            Fill = c,
                            Margin = new System.Windows.Thickness(-20,-20, 0, 0)
                        };
                    };

                    TransformGroup transforms = new TransformGroup();

                    if (scale != 1)
                    {
                        transforms.Children.Add(new ScaleTransform()
                        {
                            ScaleX = scale,
                            ScaleY = scale
                        });
                    }

                    if (!double.IsNaN(style.IconHeading) && style.IconHeading != 0)
                    {
                        transforms.Children.Add(new RotateTransform()
                        {
                            Angle = style.IconHeading
                        });
                    }

                    //TODO: Add support for Icon anchor
                    //if (style.Icon.Hotspot != null
                    //        && style.Icon.Hotspot.X.HasValue
                    //        && style.Icon.Hotspot.Y.HasValue
                    //        && style.Icon.Hotspot.XUnits.HasValue 
                    //        && style.Icon.Hotspot.XUnits.Value == Unit.Pixel)
                    //    {
                    //        transforms.Children.Add(new TranslateTransform()
                    //        {
                    //            X = style.Icon.Hotspot.X.Value,
                    //            Y = style.Icon.Hotspot.Y.Value
                    //        });
                    //    }
                    //    else
                    //    {
                    //If no hotspot is defined then center the image over the coordinate

                    img.ImageOpened += (s, a) =>
                    {
                        //Center the image
                        var i = s as Image;
                        var src = i.Source as BitmapSource;
                        i.Width = src.PixelWidth;
                        i.Height = src.PixelHeight;
                        i.Margin = new System.Windows.Thickness(-src.PixelWidth * scale / 2, -src.PixelHeight * scale / 2, 0, 0);
                    };

                    if (transforms.Children.Count > 0)
                    {
                        img.RenderTransform = transforms;
                    }

                    element.Content = img;
                }
                else
                {
                    var c = DefaultIconColorBrush;
                    if (style.IconColor.HasValue)
                    {
                        c = new SolidColorBrush(style.IconColor.Value.ToColor());
                    }
                    element.Content = new Ellipse()
                    {
                        Width = 30,
                        Height = 30,
                        Stroke = new SolidColorBrush(Colors.White),
                        StrokeThickness = 5,
                        Fill = c,
                        Margin = new System.Windows.Thickness(-20, -20, 0, 0)
                    };
                }
            }
        }

        /// <summary>
        /// Apply a style to a MapElement (MapPolygon, MapPolyline).
        /// </summary>
        /// <param name="element">MapElement to apply style to.</param>
        /// <param name="style">Style to apply to MapElement.</param>
        public static void ApplyStyle(MapElement polyShape, ShapeStyle style)
        {
            if (polyShape != null && style != null)
            {
                if (polyShape is MapPolyline)
                {
                    var polyline = polyShape as MapPolyline;

                    if (style.StrokeColor.HasValue)
                    {
                        polyline.StrokeColor = style.StrokeColor.Value.ToColor();
                    }
                    else
                    {
                        polyline.StrokeColor =  DefaultPolylineColor;
                    }

                    if (!double.IsNaN(style.StrokeThickness) && style.StrokeThickness > 0)
                    {
                        polyline.StrokeThickness = style.StrokeThickness;
                    }
                    else
                    {
                        polyline.StrokeThickness = DefaultPolylineThickness;
                    }
                }
                else if (polyShape is MapPolygon)
                {
                    var polygon = polyShape as MapPolygon;

                    if (style.FillColor.HasValue)
                    {
                        polygon.FillColor = style.FillColor.Value.ToColor();
                    }
                    else
                    {
                        polygon.FillColor =  DefaultPolygonFillColor;
                    }

                    if (style.StrokeColor.HasValue)
                    {
                        polygon.StrokeColor = style.StrokeColor.Value.ToColor();
                    }
                    else
                    {
                        polygon.StrokeColor =  DefaultPolylineColor;
                    }

                    if (!double.IsNaN(style.StrokeThickness) && style.StrokeThickness > 0)
                    {
                        polygon.StrokeThickness = style.StrokeThickness;
                    }
                    else
                    {
                        polygon.StrokeThickness = DefaultPolylineThickness;
                    }
                }
            }
        }

        #endregion

        #region Generate Shape Methods

        /// <summary>
        /// Converts a Point into a Bing Maps shape with a defined style.
        /// </summary>
        /// <param name="point">Point to convert into a Bing Maps Shape.</param>
        /// <param name="style">Style to use to render shape</param>
        /// <returns>MapOverlay to use to represent Point on Bing Maps.</returns>
        public static MapOverlay GenerateMapShape(Point point, ShapeStyle style)
        {
            if (!double.IsNaN(point.Coordinate.Latitude) && !double.IsNaN(point.Coordinate.Longitude))
            {
                var pushpin = new MapOverlay()
                {
                    GeoCoordinate = point.Coordinate.ToBMGeometry()
                };

                ApplyStyle(pushpin, style);

                (pushpin.Content as FrameworkElement).Tag = point.Metadata;

                return pushpin;
            }

            return null;
        }

        /// <summary>
        /// Converts a LineString into a Bing Maps MapPolyline with a defined style.
        /// </summary>
        /// <param name="line">LineString to convert into a MapPolyline.</param>
        /// <param name="style">Style to use to render shape</param>
        /// <returns>MapPolyine of LineString.</returns>
        public static MapPolyline GenerateMapShape(LineString line, ShapeStyle style)
        {
            var l = GenerateMapShape(line.Vertices, style);

            //Add metadata as to Tag property if added in Bing Maps
            if (l != null)
            {
                l.SetValue(MapElementExt.TagProperty, line.Metadata);
            }

            return l;
        }

        /// <summary>
        /// Converts a CoordinateCollection into a Bing Maps MapPolyline with a defined style.
        /// </summary>
        /// <param name="coordinates">CoordinateCollection to use to generate MapPolyline.</param>
        /// <param name="style">Style to use to render shape</param>
        /// <returns>MapPolyline of the CoordinateCollection.</returns>
        public static MapPolyline GenerateMapShape(CoordinateCollection coordinates, ShapeStyle style)
        {
            if (coordinates != null && coordinates.Count >= 2)
            {
                var polyline = new MapPolyline()
                {
                    Path = coordinates.ToBMGeometry()
                };

                if (style != null)
                {
                    if (style.StrokeThickness == 0)
                    {
                        return null;
                    }
                }

                ApplyStyle(polyline, style);

                return polyline;
            }

            return null;
        }

        /// <summary>
        /// Converts a Polygon into a Bing Maps MapPolygon with a defined style.
        /// </summary>
        /// <param name="polygon">Polygon to convert to a MapPolygon.</param>
        /// <param name="style">Style to use to render shape</param>
        /// <returns>A MapElement that represent the Polygon.</returns>

        public static MapElement GenerateMapShape(Polygon polygon, ShapeStyle style)
        {
            if (polygon != null && polygon.ExteriorRing != null && polygon.ExteriorRing.Count >= 3)
            {
                var shapes = new Collection<MapElement>();

                var locs = polygon.ExteriorRing;

                //Ensure polygon is closed.
                locs.Add(locs[0]);

                //Outline inner rings.
                foreach (var i in polygon.InteriorRings)
                {
                    //Ensure inner ring is closed.
                    i.Add(i[0]);

                    //Add inner rings to main list of locations
                    locs.AddRange(i);

                    //Loop back to starting point.
                    locs.Add(locs[0]);
                }

                var poly = new MapPolygon();
                foreach (var p in locs.ToBMGeometry())
                {
                    poly.Path.Add(p);
                }

                //Set polygon style
                ApplyStyle(poly, style);

                //Add metadata as to Tag property if added in Bing Maps
                if (poly != null)
                {
                    poly.SetValue(MapElementExt.TagProperty, polygon.Metadata);
                }

                return poly;
            }

            return null;
        }

        #endregion
    }
}