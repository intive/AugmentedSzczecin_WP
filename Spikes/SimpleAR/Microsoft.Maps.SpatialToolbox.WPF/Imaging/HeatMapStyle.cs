using System.ComponentModel;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Maps.SpatialToolbox.Imaging
{
    public class HeatMapStyle : INotifyPropertyChanged
    {
        #region Private Properties

        internal ColorMap[] ColorMapCollection = new ColorMap[256];
        private GradientStopCollection _heatGradient;

        #endregion

        #region Constructor

        public HeatMapStyle()
        {
            this.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case "HeatGradient":
                        SetColorMap();
                        _heatGradient.Changed += (sender, args)=>{
                             SetColorMap();
                        };
                        break;
                }
            };
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Insenity of the heat map. A value between 0 and 1.
        /// </summary>
        public double Intensity { get; set; }

        /// <summary>
        /// Gives all values the same opacity to create a hard edge on each data point. 
        /// When set to false (default) the data points will use a fading opacity towards the edges.
        /// </summary>
        public bool EnableHardEdge { get; set; }

        ///<summary>
        ///Heat gradient to use to render heat map.
        ///</summary>
        public GradientStopCollection HeatGradient
        {
            get { return _heatGradient; }
            set
            {
                _heatGradient = value;
                OnPropertyChanged("HeatGradient");
            }
        }

        #endregion

        #region Private Methods

        private void SetColorMap()
        {
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                context.DrawRectangle(new LinearGradientBrush(_heatGradient), null, new Rect(0, 0, 256, 1));
            }

            var renderTarget = new RenderTargetBitmap(256, 1, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(visual);

            if (EnableHardEdge)
            {
                byte alpha = (byte)(255 * Intensity);

                // Loop through each pixel and create a new color mapping
                for (int x = 0; x <= 255; x++)
                {
                    var croppedBitmap = new CroppedBitmap(renderTarget, new Int32Rect(x, 0, 1, 1));
                    var pixels = new byte[4];
                    croppedBitmap.CopyPixels(pixels, 4, 0);

                    if (pixels[3] < alpha)
                    {
                        ColorMapCollection[x] = new System.Drawing.Imaging.ColorMap()
                        {
                            OldColor = System.Drawing.Color.FromArgb(x, 0, 0, 0),
                            NewColor = System.Drawing.Color.FromArgb(alpha, pixels[2], pixels[1], pixels[0])
                        };
                    }
                    else
                    {
                         ColorMapCollection[x] = new System.Drawing.Imaging.ColorMap(){
                             OldColor = System.Drawing.Color.FromArgb(x, 0, 0, 0),
                             NewColor = System.Drawing.Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0])
                         };
                    }
                }
            }
            else
            {
                // Loop through each pixel and create a new color mapping
                for (int x = 0; x <= 255; x++)
                {
                    var croppedBitmap = new CroppedBitmap(renderTarget, new Int32Rect(x, 0, 1, 1));
                    var pixels = new byte[4];
                    croppedBitmap.CopyPixels(pixels, 4, 0);
                    ColorMapCollection[x] = new System.Drawing.Imaging.ColorMap()
                    {
                        OldColor = System.Drawing.Color.FromArgb(x, 0, 0, 0),
                        NewColor = System.Drawing.Color.FromArgb(x, pixels[2], pixels[1], pixels[0])
                    };
                }
            }

            ColorMapCollection[0] = new System.Drawing.Imaging.ColorMap()
            {
                OldColor = System.Drawing.Color.FromArgb(0, 0, 0, 0),
                NewColor = System.Drawing.Color.Transparent
            };
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
