/**************************************************************************** 
 * Author: Ricky Brundritt
 * 
 * Description:
 * This class allows you to read and write GPX files. This code supports 
 * version 1.1 of the GPX schema. 
 * 
 * Notes: 
 * - trkpt & rtept tags are treated as coordinates
 * - Only the href property of link tags are captured
 * - GPX author and copyright tags are not supported.
 * - Partial support for Garmin GPX Extensions V3. (Metadata keys drop "gpxx:")
 * 
 * Garmin GPX Extensions: http://developer.garmin.com/schemas/gpxx/v3/
 * 
 * - gpxx:WaypintExtension
 *   - gpxx:DisplayMode
 *   - gpxx:Temperature
 *   - gpxx:Proximity
 *   - gpxx:Depth
 *   - gpxx:Address
 *      - gpxx:StreetAddress
 *      - gpxx:City
 *      - gpxx:State
 *      - gpxx:Country
 *      - gpxx:PostalCode
 *   - gpxx:Categories
 *      - gpxx:Category -> Metadata key stored as "Category:#" where # = [0..*]
 * - gpxx:TrackExtension
 *  - gpxx:DisplayColor (Read only)
 * - gpxx:RouteExtension
 *   - gpxx:DisplayColor (Read only)
 *  
 * Supported Namespaces:
 * http://www.topografix.com/GPX/1/1
 * http://www.garmin.com/xmlschemas/GpxExtensions/v3
 * 
 ****************************************************************************/

