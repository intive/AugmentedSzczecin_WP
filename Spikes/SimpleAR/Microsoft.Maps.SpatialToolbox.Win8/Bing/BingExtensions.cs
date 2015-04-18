#if WINDOWS_APP
using Bing.Maps;
using System;
using System.Collections.Generic;
using Windows.Devices.Geolocation;
#elif WPF
using Microsoft.Maps.MapControl.WPF;
using System.Windows.Media;
#endif

namespace Microsoft.Maps.SpatialToolbox.Bing
{
    /// <summary>
    /// A set of methods that extend both the Bing Maps Shape classes and the Geometry class. 
    /// These methods make it easy to convert between Bing Maps Shapes and Geometry objects. 
    /// </summary>
    public static class BingExtensions
    {
        #region Geometry Objects To Bing Maps geometries

        /// <summary>
        /// Converts a Geometry Coordinate object into a Bing Maps Location object.
        /// </summary>
        /// <param name="coordinate">Coordinate to convert.</param>
        /// <returns>Location object of the Coordinate.</returns>
        public static Location ToBMGeometry(this Coordinate coordinate)
        {
            //TODO: Add altitude if supported in the future.
            return new Location(coordinate.Latitude, coordinate.Longitude);
        }

        /// <summary>
        /// Converts a CoordinateCollection into a Bing Maps LocationCollection.
        /// </summary>
        /// <param name="coordinates">CoordinateCollection to convert.</param>
        /// <returns>LocationCollection of the converted CoordinateCollection.</returns>
        public static LocationCollection ToBMGeometry(this CoordinateCollection coordinates)
        {
            var locs = new LocationCollection();

            foreach (var c in coordinates)
            {
                locs.Add(new Location(c.Latitude, c.Longitude));
            }

            return locs;
        }

        /// <summary>
        /// Converts a BoundingBox into a Bing Maps LocationRect object.
        /// </summary>
        /// <param name="boundingBox">BoundingBox object to convert.</param>
        /// <returns>LocationRect of the converted BoundingBox.</returns>
        public static LocationRect ToBMGeometry(this BoundingBox boundingBox)
        {
            return new LocationRect(boundingBox.Center.ToBMGeometry(), boundingBox.Width, boundingBox.Height);
        }

        /// <summary>
        /// Converts a Point object into a Pushpin
        /// </summary>
        /// <param name="point">A Point object</param>
        /// <returns>A Pushpin object</returns>
        public static Pushpin ToBMGeometry(this Point point)
        {
            return StyleTools.GenerateMapShape(point, null) as Pushpin;

            //#if WINDOWS_APP
            //var pushpin = new Pushpin();
            //pushpin.Tag = point.Metadata;
            //MapLayer.SetPosition(pushpin, point.Coordinate.ToBMGeometry());
            //#elif WPF
            //var pushpin = new Pushpin()
            //{
            //    Location = point.Coordinate.ToBMGeometry()
            //};

            ////TODO: Add Tag to pushpin
            //// pushpin.SetValue(PushpinExt.TagProperty, point.Metadata);
            //#endif

            //return pushpin;
        }

        /// <summary>
        /// Converts a LineString object into a MapPolyline. 
        /// </summary>
        /// <param name="linestring">A LineString object</param>
        /// <returns>A MapPolyline object</returns>
        public static MapPolyline ToBMGeometry(this LineString linestring)
        {
            return StyleTools.GenerateMapShape(linestring, null);
        }

        /// <summary>
        /// Converts a Polygon into a MapPolygon. 
        /// Note that MapPolygon's do not support holes.
        /// </summary>
        /// <param name="polygon">A Polygon object</param>
        /// <returns>A MapPolygon object</returns>
        public static MapPolygon ToBMGeometry(this Polygon polygon)
        {
            var shapes = StyleTools.GenerateMapShape(polygon, null);

            if (shapes.Count > 0 && shapes[0] is MapPolygon)
            {
                return shapes[0] as MapPolygon;
            }

            return null;
        }

        #endregion

        #region Bing Maps geometries To Geometry Objects

        #if WINDOWS_APP

        /// <summary>
        /// Converts a MapShape into a Geometry object.
        /// </summary>
        /// <param name="shape">A Bing Maps MapShape object</param>
        /// <returns>A Geometry representation of the MapMultiPoint object</returns>
        public static Geometry ToGeometry(this MapShape shape)
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
        /// Converts a MapMultiPoint into a Geometry object.
        /// </summary>
        /// <param name="shape">A Bing Maps MapMultiPoint object</param>
        /// <returns>A Geometry representation of the MapMultiPoint object</returns>
        public static Geometry ToGeometry(this MapMultiPoint shape)
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

        #elif WPF

