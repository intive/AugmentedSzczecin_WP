using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Maps.SpatialToolbox.Component
{
    /// <summary>
    /// A WinRT Component friendly feed response
    /// </summary>
    [DataContract]
    public sealed class FeedResponse
    {
        #region Constructor 

        public FeedResponse()
        {
            Error = string.Empty;
            BoundingBox = string.Empty;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// An array of shapes from the feed.
        /// </summary>
        [DataMember(Name = "shapes")]
        public IList<FeedShape> Shapes { get; set; }

        /// <summary>
        /// Bounding box containing all shapes in the feed.
        /// </summary>
        [DataMember(Name = "boundingBox")]
        public string BoundingBox { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        [DataMember(Name = "error")]
        public string Error { get; set; }

        #endregion
    }
}
