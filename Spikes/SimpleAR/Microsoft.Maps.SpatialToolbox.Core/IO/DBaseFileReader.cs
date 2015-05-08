/**************************************************************************** 
 * Description:
 * This class reads dBase files as a Binary stream. This is done to avoid using 
 * System.Data which is not available in some frameworks such as WinRT & Silverlight.
 * 
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Microsoft.Maps.SpatialToolbox.IO
{
    /// <summary>
    /// This class reads dBase files as a Binary stream. 
    /// </summary>
    public static class DBaseFileReader
    {
        #region Private Constant Properties

        //The constant size of a row
        private const int fileDescriptionSize = 32;

        //The required type of the file
        private const int requiredFileType = 0x03;

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads the contents of a local dbf file.
        /// </summary>
        /// <param name="stream">The stream fo the dbf file</param>
        /// <param name="colRules">List of column indices to skip. Settign to null causes all columns to be read</param>
        /// <returns>A DBaseFile object containing the data from the DBF file</returns>
        public static DBaseFile Read(Stream stream, bool[] colRules)
        {
            //Create binary reader out of file stream
            using (BinaryReader reader = new BinaryReader(stream))
            {
                return Read(reader, int.MaxValue, colRules);
            }
        }

        /// <summary>
        /// Reads the contents of a local dbf file.
        /// </summary>
        /// <param name="reader">The binary read of the dbf file</param>
        /// <param name="colRules">List of column indices to skip. Settign to null causes all columns to be read</param>
        /// <returns>A DBaseFile object containing the data from the DBF file</returns>
        public static DBaseFile Read(BinaryReader reader, bool[] colRules)
        {
            return Read(reader, int.MaxValue, colRules);
        }

        /// <summary>
        /// Reads the contents of a local dbf file.
        /// </summary>
        /// <param name="stream">The stream fo the dbf file</param>
        /// <param name="numberOfRows">Number of rows to load from file.</param>
        /// <param name="colRules">List of column indices to skip. Settign to null causes all columns to be read</param>
        /// <returns>A DBaseFile object containing the data from the DBF file</returns>
        public static DBaseFile Read(Stream stream, int numberOfRows, bool[] colRules)
        {
             //Create binary reader out of file stream
            using (BinaryReader reader = new BinaryReader(stream))
            {
                return Read(reader, numberOfRows, colRules);
            }
        }

        /// <summary>
        /// Reads the contents of a local dbf file.
        /// </summary>
        /// <param name="reader">The binary read of the dbf file</param>
        /// <param name="numberOfRows">Number of rows to load from file.</param>
        /// <param name="colRules">List of column indices to skip. Settign to null causes all columns to be read</param>
        /// <returns>A DBaseFile object containing the data from the DBF file</returns>
        public static DBaseFile Read(BinaryReader reader, int numberOfRows, bool[] colRules)
        {
            DBaseFile file = new DBaseFile();

            //Read the header
            file.Header = ReadHeader(reader);
            numberOfRows = Math.Min(numberOfRows, file.Header.NumRows);

            int numValidRulesCols = 0;

            if (colRules != null)
            {
                foreach (bool rule in colRules)
                {
                    if (rule)
                    {
                        numValidRulesCols++;
                    }
                }
            }
            else
            {
                numValidRulesCols = file.Header.NumColumns;
            }

            //Read the rows of the DBF file
            for (int i = 0; i < numberOfRows; i++)
            {
                file.Rows.Add(ReadRow(file.Header, reader, colRules, numValidRulesCols));
            }

            return file;
        }

        /// <summary>
        /// Reads the header of a DBF file
        /// </summary>
        /// <param name="stream">A stream of the dbf file</param>
        /// <returns>A DBFFileHeader object</returns>
        public static DBaseFileHeader ReadHeader(Stream stream)
        {
            //Create binary reader out of file stream
            using (BinaryReader reader = new BinaryReader(stream))
            {
                //Read the header
                return ReadHeader(reader);
            }
        }

        /// <summary>
        /// Reads the header data from a DBF file using a BinarayReader
        /// </summary>
        /// <param name="reader">BinaryReader containing the DBF file</param>
        /// <returns>A DBaseFileHeader object with the header information of the DBF file</returns>
        public static DBaseFileHeader ReadHeader(BinaryReader reader)
        {
            //initialize the dBase Header object
            DBaseFileHeader header = new DBaseFileHeader();

            //The first byte in the reader should be the file type. Verify that this is correct. 
            if (reader.ReadByte() == requiredFileType)
            {
                //Read the date parameters
                int year = (int)reader.ReadByte() + 1900;
                int month = (int)reader.ReadByte();
                int day = (int)reader.ReadByte();
                header.LastUpdateDate = new DateTime(year, month, day);

                //Read the number of records.
                header.NumRows = reader.ReadInt32();

                //Read the length of the header structure.
                header.HeaderLength = reader.ReadInt16();

                //Read the length of a record
                header.RowLength = reader.ReadInt16();

                //Skip the reserved bytes in the header.
                reader.BaseStream.Seek(20, SeekOrigin.Current);

                //Calculate the number of columns in the header
                header.NumColumns = (header.HeaderLength - fileDescriptionSize - 1) / fileDescriptionSize;

                //Inialize the ColumDescriptions List
                header.ColumnDescriptions = new List<DBaseColumnDefinition>(header.NumColumns);

                for (int i = 0; i < header.NumColumns; i++)
                {
                    DBaseColumnDefinition columnDescription = new DBaseColumnDefinition();

                    //Read the cloumn name				
                    char[] buffer = new char[11];
                    buffer = reader.ReadChars(11);
                    columnDescription.Name = new string(buffer).Replace("\0", String.Empty);

                    //Read the column dBase data type
                    columnDescription.DBaseType = (char)reader.ReadByte();

                    //Skip the column data address
                    reader.ReadInt32();

                    //Read the column length in bytes
                    int columnLength = (int)reader.ReadByte();

                    //Make sure the length is positive
                    if (columnLength < 0)
                    {
                        columnLength += 256;
                    }

                    columnDescription.Length = columnLength;

                    //Skip the column decimal count property and the reserved bytes.
                    reader.BaseStream.Seek(15, SeekOrigin.Current);

                    //Add the column description to the list of descriptions
                    header.ColumnDescriptions.Add(columnDescription);
                }

                //The last byte is a marker for the end of the field definitions.
                reader.BaseStream.Seek(1, SeekOrigin.Current);
            }

            return header;
        }

        /// <summary>
        /// Verifies that the specified file name is valid by checking that it has the proper dbf extension.
        /// </summary>
        /// <param name="fileName">A dbf file name</param>
        /// <returns>A boolean indicating if the file name is valid</returns>
        public static bool IsValidFile(string fileName)
        {
            return (!string.IsNullOrEmpty(fileName) && fileName.ToLower().EndsWith(".dbf"));
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Read a single row from the dBase file
        /// </summary>
        /// <param name="header">DBase File header</param>
        /// <param name="reader">BinaryReader that stream is being read from</param>
        /// <param name="colRules">List of column indices to skip</param>
        /// <param name="numValidRulesCols">Number of columns that are marked true in the column rules array</param>
        /// <returns>A Dictionary of objects</returns>
        internal static Dictionary<string, object> ReadRow(DBaseFileHeader header, BinaryReader reader, bool[] colRules, int numValidRulesCols)
        {
            Dictionary<string, object> row = null;

            bool foundRow = false;

            //loop through until there are no more valid rows
            while (!foundRow)
            {
                //Initialize DBaseRow
                row = new Dictionary<string, object>();

                //Read the deleted flag
                char deletedFlag = (char)reader.ReadChar();

                //Read the row length
                int rowLength = 1; // for the deleted character just read.

                char[] buffer;  //a reusable character array used for holding characters
                string tempString;  //a resuable variable for string a string
                object cellObject;

                //Read the rows
                for (int j = 0; j < header.NumColumns; j++)
                {
                    //Get the cell length
                    int cellLength = header.ColumnDescriptions[j].Length;
                    rowLength = rowLength + cellLength;

                    // read the data.
                    cellObject = null;

                    if (colRules == null || colRules[j])
                    {
                        //Switch on the DBaseType of the cell
                        switch (header.ColumnDescriptions[j].DBaseType)
                        {
                            case 'L':   //boolean
                                char tempChar = (char)reader.ReadByte();

                                // Check DBase boolean characters (T,t,F,f,Y,y,N,n)
                                if (char.ToUpper(tempChar) == 'T' || char.ToUpper(tempChar) == 'Y')
                                {
                                    cellObject = true;
                                }
                                else
                                {
                                    cellObject = false;
                                }
                                break;

                            case 'C':   //String or character
                                buffer = new char[cellLength];
                                buffer = reader.ReadChars(cellLength);

                                //Trim whitespaces and remove any \0 values
                                cellObject = new string(buffer).Trim().Replace("\0", String.Empty);
                                break;

                            case 'D':   //Date YYYYMMDD
                                buffer = new char[8];
                                buffer = reader.ReadChars(8);

                                //Parse the date parameters out of the character array
                                int year = Int32.Parse(new string(buffer, 0, 4), CultureInfo.InvariantCulture);
                                int month = Int32.Parse(new string(buffer, 4, 2), CultureInfo.InvariantCulture);
                                int day = Int32.Parse(new string(buffer, 6, 2), CultureInfo.InvariantCulture);

                                //create a DateTime object
                                cellObject = new DateTime(year, month, day);
                                break;

                            case 'I':   //Integer
                                buffer = new char[cellLength];
                                buffer = reader.ReadChars(cellLength);
                                tempString = new string(buffer).Trim();

                                int intNumber;

                                //Try and parse the integer
                                if (int.TryParse(tempString, NumberStyles.Integer, CultureInfo.InvariantCulture, out intNumber))
                                {
                                    cellObject = intNumber;
                                }
                                break;

                            case 'N': //Double
                                buffer = new char[cellLength];
                                buffer = reader.ReadChars(cellLength);
                                tempString = new string(buffer).Trim();

                                double doubleNumber;

                                //Try and parse the double
                                if (double.TryParse(tempString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out doubleNumber))
                                {
                                    cellObject = doubleNumber;
                                }
                                break;
                            case 'F': //float
                                buffer = new char[cellLength];
                                buffer = reader.ReadChars(cellLength);
                                tempString = new string(buffer).Trim();

                                float floatNumber;

                                //Try and parse the float
                                if (float.TryParse(tempString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out floatNumber))
                                {
                                    cellObject = floatNumber;
                                }
                                break;
                            default:
                                break;
                        }

                        //Add the cell object to the row
                        row.Add(header.ColumnDescriptions[j].Name, cellObject);
                    }
                    else
                    {
                        reader.BaseStream.Seek(cellLength, SeekOrigin.Current);
                    }
                }

                //Check to see if the full row has been read.
                if (rowLength < header.RowLength)
                {
                    //Skip to the end of the row
                    reader.BaseStream.Seek(header.RowLength - rowLength, SeekOrigin.Current);
                }

                //Add the row if it is not deleted.
                if (deletedFlag != '*')
                {
                    foundRow = true;
                }
            }

            return row;
        }
		
		#endregion	
	}
}
