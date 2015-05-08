/****************************************************************************
* Description:
* This class allows you to read and write GeoRSS files into Bing Maps.
*
* Currently supports:
*
* Feed Tags:
*   - item
*   - entry
*
* GeoRSS Tags:
*   - georss:point
*   - georss:line
*   - georss:polygon
*   - georss:circle
*   - georss:where
*   - geo:lat/geo:long/geo:lon
*
* GML Tags:
*   - gml:point
*   - gml:lineString
*   - gml:polygon
*   - gml:pos
*   - gml:coordinates
*   - gml:poslist
*   - gml:interior
*   - gml:exterior
*   - gml:linearring
*   - gml:outerboundaryis
*   - gml:innerboundaryis
*
* Metadata Tags:
*   - title
*   - content/summary/description
*   - icon/mappoint:icon
*   - link
*
****************************************************************************/

using Microsoft.Maps.SpatialToolbox.Internals;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Maps.SpatialToolbox.IO
{
    /// <summary>
    /// A class for reading and writing GeoRSS files
    /// </summary>
    public class GeoRssFeed : BaseTextFeed
    {
        #region Private Properties

        private int embeddedStyleCnt = 0;

        private Regex SpacesRx = new Regex(@"\s{2,}");
        private Regex CoordArtifactRx = new Regex(@"[\n\t,]");
        private string[] SpaceSplitter = new string[] {" "};

        private const string GeoRssNamespace = "http://www.georss.org/georss";
        private const string MapPointNamespace = "http://virtualearth.msn.com/apis/annotate#";
        private const string GmlNamespace = "http://www.opengis.net/gml";
        private const string GeoNamespace = "http://www.w3.org/2003/01/geo/wgs84_pos#";

        private XNamespace geoRssNS = XNamespace.Get(GeoRssNamespace);

        #endregion

        #region Constructor

        public GeoRssFeed() :
            base()
        {
        }

        public GeoRssFeed(bool stripHtml) :
            base(stripHtml)
        {
        }

        public GeoRssFeed(double tolerance)
            : base(tolerance)
        {
        }

        public GeoRssFeed(bool stripHtml, double tolerance)
            : base(stripHtml, tolerance)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads a GeoRSS xml feed
        /// </summary>
        /// <param name="xml">Xml GeoRSS feed as a string.</param>
        /// <param name="stripHtml">Option to strip out HTML from string fields.</param>
        /// <returns>A SpatialDataSet containing the spatial data from the GeoRSS feed.</returns>
        public override Task<SpatialDataSet> ReadAsync(string xml)
        {
            return Task.Run<SpatialDataSet>(async () =>
            {
                var doc = XDocument.Parse(xml, LoadOptions.SetBaseUri);
                return await ParseGeoRSS(doc, string.Empty);
            });
        }

        /// <summary>
        /// Reads a GeoRSS feed.
        /// </summary>
        /// <param name="xmlUri">Uri to GeoRSS feed.</param>
        /// <returns>A SpatialDataSet containing the spatial data from the GeoRSS feed.</returns>
        public override Task<SpatialDataSet> ReadAsync(Uri xmlUri)
        {
            return Task.Run<SpatialDataSet>(async () =>
            {
                try
                {
                    using (var s = await ServiceHelper.GetStreamAsync(xmlUri))
                    {
                        var doc = XDocument.Load(s);
                        return await ParseGeoRSS(doc, GetBaseUri(xmlUri));
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
        /// Reads a GeoRSS xml feed. Strips out HTML tags by default.
        /// </summary>
        /// <param name="xmlStream">Xml GeoRSS feed as a Stream.</param>
        /// <returns>A SpatialDataSet containing the spatial data from the GeoRSS feed.</returns>
        public override Task<SpatialDataSet> ReadAsync(Stream stream)
        {
            return Task.Run<SpatialDataSet>(async () =>
            {
                try
                {
                    var doc = XDocument.Load(stream);
                    return await ParseGeoRSS(doc, string.Empty);
                }
                catch
                {
                }

                return null;
            });
        }

        /// <summary>
        /// Writes a spatial data set to a Stream as GeoRSS
        /// </summary>
        /// <param name="data">A spatial data set containing the spatial data.</param>
        /// <param name="stream">A stream to write GeoRSS to.</param>
        public override Task WriteAsync(SpatialDataSet data, Stream stream)
        {
            return Task.Run(() =>
            {
                using (var writer = XmlWriter.Create(stream, xmlWriterSettings))
                {
                    Write(data, writer);
                }
            });
        }

        /// <summary>
        /// Writes a spatial data set as a GeoRSS string.
        /// </summary>
        /// <param name="data">List of geometries to write</param>
        /// <returns>A string of GeoRSS.</returns>
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

        #region GeoRSS Read Methods

        private async Task<SpatialDataSet> ParseGeoRSS(XDocument doc, string baseUri)
        {
            var result = new SpatialDataSet();

            try
            {
                var geoms = new List<Geometry>();
                var styles = new Dictionary<string, ShapeStyle>();

                var channel = XmlUtilities.GetChildNode(doc.Root, "channel");

                if (channel != null)
                {
                    var items = XmlUtilities.GetElementsByTagName(channel, "item");
                    if (items != null)
                    {
                        for (int i = 0; i < items.Count; i++)
                        {
                            var g = await ParseGeoRSSItem(items[i], baseUri, styles);
                            if (g != null)
                            {
                                geoms.Add(g);
                            }
                        }
                    }
                }

                var entries = XmlUtilities.GetElementsByTagName(doc.Root, "entry");
                if (entries != null)
                {
                    for (int i = 0; i < entries.Count; i++)
                    {
                        var g = await ParseGeoRSSItem(entries[i], baseUri, styles);
                        if (g != null)
                        {
                            geoms.Add(g);
                        }
                    }
                }

                if (geoms.Count > 0)
                {
                    if (styles.Count > 0)
                    {
                        result.Styles = styles;
                    }

                    result.Geometries = geoms;
                    result.BoundingBox = geoms.Envelope();
                }
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            return result;
        }

        private async Task<Geometry> ParseGeoRSSItem(XElement node, string baseUri,
            Dictionary<string, ShapeStyle> styles)
        {
            Geometry geom = null;
            var metadata = new ShapeMetadata();

            string nodeName, styleKey = null, id = string.Empty;
            double tLat = double.NaN, tLon = double.NaN;

            var mapPointNS = XNamespace.Get(MapPointNamespace);
            var gmlNS = XNamespace.Get(GmlNamespace);
            var geoNS = XNamespace.Get(GeoNamespace);

            foreach (var n in node.Elements())
            {
                nodeName = n.Name.LocalName;

                if (n.Name.Namespace == geoRssNS)
                {
                    switch (nodeName)
                    {
                        case "point":
                        case "line":
                        case "polygon":
                        case "circle":
                            var polyCoords = ParseCoordinates(n);
                            geom = await ParseGeometry(n);
                            break;
                        case "where":
                            var g = await Gml.Parse(n, optimize, tolerance);
                            if (g != null)
                            {
                                geom = g;
                            }
                            break;
                        default:
                            break;
                    }
                }
                else if (n.Name.Namespace == gmlNS)
                {
                    var gm = await Gml.Parse(node, optimize, tolerance);
                    if (gm != null)
                    {
                        geom = gm;
                    }
                }
                else if (n.Name.Namespace == mapPointNS &&
                         string.Compare(nodeName, "icon", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var s = XmlUtilities.GetString(n, stripHtml);

                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        var uri = CleanUri(s, baseUri);
                        if (uri != null)
                        {
                            //Extract embedded Style
                            styleKey = "embeddedStyle_" + embeddedStyleCnt;
                            embeddedStyleCnt++;

                            if (!styles.ContainsKey(styleKey))
                            {
                                styles.Add(styleKey, new ShapeStyle()
                                {
                                    IconUrl = uri
                                });
                            }
                        }
                    }
                }
                else if (n.Name.Namespace == geoNS)
                {
                    switch (nodeName)
                    {
                        case "lat":
                            tLat = XmlUtilities.GetDouble(n, double.NaN);

                            if (!double.IsNaN(tLat))
                            {
                                if (!double.IsNaN(tLon))
                                {
                                    geom = new Point(tLat, tLon);
                                }
                            }
                            break;
                        case "lon":
                        case "long":
                            tLon = XmlUtilities.GetDouble(n, double.NaN);

                            if (!double.IsNaN(tLon))
                            {
                                if (!double.IsNaN(tLat))
                                {
                                    geom = new Point(tLat, tLon);
                                }
                            }
                            break;
                        case "point":
                            tLat = XmlUtilities.GetDouble(n, "lat", double.NaN);
                            tLon = XmlUtilities.GetDouble(n, "long", double.NaN);

                            if (!double.IsNaN(tLat) && !double.IsNaN(tLon))
                            {
                                geom = new Point(tLat, tLon);
                            }
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    switch (nodeName)
                    {
                        case "title":
                            metadata.Title = XmlUtilities.GetString(n, stripHtml);
                            break;
                        case "description":
                            metadata.Description = XmlUtilities.GetString(n, stripHtml);
                            break;
                        case "content":
                        case "summary":
                            SetMetadataString(metadata, nodeName, n, stripHtml);
                            break;
                        case "id":
                            metadata.ID = XmlUtilities.GetString(n, stripHtml);
                            break;
                        case "icon":
                            var s = XmlUtilities.GetString(n, stripHtml);

                            if (!string.IsNullOrWhiteSpace(s))
                            {
                                var uri = CleanUri(s, baseUri);
                                if (uri != null)
                                {
                                    //Extract embedded Style
                                    styleKey = "embeddedStyle_" + embeddedStyleCnt;
                                    embeddedStyleCnt++;

                                    if (!styles.ContainsKey(styleKey))
                                    {
                                        styles.Add(styleKey, new ShapeStyle()
                                        {
                                            IconUrl = uri
                                        });
                                    }
                                }
                            }
                            break;
                        case "link":
                            var href = XmlUtilities.GetStringAttribute(n, "href");

                            if (!string.IsNullOrWhiteSpace(href) && !metadata.Properties.ContainsKey(nodeName))
                            {
                                metadata.Properties.Add(nodeName, href);
                            }
                            break;
                        case "updated":
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
            }

            if (geom != null)
            {
                if (metadata.HasMetadata())
                {
                    geom.Metadata = metadata;
                }

                geom.StyleKey = styleKey;
            }

            return geom;
        }

        /// <summary>
        /// Parses a string into a coordinate.
        /// </summary>
        private void ParseCoordinate(XElement node, out double lat, out double lon)
        {
            var sCoord = XmlUtilities.GetString(node, false);
            var vals = SplitCoordString(sCoord, CoordArtifactRx, SpaceSplitter);

            if (vals.Length >= 2 && double.TryParse(vals[0], NumberStyles.Float, CultureInfo.InvariantCulture, out lat)
                && double.TryParse(vals[1], NumberStyles.Float, CultureInfo.InvariantCulture, out lon))
            {
                return;
            }

            lat = double.NaN;
            lon = double.NaN;
        }

        private async Task<Geometry> ParseGeometry(XElement node)
        {
            if (node.Name.Namespace == geoRssNS)
                switch (node.Name.LocalName)
                {
                    case "point":
                        double tLat = double.NaN, tLon = double.NaN;
                        ParseCoordinate(node, out tLat, out tLon);
                        if (!double.IsNaN(tLat) && !double.IsNaN(tLon))
                        {
                            return new Point(tLat, tLon);
                        }
                        break;
                    case "line":
                        return await ParseLineString(node);
                    case "polygon":
                        return await ParsePolygon(node);
                    case "circle":
                        var sCoord = XmlUtilities.GetString(node, false);
                        var vals = SplitCoordString(sCoord, CoordArtifactRx, SpaceSplitter);

                        double lon, lat, r;

                        if (vals.Length >= 2
                            && double.TryParse(vals[0], NumberStyles.Float, CultureInfo.InvariantCulture, out lat)
                            && double.TryParse(vals[1], NumberStyles.Float, CultureInfo.InvariantCulture, out lon)
                            && double.TryParse(vals[2], NumberStyles.Float, CultureInfo.InvariantCulture, out r))
                        {
                            var c = new Coordinate(lat, lon);
                            return new Polygon(SpatialTools.GenerateRegularPolygon(c, r, DistanceUnits.Meters, 25, 0.0));
                        }
                        break;
                    default:
                        break;
                }

            return null;
        }

        private async Task<LineString> ParseLineString(XElement node)
        {
            var lineCoords = await ParseCoordinates(node);
            if (lineCoords.Count >= 2)
            {
                return new LineString(lineCoords);
            }

            return null;
        }

        private async Task<Polygon> ParsePolygon(XElement node)
        {
            var polyCoords = await ParseCoordinates(node);
            if (polyCoords.Count >= 2)
            {
                var p = new Polygon(polyCoords);
                await p.MakeValidAsync();
                return p;
            }

            return null;
        }

        /// <summary>
        /// Parses a string list of coordinates. Handles 2D and 3D coordinate sets.
        /// </summary>
        /// <param name="dim">Number of values to represent coordinate.</param>
        /// <returns></returns>
        private async Task<CoordinateCollection> ParseCoordinates(XElement node)
        {
            if (node != null)
            {
                var sCoord = XmlUtilities.GetString(node, false);
                var vals = SplitCoordString(sCoord, CoordArtifactRx, SpaceSplitter);

                var c = new CoordinateCollection();
                double lon, lat;

                if (vals.Length >= 2)
                {
                    for (int i = 0; i < vals.Length; i = i + 2)
                    {
                        if (double.TryParse(vals[i], NumberStyles.Float, CultureInfo.InvariantCulture, out lat)
                            && double.TryParse(vals[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out lon))
                        {
                            c.Add(new Coordinate(lat, lon));
                        }
                    }
                }

                if (optimize)
                {
                    c = await SpatialTools.VertexReductionAsync(c, tolerance);
                }

                return c;
            }

            return null;
        }

        #endregion

        #region GeoRSS Write Methods

        private void Write(SpatialDataSet data, XmlWriter xmlWriter)
        {
            //<?xml version="1.0" encoding="utf-8"?>
            //<feed xmlns="http://www.w3.org/2005/Atom" 
            //      xmlns:georss="http://www.georss.org/georss" 
            //      xmlns:gml="http://www.opengis.net/gml">
            //   <title>Earthquakes</title>
            //   <subtitle>International earthquake observation labs</subtitle>
            //   <link href="http://example.org/"/>
            //   <updated>2005-12-13T18:30:02Z</updated>
            //   <author>
            //      <name>Dr. Thaddeus Remor</name>
            //      <email>tremor@quakelab.edu</email>
            //   </author>
            //   <id>urn:uuid:60a76c80-d399-11d9-b93C-0003939e0af6</id>
            //   <entry>
            //      <title>M 3.2, Mona Passage</title>
            //      <link href="http://example.org/2005/09/09/atom01"/>
            //      <id>urn:uuid:1225c695-cfb8-4ebb-aaaa-80da344efa6a</id>
            //      <updated>2005-08-17T07:02:32Z</updated>
            //      <summary>We just had a big one.</summary>

            //      <georss:where>
            //         <gml:Point>
            //            <gml:pos>45.256 -71.92</gml:pos>
            //         </gml:Point>
            //      </georss:where>
            //   </entry>
            //</feed>

            //Open document
            xmlWriter.WriteStartDocument(true);

            //Write root tag and namespaces.
            xmlWriter.WriteStartElement("feed", "http://www.w3.org/2005/Atom");
            xmlWriter.WriteAttributeString("xmlns", "georss", "http://www.w3.org/2000/xmlns/", GeoRssNamespace);
            xmlWriter.WriteAttributeString("xmlns", "gml", "http://www.w3.org/2000/xmlns/", Gml.GmlNamespace);
            xmlWriter.WriteAttributeString("xmlns", "mappoint", "http://www.w3.org/2000/xmlns/", MapPointNamespace);

            //Write document metadata.
            WriteMetadata(data.Metadata, xmlWriter);

            foreach (var item in data.Geometries)
            {
                WriteGeometry(item, xmlWriter, data.Styles);
            }

            //Close feed tag
            xmlWriter.WriteEndElement();

            //Close document
            xmlWriter.WriteEndDocument();
        }

        private void WriteMetadata(ShapeMetadata metadata, XmlWriter xmlWriter)
        {
            if (metadata != null)
            {
                if (!string.IsNullOrEmpty(metadata.Title))
                {
                    xmlWriter.WriteStartElement("title");
                    xmlWriter.WriteValue(metadata.Title);
                    xmlWriter.WriteEndElement();
                }

                if (!string.IsNullOrEmpty(metadata.Description))
                {
                    xmlWriter.WriteStartElement("description");
                    xmlWriter.WriteValue(metadata.Description);
                    xmlWriter.WriteEndElement();
                }

                foreach (var m in metadata.Properties)
                {
                    if (m.Value != null)
                    {
                        switch (m.Key.ToLowerInvariant())
                        {
                            case "title":
                            case "description":
                            case "subtitle":
                            case "content":
                            case "summary":
                            case "icon":
                                xmlWriter.WriteStartElement(m.Key);
                                xmlWriter.WriteValue(m.Value as string);
                                xmlWriter.WriteEndElement();
                                break;
                            case "mappoint:icon":
                                xmlWriter.WriteStartElement("icon", MapPointNamespace);
                                xmlWriter.WriteValue(m.Value as string);
                                xmlWriter.WriteEndElement();
                                break;
                            case "link": //   <link href="http://example.org/"/>
                                xmlWriter.WriteStartElement(m.Key);
                                xmlWriter.WriteAttributeString("href", m.Value as string);
                                xmlWriter.WriteEndElement();
                                break;
                            case "updated":
                                xmlWriter.WriteStartElement(m.Key);
                                xmlWriter.WriteValue(((DateTime) m.Value).ToString());
                                xmlWriter.WriteEndElement();
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private void WriteGeometry(Geometry geom, XmlWriter xmlWriter, Dictionary<string, ShapeStyle> styles)
        {
            if (geom != null)
            {
                if (geom is Point || geom is LineString || geom is Polygon)
                {
                    xmlWriter.WriteStartElement("entry");

                    if (geom.Metadata != null && !string.IsNullOrEmpty(geom.Metadata.ID))
                    {
                        xmlWriter.WriteStartElement("id");
                        xmlWriter.WriteValue(geom.Metadata.ID);
                        xmlWriter.WriteEndElement();
                    }

                    if (geom.Metadata != null)
                    {
                        WriteMetadata(geom.Metadata, xmlWriter);
                    }

                    if (geom is Point)
                    {
                        WritePoint(geom as Point, xmlWriter);
                    }
                    else if (geom is LineString)
                    {
                        WriteLineString(geom as LineString, xmlWriter);
                    }
                    else if (geom is Polygon)
                    {
                        WritePolygon(geom as Polygon, xmlWriter);
                    }

                    if (!string.IsNullOrEmpty(geom.StyleKey) &&
                        styles != null && styles.ContainsKey(geom.StyleKey))
                    {
                        var style = styles[geom.StyleKey];
                        if (style != null && style.IconUrl != null)
                        {
                            xmlWriter.WriteStartElement("icon");
                            xmlWriter.WriteValue(style.IconUrl.AbsoluteUri);
                            xmlWriter.WriteEndElement();
                        }
                    }

                    xmlWriter.WriteEndElement();
                }
                else
                {
                    if (geom is MultiPoint)
                    {
                        WriteMultiPoint(geom as MultiPoint, xmlWriter, styles);
                    }
                    else if (geom is MultiLineString)
                    {
                        WriteMultiLineString(geom as MultiLineString, xmlWriter, styles);
                    }
                    else if (geom is MultiPolygon)
                    {
                        WriteMultiPolygon(geom as MultiPolygon, xmlWriter, styles);
                    }
                    else if (geom is GeometryCollection)
                    {
                        WriteGeometryCollection(geom as GeometryCollection, xmlWriter, styles);
                    }
                }
            }
        }

        private void WriteCoordinate(Coordinate coord, XmlWriter xmlWriter)
        {
            if (coord.Altitude.HasValue)
            {
                xmlWriter.WriteValue(string.Format(CultureInfo.InvariantCulture, "{0:0.#####} {1:0.#####} {2}",
                    coord.Latitude, coord.Longitude, coord.Altitude.Value));
            }
            else
            {
                xmlWriter.WriteValue(string.Format(CultureInfo.InvariantCulture, "{0:0.#####} {1:0.#####}",
                    coord.Latitude, coord.Longitude));
            }
        }

        private void WriteCoordinateCollection(CoordinateCollection coords, XmlWriter xmlWriter)
        {
            foreach (var c in coords)
            {
                xmlWriter.WriteValue(string.Format(CultureInfo.InvariantCulture, "{0:0.#####} {1:0.#####} ", c.Latitude,
                    c.Longitude));
            }
        }

        private void WritePoint(Point point, XmlWriter xmlWriter)
        {
            //<georss:point>45.256 -71.92</georss:point>
            xmlWriter.WriteStartElement("point", GeoRssNamespace);
            WriteCoordinate(point.Coordinate, xmlWriter);
            xmlWriter.WriteEndElement();
        }

        private void WriteLineString(LineString linestring, XmlWriter xmlWriter)
        {
            if (linestring.Vertices != null && linestring.Vertices.Count >= 2)
            {
                if (!linestring.Vertices[0].Altitude.HasValue)
                {
                    //<georss:line>46.28548 -122.25302 46.28489 -122.25492</georss:line>
                    xmlWriter.WriteStartElement("line", GeoRssNamespace);
                    WriteCoordinateCollection(linestring.Vertices, xmlWriter);
                    xmlWriter.WriteEndElement();
                }
                else
                {
                    //<georss:where>
                    //    <gml:LineString>
                    //        <gml:posList dimension="2">
                    //            14.35954 100.57967
                    //            14.35871 100.57337
                    //        </gml:posList>
                    //    </gml:LineString>
                    //</georss:where>
                    xmlWriter.WriteStartElement("where", GeoRssNamespace);
                    Gml.CreateLineString(linestring, xmlWriter);
                    xmlWriter.WriteEndElement();
                }
            }
        }

        private void WritePolygon(Polygon polygon, XmlWriter xmlWriter)
        {
            if (polygon.ExteriorRing != null && polygon.ExteriorRing.Count >= 2)
            {
                bool hasAltitude = polygon.ExteriorRing[0].Altitude.HasValue;
                if (!hasAltitude && (polygon.InteriorRings == null || polygon.InteriorRings.Count == 0))
                {
                    //<georss:polygon>46.31409 -122.22616 46.31113 -122.22968 46.31083 -122.23320</georss:polygon>
                    xmlWriter.WriteStartElement("polygon", GeoRssNamespace);
                    WriteCoordinateCollection(polygon.ExteriorRing, xmlWriter);
                    xmlWriter.WriteEndElement();
                }
                else
                {
                    //<georss:where>
                    // <gml:Polygon>
                    //   <gml:exterior>
                    //     <gml:LinearRing>
                    //       <gml:posList>
                    //         -71.106216 42.366661
                    //         -71.105576 42.367104
                    //         -76.104378 42.367134
                    //         -71.106216 42.366661
                    //       </gml:posList>
                    //     </gml:LinearRing>
                    //   </gml:exterior>
                    // </gml:Polygon>
                    //</georss:where>
                    xmlWriter.WriteStartElement("where", GeoRssNamespace);
                    Gml.CreatePolygon(polygon, xmlWriter);
                    xmlWriter.WriteEndElement();
                }
            }
        }

        private void WriteMultiPoint(MultiPoint points, XmlWriter xmlWriter, Dictionary<string, ShapeStyle> styles)
        {
            //GeoRSS only handles a subset of GML, break up complex gemoetries into simple geoms

            foreach (var p in points)
            {
                p.Metadata = points.Metadata;
                WriteGeometry(p, xmlWriter, styles);
            }
        }

        private void WriteMultiLineString(MultiLineString linestrings, XmlWriter xmlWriter,
            Dictionary<string, ShapeStyle> styles)
        {
            //GeoRSS only handles a subset of GML, break up complex gemoetries into simple geoms

            foreach (var p in linestrings)
            {
                p.Metadata = linestrings.Metadata;
                WriteGeometry(p, xmlWriter, styles);
            }
        }

        private void WriteMultiPolygon(MultiPolygon polygons, XmlWriter xmlWriter, Dictionary<string, ShapeStyle> styles)
        {
            //GeoRSS only handles a subset of GML, break up complex gemoetries into simple geoms

            foreach (var p in polygons)
            {
                p.Metadata = polygons.Metadata;
                WriteGeometry(p, xmlWriter, styles);
            }
        }

        private void WriteGeometryCollection(GeometryCollection geoms, XmlWriter xmlWriter,
            Dictionary<string, ShapeStyle> styles)
        {
            //GeoRSS only handles a subset of GML, break up complex gemoetries into simple geoms

            foreach (var p in geoms)
            {
                p.Metadata = geoms.Metadata;
                WriteGeometry(p, xmlWriter, styles);
            }
        }

        #endregion

        #endregion
    }
}