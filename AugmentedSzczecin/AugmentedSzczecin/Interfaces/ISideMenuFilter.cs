using Caliburn.Micro;

namespace AugmentedSzczecin.Interfaces
{
    public class ISideMenuFilter : Screen
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
                    NotifyOfPropertyChange(() => SelectedCategory);
                }
            }
        }

    }
}
