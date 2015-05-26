using System;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Geolocation;
using Windows.System;
using Windows.UI.Xaml.Input;
using AugmentedSzczecin.Interfaces;
using Caliburn.Micro;

namespace AugmentedSzczecin.ViewModels
{
    public class CreatePointOfInterestViewModel : Screen
    {
        #region Private & Public Fields

        private readonly INavigationService _navigationService;
        private readonly IHttpService _httpService;
        private readonly ResourceLoader _loader = new ResourceLoader();

        #endregion

        #region Constructors

        public CreatePointOfInterestViewModel(INavigationService navigationService, IHttpService httpService)
        {
            _navigationService = navigationService;
            _httpService = httpService;
        }

        #endregion

        #region Properties

        private bool IsObligatoryFieldsValid { get; set; }

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
                    _name = value;
                    ValidateName();
                    NotifyOfPropertyChange(() => Name);
                }
            }
        }
        private bool IsNameEmptyValid { get; set; }
        private bool _isNameValid = true;
        public bool IsNameValid
        {
            get
            {
                return _isNameValid;
            }
            set
            {
                if (_isNameValid != value)
                {
                    _isNameValid = value;
                    NotifyOfPropertyChange(() => IsNameValid);
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
                    _description = value;
                    ValidateDescription();
                    NotifyOfPropertyChange(() => Description);
                }
            }
        }
        private bool IsDescriptionEmptyValid { get; set; }
        private bool _isDescriptionValid = true;
        public bool IsDescriptionValid
        {
            get
            {
                return _isDescriptionValid;
            }
            set
            {
                if (_isDescriptionValid != value)
                {
                    _isDescriptionValid = value;
                    NotifyOfPropertyChange(() => IsDescriptionValid);
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
                    _tags = value;
                    ValidateTags();
                    NotifyOfPropertyChange(() => Tags);
                }
            }
        }
        private bool IsTagsEmptyValid { get; set; }
        private bool IsTagsMatchValid { get; set; }
        private bool _isTagsValid = true;
        public bool IsTagsValid
        {
            get
            {
                return _isTagsValid;
            }
            set
            {
                if (_isTagsValid != value)
                {
                    _isTagsValid = value;
                    NotifyOfPropertyChange(() => IsTagsValid);
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
                    _street = value;
                    ValidateStreet();
                    NotifyOfPropertyChange(() => Street);
                }
            }
        }
        private bool IsStreetEmptyValid { get; set; }
        private bool IsStreetMatchValid { get; set; }
        private bool _isStreetValid = true;
        public bool IsStreetValid
        {
            get
            {
                return _isStreetValid;
            }
            set
            {
                if (_isStreetValid != value)
                {
                    _isStreetValid = value;
                    NotifyOfPropertyChange(() => IsStreetValid);
                }
            }
        }

        private string _zipCode;
        public string ZipCode
        {
            get
            {
                return _zipCode;
            }
            set
            {
                if (_zipCode != value)
                {
                    _zipCode = value;
                    ValidateZipCode();
                    NotifyOfPropertyChange(() => ZipCode);
                }
            }
        }
        private bool IsZipCodeEmptyValid { get; set; }
        private bool IsZipCodeMatchValid { get; set; }
        private bool _isZipCodeValid = true;
        public bool IsZipCodeValid
        {
            get
            {
                return _isZipCodeValid;
            }
            set
            {
                if (_isZipCodeValid != value)
                {
                    _isZipCodeValid = value;
                    NotifyOfPropertyChange(() => IsZipCodeValid);
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
                    _city = value;
                    ValidateCity();
                    NotifyOfPropertyChange(() => City);
                }
            }
        }
        private bool IsCityEmptyValid { get; set; }
        private bool IsCityMatchValid { get; set; }
        private bool _isCityValid = true;
        public bool IsCityValid
        {
            get
            {
                return _isCityValid;
            }
            set
            {
                if (_isCityValid != value)
                {
                    _isCityValid = value;
                    NotifyOfPropertyChange(() => IsCityValid);
                }
            }
        }

        private string _streetNumber;
        public string StreetNumber
        {
            get
            {
                return _streetNumber;
            }
            set
            {
                if (_streetNumber != value)
                {
                    _streetNumber = value;
                    ValidateStreetNumber();
                    NotifyOfPropertyChange(() => StreetNumber);
                }
            }
        }
        private bool IsStreetNumberEmptyValid { get; set; }
        private bool _isStreetNumberValid = true;
        public bool IsStreetNumberValid
        {
            get
            {
                return _isStreetNumberValid;
            }
            set
            {
                if (_isStreetNumberValid != value)
                {
                    _isStreetNumberValid = value;
                    NotifyOfPropertyChange(() => IsStreetNumberValid);
                }
            }
        }

        private bool _isCategorySelected = true;
        public bool IsCategorySelected
        {
            get
            {
                return _isCategorySelected;
            }
            set
            {
                if (_isCategorySelected != value)
                {
                    _isCategorySelected = value;
                    NotifyOfPropertyChange(() => IsCategorySelected);
                }
            }
        }

        private bool _isSubcategorySelected = true;
        public bool IsSubcategorySelected
        {
            get
            {
                return _isSubcategorySelected;
            }
            set
            {
                if (_isSubcategorySelected != value)
                {
                    _isSubcategorySelected = value;
                    NotifyOfPropertyChange(() => IsSubcategorySelected);
                }
            }
        }

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
                _selectedCategoryItem = value;
                NotifyOfPropertyChange(() => SelectedCategoryItem);
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
                _selectedSubcategoryItem = value;
                NotifyOfPropertyChange(() => SelectedSubcategoryItem);
            }
        }

        private bool _isExtraFieldChecked;
        public bool IsExtraFieldChecked
        {
            get { return _isExtraFieldChecked; }
            set
            {
                _isExtraFieldChecked = value;
                NotifyOfPropertyChange(() => IsExtraFieldChecked);
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

        private string[] TagsArray;
        public Geopoint Parameter { get; set; }

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
            CheckValidation();

            if (ValidationCheck)
            {

            }
        }

        public void ExtraFieldsChecked()
        {
            IsExtraFieldChecked = true;
        }

        public void ExtraFieldsUnchecked()
        {
            IsExtraFieldChecked = false;
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
            CheckObligatoryFields();
            ValidationCheck = IsNameValid
                              && IsDescriptionValid
                              && IsTagsValid
                              && IsStreetValid
                              && IsZipCodeValid
                              && IsCityValid
                              && IsStreetNumberValid
                              && IsCategorySelected
                              && IsSubcategorySelected;
        }

        private void CheckObligatoryFields()
        {
            ValidateName();
            ValidateDescription();
            ValidateTags();
            ValidateStreet();
            ValidateZipCode();
            ValidateCity();
            ValidateStreetNumber();
            ValidateCategorySelected();
            ValidateSubcategorySelected();
        }

        private void ValidateName()
        {
            IsNameValid = ValidateNameEmpty();
        }
        private bool ValidateNameEmpty()
        {
            return IsNameEmptyValid = !String.IsNullOrEmpty(Name);
        }

        private void ValidateDescription()
        {
            IsDescriptionValid = ValidateDescriptionEmpty();
        }
        private bool ValidateDescriptionEmpty()
        {
            return IsDescriptionEmptyValid = !String.IsNullOrEmpty(Description);
        }

        private void ValidateTags()
        {
            IsTagsValid = ValidateTagsEmpty() && ValidateTagsMatch();
        }
        private bool ValidateTagsEmpty()
        {
            return IsTagsEmptyValid = !String.IsNullOrEmpty(Tags);
        }
        private bool ValidateTagsMatch()
        {
            return IsTagsMatchValid = Regex.IsMatch(Tags, @"^([a-zA-ZąćęłńóśźżĄĘŁŃÓŚŹŻ0-9]+( *, *[a-zA-ZąćęłńóśźżĄĘŁŃÓŚŹŻ0-9]+)*)?$");
        }

        private void ValidateStreet()
        {
            IsStreetValid = ValidateStreetEmpty() && ValidateStreetMatch();
        }
        private bool ValidateStreetEmpty()
        {
            return IsStreetEmptyValid = !String.IsNullOrEmpty(Street);
        }
        private bool ValidateStreetMatch()
        {
            return IsStreetMatchValid = Regex.IsMatch(Street, @"^[0-9A-Za-ząćęłńóśźżĄĘŁŃÓŚŹŻ]+[0-9A-Za-ząćęłńóśźżĄĘŁŃÓŚŹŻ .]+$");
        }

        private void ValidateZipCode()
        {
            IsZipCodeValid = ValidateZipCodeEmpty() && ValidateZipCodeMatch();
        }
        private bool ValidateZipCodeEmpty()
        {
            return IsZipCodeEmptyValid = !String.IsNullOrEmpty(ZipCode);
        }
        private bool ValidateZipCodeMatch()
        {
            return IsZipCodeMatchValid = Regex.IsMatch(ZipCode, @"[0-9]{2}-[0-9]{3}");
        }

        private void ValidateCity()
        {
            IsCityValid = ValidateCityEmpty() && ValidateCityMatch();
        }
        private bool ValidateCityEmpty()
        {
            return IsCityEmptyValid = !String.IsNullOrEmpty(City);
        }
        private bool ValidateCityMatch()
        {
            return IsCityMatchValid = Regex.IsMatch(City, @"^[A-Za-ząćęłńóśźżĄĘŁŃÓŚŹŻ]+[A-Za-ząćęłńóśźżĄĘŁŃÓŚŹŻ -]+$");
        }

        private void ValidateStreetNumber()
        {
            IsStreetNumberValid = ValidateStreetNumberEmpty();
        }
        private bool ValidateStreetNumberEmpty()
        {
            return IsStreetNumberEmptyValid = !String.IsNullOrEmpty(StreetNumber);
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
            else
            {
                SelectedSubcategoryItem = null;
                IsSubcategorySelected = true;
            }
        }

        #endregion
    }
}
