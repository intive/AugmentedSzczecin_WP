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
	public class PointOfInterestHandlingService
	{
		public ObservableCollection<Place> GetPoint(string jsonString)
		{
			ObservableCollection<Place> model;
			model = JsonConvert.DeserializeObject<ObservableCollection<Place>>(jsonString);

			return model;
		}
	}
}
