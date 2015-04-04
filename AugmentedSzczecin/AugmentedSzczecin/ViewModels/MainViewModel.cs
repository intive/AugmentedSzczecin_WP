using AugmentedSzczecin.Interfaces;
using Caliburn.Micro;
using Windows.UI.Popups;

namespace AugmentedSzczecin.ViewModels
{
    public class MainViewModel : Screen
    {
        private readonly INavigationService _navigationService;
        private readonly IUserDataStorageService _userDataStorageService;

        public MainViewModel(INavigationService navigationService, IUserDataStorageService userDataStorageService)
        {
            _navigationService = navigationService;
            _userDataStorageService = userDataStorageService;
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
                bool isUserSignedIn = !_userDataStorageService.IsUserSignedIn();
                return isUserSignedIn;
            }
        }

        public void NavigateToSignIn()
        {
            _navigationService.NavigateToViewModel<SignInViewModel>();
        }

        public void SignOut()
        {
            _userDataStorageService.SignOut();
            NotifyOfPropertyChange(() => CanNavigateToSignIn);
        }

        public void CheckIfUserSignedIn()
        {
            bool isUserSignedIn = _userDataStorageService.IsUserSignedIn();
            string message = "";
            if (isUserSignedIn)
                message += "User signed in!";
            else
                message += "User signed out!";

            var msg = new MessageDialog(message);
            msg.ShowAsync();
        }
        
    }
}
