using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maps.SpatialToolbox
{
    public static class Extensions
    {
        /// <summary>
        /// Calculates the Envelope (BoundingBox) of a list of geometries.
        /// </summary>
        /// <param name="geometries">List of geometries to calculate envelope for.</param>
        /// <returns>Envolpe the contains all geometries.</returns>
        public static BoundingBox Envelope(this List<Geometry> geometries)
        {
            BoundingBox bounds = null;
            foreach (var g in geometries)
            {
                var b = g.Envelope();

                if (bounds == null)
                {
                    bounds = b;
                }
                else
                {
                    bounds = bounds.Join(b);
                }
            }

            return bounds;
        }
    }
}
