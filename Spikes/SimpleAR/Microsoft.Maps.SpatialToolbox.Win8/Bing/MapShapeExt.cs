using Bing.Maps;
using Windows.UI.Xaml;

namespace Microsoft.Maps.SpatialToolbox.Bing
{
    public static class MapShapeExt
    {
        public static readonly DependencyProperty TagProperty = DependencyProperty.Register("Tag", typeof(object), typeof(MapShape), new PropertyMetadata(null));
    }
}
