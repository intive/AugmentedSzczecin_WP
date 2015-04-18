using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Maps.SpatialToolbox;

namespace Microsoft.Maps.SpatialToolbox.Imaging
{
    public class SpatialDataRenderEngine
    {
        #region Private Properties

        private DrawingStyle _defaultStyle;

        private Bitmap _bitmap;

        private bool _isHighSpeed = true;

        private StyleColor? _backgroundColor;

        #endregion

        #region Constructor

        public SpatialDataRenderEngine()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isHighSpeed">Indicates if high speed rendering should be used. If true then quality of rendering may be degraded.</param>
        public SpatialDataRenderEngine(bool isHighSpeed)
        {
            _isHighSpeed = isHighSpeed;
        }

        public SpatialDataRenderEngine(ShapeStyle defaultStyle)
        {
            _defaultStyle = new DrawingStyle(defaultStyle);
        }

        public SpatialDataRenderEngine(ShapeStyle defaultStyle, bool isHighSpeed)
        {
            _defaultStyle = new DrawingStyle(defaultStyle);
            _isHighSpeed = isHighSpeed;
        }

        public SpatialDataRenderEngine(DrawingStyle defaultStyle)
        {
            _defaultStyle = defaultStyle;
        }

        public SpatialDataRenderEngine(DrawingStyle defaultStyle, bool isHighSpeed)
        {
            _defaultStyle = defaultStyle;
            _isHighSpeed = isHighSpeed;
        }

        #endregion

        #region Public Methods

        public Task RenderDataAsync(SpatialDataSet data, ViewInfo viewInfo)
        {
            return RenderDataAsync(data.Geometries, data.Styles, viewInfo);
        }

        public Task RenderDataAsync(List<Geometry> data, ViewInfo viewInfo)
        {
            return RenderDataAsync(data, new Dictionary<string, DrawingStyle>(), viewInfo, null);
        }

        public Task RenderDataAsync(SpatialDataSet data, ViewInfo viewInfo, StyleColor? backgroundColor)
        {
            return RenderDataAsync(data.Geometries, ConvertStyles(data.Styles), viewInfo, backgroundColor);
        }

        public Task RenderDataAsync(List<Geometry> data, ViewInfo viewInfo, StyleColor? backgroundColor)
        {
            return RenderDataAsync(data, new Dictionary<string, DrawingStyle>(), viewInfo, backgroundColor);
        }

        public Task RenderDataAsync(List<Geometry> data, Dictionary<string, ShapeStyle> styles, ViewInfo viewInfo)
        {
            return RenderDataAsync(data, ConvertStyles(styles), viewInfo, null);
        }

        public Task RenderDataAsync(List<Geometry> data, Dictionary<string, DrawingStyle> styles, ViewInfo viewInfo, StyleColor? backgroundColor)
        {
            return Task.Run(() =>
            {
                RenderData(data, styles, viewInfo, backgroundColor);
            });
        }

