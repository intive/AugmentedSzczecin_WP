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
    public sealed partial class ArPin : UserControl
    {
        public ArPin()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty PinSignProperty = DependencyProperty.Register(
                                          "PinSign",
                                          typeof(Symbol),
                                          typeof(ArPin),
                                          new PropertyMetadata(""));

        public static readonly DependencyProperty PinDistProperty = DependencyProperty.Register(
                                          "PinDist",
                                          typeof(String),
                                          typeof(ArPin),
                                          new PropertyMetadata(""));

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

        public String PinDist
        {
            get
            {
                return (String)GetValue(PinSignProperty);
            }
            set
            {
                SetValue(PinDistProperty, value);
            }
        }
    }
}
