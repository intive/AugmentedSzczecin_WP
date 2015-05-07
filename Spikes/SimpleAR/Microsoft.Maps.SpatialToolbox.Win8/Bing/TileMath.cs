using System;
using System.Text;
using System.Collections.Generic;

#if WINDOWS_APP
using Bing.Maps;
#elif WPF
using Microsoft.Maps.MapControl.WPF;
#elif WINDOWS_PHONE
using System.Device.Location;
#elif WINDOWS_PHONE_APP
using Windows.Devices.Geolocation;
#endif

namespace Microsoft.Maps.SpatialToolbox.Bing
{
    /// <summary>
    /// Quadkey Tile System math tools.
    /// 
    /// Based on: http://msdn.microsoft.com/en-us/library/bb259689.aspx
    /// </summary>
    public static class TileMath
    {
        #region Private Properties

        private const double MinLatitude = -85.05112878;
        private const double MaxLatitude = 85.05112878;
        private const double MinLongitude = -180;
        private const double MaxLongitude = 180;

        #endregion

        /// <summary>
        /// Clips a number to the specified minimum and maximum values.
        /// </summary>
        /// <param name="n">The number to clip.</param>
        /// <param name="minValue">Minimum allowable value.</param>
        /// <param name="maxValue">Maximum allowable value.</param>
        /// <returns>The clipped value.</returns>
        private static double Clip(double n, double minValue, double maxValue)
        {
            return Math.Min(Math.Max(n, minValue), maxValue);
        }

        /// <summary>
        /// Calculates width of the map in pixels at a specific zoom level from -180 degrees to 180 degrees.
        /// </summary>
        /// <param name="zoom">Zomm Level to calculate width at</param>
        /// <param name="tileWidth">Width of tile</param>
        /// <returns>Width of map in pixels</returns>
        public static double MapSize(double zoom)
        {
            return Math.Round(256 * Math.Pow(2, zoom));
        }

        /// <summary>
        /// Calculates the Ground resolution at a specific degree of latitude in the specified units per pixel.
        /// </summary>
        /// <param name="latitude">Degree of latitude to calculate resolution at</param>
        /// <param name="zoom">Zoom level to calculate resolution at</param>
        /// <param name="unitType">Distance unit type to calculate resolution in</param>
        /// <returns>Ground resolution in distance unit per pixels</returns>
        public static double GroundResolution(double latitude, double zoom, DistanceUnits units)
        {
            double earthRadius = SpatialTools.GetEarthRadius(units);
            return (Math.Cos(SpatialTools.ToRadians(latitude)) * 2 * Math.PI * earthRadius) / MapSize(zoom);
        }

        /// <summary>
        /// Global Converts a Pixel coordinate into a Geospatial coordinate at a specified zoom level. 
        /// Global Pixel coordinates are relative to the top left corner of the map (90, -180)
        /// </summary>
        /// <param name="point">Pixel coordinate</param>
        /// <param name="zoom">Zoom level</param>
        /// <returns>A coordinate that is at the specified pixel location at a specified zoom level</returns>
#if WINDOWS_APP
        public static Location GlobalPixelToCoordinate(Windows.Foundation.Point point, double zoom)
#elif WPF
        public static Location GlobalPixelToCoordinate(System.Windows.Point point, double zoom)
#elif WINDOWS_PHONE
        public static GeoCoordinate GlobalPixelToCoordinate(System.Windows.Point point, double zoom)
#elif WINDOWS_PHONE_APP
        public static BasicGeoposition GlobalPixelToCoordinate(Windows.Foundation.Point point, double zoom)
#endif
        {
            return GlobalPixelToCoordinate((int)point.X, (int)point.Y, zoom);
        }

