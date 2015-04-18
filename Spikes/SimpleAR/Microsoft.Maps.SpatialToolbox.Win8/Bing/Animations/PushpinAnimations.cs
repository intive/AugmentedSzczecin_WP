using System;

#if WINDOWS_APP
using Bing.Maps;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
#elif WPF
using Microsoft.Maps.MapControl.WPF;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
#elif WINDOWS_PHONE
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media;
using Microsoft.Phone.Maps.Controls;
#elif WINDOWS_PHONE_APP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls.Maps;
#endif

namespace Microsoft.Maps.SpatialToolbox.Bing.Animations
{
    /// <summary>
    /// A colleciton of animations for UIElements being used a pushpins on a map.
    /// Based on http://blogs.bing.com/maps/2014/09/25/part-2-bring-your-maps-to-life-creating-animations-with-bing-maps-net/
    /// </summary>
    public static class PushpinAnimations
    {
        /// <summary>
        /// Animates a pushpin using a translate transform along the Y axis. 
        /// </summary>
        /// <param name="pin">The pushpin to perform the animation on.</param>
        /// <param name="from">Intial translate position.</param>
        /// <param name="to">Final translate position.</param>
        /// <param name="duration">Length of time in ms that the animation should run for.</param>
        /// <param name="easingFunction">An easing function that specificies how the animation should progress over time.</param>
        public static void AnimateY(UIElement pin, double fromY, double toY, int duration, EasingFunctionBase easingFunction)
        {
            pin.RenderTransform = new TranslateTransform();

            var sb = new Storyboard();
            var animation = new DoubleAnimation()
            {
                From = fromY,
                To = toY,
                Duration = new TimeSpan(0, 0, 0, 0, duration),
                EasingFunction = easingFunction
            };

#if WINDOWS_APP
            Storyboard.SetTargetProperty(animation, "(UIElement.RenderTransform).(TranslateTransform.Y)");
#elif WPF
            Storyboard.SetTargetProperty(animation, new PropertyPath("RenderTransform.(TranslateTransform.Y)", pin));
#elif WINDOWS_PHONE
            Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
#elif WINDOWS_PHONE_APP
            Storyboard.SetTargetProperty(animation, "(UIElement.RenderTransform).(TranslateTransform.Y)");
#endif
            Storyboard.SetTarget(animation, pin);

            sb.Children.Add(animation);
            sb.Begin();
        }

        /// <summary>A simple animation that drops a pin from a height above it's destinated location onto the map.</summary>
        /// <param name="pin">The pushpin to perform the animation on.</param>
        /// <param name="height">The height above the destinated location to drop the pushpin from. Default is 150 pixels.</param>
        /// <param name="duration">Length of time in ms that the animation should run for. Default is 150 ms.</param>
        public static void Drop(UIElement pin, double? height, int? duration)
        {
            height = (height.HasValue && height.Value > 0) ? height : 150;
            duration = (duration.HasValue && duration.Value > 0) ? duration : 150;

#if WINDOWS_APP
            var anchor = MapLayer.GetPositionAnchor(pin);
#elif WPF 
            var anchor = MapLayer.GetPositionOffset(pin);
#elif WINDOWS_PHONE
            var anchor = new System.Windows.Point(0, 0);
#elif WINDOWS_PHONE_APP
            var anchor = MapControl.GetNormalizedAnchorPoint(pin);
#endif

            var from = anchor.Y + height.Value;

            AnimateY(pin, -from, -anchor.Y, duration.Value, new QuadraticEase()
            {
                EasingMode = EasingMode.EaseIn
            });
        }

        /// <summary>A simple animation that drops a pin from a height above it's destinated location onto the map and bounce's it to rest.</summary>
        /// <param name="pin">The pushpin to perform the animation on.</param>
        /// <param name="height">The height above the destinated location to drop the pushpin from. Default is 150 pixels.</param>
        /// <param name="duration">Length of time in ms that the animation should run for. Default is 1000 ms.</param>
        public static void Bounce(UIElement pin, double? height, int? duration)
        {
            height = (height.HasValue && height.Value > 0) ? height : 150;
            duration = (duration.HasValue && duration.Value > 0) ? duration : 1000;

#if WINDOWS_APP
            var anchor = MapLayer.GetPositionAnchor(pin);
#elif WPF
            var anchor = MapLayer.GetPositionOffset(pin);
#elif WINDOWS_PHONE
            var anchor = new System.Windows.Point(0, 0);
#elif WINDOWS_PHONE_APP
            var anchor = MapControl.GetNormalizedAnchorPoint(pin);
#endif

            var from = anchor.Y + height.Value;

            AnimateY(pin, -from, -anchor.Y, duration.Value, new BounceEase()
            {
                Bounces = 2,
                EasingMode = EasingMode.EaseOut,
                Bounciness = 2
            });
        }
    }
}
