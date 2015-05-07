using System;
using System.Runtime.Serialization;

namespace Microsoft.Maps.SpatialToolbox.Component
{
    /// <summary>
    /// A WinRT Component friendly shape style
    /// </summary>
    [DataContract]
    public sealed class FeedShapeStyle
    {
        #region Constructor 

        public FeedShapeStyle()
        {
            IconColor = string.Empty;
            FillColor = string.Empty;
            StrokeColor = string.Empty;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// A URI that points to an image to be used as an icon. 
        /// This can be locally hosted within the app or on the web.
        /// </summary>
        [DataMember(Name = "iconUrl")]
        public Uri IconUrl { get; set; }

        /// <summary>
        /// The scale of the icon. Can be used to scale the size of an icon.
        /// </summary>
        [DataMember(Name = "iconScale", EmitDefaultValue = true)]
        public double IconScale { get; set; }

        /// <summary>
        /// The heading of a icon. Used to rotate an icon.
        /// </summary>
        [DataMember(Name = "iconHeading", EmitDefaultValue = true)]
        public double IconHeading { get; set; }

        /// <summary>
        /// The hex color of an icon. This is used only if an IconUrl is not specified. 
        /// This will simply change the background color of the default pushpins.
        /// </summary>
        [DataMember(Name = "iconColor")]
        public string IconColor { get; set; }

        /// <summary>
        /// The hex color of polylines and polygon outlines.  
        /// </summary>
        [DataMember(Name = "strokeColor")]
        public string StrokeColor { get; set; }

        /// <summary>
        /// The fill color used by polygons.
        /// </summary>
        [DataMember(Name = "fillColor")]
        public string FillColor { get; set; }

        /// <summary>
        /// The thickness of polylines and polygon outlines.
        /// </summary>
        [DataMember(Name = "strokeThickness", EmitDefaultValue = true)]
        public double StrokeThickness { get; set; }

        /// <summary>
        /// A boolena value indicating if a polygon should be filled or not. The default is true.
        /// </summary>
        [DataMember(Name = "fillPolygon")]
        public bool FillPolygon { get; set; }

        /// <summary>
        /// A boolean value indicating if a polygon should be outlined or not. The default is true.
        /// </summary>
        [DataMember(Name = "outlinePolygon")]
        public bool OutlinePolygon { get; set; }

        #endregion
    }
}
