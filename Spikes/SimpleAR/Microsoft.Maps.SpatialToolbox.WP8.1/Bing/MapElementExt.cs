using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Maps;

namespace Microsoft.Maps.SpatialToolbox.Bing
{
    public static class MapElementExt
    {
        public static readonly DependencyProperty TagProperty = DependencyProperty.Register("Tag", typeof(object), typeof(MapElement), new PropertyMetadata(null));
    }
}
