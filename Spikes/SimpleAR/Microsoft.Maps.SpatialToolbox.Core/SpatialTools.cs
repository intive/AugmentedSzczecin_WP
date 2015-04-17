using System;
using System.Threading.Tasks;

namespace Microsoft.Maps.SpatialToolbox
{
    /// <summary>
    /// A set of common spatial calculation tools.
    /// </summary>
    public static class SpatialTools
    {
        #region Earth Related Constants

        /// <summary>
        /// The approximate spherical radius of the Earth
        /// NOTE: In reality the Earth is an ellipsoid.
        /// </summary>
        internal static class EarthRadius
        {
            /// <summary>
            /// Earth Radius in Kilometers
            /// </summary>
            public const double KM = 6378.135;

            /// <summary>
            /// Earth Radius in Meters
            /// </summary>
            public const double Meters = 6378135;

            /// <summary>
            /// Earth Radius in Miles
            /// </summary>
            public const double Miles = 3963.189;

            /// <summary>
            /// Earth Radius in Feet
            /// </summary>
            public const double Feet = 20925640;
        }

        #endregion

        #region Earth Radius

        /// <summary>
        /// Retrieves the radius of the earth in a specific distance unit for WGS84. Defaults unit is in Meters.
        /// </summary>
        /// <param name="units">Unit of distance measurement</param>
        /// <returns>A double that represents the radius of the earth in a specific distance unit. Defaults unit is in KM's.</returns>
        public static double GetEarthRadius(DistanceUnits units)
        {
            switch (units)
            {
                case DistanceUnits.Feet:
                    return EarthRadius.Feet;
                case DistanceUnits.Meters:
                    return EarthRadius.Meters;
                case DistanceUnits.Miles:
                    return EarthRadius.Miles;
                case DistanceUnits.Yards:
                    return ConvertDistance(EarthRadius.KM, DistanceUnits.KM, DistanceUnits.Yards);
                case DistanceUnits.KM:
                default:
                    return EarthRadius.KM;
            }
        }

        #endregion
 
        #region Degree and Radian Conversions

        /// <summary>
        /// Converts an angle that is in degrees to radians. Angle * (PI / 180)
        /// </summary>
        /// <param name="angle">An angle in degrees</param>
        /// <returns>An angle in radians</returns>
        public static double ToRadians(double angle)
        {
            return angle * (Math.PI / 180);
        }

        /// <summary>
        /// Converts an angle that is in radians to degress. Angle * (180 / PI)
        /// </summary>
        /// <param name="angle">An angle in radians</param>
        /// <returns>An angle in degrees</returns>
        public static double ToDegrees(double angle)
        {
            return angle * (180 / Math.PI);
        }

        #endregion

        #region Decimal Degree and Degree Minute Second Converstions

        /// <summary>
        /// Converts a decimal degree into a string in the format of days minutes seconds
        /// </summary>
        /// <param name="degree">Decimal degree</param>
        /// <param name="isLatitude">Boolean specifying if the degree is a latitude coordinate</param>
        /// <returns>A string version of an angle in days, minutes, seconds format.</returns>
        public static string DecimalDegreeToDMS(double degree, bool isLatitude)
        {
            var orientation = "";

            if (isLatitude)
            {
                if (degree < 0)
                {
                    orientation += "S";
                    degree *= -1;
                }
                else
                    orientation += "N";
            }
            else
            {
                if (degree < 0)
                {
                    orientation += "W";
                    degree *= -1;
                }
                else
                {
                    orientation += "E";
                }
            }

            int day = (int)degree;
            int min = (int)((degree - day) * 60);
            double sec = (degree - (double)day - (double)min / 60) * 3600;

            return string.Format("{0} {1}° {2}' {3}\"", orientation, day, min, sec);
        }

        /// <summary>
        /// Converts a days minutes seconds coordinate into a decimal degree's coordinate
        /// </summary>
        /// <param name="degree">Degree coordinate</param>
        /// <param name="minute">Minute coordinate</param>
        /// <param name="second">Second coordinate</param>
        /// <returns>A decimal degree coordinate</returns>
        public static double DMSToDecimalDegree(double degree, double minute, double second)
        {
            return degree + (minute / 60) + (second / 3600);
        }

        #endregion

        #region Distance Conversions

