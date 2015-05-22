using System;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Geolocation;
using Windows.System;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using AugmentedSzczecin.Views;
using Caliburn.Micro;

namespace AugmentedSzczecin.ViewModels
{
    public class CreatePointOfInterestViewModel : Screen
    {
        #region Private & Public Fields

        private readonly INavigationService _navigationService;
        private readonly ResourceLoader _loader = new ResourceLoader();

        private TextBox _nameBox,
            _descriptionBox,
            _tagsBox,
            _streetBox,
            _postalCodeBox,
            _cityBox,
            _houseBox,
            _placeBox;

        private ComboBox _categoryBox, _subcategoryBox;


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
                return new BindableCollection<string>(new[]
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
                _categoryBox.Background = Application.Current.Resources["TextBoxPlaceholderTextThemeBrush"] as SolidColorBrush;
                _selectedCategoryItem = value;
                ValidateCategorySelected();
                ValidateSubcategorySelected();
                CheckValidation();
                NotifyOfPropertyChange(() => SelectedCategoryItem);
                if (_selectedCategoryItem == _loader.GetString("NewPoiCategoryFirst"))
                {
                    SubcategoryVisibility = Visibility.Visible;
                }
                else
                {
                    SubcategoryVisibility = Visibility.Collapsed;
                    SelectedSubcategoryItem = null;
                }
            }
        }

        public BindableCollection<string> SubcategoryItems
        {
            get
            {
                return new BindableCollection<string>(new[]
                {
                    _loader.GetString("NewPoiSubcategoryFirst"),
                    _loader.GetString("NewPoiSubcategorySecond"),
                    _loader.GetString("NewPoiSubcategoryThird"),
                    _loader.GetString("NewPoiSubcategoryFourth"),
                    _loader.GetString("NewPoiSubcategoryFifth"),
                    _loader.GetString("NewPoiSubcategorySixth"),
                    _loader.GetString("NewPoiSubcategorySeventh"),
                    _loader.GetString("NewPoiSubcategoryEighth"),
                    _loader.GetString("NewPoiSubcategoryNinth"),
                    _loader.GetString("NewPoiSubcategoryTenth")
                });
            }
        }

        private string _selectedSubcategoryItem;
        public string SelectedSubcategoryItem
        {
            get { return _selectedSubcategoryItem; }
            set
            {
                _subcategoryBox.Background = Application.Current.Resources["TextBoxPlaceholderTextThemeBrush"] as SolidColorBrush;
                _selectedSubcategoryItem = value;
                ValidateSubcategorySelected();
                CheckValidation();
                NotifyOfPropertyChange(() => SelectedSubcategoryItem);
            }
        }