        /// <summary>
        /// Converts a MapShape into a Geometry object.
        /// </summary>
        /// <param name="shape">A Bing Maps MapShape object</param>
        /// <returns>A Geometry representation of the MapMultiPoint object</returns>
        public static Geometry ToGeometry(this MapShapeBase shape)
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

        #endif

        /// <summary>
        /// Converts a Location into a Coordinate object.
        /// </summary>
        /// <param name="location">A Bing Maps Location object</param>
        /// <returns>A Coordinate representation of the Location object</returns>
        public static Coordinate ToGeometry(this Location location)
        {
            return new Coordinate(location.Latitude, location.Longitude);
        }

        /// <summary>
        /// Converts a LocationCollection into a CoordinateCollection object.
        /// </summary>
        /// <param name="locations">A Bing Maps LocationCollection object</param>
        /// <returns>A CoordinateCollection representation of the LocationCollection object</returns>
        public static CoordinateCollection ToGeometry(this LocationCollection locations)
        {
            CoordinateCollection coords = new CoordinateCollection();

            for (int i = 0; i < locations.Count; i++)
            {
                coords.Add(new Coordinate(locations[i].Latitude, locations[i].Longitude));
            }

            return coords;
        }

        /// <summary>
        /// Converts a LocationRect into a BoundingBox object.
        /// </summary>
        /// <param name="locationRect">A Bing Maps LocationRect object</param>
        /// <returns>A BoundingBox representation of the LocationRect object</returns>
        public static BoundingBox ToGeometry(this LocationRect locationRect)
        {
            return new BoundingBox(locationRect.Center.ToGeometry(), locationRect.Width, locationRect.Height);
        }

        /// <summary>
        /// Converts a Pushpin into a Point object.
        /// </summary>
        /// <param name="pushpin">A Bing Maps Pushpin object</param>
        /// <returns>A Point representation of the Pushpin object</returns>
        public static Point ToGeometry(this Pushpin pushpin)
        {
            #if WINDOWS_APP
            return new Point(MapLayer.GetPosition(pushpin).ToGeometry());
            #elif WPF
            return new Point(pushpin.Location.ToGeometry());
            #endif
        }

        /// <summary>
        /// Converts a MapPolyline into a LineString object.
        /// </summary>
        /// <param name="polyline">A Bing Maps MapPolyline object</param>
        /// <returns>A LineString representation of the MapPolyline object</returns>
        public static LineString ToGeometry(this MapPolyline polyline)
        {
            return new LineString(polyline.Locations.ToGeometry());
        }

        /// <summary>
        /// Converts a MapPolygon into a Polygon object.
        /// </summary>
        /// <param name="polygon">A Bing Maps MapPolygon object</param>
        /// <returns>A Polygon representation of the MapPolygon object</returns>
        public static Polygon ToGeometry(this MapPolygon polygon)
        {
            return new Polygon(polygon.Locations.ToGeometry());
        }

        #endregion

        #region BasicGeoposition Extensions

        #if WINDOWS_APP

        public static LocationCollection ToLocationCollection(this IList<BasicGeoposition> pointList)
        {
            var locs = new LocationCollection();

            foreach (var p in pointList)
            {
                locs.Add(p.ToLocation());
            }

            return locs;
        }

        public static Geopoint ToGeopoint(this Location location)
        {
            return new Geopoint(new BasicGeoposition() { Latitude = location.Latitude, Longitude = location.Longitude });
        }

        public static Location ToLocation(this Geopoint location)
        {
            return new Location(location.Position.Latitude, location.Position.Longitude);
        }

        public static Location ToLocation(this BasicGeoposition location)
        {
            return new Location(location.Latitude, location.Longitude);
        }

        #endif

        #endregion

        /// <summary>
        /// Determines if two LocationRect's intersect. 
        /// </summary>
        /// <param name="rect">The base LocationRect.</param>
        /// <param name="rect2">A LocationRect to test the intersection of.</param>
        /// <returns>A boolean indicating if the two LocationRect's intersect.</returns>
        public static bool Intersects(this LocationRect rect, LocationRect rect2)
        {
            return rect.ToGeometry().Intersects(rect2.ToGeometry());
        }

        /// <summary>
        /// Calculates the area of intersection of two LocationRect's. 
        /// </summary>
        /// <param name="rect">The base LocationRect.</param>
        /// <param name="rect2">A LocationRect to calculate the intersection of.</param>
        /// <returns>A LocationRect of the area of intersection or null.</returns>
        public static LocationRect Intersection(this LocationRect rect, LocationRect rect2)
        {
            var bb = rect.ToGeometry().Intersection(rect2.ToGeometry());
            if (bb != null)
            {
                return bb.ToBMGeometry();
            }

            return null;
        }
    }
}
