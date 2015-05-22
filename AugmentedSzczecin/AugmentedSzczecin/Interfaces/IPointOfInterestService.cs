namespace AugmentedSzczecin.Interfaces
{
    public interface IPointOfInterestService
    {
        void Refresh();

        void Refresh(string latitude, string longitude, string radius, string category);
    }
}
