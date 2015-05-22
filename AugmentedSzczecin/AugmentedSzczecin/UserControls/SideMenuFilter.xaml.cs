using AugmentedSzczecin.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace AugmentedSzczecin.UserControls
{
    public sealed partial class SideMenuFilter : UserControl, INotifyPropertyChanged
    {
        public SideMenuFilter()
        {
            this.InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static readonly DependencyProperty CategoriesProperty = DependencyProperty.Register(
                                          "ItemsSourceCategories",
                                          typeof(List<Categories>),
                                          typeof(SideMenuFilter),
                                          new PropertyMetadata(""));

        public static readonly DependencyProperty SelectedCategoryProperty = DependencyProperty.Register(
                                          "SelectedCategory", 
                                          typeof(Category),
                                          typeof(SideMenuFilter),
                                          new PropertyMetadata(""));

        public static readonly DependencyProperty SelectedCategoryPathProperty = DependencyProperty.Register(
                                          "SelectedCategoryPath",
                                          typeof(string),
                                          typeof(SideMenuFilter),
                                          new PropertyMetadata(""));

        public List<Categories> ItemsSourceCategories
        {
            get
            {
                return GetValue(CategoriesProperty) as List<Categories>;
            }
            set
            {
                SetValue(CategoriesProperty, value);
            }
        }

        public Category SelectedCategory
        {
            get 
            {
                return (Category)GetValue(SelectedCategoryProperty); 
            }
            set 
            {
                SetValue(SelectedCategoryProperty, value);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedCategory"));
                }
            }
        }

        public string SelectedCategoryPath
        {
            get
            {
                return (string)GetValue(SelectedCategoryPathProperty);
            }
            set
            {
                SetValue(SelectedCategoryPathProperty, value);
            }
        }
    }
}
