using System;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Geolocation;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Input;
using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Models;
using Caliburn.Micro;

namespace AugmentedSzczecin.ViewModels
{
    public class CreatePointOfInterestViewModel : Screen
    {
        #region Private & Public Fields

        private readonly INavigationService _navigationService;
        private readonly IPointOfInterestService _pointOfInterestService;
        private readonly ResourceLoader _loader = new ResourceLoader();

        #endregion

        #region Constructors

        public CreatePointOfInterestViewModel(INavigationService navigationService, IPointOfInterestService pointOfInterestService)
        {
            _navigationService = navigationService;
            _pointOfInterestService = pointOfInterestService;
        }

        #endregion

        #region Properties

        private bool _isObligatoryFieldsValid;
        public bool IsObligatoryFieldsValid
        {
            get
            {
                return _isObligatoryFieldsValid;
            }
            set
            {
                if (_isObligatoryFieldsValid != value)
                {
                    _isObligatoryFieldsValid = value;
                    NotifyOfPropertyChange(() => IsObligatoryFieldsValid);
                }
            }
        }

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

        private bool _isExtraFieldsValid;
        public bool IsExtraFieldsValid
        {
            get
            {
                return _isExtraFieldsValid;
            }
            set
            {
                if (_isExtraFieldsValid != value)
                {
                    _isExtraFieldsValid = value;
                    NotifyOfPropertyChange(() => IsExtraFieldsValid);
                }
            }
        }

        private string _www;
        public string Www
        {
            get
            {
                return _www;
            }
            set
            {
                if (_www != value)
                {
                    _www = value;
                    ValidateWww();
                    NotifyOfPropertyChange(() => Www);
                }
            }
        }
        private bool IsWwwMatchValid { get; set; }
        private bool _isWwwValid = true;
        public bool IsWwwValid
        {
            get
            {
                return _isWwwValid;
            }
            set
            {
                if (_isWwwValid != value)
                {
                    _isWwwValid = value;
                    NotifyOfPropertyChange(() => IsWwwValid);
                }
            }
        }

        private string _phone;
        public string Phone
        {
            get
            {
                return _phone;
            }
            set
            {
                if (_phone != value)
                {
                    _phone = value;
                    ValidatePhone();
                    NotifyOfPropertyChange(() => Phone);
                }
            }
        }
        private bool IsPhoneMatchValid { get; set; }
        private bool _isPhoneValid = true;
        public bool IsPhoneValid
        {
            get
            {
                return _isPhoneValid;
            }
            set
            {
                if (_isPhoneValid != value)
                {
                    _isPhoneValid = value;
                    NotifyOfPropertyChange(() => IsPhoneValid);
                }
            }
        }

        private string _fanpage;
        public string Fanpage
        {
            get
            {
                return _fanpage;
            }
            set
            {
                if (_fanpage != value)
                {
                    _fanpage = value;
                    ValidateFanpage();
                    NotifyOfPropertyChange(() => Fanpage);
                }
            }
        }
        private bool IsFanpageMatchValid { get; set; }
        private bool _isFanpageValid = true;
        public bool IsFanpageValid
        {
            get
            {
                return _isFanpageValid;
            }
            set
            {
                if (_isFanpageValid != value)
                {
                    _isFanpageValid = value;
                    NotifyOfPropertyChange(() => IsFanpageValid);
                }
            }
        }

