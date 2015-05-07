using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Microsoft.Maps.SpatialToolbox.Imaging
{
    public class HeatMapRenderEngine
    {
        #region Private Properties

        private const int _numberOfRadiis = 24;

        private bool cancel;

        private Task<System.IO.MemoryStream> renderTask;

        #endregion

        #region Constructor

        public HeatMapRenderEngine()
        {
        }

        #endregion

        #region Public Methods

        public Task<System.IO.MemoryStream> RenderData(List<PointF> data, HeatMapStyle style, int radiusPx, int width, int height)
        {
            if (renderTask != null && renderTask.Status == TaskStatus.Running)
            {
                cancel = true;
                renderTask.Wait();
                cancel = false;
            }

            renderTask = Task.Run<System.IO.MemoryStream>(() =>
            {
                try
                {
                    if (style.ColorMapCollection != null)
                    {
                        Bitmap bmp = new Bitmap(width, height);

                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.Clear(Color.Transparent);
                            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                            g.Clip = new Region(new Rectangle(0, 0, width, height));
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

                            if (data != null)
                            {
                                var s = DateTime.Now;

                                var rect = new RectangleF(-radiusPx, -radiusPx, width + radiusPx, height + radiusPx);

                                CreateHeatMask(data, style, radiusPx, rect, g);
                                ColorizeHeatMap(bmp, style);

                                if (cancel)
                                {
                                    g.Clear(Color.Transparent);
                                }

                                var e = DateTime.Now;
                                Console.WriteLine("Render Heat Map - {0}ms", (e - s).TotalMilliseconds); 
                            }
                        }

                        var ms = new System.IO.MemoryStream();
                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                        return ms;
                    }
                }
                catch (Exception ex)
                {
                    var t = "";
                }

                return null;
            });

            return renderTask;
        }

        #endregion

        #region Private Methods

        private void CreateHeatMask(List<PointF> data, HeatMapStyle style, int radiusPx, RectangleF rect, Graphics g)
        {
            //Calculate angle between radiis
            float circleResolution = 360 / _numberOfRadiis;
            float[] radianXRadiis = new float[_numberOfRadiis];
            float[] radianYRadiis = new float[_numberOfRadiis];

            for (int i = 0; i < _numberOfRadiis; i++)
            {
                radianXRadiis[i] = (float)(radiusPx * Math.Cos((i * circleResolution) * Math.PI / 180));
                radianYRadiis[i] = (float)(radiusPx * Math.Sin((i * circleResolution) * Math.PI / 180));
            }
 
            // Create an empty array that will be populated with points from the generic list
            var circumferencePointsArray = new System.Drawing.PointF[_numberOfRadiis];

            // Store scaled and flipped intensity value for use with gradient center location
            //0.9 is to make it feather out at the edges all the time.
            float fIntensity = (float)Math.Min((1 - style.Intensity / Byte.MaxValue) * 1F, 0.9);

            // Create new color blend to tell the PathGradientBrush what colors to use and where to put them
            var gradientSpecifications = new ColorBlend(3);

            // Define positions of gradient colors, use intesity to adjust the middle color to
            // show more mask or less mask
            gradientSpecifications.Positions = new float[3] { 0, fIntensity, 1 };

            // Define gradient colors and their alpha values, adjust alpha of gradient colors to match intensity
            gradientSpecifications.Colors =  new Color[3]
            {
                Color.FromArgb(0, Color.White),
                Color.FromArgb((byte)(style.Intensity * 255), Color.Black),
                Color.FromArgb((byte)(style.Intensity * 255), Color.Black)
            };

            foreach (var p in data)
            {
                if (cancel)
                {
                    break;
                }

                if (rect.Contains(p))
                {
                    // Loop through all angles of a circle
                    // Define loop variable as a double to prevent casting in each iteration
                    // Iterate through loop on 15 degree deltas, this can be changed to improve performance
                    for (int i = 0; i < _numberOfRadiis; i++)
                    {
                        // Plot new point on the circumference of a circle of the defined radius
                        // Using the point coordinates, radius, and angle
                        // Calculate the position of this iterations point on the circle
                        // Add newly plotted circumference point to generic point list
                        circumferencePointsArray[i] = new System.Drawing.PointF(p.X + radianXRadiis[i], p.Y + radianYRadiis[i]);
                    }

                    // Create new PathGradientBrush to create a radial gradient using the circumference points
                    var gradientShaper = new PathGradientBrush(circumferencePointsArray);

                    // Pass off color blend to PathGradientBrush to instruct it how to generate the gradient
                    gradientShaper.InterpolationColors = gradientSpecifications;

                    // Draw polygon (circle) using our point array and gradient brush
                    g.FillPolygon(gradientShaper, circumferencePointsArray);
                }
            }
        }

        private void ColorizeHeatMap(Bitmap mask, HeatMapStyle style)
        {
            // Create new bitmap to act as a work surface for the colorization process
            Bitmap coloredImage = new Bitmap(mask.Width, mask.Height, mask.PixelFormat);

            // Create a graphics object from our memory bitmap so we can draw on it and clear it's drawing surface
            using (var gr = Graphics.FromImage(coloredImage))
            {
                gr.Clear(Color.Transparent);

                // Create new image attributes class to handle the color remappings
                // Inject our color map array to instruct the image attributes class how to do the colorization
                ImageAttributes remapper = new ImageAttributes();
                remapper.SetRemapTable(style.ColorMapCollection);

                // Draw our mask onto our memory bitmap work surface using the new color mapping scheme
                gr.DrawImage(mask, new Rectangle(0, 0, mask.Width, mask.Height), 0, 0, mask.Width, mask.Height, GraphicsUnit.Pixel, remapper);
            }

            using (var gr = Graphics.FromImage(mask))
            {
                gr.Clear(Color.Transparent);
                gr.DrawImage(coloredImage, new PointF(0, 0));
            }
        }

        #endregion
    }
}
