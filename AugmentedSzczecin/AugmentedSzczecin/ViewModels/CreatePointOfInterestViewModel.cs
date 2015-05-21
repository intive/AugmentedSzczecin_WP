using System;
using Windows.Devices.Geolocation;
using Windows.System;
using Caliburn.Micro;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;

namespace AugmentedSzczecin.ViewModels
{
    public class CreatePointOfInterestViewModel : Screen
    {
        #region Private & Public Fields

        private readonly INavigationService _navigationService;
        private readonly ResourceLoader _loader = new ResourceLoader();

        #endregion

        #region Constructors

        public CreatePointOfInterestViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        #endregion

        #region Properties

        public BindableCollection<string> CategoryItems
        {
            get
            {
                return new BindableCollection<string>(new string[]
                {
                    _loader.GetString("NewPoiCategoryFirst"),
                    _loader.GetString("NewPoiCategorySecond"),
                    _loader.GetString("NewPoiCategoryThird"),
                    _loader.GetString("NewPoiCategoryFourth")
                });
            }
        }

        private string _selectedCategoryItem;
        public string SelectedCategoryItem
        {
            get { return _selectedCategoryItem; }
            set
            {
                _selectedCategoryItem = value;
                NotifyOfPropertyChange(() => SelectedCategoryItem);
                if (_selectedCategoryItem == _loader.GetString("NewPoiCategoryFirst"))
                {
                    SubCategoryVisibility = Visibility.Visible;
                }
                else
                {
                    SubCategoryVisibility = Visibility.Collapsed;
                    SelectedSubCategoryItem = null;
                }
            }
        }

        public BindableCollection<string> SubCategoryItems
        {
            get
            {
                return new BindableCollection<string>(new string[]
                {
                    _loader.GetString("NewPoiSubCategoryFirst"),
                    _loader.GetString("NewPoiSubCategorySecond"),
                    _loader.GetString("NewPoiSubCategoryThird"),
                    _loader.GetString("NewPoiSubCategoryFourth"),
                    _loader.GetString("NewPoiSubCategoryFifth"),
                    _loader.GetString("NewPoiSubCategorySixth"),
                    _loader.GetString("NewPoiSubCategorySeventh"),
                    _loader.GetString("NewPoiSubCategoryEighth"),
                    _loader.GetString("NewPoiSubCategoryNinth"),
                    _loader.GetString("NewPoiSubCategoryTenth")
                });
            }
        }

        private string _selectedSubCategoryItem;
        public string SelectedSubCategoryItem
        {
            get { return _selectedSubCategoryItem; }
            set
            {
                _selectedSubCategoryItem = value;
                NotifyOfPropertyChange(() => SelectedSubCategoryItem);
            }
        }

        public BindableCollection<string> FeeOptions
        {
            get
            {
                return new BindableCollection<string>(new string[]
                {
                    _loader.GetString("PricePayable"),
                    _loader.GetString("PriceFree"),
                });
            }
        }

        private string _selectedFeeOption;
        public string SelectedFeeOption
        {
            get { return _selectedFeeOption; }
            set
            {
                _selectedFeeOption = value;
                NotifyOfPropertyChange(() => SelectedFeeOption);
            }
        }


        public Geopoint Parameter { get; set; }

        private double _latitude;
        public double Latitude
        {
            get { return _latitude; }
            set
            {
                if (_latitude != value)
                {
                    _latitude = value;
                    NotifyOfPropertyChange(() => Latitude);
                }
            }
        }

        private double _longitude;
        public double Longitude
        {
            get { return _longitude; }
            set
            {
                if (_longitude != value)
                {
                    _longitude = value;
                    NotifyOfPropertyChange(() => Longitude);
                }
            }
        }

        private Visibility _subCategoryVisibility = Visibility.Collapsed;
        public Visibility SubCategoryVisibility
        {
            get { return _subCategoryVisibility; }
            set
            {
                if (value != _subCategoryVisibility)
                {
                    _subCategoryVisibility = value;
                    NotifyOfPropertyChange(() => SubCategoryVisibility);
                }
            }
        }

        private Visibility _extraFieldsVisibility = Visibility.Collapsed;
        public Visibility ExtraFieldsVisibility
        {
            get { return _extraFieldsVisibility; }
            set
            {
                if (value != _extraFieldsVisibility)
                {
                    _extraFieldsVisibility = value;
                    NotifyOfPropertyChange(() => ExtraFieldsVisibility);
                }
            }
        }

        #endregion

        #region Override Methods

        protected override void OnActivate()
        {
            Latitude = Parameter.Position.Latitude;
            Longitude = Parameter.Position.Longitude;
        }

        #endregion

        #region Public Methods

        public void CancelNewPointOfInterestClick()
        {
            _navigationService.NavigateToViewModel<AddPointOfInterestViewModel>(Parameter);
        }

        public void AddNewPointOfInterestClick()
        {
            
        }

        public void ExtraFieldsChecked()
        {
            ExtraFieldsVisibility = Visibility.Visible;
        }

        public void ExtraFieldsUnchecked()
        {
            ExtraFieldsVisibility = Visibility.Collapsed;
        }

        public void OnKeyDown(ActionExecutionContext context)
        {
            if (((Windows.UI.Xaml.Input.KeyRoutedEventArgs)context.EventArgs).Key == VirtualKey.Enter)
            {
                FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
            }
        }

        #endregion
    }
}
