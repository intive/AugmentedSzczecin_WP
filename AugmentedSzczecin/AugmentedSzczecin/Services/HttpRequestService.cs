using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AugmentedSzczecin.Services
{
	public class HttpRequestService
	{
		private static string _page = "https://patronatwp.azure-mobile.net/tables/Place";
		public async Task<string> HttpGetAsync()
		{
			ObservableCollection<Place> model = new ObservableCollection<Place>();
			HttpClient client = new HttpClient();

			HttpResponseMessage response = await client.GetAsync(_page);
			response.EnsureSuccessStatusCode();
			string jsonString = await response.Content.ReadAsStringAsync();

			//model = JsonConvert.DeserializeObject<ObservableCollection<Place>>(jsonString);

			//return model;
			return jsonString;
		}
	}
}
