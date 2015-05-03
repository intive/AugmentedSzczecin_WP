using System;

namespace Microsoft.Maps.SpatialToolbox
{
    /// <summary>
    /// A class used to defined a set of styles for rendering Bing Maps shapes.
    /// </summary>
    public class ShapeStyle
    {
        #region Private Properties

        private bool _fillPolygon = true, _outlinePolygon = true;
        private double _strokeThickness = 3, _iconScale = 1;

        #endregion

        #region Public Properties

        /// <summary>
        /// A URI that points to an image to be used as an icon. 
        /// This can be locally hosted within the app or on the web.
        /// </summary>
        public Uri IconUrl { get; set; }

        /// <summary>
        /// The scale of the icon. Can be used to scale the size of an icon.
        /// </summary>
        public double IconScale
        {
            get { return _iconScale; }
            set { _iconScale = value; }
        }

        /// <summary>
        /// The heading of a icon. Used to rotate an icon.
        /// </summary>
        public double IconHeading { get; set; }

        /// <summary>
        /// The color of an icon. This is used only if an IconUrl is not specified. 
        /// This will simply change the background color of the default pushpins.
        /// </summary>
        public StyleColor? IconColor { get; set; }

        /// <summary>
        /// The color of polylines and polygon outlines. 
        /// </summary>
        public StyleColor? StrokeColor { get; set; }

        /// <summary>
        /// The fill color used by polygons.
        /// </summary>
        public StyleColor? FillColor { get; set; }

        /// <summary>
        /// The thickness of polylines and polygon outlines.
        /// </summary>
        public double StrokeThickness
        {
            get { return _strokeThickness; }
            set { _strokeThickness = value; }
        }

        /// <summary>
        /// A boolean value indicating if a polygon should be filled or not. The default is true.
        /// </summary>
        public bool FillPolygon
        {
            get
            {
                return _fillPolygon;
            }
            set
            {
                _fillPolygon = value;
            }
        }

        /// <summary>
        /// A boolean value indicating if a polygon should be outlined or not. The default is true.
        /// </summary>
        public bool OutlinePolygon
        {
            get
            {
                return _outlinePolygon;
            }
            set
            {
                _outlinePolygon = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a copy of the ShapeStyle
        /// </summary>
        /// <returns>A copy of the ShapeStyle.</returns>
        public ShapeStyle Copy()
        {
            return new ShapeStyle()
            {
                FillColor = FillColor,
                IconColor = IconColor,
                IconHeading = IconHeading,
                IconScale = IconScale,
                IconUrl = IconUrl,
                StrokeColor = StrokeColor,
                StrokeThickness = StrokeThickness,
                FillPolygon = FillPolygon,
                OutlinePolygon = OutlinePolygon
            };
        }

        /// <summary>
        /// Merges two styles together by taking properties that have values in the 
        /// second style object and overwritting the properties in the first style object.
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        public ShapeStyle Merge(ShapeStyle style)
        {
            var newStyle = Copy();

            if (style.FillColor.HasValue)
            {
                newStyle.FillColor = style.FillColor;
            }

            if (style.IconColor.HasValue)
            {
                newStyle.IconColor = style.IconColor;
            }

            if (!double.IsNaN(style.IconHeading))
            {
                newStyle.IconHeading = style.IconHeading;
            }

            if (!double.IsNaN(style.IconScale) && style.IconScale > 0)
            {
                newStyle.IconScale = style.IconScale;
            }

            if (style.IconUrl != null)
            {
                newStyle.IconUrl = style.IconUrl;
            }

            if (style.StrokeColor.HasValue)
            {
                newStyle.StrokeColor = style.StrokeColor;
            }

            if (!double.IsNaN(style.StrokeThickness) && style.StrokeThickness >= 0)
            {
                newStyle.StrokeThickness = style.StrokeThickness;
            }

            newStyle.FillPolygon = style.FillPolygon;
            newStyle.OutlinePolygon = style.OutlinePolygon;

            return newStyle;
        }

        public override bool Equals(object obj)
        {
            if (obj is ShapeStyle)
            {
                var s = obj as ShapeStyle;

                bool iconUrlEq = (s.IconUrl == IconUrl || (s.IconUrl != null && IconUrl != null && string.Compare(s.IconUrl.OriginalString, IconUrl.OriginalString, StringComparison.OrdinalIgnoreCase) == 0));
                bool iconColorEq = ((s.IconColor.HasValue && IconColor.HasValue && IconColor.Value.Equals(s.IconColor.Value)) || (!s.IconColor.HasValue && !IconColor.HasValue));
                bool strokeColorEq = ((s.StrokeColor.HasValue && StrokeColor.HasValue && StrokeColor.Value.Equals(s.StrokeColor.Value)) || (!s.StrokeColor.HasValue && !StrokeColor.HasValue));
                bool fillColorEq = ((s.FillColor.HasValue && FillColor.HasValue && FillColor.Value.Equals(s.FillColor.Value)) || (!s.FillColor.HasValue && !FillColor.HasValue));

                return fillColorEq &&
                    s.FillPolygon == FillPolygon &&
                    iconColorEq &&
                    s.IconHeading == IconHeading &&
                    s.IconScale == IconScale &&
                    iconUrlEq &&
                    s.OutlinePolygon == OutlinePolygon &&
                    strokeColorEq &&
                    s.StrokeThickness == StrokeThickness;
            }

 	        return base.Equals(obj);
        }

        #endregion
    }
}
