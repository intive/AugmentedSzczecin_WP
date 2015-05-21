using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace AugmentedSzczecin.UserControls
{
    public enum Category
    {
        MiejscaPubliczne,
        FirmyIUsługi,
        Wydarzenia,
        Znajomi,
        Wszystkie
    }

    public sealed partial class SideMenuFilter : UserControl
    {
        public SideMenuFilter()
        {
            this.InitializeComponent();
            Categories = CategoriesToChoose;
            //SideMenuFilterRoot.DataContext = this;
        }

        public static readonly DependencyProperty CategoriesProperty = DependencyProperty.Register(
                                          "Categories",
                                          typeof(Type),
                                          typeof(SideMenuFilter),
                                          new PropertyMetadata(""));

        public static readonly DependencyProperty SelectedCategoryProperty = DependencyProperty.Register(
                                          "SelectedCategory", 
                                          typeof(string),
                                          typeof(SideMenuFilter),
                                          new PropertyMetadata(""));
        
        public Type Categories
        {
            get
            {
                return GetValue(CategoriesProperty) as Type;
            }
            set
            {
                SetValue(CategoriesProperty, value);
            }
        }

        public Type CategoriesToChoose
        {
            get 
            {
                return typeof(Category);
            }
        }

        public string SelectedCategory
        {
            get 
            { 
                return GetValue(SelectedCategoryProperty) as string; 
            }
            set 
            { 
                SetValue(SelectedCategoryProperty, value); 
            }
        }

        private void ListOfCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string category = ListOfCategories.SelectedItem as string;
            SelectedCategory = category;
        }
    }
}
