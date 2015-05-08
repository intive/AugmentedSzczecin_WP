using System;

namespace Microsoft.Maps.SpatialToolbox
{
    /// <summary>
    /// Discribes a rectangular region. This region usually encloses a set of geometries or represents a area of view.
    /// </summary>
    public class BoundingBox : Geometry
    {
        #region Constructor

        /// <summary>
        /// Discribes a rectangular region. This region usually encloses a set of geometries or represents a area of view.
        /// </summary>
        /// <param name="center">Center of bounding box</param>
        /// <param name="width">Width of bounding box in degress</param>
        /// <param name="height">Height of bounding box in degress</param>
        public BoundingBox(Coordinate center, double width, double height)
        {
            Width = width;
            Height = height;
            Center = center;
        }

        /// <summary>
        /// Discribes a rectangular region. This region usually encloses a set of geometries or represents a area of view.
        /// </summary>
        /// <param name="minX">Mininium X value (longitude), left most coordinate.</param>
        /// <param name="maxY">Maximium Y value (laitude), northern most coordinate.</param>
        /// <param name="maxX">Maximium X value (longitude), right most coordinate.</param>
        /// <param name="minY">Minimium Y value (latitude), southern most coordinate.</param>
        public BoundingBox(double minX, double maxY, double maxX, double minY)
        {
            Height = maxY - minY;
            Width = (maxX - minX)%360;

            var cLat = maxY - Height/2;
            var cLon = minX + Width/2;

            if (cLon > 180)
            {
                cLon -= 360;
            }
            else if (cLon < -180)
            {
                cLon += 360;
            }

            Center = new Coordinate(cLat, cLon);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// North Latitude Coordinate
        /// </summary>
        public double MaxY
        {
            get { return Center.Latitude + Height/2; }
        }

        /// <summary>
        /// South Latitude Coordinate
        /// </summary>
        public double MinY
        {
            get { return Center.Latitude - Height/2; }
        }

        /// <summary>
        /// Most Easterly Longitude Coordinate (right side of bounding box)
        /// </summary>
        public double MaxX
        {
            get { return Center.Longitude + Width/2; }
        }

        /// <summary>
        /// Most Westerly Longitude Coordinate (left side of bounding box)
        /// </summary>
        public double MinX
        {
            get { return Center.Longitude - Width/2; }
        }

        /// <summary>
        /// Width of the bounding box in degress
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Height of the bounding box in degress
        /// </summary>
        public double Height { get; set; }

        public Coordinate Center { get; set; }

        /// <summary>
        /// Gets the top left coordinate of the bounding box
        /// </summary>
        public Coordinate TopLeft
        {
            get { return new Coordinate(this.MaxY, this.MinX); }
        }

        /// <summary>
        /// Gets the bottom right coordinate of the bounding box
        /// </summary>
        public Coordinate BottomRight
        {
            get { return new Coordinate(this.MinY, this.MaxX); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Joins two BoundingBox objects together to create a new BoundingBox object that encompasses both original BoundingBox's.
        /// </summary>
        /// <param name="boundingBox">BoundingBox object to join with.</param>
        /// <returns>A new BoundingBox object the encompasses the two original BoundingBox objects.</returns>
        public BoundingBox Join(BoundingBox boundingBox)
        {
            if (boundingBox == null)
            {
                return this;
            }

            //Verify both BoundingBox's are not empty
            if (double.IsNaN(this.Center.Longitude) || double.IsNaN(this.Center.Latitude))
            {
                return boundingBox;
            }

            if (double.IsNaN(boundingBox.Center.Longitude) || double.IsNaN(boundingBox.Center.Latitude))
            {
                return this;
            }

            double top = Math.Max(this.MaxY, boundingBox.MaxY);
            double bottom = Math.Min(this.MinY, boundingBox.MinY);
            double height = top - bottom;
            double lat = top - height/2;

            double right = Math.Max(this.MaxX, boundingBox.MaxX);
            double left = Math.Min(this.MinX, boundingBox.MinX);
            double width = (right - left)%360;

            double lon = left + width/2;

            if (lon > 180)
            {
                lon -= 360;
            }
            else if (lon < -180)
            {
                lon += 360;
            }

            return new BoundingBox(new Coordinate(lat, lon), width, height);
        }

        /// <summary>
        /// Determines if a coordinate is inside a bounding box.
        /// </summary>
        /// <param name="coordinate">A coordinate to check with</param>
        /// <returns>A boolean indicating is the bounding box contains a coordinate</returns>
        public bool Contains(Coordinate coordinate)
        {
            //TODO: Make spatially accurate
            var dy = Math.Abs(coordinate.Latitude - Center.Latitude);
            var dx = Math.Abs(coordinate.Longitude - Center.Longitude);

            if (dx > 180)
            {
                dx -= 360;
            }
            else if (dx < -180)
            {
                dx += 360;
            }

            return (dx <= Width/2 && dy <= Height/2);
        }

        /// <summary>
        /// Determines if a bounding box is inside a bounding box.
        /// </summary>
        /// <param name="boundingBox">A bounding box to check with</param>
        /// <returns>A boolean indicating is the bounding box contains another bounding box</returns>
        public bool Contains(BoundingBox boundingBox)
        {
            return this.Contains(boundingBox.TopLeft) && this.Contains(boundingBox.BottomRight);
        }

        /// <summary>
        /// Determines whether this bounding box intersects with the specified bounding box.
        /// </summary>
        /// <param name="rect">The bounding box to test against.</param>
        /// <returns>A boolean indicating if the two bounding boxes intersect</returns>
        public bool Intersects(BoundingBox rect)
        {
            double height = Math.Abs((double) (this.Center.Latitude - rect.Center.Latitude));
            double width = Math.Abs((double) (this.Center.Longitude - rect.Center.Longitude));
            if (width > 180.0)
            {
                width = 360.0 - width;
            }
            return ((height <= (this.Height/2 + rect.Height/2)) && (width <= (this.Width/2 + rect.Width/2)));
        }

        /// <summary>
        /// Calculates the area of intersection of two BoundingBox's. 
        /// </summary>
        /// <param name="rect">The base BoundingBox.</param>
        /// <param name="rect2">A BoundingBox to calculate the intersection of.</param>
        /// <returns>A BoundingBox of the area of intersection or null.</returns>
        public BoundingBox Intersection(BoundingBox rect2)
        {
            if (this.Intersects(rect2))
            {
                double left = this.Center.Longitude - this.Width/2;
                double left2 = rect2.Center.Longitude - rect2.Width/2;
                double right = this.Center.Longitude + this.Width/2;
                double right2 = rect2.Center.Longitude + rect2.Width/2;

                if (Math.Abs((double) (this.Center.Longitude - rect2.Center.Longitude)) > 180.0)
                {
                    if (this.Center.Longitude < rect2.Center.Longitude)
                    {
                        left += 360.0;
                        right += 360.0;
                    }
                    else
                    {
                        left2 += 360.0;
                        right2 += 360.0;
                    }
                }

                double closestLeft = Math.Max(left, left2);
                double closestRight = Math.Min(right, right2);
                double closestNorth = Math.Min(this.MaxY, rect2.MaxY);
                double closestSouth = Math.Max(this.MinY, rect2.MinY);

                return
                    new BoundingBox(
                        new Coordinate((closestNorth + closestSouth)/2.0,
                            Coordinate.NormalizeLongitude((closestLeft + closestRight)/2.0)), closestRight - closestLeft,
                        closestNorth - closestSouth);
            }

            return null;
        }

        /// <summary>
        /// Specifies the type of the geometry object as a GeometryType enumerator.
        /// </summary>
        /// <returns>A GeometryType enumerator</returns>
        public override GeometryType STGeometryType()
        {
            return GeometryType.BoundingBox;
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