        /// <summary>
        /// Global Converts a Pixel coordinate into a Geospatial coordinate at a specified zoom level. 
        /// Global Pixel coordinates are relative to the top left corner of the map (90, -180)
        /// </summary>
        /// <param name="px">X pixel coordinate</param>
        /// <param name="py">Y pixel coordinate</param>
        /// <param name="zoom">Zoom level</param>
        /// <returns>A coordinate that is at the specified pixel location at a specified zoom level</returns>
#if WINDOWS_PHONE
        public static GeoCoordinate GlobalPixelToCoordinate(int px, int py, double zoom)
#elif WINDOWS_PHONE_APP
        public static BasicGeoposition GlobalPixelToCoordinate(int px, int py, double zoom)
#else
        public static Location GlobalPixelToCoordinate(int px, int py, double zoom)
#endif
        {
            double mapSize = MapSize(zoom);
            double x = (px / mapSize) - 0.5;
            double y = 0.5 - (py / mapSize);

            var latitude = 90 - 360 * Math.Atan(Math.Exp(-y * 2 * Math.PI)) / Math.PI;
            var longitude = 360 * x;

            //double longitude = ((x * 360) / (256 * Math.Pow(2, zoom)));
            //double efactor = Math.Exp((0.5 - y / 256 / Math.Pow(2, zoom)) * 4 * Math.PI);
            //double latitude = (Math.Asin((efactor - 1) / (efactor + 1))) * (180 / Math.PI);
            #if WINDOWS_PHONE
            return new GeoCoordinate(latitude, longitude);
            #elif WINDOWS_PHONE_APP
            return new BasicGeoposition(){Latitude = latitude, Longitude = longitude};
            #else
            return new Location(latitude, longitude);
            #endif
        }

        /// <summary>
        /// Converts a point from latitude/longitude WGS-84 coordinates (in degrees)
        /// into pixel XY coordinates at a specified level of detail.
        /// </summary>
        /// <param name="location">Coordinte to convert to a global pixel.</param>
        /// <param name="zoom">Level of detail, from 1 (lowest detail)
        /// to 23 (highest detail).</param>
        /// <returns>Point object containing the the global pixel location of a coordinate.</returns>
#if WINDOWS_APP
        public static Windows.Foundation.Point LocationToGlobalPixel(Location location, double zoom)
#elif WPF
        public static System.Windows.Point LocationToGlobalPixel(Location location, double zoom)
#elif WINDOWS_PHONE
        public static System.Windows.Point LocationToGlobalPixel(GeoCoordinate location, double zoom)
#elif WINDOWS_PHONE_APP
        public static Windows.Foundation.Point LocationToGlobalPixel(BasicGeoposition location, double zoom)
#endif
        {
            double x = (location.Longitude + 180) / 360;
            double sinLatitude = Math.Sin(SpatialTools.ToRadians(location.Latitude));
            double y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);

            double mapSize = MapSize(zoom);

            #if WINDOWS_APP || WINDOWS_PHONE_APP
            return new Windows.Foundation.Point((int)(x * mapSize), (int)(y * mapSize));
            #elif WPF || WINDOWS_PHONE
            return new System.Windows.Point((int)(x * mapSize), (int)(y * mapSize));
            #endif
        }

        /// <summary>
        /// Converts a point from latitude/longitude WGS-84 coordinates (in degrees)
        /// into pixel XY coordinates at a specified level of detail.
        /// </summary>
        /// <param name="location">Coordinte to convert to a global pixel.</param>
        /// <param name="zoom">Level of detail, from 1 (lowest detail)
        /// to 23 (highest detail).</param>
        /// <returns>Point object containing the the global pixel location of a coordinate.</returns>
#if WINDOWS_APP || WINDOWS_PHONE_APP
        public static Windows.Foundation.Point LocationToGlobalPixel(Coordinate location, double zoom)
#elif WPF
        public static System.Drawing.PointF LocationToGlobalPixel(Coordinate location, double zoom)
#elif WINDOWS_PHONE
        public static System.Windows.Point LocationToGlobalPixel(Coordinate location, double zoom)
#endif
        {
            double x = (location.Longitude + 180) / 360;
            double sinLatitude = Math.Sin(SpatialTools.ToRadians(location.Latitude));
            double y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);

