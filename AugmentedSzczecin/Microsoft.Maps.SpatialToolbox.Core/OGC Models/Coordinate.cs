using System;

namespace Microsoft.Maps.SpatialToolbox
{
    /// <summary>
    /// Used to store a Geodesic Coordinate (X(Longitude), Y(Latitude), Z(Altitude)).
    /// </summary>
    public struct Coordinate
    {
        #region Private Properties

        private readonly double longitude;
        private readonly double latitude;
        private readonly double? altitude;

        #endregion

        #region Constructor

        /// <summary>
        /// Coordinate Object constructor
        /// </summary>
        /// <param name="latitude">Latitude coordinates.</param>
        /// <param name="longitude">Longitude coordinates.</param>        
        public Coordinate(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.altitude = null;
        }

        /// <summary>
        /// Coordinate Object constructor
        /// </summary>
        /// <param name="latitude">Latitude coordinates.</param>
        /// <param name="longitude">Longitude coordinates.</param> 
        /// <param name="altitude">Altitude</param>
        public Coordinate(double latitude, double longitude, double? altitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;            
            this.altitude = altitude;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Used to store Logitude coordinates.
        /// </summary>
        public double Longitude { get { return longitude; } }

        /// <summary>
        /// Used to store Latitude coordinates.
        /// </summary>
        public double Latitude { get { return latitude; } }

        ///<summary>
        /// Used to store altitude information
        ///</summary>
        public double? Altitude { get { return altitude; } }

        #endregion

        #region Public Methods

        #region Operator Definition

        /// <summary>
        /// A unique code that represents the data in this object
        /// </summary>
        /// <returns>A unique has representation of this object</returns>
        public override int GetHashCode()
        {
            return this.Altitude.GetHashCode() ^ this.Latitude.GetHashCode() ^ this.Longitude.GetHashCode();
        }

        /// <summary>
        /// Checks to see if two coordinates are equal
        /// </summary>
        /// <param name="p1">First Coordinate</param>
        /// <param name="p2">Second Coordinate</param>
        /// <returns>Boolean indicating if coordinates are equal</returns>
        public static bool operator ==(Coordinate p1, Coordinate p2)
        {
            if ((object)p2 == null && (object)p1 == null)
            {
                return true;
            }
            else if ((object)p2 == null || (object)p1 == null)
            {
                return false;
            }

            return (p1.Longitude == p2.Longitude) && (p1.Latitude == p2.Latitude) && (p1.Altitude == p2.Altitude);
        }

        /// <summary>
        /// Checks to see if two coordinates are equal
        /// </summary>
        /// <param name="p1">First Coordinate</param>
        /// <param name="p2">Second Coordinate</param>
        /// <returns>A boolean indicating if two coordinates are not equal</returns>
        public static bool operator !=(Coordinate p1, Coordinate p2)
        {
            return !(p1 == p2);
        }

        #endregion

        #region Coordinate Comparison Methods

        /// <summary>
        /// Checks to see if coordinate and an object are equal. The object must be a coordinate or inherited from it.
        /// </summary>
        /// <param name="obj">An object to compare with</param>
        /// <returns>A boolean indicating if the two coordinates are equal</returns>
        public override bool Equals(object obj)
        {
            return ((obj is Coordinate) && (this == ((Coordinate)obj)));
        }

        #endregion

        /// <summary>
        /// Calculates the Coordinate object that is on the completely opposite side of the globel. 
        /// If a straigh line is used to connect these original LatLong and the Inverse LatLong the 
        /// line will cross through the center of the Earth.
        /// </summary>
        /// <returns>Coordinate object that is on the opposite side of the globel. From this coordinate.</returns>
        public Coordinate GetInverseCoordinate()
        {
            //by multiplying the latitude by minus 1 we now have the altitude coordinate in the opposite hemisphere.
            double lat = -1 * Latitude;

            //by minusing 180 by the absolute value of the longitude we can calculate the absolute value of the new longitude
            double lon = 180 - Math.Abs(Longitude);

            //if the original longitude value is positive the inverse longitude value will be negative.
            if (Longitude > 0)
            {
                lon *= -1;
            }

            //return the inverse coordinate as a new LatLong object
            return new Coordinate(lat, lon);
        }

        /// <summary>
        /// Converts an invalid longitude value to be within the valid range, which is -180 to 180. 
        /// </summary>
        /// <param name="longitude">The longitude value to normalize.</param>
        /// <returns>A normalized longitude value.</returns>
        public static double NormalizeLongitude(double longitude)
        {
            if (!double.IsNaN(longitude))
            {
                if ((longitude >= -180.0) && (longitude <= 180.0))
                {
                    return longitude;
                }
                return (longitude - (Math.Floor((double)((longitude + 180.0) / 360.0)) * 360.0));
            }

            return double.NaN;
        }

        #endregion
    }
}