using Microsoft.Maps.SpatialToolbox.Internals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Maps.SpatialToolbox.IO
{
    /// <summary>
    /// A class that reads and write GPX formated data. 
    /// </summary>
    public class GpxFeed : BaseTextFeed
    {
        #region Private Properties

        private int embeddedStyleCnt = 0;
        private const string GpxNamespace = "http://www.topografix.com/GPX/1/1";
        private const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";
        private const string GpxxNamespace = "http://www.garmin.com/xmlschemas/GpxExtensions/v3";

        private XNamespace gpxxNS = XNamespace.Get(GpxxNamespace);

        private bool readRouteWaypoints = false;

        #endregion

        #region Constructor

        public GpxFeed()
        {
        }

        public GpxFeed(bool stripHtml)
            : base(stripHtml)
        {
        }

        public GpxFeed(double tolerance)
            : base(tolerance)
        {            
        }

        public GpxFeed(bool stripHtml, double tolerance)
            : base(stripHtml, tolerance)
        {
        }

        public GpxFeed(bool stripHtml, bool readRouteWaypoints)
            : base(stripHtml)
        {
            this.readRouteWaypoints = readRouteWaypoints;
        }

        public GpxFeed(bool stripHtml, bool readRouteWaypoints, double tolerance)
            : base(stripHtml, tolerance)
        {
            this.readRouteWaypoints = readRouteWaypoints;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads a GPX feed
        /// </summary>
        /// <param name="xml">GPX feed as a string.</param>
        /// <param name="stripHtml">Option to strip out HTML from string fields.</param>
        /// <returns>A SpatialDataSet containing the spatial data from the GPX feed.</returns>
        public override Task<SpatialDataSet> ReadAsync(string xml)
        {
            return Task.Run<SpatialDataSet>(async () =>
            {
                try
                {
                    return await ParseGPX(xml);
                }
                catch { }

                return null;
            });
        }

        /// <summary>
        /// Reads a GPX feed.
        /// </summary>
        /// <param name="xmlUri">Uri to GPX feed.</param>
        /// <returns>A SpatialDataSet containing the spatial data from the GPX feed.</returns>
        public override Task<SpatialDataSet> ReadAsync(Uri xmlUri)
        {
            return Task.Run<SpatialDataSet>(async () =>
            {
                try
                {
                    using (var s = await ServiceHelper.GetStreamAsync(xmlUri))
                    {
                        var doc = XDocument.Load(s);
                        return await ParseGPX(doc, GetBaseUri(xmlUri));
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
        /// Reads a GPX feed. Strips out HTML tags by default.
        /// </summary>
        /// <param name="xmlStream">GPX feed as a Stream.</param>
        /// <returns>A SpatialDataSet containing the spatial data from the GPX feed.</returns>
        public override Task<SpatialDataSet> ReadAsync(Stream stream)
        {
            return Task.Run<SpatialDataSet>(() =>
            {
                return ParseGPX(stream);
            });
        }

        /// <summary>
        /// Writes a spatial data set object to a Stream as GPX
        /// </summary>
        /// <param name="geometries">A spatial data set to write</param>
        /// <param name="stream">Stream to write GPX to</param>
        public override Task WriteAsync(SpatialDataSet data, Stream stream)
        {
            return Task.Run(() =>
            {
                using (var xWriter = XmlWriter.Create(stream, xmlWriterSettings))
                {
                    Write(data, xWriter);
                }
            });
        }

        /// <summary>
        /// Writes a patial data set object to a TextWriter as GPX
        /// </summary>
        /// <param name="geometries">A patial data set to write</param>
        /// <returns>A string of GPX.</returns>
        public override Task<string> WriteAsync(SpatialDataSet data)
        {
            return Task.Run<string>(() =>
            {
                var sb = new StringBuilder();
                using (var xWriter = XmlWriter.Create(sb, xmlWriterSettings))
                {
                    Write(data, xWriter);
                }

                return sb.ToString();
            });
        }


        #endregion

        #region Private Methods

        #region GPX Reading Methods

        private async Task<SpatialDataSet> ParseGPX(string gpxString)
        {
            using (var gpxReader = new StringReader(gpxString))
            {
                var nt = new NameTable();
                var nsmgr = new XmlNamespaceManager(nt);
                nsmgr.AddNamespace("gpxx", GpxxNamespace);

                var pc = new XmlParserContext(null, nsmgr, null, XmlSpace.None);

                using (var reader = XmlReader.Create(gpxReader, null, pc))
                {
                    var doc = XDocument.Load(reader);
                    return await ParseGPX(doc, string.Empty);
                }
            }
        }

        private async Task<SpatialDataSet> ParseGPX(Stream gpxStream)
        {
            var nt = new NameTable();
            var nsmgr = new XmlNamespaceManager(nt);
            nsmgr.AddNamespace("gpxx", GpxxNamespace);

            var pc = new XmlParserContext(null, nsmgr, null, XmlSpace.None);

            using(var reader = XmlReader.Create(gpxStream, null, pc))
            {
                var doc = XDocument.Load(reader);            
                return await ParseGPX(doc, string.Empty);
            }
        }

        private async Task<SpatialDataSet> ParseGPX(XDocument doc, string baseUri)
        {
            var result = new SpatialDataSet();

            try
            {
                var geoms = new List<Geometry>();
                var styles = new Dictionary<string, ShapeStyle>();

                var gpx = doc.Root;// XmlUtilities.GetChildNode(doc.Root, "gpx");
                if (gpx != null)
                {
                    string nodeName;                    

                    foreach (var n in gpx.Elements())
                    {
                        nodeName = n.Name.LocalName;
                        switch (nodeName)
                        {
                            case "wpt":
                                var p = ParseWaypoint(n, styles);
                                if (p != null)
                                {
                                    geoms.Add(p);
                                }
                                break;
                            case "rte":
                                var r = await ParseRoute(n, styles);
                                if (r != null)
                                {
                                    geoms.AddRange(r);
                                }
                                break;
                            case "trk":
                                var t = await ParseTrack(n, styles);
                                if (t != null)
                                {
                                    geoms.Add(t);
                                }
                                break;
                            case "metadata":
                                BoundingBox bounds;
                                var m = ParseDocMetadata(n, out bounds);
                                if (m != null)
                                {
                                    result.Metadata = m;
                                }
                                if (bounds != null)
                                {
                                    result.BoundingBox = bounds;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }                

                if (geoms.Count > 0)
                {
                    result.Geometries = geoms;

                    if (styles.Count > 0)
                    {
                        result.Styles = styles;
                    }

                    if (result.BoundingBox == null)
                    {
                        result.BoundingBox = geoms.Envelope();
                    }
                }
            }
            catch(Exception ex){
                result.Error = ex.Message;                
            }

            return result;
        }

        private ShapeMetadata ParseDocMetadata(XElement node, out BoundingBox bounds)
        {
            var metadata = new ShapeMetadata();
            bounds = null;

            string nodeName;

            foreach (var n in node.Elements())
            {
                nodeName = n.Name.LocalName;
                switch (nodeName)
                {
                    case "name":
                        metadata.Title = XmlUtilities.GetString(n, stripHtml);
                        break;
                    case "desc":
                        metadata.Description = XmlUtilities.GetString(n, stripHtml);
                        break;
                    case "keywords":
                        SetMetadataString(metadata, nodeName, n, stripHtml);
                        break;
                    case "bounds":
                        var minLat = XmlUtilities.GetDoubleAttribute(n, "minlat");
                        var minLon = XmlUtilities.GetDoubleAttribute(n, "minlon");
                        var maxLat = XmlUtilities.GetDoubleAttribute(n, "maxlat");
                        var maxLon = XmlUtilities.GetDoubleAttribute(n, "maxlon");

                        if (!double.IsNaN(minLat) && !double.IsNaN(minLon) 
                            && !double.IsNaN(maxLat) && !double.IsNaN(maxLon))
                        {
                            //Not all documents properly order the latitude coordinates. 
                            //Assume that the longitude coordinates are correct
                            var maxY = Math.Max(minLat, maxLat);
                            var minY = Math.Min(minLat, maxLat);
                            var minX = minLon;
                            var maxX = maxLon;
                            bounds = new BoundingBox(minX, maxY, maxX, minY);
                        }
                        break;
                    case "link":
                        var href = XmlUtilities.GetStringAttribute(n, "href");

                        if (!string.IsNullOrWhiteSpace(href) && !metadata.Properties.ContainsKey(nodeName))
                        {
                            metadata.Properties.Add(nodeName, href);
                        }
                        break;
                    case "time":
                        var time = XmlUtilities.GetDateTime(n);
                        if (time.HasValue && !metadata.Properties.ContainsKey(nodeName))
                        {
                            metadata.Properties.Add(nodeName, time);
                        }
                        break;
                    default:
                        break;
                }
            }

            return metadata;
        }

        private Point ParseWaypoint(XElement node, Dictionary<string, ShapeStyle> styles)
        {
            var lat = XmlUtilities.GetDoubleAttribute(node, "lat");
            var lon = XmlUtilities.GetDoubleAttribute(node, "lon");

            if (!double.IsNaN(lat) && !double.IsNaN(lon))
            {
                var point = new Point(new Coordinate(lat, lon));
                var metadata = new ShapeMetadata();
                var style = new ShapeStyle();
                string nodeName;

                foreach (var n in node.Elements())
                {
                    nodeName = n.Name.LocalName;
                    switch (nodeName)
                    {
                        case "name":
                            metadata.Title = XmlUtilities.GetString(n, stripHtml);
                            break;
                        case "desc":
                            metadata.Description = XmlUtilities.GetString(n, stripHtml);
                            break;
                        case "cmt":
                        case "src":
                        case "sym":
                        case "type":
                        case "fix":
                            SetMetadataString(metadata, nodeName, n, stripHtml);
                            break;
                        case "sat"://xsd:nonNegativeInteger
                        case "dgpsid":
                            var i = XmlUtilities.GetInt32(n, -1);
                            if (i >= 0 && !metadata.Properties.ContainsKey(nodeName))
                            {
                                metadata.Properties.Add(nodeName, i);
                            }
                            break;
                        case "magvar":
                        case "hdop":
                        case "vdop":
                        case "pdop":
                        case "ageofdgpsdata":
                        case "geoidheight":
                            var d = XmlUtilities.GetDouble(n, double.NaN);
                            if (!double.IsNaN(d) && !metadata.Properties.ContainsKey(nodeName))
                            {
                                metadata.Properties.Add(nodeName, d);
                            }
                            break;
                        case "link":
                            var href = XmlUtilities.GetStringAttribute(n, "href");

                            if (!string.IsNullOrWhiteSpace(href) && !metadata.Properties.ContainsKey(nodeName))
                            {
                                metadata.Properties.Add(nodeName, href);
                            }
                            break;
                        case "ele":
                            var ele = XmlUtilities.GetDouble(n, double.NaN);
                            if (!double.IsNaN(ele))
                            {
                                point.Coordinate = new Coordinate(point.Coordinate.Latitude, point.Coordinate.Longitude, ele);
                            }
                            break;
                        case "time":
                            var time = XmlUtilities.GetDateTime(n);
                            if (time.HasValue && !metadata.Properties.ContainsKey(nodeName))
                            {
                                metadata.Properties.Add(nodeName, time);
                            }
                            break;
                        case "extensions":
                            ParseExtensions(n, metadata, style);
                            break;
                        default:
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(metadata.Title) || !string.IsNullOrEmpty(metadata.Description) || metadata.Properties.Count > 0)
                {
                    point.Metadata = metadata;
                }

                if (style.StrokeColor.HasValue)
                {
                    var styleKey = "embeddedStyle_" + embeddedStyleCnt;
                    embeddedStyleCnt++;
                    styles.Add(styleKey, style);
                    point.StyleKey = styleKey;
                }

                return point;
            }

            return null;
        }

        private Coordinate? ParseWaypointAsCoordinate(XElement node)
        {
            var lat = XmlUtilities.GetDoubleAttribute(node, "lat");
            var lon = XmlUtilities.GetDoubleAttribute(node, "lon");

            if (!double.IsNaN(lat) && !double.IsNaN(lon))
            {
                var ele = XmlUtilities.GetDouble(node, "ele", double.NaN);

                if (!double.IsNaN(ele))
                {
                    return new Coordinate(lat, lon, ele);
                }

                return new Coordinate(lat, lon);
            }

            return null;
        }

        private async Task<MultiLineString> ParseTrack(XElement node, Dictionary<string, ShapeStyle> styles)
        {
            var lines = new MultiLineString();
            var metadata = new ShapeMetadata();
            var style = new ShapeStyle();
            string nodeName;

            foreach (var n in node.Elements())
            {
                nodeName = n.Name.LocalName;
                switch (nodeName)
                {
                    case "name":
                        metadata.Title = XmlUtilities.GetString(n, stripHtml);
                        break;
                    case "desc":
                        metadata.Description = XmlUtilities.GetString(n, stripHtml);
                        break;
                    case "cmt":
                    case "src":
                    case "type":
                        SetMetadataString(metadata, nodeName, n, stripHtml);
                        break;
                    case "number"://<number> xsd:nonNegativeInteger </number> [0..1] ?
                        var i = XmlUtilities.GetInt32(n, -1);
                        if (i >= 0 && !metadata.Properties.ContainsKey(nodeName))
                        {
                            metadata.Properties.Add(nodeName, i);
                        }
                        break;
                    case "trkseg":
                        var seg = await ParseTrackSegment(n);
                        if (seg != null && seg.Vertices != null && seg.Vertices.Count >= 2)
                        {
                            lines.Geometries.Add(seg);
                        }
                        break;
                    case "link":
                        var href = XmlUtilities.GetStringAttribute(n, "href");

                        if (!string.IsNullOrWhiteSpace(href) && !metadata.Properties.ContainsKey(nodeName))
                        {
                            metadata.Properties.Add(nodeName, href);
                        }
                        break;
                    case "extensions":
                        ParseExtensions(n, metadata, style);
                        break;
                    default:
                        break;
                }
            }

            if (!string.IsNullOrEmpty(metadata.Title) || !string.IsNullOrEmpty(metadata.Description) || metadata.Properties.Count > 0)
            {
                lines.Metadata = metadata;
            }

            if (style.StrokeColor.HasValue)
            {
                var styleKey = "embeddedStyle_" + embeddedStyleCnt;
                embeddedStyleCnt++;
                styles.Add(styleKey, style);
                lines.StyleKey = styleKey;
            }

            if(lines.Geometries.Count > 0)
            {
                return lines;
            }

            return null;
        }

        private async Task<LineString> ParseTrackSegment(XElement node)
        {
            var points = XmlUtilities.GetChildNodes(node, "trkpt");
            var coords = new CoordinateCollection();

            foreach (var p in points)
            {
                var c = ParseWaypointAsCoordinate(p);
                if (c.HasValue)
                {
                    coords.Add(c.Value);
                }
            }

            if (coords.Count >= 0)
            {
                if (optimize)
                {
                    coords = await SpatialTools.VertexReductionAsync(coords, tolerance);
                }

                return new LineString(coords);
            }

            return null;
        }

        private async Task<List<Geometry>> ParseRoute(XElement node, Dictionary<string, ShapeStyle> styles)
        {
            var geoms = new List<Geometry>();
            var line = new LineString();
            var metadata = new ShapeMetadata();
            var coords = new CoordinateCollection();
            var style = new ShapeStyle();
            string nodeName;

            foreach (var n in node.Elements())
            {
                nodeName = n.Name.LocalName;
                switch (nodeName)
                {
                    case "name":
                        metadata.Title = XmlUtilities.GetString(n, stripHtml);
                        break;
                    case "desc":
                        metadata.Description = XmlUtilities.GetString(n, stripHtml);
                        break;
                    case "cmt":
                    case "src":
                    case "type":
                        SetMetadataString(metadata, nodeName, n, stripHtml);
                        break;
                    case "number"://<number> xsd:nonNegativeInteger </number> [0..1] ?
                        var i = XmlUtilities.GetInt32(n, -1);
                        if (i >= 0 && !metadata.Properties.ContainsKey(nodeName))
                        {
                            metadata.Properties.Add(nodeName, i);
                        }
                        break;
                    case "link":
                        var href = XmlUtilities.GetStringAttribute(n, "href");

                        if (!string.IsNullOrWhiteSpace(href) && !metadata.Properties.ContainsKey(nodeName))
                        {
                            metadata.Properties.Add(nodeName, href);
                        }
                        break;
                    case "rtept":
                        if (readRouteWaypoints)
                        {
                            var wpt = ParseWaypoint(n, styles);
                            coords.Add(wpt.Coordinate);
                            geoms.Add(wpt);
                        }
                        else
                        {
                            var c = ParseWaypointAsCoordinate(n);
                            if (c.HasValue)
                            {
                                coords.Add(c.Value);
                            }
                        }
                        break;
                    case "extensions":
                        ParseExtensions(n, metadata, style);
                        break;
                    default:
                        break;
                }
            }

            if (!string.IsNullOrEmpty(metadata.Title) || !string.IsNullOrEmpty(metadata.Description) || metadata.Properties.Count > 0)
            {
                line.Metadata = metadata;
            }

            if (style.StrokeColor.HasValue)
            {
                var styleKey = "embeddedStyle_" + embeddedStyleCnt;
                embeddedStyleCnt++;
                styles.Add(styleKey, style);
                line.StyleKey = styleKey;
            }

            if (coords.Count >= 2)
            {
                if (optimize)
                {
                    coords = await SpatialTools.VertexReductionAsync(coords, tolerance);
                }

                line.Vertices = coords;

                geoms.Add(line);
            }

            return geoms;
        }

        private void ParseExtensions(XElement node, ShapeMetadata metadata, ShapeStyle style)
        {
            //<extensions>
            //<gpxx:WaypointExtension/>
            //</extensions>

            if (node != null)
            {
                string nodeName;

                foreach (var n in node.Elements())
                {
                    if (n.Name.Namespace == gpxxNS)
                    {
                        nodeName = n.Name.LocalName;
                        switch (nodeName)
                        {
                            case "WaypointExtension":
                                ParseWaypointExtensions(n, metadata);
                                break;
                            case "RouteExtension":
                            case "TrackExtension":
                                ParseRouteTrackExtensions(n, metadata, style);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private void ParseWaypointExtensions(XElement node, ShapeMetadata metadata)
        {
            //<gpxx:WaypointExtension>
            //<gpxx:Categories/>
            //<gpxx:Address/>
            //</gpxx:WaypointExtension>

            if (node != null)
            {
                string nodeName;

                foreach (var n in node.Elements())
                {
                    nodeName = n.Name.LocalName;
                    if (n.Name.Namespace == gpxxNS)
                    {
                        switch (nodeName)
                        {
                            case "DisplayMode":
                                SetMetadataString(metadata, nodeName, n, stripHtml);
                                break;
                            case "Temperature":
                            case "Proximity":
                            case "Depth":
                                var d = XmlUtilities.GetDouble(n, double.NaN);
                                if (!double.IsNaN(d) && !metadata.Properties.ContainsKey(nodeName))
                                {
                                    metadata.Properties.Add(nodeName, d);
                                }
                                break;
                            case "Categories":
                                ParseCategories(n, metadata);
                                break;
                            case "Address":
                                ParseAddress(n, metadata);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private void ParseRouteTrackExtensions(XElement node, ShapeMetadata metadata, ShapeStyle style)
        {
            if (node != null)
            {
                string nodeName;

                foreach (var n in node.Elements())
                {
                    if (n.Name.Namespace == gpxxNS)
                    {
                        nodeName = n.Name.LocalName;
                        switch (nodeName)
                        {
                            case "DisplayColor":
                                var c = XmlUtilities.GetColor(n);
                                if (c.HasValue)
                                {
                                    style.StrokeColor = c;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private void ParseAddress(XElement node, ShapeMetadata metadata)
        {
            //<gpxx:Address>
            //<gpxx:StreetAddress>16900 North RR 620</gpxx:StreetAddress>
            //<gpxx:City>Round Rock</gpxx:City>
            //<gpxx:State>TX</gpxx:State>
            //<gpxx:Country>USA</gpxx:Country>
            //<gpxx:PostalCode>78681-3922</gpxx:PostalCode>
            //</gpxx:Address>

            if (node != null)
            {
                string nodeName;

                foreach (var n in node.Elements())
                {
                    if (n.Name.Namespace == gpxxNS)
                    {
                        nodeName = n.Name.LocalName;
                        switch (nodeName)
                        {
                            case "StreetAddress":
                            case "City":
                            case "State":
                            case "Country":
                            case "PostalCode":
                                SetMetadataString(metadata, nodeName, n, stripHtml);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private void ParseCategories(XElement node, ShapeMetadata metadata)
        {
            //<gpxx:Categories>
            //<gpxx:Category>grocery store</gpxx:Category> [1...*]
            //</gpxx:Categories>

            if (node != null)
            {
                string nodeName;
                int idx = 0;

                foreach (var n in node.Elements())
                {
                    if (n.Name.Namespace == gpxxNS)
                    {
                        nodeName = n.Name.LocalName;
                        switch (nodeName)
                        {
                            case "Category":
                                SetMetadataString(metadata, nodeName + ":" + idx, n, stripHtml);
                                idx++;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        #endregion

        #region GPX Write Methods

        private void Write(SpatialDataSet data, XmlWriter xmlWriter)
        {
            //Open document
            xmlWriter.WriteStartDocument(true);

            //Write root tag and namespaces.
            xmlWriter.WriteStartElement("gpx", GpxNamespace);
            xmlWriter.WriteAttributeString("version", "1.1");
            xmlWriter.WriteAttributeString("xmlns", "gpxx", "http://www.w3.org/2000/xmlns/", GpxxNamespace);
            xmlWriter.WriteAttributeString("xmlns", "xsi", "http://www.w3.org/2000/xmlns/", XsiNamespace);
            xmlWriter.WriteAttributeString("xsi", "schemaLocation", XsiNamespace, "http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd");

            //Write document metadata.
            WriteDocumentMetadata(data, xmlWriter);

            foreach (var item in data.Geometries)
            {
                WriteGeometry(item, xmlWriter);
            }

            //Close feed tag
            xmlWriter.WriteEndElement();

            //Close document
            xmlWriter.WriteEndDocument();
        }

        private void WriteDocumentMetadata(SpatialDataSet data, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("metadata");

            var bounds = data.BoundingBox;
            if(bounds == null){
                bounds = ((List<Geometry>)data.Geometries).Envelope();
            }

            if(bounds != null){
                //Write bounds            
                xmlWriter.WriteStartElement("bounds");
                xmlWriter.WriteAttributeString("maxlat", bounds.MaxY.ToString("N5"));
                xmlWriter.WriteAttributeString("minlat", bounds.MinY.ToString("N5"));
                xmlWriter.WriteAttributeString("maxlon", bounds.MaxX.ToString("N5"));
                xmlWriter.WriteAttributeString("maxlon", bounds.MinX.ToString("N5"));
                xmlWriter.WriteEndElement();
            }

            if (data.Metadata != null)
            {
                if (!string.IsNullOrEmpty(data.Metadata.Title))
                {
                    xmlWriter.WriteStartElement("name");
                    xmlWriter.WriteValue(data.Metadata.Title);
                    xmlWriter.WriteEndElement();
                }

                if (!string.IsNullOrEmpty(data.Metadata.Description))
                {
                    xmlWriter.WriteStartElement("desc");
                    xmlWriter.WriteValue(data.Metadata.Description);
                    xmlWriter.WriteEndElement();
                }

                object val;

                foreach (var t in data.Metadata.Properties)
                {
                    val = t.Value;

                    if (val != null)
                    {
                        switch (t.Key)
                        {
                            case "keywords":
                                xmlWriter.WriteStartElement(t.Key);
                                xmlWriter.WriteValue(val as string);
                                xmlWriter.WriteEndElement();
                                break;
                            case "link":    //   <link href="http://example.org/"/>
                                xmlWriter.WriteStartElement(t.Key);
                                xmlWriter.WriteAttributeString("href", val as string);
                                xmlWriter.WriteEndElement();
                                break;
                            case "time":
                                xmlWriter.WriteStartElement(t.Key);
                                xmlWriter.WriteValue(((DateTime)val).ToString());
                                xmlWriter.WriteEndElement();
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            xmlWriter.WriteEndElement();
        }

        private void WriteMetadata(ShapeMetadata metadata, XmlWriter xmlWriter)
        {
            if (metadata != null)
            {
                if (!string.IsNullOrEmpty(metadata.Title))
                {
                    xmlWriter.WriteStartElement("name");
                    xmlWriter.WriteValue(metadata.Title);
                    xmlWriter.WriteEndElement();
                }

                if (!string.IsNullOrEmpty(metadata.Description))
                {
                    xmlWriter.WriteStartElement("desc");
                    xmlWriter.WriteValue(metadata.Description);
                    xmlWriter.WriteEndElement();
                }

                object val;

                var extensions = new Dictionary<string, object>();

                foreach (var t in metadata.Properties)
                {
                    val = t.Value;

                    if (val != null)
                    {
                        switch (t.Key)
                        {
                            case "name":
                            case "desc":
                            case "cmt":
                            case "src":
                            case "sym":
                            case "type":
                            case "fix":
                                xmlWriter.WriteStartElement(t.Key);
                                xmlWriter.WriteValue(val as string);
                                xmlWriter.WriteEndElement();
                                break;
                            case "link":    //   <link href="http://example.org/"/>
                                xmlWriter.WriteStartElement(t.Key);
                                xmlWriter.WriteAttributeString("href", val as string);
                                xmlWriter.WriteEndElement();
                                break;
                            case "time":
                                xmlWriter.WriteStartElement(t.Key);
                                xmlWriter.WriteValue(((DateTime)val).ToString());
                                xmlWriter.WriteEndElement();
                                break;
                            case "sat"://xsd:nonNegativeInteger
                            case "dgpsid":
                                xmlWriter.WriteStartElement(t.Key);
                                xmlWriter.WriteValue(((int)val).ToString());
                                xmlWriter.WriteEndElement();
                                break;
                            case "magvar":
                            case "hdop":
                            case "vdop":
                            case "pdop":
                            case "ageofdgpsdata":
                            case "geoidheight":
                                xmlWriter.WriteStartElement(t.Key);
                                xmlWriter.WriteValue(((double)val).ToString());
                                xmlWriter.WriteEndElement();
                                break;
                            case "streetaddress":
                            case "city":
                            case "state":
                            case "country":
                            case "postalcode":
                                extensions.Add(t.Key, val);
                                break;
                            default:
                                if (t.Key.StartsWith("Category:"))
                                {
                                    if (!extensions.ContainsKey("Categories"))
                                    {
                                        extensions.Add("Categories", new List<string>());
                                    }

                                    (extensions["Categories"] as List<string>).Add(val as string);
                                }
                                break;
                        }
                    }
                }

                if(extensions.Count > 0)
                {
                    xmlWriter.WriteStartElement("extensions");

                    if (extensions.ContainsKey("Categories"))
                    {
                        xmlWriter.WriteStartElement("Categories", GpxxNamespace);
                        foreach (var c in extensions["Categories"] as List<string>)
                        {
                            xmlWriter.WriteStartElement("Category", GpxxNamespace);
                            xmlWriter.WriteValue(c);
                            xmlWriter.WriteEndElement();
                        }
                        xmlWriter.WriteEndElement();
                    }

                    if (extensions.Count > 0)
                    {
                        xmlWriter.WriteStartElement("Address", GpxxNamespace);
                        foreach (var t in extensions)
                        {
                            val = t.Value;

                            if (val != null)
                            {
                                switch (t.Key)
                                {
                                    case "StreetAddress":
                                    case "City":
                                    case "State":
                                    case "Country":
                                    case "PostalCode":
                                        xmlWriter.WriteStartElement(t.Key, GpxxNamespace);
                                        xmlWriter.WriteString(val as string);
                                        xmlWriter.WriteEndElement();
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        xmlWriter.WriteEndElement();
                    }
                }
            }
        }

        private void WriteGeometry(Geometry geometry, XmlWriter xmlWriter)
        {
            if (geometry is Point)
            {
                WritePoint(geometry as Point, xmlWriter);
            }
            else if (geometry is LineString)
            {
                WriteLineString(geometry as LineString, xmlWriter);
            }
            else if (geometry is MultiPoint)
            {
                WriteMultiPoint(geometry as MultiPoint, xmlWriter);
            }
            else if (geometry is MultiLineString)
            {
                WriteMultiLineString(geometry as MultiLineString, xmlWriter);
            }
            else if (geometry is GeometryCollection)
            {
                foreach (var g in geometry as GeometryCollection)
                {
                    WriteGeometry(g, xmlWriter);
                }
            }
        }

        private void WritePoint(Point point, XmlWriter xmlWriter)
        {
            //<wpt lat="51.503244" lon="-0.127644">
            //  <name>10 Downing Street</name>
            //</wpt>

            if(point != null)
            {
                xmlWriter.WriteStartElement("wpt");
                xmlWriter.WriteAttributeString("lat", point.Coordinate.Latitude.ToString("N5"));
                xmlWriter.WriteAttributeString("lon", point.Coordinate.Longitude.ToString("N5"));

                if (point.Coordinate.Altitude.HasValue)
                {
                    xmlWriter.WriteStartElement("ele");
                    xmlWriter.WriteValue(point.Coordinate.Altitude.Value);
                    xmlWriter.WriteEndElement();
                }

                if (point.Metadata != null)
                {
                    WriteMetadata(point.Metadata, xmlWriter);
                }
                
                xmlWriter.WriteEndElement();
            }
        }

        //Treat LineStrings as Routes
        private void WriteLineString(LineString line, XmlWriter xmlWriter)
        {
            if (line != null)
            {
                xmlWriter.WriteStartElement("rte");

                if (line.Metadata != null)
                {
                    WriteMetadata(line.Metadata, xmlWriter);
                }

                foreach (var c in line.Vertices)
                {
                    xmlWriter.WriteStartElement("rtept");
                    xmlWriter.WriteAttributeString("lat", c.Latitude.ToString("N5"));
                    xmlWriter.WriteAttributeString("lon", c.Longitude.ToString("N5"));

                    if (c.Altitude.HasValue)
                    {
                        xmlWriter.WriteStartElement("ele");
                        xmlWriter.WriteValue(c.Altitude.Value);
                        xmlWriter.WriteEndElement();
                    }

                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
            }
        }

        private void WriteMultiPoint(MultiPoint points, XmlWriter xmlWriter)
        {
            foreach (var p in points)
            {
                WritePoint(p, xmlWriter);
            }
        }

        //Treat MultiLineStrings as Tracks
        private void WriteMultiLineString(MultiLineString multiLineString, XmlWriter xmlWriter)
        {
            if (multiLineString != null)
            {
                xmlWriter.WriteStartElement("trk");

                if (multiLineString.Metadata != null)
                {
                    WriteMetadata(multiLineString.Metadata, xmlWriter);
                }

                foreach (var line in multiLineString)
                {
                    xmlWriter.WriteStartElement("trkseg");

                    foreach (var c in line.Vertices)
                    {
                        xmlWriter.WriteStartElement("wpt");
                        xmlWriter.WriteAttributeString("lat", c.Latitude.ToString("N5"));
                        xmlWriter.WriteAttributeString("lon", c.Longitude.ToString("N5"));

                        if (c.Altitude.HasValue)
                        {
                            xmlWriter.WriteStartElement("ele");
                            xmlWriter.WriteValue(c.Altitude.Value);
                            xmlWriter.WriteEndElement();
                        }

                        xmlWriter.WriteEndElement();
                    }

                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
            }
        }

        #endregion

        #endregion
    }
}
