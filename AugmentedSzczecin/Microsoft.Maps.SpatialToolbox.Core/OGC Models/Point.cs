namespace Microsoft.Maps.SpatialToolbox
{
    /// <summary>
    /// A Point is a 0-dimensional geometric object and represents a single location in coordinate space. A Point has an
    /// Latitude coordinate value, a longitude coordinate value. 
    /// 
    /// http://www.opengeospatial.org/standards/sfa
    /// </summary>
    public class Point : Geometry
    {
        #region Constructors

        /// <summary>
        /// Coorindate Point
        /// </summary>
        public Point()
            : this(double.NaN, double.NaN, null)
        {
        }

        /// <summary>
        /// Coorindate Point
        /// </summary>
        /// <param name="latitude">Latitude value</param>  
        /// <param name="longitude">Longitude value</param>        
        public Point(double latitude, double longitude)
            : this(latitude, longitude, null)
        {
        }

        /// <summary>
        /// Coorindate Point
        /// </summary>
        /// <param name="coordinate">Coordinate of point</param>
        public Point(Coordinate coordinate)
            : this(coordinate.Latitude, coordinate.Longitude, null)
        {
        }

        /// <summary>
        /// Coorindate Point
        /// </summary>
        /// <param name="latitude">Latitude value</param>  
        /// <param name="longitude">Longitude value</param>       
        /// <param name="altitude">Altitude</param>
        public Point(double latitude, double longitude, double? altitude)
        {
            this.Coordinate = new Coordinate(latitude, longitude, altitude);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Coordinate object of Point
        /// </summary>
        public Coordinate Coordinate { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Calcualates the bounding box of a Point object
        /// </summary>
        /// <returns>A BoundingBox object that represents the bounding box of a Point object</returns>
        public new BoundingBox Envelope()
        {
            return new BoundingBox(Coordinate, 0.001, 0.001);
        }

        /// <summary>
        /// Verifies if the Point is valid. Checks that the coordinate has valid info. 
        /// Latitude is between -90 and 90, Longitude is between -180 and 180.
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            var lon = Coordinate.NormalizeLongitude(Coordinate.Longitude);
            return Coordinate != null && !double.IsNaN(Coordinate.Latitude) && !double.IsNaN(lon)
                   && Coordinate.Latitude <= 90 && Coordinate.Latitude >= -90
                   && lon <= 180 && lon >= -180;
        }

        /// <summary>
        /// Specifies the type of the geometry object as a GeometryType enumerator.
        /// </summary>
        /// <returns>A GeometryType enumerator</returns>
        public override GeometryType STGeometryType()
        {
            return GeometryType.Point;
        }

        /// <summary>
        /// Returns the number of child geometries in the shape.
        /// </summary>
        /// <returns>Number of child geometries in shape.</returns>
        public override int STNumGeometries()
        {
            return 1;
        }

        #endregion
    }
}