using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Geolocation;
using Windows.Phone.UI.Input;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using AugmentedSzczecin.Events;
using AugmentedSzczecin.Interfaces;
using AugmentedSzczecin.Models;
using Caliburn.Micro;

namespace AugmentedSzczecin.ViewModels
{
    public class CreatePointOfInterestViewModel : Screen, IHandle<CreatePointOfInterestSuccessEvent>, IHandle<CreatePointOfInterestFailedEvent>
    {
        #region Private & Public Fields

        private readonly IEventAggregator _eventAggregator;
        private readonly INavigationService _navigationService;
        private readonly IPointOfInterestService _pointOfInterestService;
        private readonly ResourceLoader _loader = new ResourceLoader();

        #endregion

        #region Constructors

        public CreatePointOfInterestViewModel(IEventAggregator eventAggregator, INavigationService navigationService, IPointOfInterestService pointOfInterestService)
        {
            _eventAggregator = eventAggregator;
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
                    _www = String.IsNullOrEmpty(value) ? null : value;
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
                    _phone = String.IsNullOrEmpty(value) ? null : value;
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
                    _fanpage = String.IsNullOrEmpty(value) ? null : value;
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
        private TimeSpan _mondayOpenFrom;
        public TimeSpan MondayOpenFrom
        {
            get { return _mondayOpenFrom; }
            set
            {
                if (_mondayOpenFrom != value)
                {
                    _mondayOpenFrom = value;
                    NotifyOfPropertyChange(() => MondayOpenFrom);
                }
            }
        }
        private TimeSpan _mondayOpenTo;
        public TimeSpan MondayOpenTo
        {
            get { return _mondayOpenTo; }
            set
            {
                if (_mondayOpenTo != value)
                {
                    _mondayOpenTo = value;
                    NotifyOfPropertyChange(() => MondayOpenTo);
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
        private TimeSpan _tuesdayOpenFrom;
        public TimeSpan TuesdayOpenFrom
        {
            get { return _tuesdayOpenFrom; }
            set
            {
                if (_tuesdayOpenFrom != value)
                {
                    _tuesdayOpenFrom = value;
                    NotifyOfPropertyChange(() => TuesdayOpenFrom);
                }
            }
        }
        private TimeSpan _tuesdayOpenTo;
        public TimeSpan TuesdayOpenTo
        {
            get { return _tuesdayOpenTo; }
            set
            {
                if (_tuesdayOpenTo != value)
                {
                    _tuesdayOpenTo = value;
                    NotifyOfPropertyChange(() => TuesdayOpenTo);
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
        private TimeSpan _wednesdayOpenFrom;
        public TimeSpan WednesdayOpenFrom
        {
            get { return _wednesdayOpenFrom; }
            set
            {
                if (_wednesdayOpenFrom != value)
                {
                    _wednesdayOpenFrom = value;
                    NotifyOfPropertyChange(() => WednesdayOpenFrom);
                }
            }
        }
        private TimeSpan _wednesdayOpenTo;
        public TimeSpan WednesdayOpenTo
        {
            get { return _wednesdayOpenTo; }
            set
            {
                if (_wednesdayOpenTo != value)
                {
                    _wednesdayOpenTo = value;
                    NotifyOfPropertyChange(() => WednesdayOpenTo);
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
        private TimeSpan _thursdayOpenFrom;
        public TimeSpan ThursdayOpenFrom
        {
            get { return _thursdayOpenFrom; }
            set
            {
                if (_thursdayOpenFrom != value)
                {
                    _thursdayOpenFrom = value;
                    NotifyOfPropertyChange(() => ThursdayOpenFrom);
                }
            }
        }
        private TimeSpan _thursdayOpenTo;
        public TimeSpan ThursdayOpenTo
        {
            get { return _thursdayOpenTo; }
            set
            {
                if (_thursdayOpenTo != value)
                {
                    _thursdayOpenTo = value;
                    NotifyOfPropertyChange(() => ThursdayOpenTo);
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
        private TimeSpan _fridayOpenFrom;
        public TimeSpan FridayOpenFrom
        {
            get { return _fridayOpenFrom; }
            set
            {
                if (_fridayOpenFrom != value)
                {
                    _fridayOpenFrom = value;
                    NotifyOfPropertyChange(() => FridayOpenFrom);
                }
            }
        }
        private TimeSpan _fridayOpenTo;
        public TimeSpan FridayOpenTo
        {
            get { return _fridayOpenTo; }
            set
            {
                if (_fridayOpenTo != value)
                {
                    _fridayOpenTo = value;
                    NotifyOfPropertyChange(() => FridayOpenTo);
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
        private TimeSpan _saturdayOpenFrom;
        public TimeSpan SaturdayOpenFrom
        {
            get { return _saturdayOpenFrom; }
            set
            {
                if (_saturdayOpenFrom != value)
                {
                    _saturdayOpenFrom = value;
                    NotifyOfPropertyChange(() => SaturdayOpenFrom);
                }
            }
        }
        private TimeSpan _saturdayOpenTo;
        public TimeSpan SaturdayOpenTo
        {
            get { return _saturdayOpenTo; }
            set
            {
                if (_saturdayOpenTo != value)
                {
                    _saturdayOpenTo = value;
                    NotifyOfPropertyChange(() => SaturdayOpenTo);
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
        private TimeSpan _sundayOpenFrom;
        public TimeSpan SundayOpenFrom
        {
            get { return _sundayOpenFrom; }
            set
            {
                if (_sundayOpenFrom != value)
                {
                    _sundayOpenFrom = value;
                    NotifyOfPropertyChange(() => SundayOpenFrom);
                }
            }
        }
        private TimeSpan _sundayOpenTo;
        public TimeSpan SundayOpenTo
        {
            get { return _sundayOpenTo; }
            set
            {
                if (_sundayOpenTo != value)
                {
                    _sundayOpenTo = value;
                    NotifyOfPropertyChange(() => SundayOpenTo);
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

        private List<Category> _categoryItems = CategoryList.GetCategoryList();
        public List<Category> CategoryItems
        {
            get { return _categoryItems; }
            set
            {
                if (_categoryItems != value)
                {
                    _categoryItems = value;
                    NotifyOfPropertyChange(() => CategoryItems);
                }
            }
        }

        private Category _selectedCategoryItem = null;
        public Category SelectedCategoryItem
        {
            get { return _selectedCategoryItem; }
            set
            {
                if (_selectedCategoryItem != value)
                {
                    _selectedCategoryItem = value;
                    NotifyOfPropertyChange(() => SelectedCategoryItem);
                }
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

        public Geopoint Parameter { get; set; }

        #endregion

        #region Override Methods

        protected override void OnActivate()
        {
            _eventAggregator.Subscribe(this);
            base.OnActivate();

            Latitude = Parameter.Position.Latitude;
            Longitude = Parameter.Position.Longitude;
        }

        protected override void OnDeactivate(bool close)
        {
            _eventAggregator.Unsubscribe(this);
            base.OnDeactivate(close);
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

        public async void Handle(CreatePointOfInterestSuccessEvent e)
        {
            await ShowCreatedPointOfInterestSuccessMessage(e);
        }

        public async void Handle(CreatePointOfInterestFailedEvent e)
        {
            var message = new MessageDialog(e.FailureMessage);
            await message.ShowAsync();
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
            IsCategorySelected = SelectedCategoryItem != null;
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
            return NewPointOfInterest();
        }

        private PointOfInterest NewPointOfInterest()
        {
            var pointOfInterest = new PointOfInterest
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
                Tags = SplitTagsToTagsArray()
            };

            if (IsExtraFieldChecked)
            {
                pointOfInterest.Www = Www;
                pointOfInterest.Phone = Phone;
                pointOfInterest.Fanpage = Fanpage;
                pointOfInterest.Opening = GetOpeningDaysAndHours();
            }
            pointOfInterest.Category = SelectedCategoryItem.EnumCategory.ToString();
            pointOfInterest.Subcategory = "PARK";

            return pointOfInterest;
        }

        private Opening[] GetOpeningDaysAndHours()
        {
            int size = GetOpeningDaysAndHoursArraySize();
            var openingDaysAndHours = new Opening[size];

            int index = 0;
            if (IsMondayEnabled)
            {
                openingDaysAndHours[index++] = new Opening()
                {
                    Day = DayOfWeek.Monday.ToString().ToUpper(),
                    Open = GetHour(MondayOpenFrom.Hours, MondayOpenFrom.Minutes),
                    Close = GetHour(MondayOpenTo.Hours, MondayOpenTo.Minutes)
                };
            }
            if (IsTuesdayEnabled)
            {
                openingDaysAndHours[index++] = new Opening()
                {
                    Day = DayOfWeek.Tuesday.ToString().ToUpper(),
                    Open = GetHour(TuesdayOpenFrom.Hours, TuesdayOpenFrom.Minutes),
                    Close = GetHour(TuesdayOpenTo.Hours, TuesdayOpenTo.Minutes)
                };
            }
            if (IsWednesdayEnabled)
            {
                openingDaysAndHours[index++] = new Opening()
                {
                    Day = DayOfWeek.Wednesday.ToString().ToUpper(),
                    Open = GetHour(WednesdayOpenFrom.Hours, WednesdayOpenFrom.Minutes),
                    Close = GetHour(WednesdayOpenTo.Hours, WednesdayOpenTo.Minutes)
                };
            }
            if (IsThursdayEnabled)
            {
                openingDaysAndHours[index++] = new Opening()
                {
                    Day = DayOfWeek.Thursday.ToString().ToUpper(),
                    Open = GetHour(ThursdayOpenFrom.Hours, ThursdayOpenFrom.Minutes),
                    Close = GetHour(ThursdayOpenTo.Hours, ThursdayOpenTo.Minutes)
                };
            }
            if (IsFridayEnabled)
            {
                openingDaysAndHours[index++] = new Opening()
                {
                    Day = DayOfWeek.Friday.ToString().ToUpper(),
                    Open = GetHour(FridayOpenFrom.Hours, FridayOpenFrom.Minutes),
                    Close = GetHour(FridayOpenTo.Hours, FridayOpenTo.Minutes)
                };
            }
            if (IsSaturdayEnabled)
            {
                openingDaysAndHours[index++] = new Opening()
                {
                    Day = DayOfWeek.Saturday.ToString().ToUpper(),
                    Open = GetHour(SaturdayOpenFrom.Hours, SaturdayOpenFrom.Minutes),
                    Close = GetHour(SaturdayOpenTo.Hours, SaturdayOpenTo.Minutes)
                };
            }
            if (IsSundayEnabled)
            {
                openingDaysAndHours[index] = new Opening()
                {
                    Day = DayOfWeek.Sunday.ToString().ToUpper(),
                    Open = GetHour(SundayOpenFrom.Hours, SundayOpenFrom.Minutes),
                    Close = GetHour(SundayOpenTo.Hours, SundayOpenTo.Minutes)
                };
            }

            return openingDaysAndHours;
        }

        private int GetOpeningDaysAndHoursArraySize()
        {
            var size = IsMondayEnabled ? 1 : 0;
            size += IsTuesdayEnabled ? 1 : 0;
            size += IsWednesdayEnabled ? 1 : 0;
            size += IsThursdayEnabled ? 1 : 0;
            size += IsFridayEnabled ? 1 : 0;
            size += IsSaturdayEnabled ? 1 : 0;
            size += IsSundayEnabled ? 1 : 0;
            return size;
        }

        private string GetHour(int hours, int minutes)
        {
            var hour = new StringBuilder();

            hour.Append(hours.ToString());
            hour.Append(":");
            hour.Append(minutes == 0 ? "00" : minutes.ToString());

            return hour.ToString();
        }

        private string[] SplitTagsToTagsArray()
        {
            Tags = Tags.ToLower();
            var delimiters = new[] { ',', ' ' };
            var tagsArray = Tags.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            return tagsArray;
        }

        private async Task ShowCreatedPointOfInterestSuccessMessage(CreatePointOfInterestSuccessEvent e)
        {
            var message = new MessageDialog(e.SuccessMessage);
            message.Commands.Add(new UICommand("Menu", BackButtonInvokedHandler));
            message.Commands.Add(new UICommand("Show", BackButtonInvokedHandler));
            message.DefaultCommandIndex = 0;
            message.CancelCommandIndex = 1;

            await message.ShowAsync();
        }

        private void BackButtonInvokedHandler(IUICommand command)
        {
            switch (command.Label)
            {
                case "Menu":
                    HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
                    _navigationService.NavigateToViewModel<MainViewModel>();
                    break;
                case "Show":
                    HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
                    _navigationService.NavigateToViewModel<CurrentMapViewModel>(Parameter);
                    break;
                default:
                    return;
            }
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame != null && rootFrame.CanGoBack)
            {
                rootFrame.GoBack();
                e.Handled = true;
            }
        }

        #endregion
    }
}