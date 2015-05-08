using Microsoft.Phone.Maps.Controls;
using System.Device.Location;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Microsoft.Maps.SpatialToolbox.Bing
{
    public static class MapShapeExtensions
    {
        #region Geometry Objects To Bing Maps geometries

        /// <summary>
        /// Converts a Geometry Coordinate object into a Bing Maps Location object.
        /// </summary>
        /// <param name="coordinate">Coordinate to convert.</param>
        /// <returns>Location object of the Coordinate.</returns>
        public static GeoCoordinate ToBMGeometry(this Coordinate coordinate)
        {
            if (coordinate.Altitude.HasValue)
            {
                return new GeoCoordinate(coordinate.Latitude, coordinate.Longitude, coordinate.Altitude.Value);
            }
            else
            {
                return new GeoCoordinate(coordinate.Latitude, coordinate.Longitude);
            }
        }

        /// <summary>
        /// Converts a CoordinateCollection into a Bing Maps GeoCoordinateCollection.
        /// </summary>
        /// <param name="coordinates">CoordinateCollection to convert.</param>
        /// <returns>GeoCoordinateCollection of the converted CoordinateCollection.</returns>
        public static GeoCoordinateCollection ToBMGeometry(this CoordinateCollection coordinates)
        {
            var locs = new GeoCoordinateCollection();

            if (coordinates.Count > 0 && coordinates[0].Altitude.HasValue)
            {
                foreach (var c in coordinates)
                {
                    locs.Add(new GeoCoordinate(c.Latitude, c.Longitude, c.Altitude.Value));
                }
            }
            else
            {
                foreach (var c in coordinates)
                {
                    locs.Add(new GeoCoordinate(c.Latitude, c.Longitude));
                }
            }

            return locs;
        }

        /// <summary>
        /// Converts a Point object into a Pushpin
        /// </summary>
        /// <param name="point">A Point object</param>
        /// <returns>A Pushpin object</returns>
        public static MapOverlay ToBMGeometry(this Point point)
        {
            var pushpin = new MapOverlay()
            {
                GeoCoordinate = point.Coordinate.ToBMGeometry(),
                Content = new Ellipse()
                {
                    Width = 30,
                    Height = 30,
                    Stroke = new SolidColorBrush(Colors.White),
                    StrokeThickness = 5,
                    Fill = new SolidColorBrush(Colors.Blue),
                    Margin = new System.Windows.Thickness(-20, -20, 0, 0),
                    Tag = point.Metadata
                }
            };

            return pushpin;
        }

        /// <summary>
        /// Converts a LineString object into a MapPolyline. 
        /// </summary>
        /// <param name="linestring">A LineString object</param>
        /// <returns>A MapPolyline object</returns>
        public static MapPolyline ToBMGeometry(this LineString linestring)
        {
            var line = new MapPolyline()
            {
                Path = linestring.Vertices.ToBMGeometry(),
                StrokeColor = new Color()
                {
                    A = 150,
                    G = 255
                },
                StrokeThickness = 3
            };
            line.SetValue(MapElementExt.TagProperty, linestring.Metadata);
            return line;
        }

        /// <summary>
        /// Converts a Polygon into a MapPolygon. 
        /// Note that MapPolygon's do not support holes.
        /// </summary>
        /// <param name="polygon">A Polygon object</param>
        /// <returns>A MapPolygon object</returns>
        public static MapPolygon ToBMGeometry(this Polygon polygon)
        {
            var myPoly = new MapPolygon()
            {
                Path = polygon.ExteriorRing.ToBMGeometry(),
                FillColor = new Color()
                {
                    A = 150,
                    G = 255
                },
                StrokeColor = new Color()
                {
                    A = 150,
                    G = 255
                },
                StrokeThickness = 3
            };

            myPoly.SetValue(MapElementExt.TagProperty, polygon.Metadata);

            //TODO: Add support for holes/inner rings in the future.

            return myPoly;
        }

        #endregion

        #region Bing Maps geometries To Geometry Objects

        /// <summary>
        /// Converts a MapElement into a Geometry object.
        /// </summary>
        /// <param name="shape">A Bing Maps MapShape object</param>
        /// <returns>A Geometry representation of the MapElement object</returns>
        public static Geometry ToGeometry(this MapElement shape)
        {
            if (shape is MapPolyline)
            {
                return (shape as MapPolyline).ToGeometry();
            }
            else if (shape is MapPolygon)
            {
                return (shape as MapPolygon).ToGeometry();
            }

            return null;
        }

        /// <summary>
        /// Converts a Location into a Coordinate object.
        /// </summary>
        /// <param name="location">A Bing Maps Location object</param>
        /// <returns>A Coordinate representation of the Location object</returns>
        public static Coordinate ToGeometry(this GeoCoordinate location)
        {
            return new Coordinate(location.Latitude, location.Longitude, location.Altitude);
        }

        /// <summary>
        /// Converts a LocationCollection into a CoordinateCollection object.
        /// </summary>
        /// <param name="locations">A Bing Maps LocationCollection object</param>
        /// <returns>A CoordinateCollection representation of the LocationCollection object</returns>
        public static CoordinateCollection ToGeometry(this GeoCoordinateCollection locations)
        {
            CoordinateCollection coords = new CoordinateCollection();

            for (int i = 0; i < locations.Count; i++)
            {
                coords.Add(new Coordinate(locations[i].Latitude, locations[i].Longitude, locations[i].Altitude));
            }

            return coords;
        }

        /// <summary>
        /// Converts a Pushpin into a Point object.
        /// </summary>
        /// <param name="pushpin">A Bing Maps Pushpin object</param>
        /// <returns>A Point representation of the Pushpin object</returns>
        public static Point ToGeometry(this MapOverlay pushpin)
        {
            return new Point(pushpin.GeoCoordinate.ToGeometry());
        }

        /// <summary>
        /// Converts a MapPolyline into a LineString object.
        /// </summary>
        /// <param name="polyline">A Bing Maps MapPolyline object</param>
        /// <returns>A LineString representation of the MapPolyline object</returns>
        public static LineString ToGeometry(this MapPolyline polyline)
        {
            return new LineString(polyline.Path.ToGeometry());
        }

        /// <summary>
        /// Converts a MapPolygon into a Polygon object.
        /// </summary>
        /// <param name="polygon">A Bing Maps MapPolygon object</param>
        /// <returns>A Polygon representation of the MapPolygon object</returns>
        public static Polygon ToGeometry(this MapPolygon polygon)
        {
            return new Polygon(polygon.Path.ToGeometry());
        }

        #endregion
    }
}
