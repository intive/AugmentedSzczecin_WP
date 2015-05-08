using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Devices.Geolocation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

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
        /// <param name="layer">MapLayer to apply style to.</param>        
        /// <param name="style">Style to apply to layers.</param>
        public static void ApplyStyle(IList<MapElement> layer, ShapeStyle style)
        {
            if (layer != null)
            {
                foreach (var p in layer)
                {
                    ApplyStyle(p, style);
                }
            }
        }

        /// <summary>
        /// Apply a style to a UIElement.
        /// </summary>
        /// <param name="element">UIElement to apply style to.</param>
        /// <param name="style">Style to apply to UIElement.</param>
        public static void ApplyStyle(UIElement element, ShapeStyle style)
        {
            if (element != null && style != null && element is Canvas)
            {
                var canvas = element as Canvas;
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
                        canvas.Children.Clear();
                        canvas.Width = 30;
                        canvas.Height = 30;
                        canvas.Margin = new Thickness(-20, -20, 0, 0);
                        canvas.Children.Add(new Ellipse()
                        {
                            Width = 30,
                            Height = 30,
                            Stroke = new SolidColorBrush(Colors.White),
                            StrokeThickness = 5,
                            Fill = c
                        });
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
                        canvas.Width = src.PixelWidth;
                        canvas.Height = src.PixelHeight;
                        canvas.Margin = new Thickness(-src.PixelWidth * scale / 2, -src.PixelHeight * scale / 2, 0, 0);
                    };

                    if (transforms.Children.Count > 0)
                    {
                        img.RenderTransform = transforms;
                    }

                    canvas.Children.Add(img);
                }
                else
                {
                    var c = DefaultIconColorBrush;
                    if (style.IconColor.HasValue)
                    {
                        c = new SolidColorBrush(style.IconColor.Value.ToColor());
                    }
                    canvas.Width = 30;
                    canvas.Height = 30;
                    canvas.Margin = new Thickness(-20, -20, 0, 0);
                    canvas.Children.Add(new Ellipse()
                    {
                        Width = 30,
                        Height = 30,
                        Stroke = new SolidColorBrush(Colors.White),
                        StrokeThickness = 5,
                        Fill = c
                    });
                }
            }
        }

        /// <summary>
        /// Apply a style to a MapElement (MapIcon, MapPolygon, MapPolyline).
        /// </summary>
        /// <param name="element">MapElement to apply style to.</param>
        /// <param name="style">Style to apply to MapElement.</param>
        public static void ApplyStyle(MapElement element, ShapeStyle style)
        {            
            if (element != null && style != null)
            {
                if (element is MapIcon)
                {
                    var pushpin = element as MapIcon;

                    if (style.IconUrl != null)
                    {
                        pushpin.Image = RandomAccessStreamReference.CreateFromUri(style.IconUrl);
                    }

                    //TODO: Add additional customizations to MapIcon
                }
                else if (element is MapPolyline)
                {
                    var polyline = element as MapPolyline;

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
                else if (element is MapPolygon)
                {
                    var polygon = element as MapPolygon;

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
        public static UIElement GenerateMapShape(Point point, ShapeStyle style)
        {
            if (!double.IsNaN(point.Coordinate.Latitude) && !double.IsNaN(point.Coordinate.Longitude))
            {
                var pushpin = new Canvas();

                ApplyStyle(pushpin, style);

                pushpin.SetValue(MapElementExt.TagProperty, point.Metadata);
                MapControl.SetLocation(pushpin, new Geopoint(point.Coordinate.ToBMGeometry()));

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

                var poly = new MapPolygon()
                {
                    Path = locs.ToBMGeometry()
                };

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