/**************************************************************************** 
 * Author: Ricky Brundritt
 * 
 * Description:
 * This class allows you to read and write KML files. 
 * 
 * Notes: 
 * - 
 * 
 * Supported Tags:
 * 
 *  
 * Supported Namespaces:
 * http://www.opengis.net/kml/2.2
 * 
 * https://developers.google.com/kml/documentation/kmlreference?csw=1
 * 
 * 
 * Uses Nuget package Microsoft.Bcl.Compression: https://www.nuget.org/packages/Microsoft.Bcl.Compression/3.9.73
 * 
 ****************************************************************************/

using Microsoft.Maps.SpatialToolbox.Internals;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Maps.SpatialToolbox.IO
{
    /// <summary>
    /// This class allows you to read and write KML files.
    /// </summary>
    public class KmlFeed : BaseTextFeed
    {
        #region Private Properties
     
        private int embeddedStyleCnt = 0;

        private const int Max_NetworkLink_Level = 3;

        private Regex CoordArtifactRx = new Regex(@"[\n\t]");
        private string[] SpaceSplitter = new string[] { " " };
        private string[] CommaSpaceSplitter = new string[] { ",", " " };

        private string[] geomTypes = new string[] { "Point", "LineString", "Polygon", "MultiGeometry" };

        private const string KmlNamespace = "http://www.opengis.net/kml/2.2";
        private const string AtomNamespace = "http://www.w3.org/2005/Atom";

        #endregion
                
        #region Constructor

        public KmlFeed():
            base()
        {
        }

        public KmlFeed(bool stripHtml):
            base(stripHtml)
        {
        }

        public KmlFeed(double tolerance)
            : base(tolerance)
        {            
        }

        public KmlFeed(bool stripHtml, double tolerance)
            : base(stripHtml, tolerance)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads a KML feed
        /// </summary>
        /// <param name="xml">KML feed as a string.</param>
        /// <param name="stripHtml">Option to strip out HTML from string fields.</param>
        /// <returns>A SpatialDataSet containing the spatial data from the KML feed.</returns>
        public override Task<SpatialDataSet> ReadAsync(string xml)
        {
            return Task.Run<SpatialDataSet>(async () =>
            {
                try
                {
                    var doc = GetKmlDoc(xml);
                    return await ParseKmlDoc(doc, 0, string.Empty);
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
        /// Reads a KML feed.
        /// </summary>
        /// <param name="xmlUri">Uri to KML feed.</param>
        /// <returns>A SpatialDataSet containing the spatial data from the KML feed.</returns>
        public override Task<SpatialDataSet> ReadAsync(Uri xmlUri)
        {
            return Task.Run<SpatialDataSet>(async () =>
            {
                try
                {
                    var doc = await GetKmlDoc(xmlUri);
                    return await ParseKmlDoc(doc, 0, GetBaseUri(xmlUri));
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
        /// Reads a KML feed.
        /// </summary>
        /// <param name="xmlStream">KML feed as a Stream.</param>
        /// <returns>A SpatialDataSet containing the spatial data from the KML feed.</returns>
        public override Task<SpatialDataSet> ReadAsync(Stream xmlStream)
        {
            return Task.Run<SpatialDataSet>(async () =>
            {
                try
                {
                    var doc = GetKmlDoc(xmlStream);
                    return await ParseKmlDoc(doc, 0, string.Empty);
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
        /// Writes a spatial data set object to a Stream as KML
        /// </summary>
        /// <param name="geometries">A spatial data set to write</param>
        /// <param name="stream">Stream to write KML to.</param>
        public override Task WriteAsync(SpatialDataSet data, Stream stream)
        {
            return WriteAsync(data, stream, false);
        }

        /// <summary>
        /// Writes a spatial data set object to a Stream as KML
        /// </summary>
        /// <param name="geometries">A spatial data set to write</param>
        /// <param name="stream">Stream to write KML to.</param>
        /// <param name="compress">A boolean indicating if the Kml feed should be compressed as a Kmz file.</param>
        public Task WriteAsync(SpatialDataSet data, Stream stream, bool compress)
        {
            return Task.Run(() =>
            {
                if (compress)
                {
                    using (var zipFile = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Create, false, Encoding.UTF8))
                    {
                        var entry = zipFile.CreateEntry("root.kml");

                        using (var xWriter = XmlWriter.Create(entry.Open(), xmlWriterSettings))
                        {
                            Write(data, xWriter);
                        }
                    }
                }
                else
                {
                    using (var xWriter = XmlWriter.Create(stream, xmlWriterSettings))
                    {
                        Write(data, xWriter);
                    }
                }
            });
        }

        /// <summary>
        /// Writes a spatial data set object as a string.
        /// </summary>
        /// <param name="geometries">A spatial data set to write</param>
        /// <returns>A string of KML</returns>
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

        #region Kml Read Methods

        private XDocument GetKmlDoc(string kmlString)
        {            
            return XDocument.Parse(kmlString);
        }

        private XDocument GetKmlDoc(Stream kmlStream)
        {
            using (var ms = new MemoryStream())
            {
                kmlStream.CopyTo(ms);

                //Check to see if file is compressed KML file (KMZ).
                if (XmlUtilities.IsStreamCompressed(ms))
                {
                    //Uncompress KMZ and look for embedded KML files.
                    var zipArchive = new System.IO.Compression.ZipArchive(ms);

                    foreach (var entry in zipArchive.Entries)
                    {
                        if (entry.Name.EndsWith(".kml", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var s = entry.Open())
                            {
                                return XDocument.Load(s);
                            }
                        }
                    }
                }
                else
                {
                    return XDocument.Load(ms);
                }
            }

            return null;
        }

        private async Task<XDocument> GetKmlDoc(Uri kmlFeedUri)
        {
            try
            {
                using (var stream = await ServiceHelper.GetStreamAsync(kmlFeedUri))
                {
                    return GetKmlDoc(stream);
                }
            }
            catch (Exception ex){
                var t = ex.Message;
            }

            return null;
        }

        private async Task<SpatialDataSet> ParseKmlDoc(XDocument doc, int requestLevel, string baseUri)
        {
            if (doc != null)
            {
                var r = await ParseFolder(doc.Root, requestLevel, baseUri);
                return r;
            }

            return new SpatialDataSet()
            {
                Error = "Unable to read KML document."
            };
        }

        private async Task<SpatialDataSet> ParseFolder(XElement doc, int requestLevel, string baseUri)
        {
            var result = new SpatialDataSet();

            var documents = (from x in doc.Elements()
                             where string.Compare(x.Name.LocalName, "Document", StringComparison.OrdinalIgnoreCase) == 0 ||
                             string.Compare(x.Name.LocalName, "Folder", StringComparison.OrdinalIgnoreCase) == 0 ||
                                string.Compare(x.Name.LocalName, "Placemark", StringComparison.OrdinalIgnoreCase) == 0
                             select x).ToList();

            foreach (var d in documents)
            {
                var data = await ParseContainer(d, requestLevel, baseUri);
                if (data.Geometries.Count > 0)
                {
                    result.Append(data);
                }

                //Get embedded folders
                var data2 = await ParseFolder(d, requestLevel, baseUri);

                if (data2.Geometries.Count > 0)
                {
                    result.Append(data2);
                }
            }

            return result;
        }

        private async Task<SpatialDataSet> ParseContainer(XElement doc, int requestLevel, string baseUri)
        {
            var result = new SpatialDataSet();

            try
            {
                var kmlStyles = new Dictionary<string, ShapeStyle>();
                var kmlStyleMaps = new Dictionary<string, string>();
                var geoms = new List<Geometry>();

                var styleMaps = XmlUtilities.GetElementsByTagName(doc, "StyleMap");
                foreach (var s in styleMaps)
                {
                    ParseStyleMap(s, kmlStyleMaps);
                };

                var styles = XmlUtilities.GetElementsByTagName(doc, "Style");
                string id;
                foreach (var s in styles)
                {
                    var style = ParseStyle(s, baseUri, out id);
                    if (style != null && !string.IsNullOrEmpty(id))
                    {
                        kmlStyles.Add(id, style);
                    }
                }

                var placemarks = XmlUtilities.GetElementsByTagName(doc, "Placemark");
                foreach (var p in placemarks)
                {
                    var g = await ParsePlacemark(p, baseUri, kmlStyleMaps, kmlStyles);
                    if (g != null)
                    {
                        geoms.Add(g);
                    }
                }

                if (requestLevel < Max_NetworkLink_Level)
                {
                    var networkLinks = XmlUtilities.GetElementsByTagName(doc, "NetworkLink");

                    foreach (var nl in networkLinks)
                    {
                        var link = ParseNetworkLink(nl, baseUri);

                        if (link != null)
                        {
                            var doc2 = await GetKmlDoc(link);

                            if (doc2 != null)
                            {
                                var data = await ParseKmlDoc(doc2, requestLevel + 1, baseUri);
                                if (data != null && data.Geometries != null && data.Geometries.Count > 0)
                                {
                                    //Merge with current results
                                    geoms.AddRange(data.Geometries);

                                    if (data.Styles != null && data.Styles.Count > 0)
                                    {
                                        foreach (var key in data.Styles.Keys)
                                        {
                                            if (!kmlStyles.ContainsKey(key))
                                            {
                                                kmlStyles.Add(key, data.Styles[key]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (geoms.Count > 0)
                {
                    result.Styles = kmlStyles;
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

        private void ParseStyleMap(XElement node, Dictionary<string, string> styleMap)
        {
            var id = XmlUtilities.GetStringAttribute(node, "id");
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            var pairs = XmlUtilities.GetChildNodes(node, "Pair");
            foreach (var p in pairs)
            {
                var key = XmlUtilities.GetString(p, "key", false);
                if (key != null && string.Compare(key, "normal", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var styleUrl = XmlUtilities.GetString(p, "styleUrl", false);
                    if (!string.IsNullOrWhiteSpace(styleUrl))
                    {
                        if (styleUrl.Contains("#"))
                        {
                            styleUrl = styleUrl.Substring(styleUrl.LastIndexOf("#") + 1);
                        }

                        if (!styleMap.ContainsKey(id))
                        {
                            styleMap.Add(id, styleUrl);
                        }
                    }
                    break;
                }
            }
        }

        private ShapeStyle ParseStyle(XElement node, string baseUrl, out string id)
        {
            id = XmlUtilities.GetStringAttribute(node, "id");
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return ParseStyle(node, baseUrl);
        }

        private ShapeStyle ParseStyle(XElement node, string baseUrl)
        {
            //<Style id="ID">
            //    <IconStyle>...</IconStyle>
            //    <LabelStyle>...</LabelStyle> - //TODO - Add Support for LabelStyle
            //    <LineStyle>...</LineStyle>
            //    <PolyStyle>...</PolyStyle>
            //    <BalloonStyle>...</BalloonStyle> - //TODO - Add Support for BalloonStyle
            //    <ListStyle>...</ListStyle>
            //</Style>

            //TODO Look into Icon hotspot

            //Parse Icon Style
            var iconStyle = XmlUtilities.GetChildNode(node, "IconStyle");
            var lineStyle = XmlUtilities.GetChildNode(node, "LineStyle");
            var polyStyle = XmlUtilities.GetChildNode(node, "PolyStyle");
            var labelStyle = XmlUtilities.GetChildNode(node, "LabelStyle");

            string iconUrl = XmlUtilities.GetString(XmlUtilities.GetChildNode(iconStyle, "Icon"), "href", false);

            var style = new ShapeStyle()
            {
                IconUrl = CleanUri(iconUrl, baseUrl),
                IconScale = XmlUtilities.GetDouble(iconStyle, "Scale", 1),
                IconColor = XmlUtilities.GetColor(iconStyle, "Color"),
                IconHeading = XmlUtilities.GetDouble(iconStyle, "Heading", 0),
                StrokeColor = XmlUtilities.GetColor(lineStyle, "Color"),
                StrokeThickness = XmlUtilities.GetDouble(lineStyle, "Width", 2),
                FillColor = XmlUtilities.GetColor(polyStyle, "Color"),
                FillPolygon = XmlUtilities.GetBoolean(polyStyle, "Fill", true),
                OutlinePolygon = XmlUtilities.GetBoolean(polyStyle, "Outline", true)
            };

            return style;
        }

        private async Task<Geometry> ParsePlacemark(XElement node, string baseUrl, Dictionary<string, string> styleMap, Dictionary<string, ShapeStyle> styles)
        {
            //<Placemark id="ID">
            //  <!-- inherited from Feature element -->
            //  <name>...</name>                      <!-- string -->
            //  <visibility>1</visibility>            <!-- boolean -->
            //  <atom:link href=" "/>                <!-- xmlns:atom -->
            //  <address>...</address>                <!-- string -->
            //  <phoneNumber>...</phoneNumber>        <!-- string -->  
            //  <Snippet maxLines="2">...</Snippet>   <!-- string -->
            //  <description>...</description>        <!-- string -->

            //  <styleUrl>...</styleUrl>              <!-- anyURI -->
            //  <StyleSelector>...</StyleSelector>

            //  <Geometry>...</Geometry>
            //</Placemark>
                        
            if (node != null)
            {
                var metadata = new ShapeMetadata();
                bool visible = true;
                string nodeName;
                string styleKey = null;
                Geometry geom = null;

                foreach (var n in node.Elements())
                {
                    nodeName = n.Name.LocalName;
                    switch (nodeName)
                    {
                        case "name":
                            metadata.Title = XmlUtilities.GetString(n, stripHtml);
                            if (!string.IsNullOrWhiteSpace(metadata.Title) && !metadata.Properties.ContainsKey(nodeName))
                            {
                                metadata.Properties.Add(nodeName, metadata.Title);
                            }
                            break;
                        case "description":
                            metadata.Description = XmlUtilities.GetString(n, stripHtml);
                            if (!string.IsNullOrWhiteSpace(metadata.Description) && !metadata.Properties.ContainsKey(nodeName))
                            {
                                metadata.Properties.Add(nodeName, metadata.Description);
                            }
                            break;
                        case "address":
                        case "phoneNumber":
                        case "Snippet ":
                            SetMetadataString(metadata, nodeName, n, stripHtml);
                            break;
                        case "visibility":
                            visible = XmlUtilities.GetBoolean(n, true);
                            break;
                        case "atom:link":
                            var href = XmlUtilities.GetStringAttribute(n, "href");

                            if (!string.IsNullOrWhiteSpace(href) && !metadata.Properties.ContainsKey(nodeName))
                            {
                                metadata.Properties.Add(nodeName, href);
                            }
                            break;
                        case "styleUrl":
                            styleKey = XmlUtilities.GetString(n, false);
                            if (!string.IsNullOrWhiteSpace(styleKey) && styleKey.Contains("#"))
                            {
                                styleKey = styleKey.Substring(styleKey.LastIndexOf("#") + 1);

                                if (!styles.ContainsKey(styleKey) && styleMap.ContainsKey(styleKey))
                                {
                                    styleKey = styleMap[styleKey];
                                }
                            }
                            break;
                        case "Style":
                            ShapeStyle tempStyle = null;
                            var id = XmlUtilities.GetStringAttribute(n, "id");
                            if (!string.IsNullOrWhiteSpace(id))
                            {
                                styleKey = id;
                            }
                            else if(!string.IsNullOrEmpty(styleKey) && styles.ContainsKey(styleKey))
                            {
                                //Create new Style and merge
                                tempStyle = styles[styleKey];
                                styleKey = string.Empty;
                            }

                            if (string.IsNullOrWhiteSpace(styleKey))
                            {
                                //Extract embedded Style
                                styleKey = "embeddedStyle_" + embeddedStyleCnt;
                                embeddedStyleCnt++;
                            }

                            if (!string.IsNullOrEmpty(styleKey) && !styles.ContainsKey(styleKey))
                            {
                                var s = ParseStyle(n, baseUrl);
                                if (s != null)
                                {
                                    if (tempStyle != null)
                                    {
                                        s = tempStyle.Merge(s);
                                    }

                                    styles.Add(styleKey, s);
                                }
                            }
                            break;
                        case "Point":
                        case "LineString":
                        case "Polygon":
                        case "MultiGeometry":
                            geom = await ParseGeometry(n);
                            break;
                        default:
                            break;
                    }
                }

                if (visible && geom != null)
                {
                    geom.StyleKey = styleKey;

                    if (metadata != null && (!string.IsNullOrEmpty(metadata.Title) || !string.IsNullOrEmpty(metadata.Description) ||
                        metadata.Properties.Count > 0))
                    {
                        geom.Metadata = metadata;
                    }

                    return geom;
                }
            }

            return null;
        }

        private async Task<Geometry> ParseGeometry(XElement node)
        {
            switch (node.Name.LocalName)
            {
                case "Point":
                    return ParsePoint(node);
                case "LineString":
                    return await ParseLineString(node);
                case "Polygon":
                    return await ParsePolygon(node);
                case "MultiGeometry":
                    var mg = new GeometryCollection();
                    var geoms = XmlUtilities.GetChildNodes(node, geomTypes);
                    foreach (var ge in geoms)
                    {
                        var g = await ParseGeometry(ge);
                        if (g != null)
                        {
                            mg.Geometries.Add(g);
                        }
                    }
                    if (mg.Geometries.Count > 0)
                    {
                        return mg;
                    }
                    break;
                default:
                    break;
            }

            return null;
        }

        private Point ParsePoint(XElement node)
        {
            //<Point id="ID">
            //  <!-- specific to Point -->
            //  <extrude>0</extrude>                        <!-- boolean -->
            //  <altitudeMode>clampToGround</altitudeMode>
            //        <!-- kml:altitudeModeEnum: clampToGround, relativeToGround, or absolute -->
            //        <!-- or, substitute gx:altitudeMode: clampToSeaFloor, relativeToSeaFloor -->
            //  <coordinates>...</coordinates>              <!-- lon,lat[,alt] -->
            //</Point>

            var c = ParseCoordinate(XmlUtilities.GetChildNode(node, "coordinates"));
            if (!double.IsNaN(c.Latitude) && ! double.IsNaN(c.Longitude))
            {
                return new Point(c);
            }

            return null;
        }

        private async Task<LineString> ParseLineString(XElement node)
        {
            var cc = await ParseCoordinates(XmlUtilities.GetChildNode(node, "coordinates"));
            if (cc != null && cc.Count >= 2)
            {
                return new LineString(cc);
            }

            return null;
        }

        private async Task<Polygon> ParsePolygon(XElement node)
        {
            //<Polygon id="ID">
            //  <!-- specific to Polygon -->
            //  <extrude>0</extrude>                       <!-- boolean -->
            //  <tessellate>0</tessellate>                 <!-- boolean -->
            //  <altitudeMode>clampToGround</altitudeMode>
            //        <!-- kml:altitudeModeEnum: clampToGround, relativeToGround, or absolute -->
            //        <!-- or, substitute gx:altitudeMode: clampToSeaFloor, relativeToSeaFloor -->
            //  <outerBoundaryIs>
            //    <LinearRing>
            //      <coordinates>...</coordinates>         <!-- lon,lat[,alt] -->
            //    </LinearRing>
            //  </outerBoundaryIs>
            //  <innerBoundaryIs>
            //    <LinearRing>
            //      <coordinates>...</coordinates>         <!-- lon,lat[,alt] -->
            //    </LinearRing>
            //  </innerBoundaryIs>
            //</Polygon>

            var cc = await ParseLinearRing(XmlUtilities.GetChildNode(node, "outerBoundaryIs"));
            if (cc != null && cc.Count >= 3)
            {
                var polygon = new Polygon(cc);

                var innerRings = XmlUtilities.GetChildNodes(node, "innerBoundaryIs");
                if (innerRings != null && innerRings.Count > 0)
                {
                    foreach (var r in innerRings)
                    {
                        var ir = await ParseLinearRing(r);

                        if (ir != null)
                        {
                            polygon.InteriorRings.Add(ir);
                        }
                    }
                }

                await polygon.MakeValidAsync();

                return polygon;
            }

            return null;
        }

        private Coordinate ParseCoordinate(XElement node)
        {
            var sCoord = XmlUtilities.GetString(node, false);
            var vals = SplitCoordString(sCoord, CoordArtifactRx, CommaSpaceSplitter);
            double lat, lon, alt;

            if (vals.Length >= 2 && double.TryParse(vals[1], NumberStyles.Float, CultureInfo.InvariantCulture, out lat)
                && double.TryParse(vals[0], NumberStyles.Float, CultureInfo.InvariantCulture, out lon))
            {
                Coordinate c = new Coordinate(lat, lon);

                if (vals.Length > 3 
                    && double.TryParse(vals[2], NumberStyles.Float, CultureInfo.InvariantCulture, out alt))
                {
                    c = new Coordinate(lat, lon, alt);
                }
                else
                {
                    c = new Coordinate(lat, lon);
                }

                return c;
            }

            return new Coordinate(double.NaN, double.NaN);
        }

        private async Task<CoordinateCollection> ParseLinearRing(XElement node)
        {
            var ring = await ParseCoordinates(XmlUtilities.GetChildNode(XmlUtilities.GetChildNode(node, "LinearRing"), "coordinates"));

            if (ring != null && ring.Count >= 3)
            {
                //Ensure Ring is closed
                if (!ring[0].Equals(ring[ring.Count - 1]))
                {
                    ring.Add(ring[0]);
                }

                return ring;
            }

            return null;
        }

        private async Task<CoordinateCollection> ParseCoordinates(XElement node)
        {
            if (node != null)
            {
                var sCoord = XmlUtilities.GetString(node, false);
                double lat, lon, alt;
                var tuples = SplitCoordString(sCoord, CoordArtifactRx, SpaceSplitter);

                var cc = new CoordinateCollection();

                foreach (var t in tuples)
                {
                    var vals = SplitCoordString(t, CoordArtifactRx, CommaSpaceSplitter);
                    if (vals.Length >= 2 
                        && double.TryParse(vals[1], NumberStyles.Float, CultureInfo.InvariantCulture, out lat)
                        && double.TryParse(vals[0], NumberStyles.Float, CultureInfo.InvariantCulture, out lon))
                    {
                        if (vals.Length > 3 && double.TryParse(vals[2], NumberStyles.Float, CultureInfo.InvariantCulture, out alt))
                        {
                            cc.Add(new Coordinate(lat, lon, alt));
                        }
                        else
                        {
                            cc.Add(new Coordinate(lat, lon));
                        }
                    }
                }

                if (optimize)
                {
                    cc = await SpatialTools.VertexReductionAsync(cc, tolerance);
                }

                return cc;
            }

            return null;
        }

        private Uri ParseNetworkLink(XElement node, string baseUrl)
        {
            //<NetworkLink id="ID">
            //  <visibility>1</visibility>
            //  <Link><href></href></Link>
            //</NetworkLink>

            string nodeName;
            bool visible = true;
            string href = string.Empty;

            foreach (var n in node.Elements())
            {
                nodeName = n.Name.LocalName;
                switch (nodeName)
                {
                    case "visibility":
                        visible = XmlUtilities.GetBoolean(n, true);
                        break;
                    case "Link":
                        href = XmlUtilities.GetString(n, "href", false);
                        break;
                }
            }

            if (visible)
            {
                return CleanUri(href, baseUrl);
            }

            return null;
        }

        #endregion

        #region KML Write Methods

        private void Write(SpatialDataSet data, XmlWriter xmlWriter)
        {
            //<?xml version="1.0" encoding="UTF-8"?>
            //<kml xmlns="http://www.opengis.net/kml/2.2">

            //  <Document>
            //    <name>balloonVisibility Example</name>
            //    <open>1</open>

            //    <Placemark id="underwater2">
            //      <name>Still swimming...</name>
            //      <description>We're about to leave the ocean, and visit the coast...</description>
            //      <Point>
            //        <coordinates>-119.779550,33.829268,0</coordinates>
            //      </Point>
            //    </Placemark>

            //    <Placemark id="onland">
            //      <name>The end</name>
            //      <description>
            //        <![CDATA[The end of our simple tour.
            //        Use 
            //        to show description balloons.]]>
            //      </description>
            //      <Point>
            //        <coordinates>-119.849578,33.968515,0</coordinates>
            //      </Point>
            //    </Placemark>


            //  </Document>
            //</kml>

            //Optimize Style mappings
            data.CleanUpStyles();

            //Open document
            xmlWriter.WriteStartDocument(true);

            //Write root tag and namespaces.
            xmlWriter.WriteStartElement("kml", KmlNamespace);
            xmlWriter.WriteAttributeString("xmlns", "http://www.opengis.net/kml/2.2");
            xmlWriter.WriteAttributeString("xmlns", "atom", "http://www.w3.org/2000/xmlns/", AtomNamespace);

            xmlWriter.WriteStartElement("Document");

            //Write document metadata.
            WriteMetadata(data.Metadata, xmlWriter);

            //Write Styles
            if (data.Styles != null)
            {
                foreach (var s in data.Styles)
                {
                    WriteStyle(s.Key, s.Value, xmlWriter);
                }
            }

            //Write Geometries
            foreach (var item in data.Geometries)
            {
                WritePlacemark(item, xmlWriter);
            }

            //Close document tag
            xmlWriter.WriteEndElement();

            //Close feed tag
            xmlWriter.WriteEndElement();

            //Close document
            xmlWriter.WriteEndDocument();
        }

        private void WriteMetadata(ShapeMetadata metadata, XmlWriter xmlWriter)
        {
            if (metadata != null)
            {
                if (!string.IsNullOrEmpty(metadata.Title) && !metadata.Properties.ContainsKey("name"))
                {
                    metadata.Properties.Add("name", metadata.Title);
                }

                if (!string.IsNullOrEmpty(metadata.Description) && !metadata.Properties.ContainsKey("description"))
                {
                    metadata.Properties.Add("description", metadata.Title);
                }

                foreach (var t in metadata.Properties)
                {
                    var val = t.Value;
                    switch (t.Key)
                    {
                        case "name":
                        case "description":
                            xmlWriter.WriteStartElement(t.Key);
                            if ((val as string).Contains("<"))
                            {
                                xmlWriter.WriteCData(val as string);
                            }else{
                                xmlWriter.WriteString(val as string);
                            }
                            xmlWriter.WriteEndElement();
                            break;
                        case "address":
                        case "phoneNumber":
                        case "Snippet ":
                            xmlWriter.WriteStartElement(t.Key);
                            xmlWriter.WriteString(val as string);
                            xmlWriter.WriteEndElement();
                            break;
                        case "visibility":
                            xmlWriter.WriteStartElement(t.Key);
                            xmlWriter.WriteString(((bool)val) ? "1" : "0");
                            xmlWriter.WriteEndElement();
                            break;
                        case "atom:link":
                            xmlWriter.WriteStartElement("link", AtomNamespace);
                            xmlWriter.WriteAttributeString("href", val as string);
                            xmlWriter.WriteEndElement();
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void WriteStyle(string styleKey, ShapeStyle style, XmlWriter xmlWriter)
        {
            //<Style id="ID">
            //  <IconStyle>...</IconStyle>
            //  <LabelStyle>...</LabelStyle> -- //TODO - Add support for LabelStyle
            //  <LineStyle>...</LineStyle>
            //  <PolyStyle>...</PolyStyle>
            //</Style>

            if (style != null && !string.IsNullOrEmpty(styleKey))
            {
                xmlWriter.WriteStartElement("Style");
                xmlWriter.WriteAttributeString("id", styleKey);

                //Write Icon Styles
                if (style.IconColor.HasValue ||
                    (!double.IsNaN(style.IconHeading) && style.IconHeading > 0) ||
                    (!double.IsNaN(style.IconScale) && style.IconScale > 0) ||
                    style.IconUrl != null)
                {
                    xmlWriter.WriteStartElement("IconStyle");

                    if (!double.IsNaN(style.IconHeading) && style.IconHeading > 0)
                    {
                        xmlWriter.WriteStartElement("heading");
                        xmlWriter.WriteString(style.IconHeading.ToString());
                        xmlWriter.WriteEndElement();
                    }

                    if (!double.IsNaN(style.IconScale) && style.IconScale > 0 && style.IconScale != 1)
                    {
                        xmlWriter.WriteStartElement("scale");
                        xmlWriter.WriteString(style.IconScale.ToString());
                        xmlWriter.WriteEndElement();
                    }

                    if (style.IconUrl != null)
                    {
                        xmlWriter.WriteStartElement("Icon");
                        xmlWriter.WriteStartElement("href");
                        xmlWriter.WriteString(style.IconUrl.AbsoluteUri);
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteEndElement();
                    }

                    if (style.IconColor.HasValue)
                    {
                        xmlWriter.WriteStartElement("color");
                        xmlWriter.WriteString(style.IconColor.Value.ToKmlColor());
                        xmlWriter.WriteEndElement();
                    }

                    xmlWriter.WriteEndElement();
                }

                //Write LineStyle
                if (style.StrokeColor.HasValue && (!double.IsNaN(style.StrokeThickness) && style.StrokeThickness > 0))
                {
                    xmlWriter.WriteStartElement("LineStyle");

                    xmlWriter.WriteStartElement("width");
                    xmlWriter.WriteString(style.StrokeThickness.ToString());
                    xmlWriter.WriteEndElement();
                    
                    xmlWriter.WriteStartElement("color");
                    xmlWriter.WriteString(style.StrokeColor.Value.ToKmlColor());
                    xmlWriter.WriteEndElement();
                    
                    xmlWriter.WriteEndElement();
                }

                //Write PolyStyle
                if (style.FillColor.HasValue)
                {
                    xmlWriter.WriteStartElement("PolyStyle");

                    xmlWriter.WriteStartElement("color");
                    xmlWriter.WriteString(style.FillColor.Value.ToKmlColor());
                    xmlWriter.WriteEndElement();

                    if (!style.FillPolygon)
                    {
                        xmlWriter.WriteStartElement("fill");
                        xmlWriter.WriteString("0");
                        xmlWriter.WriteEndElement();
                    }

                    if (!style.OutlinePolygon)
                    {
                        xmlWriter.WriteStartElement("outline");
                        xmlWriter.WriteString("0");
                        xmlWriter.WriteEndElement();
                    }

                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
            }
        }

        private void WritePlacemark(Geometry geom, XmlWriter xmlWriter)
        {
            if (geom != null)
            {
                xmlWriter.WriteStartElement("Placemark");

                if (geom.Metadata != null)
                {
                    if (!string.IsNullOrEmpty(geom.Metadata.ID))
                    {
                        xmlWriter.WriteAttributeString("id", geom.Metadata.ID);
                    }

                    WriteMetadata(geom.Metadata, xmlWriter);
                }

                if (!string.IsNullOrEmpty(geom.StyleKey))
                {
                    xmlWriter.WriteStartElement("styleUrl");
                    xmlWriter.WriteString(geom.StyleKey);
                    xmlWriter.WriteEndElement();
                }

                WriteGeometry(geom, xmlWriter);

                xmlWriter.WriteEndElement();
            }
        }

        private void WriteGeometry(Geometry geom, XmlWriter xmlWriter)
        {
            if (geom != null)
            {
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
                else if (geom is MultiPoint)
                {
                    WriteMultiPoint(geom as MultiPoint, xmlWriter);
                }
                else if (geom is MultiLineString)
                {
                    WriteMultiLineString(geom as MultiLineString, xmlWriter);
                }
                else if (geom is MultiPolygon)
                {
                    WriteMultiPolygon(geom as MultiPolygon, xmlWriter);
                }
                else if (geom is GeometryCollection)
                {
                    WriteGeometryCollection(geom as GeometryCollection, xmlWriter);
                }
            }
        }

        private void WritePoint(Point point, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("Point");
            xmlWriter.WriteStartElement("coordinates");
            xmlWriter.WriteString(string.Format(CultureInfo.InvariantCulture, "{1:0.#####},{0:0.#####},0", point.Coordinate.Latitude, point.Coordinate.Longitude));
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
        }

        private void WriteLineString(LineString line, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("LineString");
            WriteCoordinates(line.Vertices, xmlWriter, line.Is3D(), false);
            xmlWriter.WriteEndElement();
        }

        private void WritePolygon(Polygon polygon, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("Polygon");

            bool incAlt = polygon.Is3D();
            
            //Write Outer ring
            xmlWriter.WriteStartElement("outerBoundaryIs");
            xmlWriter.WriteStartElement("LinearRing");
            WriteCoordinates(polygon.ExteriorRing, xmlWriter, incAlt, true);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();

            //Write Inner Rings
            if (polygon.InteriorRings != null && polygon.InteriorRings.Count > 0)
            {
                foreach (var r in polygon.InteriorRings)
                {
                    xmlWriter.WriteStartElement("innerBoundaryIs");
                    xmlWriter.WriteStartElement("LinearRing");
                    WriteCoordinates(r, xmlWriter, incAlt, true);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                }
            }

            xmlWriter.WriteEndElement();
        }

        private void WriteMultiPoint(MultiPoint points, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("MultiGeometry");

            foreach (var p in points.Geometries)
            {
                WritePoint(p, xmlWriter);
            }

            xmlWriter.WriteEndElement();
        }

        private void WriteMultiLineString(MultiLineString lines, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("MultiGeometry");

            foreach (var p in lines.Geometries)
            {
                WriteLineString(p, xmlWriter);
            }

            xmlWriter.WriteEndElement();
        }

        private void WriteMultiPolygon(MultiPolygon polygons, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("MultiGeometry");

            foreach (var p in polygons.Geometries)
            {
                WritePolygon(p, xmlWriter);
            }

            xmlWriter.WriteEndElement();
        }

        private void WriteGeometryCollection(GeometryCollection geoms, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("MultiGeometry");

            foreach (var g in geoms.Geometries)
            {
                if (g is Point || g is LineString || g is Polygon)
                {
                    WriteGeometry(g, xmlWriter);
                }
                else if(g is MultiPoint)
                {
                    var points = g as MultiPoint;
                    foreach (var p in points.Geometries)
                    {
                        WritePoint(p, xmlWriter);
                    }
                }
                else if (g is MultiLineString)
                {
                    var lines = g as MultiLineString;
                    foreach (var p in lines.Geometries)
                    {
                        WriteLineString(p, xmlWriter);
                    }
                }
                else if (g is MultiPolygon)
                {
                    var polys = g as MultiPolygon;
                    foreach (var p in polys.Geometries)
                    {
                        WritePolygon(p, xmlWriter);
                    }
                }
            }

            xmlWriter.WriteEndElement();
        }

        private void WriteCoordinates(CoordinateCollection coords, XmlWriter xmlWriter, bool includeAltitude, bool closed)
        {
            xmlWriter.WriteStartElement("coordinates");

            if (coords != null)
            {
                if (closed && coords.Count >= 3 && coords[0] != coords[coords.Count - 1])
                {
                    coords.Add(coords[0]);
                }

                if (includeAltitude)
                {
                    foreach (var c in coords)
                    {
                        xmlWriter.WriteValue(string.Format(CultureInfo.InvariantCulture, "{1:0.#####},{0:0.#####},{2} ", c.Latitude, c.Longitude, c.Altitude.HasValue ? c.Altitude.Value : 0));
                    }
                }
                else
                {
                    foreach (var c in coords)
                    {
                        xmlWriter.WriteValue(string.Format(CultureInfo.InvariantCulture, "{1:0.#####},{0:0.#####},0 ", c.Latitude, c.Longitude));
                    }
                }
            }

            xmlWriter.WriteEndElement();
        }

        #endregion

        #endregion
    }
}
