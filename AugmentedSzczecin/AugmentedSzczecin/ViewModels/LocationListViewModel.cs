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

		public async Task<ObservableCollection<PointOfInterest>> LoadData()
		{
			string jsonString = await _httpRequestService.HttpGetAsync();
			return PointOfInterestList = _pointOfInterestHandlingService.GetPointOfInterest(jsonString);
		}

		private ObservableCollection<PointOfInterest> _pointOfInterestList = new ObservableCollection<PointOfInterest>();
		public ObservableCollection<PointOfInterest> PointOfInterestList
		{
			get { return _pointOfInterestList; }
			set
			{
				if (_pointOfInterestList != value)
				{
					_pointOfInterestList = value;
					NotifyOfPropertyChange(() => PointOfInterestList);
				}
			}
		}
	}
}
