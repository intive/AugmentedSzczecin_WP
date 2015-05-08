using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maps.SpatialToolbox
{
    /// <summary>
    /// A class used to store a collection of spatial objects.
    /// </summary>
    public class SpatialDataSet
    {
        #region Constructor

        /// <summary>
        /// A class used to store a collection of spatial objects.
        /// </summary>
        public SpatialDataSet()
        {
            Geometries = new List<Geometry>();
            Metadata = new ShapeMetadata();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Metadata related to an item
        /// </summary>
        public ShapeMetadata Metadata { get; set; }

        /// <summary>
        /// Stlyes used by the geometries in the dataset.
        /// </summary>
        public Dictionary<string, ShapeStyle> Styles { get; set; }

        /// <summary>
        /// Indicates the bounding box that encloses all shapes in the shapefile.
        /// </summary>
        public BoundingBox BoundingBox { get; set; }

        /// <summary>
        /// A collection of geometries
        /// </summary>
        public List<Geometry> Geometries { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string Error { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Appends a data set to an existing data set.
        /// </summary>
        /// <param name="data">Data set to append</param>
        public void Append(SpatialDataSet data)
        {
            if (data != null)
            {
                BoundingBox = data.BoundingBox.Join(BoundingBox);

                Geometries.AddRange(data.Geometries);

                if (data.Styles != null && data.Styles.Count > 0)
                {
                    if (Styles == null)
                    {
                        Styles = data.Styles;
                    }
                    else
                    {
                        foreach (var key in data.Styles.Keys)
                        {
                            if (!Styles.ContainsKey(key))
                            {
                                Styles.Add(key, data.Styles[key]);
                            }
                        }
                    }
                }

                if (!Metadata.HasMetadata())
                {
                    Metadata = data.Metadata;
                }

                if (!string.IsNullOrEmpty(data.Error))
                {
                    if (string.IsNullOrEmpty(Error))
                    {
                        Error = data.Error;
                    }
                    else
                    {
                        Error += "\r\n" + data.Error;
                    }
                }
            }
        }

        /// <summary>
        /// Goes through all the styles and removes duplicates.
        /// </summary>
        public void CleanUpStyles()
        {
            if (Styles != null && Geometries != null)
            {
                //key = old style key, value = new style key
                var styleMap = new Dictionary<string, string>();

                //Find duplicate styles
                foreach (var s in Styles)
                {
                    foreach (var s2 in Styles)
                    {
                        //Ensure different key and that key isn't already marked to be removed.
                        if (string.Compare(s.Key, s2.Key) != 0 && !styleMap.ContainsKey(s.Key))
                        {
                            if (s.Value.Equals(s2.Value))
                            {
                                styleMap.Add(s2.Key, s.Key);
                            }
                        }
                    }
                }

                //Update style keys on Geometries
                foreach (var g in Geometries)
                {
                    if (styleMap.ContainsKey(g.StyleKey))
                    {
                        g.StyleKey = styleMap[g.StyleKey];
                    }
                }

                //Remove old styles
                foreach (var s in styleMap)
                {
                    if (Styles.ContainsKey(s.Key))
                    {
                        Styles.Remove(s.Key);
                    }
                }
            }
        }

        #endregion
    }
}