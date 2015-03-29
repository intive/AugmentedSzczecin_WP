using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Services;
using AugmentedSzczecin.ViewModels;
using AugmentedSzczecin.Views;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;

namespace AugmentedSzczecin
{
    public sealed partial class App
    {
        private WinRTContainer container;

        public App()
        {
            InitializeComponent();
        }

        protected override void Configure()
        {
            container = new WinRTContainer();

            container.RegisterWinRTServices();

            //container.Singleton<IEventAggregator, EventAggregator>();
            //container.RegisterSingleton(typeof(EventAggregator), "EventAggregator", typeof(EventAggregator));
            container.PerRequest<MainViewModel>();
            container.PerRequest<AboutViewModel>();
            container.PerRequest<CurrentMapViewModel>();
            container.PerRequest<LocationListViewModel>();
            container.PerRequest<SignUpViewModel>();
            container.PerRequest<IPointOfInterestService, PointOfInterestService>();
            container.PerRequest<ILocationService, LocationService>();
            container.PerRequest<IRegisterService, RegisterService>();
            IoC.GetInstance = this.GetInstance;
        }

        protected override void PrepareViewFirst(Frame rootFrame)
        {
            container.RegisterNavigationService(rootFrame);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            DisplayRootView<MainView>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }
    }
}