using System;

namespace Microsoft.Maps.SpatialToolbox
{
    /// <summary>
    /// A class that holds the properties of a dBase column
    /// 
    /// dBASE Data Types: http://www.dbase.com/knowledgebase/int/db7_file_fmt.htm
    /// </summary>
    public class DBaseColumnDefinition
    {
        /// <summary>
        /// Column Name
        /// </summary>
        public string Name { get; set;}

        /// <summary>
        /// Column dBase Data Type (L, C, D, I, N, F).
        /// </summary>
        public char DBaseType { get; set; }

        /// <summary>
        /// Length of the data in bytes
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Returns the .NET equivalent type for the DBaseType
        /// </summary>
        public Type Type
        {
            get
            {
                Type type;
                switch (DBaseType)
                {
                    case 'L': //boolean
                        type = typeof(bool);
                        break;
                    case 'C': //String or character
                        type = typeof(string);
                        break;
                    case 'D': //Date
                        type = typeof(DateTime);
                        break;
                    case 'I': //Integer
                        type = typeof(int);
                        break;
                    case 'N': //Double
                        type = typeof(double);
                        break;
                    case 'F': //Float
                        type = typeof(float);
                        break;
                    default:
                        throw new NotSupportedException("Unknown DBaseType {0}" + DBaseType);
                }

                return type;
            }
        }
    }
}
