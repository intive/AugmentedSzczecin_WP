using Windows.Graphics.Display;

namespace AugmentedSzczecin.Helpers
{
    public static class ResolutionHelper
    {
        public static byte CountZoomLevel()
        {
            byte ZoomLevel = 0;
            float LogicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;
            float Zoom = 100000 / (LogicalDpi * (float)39.37);
            if (Zoom > (float)7.17 && Zoom < (float)14.33)
                ZoomLevel = 15;
            if (Zoom > (float)14.33 && Zoom < (float)28.61)
                ZoomLevel = 14;
            if (Zoom > (float)28.61 && Zoom < (float)57.22)
                ZoomLevel = 13;
            if (Zoom > (float)57.22 && Zoom < (float)114.44)
                ZoomLevel = 12;

            return ZoomLevel;
        }
    }
}
