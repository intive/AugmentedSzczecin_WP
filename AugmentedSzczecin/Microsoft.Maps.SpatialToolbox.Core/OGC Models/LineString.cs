namespace Microsoft.Maps.SpatialToolbox
{
    /// <summary>
    /// A LineString is an ordered set of vertices. Often refered to as a Polyline.
    /// </summary>
    public class LineString : Geometry
    {
        #region Constructor

        /// <summary>
        /// LineString Constructor
        /// </summary>
        public LineString()
        {
            this.Vertices = new CoordinateCollection();
        }

        /// <summary>
        /// LineString Constructor
        /// </summary>
        /// <param name="vertices">Collection of coordinate vertices</param>
        public LineString(CoordinateCollection vertices)
        {
            this.Vertices = vertices;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// A coordinate collection that represents the vertices of the LineString
        /// </summary>
        public CoordinateCollection Vertices { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Verifies that a LineString is valid. To be valid it must have at least 2 coordinates in each ring. 
        /// </summary>
        /// <returns>A boolean indicating if the LineString meets OGC standards</returns>
        public bool STIsValid()
        {
            if (this.Vertices.Count < 2)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Calcualates the bounding box of a LineString object
        /// </summary>
        /// <returns>A BoundingBox object that represents the bounding box of a LineString object</returns>
        public new BoundingBox Envelope()
        {
            return this.Vertices.Envelope();
        }

        /// <summary>
        /// Specifies the type of the geometry object as a GeometryType enumerator.
        /// </summary>
        /// <returns>A GeometryType enumerator</returns>
        public override GeometryType STGeometryType()
        {
            return GeometryType.LineString;
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