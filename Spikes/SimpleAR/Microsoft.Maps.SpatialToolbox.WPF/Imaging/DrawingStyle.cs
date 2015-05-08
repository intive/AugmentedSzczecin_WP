using System.Drawing;
using System.IO;
using System.Net;

namespace Microsoft.Maps.SpatialToolbox.Imaging
{
    public class DrawingStyle
    {
        #region Private Properties

        private Brush _fillBrush, _iconBrush;
        private Pen _strokePen;
        private Image _icon;

        #endregion

        #region Constructor

        public DrawingStyle(ShapeStyle style)
        {
            if (style.FillColor.HasValue && style.FillPolygon)
            {
                _fillBrush = style.FillColor.Value.ToDrawingBrush();
            }

            if (style.OutlinePolygon && style.StrokeColor.HasValue && !double.IsNaN(style.StrokeThickness) && style.StrokeThickness != 0)
            {
                _strokePen = style.StrokeColor.Value.ToDrawingPen((float)style.StrokeThickness);
            }

            if (style.IconColor.HasValue)
            {
                _iconBrush = style.IconColor.Value.ToDrawingBrush();                
            }

            if (style.IconUrl != null || !(!double.IsNaN(style.IconScale) && style.IconScale == 0))
            {
                try
                {
                    var wc = new WebClient();
                    var img =  Image.FromStream(new MemoryStream(wc.DownloadData(style.IconUrl)));

                    var offset = new PointF(img.Width/2, img.Height/2);
                    var heading = (!double.IsNaN(style.IconHeading)) ? style.IconHeading : 0;
                    _icon = RotateScaleImage(img, offset, (float)heading, (float)style.IconScale);
                }
                catch { }
            }
        }

        #endregion

        #region Public Properties

        public Brush FillBrush
        {
            get
            {
                return _fillBrush;
            }
        }

        public Pen StrokePen
        {
            get
            {
                return _strokePen;
            }
        }

        public Brush IconBrush
        {
            get
            {
                return _iconBrush;
            }
        }

        public Image Icon
        {
            get
            {
                return _icon;
            }
        }
        
        #endregion

        #region Public Methods

        public void Dispose()
        {
            if (_fillBrush != null)
            {
                _fillBrush.Dispose();
            }
            _fillBrush = null;

            if (_iconBrush != null)
            {
                _iconBrush.Dispose();
            }
            _iconBrush = null;

            if (_strokePen != null)
            {
                _strokePen.Dispose();
            }
            _strokePen = null;

            if (_icon != null)
            {
                _icon.Dispose();
            }
            _icon = null;
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Creates a new Image that has been scaled and rotated.
        /// </summary>
        /// <param name=""image"">The image to rotate.</param>
        /// <param name=""offset"">The position to rotate from.</param>
        /// <param name=""angle"">The amount to rotate the image, clockwise, in degrees.</param>
        /// <returns>A new image that has been scaled and rotated.</returns>
        public static Image RotateScaleImage(Image image, PointF offset, float angle, float scale)
        {
            if (image != null)
            {
                //create a new empty bitmap to hold rotated image
                var rotatedBmp = new Bitmap(image.Width, image.Height);
                rotatedBmp.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                //make a graphics object from the empty bitmap
                var g = Graphics.FromImage(rotatedBmp);

                //Put the rotation point in the center of the image
                g.TranslateTransform(offset.X, offset.Y);

                //rotate the image
                g.RotateTransform(angle);

                //scale the image
                g.ScaleTransform(scale, scale);

                //move the image back
                g.TranslateTransform(-offset.X * scale, -offset.Y * scale);                

                //draw passed in image onto graphics object
                g.DrawImage(image, new PointF(0, 0));

                return rotatedBmp;
            }

            return null;
        }

        #endregion
    }
}
