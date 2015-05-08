/****************************************************************************
* Author: Ricky Brundritt
* Based on: http://rbrundritt.wordpress.com/2010/12/10/georss-support-for-bing-maps-v7-ajax-control/
* 
* Description:
* This class reads and writes GML objects.
* GML is often used included as part of other XML file formats to represent spatial data. 
* For example a subset of the GML shapes are used by the GeoRSS feed format.
* http://en.wikipedia.org/wiki/Geography_Markup_Language
*
* Currently supported GML Tags:
*
* Reading:
*   - gml:Point
*   - gml:LineString
*   - gml:Polygon
*   - gml:MultiPoint
*   - gml:MultiLineString
*   - gml:MultiPolygon
*   - gml:MultiGeometry
*   - gml:pos
*   - gml:coordinates
*   - gml:posList
*   - gml:interior
*   - gml:exterior
*   - gml:LinearRing
*   - gml:outerboundaryis
*   - gml:innerboundaryis
*   - gml:pointMember
*   - gml:geometryMember
*   - gml:lineStringMember
*   - gml:polygonMember
*   - gml:geometryMember
*
* Writing:
*   - gml:Point
*   - gml:LineString
*   - gml:Polygon
*   - gml:MultiPoint
*   - gml:MultiLineString
*   - gml:MultiPolygon
*   - gml:MultiGeometry
*   - gml:pos
*   - gml:posList
*   - gml:LinearRing
*   - gml:outerboundaryis
*   - gml:innerboundaryis
*   - gml:pointMember
*   - gml:geometryMember
*   - gml:lineStringMember
*   - gml:polygonMember
*   - gml:geometryMember
****************************************************************************/

