using Windows.Devices.Geolocation;

namespace AugmentedSzczecin.PointBasedClustering
{
    public class ItemLocation
    {
        public ItemLocation(object item, Geopoint location)
        {
            Item = item;
            Location = location;
        }

        public object Item { get; set; }
        public Geopoint Location { get; set; }
    }
}
