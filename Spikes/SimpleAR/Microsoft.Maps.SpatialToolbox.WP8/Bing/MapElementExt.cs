using Microsoft.Phone.Maps.Controls;
using System.Windows;

namespace Microsoft.Maps.SpatialToolbox.Bing
{
    public static class MapElementExt
    {
        public static readonly DependencyProperty TagProperty = DependencyProperty.Register("Tag", typeof(object), typeof(MapElement), new PropertyMetadata(null));
    }
}
