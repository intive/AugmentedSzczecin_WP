using Microsoft.Maps.SpatialToolbox.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace Microsoft.Maps.SpatialToolbox.Component
{
    public sealed class FeedService
    {
        public static IAsyncOperation<FeedResponse> ReadFeedStreamAsync(IRandomAccessStream stream, string feedType)
        {
            return Task.Run<FeedResponse>(async () =>
            {
                var response = new FeedResponse();

                try
                {
                    var feed = GetBaseFeed(feedType);

                    if (feed != null)
                    {
                        var data = await feed.ReadAsync(stream.AsStreamForRead());

                        if (data != null && data.Geometries != null && data.Geometries.Count > 0)
                        {                            
                            response.Shapes = new List<FeedShape>();

                            FeedShapeStyle style;

                            foreach (var g in data.Geometries)
                            {
                                style = null;

                                if(!string.IsNullOrEmpty(g.StyleKey) && data.Styles != null && data.Styles.ContainsKey(g.StyleKey))
                                {
                                    style = ConvertStyle(data.Styles[g.StyleKey]);
                                }

                                var s = new FeedShape(g.ToString())
                                {
                                    Style = style
                                };

                                if (g.Metadata != null)
                                {
                                    s.Title = g.Metadata.Title;
                                    s.Description = g.Metadata.Description;
                                    s.Metadata = g.Metadata.Properties;
                                }

                                response.Shapes.Add(s);
                            }

                            if (data.BoundingBox != null)
                            {
                                response.BoundingBox = string.Format("{0},{1},{2},{3}",
                                    data.BoundingBox.Center.Latitude,
                                    data.BoundingBox.Center.Longitude,
                                    data.BoundingBox.Width,
                                    data.BoundingBox.Height);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.Error = ex.Message;
                }

                return response;
            }).AsAsyncOperation<FeedResponse>();
        }

        #region Private Methods

        private static BaseFeed GetBaseFeed(string feedType)
        {
            switch (feedType.ToLowerInvariant())
            {
                case "georss":
                    return new GeoRssFeed();
                case "gpx":
                    return new GpxFeed();
                case "kml":
                    return new KmlFeed();
                case "shp":
                    return new ShapefileReader();
                case "wkt":
                    return new WellKnownText();
                case "wkb":
                    return new WellKnownBinary();
                case "geojson":
                    return new GeoJsonFeed();
                default:
                    break;
            }

            return null;
        }

        private static FeedShapeStyle ConvertStyle(ShapeStyle style)
        {
            if(style == null)
            {
                return null;
            }

            var s = new FeedShapeStyle()
            {
                FillPolygon = style.FillPolygon,
                IconHeading = style.IconHeading,
                IconScale = style.IconScale,
                IconUrl = style.IconUrl,
                OutlinePolygon = style.OutlinePolygon,
                StrokeThickness = style.StrokeThickness
            };

            if (style.FillColor.HasValue)
            {
                s.FillColor = style.FillColor.Value.ToString();
            }

            if (style.IconColor.HasValue)
            {
                s.IconColor = style.IconColor.Value.ToString();
            }

            if (style.StrokeColor.HasValue)
            {
                s.StrokeColor = style.StrokeColor.Value.ToString();
            }

            return s;
        }

        #endregion
    }
}
