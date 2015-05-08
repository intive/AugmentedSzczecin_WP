using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Maps.SpatialToolbox
{
    /// <summary>
    /// A polygon consists of one or more rings. A ring is a connected sequence of four or more
    /// points that form a closed, non-self-intersecting loop. A polygon may contain multiple
    /// outer rings. The order of vertices or orientation for a ring indicates which side of the ring
    /// is the interior of the polygon. The neighborhood to the right of an observer walking along
    /// the ring in vertex order is the neighborhood inside the polygon. Vertices of rings defining
    /// holes in polygons are in a counterclockwise direction. Vertices for a single, ringed
    /// polygon are, therefore, always in clockwise order.
    /// 
    /// http://www.esri.com/library/whitepapers/pdfs/shapefile.pdf
    /// </summary>
    public class Polygon : Geometry
    {
        #region Constructor

        public Polygon()
        {
            this.InteriorRings = new List<CoordinateCollection>();
        }

        public Polygon(CoordinateCollection extriorRing)
        {
            this.ExteriorRing = extriorRing;
            this.InteriorRings = new List<CoordinateCollection>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// A CoordinateCollection object representing the Exterior ring of a Polygon
        /// </summary>
        public CoordinateCollection ExteriorRing { get; set; }

        /// <summary>
        /// List of CoordinateCollection objects representing the interior rings of a Polygon
        /// </summary>
        public List<CoordinateCollection> InteriorRings { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Verifies that a Polygon is valid. To be valid it must have at least 4 coordinates in each ring. 
        /// The first and last coordinate of each ring must be the same. Exterior rings coordinates must be 
        /// ordered counter clockwise and interior ring cooridnates clockwise.
        /// </summary>
        /// <returns>A boolean indicating if Polygon meets OGC standards</returns>
        public bool STIsValid()
        {
            if (!this.ExteriorRing.IsCCW())
            {
                return false;
            }

            foreach (CoordinateCollection coordinates in this.InteriorRings)
            {
                if (coordinates.IsCCW())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Makes a Polygon object valid if it is invalid
        /// </summary>
        public Task MakeValidAsync()
        {
            return Task.Run(() =>
            {
                if (this.ExteriorRing.Count > 0)
                {
                    //Ensure that the exterior rings start and end coordinate are the same
                    if (this.ExteriorRing[0] != this.ExteriorRing[this.ExteriorRing.Count - 1])
                    {
                        this.ExteriorRing.Add(this.ExteriorRing[0]);
                    }

                    //ensure that the exterior ring has at least three coordinates minimium is any
                    while (this.ExteriorRing.Count < 3)
                    {
                        this.ExteriorRing.Add(this.ExteriorRing[this.ExteriorRing.Count - 1]);
                    }
                }

                if (this.InteriorRings != null)
                {
                    foreach (var coordinates in this.InteriorRings)
                    {
                        if (coordinates[0] != coordinates[coordinates.Count - 1])
                        {
                            coordinates.Add(coordinates[0]);
                        }

                        //ensure that each ring has at least three coordinates minimium is any
                        while (coordinates.Count < 3)
                        {
                            coordinates.Add(coordinates[coordinates.Count - 1]);
                        }
                    }
                }

                EnsureRingOrientation();
            });
        }

        /// <summary>
        /// Ensures the extrior ring is in a counter clock wise orientation 
        /// and the interior rings are in a clock wise orientation.
        /// </summary>
        public void EnsureRingOrientation()
        {
            if (!this.ExteriorRing.IsCCW())
            {
                this.ExteriorRing.Reverse();
            }

            if (this.InteriorRings != null)
            {
                foreach (var coordinates in this.InteriorRings)
                {
                    if (coordinates.IsCCW())
                    {
                        coordinates.Reverse();
                    }
                }
            }
        }

        /// <summary>
        /// Calcualates the bounding box of a Polygon object
        /// </summary>
        /// <returns>A LocationRect object that represents the bounding box of a Polygon object</returns>
        public new BoundingBox Envelope()
        {
            return this.ExteriorRing.Envelope();
        }

        /// <summary>
        /// Determines if a coordinate is within a polygon.
        /// </summary>
        /// <param name="coordinate">Coordinate to query</param>
        /// <returns>A boolean indicating if a coordinate is contained inside the polygon.</returns>
        public bool ContainsCoordinate(Coordinate coordinate)
        {
            //TODO: Make spatially accurate
            if (CoordinateInRing(this.ExteriorRing, coordinate))
            {
                for (int i = 0; i < this.InteriorRings.Count; i++)
                {
                    if (!CoordinateInRing(this.InteriorRings[i], coordinate))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if a Point is within a polygon.
        /// </summary>
        /// <param name="point">Point to query</param>
        /// <returns>A boolean indicating if a Point is contained inside the polygon.</returns>
        public bool ContainsPoint(Point point)
        {
            return this.ContainsCoordinate(point.Coordinate);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determines if a Coordinate is within a ring. 
        /// 
        /// Algorithm based on: http://msdn.microsoft.com/en-us/library/cc451895.aspx
        /// </summary>
        /// <param name="cc">LocationCollection that makes up a ring</param>
        /// <param name="coord">Coordinate to search with</param>
        /// <returns>A boolean indicating if a coordinate is contained inside the LocationCollection.</returns>
        private bool CoordinateInRing(CoordinateCollection cc, Coordinate coord)
        {
            //TODO: Make spatially accurate
            int j = cc.Count - 1;
            bool inPoly = false;

            for (int i = 0; i < cc.Count; i++)
            {
                if (cc[i].Longitude < coord.Longitude && cc[j].Longitude >= coord.Longitude ||
                    cc[j].Longitude < coord.Longitude && cc[i].Longitude >= coord.Longitude)
                {
                    if (cc[i].Latitude +
                        (coord.Longitude - cc[i].Longitude)/(cc[j].Longitude - cc[i].Longitude)*
                        (cc[j].Latitude - cc[i].Latitude) < coord.Latitude)
                    {
                        inPoly = !inPoly;
                    }
                }

                j = i;
            }

            return inPoly;
        }

        /// <summary>
        /// Specifies the type of the geometry object as a GeometryType enumerator.
        /// </summary>
        /// <returns>A GeometryType enumerator</returns>
        public override GeometryType STGeometryType()
        {
            return GeometryType.Polygon;
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