            double mapSize = MapSize(zoom);

            #if WINDOWS_APP || WINDOWS_PHONE_APP
            return new Windows.Foundation.Point((int)(x * mapSize), (int)(y * mapSize));
            #elif WINDOWS_PHONE
            return new System.Windows.Point((int)(x * mapSize), (int)(y * mapSize));
            #elif WPF
            return new System.Drawing.PointF((float)(x * mapSize), (float)(y * mapSize));
            #endif
        }

#if WINDOWS_APP
        /// <summary>
        /// Converts pixel XY coordinates into tile XY coordinates of the tile containing
        /// the specified pixel.
        /// </summary>
        /// <param name="pixelX">Pixel X coordinate.</param>
        /// <param name="pixelY">Pixel Y coordinate.</param>
        /// <param name="tileX">Output parameter receiving the tile X coordinate.</param>
        /// <param name="tileY">Output parameter receiving the tile Y coordinate.</param>
        public static void GlobalPixelToTileXY(Windows.Foundation.Point point, out int tileX, out int tileY)
        {
            tileX = (int)point.X / 256;
            tileY = (int)point.Y / 256;
        }

        /// <summary>
        /// Converts tile XY coordinates into a global pixel XY coordinates of the upper-left pixel
        /// of the specified tile.
        /// </summary>
        /// <param name="tileX">Tile X coordinate.</param>
        /// <param name="tileY">Tile Y coordinate.</param>
        /// <returns>Global pixel coordinate of the top left corner of a tile.</returns>
        public static Windows.Foundation.Point TileXYToGlobalPixel(int tileX, int tileY)
        {
            int pixelX = tileX * 256;
            int pixelY = tileY * 256;

            return new Windows.Foundation.Point(pixelX, pixelY);
        }

#elif WPF
        /// <summary>
        /// Converts pixel XY coordinates into tile XY coordinates of the tile containing
        /// the specified pixel.
        /// </summary>
        /// <param name="pixelX">Pixel X coordinate.</param>
        /// <param name="pixelY">Pixel Y coordinate.</param>
        /// <param name="tileX">Output parameter receiving the tile X coordinate.</param>
        /// <param name="tileY">Output parameter receiving the tile Y coordinate.</param>
        public static void GlobalPixelToTileXY(System.Windows.Point point, out int tileX, out int tileY)
        {
            tileX = (int)point.X / 256;
            tileY = (int)point.Y / 256;
        }

        /// <summary>
        /// Converts tile XY coordinates into a global pixel XY coordinates of the upper-left pixel
        /// of the specified tile.
        /// </summary>
        /// <param name="tileX">Tile X coordinate.</param>
        /// <param name="tileY">Tile Y coordinate.</param>
        /// <returns>Global pixel coordinate of the top left corner of a tile.</returns>
        public static System.Windows.Point TileXYToGlobalPixel(int tileX, int tileY)
        {
            int pixelX = tileX * 256;
            int pixelY = tileY * 256;

            return new System.Windows.Point(pixelX, pixelY);
        }

