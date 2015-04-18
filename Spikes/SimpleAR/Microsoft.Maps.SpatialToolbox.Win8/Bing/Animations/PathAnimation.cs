using System;
using System.Collections.Generic;

#if WINDOWS_APP
using Bing.Maps;
using Windows.UI.Xaml;
#elif WPF
using System.Windows.Threading;
using Microsoft.Maps.MapControl.WPF;
#elif WINDOWS_PHONE
using System.Windows.Threading;
using Microsoft.Phone.Maps.Controls;
using System.Device.Location;
#elif WINDOWS_PHONE_APP
using Windows.UI.Xaml;
using Windows.Devices.Geolocation;
#endif

namespace Microsoft.Maps.SpatialToolbox.Bing.Animations
{
    /// <summary>
    /// A class for creating animations along a path.
    /// Based on http://blogs.bing.com/maps/2014/09/25/part-2-bring-your-maps-to-life-creating-animations-with-bing-maps-net/
    /// </summary>
    public class PathAnimation
    {
        #region Private Properties

        private const int _delay = 30;
        private const double EARTH_RADIUS_KM = 6378.1;

        private DispatcherTimer _timerId;

#if WINDOWS_PHONE
        private GeoCoordinateCollection _path;
        private GeoCoordinateCollection _intervalLocs;
#elif WINDOWS_PHONE_APP
        private List<BasicGeoposition> _path;
        private List<BasicGeoposition> _intervalLocs;
#else
        private LocationCollection _path;
        private LocationCollection _intervalLocs;
#endif

        private bool _isGeodesic = false;        
        private List<int> _intervalIdx;

        private int? _duration;
        private int _frameIdx = 0;
        private bool _isPaused;

        #endregion

        #region Constructor

        /// <summary>This class extends from the BaseAnimation class and cycles through a set of coordinates over a period of time, calculating mid-point coordinates along the way.</summary>
        /// <param name="path">An array of locations to cycle through.</param>
        /// <param name="intervalCallback">A function that is called when a frame is to be rendered. This callback function recieves four values; current cordinate, index on path and frame index.</param>
        /// <param name="isGeodesic">Indicates if the path should follow the curve of the earth.</param>
        /// <param name="duration">Length of time in ms that the animation should run for. Default is 1000 ms.</param>
#if WINDOWS_PHONE
        public PathAnimation(GeoCoordinateCollection path, IntervalCallback intervalCallback, bool isGeodesic, int? duration)
#elif WINDOWS_PHONE_APP
        public PathAnimation(List<BasicGeoposition> path, IntervalCallback intervalCallback, bool isGeodesic, int? duration)
#else
        public PathAnimation(LocationCollection path, IntervalCallback intervalCallback, bool isGeodesic, int? duration)
#endif
        {
            _path = path;
            _isGeodesic = isGeodesic;
            _duration = duration;

            PreCalculate();

            _timerId = new DispatcherTimer();
            _timerId.Interval = new TimeSpan(0, 0, 0, 0, _delay);

            _timerId.Tick += (s, a) =>
            {
                if (!_isPaused)
                {
                    double progress = (double)(_frameIdx * _delay) / (double)_duration.Value;

                    if (progress > 1)
                    {
                        progress = 1;
                    }

                    if (intervalCallback != null)
                    {
                        intervalCallback(_intervalLocs[_frameIdx], _intervalIdx[_frameIdx], _frameIdx);
                    }

                    if (progress == 1)
                    {
                        _timerId.Stop();
                    }

                    _frameIdx++;
                }
            };
        }

        #endregion

        #region Public Delgates

#if WINDOWS_PHONE
        public delegate void IntervalCallback(GeoCoordinate loc, int pathIdx, int frameIdx);
#elif WINDOWS_PHONE_APP
        public delegate void IntervalCallback(BasicGeoposition loc, int pathIdx, int frameIdx);
#else
        public delegate void IntervalCallback(Location loc, int pathIdx, int frameIdx);
#endif

        #endregion

        #region Public Properties

        /// <summary>
        /// The coordinates that make up the path of the animation.
        /// </summary>
#if WINDOWS_PHONE
        public GeoCoordinateCollection Path
#elif WINDOWS_PHONE_APP
        public List<BasicGeoposition> Path
#else
        public LocationCollection Path
#endif
        {
            get { return _path; }
            set
            {
                _path = value;
                PreCalculate();
            }
        }

        /// <summary>
        /// A boolean indicating if the path should be geodesic (follw curvature of earth).
        /// </summary>
        public bool IsGeodesic
        {
            get { return _isGeodesic; }
            set
            {
                _isGeodesic = value;
                PreCalculate();
            }
        }

