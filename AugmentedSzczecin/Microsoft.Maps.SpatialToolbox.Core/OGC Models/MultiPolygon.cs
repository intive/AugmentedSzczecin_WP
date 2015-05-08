using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maps.SpatialToolbox
{
    /// <summary>
    /// Represents a collection of Polygons
    /// </summary>
    public class MultiPolygon : GeometryCollection<Polygon>
    {
        #region Constructor

        /// <summary>
        /// MultiPolygon Constructor
        /// </summary>
        public MultiPolygon()
        {
        }

        /// <summary>
        /// MultiPolygon Constructor
        /// </summary>
        /// <param name="polygons">List of Polygon objects</param>
        public MultiPolygon(IEnumerable<Polygon> polygons)
            : base(polygons)
        {
        }

        /// <summary>
        /// MultiPolygon Constructor
        /// </summary>
        /// <param name="capacity">Number of Polygon's in MultiPolygon</param>
        public MultiPolygon(int capacity)
            : base(capacity)
        {
        }

        #endregion

        #region Public Methods

        /// <summary> 
        /// Verifies that a MultiPolygon is valid. To be valid all of it's polygons must have at least 4 coordinates in each ring. 
        /// The first and last coordinate of each ring must be the same. Exterior rings coordinates must be 
        /// ordered counter clockwise and interior ring cooridnates clockwise.
        /// </summary>
        /// <returns>A boolean indicating if MultiPolygon meets OGC standards</returns>
        public bool STIsValid()
        {
            foreach (Polygon polygon in this.Geometries)
            {
                if (!polygon.STIsValid())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Makes a MultiPolygon object valid if it is invalid
        /// </summary>
        public async Task MakeValidAsync()
        {
            await Task.Run(async ()=>{
                try
                {
                    foreach (Polygon polygon in this.Geometries)
                    {
                        await polygon.MakeValidAsync();
                    }
                }
                catch { }
            });
        }

        /// <summary>
        /// Determines if a coordinate is within a polygon.
        /// </summary>
        /// <param name="coordinate">Coordinate to query</param>
        /// <returns>A boolean indicating if a coordinate is contained inside the polygon.</returns>
        public bool ContainsCoordinate(Coordinate coordinate)
        {
            for (int i = 0; i < this.Geometries.Count; i++)
            {
                if (this.Geometries[i].ContainsCoordinate(coordinate))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if a Point is within the MultiPolygon.
        /// </summary>
        /// <param name="point">Point to query</param>
        /// <returns>A boolean indicating if a Point is contained inside the MultiPolygon.</returns>
        public bool ContainsPoint(Point point)
        {
            for (int i = 0; i < this.Geometries.Count; i++)
            {
                if (this.Geometries[i].ContainsPoint(point))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Specifies the type of the geometry object as a GeometryType enumerator.
        /// </summary>
        /// <returns>A GeometryType enumerator</returns>
        public override GeometryType STGeometryType()
        {
            return GeometryType.MultiPolygon;
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
