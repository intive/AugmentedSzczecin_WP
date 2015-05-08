/**************************************************************************** 
 * Author: Ricky Brundritt
 * 
 * Description:
 * Reads and writes Well Known Text (WKT) using a Geometry object
 *
 * See Also:
 * http://en.wikipedia.org/wiki/Well-known_text
 * http://www.opengeospatial.org/standards/sfa
 * 
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Maps.SpatialToolbox.IO
{
    /// <summary>
    /// Reads and writes Well Known Text (WKT) using a Geometry object
    /// </summary>
    public class WellKnownText : BaseFeed
    {
        #region Private Properties

        private static Regex _coordRx = new Regex(@"([0-9\.-]+)");
        private static Regex _singleShapeRx = new Regex(@"\),\s*\(");
        private static Regex _multiShapeRx = new Regex(@"\)\),\s*\(\(");

        private static Regex _collectionRx = new Regex(@"GEOMETRYCOLLECTION\(", RegexOptions.IgnoreCase);
        private static Regex _shapeDelimiterRx = new Regex(@"(\)),\s*([a-zA-Z])");

        private static char[] _commonDelimiter = new char[] { ',' };
        private static char[] _pipeDelimiter = new char[] { '|' };

        #endregion

        #region Constructor

        public WellKnownText()
        {
        }

        public WellKnownText(double tolerance)
            : base(tolerance)
        {            
        }

        #endregion

        #region Public Methods

        public async Task<Geometry> Read(string wellKnownText)
        {
            return await ParseGeometry(wellKnownText);
        }

        /// <summary>
        /// Reads a  Well Known Text from a string as a Geometry
        /// </summary>
        /// <param name="wellKnownText">Well Known Text string to read.</param>
        /// <returns>A Geometry object.</returns>
        public Task<Geometry> ReadAsync(string wellKnownText)
        {
            return Task.Run<Geometry>(() =>
            {
                return ParseGeometry(wellKnownText);
            });
        }

        /// <summary>
        /// Reads a  Well Known Text items from a stream. One item per line.
        /// </summary>
        /// <param name="stream">Stream to read.</param>
        /// <returns>A spatial data set.</returns>
        public override Task<SpatialDataSet> ReadAsync(Stream stream)
        {
            return Task.Run<SpatialDataSet>(async () =>
            {
                try
                {
                    using (var reader = new StreamReader(stream))
                    {
                        string line;
                        var geoms = new List<Geometry>();

                        while ((line = reader.ReadLine()) != null)
                        {
                            var geom = await Read(line);
                            if (geom != null)
                            {
                                geoms.Add(geom);
                            }
                        }

                        return new SpatialDataSet()
                        {
                            Geometries = geoms,
                            BoundingBox = geoms.Envelope()
                        };
                    }
                }
                catch { }

                return null;
            });
        }

        public static string Write(Geometry Geometry)
        {
            return Write(Geometry, Geometry.Is3D());
        }

        public static string Write(Geometry Geometry, bool includeAltitude)
        {
            using (var writer = new StringWriter())
            {
                WriteGeometry(Geometry, writer, includeAltitude);
                return writer.ToString();
            }
        }

        public override Task WriteAsync(SpatialDataSet data, Stream stream)
        {
            return Task.Run(() =>
            {
                using (var writer = new StreamWriter(stream))
                {
                    if (data.Geometries != null)
                    {
                        foreach (var g in data.Geometries)
                        {
                            writer.WriteLine(Write(g));
                        }
                    }
                }
            });
        }

        #endregion

        #region WKT Reader Methods

        #region Geometry Readers

        /// <summary>
        /// Creates a Geometry object from a WKT
        /// </summary>
        private async Task<Geometry> ParseGeometry(string wkt)
        {
            Geometry geometry = null;

            if (wkt.StartsWith("POINT", System.StringComparison.OrdinalIgnoreCase))
            {
                geometry = ParsePoint(wkt);
            }
            else if (wkt.StartsWith("LINESTRING", System.StringComparison.OrdinalIgnoreCase))
            {
                geometry = await ParseLineString(wkt);
            }
            else if (wkt.StartsWith("POLYGON", System.StringComparison.OrdinalIgnoreCase))
            {
                geometry = await ParsePolygon(wkt);
            }
            else if (wkt.StartsWith("MULTIPOINT", System.StringComparison.OrdinalIgnoreCase))
            {
                geometry = await ParseMultiPoint(wkt);
            }
            else if (wkt.StartsWith("MULTILINESTRING", System.StringComparison.OrdinalIgnoreCase))
            {
                geometry = await ParseMultiLineString(wkt);
            }
            else if (wkt.StartsWith("MULTIPOLYGON", System.StringComparison.OrdinalIgnoreCase))
            {
                geometry = await ParseMultiPolygon(wkt);
            }
            else if (wkt.StartsWith("GEOMETRYCOLLECTION", System.StringComparison.OrdinalIgnoreCase))
            {
                geometry = await ParseGeometryCollection(wkt);
            }

            return geometry;
        }

        private Coordinate? ParseCoord(string wkt)
        {
            double lat = double.NaN, lon = double.NaN, alt = double.NaN, temp;

            int matchCount = 0;
            
            for (var m = _coordRx.Match(wkt); m.Success; m = m.NextMatch())
            {
                if (double.TryParse(m.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out temp))
                {
                    switch (matchCount)
                    {
                        case 0:
                            lon = temp;
                            break;
                        case 1:
                            lat = temp;
                            break;
                        case 2:
                            alt = temp;
                            break;
                        default:
                            break;
                    }
                }

                matchCount++;
            }

            if (!double.IsNaN(lat) && !double.IsNaN(lon))
            {
                Coordinate c; 

                if (double.IsNaN(alt))
                {
                    c = new Coordinate(lat, lon);
                }
                else
                {
                    c = new Coordinate(lat, lon, alt);
                }

                return c;
            }

            return null;
        }

        private async Task<CoordinateCollection> ParseRing(string wkt, int minCoords, bool closed, bool optimize)
        {
            string[] coords = wkt.Split(_commonDelimiter, StringSplitOptions.RemoveEmptyEntries);

            var locs = new CoordinateCollection();

            foreach (var coord in coords)
            {
                var c = ParseCoord(coord);
                if (c.HasValue)
                {
                    locs.Add(c.Value);
                }
            }

            if (optimize)
            {
                locs = await SpatialTools.VertexReductionAsync(locs, tolerance);
            }

            if(locs.Count > minCoords)
            {
                //Ensure the ring is closed
                if(closed && !locs[0].Equals(locs[locs.Count - 1]))
                {
                    locs.Add(locs[0]);
                }

                return locs;
            }

            return null;
        }

        /// <summary>
        /// Creates a Point from a WKT
        /// </summary>
        private Point ParsePoint(string wkt)
        {
            var c = ParseCoord(wkt);

            if(c.HasValue)
            {
                return new Point(c.Value);
            }

            return null;
        }

        /// <summary>
        /// Creates a LineString from a WKT
        /// </summary>
        private async Task<LineString> ParseLineString(string wkt)
        {
            var coords = await ParseRing(wkt, 2, false, optimize);

            if(coords != null){
                return new LineString(coords);
            }

            return null;
        }

        /// <summary>
        /// Creates a Polygon from a WKT
        /// </summary>
        private async Task<Polygon> ParsePolygon(string wkt)
        {
            string[] rings = _singleShapeRx.Split(wkt);

            if (rings.Length > 0)
            {
                var ring = await ParseRing(rings[0], 3, true, optimize);

                if(ring != null)
                {
                    var p = new Polygon(ring);
                    
                    for(int i = 1; i < rings.Length; i++){
                        ring = await ParseRing(rings[i], 3, true, optimize);

                        if(ring != null)
                        {
                            p.InteriorRings.Add(ring);
                        }
                    }

                    return p;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a MultiPoint from a WKT
        /// </summary>
        private async Task<MultiPoint> ParseMultiPoint(string wkt)
        {
            var points = await ParseRing(wkt, 1, false, false);

            if(points != null)
            {
                var mp = new MultiPoint();
                foreach (var p in points)
                {
                    mp.Geometries.Add(new Point(p));                    
                }

                return mp;
            }

            return null;
        }

        /// <summary>
        /// Creates a MultiLineString from a WKT
        /// </summary>
        private async Task<MultiLineString> ParseMultiLineString(string wkt)
        {
            string[] rings = _singleShapeRx.Split(wkt);

            var ml = new MultiLineString();

            foreach (var ringWkt in rings)
            {
                var ring = await ParseRing(ringWkt, 2, false, optimize);

                if (ring != null)
                {
                    ml.Geometries.Add(new LineString(ring));
                }
            }

            if (ml.Geometries.Count > 0)
            {
                return ml;
            }

            return null;
        }

        /// <summary>
        /// Creates a MultiPolygon from a WKT
        /// </summary>
        private async Task<MultiPolygon> ParseMultiPolygon(string wkt)
        {
            string[] polys = _multiShapeRx.Split(wkt);

            var mp = new MultiPolygon();

            foreach (var polyWkt in polys)
            {
                var p = await ParsePolygon(polyWkt);
                if (p != null)
                {
                    mp.Geometries.Add(p);
                }
            }

            if (mp.Geometries.Count > 0)
            {
                return mp;
            }

            return null;
        }

        /// <summary>
        /// Creates a GeometryCollection from a WKT
        /// </summary>
        private async Task<GeometryCollection> ParseGeometryCollection(string wkt)
        {
            //Remove the Geometry collection name and opening bracket
            wkt = _collectionRx.Replace(wkt, "");

            //Remove the closing bracket
            wkt = wkt.Remove(wkt.LastIndexOf(')'));

            //Replace the seperator between the shapes with a pipe delimeter for easier splitting
            wkt = _shapeDelimiterRx.Replace(wkt, "$1|$2");

            var gc = new GeometryCollection();

            var shapes = wkt.Split(_pipeDelimiter);
            foreach (var shape in shapes)
            {
                var g = await ParseGeometry(shape);
                if (g != null)
                {
                    gc.Geometries.Add(g);
                }
            }

            if (gc.Geometries.Count > 0)
            {
                return gc;
            }

            return null;
        }

        #endregion

        #endregion

        #region WKT Writer Methods

        #region Location Writers

        /// <summary>
        /// Writes a Location as WKT
        /// </summary>
        private static void WriteLocation(Coordinate location, TextWriter writer, bool includeAltitude)
        {
            writer.Write(location.Longitude.ToString("0.#####", CultureInfo.InvariantCulture));
            writer.Write(" ");
            writer.Write(location.Latitude.ToString("0.#####", CultureInfo.InvariantCulture));

            if (includeAltitude)
            {
                writer.Write(" ");
                writer.Write((location.Altitude.HasValue) ? location.Altitude.Value : 0);
            }
        }

        /// <summary>
        /// Writes a Location collection to the writer
        /// </summary>
        /// <param name="Locations"></param>
        private static void WriteLocations(CoordinateCollection Locations, TextWriter writer, bool includeAltitude)
        {
            writer.Write("(");

            for (int i = 0; i < Locations.Count; i++)
            {
                if (i > 0)
                {
                    writer.Write(",");
                }

                WriteLocation(Locations[i], writer, includeAltitude);
            }

            writer.Write(")");
        }

        #endregion

        private static void WriteGeometry(Geometry Geometry, TextWriter writer, bool includeAltitude)
        {
            if (Geometry is Point)
            {
                WritePoint(Geometry as Point, writer, includeAltitude);
            }
            else if (Geometry is LineString)
            {
                WriteLineString(Geometry as LineString, writer, includeAltitude);
            }
            else if (Geometry is Polygon)
            {
                WritePolygon(Geometry as Polygon, writer, includeAltitude);
            }
            else if (Geometry is MultiPoint)
            {
                WriteMultiPoint(Geometry as MultiPoint, writer, includeAltitude);
            }
            else if (Geometry is MultiLineString)
            {
                WriteMultiLineString(Geometry as MultiLineString, writer, includeAltitude);
            }
            else if (Geometry is MultiPolygon)
            {
                WriteMultiPolygon(Geometry as MultiPolygon, writer, includeAltitude);
            }
            else if (Geometry is GeometryCollection)
            {
                WriteGeometryCollection(Geometry as GeometryCollection, writer, includeAltitude);
            }
            else if (Geometry is BoundingBox)
            {
                //Ignore bounding boxes
            }
            else
            {
                throw new Exception("Invalid Geometry Type");
            }
        }

        private static void WritePoint(Point point, TextWriter writer, bool includeAltitude)
        {
            writer.Write("POINT(");
            WriteLocation(point.Coordinate, writer, includeAltitude);
            writer.Write(")");
        }

        private static void WriteLineString(LineString lineString, TextWriter writer, bool includeAltitude)
        {
            writer.Write("LINESTRING");
            WriteLocations(lineString.Vertices, writer, includeAltitude);
        }

        private static void WritePolygon(Polygon polygon, TextWriter writer, bool includeAltitude)
        {
            writer.Write("POLYGON");
            writer.Write("(");
            //write the extrior ring
            WriteLocations(polygon.ExteriorRing, writer, includeAltitude);

            //write the interior rings of the polygon
            foreach (CoordinateCollection Locations in polygon.InteriorRings)
            {
                writer.Write(",");
                WriteLocations(Locations, writer, includeAltitude);
            }

            writer.Write(")");
        }

        private static void WriteMultiPoint(MultiPoint multPoint, TextWriter writer, bool includeAltitude)
        {
            writer.Write("MULTIPOINT(");

            for (int i = 0; i < multPoint.Geometries.Count; i++)
            {
                if (i > 0)
                {
                    writer.Write(",");
                }

                WriteLocation(multPoint.Geometries[i].Coordinate, writer, includeAltitude);
            }

            writer.Write(")");
        }

        private static void WriteMultiLineString(MultiLineString multLineString, TextWriter writer, bool includeAltitude)
        {
            writer.Write("MULTILINESTRING(");

            for (int i = 0; i < multLineString.Geometries.Count; i++)
            {
                if (i > 0)
                {
                    writer.Write(",");
                }

                WriteLocations(multLineString.Geometries[i].Vertices, writer, includeAltitude);
            }

            writer.Write(")");
        }

        private static void WriteMultiPolygon(MultiPolygon multPolygon, TextWriter writer, bool includeAltitude)
        {
            writer.Write("MULTIPOLYGON(");

            for (int i = 0; i < multPolygon.Geometries.Count; i++)
            {
                if (i > 0)
                {
                    writer.Write(",");
                }

                writer.Write("(");
                //write the extrior ring
                WriteLocations(multPolygon.Geometries[i].ExteriorRing, writer, includeAltitude);

                //write the interior rings of the polygon
                foreach (CoordinateCollection Locations in multPolygon.Geometries[i].InteriorRings)
                {
                    writer.Write(",");
                    WriteLocations(Locations, writer, includeAltitude);
                }

                writer.Write(")");
            }

            writer.Write(")");
        }

        private static void WriteGeometryCollection(GeometryCollection GeometryCollection, TextWriter writer, bool includeAltitude)
        {
            writer.Write("GEOMETRYCOLLECTION(");

            for (int i = 0; i < GeometryCollection.Geometries.Count; i++)
            {
                if (i > 0)
                {
                    writer.Write(",");
                }

                WriteGeometry(GeometryCollection.Geometries[i], writer, includeAltitude);
            }
            writer.Write(")");
        }

        private static void WriteLocationRect(BoundingBox boundingBox, TextWriter writer)
        {
            writer.Write("POLYGON((");
            WriteLocation(new Coordinate(boundingBox.MinY, boundingBox.MinX), writer, false);
            writer.Write(",");
            WriteLocation(new Coordinate(boundingBox.MinY, boundingBox.MaxX), writer, false);
            writer.Write(",");
            WriteLocation(new Coordinate(boundingBox.MaxY, boundingBox.MaxX), writer, false);
            writer.Write(",");
            WriteLocation(new Coordinate(boundingBox.MaxY, boundingBox.MinX), writer, false);
            writer.Write(",");
            WriteLocation(new Coordinate(boundingBox.MinY, boundingBox.MinX), writer, false);
            writer.Write("))");
        }

        #endregion
    }
}

