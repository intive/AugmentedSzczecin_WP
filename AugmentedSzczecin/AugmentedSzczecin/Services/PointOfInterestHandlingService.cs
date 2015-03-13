using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AugmentedSzczecin.Services
{
    public class PointOfInterestHandlingService : IPointOfInterestHandlingService
    {
        public ObservableCollection<PointOfInterest> GetPointOfInterest(string jsonString)
        {
            ObservableCollection<PointOfInterest> model;
            model = JsonConvert.DeserializeObject<ObservableCollection<PointOfInterest>>(jsonString);

            return model;
        }
    }
}
