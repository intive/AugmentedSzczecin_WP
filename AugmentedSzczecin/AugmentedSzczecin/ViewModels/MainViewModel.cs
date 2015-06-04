using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AugmentedSzczecin.Interfaces;
using Caliburn.Micro;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using AugmentedSzczecin.Views;

namespace AugmentedSzczecin.ViewModels
{
    public class MainViewModel : Screen
    {
        private readonly INavigationService _navigationService;
        private readonly IAccountService _accountService;
        private readonly DispatcherTimer _timer;

        public MainViewModel(INavigationService navigationService, IAccountService accountService)
        {
            _navigationService = navigationService;
            _accountService = accountService;

            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(2)
            };

            _timer.Tick += ChangeImage;
            _timer.Start();
        }

        private int _selectedBackground;
        public int SelectedBackground
        {
            get { return _selectedBackground; }
            set
            {
                if (_selectedBackground != value)
                {
                    _selectedBackground = value;
                    NotifyOfPropertyChange(() => SelectedBackground);
                }
            }
        }

        private List<FlipViewItem> _backgroundList = GetBackgroundList();
        public List<FlipViewItem> BackgroundList
        {
            get { return _backgroundList; }
            set
            {
                if (_backgroundList != value)
                {
                    _backgroundList = value;
                    NotifyOfPropertyChange(() => BackgroundList);
                }
            }
        }

        private static List<FlipViewItem> GetBackgroundList()
        {
            return new List<FlipViewItem>()
            {
                new FlipViewItem()
                {
                    Background = new ImageBrush()
                    {
                        Stretch = Stretch.Fill,
                        ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/SampleBackgrounds/szczecin_01.png"))
                    }
                },
                new FlipViewItem()
                {
                    Background = new ImageBrush()
                    {
                        Stretch = Stretch.Fill,
                        ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/SampleBackgrounds/szczecin_02.png"))
                    }
                },
                new FlipViewItem()
                {
                    Background = new ImageBrush()
                    {
                        Stretch = Stretch.Fill,
                        ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/SampleBackgrounds/szczecin_03.png"))
                    }
                },
                new FlipViewItem()
                {
                    Background = new ImageBrush()
                    {
                        Stretch = Stretch.Fill,
                        ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/SampleBackgrounds/szczecin_04.png"))
                    }
                },
                new FlipViewItem()
                {
                    Background = new ImageBrush()
                    {
                        Stretch = Stretch.Fill,
                        ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/SampleBackgrounds/szczecin_05.png"))
                    }
                },
                new FlipViewItem()
                {
                    Background = new ImageBrush()
                    {
                        Stretch = Stretch.Fill,
                        ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/SampleBackgrounds/szczecin_06.png"))
                    }
                }
            };
        }

        private void ChangeImage(object sender, object o)
        {
            var totalItems = BackgroundList.Count;

            var newItemIndex = (SelectedBackground + 1) % totalItems;

            SelectedBackground = newItemIndex;
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

        public void DisplayedItemChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