        /// <summary>
        /// Converts a distance from distance unit to another.
        /// </summary>
        /// <param name="distance">a double that represents the distance.</param>
        /// <param name="fromUnits">The distance unit the original distance is in.</param>
        /// <param name="toUnits">The disired distance unit to convert to.</param>
        /// <returns>A distance in the new units.</returns>
        public static double ConvertDistance(double distance, DistanceUnits fromUnits, DistanceUnits toUnits)
        {
            //Convert the distance to kilometers
            switch (fromUnits)
            {
                case DistanceUnits.Meters:
                    distance /= 1000;
                    break;
                case DistanceUnits.Feet:
                    distance /= 3288.839895;
                    break;
                case DistanceUnits.Miles:
                    distance *= 1.609344;
                    break;
                case DistanceUnits.Yards:
                    distance *= 0.0009144;
                    break;
                case DistanceUnits.KM:
                    break;
            }

            //Convert from kilometers to output distance unit
            switch (toUnits)
            {
                case DistanceUnits.Meters:
                    distance *= 1000;
                    break;
                case DistanceUnits.Feet:
                    distance *= 5280;
                    break;
                case DistanceUnits.Miles:
                    distance /= 1.609344;
                    break;
                case DistanceUnits.Yards:
                    distance *= 1093.6133;
                    break;
                case DistanceUnits.KM:
                    break;
            }

            return distance;
        }

        #endregion

        #region Calaculate Heading

        /// <summary>
        /// Calculates the heading from one Coordinate to another.
        /// </summary>
        /// <param name="origin">Point of origin.</param>
        /// <param name="destination">Destination point to calculate relative heading to.</param>
        /// <returns>A heading degrees between 0 and 360. 0 degrees points due North.</returns>
        public static double CalculateHeading(Coordinate origin, Coordinate destination)
        {
            double radianLat1 = ToRadians(origin.Latitude);
            double radianLat2 = ToRadians(destination.Latitude);

            double dLon = ToRadians(destination.Longitude - origin.Longitude);

            double dy = Math.Sin(dLon) * Math.Cos(radianLat2);
            double dx = Math.Cos(radianLat1) * Math.Sin(radianLat2) - Math.Sin(radianLat1) * Math.Cos(radianLat2) * Math.Cos(dLon);

            return (ToDegrees(Math.Atan2(dy, dx)) + 360) % 360;
        }

        #endregion

        #region Calculate Destination Coordinate

        /// <summary>
        /// Calculates a destination coordinate based on a starting coordinate, a bearing, a distance, and a distance unit type.
        /// </summary>
        /// <param name="origin">Coordinate that the destination is relative to</param>
        /// <param name="bearing">A bearing (heading) angle between 0 - 360 degrees. 0 - North, 90 - East, 180 - South, 270 - West</param>
        /// <param name="distance">Distance that destination is away</param>
        /// <param name="units">Unit of distance measurement</param>
        /// <returns>A coordinate that is the specified distance away from the origin</returns>
        public static Coordinate CalculateDestinationCoordinate(Coordinate origin, double bearing, double distance, DistanceUnits units)
        {
            var radius = GetEarthRadius(units);

            //convert latitude, longitude and heading into radians
            double latitudeRad = ToRadians(origin.Latitude);
            double longitudeRad = ToRadians(origin.Longitude);
            double bearingRad = ToRadians(bearing);

            double centralAngle = distance / radius;
            double destinationLatitudeRad = Math.Asin(Math.Sin(latitudeRad) * Math.Cos(centralAngle) + Math.Cos(latitudeRad) * Math.Sin(centralAngle) * Math.Cos(bearingRad));
            double destinationLongitudeRad = longitudeRad + Math.Atan2(Math.Sin(bearingRad) * Math.Sin(centralAngle) * Math.Cos(latitudeRad), Math.Cos(centralAngle) - Math.Sin(latitudeRad) * Math.Sin(destinationLatitudeRad));

            return new Coordinate(ToDegrees(destinationLatitudeRad), ToDegrees(destinationLongitudeRad));
        }

        #endregion

        #region Calculate Midpoint LatLong

