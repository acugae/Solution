namespace Solution.Data.Provider;

/// <summary>
/// Recupera un flusso di dati di sola lettura da un database.
/// </summary>
public class DataReader : System.Data.IDataReader
{
    private IDataReader _DataReader;
    /// <summary>
    /// 
    /// </summary>
    public void Dispose()
    {
        _DataReader = null;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="IDReader"></param>
    public DataReader(IDataReader IDReader)
    {
        _DataReader = IDReader;
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.IDataReader IDataReader
    {
        get { return _DataReader; }
        set { _DataReader = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public void Close()
    {
        _DataReader.Close();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public System.Data.DataTable GetSchemaTable()
    {
        return _DataReader.GetSchemaTable();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public System.Boolean NextResult()
    {
        return _DataReader.NextResult();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public System.Boolean Read()
    {
        return _DataReader.Read();
    }
    /// <summary>
    /// 
    /// </summary>
    public int Depth
    {
        get { return _DataReader.Depth; }
    }
    /// <summary>
    /// 
    /// </summary>
    public bool IsClosed
    {
        get { return _DataReader.IsClosed; }
    }
    /// <summary>
    /// 
    /// </summary>
    public int RecordsAffected
    {
        get { return _DataReader.RecordsAffected; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public System.Boolean GetBoolean(System.Int32 i)
    {
        return _DataReader.GetBoolean(i);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public System.Byte GetByte(System.Int32 i)
    {
        return _DataReader.GetByte(i);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <param name="fieldOffset"></param>
    /// <param name="buffer"></param>
    /// <param name="bufferoffset"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public System.Int64 GetBytes(System.Int32 i, System.Int64 fieldOffset, byte[] buffer, System.Int32 bufferoffset, System.Int32 length)
    {
        return _DataReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public System.Char GetChar(System.Int32 i)
    {
        return _DataReader.GetChar(i);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <param name="fieldoffset"></param>
    /// <param name="buffer"></param>
    /// <param name="bufferoffset"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public System.Int64 GetChars(System.Int32 i, System.Int64 fieldoffset, char[] buffer, System.Int32 bufferoffset, System.Int32 length)
    {
        return _DataReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public System.Data.IDataReader GetData(System.Int32 i)
    {
        return _DataReader.GetData(i);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public System.String GetDataTypeName(System.Int32 i)
    {
        return _DataReader.GetDataTypeName(i);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public System.DateTime GetDateTime(System.Int32 i)
    {
        return _DataReader.GetDateTime(i);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public System.Decimal GetDecimal(System.Int32 i)
    {
        return _DataReader.GetDecimal(i);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public System.Double GetDouble(System.Int32 i)
    {
        return _DataReader.GetDouble(i);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public System.Type GetFieldType(System.Int32 i)
    {
        return _DataReader.GetFieldType(i);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public System.Single GetFloat(System.Int32 i)
    {
        return _DataReader.GetFloat(i);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public System.Guid GetGuid(System.Int32 i)
    {
        return _DataReader.GetGuid(i);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public System.Int16 GetInt16(System.Int32 i)
    {
        return _DataReader.GetInt16(i);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public System.Int32 GetInt32(System.Int32 i)
    {
        return _DataReader.GetInt32(i);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public System.Int64 GetInt64(System.Int32 i)
    {
        return _DataReader.GetInt64(i);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public System.String GetName(System.Int32 i)
    {
        return _DataReader.GetName(i);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public System.Int32 GetOrdinal(System.String name)
    {
        return _DataReader.GetOrdinal(name);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public System.String GetString(System.Int32 i)
    {
        return _DataReader.GetString(i);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public System.Object GetValue(System.Int32 i)
    {
        return _DataReader.GetValue(i);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public System.Int32 GetValues(object[] values)
    {
        return _DataReader.GetValues(values);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public System.Boolean IsDBNull(System.Int32 i)
    {
        return _DataReader.IsDBNull(i);
    }
    /// <summary>
    /// 
    /// </summary>
    public int FieldCount
    {
        get { return _DataReader.FieldCount; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public object this[string name]
    {
        get { return _DataReader[name]; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public object this[int index]
    {
        get { return _DataReader[index]; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public DataSet ToDataSet()
    {
        DataSet dataSet = new DataSet();
        do
        {
            // Create new data table

            DataTable schemaTable = _DataReader.GetSchemaTable();
            DataTable dataTable = new DataTable();

            if (schemaTable != null)
            {
                // A query returning records was executed
                for (int i = 0; i < schemaTable.Rows.Count; i++)
                {
                    DataRow dataRow = schemaTable.Rows[i];
                    // Create a column name that is unique in the data table
                    string columnName = (string)dataRow["ColumnName"]; //+ "<C" + i + "/>";
                                                                       // Add the column definition to the data table
                    DataColumn column = new DataColumn(columnName, (Type)dataRow["DataType"]);
                    dataTable.Columns.Add(column);
                }

                dataSet.Tables.Add(dataTable);

                // Fill the data table we just created

                while (_DataReader.Read())
                {
                    DataRow dataRow = dataTable.NewRow();
                    for (int i = 0; i < _DataReader.FieldCount; i++)
                        dataRow[i] = _DataReader.GetValue(i);
                    dataTable.Rows.Add(dataRow);
                }
            }
            else
            {
                // No records were returned
                DataColumn column = new DataColumn("RowsAffected");
                dataTable.Columns.Add(column);
                dataSet.Tables.Add(dataTable);
                DataRow dataRow = dataTable.NewRow();
                dataRow[0] = _DataReader.RecordsAffected;
                dataTable.Rows.Add(dataRow);
            }
        }
        while (_DataReader.NextResult());
        this.Close();
        return dataSet;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oDataSet"></param>
    /// <param name="MappingTableName"></param>
    /// <returns></returns>
    public DataSet ToDataSet(DataSet oDataSet, string MappingTableName)
    {
        oDataSet ??= new DataSet();
        DataSet otmpDS = ToDataSet();
        DataTable oDT = new DataTable(MappingTableName);
        if (otmpDS.Tables.Count > 0)
        {
            oDT = otmpDS.Tables[0].Copy();
            oDataSet.Tables.Add(oDT);
        }
        return oDataSet;
    }

}
