using AugmentedSzczecin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
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

			
			return jsonString;
		}
	}
}
