using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maps.SpatialToolbox.IO
{
    /// <summary>
    /// An abstract class used to define a common set for reading and writing methods for feeds.
    /// </summary>
    public abstract class BaseFeed
    {
        #region Internal Properties

        internal bool optimize = false;
        internal double tolerance;

        #endregion

        #region Constructor

        /// <summary>
        /// An abstract class used to define a common set for reading and writing methods for feeds.
        /// </summary>
        public BaseFeed()
        {
        }

        /// <summary>
        /// An abstract class used to define a common set for reading and writing methods for feeds.
        /// </summary>
        /// <param name="tolerance">Tolerance to use when optimzing shapes using the Vertex reduction method.</param>
        public BaseFeed(double tolerance)
        {
            this.tolerance = tolerance;
            optimize = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads a feed from a stream.
        /// </summary>
        /// <param name="stream">Stream to read.</param>
        /// <returns>A spatial data set.</returns>
        public abstract Task<SpatialDataSet> ReadAsync(Stream stream);

        /// <summary>
        /// Writes a spatial data set to a Stream 
        /// </summary>
        /// <param name="geometries">A spatial data set to write</param>
        /// <param name="stream">Stream to write to</param>
        public abstract Task WriteAsync(SpatialDataSet data, Stream stream);

        #endregion
    }
}
