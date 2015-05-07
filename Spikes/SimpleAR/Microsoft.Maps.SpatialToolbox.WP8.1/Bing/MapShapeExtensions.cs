using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Maps;

namespace Microsoft.Maps.SpatialToolbox.Bing
{
    public static class MapShapeExtensions
    {
        #region Geometry Objects To Bing Maps geometries

        /// <summary>
        /// Converts a Geometry Coordinate object into a Geopoint object.
        /// </summary>
        /// <param name="coordinate">Coordinate to convert.</param>
        /// <returns>Geopoint object of the Coordinate.</returns>
        public static BasicGeoposition ToBMGeometry(this Coordinate coordinate)
        {
            if (coordinate.Altitude.HasValue)
            {
                return new BasicGeoposition()
                {
                    Latitude = coordinate.Latitude,
                    Longitude = coordinate.Longitude,
                    Altitude = coordinate.Altitude.Value
                };
            }
            else
            {
                return new BasicGeoposition()
                {
                    Latitude = coordinate.Latitude,
                    Longitude = coordinate.Longitude
                };
            }
        }

        /// <summary>
        /// Converts a CoordinateCollection into a Bing Maps Geopath.
        /// </summary>
        /// <param name="coordinates">CoordinateCollection to convert.</param>
        /// <returns>Geopath of the converted CoordinateCollection.</returns>
        public static Geopath ToBMGeometry(this CoordinateCollection coordinates)
        {
            var locs = new System.Collections.Generic.List<BasicGeoposition>();

            if (coordinates.Count > 0 && coordinates[0].Altitude.HasValue)
            {
                foreach (var c in coordinates)
                {
                    locs.Add(new BasicGeoposition()
                    {
                        Latitude = c.Latitude,
                        Longitude = c.Longitude,
                        Altitude = c.Altitude.Value
                    });
                }
            }
            else
            {
                foreach (var c in coordinates)
                {
                    locs.Add(new BasicGeoposition()
                    {
                        Latitude = c.Latitude,
                        Longitude = c.Longitude
                    });
                }
            }

            return new Geopath(locs);
        }

        /// <summary>
        /// Converts a BoundingBox into a Bing Maps GeoboundingBox object.
        /// </summary>
        /// <param name="boundingBox">BoundingBox object to convert.</param>
        /// <returns>GeoboundingBox of the converted BoundingBox.</returns>
        public static GeoboundingBox ToBMGeometry(this BoundingBox boundingBox)
        {
            return new GeoboundingBox(boundingBox.TopLeft.ToBMGeometry(), boundingBox.BottomRight.ToBMGeometry());
        }

        /// <summary>
        /// Converts a Point object into a MapIcon
        /// </summary>
        /// <param name="point">A Point object</param>
        /// <returns>A MapIcon object</returns>
        public static MapIcon ToBMGeometry(this Point point)
        {
            var pushpin = new MapIcon()
            {
                Location = new Geopoint(point.Coordinate.ToBMGeometry())
            };

            pushpin.SetValue(MapElementExt.TagProperty, point.Metadata);

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
        /// <param name="shape">A Bing Maps MapElement object</param>
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
            else if (shape is MapIcon)
            {
                return (shape as MapIcon).ToGeometry();
            }

            return null;
        }

        /// <summary>
        /// Converts a BasicGeoposition into a Coordinate object.
        /// </summary>
        /// <param name="location">A BasicGeoposition object</param>
        /// <returns>A Coordinate representation of the BasicGeoposition object</returns>
        public static Coordinate ToGeometry(this BasicGeoposition location)
        {
            return new Coordinate(location.Latitude, location.Longitude, location.Altitude);
        }

        /// <summary>
        /// Converts a Geopoint into a Coordinate object.
        /// </summary>
        /// <param name="location">A Bing Maps Geopoint object</param>
        /// <returns>A Coordinate representation of the Geopoint object</returns>
        public static Coordinate ToGeometry(this Geopoint location)
        {
            if (location.Position.Altitude != 0)
            {
                return new Coordinate(location.Position.Latitude, location.Position.Longitude);
            }
            else
            {
                return new Coordinate(location.Position.Latitude, location.Position.Longitude, location.Position.Altitude);
            }
        }

        /// <summary>
        /// Converts a Geopath into a CoordinateCollection object.
        /// </summary>
        /// <param name="locations">A Geopath object</param>
        /// <returns>A CoordinateCollection representation of the Geopath object</returns>
        public static CoordinateCollection ToGeometry(this Geopath locations)
        {
            CoordinateCollection coords = new CoordinateCollection();

            for (int i = 0; i < locations.Positions.Count; i++)
            {
                coords.Add(locations.Positions[i].ToGeometry());
            }

            return coords;
        }

        /// <summary>
        /// Converts a GeoboundingBox into a BoundingBox object.
        /// </summary>
        /// <param name="locationRect">A Bing Maps GeoboundingBox object</param>
        /// <returns>A BoundingBox representation of the GeoboundingBox object</returns>
        public static BoundingBox ToGeometry(this GeoboundingBox locationRect)
        {
            var topLeft = locationRect.NorthwestCorner;
            var bottomRight = locationRect.SoutheastCorner;
            return new BoundingBox(topLeft.Longitude, topLeft.Latitude, bottomRight.Longitude, bottomRight.Latitude);
        }

        /// <summary>
        /// Converts a MapIcon into a Point object.
        /// </summary>
        /// <param name="pushpin">A MapIcon object</param>
        /// <returns>A Point representation of the MapIcon object</returns>
        public static Point ToGeometry(this MapIcon pushpin)
        {
            return new Point(pushpin.Location.ToGeometry());
        }

        /// <summary>
        /// Converts a UIElement into a Point object.
        /// </summary>
        /// <param name="pushpin">A UIElement object</param>
        /// <returns>A Point representation of the UIElement object</returns>
        public static Point ToGeometry(this UIElement pushpin)
        {
            var loc = pushpin.GetValue(MapControl.LocationProperty);
            if (loc != null && loc is Geopoint)
            {
                return new Point((loc as Geopoint).ToGeometry());
            }

            return null;
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