using Microsoft.Maps.SpatialToolbox.Internals;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Maps.SpatialToolbox.IO
{
    /// <summary>
    /// This class contains a set of methods for reading and writing GML objects.
    /// Note that GML is often used in combination with other feed types and is not normally used as it's own feed.
    /// </summary>
    public static class Gml
    {
        #region Private Properties

        private static Regex SpacesRx = new Regex(@"[\s]{2,}");
        private static Regex CoordArtifactRx = new Regex(@"[\n\t,]");
        private static string[] SpaceSplitter = new string[] {" "};

        internal const string GmlNamespace = "http://www.opengis.net/gml";
        private static XNamespace gmlNS = XNamespace.Get(GmlNamespace);

        #endregion

        #region GML Read Methods

        /// <summary>
        /// Parses a GML element as a Geometry from a XElement
        /// </summary>
        /// <param name="node">Node containing the GML information.</param>
        /// <param name="optimize">A boolean indicating if the shape should be optimized when parsed.</param>
        /// <param name="tolerance">Tolerance to use in the Vertex Reduction method when optimizing</param>
        /// <returns>A Geometry representing the ML node.</returns>
        public static async Task<Geometry> Parse(XElement node, bool optimize, double tolerance)
        {
            Geometry geom = null;

            //if (node != null && (node.Name == null || string.IsNullOrEmpty(node.Name.LocalName)))
            //{
            //    if (node.NextSibling != null)
            //    {
            //        node = node.NextSibling;
            //    }
            //    else
            //    {
            //        node = null;
            //    }
            //}

            if (node != null)
            {
                string nodeName;

                foreach (var n in node.Elements())
                {
                    if (n.Name.Namespace == gmlNS)
                    {
                        nodeName = n.Name.LocalName.ToLowerInvariant();
                        switch (nodeName)
                        {
                            case "point":
                            case "linestring":
                            case "polygon":
                            case "multipoint":
                            case "multilintstring":
                            case "multipolygon":
                            case "multigeometry":
                                geom = await ParseGmlNode(n, optimize, tolerance);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            return geom;
        }

        private static async Task<Geometry> ParseGmlNode(XElement node, bool optimize, double tolerance)
        {
            Geometry geom = null;
            List<Geometry> geoms;
            Coordinate tempCoord;

            if (node != null && node.Name.Namespace == gmlNS)
            {
                string nodeName = node.Name.LocalName.ToLowerInvariant();
                switch (nodeName)
                {
                    case "point":
                        var cNode = XmlUtilities.GetChildNode(node, "pos");

                        if (cNode == null)
                        {
                            cNode = XmlUtilities.GetChildNode(node, "coordinates");
                        }

                        if (cNode != null)
                        {
                            tempCoord = ParsePos(cNode);
                            if (!double.IsNaN(tempCoord.Latitude) && !double.IsNaN(tempCoord.Longitude))
                            {
                                geom = new Point(tempCoord);
                            }
                        }
                        break;
                    case "linestring":
                        var pos = XmlUtilities.GetChildNodes(node, "poslist");
                        if (pos == null)
                        {
                            pos = XmlUtilities.GetChildNodes(node, "coordinates");
                        }

                        var lineCoords = new List<CoordinateCollection>();

                        foreach (var c in pos)
                        {
                            lineCoords.Add(await ParsePosList(c, optimize, tolerance));
                        }

                        var lines = new List<LineString>();

                        foreach (var l in lineCoords)
                        {
                            if (l != null && l.Count >= 2)
                            {
                                lines.Add(new LineString(l));
                            }
                        }

                        if (lines.Count == 1)
                        {
                            geom = lines[0];
                        }
                        else if (lines.Count > 1)
                        {
                            geom = new MultiLineString(lines);
                        }
                        break;
                    case "polygon":
                        CoordinateCollection exRing = null;
                        var inRings = new List<CoordinateCollection>();

                        foreach (var c in node.Elements())
                        {
                            nodeName = c.Name.LocalName.ToLowerInvariant();
                            switch (nodeName)
                            {
                                case "exterior":
                                case "outerboundaryis":
                                    var exR = await ParseLinearRings(c, optimize, tolerance);
                                    if (exR != null)
                                    {
                                        exRing = exR;
                                    }
                                    break;
                                case "interior":
                                case "innerboundaryis":
                                    var inR = await ParseLinearRings(c, optimize, tolerance);
                                    if (inR != null)
                                    {
                                        inRings.Add(inR);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }

                        if (exRing != null && exRing.Count >= 3)
                        {
                            geom = new Polygon(exRing);

                            if (inRings.Count > 0)
                            {
                                (geom as Polygon).InteriorRings = inRings;
                            }
                        }
                        break;
                    case "multipoint":
                        var mp = new MultiPoint();
                        geoms = await ParseGmlMembers(node, optimize, tolerance);
                        foreach (var p in geoms)
                        {
                            if (p is Point)
                            {
                                mp.Geometries.Add(p as Point);
                            }
                        }
                        geom = mp;
                        break;
                    case "multilintstring":
                        var mline = new MultiLineString();
                        geoms = await ParseGmlMembers(node, optimize, tolerance);
                        foreach (var p in geoms)
                        {
                            if (p is LineString)
                            {
                                mline.Geometries.Add(p as LineString);
                            }
                        }
                        geom = mline;
                        break;
                    case "multipolygon":
                        var mpoly = new MultiPolygon();
                        geoms = await ParseGmlMembers(node, optimize, tolerance);
                        foreach (var p in geoms)
                        {
                            if (p is Polygon)
                            {
                                mpoly.Geometries.Add(p as Polygon);
                            }
                        }
                        geom = mpoly;
                        break;
                    case "multigeometry":
                        geom = new GeometryCollection(await ParseGmlMembers(node, optimize, tolerance));
                        break;
                    default:
                        break;
                }
            }

            return geom;
        }

        private static async Task<List<Geometry>> ParseGmlMembers(XElement node, bool optimize, double tolerance)
        {
            var geoms = new List<Geometry>();

            string nodeName;

            foreach (var n in node.Elements())
            {
                if (n.Name.Namespace == gmlNS)
                {
                    nodeName = n.Name.LocalName.ToLowerInvariant();
                    switch (nodeName)
                    {
                        case "pointmember":
                        case "linestringmember":
                        case "polygonmember":
                        case "geometrymember":
                            var geom = await Parse(n, optimize, tolerance);
                            if (geom != null)
                            {
                                geoms.Add(geom);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            return geoms;
        }

        private static async Task<CoordinateCollection> ParseLinearRings(XElement rings, bool optimize, double tolerance)
        {
            CoordinateCollection rCoords = null;

            foreach (var c in rings.Elements())
            {
                if (c.Name.Namespace == gmlNS &&
                    string.Compare(c.Name.LocalName, "linearring", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    foreach (var cc in c.Elements())
                    {
                        var coords = await ParsePosList(cc, optimize, tolerance);

                        if (coords != null && coords.Count >= 3)
                        {
                            rCoords = coords;
                            break;
                        }
                    }
                }
            }

            if (optimize)
            {
                rCoords = await SpatialTools.VertexReductionAsync(rCoords, tolerance);
            }

            return rCoords;
        }

        private static async Task<CoordinateCollection> ParsePosList(XElement node, bool optimize, double tolerance)
        {
            if (node.Name.Namespace == gmlNS &&
                (string.Compare(node.Name.LocalName, "poslist", StringComparison.OrdinalIgnoreCase) == 0 ||
                 string.Compare(node.Name.LocalName, "coordinates", StringComparison.OrdinalIgnoreCase) == 0))
            {
                var dimension = XmlUtilities.GetDoubleAttribute(node, "dimension");
                int dim = 2;

                if (!double.IsNaN(dimension) && dimension > 2)
                {
                    dim = (int) dimension;
                }

                var sCoord = XmlUtilities.GetString(node, false);
                var vals = CoordArtifactRx.Replace(sCoord, " ")
                    .Split(SpaceSplitter, StringSplitOptions.RemoveEmptyEntries);

                var c = new CoordinateCollection();
                double lat, lon, alt;

                if (dim == 3 && vals.Length >= 3)
                {
                    for (var i = 0; i < vals.Length; i = i + 3)
                    {
                        if (double.TryParse(vals[i], NumberStyles.Float, CultureInfo.InvariantCulture, out lat)
                            && double.TryParse(vals[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out lon)
                            && double.TryParse(vals[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out alt))
                        {
                            c.Add(new Coordinate(lat, lon, alt));
                        }
                    }
                }
                else if (dim == 2 && vals.Length >= 2)
                {
                    for (var i = 0; i < vals.Length; i = i + 2)
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

        private static Coordinate ParsePos(XElement node)
        {
            var sCoord = XmlUtilities.GetString(node, false);
            var vals = CoordArtifactRx.Replace(sCoord, " ").Split(SpaceSplitter, StringSplitOptions.RemoveEmptyEntries);
            double lat, lon, alt;

            if (vals.Length >= 2 && double.TryParse(vals[0], NumberStyles.Float, CultureInfo.InvariantCulture, out lat)
                && double.TryParse(vals[1], NumberStyles.Float, CultureInfo.InvariantCulture, out lon))
            {
                Coordinate c;

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

        #endregion

        #region GML Write Methods

        /// <summary>
        /// Writes a Gemoetry object as a GML element to a xml writer.
        /// </summary>
        /// <param name="geom">Geometry object to write.</param>
        /// <param name="xmlWriter">The xml writer to write GML to.</param>
        public static void Create(Geometry geom, XmlWriter xmlWriter)
        {
            if (geom is Point)
            {
                CreatePoint(geom as Point, xmlWriter);
            }
            else if (geom is LineString)
            {
                CreateLineString(geom as LineString, xmlWriter);
            }
            else if (geom is Polygon)
            {
                CreatePolygon(geom as Polygon, xmlWriter);
            }
            else if (geom is MultiPoint)
            {
                CreateMultiPoint(geom as MultiPoint, xmlWriter);
            }
            else if (geom is MultiLineString)
            {
                CreateMultiLineString(geom as MultiLineString, xmlWriter);
            }
            else if (geom is MultiPolygon)
            {
                CreateMultiPolygon(geom as MultiPolygon, xmlWriter);
            }
            else if (geom is GeometryCollection)
            {
                CreateGeometryCollection(geom as GeometryCollection, xmlWriter);
            }
        }

        private static void CreatePos(Coordinate coord, int dimension, XmlWriter xmlWriter)
        {
            //<gml:pos srsDimension="2">45.67 88.56</gml:pos>

            xmlWriter.WriteStartElement("pos", GmlNamespace);
            if (dimension == 3)
            {
                xmlWriter.WriteAttributeString("dimension", "3");
                xmlWriter.WriteValue(string.Format(CultureInfo.InvariantCulture, "{0:0.#####},{1:0.#####},{2}",
                    coord.Latitude, coord.Longitude, coord.Altitude.Value));
            }
            else
            {
                xmlWriter.WriteValue(string.Format(CultureInfo.InvariantCulture, "{0:0.#####},{1:0.#####}",
                    coord.Latitude, coord.Longitude));
            }
            xmlWriter.WriteEndElement();
        }

        private static void CreatePosList(CoordinateCollection coords, int dimension, XmlWriter xmlWriter)
        {
            //<gml:posList dimension="2">
            //    14.35954 100.57967
            //    14.35871 100.57337
            //</gml:posList>
            xmlWriter.WriteStartElement("posList", GmlNamespace);

            if (dimension <= 2)
            {
                xmlWriter.WriteAttributeString("dimension", "2");

                foreach (var c in coords)
                {
                    xmlWriter.WriteValue(string.Format(CultureInfo.InvariantCulture, "{0:0.#####},{1:0.#####} ",
                        c.Latitude, c.Longitude));
                }
            }
            else
            {
                //<gml:posList dimension="3">
                //    14.35954 100.57967 0
                //    14.35871 100.57337 100
                //</gml:posList>
                xmlWriter.WriteAttributeString("dimension", "3");

                foreach (var c in coords)
                {
                    xmlWriter.WriteValue(string.Format(CultureInfo.InvariantCulture, "{0:0.#####},{1:0.#####},{2} ",
                        c.Latitude, c.Longitude, c.Altitude.HasValue ? c.Altitude.Value : 0));
                }
            }

            xmlWriter.WriteEndElement();
        }

        private static void CreateLinearRing(CoordinateCollection ring, int dimension, XmlWriter xmlWriter)
        {
            //     <gml:LinearRing>
            //       <gml:posList>
            //         -71.106216 42.366661
            //         -71.105576 42.367104
            //         -76.104378 42.367134
            //         -71.106216 42.366661
            //       </gml:posList>
            //     </gml:LinearRing>

            //Close ring if it isn't already
            if (ring != null && ring.Count >= 3 && ring[0] != ring[ring.Count - 1])
            {
                ring.Add(ring[0]);
            }

            xmlWriter.WriteStartElement("LinearRing", GmlNamespace);
            CreatePosList(ring, dimension, xmlWriter);
            xmlWriter.WriteEndElement();
        }

        internal static void CreatePoint(Point point, XmlWriter xmlWriter)
        {
            //<gml:Point>
            //  <gml:pos>1.157092,45.839186</gml:pos>
            //</gml:Point>
            xmlWriter.WriteStartElement("Point", GmlNamespace);
            CreatePos(point.Coordinate, (point.Coordinate.Altitude.HasValue) ? 3 : 2, xmlWriter);
            xmlWriter.WriteEndElement();
        }

        internal static void CreateLineString(LineString linestring, XmlWriter xmlWriter)
        {
            if (linestring.Vertices != null && linestring.Vertices.Count >= 2)
            {
                int dim = (linestring.Vertices[0].Altitude.HasValue) ? 3 : 2;

                //    <gml:LineString>
                //        <gml:posList dimension="2">
                //            14.35954 100.57967
                //            14.35871 100.57337
                //        </gml:posList>
                //    </gml:LineString>
                xmlWriter.WriteStartElement("LineString", GmlNamespace);
                CreatePosList(linestring.Vertices, dim, xmlWriter);
                xmlWriter.WriteEndElement();
            }
        }

        internal static void CreatePolygon(Polygon polygon, XmlWriter xmlWriter)
        {
            if (polygon.ExteriorRing != null && polygon.ExteriorRing.Count >= 2)
            {
                int dim = polygon.ExteriorRing[0].Altitude.HasValue ? 3 : 2;

                // <gml:Polygon>
                //   <gml:outerBoundaryIs>
                //     <gml:LinearRing>
                //       <gml:posList>
                //         -71.106216 42.366661
                //         -71.105576 42.367104
                //         -76.104378 42.367134
                //         -71.106216 42.366661
                //       </gml:posList>
                //     </gml:LinearRing>
                //   </gml:outerBoundaryIs>
                // </gml:Polygon>

                xmlWriter.WriteStartElement("Polygon", GmlNamespace);

                xmlWriter.WriteStartElement("outerBoundaryIs", GmlNamespace);
                CreateLinearRing(polygon.ExteriorRing, dim, xmlWriter);
                xmlWriter.WriteEndElement();

                if (polygon.InteriorRings != null)
                {
                    foreach (var r in polygon.InteriorRings)
                    {
                        xmlWriter.WriteStartElement("innerBoundaryIs", GmlNamespace);
                        CreateLinearRing(r, dim, xmlWriter);
                        xmlWriter.WriteEndElement();
                    }
                }

                xmlWriter.WriteEndElement();
            }
        }

        internal static void CreateMultiPoint(MultiPoint points, XmlWriter xmlWriter)
        {
            //<gml:MultiPoint>
            //  <gml:pointMember>
            //    <gml:Point>
            //      <gml:posList>0.490018,45.654676</gml:posList>
            //    </gml:Point>
            //  </gml:pointMember>
            //  <gml:pointMember>
            //    <gml:Point>
            //      <gml:posList>1.157092,45.839186</gml:posList>
            //    </gml:Point>
            //  </gml:pointMember>
            //</gml:MultiPoint>

            xmlWriter.WriteStartElement("MultiPoint", GmlNamespace);

            foreach (var p in points)
            {
                xmlWriter.WriteStartElement("pointMember", GmlNamespace);
                CreatePoint(p, xmlWriter);
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
        }

        internal static void CreateMultiLineString(MultiLineString lines, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("MultiLineString", GmlNamespace);

            foreach (var l in lines)
            {
                xmlWriter.WriteStartElement("lineStringMember", GmlNamespace);
                CreateLineString(l, xmlWriter);
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
        }

        internal static void CreateMultiPolygon(MultiPolygon polygons, XmlWriter xmlWriter)
        {
            //<gml:MultiPolygon>
            // <gml:polygonMember>
            //   <gml:Polygon>
            //     <gml:outerBoundaryIs>
            //       <gml:LinearRing>
            //         <gml:posList>...</gml:posList>
            //       </gml:LinearRing>
            //     </gml:outerBoundaryIs>
            //   </gml:Polygon>
            // </gml:polygonMember>
            // <gml:polygonMember>
            //   <gml:Polygon>
            //     <gml:outerBoundaryIs>
            //       <gml:LinearRing>
            //         <gml:posList>...</gml:posList>
            //       </gml:LinearRing>
            //     </gml:outerBoundaryIs>
            //   </gml:Polygon>
            // </gml:polygonMember>
            //</gml:MultiPolygon>
            xmlWriter.WriteStartElement("MultiPolygon", GmlNamespace);

            foreach (var p in polygons)
            {
                xmlWriter.WriteStartElement("polygonMember", GmlNamespace);
                CreatePolygon(p, xmlWriter);
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
        }

        internal static void CreateGeometryCollection(GeometryCollection geoms, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("MultiGeometry", GmlNamespace);

            foreach (var g in geoms)
            {
                xmlWriter.WriteStartElement("geometryMember", GmlNamespace);
                Create(g, xmlWriter);
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
        }

        #endregion
    }
}