using AugmentedSzczecin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AugmentedSzczecin.Interfaces
{
    public interface IPointOfInterestHandlingService
    {
        ObservableCollection<PointOfInterest> GetPointOfInterest(string jsonString);
    }
}
