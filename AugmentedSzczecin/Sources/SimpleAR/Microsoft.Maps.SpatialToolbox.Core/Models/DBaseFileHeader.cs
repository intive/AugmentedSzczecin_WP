using System;
using System.Collections.Generic;

namespace Microsoft.Maps.SpatialToolbox
{
    /// <summary>
    /// A class that holds the properties of a dBase file header
    /// </summary>
    public class DBaseFileHeader
    {
        #region Constructor

        /// <summary>
        /// DBaseFileHeader constructor
        /// </summary>
        public DBaseFileHeader() 
        {
            ColumnDescriptions = new List<DBaseColumnDefinition>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// An List of DBaseColumnDescription objects
        /// </summary>
        public IList<DBaseColumnDefinition> ColumnDescriptions { get; set; }

        /// <summary>
        /// Return the length of the header
        /// </summary>
        /// <returns></returns>
        public int HeaderLength { get; set; }

        /// <summary>
        /// the date when the dbf file was last updated
        /// </summary>
        public DateTime LastUpdateDate { get; set; }

        /// <summary>
        /// The number of columns in the dbf file
        /// </summary>
        public int NumColumns { get; set; }

        /// <summary>
        /// Return the number of rows in the dbf file.
        /// </summary>
        /// <returns></returns>
        public int NumRows { get; set; }

        /// <summary>
        /// The length of a row in bytes
        /// </summary>
        public int RowLength { get; set; }

        #endregion
    }
}
