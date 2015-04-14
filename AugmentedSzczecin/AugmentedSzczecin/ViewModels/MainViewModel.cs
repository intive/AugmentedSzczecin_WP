using AugmentedSzczecin.Interfaces;
using Caliburn.Micro;
using Windows.UI.Popups;

namespace AugmentedSzczecin.ViewModels
{
    public class MainViewModel : Screen
    {
        private readonly INavigationService _navigationService;
        private readonly IAccountService _accountService;

        public MainViewModel(INavigationService navigationService, IAccountService accountService)
        {
            _navigationService = navigationService;
            _accountService = accountService;
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

        public void NavigateToSignUp()
        {
            _navigationService.NavigateToViewModel<SignUpViewModel>();
        }

        public bool CanNavigateToSignIn
        {
            get
            {
                NotifyOfPropertyChange(() => CanSignOut);
                bool isUserSignedIn = !_accountService.IsUserSignedIn();
                return isUserSignedIn;
            }
        }

        public void NavigateToSignIn()
        {
            _navigationService.NavigateToViewModel<SignInViewModel>();
        }

        public bool CanSignOut
        {
            get
            {
                bool isUserSignedIn = _accountService.IsUserSignedIn();
                return isUserSignedIn;
            }
        }

        public void SignOut()
        {
            _accountService.SignOut();
            NotifyOfPropertyChange(() => CanNavigateToSignIn);
        }
    }
}
