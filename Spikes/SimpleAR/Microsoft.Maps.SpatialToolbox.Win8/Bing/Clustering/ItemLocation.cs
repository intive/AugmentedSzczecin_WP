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
    /// A class that links an item to alocation.
    /// </summary>
    public class ItemLocation
    {
        /// <summary>
        /// A class that links an item to alocation.
        /// </summary>
        /// <param name="item">Item to link to location.</param>
        /// <param name="location">Location to link to.</param>
#if WINDOWS_PHONE_APP
        public ItemLocation(object item, Geopoint location)
#elif WINDOWS_PHONE
        public ItemLocation(object item, GeoCoordinate location)
#else
        public ItemLocation(object item, Location location)
#endif         
        {
            Item = item;
            Location = location;
        }

        /// <summary>
        /// Item to tie to a location such as metadata or view model.
        /// </summary>
        public object Item { get; set; }

        /// <summary>
        /// Location that item is related to.
        /// </summary>
#if WINDOWS_PHONE_APP
        public Geopoint Location { get; set; }
#elif WINDOWS_PHONE
        public GeoCoordinate Location { get; set; }
#else
        public Location Location { get; set; }
#endif
    }
}
