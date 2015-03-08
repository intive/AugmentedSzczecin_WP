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
			ObservableCollection<Place> model = null;
			HttpClient client = new HttpClient();

			HttpResponseMessage response = await client.GetAsync(_page);
			response.EnsureSuccessStatusCode();
			string jsonString = await response.Content.ReadAsStringAsync();

			//ALTERNATIVE SOLUTION
			DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ObservableCollection<Place>));
			MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
			model = (ObservableCollection<Place>)ser.ReadObject(stream);

			//ORIGINAL SOLUTION - nulls
			//model = JsonConvert.DeserializeObject<ObservableCollection<Place>>(jsonString);
			//string x = "[{ \"id\": \"112C8C46-C35B-4A41-A213-C627CFF9351F\", \"Name\": \"WI ZUT\", \"Address\": \"ul. Żołnierska 49\", \"Latitude\": 47.6785619, \"Longitude\": -122.1311156, \"HasWifi\": true }, { \"id\": \"112C8C46-C35B-4A41-A213-C627CFF9351F\", \"Name\": \"WI ZUT\", \"Address\": \"ul. Żołnierska 49\", \"Latitude\": 47.6785619, \"Longitude\": -122.1311156, \"HasWifi\": true }]";
			//model = JsonConvert.DeserializeObject<ObservableCollection<Place>>(x);

			return model;
		}
	}
}
