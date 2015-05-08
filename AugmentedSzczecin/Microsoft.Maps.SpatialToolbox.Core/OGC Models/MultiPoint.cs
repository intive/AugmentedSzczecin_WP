using System.Collections.Generic;

namespace Microsoft.Maps.SpatialToolbox
{
    /// <summary>
    /// A MultiPoint is a 0-dimensional GeometryCollection. The elements of a MultiPoint are restricted to Points. The
    /// Points are not connected or ordered in any semantically important way.
    /// 
    /// A MultiPoint is simple if no two Points in the MultiPoint are equal (have identical coordinate values Latitude/Longitude).
    /// 
    /// http://www.opengeospatial.org/standards/sfa
    /// </summary>
    public class MultiPoint : GeometryCollection<Point>
    {
        #region Constructor

        /// <summary>
        /// MultiPoint Constructor
        /// </summary>
        public MultiPoint()
        {
        }

        /// <summary>
        /// MultiPoint Constructor
        /// </summary>
        /// <param name="points">List of Point objects</param>
        public MultiPoint(IEnumerable<Point> points)
            : base(points)
        {
        }


        /// <summary>
        /// MultiPoint Constructor
        /// </summary>
        /// <param name="capacity">Number of Point's in MultiPoint</param>
        public MultiPoint(int capacity)
            : base(capacity)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks that the Geometry is valid
        /// </summary>
        public bool IsValid
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns the coordinate at the given position.
        /// </summary>
        /// <param name="n">The index of the coordinate to retrieve, beginning at 0.
        /// </param>
        /// <returns>The n'th coordinate.</returns>
        public Coordinate GetCoordinate(int n)
        {
            return base.Geometries[n].Coordinate;
        }

        /// <summary>
        /// Specifies the type of the geometry object as a GeometryType enumerator.
        /// </summary>
        /// <returns>A GeometryType enumerator</returns>
        public override GeometryType STGeometryType()
        {
            return GeometryType.MultiPoint;
        }

        /// <summary>
        /// Returns the number of child geometries in the shape.
        /// </summary>
        /// <returns>Number of child geometries in shape.</returns>
        public override int STNumGeometries()
        {
            return this.Geometries.Count;
        }

        #endregion
    }
}
