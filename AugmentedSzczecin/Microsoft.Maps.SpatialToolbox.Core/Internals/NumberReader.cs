using System;
using System.IO;

namespace Microsoft.Maps.SpatialToolbox.IO
{
    /// <summary>
    /// This class contains a set of methods for reading numbers from a BinaryReader 
    /// </summary>
    internal static class NumberReader
    {
        #region Common Number Readers

        public static int ReadIntLE(BinaryReader reader)
        {
            return ReadInt32(reader, ByteOrder.Ndr);
        }

        public static int ReadIntBE(BinaryReader reader)
        {
            return ReadInt32(reader, ByteOrder.Xdr);
        }

        public static double ReadDoubleLE(BinaryReader reader)
        {
            return ReadDouble(reader, ByteOrder.Ndr);
        }

        public static double ReadDoubleBE(BinaryReader reader)
        {
            return ReadDouble(reader, ByteOrder.Xdr);
        }

        public static int ReadInt32(BinaryReader reader, ByteOrder wkbByteOrder)
        {
            if (wkbByteOrder == ByteOrder.Xdr)
            {
                byte[] bytes = BitConverter.GetBytes(reader.ReadInt32());
                Array.Reverse(bytes);
                return BitConverter.ToInt32(bytes, 0);
            }
            else //wkbByteOrder == ByteOrder.Ndr
            {
                return reader.ReadInt32();
            }
        }

        public static uint ReadUInt32(BinaryReader reader, ByteOrder wkbByteOrder)
        {
            if (wkbByteOrder == ByteOrder.Xdr)
            {
                byte[] bytes = BitConverter.GetBytes(reader.ReadUInt32());
                Array.Reverse(bytes);
                return BitConverter.ToUInt32(bytes, 0);
            }
            else //wkbByteOrder == ByteOrder.Ndr
            {
                return reader.ReadUInt32();
            }
        }

        public static double ReadDouble(BinaryReader reader, ByteOrder wkbByteOrder)
        {
            if (wkbByteOrder == ByteOrder.Xdr)
            {
                byte[] bytes = BitConverter.GetBytes(reader.ReadDouble());
                Array.Reverse(bytes);
                return BitConverter.ToDouble(bytes, 0);
            }
            else //wkbByteOrder == ByteOrder.Ndr
            {
                return reader.ReadDouble();
            }
        }

        #endregion
    }
}