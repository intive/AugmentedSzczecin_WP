using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Services;
using AugmentedSzczecin.ViewModels;
using AugmentedSzczecin.Views;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Activation;
using Windows.Media.Capture;
using Windows.UI.Xaml;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

namespace AugmentedSzczecin
{
    public sealed partial class App
    {
        private WinRTContainer _container;
        public MediaCapture _mediaCapture;

        public App()
        {
            InitializeComponent();
        }

        protected override void Configure()
        {
            _container = new WinRTContainer();

            _container.RegisterWinRTServices();

            _container.PerRequest<MainViewModel>();
            _container.PerRequest<AboutViewModel>();
            _container.PerRequest<AugmentedViewModel>();
            _container.PerRequest<CurrentMapViewModel>();
            _container.PerRequest<LocationListViewModel>();
            _container.PerRequest<SignUpViewModel>();
            _container.PerRequest<SignInViewModel>();
            _container.PerRequest<ResetPasswordViewModel>();
            _container.PerRequest<AddPointOfInterestViewModel>();
            _container.PerRequest<CreatePointOfInterestViewModel>();
            _container.Singleton<IPointOfInterestService, PointOfInterestService>();
            _container.Singleton<ILocationService, LocationService>();
            _container.Singleton<IAccountService, AccountService>();
            _container.Singleton<IHttpService, HttpService>();
            _container.Singleton<IUserDataStorageService, UserDataStorageService>();
            IoC.GetInstance = GetInstance;

            HideStatusBar();
        }

        private static void HideStatusBar()
        {
            var statusBar = StatusBar.GetForCurrentView();
            statusBar.HideAsync();
        }

        protected override void PrepareViewFirst(Frame rootFrame)
        {
            _container.RegisterNavigationService(rootFrame);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            DisplayRootView<MainView>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }
    }
}