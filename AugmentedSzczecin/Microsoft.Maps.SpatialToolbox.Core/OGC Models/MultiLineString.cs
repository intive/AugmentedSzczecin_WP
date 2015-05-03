using System.Collections.Generic;

namespace Microsoft.Maps.SpatialToolbox
{
    /// <summary>
    /// A MultiLineString is a collection of LineStrings.
    /// </summary>
    public class MultiLineString : GeometryCollection<LineString>
    {
        #region Constructor 

        /// <summary>
        /// MultiLineString Constructor
        /// </summary>
        public MultiLineString()
        {
        }

        /// <summary>
        /// MultiLineString Constructor
        /// </summary>
        /// <param name="lines">List of LineString objects</param>
        public MultiLineString(IEnumerable<LineString> lines)
            : base(lines)
        {
        }

        /// <summary>
        /// MultiLineString Constructor
        /// </summary>
        /// <param name="capacity">Number of LineStrings in MultiLineString</param>
        public MultiLineString(int capacity)
            : base(capacity)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Verifies that a MultiLineString is valid. To be valid all of it's LineStrings must have at least 2 coordinates in each ring. 
        /// </summary>
        /// <returns>A boolean indicating if the MultiLineString meets OGC standards</returns>
        public bool STIsValid()
        {
            foreach (LineString lineString in this.Geometries)
            {
                if (!lineString.STIsValid())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Specifies the type of the geometry object as a GeometryType enumerator.
        /// </summary>
        /// <returns>A GeometryType enumerator</returns>
        public override GeometryType STGeometryType()
        {
            return GeometryType.MultiLineString;
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
