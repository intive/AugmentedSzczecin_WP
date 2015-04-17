using System.Collections.Generic;

namespace Microsoft.Maps.SpatialToolbox
{
    /// <summary>
    /// A class used to store metadata of a spatial object.
    /// </sumC:\Users\richbrun\Desktop\Code Samples\CodePlex37\Mirosoft.Maps.SpatialToolbox\Source\Microsoft.Maps.SpatialToolbox.Core\Models\ShapeMetadata.csmary>
    public class ShapeMetadata
    {
        #region Private Properties

        private Dictionary<string, object> _properties;

        #endregion

        #region Constructor

        /// <summary>
        /// A class used to store metadata of a spatial object.
        /// </summary>
        public ShapeMetadata()
        {
            _properties = new Dictionary<string, object>();
            ID = string.Empty;
            Title = string.Empty;
            Description = string.Empty;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// A unique identifier for the Geometry
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Main title or name.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Main descriotion.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A dictionary of all additional metadata objects by name.
        /// </summary>
        public Dictionary<string, object> Properties
        {
            get
            {
                return _properties;
            }
        }

        #endregion

        #region Public Methods

        public bool HasMetadata()
        {
            return (!string.IsNullOrEmpty(ID) || !string.IsNullOrEmpty(Title) || !string.IsNullOrEmpty(Description) || Properties.Count > 0);
        }

        #endregion
    }
}
