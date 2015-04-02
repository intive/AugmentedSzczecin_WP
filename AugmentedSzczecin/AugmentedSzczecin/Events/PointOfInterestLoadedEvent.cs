using System.Collections.ObjectModel;
using AugmentedSzczecin.Models;

namespace AugmentedSzczecin.Events
{
    public class PointOfInterestLoadedEvent
    {
        public ObservableCollection<PointOfInterest> PointOfInterestList = new ObservableCollection<PointOfInterest>();
    }
}
