using Caliburn.Micro;

namespace AugmentedSzczecin.AbstractClasses
{
    public abstract class SideMenuFilter : Screen
    {
        private string _selectedCategory;
        public string SelectedCategory
        {
            get
            {
                return _selectedCategory;
            }
            set
            {
                if (value != _selectedCategory)
                {
                    _selectedCategory = value;
                    RefreshPOIFilteredByCategory();
                    NotifyOfPropertyChange(() => SelectedCategory);
                }
            }
        }

        protected abstract void RefreshPOIFilteredByCategory();
    }
}