#endif
        /// <summary>
        /// Converts tile XY coordinates into a QuadKey at a specified level of detail.
        /// </summary>
        /// <param name="tileX">Tile X coordinate.</param>
        /// <param name="tileY">Tile Y coordinate.</param>
        /// <param name="zoom">Zoom level</param>
        /// <returns>A string containing the QuadKey.</returns>
        public static string TileXYToQuadKey(int tileX, int tileY, int zoom)
        {
            var quadKey = new StringBuilder();
            for (int i = zoom; i > 0; i--)
            {
                char digit = '0';
                int mask = 1 << (i - 1);
                if ((tileX & mask) != 0)
                {
                    digit++;
                }
                if ((tileY & mask) != 0)
                {
                    digit++;
                    digit++;
                }
                quadKey.Append(digit);
            }
            return quadKey.ToString();
        }

        /// <summary>
        /// Converts a QuadKey into tile XY coordinates.
        /// </summary>
        /// <param name="quadKey">QuadKey of the tile.</param>
        /// <param name="tileX">Output parameter receiving the tile X coordinate.</param>
        /// <param name="tileY">Output parameter receiving the tile Y coordinate.</param>
        /// <param name="zoom">Output parameter receiving the zoom level.</param>
        public static void QuadKeyToTileXY(string quadKey, out int tileX, out int tileY, out int zoom)
        {
            tileX = tileY = 0;
            zoom = quadKey.Length;
            for (int i = zoom; i > 0; i--)
            {
                int mask = 1 << (i - 1);
                switch (quadKey[zoom - i])
                {
                    case '0':
                        break;

                    case '1':
                        tileX |= mask;
                        break;

                    case '2':
                        tileY |= mask;
                        break;

                    case '3':
                        tileX |= mask;
                        tileY |= mask;
                        break;

                    default:
                        throw new ArgumentException("Invalid QuadKey digit sequence.");
                }
            }
        }

        /// <summary>
        /// Calculates the XY tile position that a coordinate falls into for a specific zoom level.
        /// </summary>
        /// <param name="latitude">Latitude coordinate.</param>
        /// <param name="longitude">Longitude coordinate.</param>
        /// <param name="zoomLevel">Zoom level</param>
        /// <param name="tileX">Tile X positon</param>
        /// <param name="tileY">Tile Y position</param>
        public static void LocationToTileXY(double latitude, double longitude, int zoomLevel, out int tileX, out int tileY)
        {
            latitude = Clip(latitude, MinLatitude, MaxLatitude);
            longitude = Clip(longitude, MinLongitude, MaxLongitude);

            double x = (longitude + 180) / 360;
            double sinLatitude = Math.Sin(latitude * Math.PI / 180);
            double y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);

            uint mapSize = (uint)MapSize((double)zoomLevel);
            tileX = (int)Math.Floor(Clip(x * mapSize + 0.5, 0, mapSize - 1) / 256);
            tileY = (int)Math.Floor(Clip(y * mapSize + 0.5, 0, mapSize - 1) / 256);
        }

#if WINDOWS_APP || WPF

        public static LocationRect QuadKeyToBoundingBox(QuadKey quadKey)
        {
            return QuadKeyToBoundingBox(quadKey.Key);
        }

        /// <summary>
        /// Calculates the tile QuadKey values that are within a LocationRect at a specific zoom level.
        /// </summary>
        /// <param name="view">LocationRect to search in.</param>
        /// <param name="zoomLevel">Zoom level to calculate tiles for.</param>
        /// <returns>A list of QuadKey values.</returns>
        public static List<QuadKey> GetQuadKeysInView(LocationRect view, double zoomLevel)
        {
            List<QuadKey> keys = new List<QuadKey>();
            int zoom = (int)Math.Round(zoomLevel);
            int tlX, tlY, brX, brY;

            LocationToTileXY(view.North, view.West, zoom, out tlX, out tlY);
            LocationToTileXY(view.South, view.East, zoom, out brX, out brY);

            for (int x = tlX; x <= brX; x++)
            {
                for (int y = tlY; y <= brY; y++)
                {
                    keys.Add(new QuadKey(x, y, zoom));
                }
            }

            return keys;
        }

        /// <summary>
        /// Calculates the bounding coordinates of a Quadkey tile
        /// </summary>
        /// <param name="quadkey">Quadkey of a tile</param>
        /// <returns>Bounding box of a quadkey tile</returns>
        public static LocationRect QuadKeyToBoundingBox(string quadkey)
        {
            int tileX, tileY, zoom;

            QuadKeyToTileXY(quadkey, out tileX, out tileY, out zoom);
            return TileXYToBoundingBox(tileX, tileY, zoom);
        }

        /// <summary>
        /// Calculates the bounding box of a tile
        /// </summary>
        /// <param name="tileX">Tile X coordinate</param>
        /// <param name="tileY">Tile Y coordinate</param>
        /// <param name="zoom">Zoom level</param>
        /// <returns>Bounding box of a tile</returns>
        public static LocationRect TileXYToBoundingBox(int tileX, int tileY, int zoom)
        {
            //Top left corner pixel coordinates
            int x1 = tileX * 256;
            int y1 = tileY * 256;

            //Bottom right corner pixel coordinates
            int x2 = x1 + 256;
            int y2 = y1 + 256;

            return new LocationRect(GlobalPixelToCoordinate(x1, y1, zoom),
                GlobalPixelToCoordinate(x2, y2, zoom));
        }
