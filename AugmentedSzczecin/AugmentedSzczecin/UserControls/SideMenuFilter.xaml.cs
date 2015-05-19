using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
            (this.Content as FrameworkElement).DataContext = this;
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
