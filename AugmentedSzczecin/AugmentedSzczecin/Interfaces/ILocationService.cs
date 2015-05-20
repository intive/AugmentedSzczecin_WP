using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace AugmentedSzczecin.Interfaces
{
    public interface ILocationService
    {
        Task<Geopoint> GetGeolocation();
        Task<Geopoint> GetGeolocation(Geopoint geoParameter);

        bool IsGeolocationEnabled();
        
    }
}
