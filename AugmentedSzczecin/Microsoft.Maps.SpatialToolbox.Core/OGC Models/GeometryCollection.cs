using System;
using System.Collections.Generic;

namespace Microsoft.Maps.SpatialToolbox
{
    /// <summary>
    /// Regresents a collection of Geometry objects.
    /// </summary>
    public class GeometryCollection : GeometryCollection<Geometry>
    {
        #region Constructors

        /// <summary>
        /// Regresents a coolection of Geometry objects.
        /// </summary>
        public GeometryCollection()
        {
        }

        /// <summary>
        /// Regresents a coolection of Geometry objects.
        /// </summary>
        /// <param name="geographies">A collection of gegraphy objects</param>
        public GeometryCollection(IEnumerable<Geometry> geographies)
            : base(geographies)
        {
        }

        /// <summary>
        /// Regresents a coolection of Geometry objects.
        /// </summary>
        /// <param name="capacity">An integer representing the size of the collection</param>
        public GeometryCollection(int capacity)
            : base(capacity)
        {
        }

        #endregion
    }

    /// <summary>
    /// Regresents a Generic collection of Specific Geometry objects, such as 
    /// Polygons (MultiPolygon), LineString (MultiLineString), Points (MultiPoints).
    /// </summary>
    public class GeometryCollection<T> : Geometry
    {
        #region Constructor

        /// <summary>
        /// Regresents a Generic coolection of Specific Geometry objects, such as 
        /// Polygons (MultiPolygon), LineString (MultiLineString), Points (MultiPoints).
        /// </summary>
        internal GeometryCollection()
        {
            this.Geometries = new List<T>();
        }

        /// <summary>
        /// Regresents a Generic coolection of Specific Geometry objects, such as 
        /// Polygons (MultiPolygon), LineString (MultiLineString), Points (MultiPoints).
        /// </summary>
        /// <param name="geographies">A collection of Geometry objects</param>
        internal GeometryCollection(IEnumerable<T> geographies)
        {
            this.Geometries = new List<T>(geographies);
        }

        /// <summary>
        /// Regresents a Generic coolection of Specific Geometry objects, such as 
        /// Polygons (MultiPolygon), LineString (MultiLineString), Points (MultiPoints).
        /// </summary>
        /// <param name="capacity">An integer representing the size of the collection</param>
        internal GeometryCollection(int capacity)
        {
            this.Geometries = new List<T>(capacity);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// List of Geometry objects
        /// </summary>
        public List<T> Geometries { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// An enumerator that iterates through the List
        /// </summary>
        /// <returns>An enumerator that iterates through the List</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this.Geometries.GetEnumerator();
        }

        /// <summary>
        /// Calculates an Bounding Box that encompasses all Geometry objects in a Geometry collection
        /// </summary>
        /// <returns>A BoundingBox object of the bounding box that encompassess the Geometry Collection</returns>
        public new BoundingBox Envelope()
        {
            if ((this == null) || (this.Geometries.Count == 0))
            {
                return null;
            }

            BoundingBox boundingBox = null;

            foreach (var Geometry in this.Geometries)
            {
                //initialize the bounding box to the first Geometry object in the collection
                if (boundingBox == null)
                {
                    boundingBox = (Geometry as Geometry).Envelope();
                }
                else
                {
                    //Join the bounding boxes of all Geometry objects together
                    boundingBox = boundingBox.Join((Geometry as Geometry).Envelope());
                }
            }

            return boundingBox;
        }

        /// <summary>
        /// Specifies the type of the geometry object as a GeometryType enumerator.
        /// </summary>
        /// <returns>A GeometryType enumerator</returns>
        public override GeometryType STGeometryType()
        {
            return GeometryType.GeometryCollection;
        }

        // <summary>
        /// Returns the number of child geometries in the shape.
        /// </summary>
        /// <returns>Number of child geometries in shape.</returns>
        public override int STNumGeometries()
        {
            return this.Geometries.Count;
        }

        #endregion
    }
}