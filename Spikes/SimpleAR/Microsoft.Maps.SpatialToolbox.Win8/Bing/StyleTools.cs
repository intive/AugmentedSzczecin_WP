using Microsoft.Maps.SpatialToolbox.IO;
using System.Collections.Generic;

#if WINDOWS_APP
using Bing.Maps;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
#elif WPF
using Microsoft.Maps.MapControl.WPF;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif

namespace Microsoft.Maps.SpatialToolbox.Bing
{
    /// <summary>
    /// A set of tools the apply styles to Bing Maps shapes.
    /// </summary>
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
                foreach (var p in pinLayer.Children)
                {
                    #if WINDOWS_APP
                    ApplyStyle(p as FrameworkElement, style);
                    #elif WPF
                    if (p is MapShapeBase)
                    {
                        ApplyStyle(p as MapShapeBase, style);
                    }
                    else
                    {
                        ApplyStyle(p as FrameworkElement, style);
                    }
                    #endif
                }
            }
        }

#if WINDOWS_APP
        /// <summary>
        /// Apply a single style to all shapes in a MapShapeLayer.
        /// </summary>
        /// <param name="polyLayer">MapShapeLayer to apply style to.</param>
        /// <param name="style">Style to apply to layers.</param>
        public static void ApplyStyle(MapShapeLayer polyLayer, ShapeStyle style)
        {
            if (polyLayer != null)
            {
                foreach (var p in polyLayer.Shapes)
                {
                    ApplyStyle(p, style);
                }
            }
        }

        /// <summary>
        /// Apply a single style to all shapes in a MapLayer and a MapShapeLayer.
        /// </summary>
        /// <param name="pinLayer">MapLayer to apply style to.</param>
        /// <param name="polyLayer">MapShapeLayer to apply style to.</param>
        /// <param name="style">Style to apply to layers.</param>
        public static void ApplyStyle(MapLayer pinLayer, MapShapeLayer polyLayer, ShapeStyle style)
        {
            if (pinLayer != null)
            {
                foreach (var p in pinLayer.Children)
                {
                    ApplyStyle(p as FrameworkElement, style);
                }
            }

            if (polyLayer != null)
            {
                foreach (var p in polyLayer.Shapes)
                {
                    ApplyStyle(p, style);
                }
            }
        }
#endif

        /// <summary>
        /// Apply a style to a FrameworkElement.
        /// </summary>
        /// <param name="element">FrameworkElement to apply style to.</param>
        /// <param name="style">Style to apply to FrameworkElement.</param>
        public static void ApplyStyle(FrameworkElement element, ShapeStyle style)
        {
            if (element != null && style != null)
            {
                double scale = 1;

                if (!double.IsNaN(style.IconScale) && style.IconScale != 0 && style.IconScale != 1)
                {
                    scale = style.IconScale;
                }

                if (element is Pushpin)
                {
                    var pin = element as Pushpin;

                    var c = DefaultIconColorBrush;
                    if (style.IconColor.HasValue)
                    {
                        c = new SolidColorBrush(style.IconColor.Value.ToColor());
                    }

                    pin.Background = c;
                }
                else if (element is StackPanel)
                {
                    var panel = element as StackPanel;

                    if (style.IconUrl != null)
                    {
                        panel.Children.Clear();

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

                        var img = new Image();
                        
                        #if WPF
                        var bmp = new BitmapImage();
                        bmp.CreateOptions = BitmapCreateOptions.None;
                        bmp.BeginInit();
                        bmp.DownloadCompleted += (s, a) =>
                        {
                            var b = s as BitmapImage;

                            //Center the image
                            img.Width = b.PixelWidth;
                            img.Height = b.PixelHeight;
                            panel.Margin = new Thickness(-b.PixelWidth * scale / 2, -b.PixelHeight * scale / 2, 0, 0);
                        };
                        bmp.UriSource = style.IconUrl;
                        bmp.EndInit();
                        img.Source = bmp;

                        if (!bmp.IsDownloading)
                        {
                            //Center the image
                            img.Width = bmp.PixelWidth;
                            img.Height = bmp.PixelHeight;
                            panel.Margin = new Thickness(-bmp.PixelWidth * scale / 2, -bmp.PixelHeight * scale / 2, 0, 0);
                        }
                        #else
                        img.Source = new BitmapImage(style.IconUrl);
                        #endif

                        //If the image fails then load a default pushpin
                        img.ImageFailed += (s, a) =>
                        {
                            panel.Children.Clear();

                            var c = DefaultIconColorBrush;
                            if (style.IconColor.HasValue)
                            {
                                c = new SolidColorBrush(style.IconColor.Value.ToColor());
                            }
                            panel.Children.Add(new Pushpin() { Background = c });
                        };
                        
                        #if WINDOWS_APP
                        img.ImageOpened += (s, a) =>
                        {
                            //Center the image
                            var i = s as Image;
                            var src = i.Source as BitmapSource;
                            i.Width = src.PixelWidth;
                            i.Height = src.PixelHeight;
                            panel.Margin = new Windows.UI.Xaml.Thickness(-src.PixelWidth * scale / 2, -src.PixelHeight * scale / 2, 0, 0);
                        };
                        #endif

                        if (transforms.Children.Count > 0)
                        {
                            img.RenderTransform = transforms;
                        }

                        panel.Children.Add(img);
                    }
                    else
                    {
                        panel.Children.Clear();

                        var c = DefaultIconColorBrush;
                        if (style.IconColor.HasValue)
                        {
                            c = new SolidColorBrush(style.IconColor.Value.ToColor());
                        }

                        var pin = new Pushpin() { Background = c };
                        pin.Margin = new Thickness(-pin.Width / 2, -pin.Height, 0, 0);

                        panel.Children.Add(pin);
                    }
                }
            }
        }

