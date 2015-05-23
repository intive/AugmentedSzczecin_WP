using AugmentedSzczecin.Models;

namespace AugmentedSzczecin.Interfaces
{
    public interface IPointOfInterestService
    {
        /// <summary>
        /// Load all places
        /// </summary>
        void LoadPlaces();

        /// <summary>
        /// Load points of interest in specified range
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="radius"></param>
        void LoadPoIs(double latitude, double longitude, int radius, CategoryType category);
    }
}
