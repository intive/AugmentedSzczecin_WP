using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Maps.SpatialToolbox.IO
{
    /// <summary>
    /// This class is capable of reading an ESRI Shapefile (.shp) file 
    /// and converting it into a collection of ShapefileItems.
    /// http://www.esri.com/library/whitepapers/pdfs/shapefile.pdf
    /// </summary>
    public class ShapefileReader : BaseFeed
    {
        #region Constructor

        public ShapefileReader()
        {
        }

        public ShapefileReader(double tolerance) : base(tolerance)
        {
        }

        #endregion

        #region Public methods

        #region Read/Write Methods

        /// <summary>
        /// Read shapes items from the given stream.
        /// </summary>
        /// <param name="stream">A Shapefile as a Stream</param>
        /// <returns>A list fo shapefile items</returns>
        public override Task<SpatialDataSet> ReadAsync(Stream stream)
        {
            return Task.Run<SpatialDataSet>(async () =>
            {
                try
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        return await ReadAsync(reader);
                    }
                }
                catch
                {
                }

                return null;
            });
        }

        /// <summary>
        /// Read shapes items from the given BinaryReader.
        /// </summary>
        /// <param name="reader">A Shapefile as a BinaryReader</param>
        /// <returns>A List of ShapefileItem objects</returns>
        public Task<SpatialDataSet> ReadAsync(BinaryReader reader)
        {
            return Task.Run<SpatialDataSet>(async () =>
            {
                try
                {
                    using (reader)
                    {
                        var data = new SpatialDataSet();

                        ShapefileShapeType shapeType;
                        BoundingBox bounds;

                        // Read the File Header.
                        ReadFileHeader(reader, out shapeType, out bounds);

                        //Read in shape records
                        data.Geometries = await ReadShapeRecords(reader, shapeType);
                        data.BoundingBox = bounds;

                        return data;
                    }
                }
                catch
                {
                }

                return null;
            });
        }

        /// <summary>
        /// DO NOT USE. This is not implemented.
        /// </summary>
        public override Task WriteAsync(SpatialDataSet data, Stream stream)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region File Header Readers

        /// <summary>
        /// Reads the header of the shapefile
        /// </summary>
        /// <param name="stream">A Shapefile as a Stream</param>
        /// <param name="shapeType">The type of shapes contained in the Shapefile.</param>
        /// <param name="bounds">Bounding box of the shapes in the Shapefile.</param>
        public void ReadHeader(Stream stream, out ShapefileShapeType shapeType, out BoundingBox bounds)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                // Read the File Header.
                ReadFileHeader(reader, out shapeType, out bounds);
            }
        }

        /// <summary>
        /// Reads the header of the shapefile
        /// </summary>
        /// <param name="reader">A Shapefile as a BinaryReader</param>
        /// <param name="shapeType">The type of shapes contained in the Shapefile.</param>
        /// <param name="bounds">Bounding box of the shapes in the Shapefile.</param>
        public void ReadHeader(BinaryReader reader, out ShapefileShapeType shapeType, out BoundingBox bounds)
        {
            using (reader)
            {
                // Read the File Header.
                ReadFileHeader(reader, out shapeType, out bounds);
            }
        }

        #endregion

        #region DBF File Merging

        /// <summary>
        /// Takes a dbf file name and a Shapefile and adds the dbf row to each ShapefileItem's attributes.
        /// </summary>
        /// <param name="dbfFileStream">The file stream of the dbf file to be read</param>
        /// <param name="shapeFile">A Shapefile object that contains a list of ShapefileItem objects to get the attributes for</param>
        /// <param name="colRules">List of column indices to skip. Settign to null causes all columns to be read</param>
        public static Task<bool> MergeDBFData(Stream dbfFileStream, SpatialDataSet dataSet, bool[] colRules)
        {
            return Task.Run<bool>(async () =>
            {
                try
                {
                    //Create binary reader out of file stream
                    using (BinaryReader reader = new BinaryReader(dbfFileStream))
                    {
                        await MergeDBFData(reader, dataSet, colRules);
                    }

                    return true;
                }
                catch
                {
                }

                return false;
            });
        }

        /// <summary>
        /// Takes a dbf file name and a Shapefile and adds the dbf row to each ShapefileItem's attributes.
        /// </summary>
        /// <param name="dbfReader">The binary reader of the dbf file to be read</param>
        /// <param name="shapeFile">A Shapefile object that contains a list of ShapefileItem objects to get the attributes for</param>
        /// <param name="colRules">List of column indices to skip. Settign to null causes all columns to be read</param>
        public static Task<bool> MergeDBFData(BinaryReader dbfReader, SpatialDataSet dataSet, bool[] colRules)
        {
            return Task.Run<bool>(() =>
            {
                //Read the header
                var attributesHeader = DBaseFileReader.ReadHeader(dbfReader);

                int numItems = dataSet.Geometries.Count;

                //Verify that the DBF file has the same number of rows as there are ShapefileItem's
                if (numItems != attributesHeader.NumRows)
                {
                    throw new Exception("Inconsistant number of records between dbf and List of ShapefileItem's.");
                }
                else
                {
                    int numValidRulesCols = 0;

                    if (colRules != null)
                    {
                        foreach (bool rule in colRules)
                        {
                            if (rule)
                            {
                                numValidRulesCols++;
                            }
                        }
                    }
                    else
                    {
                        numValidRulesCols = attributesHeader.NumColumns;
                    }

                    //Loop through each item and read the attributes
                    for (int i = 0; i < numItems; i++)
                    {
                        var r = DBaseFileReader.ReadRow(attributesHeader, dbfReader, colRules, numValidRulesCols);
                        if (r != null)
                        {
                            foreach (KeyValuePair<string, object> row in r)
                            {
                                dataSet.Geometries[i].Metadata.Properties.Add(row.Key, row.Value);
                            }
                        }
                    }
                }
                return true;
            });
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Reads the header of a Shapefile
        /// </summary>
        /// <param name="reader">The Shapefile stream</param>
        /// <returns>The shapefile header</returns>
        private void ReadFileHeader(BinaryReader reader, out BoundingBox bounds)
        {
            // Read File Code.
            int FileCode = NumberReader.ReadIntBE(reader);

            //Verify that the file has the correct file code (9994)
            if (FileCode != 9994)
            {
                throw new FormatException("Invalid FileCode encountered. Expecting a file code of 9994.");
            }

            //Skip unused section
            reader.BaseStream.Seek(20, SeekOrigin.Current);

            // Read the File Length.
            var fileLength = NumberReader.ReadIntBE(reader);

            // Skip the Version number of the shapefile.
            reader.BaseStream.Seek(4, SeekOrigin.Current);

            // Get the shape type of the shapefile. Note that shapefiles can only contain one type of shape per file
            var shapeType = (ShapefileShapeType) NumberReader.ReadIntLE(reader);

            // Get the bounding box of the Bounding Box.
            bounds = ReadBoundingBox(reader);
        }

        /// <summary>
        /// Reads the header of a Shapefile
        /// </summary>
        /// <param name="reader">The Shapefile stream</param>
        /// <returns>The shapefile header</returns>
        private void ReadFileHeader(BinaryReader reader, out ShapefileShapeType shapeType, out BoundingBox bounds)
        {
            // Read File Code.
            int FileCode = NumberReader.ReadIntBE(reader);

            //Verify that the file has the correct file code (9994)
            if (FileCode != 9994)
            {
                throw new FormatException("Invalid FileCode encountered. Expecting a file code of 9994.");
            }

            //Skip unused section
            reader.BaseStream.Seek(20, SeekOrigin.Current);

            // Read the File Length.
            long fileLength = NumberReader.ReadIntBE(reader);

            // Skip the Version number of the shapefile.
            reader.BaseStream.Seek(4, SeekOrigin.Current);

            // Get the shape type of the shapefile. Note that shapefiles can only contain one type of shape per file
            shapeType = (ShapefileShapeType) NumberReader.ReadIntLE(reader);

            // Get the bounding box of the Bounding Box.
            bounds = ReadBoundingBox(reader);
        }

        /// <summary>
        /// Reads all shapes in the Shapefile
        /// </summary>
        /// <param name="reader">The Shapefile stream</param>
        /// <param name="shapeType">The type of shape to read</param>
        /// <returns>A list of Geometry objects</returns>
        private async Task<List<Geometry>> ReadShapeRecords(BinaryReader reader, ShapefileShapeType shapeType)
        {
            List<Geometry> items = null;

            // Skip the end of the file header.
            reader.BaseStream.Seek(100, SeekOrigin.Begin);

            // Read the shapes depending on the ShapeType.
            switch (shapeType)
            {
                case ShapefileShapeType.NullShape: // Do nothing.                    
                    break;
                case ShapefileShapeType.Point:
                case ShapefileShapeType.PointZ:
                case ShapefileShapeType.PointM:
                case ShapefileShapeType.PointZM:
                    items = ReadPointData(reader);
                    break;
                case ShapefileShapeType.LineString:
                case ShapefileShapeType.LineStringZ:
                case ShapefileShapeType.LineStringM:
                case ShapefileShapeType.LineStringZM:
                    items = await ReadLineStringData(reader);
                    break;
                case ShapefileShapeType.Polygon:
                case ShapefileShapeType.PolygonZ:
                case ShapefileShapeType.PolygonM:
                case ShapefileShapeType.PolygonZM:
                    items = ReadPolygonData(reader);
                    break;
                case ShapefileShapeType.MultiPoint:
                case ShapefileShapeType.MultiPointZ:
                case ShapefileShapeType.MultiPointM:
                case ShapefileShapeType.MultiPointZM:
                    items = ReadMultiPointData(reader);
                    break;
                default: //throw an error indicating that an unsupported shape type is in the file
                    throw new Exception("ShapeType " + shapeType.ToString() + " is not supported.");
            }

            return items;
        }

        /// <summary>
        /// Read the header of a record
        /// </summary>
        /// <param name="reader">The Shapefile stream</param>
        /// <returns>The number of records in the file</returns>
        private int ReadRecordHeader(BinaryReader reader)
        {
            int recordNumber = NumberReader.ReadIntBE(reader);

            //Skip content length and shape type properties of shape record
            reader.BaseStream.Seek(8, SeekOrigin.Current);

            return recordNumber;
        }

        /// <summary>
        /// Read in a coordinate object
        /// </summary>
        /// <param name="reader">The Shapefile stream</param>
        /// <returns>A coordinate</returns>
        private Coordinate ReadCoordinate(BinaryReader reader)
        {
            double lon = NumberReader.ReadDoubleLE(reader);
            double lat = NumberReader.ReadDoubleLE(reader);

            return new Coordinate(lat, lon);
        }

        /// <summary>
        /// Reads the BoundingBox property of a record
        /// </summary>
        /// <param name="reader">The Shapefile stream</param>
        /// <returns>A bounding box of a record</returns>
        private BoundingBox ReadBoundingBox(BinaryReader reader)
        {
            //Read the standard bounding latitude and longitude values of the bounding box
            var minX = NumberReader.ReadDoubleLE(reader);
            var minY = NumberReader.ReadDoubleLE(reader);
            var maxX = NumberReader.ReadDoubleLE(reader);
            var maxY = NumberReader.ReadDoubleLE(reader);

            var width = Math.Abs(maxX - minX);
            var height = Math.Abs(maxY - minY);
            var center = new Coordinate(maxY - height/2, maxX - width/2);

            return new BoundingBox(center, width, height);
        }

        /// <summary>
        /// Read the Parts index array for a record
        /// </summary>
        /// <param name="reader">The Shapefile stream</param>
        /// <param name="numParts">The number of parts in the array</param>
        /// <param name="numPoints">The number of points in total</param>
        /// <returns>An array of part indexes</returns>
        private int[] ReadParts(BinaryReader reader, int numParts, int numPoints)
        {
            int[] parts = new int[numParts + 1];

            for (int i = 0; i < numParts; i++)
            {
                parts[i] = NumberReader.ReadIntLE(reader);
            }

            parts[numParts] = numPoints;

            return parts;
        }

        /// <summary>
        /// Reads Pointdata from shapefile
        /// </summary>
        /// <param name="reader">The shapefile stream</param>
        /// <returns>A list of Geometry objects</returns>
        private List<Geometry> ReadPointData(BinaryReader reader)
        {
            var items = new List<Geometry>();

            // For each header                
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                // Read Record Header.
                int id = ReadRecordHeader(reader);

                //Read the coordinate and create a point object
                var item = new Point(ReadCoordinate(reader));
                item.Metadata.ID = id.ToString();
                items.Add(item);
            }

            return items;
        }

        /// <summary>
        /// Reads LineString shapefile
        /// </summary>
        /// <param name="reader">The shapefile stream</param>
        /// <returns>A list of Geometry objects</returns>
        private async Task<List<Geometry>> ReadLineStringData(BinaryReader reader)
        {
            var items = new List<Geometry>();
            int numParts, numPoints;

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                Geometry item;

                // Read Record Header.
                int id = ReadRecordHeader(reader);
                var boundingBox = ReadBoundingBox(reader);

                // Read the number of Parts and Points in the shape.
                numParts = NumberReader.ReadIntLE(reader);
                numPoints = NumberReader.ReadIntLE(reader);

                int[] parts = ReadParts(reader, numParts, numPoints);

                //First read all the rings
                var rings = new List<CoordinateCollection>();

                var multiline = new MultiLineString();
                for (int ringID = 0; ringID < numParts; ringID++)
                {
                    var line = new LineString();

                    for (int i = parts[ringID]; i < parts[ringID + 1]; i++)
                    {
                        line.Vertices.Add(ReadCoordinate(reader));
                    }

                    if (optimize)
                    {
                        line.Vertices = await SpatialTools.VertexReductionAsync(line.Vertices, tolerance);
                    }

                    multiline.Geometries.Add(line);
                }

                if (numParts == 1)
                {
                    item = (Geometry) multiline.Geometries[0];
                }
                else
                {
                    item = multiline;
                }

                item.Metadata.ID = id.ToString();
                item.Metadata.Properties.Add("BoundingBox", boundingBox);

                items.Add(item);
            }

            return items;
        }

        /// <summary>
        /// Read a shapefile Polygon record.
        /// </summary>
        /// <param name="reader">The Shapefile stream</param>
        /// <returns>A list of Geometry objects</returns>
        private List<Geometry> ReadPolygonData(BinaryReader reader)
        {
            var items = new List<Geometry>();
            int numParts, numPoints;

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                Geometry item;

                // Read Record Header.
                int id = ReadRecordHeader(reader);
                var boundingBox = ReadBoundingBox(reader);

                // Read the number of Parts and Points in the shape.
                numParts = NumberReader.ReadIntLE(reader);
                numPoints = NumberReader.ReadIntLE(reader);

                int[] parts = ReadParts(reader, numParts, numPoints);

                //First read all the rings
                var rings = new List<CoordinateCollection>();

                for (int ringID = 0; ringID < numParts; ringID++)
                {
                    var ring = new CoordinateCollection();

                    for (int i = parts[ringID]; i < parts[ringID + 1]; i++)
                    {
                        ring.Add(ReadCoordinate(reader));
                    }

                    if (optimize)
                    {
                        ring = SpatialTools.VertexReduction(ring, tolerance);
                    }

                    rings.Add(ring);
                }

                // Vertices for a single, ringed polygon are always in clockwise order. 
                // Rings defining holes in these polygons have a counterclockwise orientation.

                bool[] IsCounterClockWise = new bool[rings.Count];

                int PolygonCount = 0;

                //determine the orientation of each ring.
                for (int i = 0; i < rings.Count; i++)
                {
                    IsCounterClockWise[i] = rings[i].IsCCW();

                    if (!IsCounterClockWise[i])
                    {
                        PolygonCount++; //count the number of polygons
                    }
                }

                //if the polygon count is 1 then there is only one polygon to create.
                if (PolygonCount == 1)
                {
                    Polygon polygon = new Polygon();
                    polygon.ExteriorRing = rings[0];
                    if (rings.Count > 1)
                    {
                        for (int i = 1; i < rings.Count; i++)
                        {
                            polygon.InteriorRings.Add(rings[i]);
                        }
                    }

                    item = polygon;
                }
                else
                {
                    MultiPolygon multPolygon = new MultiPolygon();
                    Polygon poly = new Polygon();
                    poly.ExteriorRing = rings[0];

                    for (int i = 1; i < rings.Count; i++)
                    {
                        if (!IsCounterClockWise[i])
                        {
                            multPolygon.Geometries.Add(poly);
                            poly = new Polygon(rings[i]);
                        }
                        else
                        {
                            poly.InteriorRings.Add(rings[i]);
                        }
                    }

                    multPolygon.Geometries.Add(poly);

                    item = multPolygon;
                }

                item.Metadata.ID = id.ToString();
                item.Metadata.Properties.Add("Bounding Box", boundingBox);

                items.Add(item);
            }

            return items;
        }

        /// <summary>
        /// Reads MultiPoint shapefile
        /// </summary>
        /// <param name="reader">The Shapefile stream</param>
        /// <returns>A list of Geometry objects</returns>
        private List<Geometry> ReadMultiPointData(BinaryReader reader)
        {
            var items = new List<Geometry>();
            int numPoints;

            // For each header                
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                Geometry item;

                // Read Record Header.
                int id = ReadRecordHeader(reader);
                var boundingBox = ReadBoundingBox(reader);

                // Num Points.
                numPoints = NumberReader.ReadIntLE(reader);

                if (numPoints == 1) //if there is only one point then create a point geography
                {
                    item = new Point(ReadCoordinate(reader));
                }
                else
                {
                    MultiPoint mp = new MultiPoint(numPoints);

                    // Read in all the Points.           
                    for (int i = 0; i < numPoints; i++)
                    {
                        mp.Geometries.Add(new Point(ReadCoordinate(reader)));
                    }

                    item = mp;
                }

                item.Metadata.ID = id.ToString();
                item.Metadata.Properties.Add("Bounding Box", boundingBox);

                items.Add(item);
            }

            return items;
        }

        #endregion
    }
}