using Windows.Devices.Geolocation;
using Windows.Foundation;

namespace AugmentedSzczecin.Model
{
    public class LocationForMap
    {
        public string Name { get; set; }
        public Geopoint Geopoint { get; set; }

        public Point Anchor { get { return new Point(0.5, 1); } }
    }
}
