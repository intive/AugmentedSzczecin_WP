#if WINDOWS_APP
using Bing.Maps;
#elif WPF
using Microsoft.Maps.MapControl.WPF;
#elif WINDOWS_PHONE
using System.Collections.Generic;
using System.Device.Location;
#elif WINDOWS_PHONE_APP
using System.Collections.Generic;
using Windows.Devices.Geolocation;
#endif

namespace Microsoft.Maps.SpatialToolbox.Bing
{
    /// <summary>
    /// This class defines the position of a map tile. 
    /// </summary>
    public class QuadKey
    {
        #region Private Properties

        private string _key;
        private int _x, _y, _z;

        #if WINDOWS_APP || WPF
        internal LocationCollection _locations;
        #elif WINDOWS_PHONE
        internal List<GeoCoordinate> _locations;
        #elif WINDOWS_PHONE_APP
        internal GeoboundingBox _locations;
        #endif

        #endregion

        #region Constructor

        /// <summary>
        /// This class defines the position of a map tile. 
        /// </summary>
        /// <param name="key">Quadkey string value.</param>
        public QuadKey(string key)
        {
            _key = key;
            UpdateXYZoom();
        }

        /// <summary>
        /// This class defines the position of a map tile. 
        /// </summary>
        /// <param name="x">Tile X position.</param>
        /// <param name="y">Tile Y position.</param>
        /// <param name="zoom">Zoom level.</param>
        public QuadKey(int x, int y, int zoom)
        {
            _x = x;
            _y = y;
            _z = zoom;
            UpdateKey();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// String QuadKey value.
        /// </summary>
        public string Key { get { return _key; } }

        /// <summary>
        /// Tiles X position.
        /// </summary>
        public int X { get { return _x; } }

        /// <summary>
        /// Tiles Y position.
        /// </summary>
        public int Y { get { return _y; } }

        /// <summary>
        /// Zoom level of tile.
        /// </summary>
        public int ZoomLevel { get { return _z; } }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a string representation of a QuadKey.
        /// </summary>
        /// <returns>QuadKey as a string.</returns>
        public override string ToString()
        {
            return _key;
        }

        /// <summary>
        /// Determines if two quadkey values are the same.
        /// </summary>
        /// <param name="obj">Comparision value, QuadKey or string</param>
        /// <returns>A boolean indicating if the QuadKey or string value are equal to the QuadKey value.</returns>
        public override bool Equals(object obj)
        {
            if (obj is QuadKey)
            {
                return this._key.Equals((obj as QuadKey).Key);
            }
            else if (obj is string)
            {
                return this._key.Equals(obj as string);
            }

            return false;
        }

        #endregion

        #region Private Methds

        private void UpdateXYZoom()
        {
            TileMath.QuadKeyToTileXY(_key, out _x, out _y, out _z);

            var rect = TileMath.QuadKeyToBoundingBox(this);

            #if WINDOWS_APP || WPF
            _locations = new LocationCollection()
            {
                rect.Northwest,
                new Location(rect.North, rect.East),
                rect.Southeast,
                new Location(rect.South, rect.West),
            };
            #elif WINDOWS_PHONE || WINDOWS_PHONE_APP
            _locations = rect;
            #endif              
        }

        private void UpdateKey()
        {
            _key = TileMath.TileXYToQuadKey(_x, _y, _z);

            var rect = TileMath.QuadKeyToBoundingBox(this);
            #if WINDOWS_APP || WPF
            _locations = new LocationCollection()
            {
                rect.Northwest,
                new Location(rect.North, rect.East),
                rect.Southeast,
                new Location(rect.South, rect.West),
            };
            #elif WINDOWS_PHONE || WINDOWS_PHONE_APP
            _locations = rect;
            #endif  
        }

        #endregion
    }
}