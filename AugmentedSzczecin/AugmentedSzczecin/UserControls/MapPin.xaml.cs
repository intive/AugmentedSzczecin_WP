using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class MapPin : UserControl
    {
        public MapPin()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty PinColorProperty = DependencyProperty.Register(
                                          "PinColor",
                                          typeof(Brush),
                                          typeof(MapPin),
                                          new PropertyMetadata(""));

        public static readonly DependencyProperty PinColorBaseProperty = DependencyProperty.Register(
                                          "PinColorBase",
                                          typeof(Brush),
                                          typeof(MapPin),
                                          new PropertyMetadata(""));

        public static readonly DependencyProperty PinSignProperty = DependencyProperty.Register(
                                          "PinSign",
                                          typeof(Symbol),
                                          typeof(MapPin),
                                          new PropertyMetadata(""));

        public Brush PinColor
        {
            get
            {
                return GetValue(PinColorProperty) as Brush;
            }
            set
            {
                SetValue(PinColorProperty, value);
            }
        }

        public Brush PinColorBase
        {
            get
            {
                return GetValue(PinColorBaseProperty) as Brush;
            }
            set
            {
                SetValue(PinColorBaseProperty, value);
            }
        }

        public Symbol PinSign
        {
            get
            {
                return (Symbol)GetValue(PinSignProperty);
            }
            set
            {
                SetValue(PinSignProperty, value);
            }
        }
    }
}
