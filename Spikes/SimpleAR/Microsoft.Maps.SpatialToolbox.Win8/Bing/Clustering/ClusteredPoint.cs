using System.Collections.Generic;

#if WINDOWS_APP
using Bing.Maps;
#elif WPF
using Microsoft.Maps.MapControl.WPF;
#elif WINDOWS_PHONE_APP
using Windows.Devices.Geolocation;
#elif WINDOWS_PHONE
using System.Device.Location;
#endif

namespace Microsoft.Maps.SpatialToolbox.Bing.Clustering
{
    /// <summary>
    /// An object used for storing clustered data point intformation
    /// </summary>
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
#if WINDOWS_PHONE_APP
        public Geopoint Location { get; set; }
#elif WINDOWS_PHONE
        public GeoCoordinate Location { get; set; }
#else
        public Location Location { get; set; }
#endif

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
