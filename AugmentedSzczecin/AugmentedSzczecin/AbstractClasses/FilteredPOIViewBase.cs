using Caliburn.Micro;
using AugmentedSzczecin.Models;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;

namespace AugmentedSzczecin.AbstractClasses
{
    public abstract class FilteredPOIViewBase : Screen
    {
        private List<Categories> _listOfCategories = new List<Categories>()
                                                    {
                                                        new Categories() {Text = "Miejsca publiczne", EnumCategory = Category.PLACE},
                                                        new Categories() {Text = "Firmy i usługi", EnumCategory = Category.POI},
                                                        new Categories() {Text = "Wydarzenia", EnumCategory = Category.EVENT},
                                                        new Categories() {Text = "Znajomi", EnumCategory = Category.PERSON},
                                                        new Categories() {Text = "Wszystkie", EnumCategory = Category.ALL},
                                                    };

        public List<Categories> ListOfCategories
        {
            get
            {
                return _listOfCategories;
            }
            set
            {
                if (value != _listOfCategories)
                {
                    _listOfCategories = value;
                    NotifyOfPropertyChange(() => ListOfCategories);
                }
            }
        }

        private string _selectedValuePath = "EnumCategory";
        public string SelectedValuePath
        {
            get
            {
                return _selectedValuePath;
            }
            set
            {
                if (value != _selectedValuePath)
                {
                    _selectedValuePath = value;
                    NotifyOfPropertyChange(() => SelectedValuePath);
                }
            }
        }

        private Category _selectedValue;
        public Category SelectedValue
        {
            get
            {
                return _selectedValue;
            }
            set
            {
                if (value != _selectedValue)
                {
                    _selectedValue = value;
                    RefreshPOIFilteredByCategory();
                    NotifyOfPropertyChange(() => SelectedValue);
                }
            }
        }

        protected abstract void RefreshPOIFilteredByCategory();
    }
}
