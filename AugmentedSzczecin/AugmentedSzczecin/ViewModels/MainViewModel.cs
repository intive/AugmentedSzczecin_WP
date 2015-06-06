using AugmentedSzczecin.Interfaces;
using Caliburn.Micro;
using Windows.ApplicationModel.Resources;
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
                NotifyOfPropertyChange(() => CanNavigateToAddPointOfInterest);
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

        public bool CanNavigateToAddPointOfInterest
        {
            get
            {
                bool isUserSignedIn = _accountService.IsUserSignedIn();
                return true; //isUserSignedIn;
            }
        }

        public void SignOut()
        {
            var loader = new ResourceLoader();
            var msg = new MessageDialog(loader.GetString("SignOutConfirmationText"));

            msg.Commands.Add(new UICommand("Sign out", new UICommandInvokedHandler(SignOutConfirmed)));
            msg.Commands.Add(new UICommand("Cancel"));

            msg.ShowAsync();
        }

        private void SignOutConfirmed(IUICommand command)
        {
            _accountService.SignOut();
            NotifyOfPropertyChange(() => CanNavigateToSignIn);
        }

        public void NavigateToAugmentedView()
        {
            _navigationService.NavigateToViewModel<AugmentedViewModel>();
        }

        public void NavigateToAddPointOfInterest()
        {
            _navigationService.NavigateToViewModel<AddPointOfInterestViewModel>();
        }
    }
}
