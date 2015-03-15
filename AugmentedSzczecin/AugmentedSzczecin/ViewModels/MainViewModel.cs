using Caliburn.Micro;

namespace AugmentedSzczecin.ViewModels
{
	public class MainViewModel : Screen
	{
		private INavigationService _navigationService;

		public MainViewModel(INavigationService navigationService)
		{
			_navigationService = navigationService;
		}

        public void NavigateToAbout()
        {
            _navigationService.NavigateToViewModel<AboutViewModel>();
        }

        public void NavigateToCurrentMap()
        {
            _navigationService.NavigateToViewModel<CurrentMapViewModel>();
        }

        public void NavigateToLocationList()
        {
            _navigationService.NavigateToViewModel<LocationListViewModel>();
        }
    }
}
