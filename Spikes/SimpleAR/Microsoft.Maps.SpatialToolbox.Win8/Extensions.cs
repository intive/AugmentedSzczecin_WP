#if WINDOWS_APP || WINDOWS_PHONE_APP
using Windows.UI;
#elif WINDOWS_PHONE
using System.Windows.Media;
#elif WPF
using Microsoft.Maps.SpatialToolbox.IO;
using Microsoft.SqlServer.Types;
using System.Data.SqlTypes;
using System.Threading.Tasks;
using System.Windows.Media;
#endif

namespace Microsoft.Maps.SpatialToolbox
{
    public static class Extensions
    {
        public static Color ToColor(this StyleColor sColor)
        {
            return Color.FromArgb(sColor.A, sColor.R, sColor.G, sColor.B);
        }

        public static StyleColor ToStyleColor(this Color color)
        {
            return StyleColor.FromArgb(color.A, color.R, color.G, color.B);
        }

#if WPF
        public static System.Drawing.Color ToDrawingColor(this StyleColor sColor)
        {
            if (sColor.A == 0)
            {
                return System.Drawing.Color.Transparent;
            }

            return System.Drawing.Color.FromArgb(sColor.A, sColor.R, sColor.G, sColor.B);
        }

        public static System.Drawing.Pen ToDrawingPen(this StyleColor sColor, float width)
        {
            var pen = new System.Drawing.Pen(sColor.ToDrawingBrush(), width);
            
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;

            return pen;
        }

        public static System.Drawing.Brush ToDrawingBrush(this StyleColor sColor)
        {
            return new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(sColor.A, sColor.R, sColor.G, sColor.B));
        }

        #region SQL Geometry Extensions

        public static SqlGeometry ToSqlGeometry(this Geometry geometry)
        {
            if (geometry == null)
            {
                return null;
            }

            return SqlGeometry.STGeomFromWKB(new SqlBytes(geometry.STAsBinary()), 4326);
        }

        public static Task<Geometry> ToGeometry(this SqlGeometry geometry)
        {
            return Task.Run<Geometry>(async () =>
            {
                if (geometry == null)
                {
                    return null;
                }

                var reader = new WellKnownBinary();
                return await reader.Read(geometry.STAsBinary().Value);
            });
        }

        #endregion

        #region SQL Geography Extensions

        public static SqlGeography ToSqlGeography(this Geometry geometry)
        {
            var geom = geometry.ToSqlGeometry();

            if (geom != null)
            {
                geom = geom.MakeValid();

                return SqlGeography.STGeomFromWKB(geom.STAsBinary(), geom.STSrid.Value);
            }

            return null;
        }

        public static Task<Geometry> ToGeometry(this SqlGeography geography)
        {
            return Task.Run<Geometry>(async () =>
            {
                if (geography == null)
                {
                    return null;
                }

                var reader = new WellKnownBinary();
                return await reader.Read(geography.STAsBinary().Value);
            });
        }

        #endregion

#endif
    }
}
