using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maps.SpatialToolbox
{
    /// <summary>
    /// Represents a List of Coordinates
    /// </summary>
    public class CoordinateCollection : List<Coordinate>
    {
        #region Constructors

        /// <summary>
        /// List of Coordinate Objects
        /// </summary>
        public CoordinateCollection()
        {
        }

        /// <summary>
        /// List of Coordinate Objects
        /// </summary>
        /// <param name="coordinates">List of Coordinate Objects</param>
        public CoordinateCollection(IEnumerable<Coordinate> coordinates)
            : base(coordinates)
        {
        }

        /// <summary>
        /// List of Coordinate Objects
        /// </summary>
        /// <param name="capacity">Capcity of Coordinate List</param>
        public CoordinateCollection(int capacity)
            : base(capacity)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines if the coordinates in a CoordinateCollection are in a counter clockwise order. 
        /// This is important when performing certain calculates and to ensure compatibility with OGC standards.
        /// </summary>
        /// <returns>A boolean indicating if the coordinates are in a counter clockwise order</returns>
        public bool IsCCW()
        {
            int count = this.Count;

            //Ensure that there are at least 4 coordinates. 
            //Polygons in SQL 2008 must start and end with the same coordinate. 
            //As such they require a minium of 4 coordinates to be valid.
            if (count < 4)
            {
                return false;
            }

            Coordinate coordinate = this[0];
            int index1 = 0;

            for (int i = 1; i < count; i++)
            {
                Coordinate coordinate2 = this[i];
                if (coordinate2.Latitude > coordinate.Latitude)
                {
                    coordinate = coordinate2;
                    index1 = i;
                }
            }

            int num4 = index1 - 1;

            if (num4 < 0)
            {
                num4 = count - 2;
            }

            int num5 = index1 + 1;

            if (num5 >= count)
            {
                num5 = 1;
            }

            Coordinate coordinate3 = this[num4];
            Coordinate coordinate4 = this[num5];

            double num6 = ((coordinate4.Longitude - coordinate.Longitude) * (coordinate3.Latitude - coordinate.Latitude)) -
                ((coordinate4.Latitude - coordinate.Latitude) * (coordinate3.Longitude - coordinate.Longitude));

            if (num6 == 0.0)
            {
                return (coordinate3.Longitude > coordinate4.Longitude);
            }

            return (num6 > 0.0);
        }

        /// <summary>
        /// Calculates the bounding box of a collection of coordinates
        /// </summary>
        /// <returns>A bounding box of a CoordinateCollection object as a BoundingBox object</returns>
        public BoundingBox Envelope()
        {
            if (Count == 1)
            {
                return new BoundingBox(this[0], 0.001, 0.001);
            }
            else if (this.Count > 0)
            {
                //var bb = new BoundingBox(this[0], 0.001, 0.001);

                //for (int i = 1; i < Count; i++)
                //{
                //    bb.Include(this[i]);
                //}

                //return bb;
                var minX = this.Min(c => c.Longitude);
                var minY = this.Min(c => c.Latitude);
                var maxX = this.Max(c => c.Longitude);
                var maxY = this.Max(c => c.Latitude);

                return new BoundingBox(minX, maxY, maxX, minY);
            }

            return null;
        }

        #endregion
    }
}
