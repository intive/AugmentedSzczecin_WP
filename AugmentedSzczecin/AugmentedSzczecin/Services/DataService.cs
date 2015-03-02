using AugmentedSzczecin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Json;
using System.IO;

namespace AugmentedSzczecin.Services
{
	public class DataService
	{
		private static string _page = "https://patronatwp.azure-mobile.net/tables/Place";
		public async Task<ObservableCollection<Place>> RunAsync()
		{
			ObservableCollection<Place> model = new ObservableCollection<Place>();
			HttpClient client = new HttpClient();

			HttpResponseMessage response = await client.GetAsync(_page);
			response.EnsureSuccessStatusCode();
			string jsonString = await response.Content.ReadAsStringAsync();

			model = JsonConvert.DeserializeObject<ObservableCollection<Place>>(jsonString);

			return model;
		}
	}
}
