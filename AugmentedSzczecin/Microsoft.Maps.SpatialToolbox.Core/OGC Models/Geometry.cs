/*
 * Based on the OGC Simple Features: http://www.opengeospatial.org/standards/sfa
 */

using Microsoft.Maps.SpatialToolbox.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Maps.SpatialToolbox
{
    /// <summary>
    /// A general class that is inherited by all simple shapes (Point, LineString, Polygon)
    /// </summary>
    public abstract class Geometry
    {
        #region Constructor

        /// <summary>
        /// Geometry object constructor. Base object class for a geospatial shape.
        /// </summary>
        protected Geometry()
        {
            Metadata = new ShapeMetadata();
        }

        #endregion

        #region Public Properties

        ///// <summary>
        ///// A unique identifier for the Geometry
        ///// </summary>
        //public string ID { get; set; }

        /// <summary>
        /// A spatial reference identifier for the data.
        /// </summary>
        public string SRID { get; set; }

        /// <summary>
        /// An array of metadata related to an item
        /// </summary>
        public ShapeMetadata Metadata { get; set; }

        /// <summary>
        /// A unique identifier used for referencing in styles
        /// </summary>
        public string StyleKey { get; set; }

        #endregion

        #region Public Methods

        #region Override Methods

        /// <summary>
        /// Converts a Geometry object into Well Known Text (WKT)
        /// </summary>
        /// <returns>Returns a well known text version of the Geometry object</returns>
        public override string ToString()
        {
            return IO.WellKnownText.Write(this, Is3D());
        }

        #endregion

        #region Data Converters

        /// <summary>
        /// Converts a Geometry object into Well Known Text (WKT)
        /// </summary>
        /// <returns>Returns a well known text version of the Geometry object</returns>
        public string STAsText()
        {
            return this.ToString();
        }

        /// <summary>
        /// Generates the Well Known Binary (WKB) version of a Geometry
        /// </summary>
        /// <returns>Well Known Binary (WKB) representation of a Geometry object</returns>
        public byte[] ToBinary()
        {
            return IO.WellKnownBinary.Write(this);
        }

        /// <summary>
        /// Generates the Well Known Binary (WKB) version of a Geometry
        /// </summary>
        /// <returns>Well Known Binary (WKB) representation of a Geometry object</returns>
        public byte[] STAsBinary()
        {
            return this.ToBinary();
        }

        #endregion

        #region Geospatial Methods

        /// <summary>
        /// Returns an approximation of the given Geometry instance produced by 
        /// running polylines and polygons through the vertex reduction algorithm. 
        /// 
        /// The following Geometries are not reduced:
        ///  - Point
        ///  - MultiPoint
        ///  - BoundingBox
        /// </summary>
        /// <param name="tolerance">
        /// A tolerance is the closest distnace any two points can be 
        /// to each other. The tolerance must be a positive number.
        /// </param>
        public async Task Reduce(double tolerance)
        {
            if (this is LineString)
            {
                LineString l = this as LineString;
                l.Vertices = await SpatialTools.VertexReductionAsync(l.Vertices, tolerance);
            }
            else if (this is Polygon)
            {
                Polygon p = this as Polygon;

                //reduce the extrior ring
                p.ExteriorRing = await SpatialTools.VertexReductionAsync(p.ExteriorRing, tolerance);

                //Reduce the interior rings
                for (int i = 0; i < p.InteriorRings.Count; i++)
                {
                    p.InteriorRings[i] = await SpatialTools.VertexReductionAsync(p.InteriorRings[i], tolerance);
                }

                //Makes sure polygon is valid
                await p.MakeValidAsync();
            }
            else if (this is MultiLineString)
            {
                MultiLineString ml = this as MultiLineString;

                for(int i = 0; i < ml.Geometries.Count; i++)
                {
                    ml.Geometries[i].Vertices = await SpatialTools.VertexReductionAsync(ml.Geometries[i].Vertices, tolerance);
                }
            }
            else if (this is MultiPolygon)
            {
                MultiPolygon mp = this as MultiPolygon;

                for (int i = 0; i < mp.Geometries.Count; i++)
                {
                    //reduce the extrior ring
                    mp.Geometries[i].ExteriorRing = await SpatialTools.VertexReductionAsync(mp.Geometries[i].ExteriorRing, tolerance);

                    //Reduce the interior rings
                    for (int j = 0; j < mp.Geometries[i].InteriorRings.Count; j++)
                    {
                        mp.Geometries[i].InteriorRings[j] = await SpatialTools.VertexReductionAsync(mp.Geometries[i].InteriorRings[j], tolerance);
                    }

                    //Makes sure polygon is valid
                    await mp.Geometries[i].MakeValidAsync();
                }
            }
            else if (this is GeometryCollection)
            {
                GeometryCollection gc = this as GeometryCollection;

                foreach (Geometry g in gc)
                {
                    await g.Reduce(tolerance);
                }
            }
        }

        /// <summary>
        /// Calculates the number of coordinates in a Geometry object.
        /// </summary>
        /// <returns>0 if the Geometry is null, otherwise the number of coordinates in the Geometry</returns>
        public int STNumPoints()
        {
            int count = 0;

            if (this == null)
            {
                return count;
            }
            else if (this is Point)
            {
                return 1;
            }
            else if (this is MultiPoint)
            {
                return (this as MultiPoint).Geometries.Count;
            }
            else if (this is LineString)
            {
                return (this as LineString).Vertices.Count;
            }
            else if (this is Polygon)
            {
                count = (this as Polygon).ExteriorRing.Count;

                foreach (var coordinates in (this as Polygon).InteriorRings)
                {
                    count += coordinates.Count;
                }

                return count;
            }
            else if (this is MultiLineString)
            {
                foreach (var l in (this as MultiLineString).Geometries)
                {
                    count += l.STNumPoints();
                }
            }
            else if (this is MultiPolygon)
            {
                foreach (var p in (this as MultiPolygon).Geometries)
                {
                    count += p.STNumPoints();
                }
            }
            else if (this is GeometryCollection)
            {
                foreach (var Geometry in (this as GeometryCollection).Geometries)
                {
                    count += Geometry.STNumPoints();
                }
            }

            return count;
        }

        /// <summary>
        /// Calculates the envelope (bounding box) of a Geometry object
        /// </summary>
        /// <returns>A BoundingBox object that represents the bounding box of a Geometry object</returns>
        public BoundingBox Envelope()
        {
            if (this is Point)
            {
                return (this as Point).Envelope();
            }
            else if (this is LineString)
            {
                return (this as LineString).Envelope();
            }
            else if (this is Polygon)
            {
                return (this as Polygon).Envelope();
            }
            else if (this is MultiPoint)
            {
                return (this as MultiPoint).Envelope();
            }
            else if (this is MultiLineString)
            {
                return (this as MultiLineString).Envelope();
            }
            else if (this is MultiPolygon)
            {
                return (this as MultiPolygon).Envelope();
            }
            else if (!(this is GeometryCollection))
            {
                throw new Exception("Gemetry type not recognized");
            }

            return (this as GeometryCollection).Envelope();
        }

        /// <summary>
        /// Checks first coordinate in Geography to see if it contains a valid altitude
        /// </summary>
        /// <returns>A boolean indicating if the Shape has a valid altitude</returns>
        public bool Is3D()
        {
            if (this is Point)
            {
                return (this as Point).Coordinate.Altitude.HasValue;
            }
            else if (this is LineString)
            {
                LineString l = (this as LineString);

                if (l.Vertices.Count > 0)
                {
                    return l.Vertices[0].Altitude.HasValue;
                }

                return false;
            }
            else if (this is Polygon)
            {
                Polygon p = (this as Polygon);

                if (p.ExteriorRing.Count > 0)
                {
                    return p.ExteriorRing[0].Altitude.HasValue;
                }

                return false;
            }
            else if (this is MultiPoint)
            {
                MultiPoint mp = (this as MultiPoint);

                if (mp.Geometries.Count > 0)
                {
                    return mp.Geometries[0].Coordinate.Altitude.HasValue;
                }

                return false;
            }
            else if (this is MultiLineString)
            {
                MultiLineString ml = (this as MultiLineString);

                if (ml.Geometries.Count > 0 && ml.Geometries[0].Vertices.Count > 0)
                {
                    return ml.Geometries[0].Vertices[0].Altitude.HasValue;
                }

                return false;
            }
            else if (this is MultiPolygon)
            {
                MultiPolygon mp = (this as MultiPolygon);

                if (mp.Geometries.Count > 0 && mp.Geometries[0].ExteriorRing.Count > 0)
                {
                    return mp.Geometries[0].ExteriorRing[0].Altitude.HasValue;
                }

                return false;
            }
            else if (this is GeometryCollection)
            {
                var gc = (this as GeometryCollection);

                if (gc.Geometries.Count > 0)
                {
                    return gc.Geometries[0].Is3D();
                }

                return false;
            }

            return false;
        }

        #endregion

        #endregion

        #region Abstract Methds

        /// <summary>
        /// Specifies the type of the geometry object as a GeometryType enumerator.
        /// </summary>
        /// <returns>A GeometryType enumerator</returns>
        public abstract GeometryType STGeometryType(); 

        /// <summary>
        /// Returns the number of child geometries in the shape.
        /// </summary>
        /// <returns>Number of child geometries in shape.</returns>
        public abstract int STNumGeometries();

        #endregion
    }
}
