using Microsoft.Maps.SpatialToolbox.Bing;
using System.Collections.Generic;
using System.Drawing;

namespace Microsoft.Maps.SpatialToolbox.Imaging
{
    public class ViewInfo
    {
        #region Private Properties

        private PointF _topLeftGlobalPixel;
        public int _width, _height;
        double _zoom;

        #endregion

        #region Constructor

        public ViewInfo(QuadKey quadKey)
        {
            _width = 256;
            _height = 256;
            _topLeftGlobalPixel = new PointF(quadKey.X * 256, quadKey.Y * 256);
            _zoom = quadKey.ZoomLevel;
        }

        public ViewInfo(int width, int height, QuadKey quadKey)
        {
            _width = width;
            _height = height;
            _topLeftGlobalPixel = new PointF(quadKey.X * 256, quadKey.Y * 256);
            _zoom = quadKey.ZoomLevel;
        }

        public ViewInfo(int width, int height, Coordinate topLeftCorner, double zoom)
        {
            _width = width;
            _height = height;
            _zoom = zoom;

            _topLeftGlobalPixel = TileMath.LocationToGlobalPixel(topLeftCorner, zoom);
        }

        #endregion

        #region Public Properties

        public PointF TopLeftGlobalPixel { get { return _topLeftGlobalPixel; } }

        public double Zoom { get { return _zoom; } }

        public int Width { get { return _width; } }

        public int Height { get { return _height; } }

        #endregion

        #region Public Methods

        public PointF[] RingToPointFList(CoordinateCollection coords)
        {
            int numPoints = coords.Count;
            var points = new PointF[numPoints];

            System.Threading.Tasks.Parallel.For(0, numPoints, (i) =>
            {
                points[i] = LatLongToPointF(coords[i]);
            });

            return points;
        }

        public PointF LatLongToPointF(Coordinate coord)
        {
            var globalPx = TileMath.LocationToGlobalPixel(coord, _zoom);
            globalPx.X -= _topLeftGlobalPixel.X;
            globalPx.Y -= _topLeftGlobalPixel.Y;
            return globalPx;
        }

        #endregion
    }
}