        public BindableCollection<string> FeeOptions
        {
            get
            {
                return new BindableCollection<string>(new[]
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

        private Visibility _subcategoryVisibility = Visibility.Collapsed;
        public Visibility SubcategoryVisibility
        {
            get { return _subcategoryVisibility; }
            set
            {
                if (value != _subcategoryVisibility)
                {
                    _subcategoryVisibility = value;
                    NotifyOfPropertyChange(() => SubcategoryVisibility);
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

        private bool IsObligatoryFieldsValid { get; set; }
        private bool IsNameEmptyValid { get; set; }
        private bool IsDescriptionEmptyValid { get; set; }
        private bool IsTagsEmptyValid { get; set; }
        private bool IsTagsMatchValid { get; set; }
        private bool IsStreetEmptyValid { get; set; }
        private bool IsStreetMatchValid { get; set; }
        private bool IsPostalCodeEmptyValid { get; set; }
        private bool IsPostalCodeMatchValid { get; set; }
        private bool IsCityEmptyValid { get; set; }
        private bool IsCityMatchValid { get; set; }
        private bool IsHouseEmptyValid { get; set; }
        private bool IsPlaceEmptyValid { get; set; }
        private bool IsCategorySelected { get; set; }
        private bool IsSubcategorySelected { get; set; }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _nameBox.Background = Application.Current.Resources["TextBoxPlaceholderTextThemeBrush"] as SolidColorBrush;
                    _name = value;
                    ValidateNameEmpty();
                    CheckValidation();
                    NotifyOfPropertyChange(() => Name);
                }
            }
        }

        private string _description;
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (_description != value)
                {
                    _descriptionBox.Background = Application.Current.Resources["TextBoxPlaceholderTextThemeBrush"] as SolidColorBrush;
                    _description = value;
                    ValidateDescriptionEmpty();
                    CheckValidation();
                    NotifyOfPropertyChange(() => Description);
                }
            }
        }

        private string _tags;
        public string Tags
        {
            get
            {
                return _tags;
            }
            set
            {
                if (_tags != value)
                {
                    _tagsBox.Background = Application.Current.Resources["TextBoxPlaceholderTextThemeBrush"] as SolidColorBrush;
                    _tags = value;
                    ValidateTagsEmpty();
                    ValidateTagsMatch();
                    CheckValidation();
                    NotifyOfPropertyChange(() => Tags);
                }
            }
        }

        private string _street;
        public string Street
        {
            get
            {
                return _street;
            }
            set
            {
                if (_street != value)
                {
                    _streetBox.Background = Application.Current.Resources["TextBoxPlaceholderTextThemeBrush"] as SolidColorBrush;
                    _street = value;
                    ValidateStreetEmpty();
                    ValidateStreetMatch();
                    CheckValidation();
                    NotifyOfPropertyChange(() => Street);
                }
            }
        }

        private string _postalCode;
        public string PostalCode
        {
            get
            {
                return _postalCode;
            }
            set
            {
                if (_postalCode != value)
                {
                    _postalCodeBox.Background = Application.Current.Resources["TextBoxPlaceholderTextThemeBrush"] as SolidColorBrush;
                    _postalCode = value;
                    ValidatePostalCodeEmpty();
                    ValidatePostalCodeMatch();
                    CheckValidation();
                    NotifyOfPropertyChange(() => PostalCode);
                }
            }
        }

        private string _city;
        public string City
        {
            get
            {
                return _city;
            }
            set
            {
                if (_city != value)
                {
                    _cityBox.Background = Application.Current.Resources["TextBoxPlaceholderTextThemeBrush"] as SolidColorBrush;
                    _city = value;
                    ValidateCityEmpty();
                    ValidateCityMatch();
                    CheckValidation();
                    NotifyOfPropertyChange(() => City);
                }
            }
        }

        private string _house;
        public string House
        {
            get
            {
                return _house;
            }
            set
            {
                if (_house != value)
                {
                    _houseBox.Background = Application.Current.Resources["TextBoxPlaceholderTextThemeBrush"] as SolidColorBrush;
                    _house = value;
                    ValidateHouseEmpty();
                    CheckValidation();
                    NotifyOfPropertyChange(() => House);
                }
            }
        }

        private string _place;
        public string Place
        {
            get
            {
                return _place;
            }
            set
            {
                if (_place != value)
                {
                    _placeBox.Background = Application.Current.Resources["TextBoxPlaceholderTextThemeBrush"] as SolidColorBrush;
                    _place = value;
                    ValidatePlaceEmpty();
                    CheckValidation();
                    NotifyOfPropertyChange(() => Place);
                }
            }
        }

        private bool _validationCheck;
        public bool ValidationCheck
        {
            get
            {
                return _validationCheck;
            }
            set
            {
                _validationCheck = value;
                NotifyOfPropertyChange(() => ValidationCheck);
            }
        }

        #endregion

        #region Override Methods

        protected override void OnActivate()
        {
            Latitude = Parameter.Position.Latitude;
            Longitude = Parameter.Position.Longitude;
        }

        protected override void OnViewAttached(object view, object context)
        {
            _nameBox = ((CreatePointOfInterestView)view).Name;
            _descriptionBox = ((CreatePointOfInterestView)view).Description;
            _tagsBox = ((CreatePointOfInterestView)view).Tags;
            _streetBox = ((CreatePointOfInterestView)view).Street;
            _postalCodeBox = ((CreatePointOfInterestView)view).PostalCode;
            _cityBox = ((CreatePointOfInterestView)view).City;
            _houseBox = ((CreatePointOfInterestView)view).House;
            _placeBox = ((CreatePointOfInterestView)view).Place;
            _categoryBox = ((CreatePointOfInterestView)view).CategoryItems;
            _subcategoryBox = ((CreatePointOfInterestView)view).SubcategoryItems;

            base.OnViewAttached(view, context);
        }

        #endregion

        #region Public Methods

        public void CancelNewPointOfInterestClick()
        {
            _navigationService.NavigateToViewModel<AddPointOfInterestViewModel>(Parameter);
        }

        public void AddNewPointOfInterestClick()
        {
            if (ValidationCheck)
            {

            }
            else
            {
                WrongValidationMessageDialog();
            }
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
            if (((KeyRoutedEventArgs)context.EventArgs).Key == VirtualKey.Enter)
            {
                FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
            }
        }

        #endregion

        #region Private Methods

        private void CheckValidation()
        {
            //// OBOWIAZKOWE

            ////Adres
            //// kateogrie
            //IsCategorySelected
            //isSubcategorySelected

            //// DODATKOWE
            //IsExtraFieldsSelected
            //// adress WWW, 100
            //IsWebsiteMatchValid

            ////telefon 9 - tylko cyfry
            //IsPhoneMatchValid

            //// link wikipedia, 100
            //IsWikipediaMatchValid

            //// fan page, 100
            //IsFanpageMatchValid

            //    IsObligatoryFieldsValid =
            //        IsExtraFieldsValid =
            IsObligatoryFieldsValid = IsNameEmptyValid
                && IsDescriptionEmptyValid
                && IsTagsEmptyValid && IsTagsMatchValid
                && IsStreetEmptyValid && IsStreetMatchValid
                && IsPostalCodeEmptyValid && IsPostalCodeMatchValid
                && IsCityEmptyValid && IsCityMatchValid
                && IsHouseEmptyValid
                && IsPlaceEmptyValid
                && IsCategorySelected && IsSubcategorySelected;

            ValidationCheck = IsObligatoryFieldsValid;
        }

        private void ValidateNameEmpty()
        {
            IsNameEmptyValid = !String.IsNullOrEmpty(Name);
        }

        private void ValidateDescriptionEmpty()
        {
            IsDescriptionEmptyValid = !String.IsNullOrEmpty(Description);
        }

        private void ValidateTagsEmpty()
        {
            IsTagsEmptyValid = !String.IsNullOrEmpty(Tags);
        }

        private void ValidateTagsMatch()
        {
            IsTagsMatchValid = Regex.IsMatch(Tags, @"^([a-zA-ZąćęłńóśźżĄĘŁŃÓŚŹŻ0-9]+( *, *[a-zA-ZąćęłńóśźżĄĘŁŃÓŚŹŻ0-9]+)*)?$");
        }

        private void ValidateStreetEmpty()
        {
            IsStreetEmptyValid = !String.IsNullOrEmpty(Street);
        }

        private void ValidateStreetMatch()
        {
            IsStreetMatchValid = Regex.IsMatch(Street, @"^[0-9A-Za-ząćęłńóśźżĄĘŁŃÓŚŹŻ]+[0-9A-Za-ząćęłńóśźżĄĘŁŃÓŚŹŻ .]+$");
        }

        private void ValidatePostalCodeEmpty()
        {
            IsPostalCodeEmptyValid = !String.IsNullOrEmpty(PostalCode);
        }

        private void ValidatePostalCodeMatch()
        {
            IsPostalCodeMatchValid = Regex.IsMatch(PostalCode, @"[0-9]{2}-[0-9]{3}");
        }

        private void ValidateCityEmpty()
        {
            IsCityEmptyValid = !String.IsNullOrEmpty(City);
        }

        private void ValidateCityMatch()
        {
            IsCityMatchValid = Regex.IsMatch(City, @"^[A-Za-ząćęłńóśźżĄĘŁŃÓŚŹŻ]+[A-Za-ząćęłńóśźżĄĘŁŃÓŚŹŻ -]+$");
        }

        private void ValidateHouseEmpty()
        {
            IsHouseEmptyValid = !String.IsNullOrEmpty(House);
        }

        private void ValidatePlaceEmpty()
        {
            IsPlaceEmptyValid = !String.IsNullOrEmpty(Place);
        }

        private void ValidateCategorySelected()
        {
            IsCategorySelected = !String.IsNullOrEmpty(SelectedCategoryItem);
        }

        private void ValidateSubcategorySelected()
        {
            if (SelectedCategoryItem == CategoryItems[0])
            {
                IsSubcategorySelected = !String.IsNullOrEmpty(SelectedSubcategoryItem);
            }
        }

        private async void WrongValidationMessageDialog()
        {
            var loader = new ResourceLoader();
            var message = "";

            if (!IsNameEmptyValid
                && !IsDescriptionEmptyValid
                && !IsTagsEmptyValid && !IsTagsMatchValid
                && !IsStreetEmptyValid && !IsStreetMatchValid
                && !IsPostalCodeEmptyValid && !IsPostalCodeMatchValid
                && !IsCityEmptyValid && !IsCityMatchValid
                && !IsHouseEmptyValid
                && !IsPlaceEmptyValid
                && !IsCategorySelected && !IsSubcategorySelected)
            {
                message += loader.GetString("CreatePointOfInterestEmptyForm");
            }
            else
            {
                if (!IsNameEmptyValid)
                {
                    message += HighlightWrongTextBox(_nameBox, "CreatePointOfInterestNameEmpty");
                }
                if (!IsDescriptionEmptyValid)
                {
                    message += HighlightWrongTextBox(_descriptionBox, "CreatePointOfInterestDescriptionEmpty");
                }
                if (!IsTagsEmptyValid)
                {
                    message += HighlightWrongTextBox(_tagsBox, "CreatePointOfInterestTagsEmpty");
                }
                if (IsTagsEmptyValid && !IsTagsMatchValid)
                {
                    message += HighlightWrongTextBox(_tagsBox, "CreatePointOfInterestTagsMatch");
                }
                if (!IsStreetEmptyValid)
                {
                    message += HighlightWrongTextBox(_streetBox, "CreatePointOfInterestStreetEmpty");
                }
                if (IsStreetEmptyValid && !IsStreetMatchValid)
                {
                    message += HighlightWrongTextBox(_streetBox, "CreatePointOfInterestStreetMatch");
                }
                if (!IsPostalCodeEmptyValid)
                {
                    message += HighlightWrongTextBox(_postalCodeBox, "CreatePointOfInterestPostalCodeEmpty");
                }
                if (IsPostalCodeEmptyValid && !IsPostalCodeMatchValid)
                {
                    message += HighlightWrongTextBox(_postalCodeBox, "CreatePointOfInterestPostalCodeMatch");
                }
                if (!IsCityEmptyValid)
                {
                    message += HighlightWrongTextBox(_cityBox, "CreatePointOfInterestCityEmpty");
                }
                if (IsCityEmptyValid && !IsCityMatchValid)
                {
                    message += HighlightWrongTextBox(_cityBox, "CreatePointOfInterestCityMatch");
                }
                if (!IsHouseEmptyValid)
                {
                    message += HighlightWrongTextBox(_houseBox, "CreatePointOfInterestHouseEmpty");
                }
                if (!IsPlaceEmptyValid)
                {
                    message += HighlightWrongTextBox(_placeBox, "CreatePointOfInterestPlaceEmpty");
                }
                if (!IsCategorySelected)
                {
                    message += HighlightWrongComboBox(_categoryBox, "CreatePointOfInterestCategoryNotSelected");
                }
                if (IsCategorySelected)
                {
                    if (SelectedCategoryItem == CategoryItems[0])
                    {
                        if (!IsSubcategorySelected)
                        {
                            message += HighlightWrongComboBox(_subcategoryBox, "CreatePointOfInterestSubcategoryNotSelected");
                        }
                    }
                }
            }

            var msg = new MessageDialog(message);
            await msg.ShowAsync();
        }

        private string HighlightWrongComboBox(ComboBox comboBox, string stringResource)
        {
            var loader = new ResourceLoader();

            var message = loader.GetString(stringResource) + "\n";
            comboBox.Background = new SolidColorBrush(Colors.OrangeRed);

            return message;
        }

        private string HighlightWrongTextBox(TextBox textBox, string stringResource)
        {
            var loader = new ResourceLoader();

            var message = loader.GetString(stringResource) + "\n";
            textBox.Background = new SolidColorBrush(Colors.OrangeRed);

            return message;
        }

        #endregion
    }
}
