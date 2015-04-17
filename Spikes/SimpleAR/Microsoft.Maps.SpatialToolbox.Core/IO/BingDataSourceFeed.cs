using Microsoft.Maps.SpatialToolbox.Internals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.Maps.SpatialToolbox.IO
{
    public class BingDataSourceFeed : BaseFeed
    {
        #region Private Properties

        private string _dataSourceName = "EntityDataSet";
        private string _entityTypeName = "Entity";
        private string _geometryColumnName = "Geom";
        private string _primaryKeyName = "EntityID";

        private const string _schemaText = "Bing Spatial Data Services, 1.0, {0}";
        private const string _columnHeaderPattern = @"^([a-zA-Z0-9_]+)\(([a-zA-Z0-9\.,]+)\)$";

        private const int _maxGeomPoints = 2000;
        private const int _maxStringLength = 2560;

        private Regex nameRegex = new Regex("^[a-zA-Z0-9~`!$^_={}]+$");
        private Regex propertyNameRegex = new Regex("^([a-zA-Z_][a-zA-Z0-9][a-zA-Z0-9_]*|[a-zA-Z][a-zA-Z0-9_]*)$");

        private const string XmlnsNamespace = "http://www.w3.org/2000/xmlns/";
        private const string XmlSchemaNamespace = "http://www.w3.org/2001/XMLSchema";
        private const string MsDataNamespace = "urn:schemas-microsoft-com:xml-msdata";

        private XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
        {
            Encoding = System.Text.Encoding.UTF8,
            Indent = true,
            NamespaceHandling = NamespaceHandling.OmitDuplicates
        };

        private BingDataSourceType _dataSourceType = BingDataSourceType.XML;

        #endregion

        #region Public Constant Properties

        public const string DataSourceNameKey = "DataSourceName";

        public const string EntityTypeNameKey = "EntityTypeName";

        public const string PrimaryKeyNameKey = "PrimaryKeyName";

        public const string GeometryColumnNameKey = "GeometryColumnName";

        #endregion

        #region Constructor

        public BingDataSourceFeed(BingDataSourceType type)
            : base()
        {
            _dataSourceType = type;
        }

        public BingDataSourceFeed(BingDataSourceType type, string name, string entityTypeName)
            : base()
        {
            _dataSourceType = type;

            if (IsValidName(name))
            {
                _dataSourceName = name;
            }

            if (IsValidName(entityTypeName))
            {
                _entityTypeName = entityTypeName;
            }
        }

        public BingDataSourceFeed(BingDataSourceType type, string name, string entityTypeName, string geomColumnName)
            : base()
        {
            _dataSourceType = type;

            if (IsValidName(name))
            {
                _dataSourceName = name;
            }

            if (IsValidName(entityTypeName))
            {
                _entityTypeName = entityTypeName;
            }

            if (IsValidPropertyName(geomColumnName))
            {
                _geometryColumnName = geomColumnName;
            }
        }

        #endregion

        #region Public Methods

        public override Task<SpatialDataSet> ReadAsync(Stream stream)
        {
            return Task.Run<SpatialDataSet>(async () =>
            {
                if (_dataSourceType == BingDataSourceType.XML)
                {
                    var doc = XDocument.Load(stream);
                    return await ParseDataSource(doc);
                }

                using (var reader = new StreamReader(stream))
                {
                    return await ParseDelimitedDataSource(reader);
                }
            });
        }

        public Task<SpatialDataSet> ReadAsync(string dataSource)
        {
            return Task.Run<SpatialDataSet>(async () =>
            {
                if (_dataSourceType == BingDataSourceType.XML)
                {
                    var doc = XDocument.Parse(dataSource, LoadOptions.SetBaseUri);
                    return await ParseDataSource(doc);
                }
                
                using (var reader = new StringReader(dataSource))
                {
                    return await ParseDelimitedDataSource(reader);
                }
            });
        }

        public Task<string> WriteAsync(SpatialDataSet data)
        {
            return Task.Run<string>(() =>
            {
                if (_dataSourceType == BingDataSourceType.XML)
                {
                    var sb = new StringBuilder();

                    using (var xWriter = XmlWriter.Create(sb, xmlWriterSettings))
                    {
                        Write(data, xWriter);
                    }

                    return sb.ToString();
                }
                else
                {
                    using (var writer = new MemoryStream())
                    {
                        Write(data, writer);

                        using (var reader = new StreamReader(writer))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            });
        }

        public override Task WriteAsync(SpatialDataSet data, Stream stream)
        {
            return Task.Run(() =>
            {
                if (_dataSourceType == BingDataSourceType.XML)
                {
                    using (var writer = XmlWriter.Create(stream, xmlWriterSettings))
                    {
                        Write(data, writer);
                    }
                }
                else
                {
                    Write(data, stream);
                }
            });
        }

        #endregion

        #region Private Methods

        #region Data Source Read Methods

        private async Task<SpatialDataSet> ParseDelimitedDataSource(TextReader textReader)
        {
            char delimiter;

            switch (_dataSourceType)
            {
                case BingDataSourceType.PIPE:
                    delimiter = '|';
                    break;
                case BingDataSourceType.TAB:
                    delimiter = '\t';
                    break;
                case BingDataSourceType.CSV:
                default:
                    delimiter = ',';
                    break;
            }

            var result = new SpatialDataSet();
            var geoms = new List<Geometry>();
            string primaryKeyName = string.Empty;
            var columnInfo = new Dictionary<string,Type>();
            var colNames = new List<string>();

            try
            {
                using (var reader = new DelimitedFileReader(textReader, delimiter))
                {
                    var row = reader.GetNextRow();

                    if (row != null && row.Count > 0)
                    {
                        //Parse Schema Version info.
                        if (row[0].StartsWith("Bing Spatial Data Services", StringComparison.OrdinalIgnoreCase))
                        {
                            string entityName = string.Empty;

                            if (row[0].Contains(","))
                            {
                                var r = row[0].Split(new char[] { ',' });
                                if (r.Length >= 3)
                                {
                                    entityName = r[2].Trim();
                                }
                            }

                            if (row.Count >= 3)
                            {
                                entityName = row[2].Trim();
                            }

                            if (IsValidName(entityName))
                            {
                                result.Metadata.Properties.Add(EntityTypeNameKey, entityName);
                            }

                            row = reader.GetNextRow();
                        }

                        //Parse header row                    
                        columnInfo = ParseColumnHeader(row, out colNames, out primaryKeyName);

                        if (!string.IsNullOrWhiteSpace(primaryKeyName))
                        {
                            result.Metadata.Properties.Add(PrimaryKeyNameKey, primaryKeyName);
                        }

                        if (columnInfo.ContainsValue(typeof(Geometry)))
                        {
                            foreach (var k in columnInfo.Keys)
                            {
                                if (columnInfo[k] == typeof(Geometry))
                                {
                                    result.Metadata.Properties.Add(GeometryColumnNameKey, k);
                                    break;
                                }
                            }
                        }

                        row = reader.GetNextRow();
                    }

                    while (row != null && row.Count > 0)
                    {
                        var g = await ParseGeometry(row, colNames, columnInfo, primaryKeyName);

                        if (g != null)
                        {
                            geoms.Add(g);
                        }

                        row = reader.GetNextRow();
                    }

                    if (geoms.Count > 0)
                    {
                        result.Geometries = geoms;
                        result.BoundingBox = geoms.Envelope();
                    }
                }
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            return result;
        }

        private Dictionary<string, Type> ParseColumnHeader(List<string> row, out List<string> colNames, out string primaryKeyName)
        {
            var cols = new Dictionary<string, Type>();
            colNames = new List<String>();
            primaryKeyName = string.Empty;

            int len = row.Count;
            string name, typeName;
            bool isPrimaryKey = false;
            Type type;

            var headerRegex = new Regex(_columnHeaderPattern);

            for (int i = 0; i < len; i++)
            {
                isPrimaryKey = false;
                name = row[i];
                typeName = string.Empty;

                if (i < len - 1 && row[i + 1].StartsWith("primaryKey)", StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(0, name.IndexOf('('));

                    //Parse out type name
                    typeName = row[i].Substring(row[i].IndexOf('(')+1);

                    if (typeName.Contains(","))
                    {
                        typeName = typeName.Replace(",", "");
                    }

                    isPrimaryKey = true;
                    i++;
                }
                else if (headerRegex.IsMatch(name))
                {
                    Match m = headerRegex.Match(name);

                    //Expect column header to look like:
                    //OData: EntityID(Edm.String,primarykey) or Longitude(Edm.Double)
                    if (m.Success && m.Groups.Count >= 3)
                    {
                        name = m.Groups[1].Value;
                        typeName = m.Groups[2].Value;

                        if (typeName.IndexOf(",") > -1)
                        {
                            typeName = typeName.Substring(0, typeName.IndexOf(","));
                        }

                        if (m.Groups[2].Value.IndexOf("primaryKey", StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            isPrimaryKey = true;
                        }
                    }
                }

                //Validate Data Type
                switch (typeName.ToLowerInvariant())
                {
                    case "edm.int64":
                    case "int":
                    case "long":
                        type = typeof(long);
                        break;
                    case "edm.double":
                    case "float":
                    case "double":
                        type = typeof(double);
                        break;
                    case "edm.boolean":
                    case "bool":
                    case "boolean":
                        type = typeof(bool);
                        break;
                    case "edm.datetime":
                    case "date":
                    case "datetime":
                        type = typeof(DateTime);
                        break;
                    case "edm.geography":
                        type = typeof(Geometry);
                        break;
                    case "edm.string":
                    case "varchar":     //Multimap
                    case "text":        //Multimap
                    default:
                        type = typeof(string);
                        break;
                }

                if (!cols.ContainsKey(name))
                {
                    cols.Add(name, type);
                    colNames.Add(name);

                    if (isPrimaryKey)
                    {
                        primaryKeyName = name;
                    }
                }
            }

            return cols;
        }

        private async Task<Geometry> ParseGeometry(List<string> row, List<string> columnNames, Dictionary<string, Type> columnInfo, string primaryKeyName)
        {
            Geometry g = null;
            var metadata = new ShapeMetadata();
            string nodeName;
            double lat = double.NaN, lon = double.NaN, tempDouble;
            long tempLong;
            bool tempBool;
            DateTime tempDate;

            if (row.Count == columnNames.Count)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    nodeName = columnNames[i];

                    if (columnInfo.ContainsKey(nodeName) && !metadata.Properties.ContainsKey(nodeName))
                    {
                        var type = columnInfo[nodeName];

                        if (type == typeof(string))
                        {
                            if (string.Compare(nodeName, primaryKeyName, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                metadata.ID = row[i];
                            }

                            metadata.Properties.Add(nodeName, row[i]);
                        }
                        else if (type == typeof(long))
                        {
                            if (!long.TryParse(row[i], out tempLong))
                            {
                                tempLong = 0;
                            }

                            metadata.Properties.Add(nodeName, tempLong);
                        }
                        else if (type == typeof(double))
                        {
                            if (double.TryParse(row[i], out tempDouble))
                            {
                                if (string.Compare(nodeName, "Latitude", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    lat = tempDouble;
                                }
                                else if (string.Compare(nodeName, "Longitude", StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    lon = tempDouble;
                                }

                                metadata.Properties.Add(nodeName, tempDouble);
                            }
                        }
                        else if (type == typeof(bool))
                        {
                            if (!bool.TryParse(row[i], out tempBool))
                            {
                                tempBool = false;
                            }

                            metadata.Properties.Add(nodeName, tempBool);
                        }
                        else if (type == typeof(DateTime))
                        {
                            if (DateTime.TryParse(row[i], out tempDate))
                            {
                                metadata.Properties.Add(nodeName, tempDate);
                            }
                        }
                        else if (type == typeof(Geometry))
                        {
                            if (!string.IsNullOrWhiteSpace(row[i]))
                            {
                                g = await new Microsoft.Maps.SpatialToolbox.IO.WellKnownText().Read(row[i]);
                            }
                        }
                    }
                }

                if (g == null)
                {
                    g = new Point(lat, lon);
                }

                if (metadata.HasMetadata())
                {
                    g.Metadata = metadata;
                }
            }            

            return g;
        }

        private async Task<SpatialDataSet> ParseDataSource(XDocument doc)
        {
            var result = new SpatialDataSet();

            try
            {
                var geoms = new List<Geometry>();
                string primaryKeyName = string.Empty;
                var columnInfo = new Dictionary<string, Type>();
                
                var schema = doc.Root.FirstNode as XElement;

                if (schema != null)
                {
                    var idAttr = XmlUtilities.GetAttribute(schema, "id");
                    if (idAttr != null)
                    {
                        result.Metadata.Properties.Add(DataSourceNameKey, idAttr.Value);
                    }

                    var dsSchema = (schema.FirstNode as XElement);

                    var entitySchema = ((dsSchema.FirstNode as XElement).FirstNode as XElement).FirstNode;
                    var entityNameAttr = XmlUtilities.GetAttribute(entitySchema as XElement, "name");

                    if (entityNameAttr != null)
                    {
                        result.Metadata.Properties.Add(EntityTypeNameKey, entityNameAttr.Value);
                    }

                    var sequenceSchema = ((entitySchema as XElement).FirstNode as XElement).FirstNode as XElement;
                    var sequenceElms = sequenceSchema.Elements();

                    foreach (var e in sequenceElms)
                    {
                        var elm = e as XElement;
                        var name = XmlUtilities.GetAttribute(elm, "name");
                        var xmlType = XmlUtilities.GetAttribute(elm, "type");

                        if (name != null && xmlType != null)
                        {
                            columnInfo.Add(name.Value, GetXmlPropertyType(xmlType.Value));
                        }
                    }

                    var primaryKeySchema = (dsSchema.LastNode as XElement).LastNode;

                    var primaryKeyAttr = XmlUtilities.GetAttribute(primaryKeySchema as XElement, "xpath");
                    if (primaryKeyAttr != null)
                    {
                        primaryKeyName = primaryKeyAttr.Value;
                        result.Metadata.Properties.Add(PrimaryKeyNameKey, primaryKeyName);
                    }

                    if(columnInfo.ContainsValue(typeof(Geometry)))
                    {
                        foreach (var k in columnInfo.Keys)
                        {
                            if (columnInfo[k] == typeof(Geometry))
                            {
                                result.Metadata.Properties.Add(GeometryColumnNameKey, k);
                                break;
                            }
                        }    
                    }
                    
                    var n = schema.NextNode;
                    while (n != null)
                    {
                        var g = await ParseGeometry(n as XElement, columnInfo, primaryKeyName);

                        if (g != null)
                        {
                            geoms.Add(g);
                        }

                        n = n.NextNode;
                    }

                    if (geoms.Count > 0)
                    {
                        result.Geometries = geoms;
                        result.BoundingBox = geoms.Envelope();
                    }
                }
            }
            catch (Exception ex)
            {
                result.Error = ex.Message;
            }

            return result;
        }

        private async Task<Geometry> ParseGeometry(XElement node, Dictionary<string, Type> columnInfo, string primaryKeyName)
        {
            XElement n = node.FirstNode as XElement;

            Geometry g = null;
            var metadata = new ShapeMetadata();
            string nodeName, tempString;
            double lat = double.NaN, lon = double.NaN, tempDouble;
            
            while (n != null)
            {
                nodeName = n.Name.LocalName;

                if (columnInfo.ContainsKey(nodeName) && !metadata.Properties.ContainsKey(nodeName))
                {
                    var type = columnInfo[nodeName];

                    if (type == typeof(string))
                    {
                        tempString = XmlUtilities.GetString(n, false);

                        if (string.Compare(nodeName, primaryKeyName, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            metadata.ID = tempString;
                        }

                        metadata.Properties.Add(nodeName, tempString);
                    }
                    else if (type == typeof(long))
                    {
                        metadata.Properties.Add(nodeName, XmlUtilities.GetInt64(n, 0));
                    }
                    else if (type == typeof(double))
                    {
                        tempDouble = XmlUtilities.GetDouble(n, double.NaN);

                        if (!double.IsNaN(tempDouble))
                        {
                            if (string.Compare(nodeName, "Latitude", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                lat = tempDouble;
                            }
                            else if (string.Compare(nodeName, "Longitude", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                lon = tempDouble;
                            }
                            
                            metadata.Properties.Add(nodeName, tempDouble);
                        }
                    }
                    else if (type == typeof(bool))
                    {
                        metadata.Properties.Add(nodeName, XmlUtilities.GetBoolean(n, false));
                    }
                    else if (type == typeof(DateTime))
                    {
                        var time = XmlUtilities.GetDateTime(n);
                        if (time.HasValue)
                        {
                            metadata.Properties.Add(nodeName, time);
                        }
                    }
                    else if (type == typeof(Geometry))
                    {
                        var sGeom = XmlUtilities.GetString(n, false);

                        if (!string.IsNullOrWhiteSpace(sGeom))
                        {
                            g = await new Microsoft.Maps.SpatialToolbox.IO.WellKnownText().Read(sGeom);
                        }
                    }
                }

                n = n.NextNode as XElement;
            }

            if (g == null)
            {
                g = new Point(lat, lon);
            }

            if (metadata.HasMetadata())
            {
                g.Metadata = metadata;
            }
            
            return g;
        }

        #endregion

        #region Data Source Write Methods

        private void Write(SpatialDataSet data, Stream outputStream)
        {
            char delimiter;

            switch (_dataSourceType)
            {
                case BingDataSourceType.PIPE:
                    delimiter = '|';
                    break;
                case BingDataSourceType.TAB:
                    delimiter = '\t';
                    break;
                case BingDataSourceType.CSV:
                default:
                    delimiter = ',';
                    break;
            }

            Dictionary<string, Type> columnInfo;            
            bool hasGeometry;
            string dataSourceName, entityTypeName, primaryKeyName, geometryColumnName;

            data = CleanDataSet(data, out columnInfo, out dataSourceName, out entityTypeName, out primaryKeyName, out geometryColumnName, out hasGeometry);

            var colNames = new List<string>(columnInfo.Keys);

            if (!columnInfo.ContainsKey(primaryKeyName))
            {
                columnInfo.Add(primaryKeyName, typeof(string));
                colNames.Insert(0, primaryKeyName);
            }

            if (!columnInfo.ContainsKey("Latitude"))
            {
                columnInfo.Add("Latitude", typeof(double));
                colNames.Add("Latitude");
            }

            if (!columnInfo.ContainsKey("Longitude"))
            {
                columnInfo.Add("Longitude", typeof(double));
                colNames.Add("Longitude");
            }

            if (hasGeometry && !columnInfo.ContainsKey(geometryColumnName))
            {
                columnInfo.Add(geometryColumnName, typeof(string));
                colNames.Add(geometryColumnName);
            }

            using (var writer = new DelimitedFileWriter(outputStream, columnInfo.Count, delimiter))
            {
                //Add Bing Schema Information
                writer.WriteLine(string.Format(_schemaText, entityTypeName));

                var headerCells = new List<string>();

                //Generate Header Information
                foreach (var colName in colNames)
                {
                    var name = colName + "(" + GetDelimitedPropertyType(columnInfo[colName]);

                    if (string.CompareOrdinal(colName, primaryKeyName) == 0)
                    {
                        name += ",primaryKey";
                    }

                    name += ")";

                    headerCells.Add(name);
                }

                //Write Header Information
                writer.WriteRow(headerCells, false);

                if (data != null)
                {
                    //Write the rows
                    foreach (var item in data.Geometries)
                    {
                        WriteGeometry(item, writer, colNames, primaryKeyName, geometryColumnName, hasGeometry);
                    }
                }
            }
        }

        private void WriteGeometry(Geometry geom, DelimitedFileWriter writer, List<string> colNames, string primaryKeyName, string geometryColumnName, bool hasGeometry)
        {
            if (geom.Metadata != null)
            {
                var row = new List<object>();

                foreach (var colName in colNames)
                {
                    if (hasGeometry && string.CompareOrdinal(colName, geometryColumnName) == 0)
                    {
                        row.Add(geom.ToString());
                    }
                    else if (string.CompareOrdinal(colName, "Latitude") == 0 && geom is Point)
                    {
                        var p = geom as Point;

                        if (p.IsValid())
                        {
                            row.Add(p.Coordinate.Latitude.ToString("N5"));
                        }
                        else
                        {
                            row.Add(string.Empty);
                        }
                    }
                    else if (string.CompareOrdinal(colName, "Longitude") == 0 && geom is Point)
                    {
                        var p = geom as Point;

                        if (p.IsValid())
                        {
                            row.Add(p.Coordinate.Longitude.ToString("N5"));
                        }
                        else
                        {
                            row.Add(string.Empty);
                        }
                    }
                    else if (string.CompareOrdinal(colName, primaryKeyName) == 0)
                    {
                        row.Add(geom.Metadata.ID);
                    }
                    else if (geom.Metadata.Properties.ContainsKey(colName))
                    {
                        row.Add(geom.Metadata.Properties[colName]);
                    }
                    else
                    {
                        row.Add(string.Empty);
                    }
                }

                writer.WriteRow(row, true);
            }
        }
        
        private void Write(SpatialDataSet data, XmlWriter xmlWriter)
        {
            //<?xml version="1.0" encoding="UTF-8"?>
            //<DataSourceNameData>
            //  <xs:schema id="DataSourceName" xmlns="" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
            //    <xs:element name="DataSourceName" msdata:IsDataSet="true" msdata:UseCurrentLocale="true">
            //      <xs:complexType>
            //        <xs:choice minOccurs="0" maxOccurs="unbounded">
            //          <xs:element name="EntityTypeName">
            //            <xs:complexType>
            //              <xs:sequence>
            //                <xs:element name="AddressLine" type="xs:string" minOccurs="0" />
            //                <xs:element name="Locality" type="xs:string" minOccurs="0" />
            //                <xs:element name="AdminDistrict" type="xs:string" minOccurs="0" />
            //                <xs:element name="CountryRegion" type="xs:string" minOccurs="0" />
            //                <xs:element name="PostalCode" type="xs:string" minOccurs="0" />
            //                <xs:element name="Name" type="xs:string" minOccurs="0" />
            //                <xs:element name="EntityID" type="xs:string" />
            //                <xs:element name="Longitude" type="xs:double" minOccurs="0" />
            //                <xs:element name="Latitude" type="xs:double" minOccurs="0" />
            //                <xs:element name="StoreLayout" type="xs:anyType" minOccurs="0" />
            //              </xs:sequence>
            //            </xs:complexType>
            //          </xs:element>
            //        </xs:choice>
            //      </xs:complexType>
            //      <xs:unique name="Constraint1" msdata:PrimaryKey="true">
            //        <xs:selector xpath=".//EntityTypeName" />
            //        <xs:field xpath="EntityID" />
            //      </xs:unique>
            //    </xs:element>
            //  </xs:schema>
            //  <EntityTypeName>
            //    <AddressLine>Løven</AddressLine>
            //    <Locality>Aalborg</Locality>
            //    <AdminDistrict>Nordjyllands Amt</AdminDistrict>
            //    <CountryRegion>Danmark</CountryRegion>
            //    <PostalCode>9200</PostalCode>
            //    <Name>Fourth Coffee Store #22067</Name>
            //    <EntityID>-22067</EntityID>
            //    <Longitude>9.87443416667</Longitude>
            //    <Latitude>57.00376611111</Latitude>
            //    <StoreLayout>POLYGON((9.86445 57.13876,9.89266 57.13876, 9.89266 56.94234,9.86445 56.94234,9.86445 57.13876))</StoreLayout>
            //  </EntityTypeName>
            //</DataSourceNameData>

            Dictionary<string, Type> columnInfo;
            bool hasGeometry;
            string dataSourceName, entityTypeName, primaryKeyName, geometryColumnName;

            data = CleanDataSet(data, out columnInfo, out dataSourceName, out entityTypeName, out primaryKeyName, out geometryColumnName, out hasGeometry);

            //Open document
            xmlWriter.WriteStartDocument(true);           

            //Write root tag -> Data Source Name.
            xmlWriter.WriteStartElement(dataSourceName + "Data");
           
            //Write schema info
            WriteXmlSchema(xmlWriter, columnInfo, dataSourceName, entityTypeName, primaryKeyName, geometryColumnName, hasGeometry);

            if (data != null)
            {
                foreach (var item in data.Geometries)
                {
                    WriteXmlGeometry(item, xmlWriter, columnInfo, dataSourceName, entityTypeName, primaryKeyName, geometryColumnName, hasGeometry);
                }
            }
            
            //Close document
            xmlWriter.WriteEndDocument();
        }

        private void WriteXmlSchema(XmlWriter xmlWriter, Dictionary<string, Type> columnInfo, string dataSourceName, string entityTypeName, string primaryKeyName, string geometryColumnName, bool hasGeometry)
        {
            //  <xs:schema id="DataSourceName" xmlns="" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
            //    <xs:element name="DataSourceName" msdata:IsDataSet="true" msdata:UseCurrentLocale="true">
            //      <xs:complexType>
            //        <xs:choice minOccurs="0" maxOccurs="unbounded">
            //          <xs:element name="EntityTypeName">
            //            <xs:complexType>
            //              <xs:sequence>
            //                <xs:element name="AddressLine" type="xs:string" minOccurs="0" />
            //                <xs:element name="Locality" type="xs:string" minOccurs="0" />
            //                <xs:element name="AdminDistrict" type="xs:string" minOccurs="0" />
            //                <xs:element name="CountryRegion" type="xs:string" minOccurs="0" />
            //                <xs:element name="PostalCode" type="xs:string" minOccurs="0" />
            //                <xs:element name="Name" type="xs:string" minOccurs="0" />
            //                <xs:element name="PrimaryKeyName" type="xs:string" />
            //                <xs:element name="Longitude" type="xs:double" minOccurs="0" />
            //                <xs:element name="Latitude" type="xs:double" minOccurs="0" />
            //                <xs:element name="StoreLayout" type="xs:anyType" minOccurs="0" />
            //              </xs:sequence>
            //            </xs:complexType>
            //          </xs:element>
            //        </xs:choice>
            //      </xs:complexType>
            //      <xs:unique name="Constraint1" msdata:PrimaryKey="true">
            //        <xs:selector xpath=".//EntityTypeName" />
            //        <xs:field xpath="PrimaryKeyName" />
            //      </xs:unique>
            //    </xs:element>
            //  </xs:schema>

            //Write start of Schema 
            xmlWriter.WriteStartElement("xs", "schema", XmlSchemaNamespace);

            xmlWriter.WriteAttributeString("id", dataSourceName);
            xmlWriter.WriteAttributeString("xmlns", "xs", XmlnsNamespace, XmlSchemaNamespace);
            xmlWriter.WriteAttributeString("xmlns", "msdata", XmlnsNamespace, MsDataNamespace);

            //Write start of data source element
            xmlWriter.WriteStartElement("element", XmlSchemaNamespace);

            xmlWriter.WriteAttributeString("name", dataSourceName);
            xmlWriter.WriteAttributeString("IsDataSet", MsDataNamespace, "true");
            xmlWriter.WriteAttributeString("UseCurrentLocale", MsDataNamespace, "true");

            xmlWriter.WriteStartElement("complexType", XmlSchemaNamespace);

            xmlWriter.WriteStartElement("choice", XmlSchemaNamespace);
            xmlWriter.WriteAttributeString("minOccurs", "0");
            xmlWriter.WriteAttributeString("maxOccurs", "unbounded");

            //Write start of EntityTypeName element
            xmlWriter.WriteStartElement("element", XmlSchemaNamespace);

            xmlWriter.WriteAttributeString("name", entityTypeName);

            xmlWriter.WriteStartElement("complexType", XmlSchemaNamespace);

            xmlWriter.WriteStartElement("sequence", XmlSchemaNamespace);

            xmlWriter.WriteStartElement("element", XmlSchemaNamespace);
            xmlWriter.WriteAttributeString("name", primaryKeyName);
            xmlWriter.WriteAttributeString("type", "xs:string");
            xmlWriter.WriteEndElement();

            if (columnInfo != null)
            {
                foreach (var key in columnInfo.Keys)
                {
                    xmlWriter.WriteStartElement("element", XmlSchemaNamespace);
                    xmlWriter.WriteAttributeString("name", key);
                    xmlWriter.WriteAttributeString("type", GetXmlPropertyType(columnInfo[key]));
                    xmlWriter.WriteAttributeString("minOccurs", "0");

                    if(columnInfo[key] == typeof(DateTime)){
                        xmlWriter.WriteAttributeString("DateTimeMode", MsDataNamespace, "Utc");
                    }

                    xmlWriter.WriteEndElement();
                }
            }

            xmlWriter.WriteStartElement("element", XmlSchemaNamespace);
            xmlWriter.WriteAttributeString("name", "Latitude");
            xmlWriter.WriteAttributeString("type", "xs:double");
            xmlWriter.WriteAttributeString("minOccurs", "0");
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("element", XmlSchemaNamespace);
            xmlWriter.WriteAttributeString("name", "Longitude");
            xmlWriter.WriteAttributeString("type", "xs:double");
            xmlWriter.WriteAttributeString("minOccurs", "0");
            xmlWriter.WriteEndElement();

            if (hasGeometry)
            {
                xmlWriter.WriteStartElement("element", XmlSchemaNamespace);
                xmlWriter.WriteAttributeString("name", geometryColumnName);
                xmlWriter.WriteAttributeString("type", "xs:anyType");
                xmlWriter.WriteAttributeString("minOccurs", "0");
                xmlWriter.WriteEndElement();
            }

            //Close sequence
            xmlWriter.WriteEndElement();

            //Close end of ComplexType
            xmlWriter.WriteEndElement();

            //Close end of EntityTypeName element
            xmlWriter.WriteEndElement();

            //Close end of choice
            xmlWriter.WriteEndElement();

            //Close end of ComplexType
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("unique", XmlSchemaNamespace);
            xmlWriter.WriteAttributeString("name", "Constraint1");
            xmlWriter.WriteAttributeString("PrimaryKey", MsDataNamespace, "true");

            xmlWriter.WriteStartElement("selector", XmlSchemaNamespace);
            xmlWriter.WriteAttributeString("xpath", ".//" + entityTypeName);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("field", XmlSchemaNamespace);
            xmlWriter.WriteAttributeString("xpath", primaryKeyName);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();

            //Close end of data source element
            xmlWriter.WriteEndElement();

            //Close end of Schema
            xmlWriter.WriteEndElement();
        }

        private void WriteXmlGeometry(Geometry geom, XmlWriter xmlWriter, Dictionary<string, Type> columnInfo, string dataSourceName, string entityTypeName, string primaryKeyName, string geometryColumnName, bool hasGeometry)
        {
            if (geom.Metadata != null)
            {
                xmlWriter.WriteStartElement(entityTypeName);

                xmlWriter.WriteElementString(primaryKeyName, geom.Metadata.ID);
                string tempString;

                foreach (var key in geom.Metadata.Properties.Keys)
                {
                    if (columnInfo.ContainsKey(key))
                    {
                        //TODO: Consider adding limit to string properties to match limits of API.

                        if (columnInfo[key] == typeof(DateTime) && geom.Metadata.Properties[key] != null)
                        {
                            tempString = ((DateTime)geom.Metadata.Properties[key]).ToUniversalTime().ToString("O");
                        }
                        else
                        {
                            tempString = geom.Metadata.Properties[key].ToString();
                        }

                        if(!string.IsNullOrWhiteSpace(tempString))
                        {
                            xmlWriter.WriteElementString(key, tempString);
                        }
                    }
                }

                if (hasGeometry)
                {
                    xmlWriter.WriteElementString(geometryColumnName, geom.ToString());
                }
                else if(geom is Point)
                {
                    var p = geom as Point;

                    //If the coordinate has values that are NaN then it's a placeholder that should be left empty.
                    if (!double.IsNaN(p.Coordinate.Latitude) && !double.IsNaN(p.Coordinate.Longitude))
                    {
                        xmlWriter.WriteElementString("Latitude", p.Coordinate.Latitude.ToString("N5"));
                        xmlWriter.WriteElementString("Longitude", p.Coordinate.Longitude.ToString("N5"));
                    }
                    else
                    {
                        xmlWriter.WriteElementString("Latitude", "");
                        xmlWriter.WriteElementString("Longitude", "");                        
                    }
                }
                else
                {
                    xmlWriter.WriteElementString("Latitude", "");
                    xmlWriter.WriteElementString("Longitude", "");
                }

                xmlWriter.WriteEndElement();
            }
        }

        #endregion

        #region Helper Methods

        private string GetDelimitedPropertyType(Type type)
        {
            if (type == typeof(string) || type == typeof(String))
            {
                return "Edm.String";
            }
            else if (type == typeof(int) || type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64) || type == typeof(long))
            {
                return "Edm.Int64";
            }
            else if (type == typeof(bool) || type == typeof(Boolean))
            {
                return "Edm.Boolean";
            }
            else if (type == typeof(double) || type == typeof(Double))
            {
                return "Edm.Double";
            }
            else if (type == typeof(DateTime))
            {
                return "Edm.dateTime";
            }
            else if (type == typeof(Geometry))
            {
                return "Edm.Geography";
            }

            return "Edm.String";
        }

        private string GetXmlPropertyType(Type type)
        {
            if (type == typeof(string) || type == typeof(String))
            {
                return "xs:string";
            }
            else if (type == typeof(int) || type == typeof(Int16) || type == typeof(Int32) || type == typeof(Int64) || type == typeof(long))
            {
                return "xs:long";
            }
            else if (type == typeof(bool) || type == typeof(Boolean))
            {
                return "xs:boolean";
            }
            else if (type == typeof(double) || type == typeof(Double))
            {
                return "xs:double";
            }
            else if (type == typeof(DateTime))
            {
                return "xs:dateTime";
            }
            else if (type == typeof(Geometry))
            {
                return "xs:anyType";
            }

            return "xs:string";
        }

        private Type GetXmlPropertyType(string type)
        {
            switch (type.ToLowerInvariant())
            {
                case "xs:long":
                    return typeof(long);
                case "xs:boolean":
                    return typeof(bool);
                case "xs:double":
                    return typeof(double);
                case "xs:datetime":
                    return typeof(DateTime);
                case "xs:anytype":
                    return typeof(Geometry);
                case "xs:string":
                default:
                    return typeof(string);

            }
        }

        private SpatialDataSet CleanDataSet(SpatialDataSet data, out Dictionary<string, Type> columnInfo, out string dataSourceName, out string entityTypeName, out string primaryKeyName, out string geometryColumnName, out bool hasGeometry)
        {
            columnInfo = new Dictionary<string, Type>();
            hasGeometry = false;
            dataSourceName = _dataSourceName;
            entityTypeName = _entityTypeName;
            primaryKeyName = _primaryKeyName;
            geometryColumnName = _geometryColumnName;
            
            if (data != null)
            {
                if (data.Metadata != null && data.Metadata.Properties != null)
                {
                    if (data.Metadata.Properties.ContainsKey(DataSourceNameKey))
                    {
                        var s = data.Metadata.Properties[DataSourceNameKey].ToString();

                        if (IsValidName(s))
                        {
                            dataSourceName = s;
                        }
                    }

                    if (data.Metadata.Properties.ContainsKey(EntityTypeNameKey))
                    {
                        var s = data.Metadata.Properties[EntityTypeNameKey].ToString();

                        if (IsValidName(s))
                        {
                            entityTypeName = s;
                        }
                    }

                    if (data.Metadata.Properties.ContainsKey(PrimaryKeyNameKey))
                    {
                        var s = data.Metadata.Properties[PrimaryKeyNameKey].ToString();

                        if (IsValidPropertyName(s))
                        {
                            primaryKeyName = s;
                        }
                    }

                    if (data.Metadata.Properties.ContainsKey(GeometryColumnNameKey))
                    {
                        var s = data.Metadata.Properties[GeometryColumnNameKey].ToString();

                        if (IsValidPropertyName(s))
                        {
                            geometryColumnName = s;
                        }
                    }
                }


                var ids = new List<string>();
                long idIdx = 1000000;
                Geometry g;

                for (var i = 0; i < data.Geometries.Count; i++)
                {
                    g = data.Geometries[i];

                    if (!(g is Point))
                    {
                        hasGeometry = true;
                    }
                    else if (g.STNumPoints() > _maxGeomPoints)
                    {
                        //TODO: Reduce resolution of data so that it fits into limits                        
                    }

                    if (g.Metadata != null && g.Metadata.Properties != null)
                    {
                        //Check metadata
                        foreach (var key in g.Metadata.Properties.Keys)
                        {
                            if (!columnInfo.ContainsKey(key) && IsValidPropertyName(key) &&
                                string.Compare(key, "Latitude", StringComparison.OrdinalIgnoreCase) != 0 &&
                                string.Compare(key, "Longitude", StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                columnInfo.Add(key, g.Metadata.Properties[key].GetType());
                            }
                        }

                        //Check ID
                        if (string.IsNullOrWhiteSpace(g.Metadata.ID) || ids.Contains(g.Metadata.ID))
                        {
                            if (g.Metadata.Properties.ContainsKey(primaryKeyName) &&
                                !string.IsNullOrWhiteSpace(g.Metadata.Properties[primaryKeyName].ToString()) &&
                                !ids.Contains(g.Metadata.Properties[primaryKeyName].ToString()))
                            {
                                g.Metadata.ID = g.Metadata.Properties[primaryKeyName].ToString();
                            }
                            else
                            {
                                data.Geometries[i].Metadata.ID = idIdx.ToString();
                                idIdx++;
                            }
                        }

                        ids.Add(data.Geometries[i].Metadata.ID);
                    }
                }
            }

            return data;
        }

        //A data source name can have up to 50 characters and can contain alphanumeric characters and any of the following:
        // ~ ` ! $ ^ _ = { }
        private bool IsValidName(string name)
        {
            return (!string.IsNullOrWhiteSpace(name) && name.Length <= 50 && nameRegex.IsMatch(name));
        }

        //The property name can have up to 50 characters.
        //• The property name must contain alphanumeric characters and underscores (_) only.
        //• The first character of the property name must be a letter or an underscore.
        //• The property name cannot start with a two underscores (__).
        //• Property names are case-insensitive.
        private bool IsValidPropertyName(string name)
        {
            return (!string.IsNullOrWhiteSpace(name) && name.Length <= 50 && propertyNameRegex.IsMatch(name) && string.Compare(name, _primaryKeyName, StringComparison.OrdinalIgnoreCase) != 0);
        }

        #endregion

        #endregion
    }
}
