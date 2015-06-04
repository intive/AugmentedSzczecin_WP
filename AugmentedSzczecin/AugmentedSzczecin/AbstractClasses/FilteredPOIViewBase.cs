using Caliburn.Micro;
using AugmentedSzczecin.Models;
using System.Collections.Generic;

namespace AugmentedSzczecin.AbstractClasses
{
    public abstract class FilteredPOIViewBase : Screen
    {
        private List<Category> _listOfCategories = CategoryList.GetCategoryList();

        public List<Category> ListOfCategories
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

        private CategoryType _selectedValue;
        public CategoryType SelectedValue
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
                    FilterByCategory();
                    NotifyOfPropertyChange(() => SelectedValue);
                }
            }
        }

        private bool _isFilterPanelVisible = false;
        public bool IsFilterPanelVisible
        {
            get
            {
                return _isFilterPanelVisible;
            }
            set
            {
                if (value != _isFilterPanelVisible)
                {
                    _isFilterPanelVisible = value;
                    NotifyOfPropertyChange(() => IsFilterPanelVisible);
                }
            }
        }

        protected void FilterByCategory()
        {
            IsFilterPanelVisible = false;
            RefreshPOIFilteredByCategory();
        }

        protected abstract void RefreshPOIFilteredByCategory();
    }
}
