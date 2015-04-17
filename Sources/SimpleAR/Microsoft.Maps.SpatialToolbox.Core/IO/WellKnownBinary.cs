/**************************************************************************** 
 * Author: Ricky Brundritt
 * 
 * Description:
 * This class reads and writes Geometry objects using Well Known Binary.
 *
 * See Also:
 * http://www.opengeospatial.org/standards/sfa
 * http://publib.boulder.ibm.com/infocenter/idshelp/v10/index.jsp?topic=/com.ibm.spatial.doc/spat274.htm
 * http://en.wikipedia.org/wiki/Well-known_text 
 * 
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maps.SpatialToolbox.IO
{
    /// <summary>
    /// This class reads and writes Geometry objects using Well Known Binary.
    /// </summary>
    public class WellKnownBinary : BaseFeed
    {
        #region Constructor

        public WellKnownBinary()
        {
        }

        public WellKnownBinary(double tolerance)
            : base(tolerance)
        {            
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads a geometry from an array of bytes of Well Known Binary.
        /// </summary>
        /// <param name="data">An array of bytes of Well Known Binary</param>
        /// <returns>A geometry object.</returns>
        public async Task<Geometry> Read(byte[] data)
        {
            // Create a memory stream using the suppiled byte array.
            using (MemoryStream ms = new MemoryStream(data))
            {
                // Create a new binary reader using the newly created memorystream.
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    // Call the main read function.
                    return await Read(reader);
                }
            }
        }

        /// <summary>
        /// Reads a geometry from a stream of Well Known Binary asynchronously.
        /// </summary>
        /// <param name="stream">Stream of Well Known Binary</param>
        /// <returns>A SpatialDataSet containing the geometry. Only the one geometry is return.</returns>
        public override Task<SpatialDataSet> ReadAsync(Stream stream)
        {
            return Task.Run<SpatialDataSet>(async () =>
            {
                try
                {
                    // Create a new binary reader using the newly created memorystream.
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        // Call the main read function.
                        var g = await Read(reader);

                        var data = new SpatialDataSet()
                        {
                            BoundingBox = g.Envelope()
                        };

                        data.Geometries.Add(g);

                        return data;
                    }
                }
                catch (Exception ex)
                {
                    return new SpatialDataSet()
                    {
                        Error = ex.Message
                    };
                }
            });
        }

        /// <summary>
        /// Reads a geometry from an BinaryReader containing of Well Known Binary.
        /// </summary>
        /// <param name="data">A BinaryReader containing Well Known Binary</param>
        /// <returns>A geometry object.</returns>
        public async Task<Geometry> Read(BinaryReader reader)
        {
            // Get the first Byte in the array. This specifies if the WKB is in
            // XDR (big-endian) format of NDR (little-endian) format.
            ByteOrder wkbByteOrder = (ByteOrder)reader.ReadByte();

            // Get the type of this geometry.
            WKBShapeType type = (WKBShapeType)NumberReader.ReadUInt32(reader, wkbByteOrder);

            bool includeAltitude = false, includeM = false;

            //check to see if the coordinates should include the Z coordinate
            if (((type > ((WKBShapeType)0x3e8)) && (type < ((WKBShapeType)0x7d0))) || (type > ((WKBShapeType)0xbb8)))
            {
                includeAltitude = true;
            }

            //check to see if the coordinates should include a Measure
            if (type > ((WKBShapeType)0x7d0))
            {
                includeM = true;
            }

            return await ReadGeometry(reader, wkbByteOrder, type, includeAltitude, includeM);
        }

        /// <summary>
        /// Turns a geometry into it's Well Known Binary equivalent as an array of bytes.
        /// </summary>
        /// <param name="geometry">A geometry to convert to Well Known Binary.</param>
        /// <returns>An array of bytes representing the Geometry as Well Known Binary.</returns>
        public static byte[] Write(Geometry geometry)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    //Write the Geometry
                    WriteGeometry(geometry, writer);
                }

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Writes the first geometry in a spatial data set as a Well Known Binary to a stream.
        /// </summary>
        /// <param name="geometry">A spatial data set.</param>
        /// <param name="stream">Stream to write to.</param>
        public override Task WriteAsync(SpatialDataSet data, Stream stream)
        {
            return Task.Run(() =>
            {
                if (data != null && data.Geometries != null && data.Geometries.Count > 0)
                {
                    using (var writer = new BinaryWriter(stream))
                    {
                        //Write the Geometry
                        WriteGeometry(data.Geometries[0], writer);
                    }
                }
            });
        }

        #endregion

        #region WKB Reader Methods

        #region Common Coordinate Readers

        private Coordinate ReadCoordinate(BinaryReader reader, ByteOrder wkbByteOrder, WKBShapeType type, bool includeAltitude, bool includeM)
        {
            double lon = NumberReader.ReadDouble(reader, wkbByteOrder);
            double lat = NumberReader.ReadDouble(reader, wkbByteOrder);

            Coordinate coordinate;

            if (!includeAltitude)
            {
                coordinate = new Coordinate(lat, lon);                
            }
            else
            {
                double z = NumberReader.ReadDouble(reader, wkbByteOrder);
                coordinate = new Coordinate(lat, lon, z);  
            }

            if (includeM)
            {
                //Read past the measure value if included
                NumberReader.ReadDouble(reader, wkbByteOrder);
            }

            return coordinate;
        }

        private async Task<CoordinateCollection> ReadCoordinates(BinaryReader reader, ByteOrder wkbByteOrder, WKBShapeType type, bool includeAltitude, bool includeM)
        {
            // Get the number of points in this linestring.
            int numPoints = (int)NumberReader.ReadUInt32(reader, wkbByteOrder);

            // Create a new array of coordinates.
            var coords = new CoordinateCollection();

            // Loop on the number of points in the ring.
            for (int i = 0; i < numPoints; i++)
            {
                coords.Add(ReadCoordinate(reader, wkbByteOrder, type, includeAltitude, includeM));
            }

            if (optimize)
            {
                coords = await SpatialTools.VertexReductionAsync(coords, tolerance);
            }

            return coords;
        }

        private async Task<CoordinateCollection> ReadLinearRing(BinaryReader reader, ByteOrder wkbByteOrder, WKBShapeType type, bool includeAltitude, bool includeM)
        {
            var ring = await ReadCoordinates(reader, wkbByteOrder, type, includeAltitude, includeM);

            int count = ring.Count;

            //Check to see if ring is closed. If not add the first point to the end.
            if (count > 0 && ring[0] != ring[count - 1])
            {
                ring.Add(ring[0]);
            }

            return ring;
        }

        #endregion

        #region Geometry Readers

        private async Task<Geometry> ReadGeometry(BinaryReader reader, ByteOrder wkbByteOrder, WKBShapeType type, bool includeAltitude, bool includeM)
        {
            switch (type)
            {
                case WKBShapeType.WkbPoint:
                case WKBShapeType.WkbPointZ:
                case WKBShapeType.WkbPointM:
                case WKBShapeType.WkbPointZM:
                    return ReadPoint(reader, wkbByteOrder, type, includeAltitude, includeM);
                case WKBShapeType.WkbLineString:
                case WKBShapeType.WkbLineStringZ:
                case WKBShapeType.WkbLineStringM:
                case WKBShapeType.WkbLineStringZM:
                    return await ReadLineString(reader, wkbByteOrder, type, includeAltitude, includeM);
                case WKBShapeType.WkbPolygon:
                case WKBShapeType.WkbPolygonZ:
                case WKBShapeType.WkbPolygonM:
                case WKBShapeType.WkbPolygonZM:
                    return await ReadPolygon(reader, wkbByteOrder, type, includeAltitude, includeM);
                case WKBShapeType.WkbMultiPoint:
                case WKBShapeType.WkbMultiPointZ:
                case WKBShapeType.WkbMultiPointM:
                case WKBShapeType.WkbMultiPointZM:
                    return ReadMultiPoint(reader, wkbByteOrder, type, includeAltitude, includeM);
                case WKBShapeType.WkbMultiLineString:
                case WKBShapeType.WkbMultiLineStringZ:
                case WKBShapeType.WkbMultiLineStringM:
                case WKBShapeType.WkbMultiLineStringZM:
                    return await ReadMultiLineString(reader, wkbByteOrder, type, includeAltitude, includeM);
                case WKBShapeType.WkbMultiPolygon:
                case WKBShapeType.WkbMultiPolygonZ:
                case WKBShapeType.WkbMultiPolygonM:
                case WKBShapeType.WkbMultiPolygonZM:
                    return await ReadMultiPolygon(reader, wkbByteOrder, type, includeAltitude, includeM);
                case WKBShapeType.WkbGeometryCollection:
                case WKBShapeType.WkbGeometryCollectionZ:
                case WKBShapeType.WkbGeometryCollectionM:
                case WKBShapeType.WkbGeometryCollectionZM:
                    return await ReadGeometryCollection(reader, wkbByteOrder, type, includeAltitude, includeM);
                default:
                    if (!Enum.IsDefined(typeof(WKBShapeType), type))
                    {
                        throw new ArgumentException("Geometry type not recognized");
                    }
                    else
                    {
                        throw new NotSupportedException("Geometry type '" + type + "' not supported");
                    }
            }
        }

        private Point ReadPoint(BinaryReader reader, ByteOrder wkbByteOrder, WKBShapeType type, bool includeAltitude, bool includeM)
        {
            return new Point(ReadCoordinate(reader, wkbByteOrder, type, includeAltitude, includeM));
        }

        private async Task<LineString> ReadLineString(BinaryReader reader, ByteOrder wkbByteOrder, WKBShapeType type, bool includeAltitude, bool includeM)
        {
            var c = await ReadCoordinates(reader, wkbByteOrder, type, includeAltitude, includeM);
            return new LineString(c);
        }

        private async Task<Polygon> ReadPolygon(BinaryReader reader, ByteOrder wkbByteOrder, WKBShapeType type, bool includeAltitude, bool includeM)
        {
            // Get the Number of rings in this Polygon.
            int numRings = (int)NumberReader.ReadUInt32(reader, wkbByteOrder);

            var c = await ReadLinearRing(reader, wkbByteOrder, type, includeAltitude, includeM);
            Polygon shell = new Polygon(c);

            // Create a new array of linearrings for the interior rings.
            for (int i = 0; i < (numRings - 1); i++)
            {
                c = await ReadLinearRing(reader, wkbByteOrder, type, includeAltitude, includeM);
                shell.InteriorRings.Add(c);
            }

            // Create and return the Poylgon.
            return shell;
        }

        private MultiPoint ReadMultiPoint(BinaryReader reader, ByteOrder wkbByteOrder, WKBShapeType type, bool includeAltitude, bool includeM)
        {
            // Get the number of points in this multPoint.
            int numPoints = (int)NumberReader.ReadUInt32(reader, wkbByteOrder);

            // Create a new array for the points.
            MultiPoint points = new MultiPoint();

            // Loop on the number of points.
            for (int i = 0; i < numPoints; i++)
            {
                // Read point header
                reader.ReadByte();
                NumberReader.ReadUInt32(reader, wkbByteOrder);

                // Create the next point and add it to the point array.
                points.Geometries.Add(ReadPoint(reader, wkbByteOrder, type, includeAltitude, includeM));
            }

            return points;
        }

        private async Task<MultiLineString> ReadMultiLineString(BinaryReader reader, ByteOrder wkbByteOrder, WKBShapeType type, bool includeAltitude, bool includeM)
        {
            // Get the number of linestrings in this multLineString.
            int numLineStrings = (int)NumberReader.ReadUInt32(reader, wkbByteOrder);

            // Create a new array for the linestrings .
            MultiLineString mline = new MultiLineString(numLineStrings);

            LineString line;

            // Loop on the number of linestrings.
            for (int i = 0; i < numLineStrings; i++)
            {
                // Read linestring header
                reader.ReadByte();
                NumberReader.ReadUInt32(reader, wkbByteOrder);

                line = await ReadLineString(reader, wkbByteOrder, type, includeAltitude, includeM);

                // Create the next linestring and add it to the array.
                mline.Geometries.Add(line);
            }

            // Create and return the MultiLineString.
            return mline;
        }

        private async Task<MultiPolygon> ReadMultiPolygon(BinaryReader reader, ByteOrder wkbByteOrder, WKBShapeType type, bool includeAltitude, bool includeM)
        {
            // Get the number of Polygons.
            int numPolygons = (int)NumberReader.ReadUInt32(reader, wkbByteOrder);

            // Create a new array for the Polygons.
            MultiPolygon polygons = new MultiPolygon(numPolygons);

            Polygon p;

            // Loop on the number of polygons.
            for (int i = 0; i < numPolygons; i++)
            {
                // read polygon header
                reader.ReadByte();
                NumberReader.ReadUInt32(reader, wkbByteOrder);

                p = await ReadPolygon(reader, wkbByteOrder, type, includeAltitude, includeM);

                // Create the next polygon and add it to the array.
                polygons.Geometries.Add(p);
            }

            //Create and return the MultiPolygon.
            return polygons;
        }

        private async Task<GeometryCollection> ReadGeometryCollection(BinaryReader reader, ByteOrder wkbByteOrder, WKBShapeType type, bool includeAltitude, bool includeM)
        {
            // The next byte in the array tells the number of geometries in this collection.
            int numGeometries = (int)NumberReader.ReadUInt32(reader, wkbByteOrder);

            // Create a new array for the geometries.
            var geographies = new GeometryCollection(numGeometries);

            Geometry g;
            // Loop on the number of geometries.
            for (int i = 0; i < numGeometries; i++)
            {
                g = await ReadGeometry(reader, wkbByteOrder, type, includeAltitude, includeM);

                // Call the main create function with the next geometry.
                geographies.Geometries.Add(g);
            }

            // Create and return the next geometry.
            return geographies;
        }

        #endregion

        #endregion

        #region WKB Writer Methods

        private static void WriteGeometry(Geometry Geometry, BinaryWriter writer)
        {
            if (Geometry is Point)
            {
                WritePoint(Geometry as Point, writer);
            }
            else if (Geometry is LineString)
            {
                WriteLineString(Geometry as LineString, writer);
            }
            else if (Geometry is Polygon)
            {
                WritePolygon(Geometry as Polygon, writer);
            }
            else if (Geometry is MultiPoint)
            {
                WriteMultiPoint(Geometry as MultiPoint, writer);
            }
            else if (Geometry is MultiLineString)
            {
                WriteMultiLineString(Geometry as MultiLineString, writer);
            }
            else if (Geometry is MultiPolygon)
            {
                WriteMultiPolygon(Geometry as MultiPolygon, writer);
            }
            else if (Geometry is GeometryCollection<Geometry>)
            {
                WriteGeometryCollection(Geometry as GeometryCollection<Geometry>, writer);
            }
            else
            {
                throw new Exception("Invalid Geometry Type");
            }
        }

        private static void WriteCoordinate(Coordinate coordinate, BinaryWriter writer, bool includeAltitude)
        {
            writer.Write(coordinate.Longitude);
            writer.Write(coordinate.Latitude);

            if (includeAltitude)
            {
                writer.Write(coordinate.Altitude.Value);
            }
        }

        private static void WriteCoordinates(CoordinateCollection coords, BinaryWriter writer, bool includeAltitude)
        {
            writer.Write((int)coords.Count);    //write the number of coordinates

            for (int i = 0; i < coords.Count; i++)
            {
                WriteCoordinate(coords[i], writer, includeAltitude);
            }
        }

        private static void WritePoint(Point point, BinaryWriter writer)
        {
            bool includeAltitude = false;

            //write the byte order
            writer.Write((byte)ByteOrder.Ndr);

            if (point.Coordinate.Altitude.HasValue)
            {
                writer.Write((uint)WKBShapeType.WkbPointZ);
                includeAltitude = true;
            }
            else
            {
                writer.Write((uint)WKBShapeType.WkbPoint);
            }

            WriteCoordinate(point.Coordinate, writer, includeAltitude);
        }

        private static void WriteLineString(LineString lineString, BinaryWriter writer)
        {
            bool includeAltitude = false;

            //write the byte order
            writer.Write((byte)ByteOrder.Ndr);

            if (lineString.Vertices.Count > 0)
            {
                if (lineString.Vertices[0].Altitude.HasValue)
                {
                    writer.Write((uint)WKBShapeType.WkbLineStringZ);
                    includeAltitude = true;
                }
                else
                {
                    writer.Write((uint)WKBShapeType.WkbLineString);
                }
            }
            else
            {  //If there is no vertices then default to a standard LineString
                writer.Write((uint)WKBShapeType.WkbLineString);
            }

            WriteCoordinates(lineString.Vertices, writer, includeAltitude);
        }

        private static void WritePolygon(Polygon polygon, BinaryWriter writer)
        {
            bool includeAltitude = false;

            //write the byte order
            writer.Write((byte)ByteOrder.Ndr);

            if (polygon.ExteriorRing.Count > 0)
            {
                if (polygon.ExteriorRing[0].Altitude.HasValue)
                {
                    writer.Write((uint)WKBShapeType.WkbPolygonZ);
                    includeAltitude = true;
                }
                else
                {
                    writer.Write((uint)WKBShapeType.WkbPolygon);
                }
            }
            else
            {
                //If there is no vertices then default to a standard LineString
                writer.Write((uint)WKBShapeType.WkbPolygon);
            }

            writer.Write((int)polygon.InteriorRings.Count + 1);                 //write the number of rings

            WriteCoordinates(polygon.ExteriorRing, writer, includeAltitude);         //write the exterior ring

            for (int i = 0; i < polygon.InteriorRings.Count; i++)
            {
                WriteCoordinates(polygon.InteriorRings[i], writer, includeAltitude); //write the interior ring
            }
        }

        private static void WriteMultiPoint(MultiPoint points, BinaryWriter writer)
        {
            //write the byte order
            writer.Write((byte)ByteOrder.Ndr);

            if (points.Geometries.Count > 0)
            {
                if (points.Geometries[0].Coordinate.Altitude != null)
                {
                    writer.Write((uint)WKBShapeType.WkbMultiPointZ);
                }
                else
                {
                    writer.Write((uint)WKBShapeType.WkbMultiPoint);
                }
            }
            else
            {
                writer.Write((uint)WKBShapeType.WkbMultiPoint);
            }

            writer.Write((int)points.Geometries.Count);    //write the number of points

            for (int i = 0; i < points.Geometries.Count; i++)
            {
                WritePoint(points.Geometries[i], writer);
            }
        }

        private static void WriteMultiLineString(MultiLineString lineStrings, BinaryWriter writer)
        {
            //write the byte order
            writer.Write((byte)ByteOrder.Ndr);

            if (lineStrings.Geometries.Count > 0 && lineStrings.Geometries[0].Vertices.Count > 0)
            {
                if (lineStrings.Geometries[0].Vertices[0].Altitude.HasValue)
                {
                    writer.Write((uint)WKBShapeType.WkbMultiLineStringZ);
                }
                else
                {
                    writer.Write((uint)WKBShapeType.WkbMultiLineString);
                }
            }
            else
            {
                writer.Write((uint)WKBShapeType.WkbMultiLineString);
            }

            writer.Write((int)lineStrings.Geometries.Count);    //write the number of LineStrings

            for (int i = 0; i < lineStrings.Geometries.Count; i++)
            {
                WriteLineString(lineStrings.Geometries[i], writer);
            }
        }

        private static void WriteMultiPolygon(MultiPolygon polygons, BinaryWriter writer)
        {
            //write the byte order
            writer.Write((byte)ByteOrder.Ndr);

            if (polygons.Geometries.Count > 0 && polygons.Geometries[0].ExteriorRing.Count > 0)
            {
                if (polygons.Geometries[0].ExteriorRing[0].Altitude.HasValue)
                {
                    writer.Write((uint)WKBShapeType.WkbMultiPolygonZ);
                }
                else
                {
                    writer.Write((uint)WKBShapeType.WkbMultiPolygon);
                }
            }
            else
            {
                writer.Write((uint)WKBShapeType.WkbMultiPolygon);
            }

            writer.Write((int)polygons.Geometries.Count);    //write the number of polygons

            for (int i = 0; i < polygons.Geometries.Count; i++)
            {
                WritePolygon(polygons.Geometries[i], writer);
            }
        }

        private static void WriteGeometryCollection(GeometryCollection<Geometry> geographies, BinaryWriter writer)
        {
            //write the byte order
            writer.Write((byte)ByteOrder.Ndr);

            writer.Write((uint)WKBShapeType.WkbGeometryCollection);

            writer.Write((int)geographies.Geometries.Count);           //write the number of geographies
            for (int i = 0; i < geographies.Geometries.Count; i++)
            {
                WriteGeometry(geographies.Geometries[i], writer);
            }
        }

        #endregion
    }
}