        /// <summary>
        /// The length of time the animation should take in ms.
        /// </summary>
        public int? Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                PreCalculate();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the animation.
        /// </summary>
        public void Play()
        {
            _frameIdx = 0;
            _isPaused = false;
            _timerId.Start();
        }

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        public void Pause()
        {
            _isPaused = true;
        }

        /// <summary>
        /// Stops the animation and resets animation back to first frame.
        /// </summary>
        public void Stop()
        {
            if (_timerId.IsEnabled)
            {
                _frameIdx = 0;
                _isPaused = false;
                _timerId.Stop();
            }
        }

        #endregion

        #region Private Methods

        private void PreCalculate()
        {
            //Stop the timer
            if (_timerId != null && _timerId.IsEnabled)
            {
                _timerId.Stop();
            }

            _duration = (_duration.HasValue && _duration.Value > 0) ? _duration : 150;

#if WINDOWS_PHONE
            _intervalLocs = new GeoCoordinateCollection();
#elif WINDOWS_PHONE_APP
            _intervalLocs = new List<BasicGeoposition>();
#else
            _intervalLocs = new LocationCollection();
#endif
            _intervalIdx = new List<int>();

            _intervalLocs.Add(_path[0]);
            _intervalIdx.Add(0);

            double dlat, dlon;
            double totalDistance = 0;

            if (_isGeodesic)
            {
                //Calcualte the total distance along the path in KM's.
                for (var i = 0; i < _path.Count - 1; i++)
                {
                    totalDistance += SpatialTools.HaversineDistance(_path[i].ToGeometry(), _path[i + 1].ToGeometry(), DistanceUnits.KM);
                }
            }
            else
            {
                //Calcualte the total distance along the path in degrees.
                for (var i = 0; i < _path.Count - 1; i++)
                {
                    dlat = _path[i + 1].Latitude - _path[i].Latitude;
                    dlon = _path[i + 1].Longitude - _path[i].Longitude;

                    totalDistance += Math.Sqrt(dlat * dlat + dlon * dlon);
                }
            }

            int frameCount = (int)Math.Ceiling((double)_duration.Value / (double)_delay);
            int idx = 0;
            double progress;

            //Pre-calculate step points for smoother rendering.
            for (var f = 0; f < frameCount; f++)
            {
                progress = (double)(f * _delay) / (double)_duration.Value;

                double travel = progress * totalDistance;
                double alpha = 0;
                double dist = 0;
                double dx = travel;

                for (var i = 0; i < _path.Count - 1; i++)
                {

                    if (_isGeodesic)
                    {
                        dist += SpatialTools.HaversineDistance(_path[i].ToGeometry(), _path[i + 1].ToGeometry(), DistanceUnits.KM);
                    }
                    else
                    {
                        dlat = _path[i + 1].Latitude - _path[i].Latitude;
                        dlon = _path[i + 1].Longitude - _path[i].Longitude;
                        alpha = Math.Atan2(dlat * Math.PI / 180, dlon * Math.PI / 180);
                        dist += Math.Sqrt(dlat * dlat + dlon * dlon);
                    }

                    if (dist >= travel)
                    {
                        idx = i;
                        break;
                    }

                    dx = travel - dist;
                }

                if (dx != 0 && idx < _path.Count - 1)
                {
                    if (_isGeodesic)
                    {
                        var bearing = SpatialTools.CalculateHeading(_path[idx].ToGeometry(), _path[idx + 1].ToGeometry());
                        _intervalLocs.Add(SpatialTools.CalculateDestinationCoordinate(_path[idx].ToGeometry(), bearing, dx, DistanceUnits.KM).ToBMGeometry());
                    }
                    else
                    {
                        dlat = dx * Math.Sin(alpha);
                        dlon = dx * Math.Cos(alpha);

#if WINDOWS_PHONE
                        _intervalLocs.Add(new GeoCoordinate(_path[idx].Latitude + dlat, _path[idx].Longitude + dlon));
#elif WINDOWS_PHONE_APP
                        _intervalLocs.Add(new BasicGeoposition(){
                            Latitude = _path[idx].Latitude + dlat, 
                            Longitude = _path[idx].Longitude + dlon
                        });
#else
                        _intervalLocs.Add(new Location(_path[idx].Latitude + dlat, _path[idx].Longitude + dlon));
#endif
                    }

                    _intervalIdx.Add(idx);
                }
            }

            //Ensure the last location is the last coordinate in the path.
            _intervalLocs.Add(_path[_path.Count - 1]);
            _intervalIdx.Add(_path.Count - 1);
        }

        #endregion
    }
}