#if WINDOWS_APP
        /// <summary>
        /// Apply a style to a MapShape (MapPolygon, MapPolyline).
        /// </summary>
        /// <param name="element">MapShape to apply style to.</param>
        /// <param name="style">Style to apply to MapShape.</param>
        public static void ApplyStyle(MapShape polyShape, ShapeStyle style)
        {
            if (polyShape != null && style != null)
            {
                if (polyShape is MapPolyline)
                {
                    var polyline = polyShape as MapPolyline;

                    if (style.StrokeColor.HasValue)
                    {
                        polyline.Color = style.StrokeColor.Value.ToColor();
                    }
                    else
                    {
                        polyline.Color = DefaultPolylineColor;
                    }

                    if (!double.IsNaN(style.StrokeThickness) && style.StrokeThickness > 0)
                    {
                        polyline.Width = style.StrokeThickness;
                    }
                    else
                    {
                        polyline.Width = DefaultPolylineThickness;
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
                        polygon.FillColor = DefaultPolygonFillColor;
                    }
                }
            }
        }

#elif WPF
        /// <summary>
        /// Apply a style to a MapShapeBase (MapPolygon, MapPolyline).
        /// </summary>
        /// <param name="element">MapShapeBase to apply style to.</param>
        /// <param name="style">Style to apply to MapShape.</param>
        public static void ApplyStyle(MapShapeBase polyShape, ShapeStyle style)
        {
            if (polyShape != null && style != null)
            {
                polyShape.StrokeLineJoin = PenLineJoin.Round;
                polyShape.StrokeStartLineCap = PenLineCap.Round;
                polyShape.StrokeEndLineCap = PenLineCap.Round;

                if (polyShape is MapPolyline)
                {
                    var polyline = polyShape as MapPolyline;

                    if (style.StrokeColor.HasValue)
                    {
                        polyline.Stroke = new SolidColorBrush(style.StrokeColor.Value.ToColor());
                    }
                    else
                    {
                        polyline.Stroke =  new SolidColorBrush(DefaultPolylineColor);
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
                        polygon.Fill = new SolidColorBrush(style.FillColor.Value.ToColor());
                    }
                    else
                    {
                        polygon.Fill =  new SolidColorBrush(DefaultPolygonFillColor);
                    }

                    if (style.StrokeColor.HasValue)
                    {
                        polygon.Stroke = new SolidColorBrush(style.StrokeColor.Value.ToColor());
                    }
                    else
                    {
                        polygon.Stroke = new SolidColorBrush(DefaultPolylineColor);
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
#endif

        #endregion

        #region Generate Shape Methods

        /// <summary>
        /// Converts a Point into a Bing Maps shape with a defined style.
        /// </summary>
        /// <param name="point">Point to convert into a Bing Maps Shape.</param>
        /// <param name="style">Style to use to render shape</param>
        /// <returns>FrameworkElement to use to represent Point on Bing Maps.</returns>
        public static FrameworkElement GenerateMapShape(Point point, ShapeStyle style)
        {
            if (!double.IsNaN(point.Coordinate.Latitude) && !double.IsNaN(point.Coordinate.Longitude))
            {
                FrameworkElement elm;

                if (style == null || (!style.IconColor.HasValue && style.IconHeading == 0 && style.IconScale == 1 && style.IconUrl == null))
                {
                    elm = new Pushpin() { Background = DefaultIconColorBrush };
                    elm.Margin = new Thickness(-elm.Width / 2, -elm.Height, 0, 0);
                }
                else
                {
                    elm = new StackPanel();
                    ApplyStyle(elm, style);
                }
                
                elm.Tag = point.Metadata;

                MapLayer.SetPosition(elm, point.Coordinate.ToBMGeometry());
                return elm;
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
#if WINDOWS_APP
                l.SetValue(MapShapeExt.TagProperty, line.Metadata);
#elif WPF
                l.Tag = line.Metadata;
#endif
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
                    Locations = coordinates.ToBMGeometry()
                };

                if (style != null && style.StrokeThickness == 0)
                {
                    return null;
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
        /// <returns>A List of MapShapes that represent the Polygon. MapPolylines are returned to draw Polygon outline.</returns>

#if WPF
        public static List<MapShapeBase> GenerateMapShape(Polygon polygon, ShapeStyle style)
        {
            if (polygon != null && polygon.ExteriorRing != null && polygon.ExteriorRing.Count >= 3)
            {
                var shapes = new List<MapShapeBase>();
                var locs = polygon.ExteriorRing;

                //Ensure polygon is closed.
                locs.Add(locs[0]);

                //This uses a workaround to create outline of polygon
                if (style == null || style.OutlinePolygon)
                {
                    var l = GenerateMapShape(locs, style);

                    if (l != null)
                    {
                        shapes.Add(l);
                    }
                }

                //Outline inner rings.
                foreach (var i in polygon.InteriorRings)
                {
                    //Ensure inner ring is closed.
                    i.Add(i[0]);

                    var l = GenerateMapShape(i, style);

                    if (l != null)
                    {
                        if (style == null || style.OutlinePolygon)
                        {
                            shapes.Add(l);
                        }

                        //Add inner rings to main list of locations
                        locs.AddRange(i);

                        //Loop back to starting point.
                        locs.Add(locs[0]);
                    }
                }

                var poly = new MapPolygon()
                {
                    Locations = locs.ToBMGeometry()
                };

                //Set polygon style
                if (polygon.InteriorRings.Count > 0)
                {
                    //Make stroke transparent as outline being done using polylines
                    var s = style.Copy();
                    s.StrokeColor = new StyleColor() { A = 0 };
                    ApplyStyle(poly, s);
                }
                else
                {
                    ApplyStyle(poly, style);
                }

                //Add metadata as to Tag property if added in Bing Maps
                if (poly != null)
                {
                    poly.Tag = polygon.Metadata;
                }

                shapes.Insert(0, poly);

                return shapes;
            }

            return null;
        }
#elif WINDOWS_APP
        public static List<MapShape> GenerateMapShape(Polygon polygon, ShapeStyle style)
        {
            if (polygon != null && polygon.ExteriorRing != null && polygon.ExteriorRing.Count >= 3)
            {
                var shapes = new List<MapShape>();

                var locs = polygon.ExteriorRing;

                //Ensure polygon is closed.
                locs.Add(locs[0]);

                //This uses a workaround to create outline of polygon
                if (style == null || style.OutlinePolygon)
                {
                    var l = GenerateMapShape(locs, style);

                    if (l != null)
                    {
                        shapes.Add(l);
                    }
                }

                //Outline inner rings.
                foreach (var i in polygon.InteriorRings)
                {
                    //Ensure inner ring is closed.
                    i.Add(i[0]);

                    var l = GenerateMapShape(i, style);

                    if (l != null)
                    {
                        if (style == null || style.OutlinePolygon)
                        {
                            shapes.Add(l);
                        }

                        //Add inner rings to main list of locations
                        locs.AddRange(i);

                        //Loop back to starting point.
                        locs.Add(locs[0]);
                    }
                }

                var poly = new MapPolygon()
                {
                    Locations = locs.ToBMGeometry()
                };

                //Set polygon style
                ApplyStyle(poly, style);

                //Add metadata as to Tag property if added in Bing Maps
                if (poly != null)
                {
                    poly.SetValue(MapShapeExt.TagProperty, polygon.Metadata);
                }

                shapes.Insert(0, poly);

                return shapes;
            }

            return null;
        }

#endif

        #endregion
    }
}
