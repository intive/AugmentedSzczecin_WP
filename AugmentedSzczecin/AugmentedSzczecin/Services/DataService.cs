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

namespace AugmentedSzczecin.Services
{
	public class DataService
	{
		static string _page = "https://patronatwp.azure-mobile.net/tables/Place";
		public async Task<List<Place>> RunAsync()
		{
			List<Place> model = null;
			HttpClient client = new HttpClient();

			HttpResponseMessage response = await client.GetAsync(_page);
			response.EnsureSuccessStatusCode();
			var jsonString = await response.Content.ReadAsStringAsync();

			model = JsonConvert.DeserializeObject<List<Place>>(jsonString);
			return (model);
		}
	}
}