        public void RenderData(List<Geometry> data, Dictionary<string, DrawingStyle> styles, ViewInfo viewInfo, StyleColor? backgroundColor)
        {
            try
            {
                if (_bitmap == null || _bitmap.Width != viewInfo.Width || _bitmap.Height != viewInfo.Height)
                {
                    _bitmap = new Bitmap(viewInfo.Width, viewInfo.Height);
                }

                using (Graphics g = Graphics.FromImage(_bitmap))
                {
                    _backgroundColor = backgroundColor;
                    var background = (backgroundColor.HasValue) ? backgroundColor.Value.ToDrawingColor() : Color.Transparent;

                    g.Clear(background);
                    g.Clip = new Region(new Rectangle(0, 0, viewInfo.Width, viewInfo.Height));

                    if (!_isHighSpeed)
                    {
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    }
                    else
                    {
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                    }                        

                    if (data != null)
                    {
                        foreach(var geo in data)
                        {
                            DrawGeometery(geo, styles, viewInfo, g);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var t = "";
            }
        }

        public System.IO.MemoryStream GetImageStream(ImageFormat imgFormat)
        {
            System.Drawing.Imaging.ImageFormat format;

            switch (imgFormat)
            {
                case ImageFormat.GIF:
                    format = System.Drawing.Imaging.ImageFormat.Gif;
                    break;
                case ImageFormat.JPEG:
                    format = System.Drawing.Imaging.ImageFormat.Jpeg;
                    break;
                case ImageFormat.PNG:
                default:
                    format = System.Drawing.Imaging.ImageFormat.Png;
                    break;
            }

            var ms = new System.IO.MemoryStream();
            _bitmap.Save(ms, format);

            return ms;
        }

        public void SaveImage(ImageFormat imgFormat, System.IO.Stream outputStream)
        {
            System.Drawing.Imaging.ImageFormat format;

            switch (imgFormat)
            {
                case ImageFormat.GIF:
                    format = System.Drawing.Imaging.ImageFormat.Gif;
                    break;
                case ImageFormat.JPEG:
                    format = System.Drawing.Imaging.ImageFormat.Jpeg;
                    break;
                case ImageFormat.PNG:
                default:
                    format = System.Drawing.Imaging.ImageFormat.Png;
                    break;
            }

            _bitmap.Save(outputStream, format);
        }

        public Task<bool> IsImageEmpty()
        {
            return Task.Run<bool>(() =>
            {
                var background = (_backgroundColor.HasValue) ? _backgroundColor.Value.ToDrawingColor() : Color.Transparent;
                return IsImageOneColor(_bitmap, background);
            });
        }

        public void Dispose()
        {
            if (_defaultStyle != null)
            {
                _defaultStyle.Dispose();
            }

            _bitmap.Dispose();
        }

        #endregion

        #region Private Methods

        private void DrawGeometery(Geometry geom, Dictionary<string, DrawingStyle> styles, ViewInfo viewInfo, Graphics g)
        {
            DrawingStyle s = null;

            if (styles != null && !string.IsNullOrEmpty(geom.StyleKey) && styles.ContainsKey(geom.StyleKey))
            {
                s = styles[geom.StyleKey];
            }
            else if (_defaultStyle != null)
            {
                s = _defaultStyle;
            }

            DrawGeometery(geom, s, viewInfo, g);
        }

        private void DrawGeometery(Geometry geom, DrawingStyle style, ViewInfo viewInfo, Graphics g)
        {
            if (style != null)
            {
                switch (geom.STGeometryType())
                {
                    case GeometryType.Point:
                        DrawPoint(geom as Point, style, viewInfo, g);
                        break;
                    case GeometryType.LineString:
                        DrawLineString(geom as LineString, style, viewInfo, g);
                        break;
                    case GeometryType.Polygon:
                        DrawPolygon(geom as Polygon, style, viewInfo, g);
                        break;
                    case GeometryType.MultiPoint:
                        DrawMutiPoint(geom as MultiPoint, style, viewInfo, g);
                        break;
                    case GeometryType.MultiLineString:
                        DrawMultiLineString(geom as MultiLineString, style, viewInfo, g);
                        break;
                    case GeometryType.MultiPolygon:
                        DrawMultiPolygon(geom as MultiPolygon, style, viewInfo, g);
                        break;
                    case GeometryType.GeometryCollection:
                        int numGeoms = geom.STNumGeometries();
                        var gc = geom as GeometryCollection;
                        for (int i = 1; i <= numGeoms; i++)
                        {
                            DrawGeometery(gc.Geometries[i], style, viewInfo, g);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void DrawPoint(PointF point, DrawingStyle style, Graphics g)
        {
            if (style != null && point != null)
            {
                if (style.Icon != null)
                {
                    var x = (float)(point.X - style.Icon.Width / 2);
                    var y = (float)(point.Y - style.Icon.Height / 2);

                    g.DrawImage(style.Icon, x, y);
                }
                else if (style.IconBrush != null)
                {
                    g.FillEllipse(style.IconBrush, point.X - 15, point.Y - 15, 30, 30);

                    if (style.StrokePen != null)
                    {
                        g.DrawEllipse(style.StrokePen, point.X - 15, point.Y - 15, 30, 30);
                    }
                }
            }
        }

        private void DrawPoint(Point point, DrawingStyle style, ViewInfo viewInfo, Graphics g)
        {
            DrawPoint(viewInfo.LatLongToPointF(point.Coordinate), style, g);
        }

        private void DrawLineString(LineString line, DrawingStyle style, ViewInfo viewInfo, Graphics g)
        {
            if (style.StrokePen != null)
            {
                g.DrawLines(style.StrokePen, viewInfo.RingToPointFList(line.Vertices));
            }
        }

        private void DrawPolygon(Polygon polygon, DrawingStyle style, ViewInfo viewInfo, Graphics g)
        {
            if (style.StrokePen != null || style.FillBrush != null)
            {
                var p = viewInfo.RingToPointFList(polygon.ExteriorRing);

                var path = new GraphicsPath(FillMode.Winding);

                //Draw external ring
                path.AddPolygon(p);

                //Cut out inner polygons
                int numRings = polygon.InteriorRings.Count;
                for (int i = 0; i < numRings; i++)
                {
                    p = viewInfo.RingToPointFList(polygon.InteriorRings[i]);
                    path.AddPolygon(p);
                }

                if (style.FillBrush != null)
                {
                    g.FillPath(style.FillBrush, path);
                }

                if (style.StrokePen != null)
                {
                    g.DrawPath(style.StrokePen, path);
                }
            }
        }

        private void DrawMutiPoint(MultiPoint mPoint, DrawingStyle style, ViewInfo viewInfo, Graphics g)
        {
            foreach (var point in mPoint.Geometries)
            {
                var p = viewInfo.LatLongToPointF(point.Coordinate);
                DrawPoint(p, style, g);
            }
        }

        private void DrawMultiLineString(MultiLineString mLine, DrawingStyle style, ViewInfo viewInfo, Graphics g)
        {
            foreach (var line in mLine.Geometries)
            {
                DrawLineString(line, style, viewInfo, g);
            }
        }

        private void DrawMultiPolygon(MultiPolygon mPolygon, DrawingStyle style, ViewInfo viewInfo, Graphics g)
        {
            foreach (var polygon in mPolygon.Geometries)
            {
                DrawPolygon(polygon, style, viewInfo, g);
            }
        }

        private Dictionary<string, DrawingStyle> ConvertStyles(Dictionary<string, ShapeStyle> styles)
        {
            if (styles != null)
            {
                var newStyles = new Dictionary<string, DrawingStyle>();

                foreach (var key in styles.Keys)
                {
                    newStyles.Add(key, new DrawingStyle(styles[key]));
                }

                return newStyles;
            }

            return null;
        }

        private bool IsImageOneColor(Bitmap img, Color comparisonColor)
        {
            BitmapData bmpData = img.LockBits(new Rectangle(0, 0, img.Width - 1, img.Height - 1), ImageLockMode.ReadOnly, img.PixelFormat);

            int bytes  = Math.Abs(bmpData.Stride);

            var loop = Parallel.For(0, img.Height, (y, loopState) =>
            {
                bool isNotOneColor = false;
                byte[] rgbValues = new byte[bytes];
                Marshal.Copy(bmpData.Scan0 + (y * bytes), rgbValues, 0, bytes);

                for (int x = 0; x <= bytes - 1; x += 4)
                {
                    if (comparisonColor.A == 0)
                    {
                        isNotOneColor |= (comparisonColor.A != rgbValues[x + 3]);
                    }
                    else
                    {
                        isNotOneColor |= (comparisonColor.A != rgbValues[x + 3] ||
                            comparisonColor.B != rgbValues[x] ||
                            comparisonColor.G != rgbValues[x + 1] ||
                            comparisonColor.R != rgbValues[x + 2]);
                    }

                    if (isNotOneColor)
                    {
                        break;
                    }
                }

                if (isNotOneColor)
                {
                    loopState.Stop();
                }
            });

            img.UnlockBits(bmpData);

            return loop.IsCompleted;
        }

        #endregion
    }
}
