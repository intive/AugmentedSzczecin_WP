namespace Microsoft.Maps.SpatialToolbox.IO
{
    /// <summary>
    /// This enumerator represents the different types of shapes that can be in a Shapefiles
    /// </summary>
    internal enum WKBShapeType : uint
    {
        WkbGeometryCollection = 7,
        WkbGeometryCollectionM = 0x7d7,
        WkbGeometryCollectionZ = 0x3ef,
        WkbGeometryCollectionZM = 0xbbf,
        WkbLineString = 2,
        WkbLineStringM = 0x7d2,
        WkbLineStringZ = 0x3ea,
        WkbLineStringZM = 0xbba,
        WkbMultiLineString = 5,
        WkbMultiLineStringM = 0x7d5,
        WkbMultiLineStringZ = 0x3ed,
        WkbMultiLineStringZM = 0xbbd,
        WkbMultiPoint = 4,
        WkbMultiPointM = 0x7d4,
        WkbMultiPointZ = 0x3ec,
        WkbMultiPointZM = 0xbbc,
        WkbMultiPolygon = 6,
        WkbMultiPolygonM = 0x7d6,
        WkbMultiPolygonZ = 0x3ee,
        WkbMultiPolygonZM = 0xbbe,
        WkbPoint = 1,
        WkbPointM = 0x7d1,
        WkbPointZ = 0x3e9,
        WkbPointZM = 0xbb9,
        WkbPolygon = 3,
        WkbPolygonM = 0x7d3,
        WkbPolygonZ = 0x3eb,
        WkbPolygonZM = 0xbbb,
        WkbPolyhedralSurface = 15,
        WkbPolyhedralSurfaceM = 0x7df,
        WkbPolyhedralSurfaceZ = 0x3f7,
        WkbPolyhedralSurfaceZM = 0xbc7,
        WkbTIN = 0x10,
        WkbTINM = 0x7e0,
        WkbTINZ = 0x3f8,
        WkbTinZM = 0xbc8,
        WkbTriangle = 0x11,
        WkbTriangleM = 0x7e1,
        WkbTrianglez = 0x3f9,
        WkbTriangleZM = 0xbc9
    }
}