        /// <summary>
        /// Calculates the midpoint coordinate between two Coordinates.
        /// </summary>
        /// <param name="origin">First coordinate to calculate midpoint between</param>
        /// <param name="destination">Second coordinate to calculate midpoint between</param>
        /// <returns>A coordinate that lies equidistance from the two specified coordinates. 
        /// This is calculated along the shortest path between the two coordinates.</returns>
        public static Coordinate CalculateMidpoint(Coordinate origin, Coordinate destination)
        {
            double arcLength = HaversineDistance(origin, destination, DistanceUnits.KM);
            double brng = CalculateHeading(origin, destination);

            return CalculateDestinationCoordinate(origin, brng, arcLength / 2, DistanceUnits.KM);
        }

        #endregion

        #region Haversine Distance Calculation method

        /// <summary>
        /// Calculate the distance between two coordinates on the surface of a sphere (Earth).
        /// </summary>
        /// <param name="origin">First coordinate to calculate distance between</param>
        /// <param name="destination">Second coordinate to calculate distance between</param>
        /// <param name="units">Unit of distance measurement</param>
        /// <returns>The shortest distance in the specifed units</returns>
        public static double HaversineDistance(Coordinate origin, Coordinate destination, DistanceUnits units)
        {
            double radius = GetEarthRadius(units);

            double dLat = ToRadians(destination.Latitude - origin.Latitude);
            double dLon = ToRadians(destination.Longitude - origin.Longitude);

            double a = Math.Pow(Math.Sin(dLat / 2), 2) + Math.Pow(Math.Cos(ToRadians(origin.Latitude)), 2) * Math.Pow(Math.Sin(dLon / 2), 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return radius * c;
        }

        #endregion

        #region Circle Coordinate Generator

        /// <summary>
        /// Calculates a list of coordinates that are an equal distance away from a central point to create an approximated circle.
        /// </summary>
        /// <param name="center">Center of the circle.</param>
        /// <param name="radius">Radius of the circle.</param>
        /// <param name="distanceUnits">Distance units of radius.</param>
        /// <returns>A list of coordinates that form a circle</returns>
        public static CoordinateCollection GenerateCircle(Coordinate center, double radius, DistanceUnits units)
        {
            return GenerateRegularPolygon(center, radius, units, 25, 0);
        }

        #endregion

        #region Regular Polygon Coordinate Generator

        /// <summary>
        /// Calculates a list of coordinates that are an equal distance away from a central point to create a regular polygon.
        /// </summary>
        /// <param name="center">Center of the polygon.</param>
        /// <param name="radius">Radius of the polygon.</param>
        /// <param name="distanceUnits">Distance units of radius.</param>
        /// <param name="numberOfPoints">Number of points the polygon should have.</param>
        /// <param name="offset">The offset to rotate the polygon. When 0 the first coordinate will align with North.</param>
        /// <returns>A list of coordinates that form a regular polygon</returns>
        public static CoordinateCollection GenerateRegularPolygon(Coordinate center, double radius, DistanceUnits units, int numberOfPoints, double offset)
        {
            var points = new CoordinateCollection();
            double centralAngle = 360 / numberOfPoints;

            for (var i = 0; i <= numberOfPoints; i++) {
                points.Add(CalculateDestinationCoordinate(center, (i * centralAngle + offset) % 360, radius, units));
            }

            return points;
        }

        #endregion

        #region Calculate Geodesic Coordinates

        /// <summary>
        /// Takes a list of coordinates and fills in the space between them with accurately 
        /// positioned pints to form a Geodesic path.
        /// 
        /// Source: http://alastaira.wordpress.com/?s=geodesic
        /// </summary>
        /// <param name="coordinates">List of coordinates to work with.</param>
        /// <param name="nodeSize">Number of nodes to insert between each coordinate</param>
        /// <returns>A set of coordinates that for geodesic paths.</returns>
        public static CoordinateCollection CalculateGeodesic(CoordinateCollection coordinates, int nodeSize)
        {
            if (nodeSize <= 0) 
            { 
                nodeSize = 32;
            }
                  
            var locs = new CoordinateCollection();   
    
            for (var i = 0; i < coordinates.Count - 1; i++) 
            {                  
                    // Convert coordinates from degrees to Radians           
                    var lat1 = ToRadians(coordinates[i].Latitude);        
                    var lon1 = ToRadians(coordinates[i].Longitude);           
                    var lat2 = ToRadians(coordinates[i + 1].Latitude);           
                    var lon2 = ToRadians(coordinates[i + 1].Longitude);   
        
                    // Calculate the total extent of the route           
                    var d = 2 * Math.Asin(Math.Sqrt(Math.Pow((Math.Sin((lat1 - lat2) / 2)), 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow((Math.Sin((lon1 - lon2) / 2)), 2)));           
                    
                    // Calculate positions at fixed intervals along the route
                    for (var k = 0; k <= nodeSize; k++) 
                    {             
                        var f = (k / (double)nodeSize);             
                        var A = Math.Sin((1 - f) * d) / Math.Sin(d);             
                        var B = Math.Sin(f * d) / Math.Sin(d);             
                        
                        // Obtain 3D Cartesian coordinates of each point             
                        var x = A * Math.Cos(lat1) * Math.Cos(lon1) + B * Math.Cos(lat2) * Math.Cos(lon2);             
                        var y = A * Math.Cos(lat1) * Math.Sin(lon1) + B * Math.Cos(lat2) * Math.Sin(lon2);             
                        var z = A * Math.Sin(lat1) + B * Math.Sin(lat2);             
                        
                        // Convert these to latitude/longitude             
                        var lat = Math.Atan2(z, Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)));             
                        var lon = Math.Atan2(y, x);             

                        // Add this to the array             
                        locs.Add(new Coordinate(ToDegrees(lat), ToDegrees(lon)));          
                    }
            }
            
            return locs;
        }

