namespace Solution.Data.Provider;

/// <summary>
/// Rappresenta un gruppo di comandi SQL e una connessione a un database utilizzati per riempire la classe DataSet e aggiornare l'origine dati. 
/// </summary>
public class DataAdapter
{
    private readonly DbDataAdapter dataAdapter;
    private Connection oCn;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oCn"></param>
    public DataAdapter(Connection oCn)
    {
        this.oCn = oCn;
        dataAdapter = oCn.Provider.CreateDataAdapter();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oCn"></param>
    /// <param name="strSQL"></param>
    public DataAdapter(Connection oCn, string strSQL)
    {
        this.oCn = oCn;
        dataAdapter = oCn.Provider.CreateDataAdapter(strSQL, oCn.DbConnection);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oCmd"></param>
    public DataAdapter(Command oCmd)
    {
        this.oCn = oCmd.Connection;
        dataAdapter = oCn.Provider.CreateDataAdapter(oCmd.DbCommand);
    }
    /// <summary>
    /// 
    /// </summary>
    public Connection Connection
    {
        get { return oCn; }
        set { oCn = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public DbDataAdapter IDbDataAdapter
    {
        get { return dataAdapter; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataSet"></param>
    /// <returns></returns>
    public System.Int32 Fill(DataSet dataSet)
    {
        return dataAdapter.Fill(dataSet);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataSet"></param>
    /// <param name="srcTable"></param>
    /// <returns></returns>
    public System.Int32 Fill(DataSet dataSet, string srcTable)
    {
        DataSet oDS = new DataSet();
        int result = dataAdapter.Fill(oDS);
        oDS.Tables[0].TableName = srcTable;
        if (!dataAdapter.TableMappings.Contains(srcTable))
            dataAdapter.TableMappings.Add(srcTable, srcTable);
        dataSet.Tables.Add(oDS.Tables[0].Copy());
        oDS.Tables.Clear();
        oDS.Clear();
        oDS = null;
        return result;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataSet"></param>
    /// <param name="schemaType"></param>
    /// <returns></returns>
    public System.Data.DataTable[] FillSchema(System.Data.DataSet dataSet, System.Data.SchemaType schemaType)
    {
        return dataAdapter.FillSchema(dataSet, schemaType);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataSet"></param>
    /// <param name="schemaType"></param>
    /// <param name="srcTable"></param>
    /// <returns></returns>
    public System.Data.DataTable[] FillSchema(System.Data.DataSet dataSet, System.Data.SchemaType schemaType, string srcTable)
    {
        if (!dataAdapter.TableMappings.Contains(srcTable))
            dataAdapter.TableMappings.Add(srcTable, srcTable + "DS");
        return dataAdapter.FillSchema(dataSet, schemaType);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public System.Data.IDataParameter[] GetFillParameters()
    {
        return dataAdapter.GetFillParameters();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataSet"></param>
    /// <returns></returns>
    public System.Int32 Update(System.Data.DataSet dataSet)
    {
        return dataAdapter.Update(dataSet);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataSet"></param>
    /// <param name="srcTable"></param>
    /// <returns></returns>
    public System.Int32 Update(System.Data.DataSet dataSet, string srcTable)
    {
        if (!dataAdapter.TableMappings.Contains(srcTable))
            dataAdapter.TableMappings.Add(srcTable, srcTable + "DS");
        return dataAdapter.Update(dataSet);
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.ITableMappingCollection TableMappings
    {
        get { return dataAdapter.TableMappings; }
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.MissingSchemaAction MissingSchemaAction
    {
        get { return dataAdapter.MissingSchemaAction; }
        set { dataAdapter.MissingSchemaAction = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.MissingMappingAction MissingMappingAction
    {
        get { return dataAdapter.MissingMappingAction; }
        set { dataAdapter.MissingMappingAction = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public DbCommand? UpdateCommand
    {
        get { return dataAdapter.UpdateCommand; }
        set { dataAdapter.UpdateCommand = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public DbCommand? SelectCommand
    {
        get { return dataAdapter.SelectCommand; }
        set { dataAdapter.SelectCommand = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public DbCommand? InsertCommand
    {
        get { return dataAdapter.InsertCommand; }
        set { dataAdapter.InsertCommand = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public DbCommand? DeleteCommand
    {
        get { return dataAdapter.DeleteCommand; }
        set { dataAdapter.DeleteCommand = value; }
    }
}
