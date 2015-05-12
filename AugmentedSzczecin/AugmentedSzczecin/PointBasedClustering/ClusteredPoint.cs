using System.Collections.Generic;
using Windows.Devices.Geolocation;

namespace AugmentedSzczecin.PointBasedClustering
{
    public class ClusteredPoint
    {
        public ClusteredPoint()
        {
            ItemIndices = new List<int>();
        }

        #region Public Properties

        /// <summary>
        /// Zoom level that the clustered point is for.
        /// </summary>
        public int Zoom { get; set; }

        /// <summary>
        /// Location that the clustered point represents
        /// </summary>
        public Geopoint Location { get; set; }

        /// <summary>
        /// A list of item indices
        /// </summary>
        public IList<int> ItemIndices { get; set; }

        #endregion

        #region Internal Properties

        internal double Left { get; set; }
        internal double Right { get; set; }
        internal double Top { get; set; }
        internal double Bottom { get; set; }

        #endregion
    }
}
