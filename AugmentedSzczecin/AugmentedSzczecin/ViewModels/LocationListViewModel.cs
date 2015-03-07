using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Models;
using AugmentedSzczecin.Services;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace AugmentedSzczecin.ViewModels
{
	public class LocationListViewModel : Screen
	{
		private IHttpRequestService _httpRequestService;
		private IPointOfInterestHandlingService _pointOfInterestHandlingService;

		public LocationListViewModel(IHttpRequestService httpRequestService, IPointOfInterestHandlingService pointOfInterestHandlingService)
		{
			_httpRequestService = httpRequestService;
			_pointOfInterestHandlingService = pointOfInterestHandlingService;

			LoadData();
		}

		private async void LoadData()
		{
			string jsonString = await _httpRequestService.HttpGetAsync();
			Places = _pointOfInterestHandlingService.GetPoint(jsonString);
		}

		private ObservableCollection<Place> _places = new ObservableCollection<Place>();
		public ObservableCollection<Place> Places
		{
			get { return _places; }
			set
			{
				if (_places != value)
				{
					_places = value;
					NotifyOfPropertyChange(() => Places);
				}
			}
		}
	}
}
