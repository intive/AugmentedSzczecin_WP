namespace Microsoft.Maps.SpatialToolbox.IO
{
    /// <summary>
    /// This enumerator represents the two types of byte orders that binary data can have; Big Endian (XDR) and Little Endian (NDR).
    /// 
    /// The XDR representation of an Unsigned Integer is Big Endian (most significant byte first).
    /// The XDR representation of a Double is Big Endian (sign bit is first byte).
    /// 
    /// The NDR representation of an Unsigned Integer is Little Endian (least significant byte first).
    /// The NDR representation of a Double is Little Endian (sign bit is last byte).
    /// </summary>
    internal enum ByteOrder : byte
    {
        /// <summary>
        /// XDR (Big Endian) Encoding of Numeric Types
        /// http://publib.boulder.ibm.com/infocenter/idshelp/v10/index.jsp?topic=/com.ibm.spatial.doc/spat274.htm
        /// </summary>
        Xdr = 0,

        /// <summary>
        /// NDR (Little Endian) Encoding of Numeric Types
        /// http://publib.boulder.ibm.com/infocenter/idshelp/v10/index.jsp?topic=/com.ibm.spatial.doc/spat274.htm
        /// </summary>
        Ndr = 1
    }
}