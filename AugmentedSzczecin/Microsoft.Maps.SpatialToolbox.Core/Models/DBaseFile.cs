using System.Collections.Generic;

namespace Microsoft.Maps.SpatialToolbox
{
    /// <summary>
    /// A class used to represent a dBase database file as an object.
    /// </summary>
    public class DBaseFile
    {
        #region Constructor

        /// <summary>
        /// A class used to represent a dBase database file as an object.
        /// </summary>
        public DBaseFile()
        {
            Header = new DBaseFileHeader();
            Rows = new List<Dictionary<string, object>>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Header information for columnes. 
        /// </summary>
        public DBaseFileHeader Header { get; set; }

        /// <summary>
        /// A list of Dictionaries used to store a row of information.
        /// </summary>
        public IList<Dictionary<string, object>> Rows { get; set; }

        #endregion
    }
}