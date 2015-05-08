using Microsoft.Maps.SpatialToolbox.Internals;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Maps.SpatialToolbox.IO
{
    /// <summary>
    /// An abstract class used to define a common set for reading and writing Xml, and GeoJSON feeds.
    /// </summary>
    public abstract class BaseTextFeed : BaseFeed
    {
        #region Internal Property

        internal bool stripHtml;

        internal XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
        {
            Encoding = System.Text.Encoding.UTF8,
            Indent = true,
            NamespaceHandling = NamespaceHandling.OmitDuplicates
        };

        #endregion

        #region Constructor

        /// <summary>
        /// An abstract class used to define a common set for reading and writing Xml, and GeoJSON feeds.
        /// </summary>
        public BaseTextFeed()
        {
            stripHtml = false;
        }

        public BaseTextFeed(bool stripHtml)
        {
            this.stripHtml = stripHtml;
        }

        public BaseTextFeed(double tolerance) : base(tolerance)
        {
        }

        public BaseTextFeed(bool stripHtml, double tolerance)
            : base(tolerance)
        {
            this.stripHtml = stripHtml;
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Reads a feed
        /// </summary>
        /// <param name="feed">A feed as a string.</param>
        /// <param name="stripHtml">Option to strip out HTML from string fields.</param>
        /// <returns>A SpatialDataSet containing the spatial data from the feed.</returns>
        public abstract Task<SpatialDataSet> ReadAsync(string feed);

        public abstract Task<SpatialDataSet> ReadAsync(Uri feedUri);

        public abstract Task<string> WriteAsync(SpatialDataSet data);

        #endregion

        #region Internal Methods

        internal string GetBaseUri(Uri uri)
        {
            string baseUri = string.Empty;

            var idx = uri.AbsoluteUri.LastIndexOf("/");

            if (idx >= 0)
            {
                baseUri = uri.AbsoluteUri.Substring(0, idx + 1);
            }

            return baseUri;
        }

        internal void SetMetadataString(ShapeMetadata metadata, string nodeName, XElement node, bool stripHtml)
        {
            var s = XmlUtilities.GetString(node, stripHtml);

            if (!string.IsNullOrWhiteSpace(s) && !metadata.Properties.ContainsKey(nodeName))
            {
                metadata.Properties.Add(nodeName, s);
            }
        }

        internal string[] SplitCoordString(string coordString, Regex artifaxRx, string[] splitters)
        {
            return artifaxRx.Replace(coordString, " ").Split(splitters, StringSplitOptions.RemoveEmptyEntries);
        }

        internal Uri CleanUri(string uri, string baseUrl)
        {
            if (!string.IsNullOrWhiteSpace(uri))
            {
                if (!uri.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    && !uri.StartsWith("ms-appx", StringComparison.OrdinalIgnoreCase))
                {
                    uri = baseUrl + uri;
                }

                try
                {
                    return new Uri(uri);
                }
                catch
                {
                }
            }

            return null;
        }

        #endregion
    }
}