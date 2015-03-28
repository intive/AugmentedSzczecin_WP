using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AugmentedSzczecin.Models;

namespace AugmentedSzczecin.Events
{
    public class PointOfInterestLoadedEvent
    {
        //source
        //hash
        public ObservableCollection<PointOfInterest> PointOfInterestList = new ObservableCollection<PointOfInterest>();
    }
}