#elif WINDOWS_PHONE
        public static List<GeoCoordinate> QuadKeyToBoundingBox(QuadKey quadKey)
        {
            return QuadKeyToBoundingBox(quadKey.Key);
        }

        /// <summary>
        /// Calculates the bounding coordinates of a Quadkey tile
        /// </summary>
        /// <param name="quadkey">Quadkey of a tile</param>
        /// <returns>Bounding box of a quadkey tile (NorthWest, SouthEast)</returns>
        public static List<GeoCoordinate> QuadKeyToBoundingBox(string quadkey)
        {
            int tileX, tileY, zoom;

            QuadKeyToTileXY(quadkey, out tileX, out tileY, out zoom);
            return TileXYToBoundingBox(tileX, tileY, zoom);
        }

        /// <summary>
        /// Calculates the bounding box of a tile
        /// </summary>
        /// <param name="tileX">Tile X coordinate</param>
        /// <param name="tileY">Tile Y coordinate</param>
        /// <param name="zoom">Zoom level</param>
        /// <returns>Bounding box of a tile (NorthWest, SouthEast)</returns>
        public static List<GeoCoordinate> TileXYToBoundingBox(int tileX, int tileY, int zoom)
        {
            //Top left corner pixel coordinates
            int x1 = tileX * 256;
            int y1 = tileY * 256;

            //Bottom right corner pixel coordinates
            int x2 = x1 + 256;
            int y2 = y1 + 256;

            return new List<GeoCoordinate>(){
                GlobalPixelToCoordinate(x1, y1, zoom),
                GlobalPixelToCoordinate(x2, y2, zoom)
            };
        }
#elif WINDOWS_PHONE_APP
        public static GeoboundingBox QuadKeyToBoundingBox(QuadKey quadKey)
        {
            return QuadKeyToBoundingBox(quadKey.Key);
        }

        /// <summary>
        /// Calculates the bounding coordinates of a Quadkey tile
        /// </summary>
        /// <param name="quadkey">Quadkey of a tile</param>
        /// <returns>Bounding box of a quadkey tile (NorthWest, SouthEast)</returns>
        public static GeoboundingBox QuadKeyToBoundingBox(string quadkey)
        {
            int tileX, tileY, zoom;

            QuadKeyToTileXY(quadkey, out tileX, out tileY, out zoom);
            return TileXYToBoundingBox(tileX, tileY, zoom);
        }

        /// <summary>
        /// Calculates the bounding box of a tile
        /// </summary>
        /// <param name="tileX">Tile X coordinate</param>
        /// <param name="tileY">Tile Y coordinate</param>
        /// <param name="zoom">Zoom level</param>
        /// <returns>Bounding box of a tile (NorthWest, SouthEast)</returns>
        public static GeoboundingBox TileXYToBoundingBox(int tileX, int tileY, int zoom)
        {
            //Top left corner pixel coordinates
            int x1 = tileX * 256;
            int y1 = tileY * 256;

            //Bottom right corner pixel coordinates
            int x2 = x1 + 256;
            int y2 = y1 + 256;

            return new GeoboundingBox(
                GlobalPixelToCoordinate(x1, y1, zoom),
                GlobalPixelToCoordinate(x2, y2, zoom)
            );
        }
#endif
    }
}
