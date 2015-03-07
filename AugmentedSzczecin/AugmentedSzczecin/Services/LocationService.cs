using AugmentedSzczecin.Interfaces;
using System;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace AugmentedSzczecin.Services
{
    public class LocationService : ILocationService
    {
        public async Task<Geopoint> SetGeolocation()
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            Geoposition geoposition = await geolocator.GetGeopositionAsync(
            maximumAge: TimeSpan.FromMinutes(5),
            timeout: TimeSpan.FromSeconds(10)
            );

            BasicGeoposition myLocation = new BasicGeoposition
            {
                Longitude = geoposition.Coordinate.Longitude,
                Latitude = geoposition.Coordinate.Latitude
            };

            Geopoint currentLocation = new Geopoint(myLocation);

            return currentLocation;
        }
    }
}
