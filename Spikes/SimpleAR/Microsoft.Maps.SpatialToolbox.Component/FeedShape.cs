using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Maps.SpatialToolbox.Component
{
    /// <summary>
    /// A WinRT Component friendly shape.
    /// </summary>
    [DataContract]
    public sealed class FeedShape
    {
        #region Constructor

        public FeedShape()
        {
        }

        public FeedShape(string wkt)
        {
            WKT = wkt;
            Style = null;
            Title = string.Empty;
            Description = string.Empty;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Well Known Text of shape.
        /// </summary>
        [DataMember(Name = "wkt")]
        public string WKT { get; set; }

        /// <summary>
        /// Style used for shape.
        /// </summary>
        [DataMember(Name = "style")]
        public FeedShapeStyle Style { get; set; }

        /// <summary>
        /// Main title or name.
        /// </summary>
        [DataMember(Name = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Main descriotion.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }

        /// <summary>
        /// A dictionary of all additional metadata objects by name.
        /// </summary>
        [DataMember(Name = "metadata")]
        public IDictionary<string, object> Metadata { get; set; }

        #endregion
    }
}