        private bool _isMondayEnabled;
        public bool IsMondayEnabled
        {
            get { return _isMondayEnabled; }
            set
            {
                if (_isMondayEnabled != value)
                {
                    _isMondayEnabled = value;
                    NotifyOfPropertyChange(() => IsMondayEnabled);
                }
            }
        }
        private bool _isTuesdayEnabled;
        public bool IsTuesdayEnabled
        {
            get { return _isTuesdayEnabled; }
            set
            {
                if (_isTuesdayEnabled != value)
                {
                    _isTuesdayEnabled = value;
                    NotifyOfPropertyChange(() => IsTuesdayEnabled);
                }
            }
        }
        private bool _isWednesdayEnabled;
        public bool IsWednesdayEnabled
        {
            get { return _isWednesdayEnabled; }
            set
            {
                if (_isWednesdayEnabled != value)
                {
                    _isWednesdayEnabled = value;
                    NotifyOfPropertyChange(() => IsWednesdayEnabled);
                }
            }
        }
        private bool _isThursdayEnabled;
        public bool IsThursdayEnabled
        {
            get { return _isThursdayEnabled; }
            set
            {
                if (_isThursdayEnabled != value)
                {
                    _isThursdayEnabled = value;
                    NotifyOfPropertyChange(() => IsThursdayEnabled);
                }
            }
        }
        private bool _isFridayEnabled;
        public bool IsFridayEnabled
        {
            get { return _isFridayEnabled; }
            set
            {
                if (_isFridayEnabled != value)
                {
                    _isFridayEnabled = value;
                    NotifyOfPropertyChange(() => IsFridayEnabled);
                }
            }
        }
        private bool _isSaturdayEnabled;
        public bool IsSaturdayEnabled
        {
            get { return _isSaturdayEnabled; }
            set
            {
                if (_isSaturdayEnabled != value)
                {
                    _isSaturdayEnabled = value;
                    NotifyOfPropertyChange(() => IsSaturdayEnabled);
                }
            }
        }
        private bool _isSundayEnabled;
        public bool IsSundayEnabled
        {
            get { return _isSundayEnabled; }
            set
            {
                if (_isSundayEnabled != value)
                {
                    _isSundayEnabled = value;
                    NotifyOfPropertyChange(() => IsSundayEnabled);
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
                    CategoryType.PLACE.ToString(),
                    CategoryType.COMMERCIAL.ToString(),
                    CategoryType.EVENT.ToString(),
                    CategoryType.PERSON.ToString()
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
                var newPointOfInterest = CreatePointOfInterest();
                _pointOfInterestService.AddPointOfInterest(newPointOfInterest);
            }
            else
            {
                WrongValidationMessageDialog();
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
            CheckExtraFields();
            ValidationCheck = IsObligatoryFieldsValid && IsExtraFieldsValid;
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

            CheckObligatoryFieldsProperties();
        }
        private void CheckObligatoryFieldsProperties()
        {
            IsObligatoryFieldsValid = IsNameValid
                                      && IsDescriptionValid
                                      && IsTagsValid
                                      && IsStreetValid
                                      && IsZipCodeValid
                                      && IsCityValid
                                      && IsStreetNumberValid
                                      && IsCategorySelected;
        }

        private void CheckExtraFields()
        {
            ValidateWww();
            ValidatePhone();
            ValidateFanpage();

            CheckExtraFieldsProperties();
        }
        private void CheckExtraFieldsProperties()
        {
            IsExtraFieldsValid = IsWwwValid
                                 && IsPhoneValid
                                 && IsFanpageValid;
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


        private void ValidateWww()
        {
            IsWwwValid = ValidateWwwMatch();
        }
        private bool ValidateWwwMatch()
        {
            if (!String.IsNullOrEmpty(Www))
            {
                return IsWwwMatchValid = Regex.IsMatch(Www, @"^(http\:\/\/|https\:\/\/)?([a-z0-9][a-z0-9\-]*\.)+[a-z0-9][a-z0-9\-]*$");
            }
            return true;
        }

        private void ValidatePhone()
        {
            IsPhoneValid = ValidatePhoneMatch();
        }
        private bool ValidatePhoneMatch()
        {
            if (!String.IsNullOrEmpty(Phone))
            {
                return IsPhoneMatchValid = Regex.IsMatch(Phone, "[0-9]{9}");
            }
            return true;
        }

        private void ValidateFanpage()
        {
            IsFanpageValid = ValidateFanpageMatch();
        }
        private bool ValidateFanpageMatch()
        {
            if (!String.IsNullOrEmpty(Fanpage))
            {
                return IsFanpageMatchValid = Regex.IsMatch(Www, @"^(http\:\/\/|https\:\/\/)?([a-z0-9][a-z0-9\-]*\.)+[a-z0-9][a-z0-9\-]*$");
            }
            return true;
        }

        private async void WrongValidationMessageDialog()
        {
            var loader = new ResourceLoader();
            var message = loader.GetString("ValidationErrorGeneralMessage");

            var msg = new MessageDialog(message);
            await msg.ShowAsync();
        }

        private PointOfInterest CreatePointOfInterest()
        {
            var newPointOfInterest = NewPointOfInterest();
            return newPointOfInterest;
        }

        private PointOfInterest NewPointOfInterest()
        {
            return new PointOfInterest
            {
                Name = Name,
                Description = Description,
                Location = new Location()
                {
                    Latitude = Latitude,
                    Longitude = Longitude
                },
                Address = new Address()
                {
                    City = City,
                    Street = Street,
                    ZipCode = ZipCode,
                    StreetNumber = StreetNumber
                },
                Tags = SplitTagsToTagsArray(),
                Www = Www,
                Phone = Phone,
                Fanpage = Fanpage,
                Opening = new[]
                {
                    new Opening()
                    {
                        Day = DayOfWeek.Monday.ToString().ToUpper(),
                        Open = "8:30",
                        Close = "16:30"
                    },
                    new Opening()
                    {
                        Day = DayOfWeek.Friday.ToString().ToUpper(),
                        Open = "9:30",
                        Close = "17:30"
                    }
                },
                Subcategory = SelectedCategoryItem.ToUpper()
            };
        }

        private string[] SplitTagsToTagsArray()
        {
            Tags = Tags.ToLower();
            var tagsArray = Tags.Split(',');
            return tagsArray;
        }

        #endregion
    }
}