        #endregion

        #region Vertex Reduction Algorithm

        /**
         * Vertex reduction is the brute-force algorithm for simplifying a List of coordinates. 
         * For this algorithm, a polygon vertex is discarded when its distance from a prior 
         * initial vertex is less than some minimum tolerance ε > 0. Specifically, after fixing 
         * an initial vertex V0, successive vertices Vi are tested and rejected if they are less 
         * than ε away from V0. But, when a vertex is found that is further away than ε, then it 
         * is accepted as part of the new simplified polygon, and it also becomes the new initial 
         * vertex for further simplification of the polygon.  Thus, the resulting edge segments 
         * between accepted vertices are larger than the ε tolerance. This algorithm perserves 
         * the start and end points of the List of coordinates.
         * 
         * Based on:
         * http://softsurfer.com/Archive/algorithm_0205/algorithm_0205.htm
         * http://rbrundritt.wordpress.com/2011/12/03/vertex-reductionone-of-my-secret-weapons/
         */

        public static Task<CoordinateCollection> VertexReductionAsync(CoordinateCollection coordinates, double tolerance)
        {
            return Task.Run<CoordinateCollection>(() =>
            {
                return VertexReduction(coordinates, tolerance);
            });
        }

        /// <summary>
        /// Vertex reduction is the brute-force algorithm for simplifying a list of coordinates. 
        /// It does this by ensuring that no two coordinates are closer than a specified distance 
        /// in the unit of measurment. If working with coordinates in degrees then the distance 
        /// would be in degrees.
        /// </summary>
        /// <param name="coordinates">A list of coordinates to reduce</param>
        /// <param name="tolerance">A distance in the unit of the coordinate parameters in which no two coordinates should be closer than.</param>
        /// <returns>A list of coordinates where no two cordinates are closer than the tolerance distance.</returns>
        public static CoordinateCollection VertexReduction(CoordinateCollection coordinates, double tolerance)
        {
            //Verify that there are at least 2 or more coordinates in the LocationCollection
            if (coordinates == null || coordinates.Count < 3)
            {
                return coordinates;
            }

            var newCoordinates = new CoordinateCollection();

            //Store the initial cooridnate
            newCoordinates.Add(coordinates[0]);

            var baseCoord = coordinates[0];

            for (int i = 1; i < coordinates.Count - 1; i++)
            {
                //check to see if the distance between the base coordinate and the coordinate in question is outside the tolerance distance.
                if (Math.Sqrt(Math.Pow(coordinates[i].Latitude - baseCoord.Latitude, 2) + Math.Pow(coordinates[i].Longitude - baseCoord.Longitude, 2)) > tolerance)
                {
                    //store the coordinate and make it the new base coordinate for comparison.
                    newCoordinates.Add(coordinates[i]);
                    baseCoord = coordinates[i];
                }
            }

            //store the last cooridnate
            newCoordinates.Add(coordinates[coordinates.Count - 1]);

            //ensure there are enough points to create a SQLGeometry object
            if (newCoordinates.Count > 4)
            {
                //return the new coordinate collection
                return newCoordinates;
            }

            //return the original coordinates
            return coordinates;
        }

        #endregion
    }
}
