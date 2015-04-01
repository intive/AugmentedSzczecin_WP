using Windows.Graphics.Display;

namespace AugmentedSzczecin.Helpers
{
    public static class ResolutionHelper
    {
        public static byte CountZoomLevel()
        {
            byte zoomLevel = 0;
            float logicalDpi = GetLogicalDpi();
            float zoom = 100000 / (logicalDpi * (float)39.37);
            if (zoom > (float)7.17 && zoom < (float)14.33)
                zoomLevel = 15;
            if (zoom > (float)14.33 && zoom < (float)28.61)
                zoomLevel = 14;
            if (zoom > (float)28.61 && zoom < (float)57.22)
                zoomLevel = 13;
            if (zoom > (float)57.22 && zoom < (float)114.44)
                zoomLevel = 12;

            return zoomLevel;
        }

        private static float GetLogicalDpi()
        {
            return DisplayInformation.GetForCurrentView().LogicalDpi;
        }
    }
}
