namespace Solution.Data.Provider;

/// <summary>
/// Rappresenta un gruppo di comandi SQL e una connessione a un database utilizzati per riempire la classe DataSet e aggiornare l'origine dati. 
/// </summary>
public class cDataAdapter : System.Data.IDbDataAdapter
{
    private IDbDataAdapter _DataAdapter;
    private cConnection oCn;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oCn"></param>
    public cDataAdapter(cConnection oCn)
    {
        this.oCn = oCn;
        _DataAdapter = oCn.Provider.CreateDataAdapter();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oCn"></param>
    /// <param name="strSQL"></param>
    public cDataAdapter(cConnection oCn, string strSQL)
    {
        this.oCn = oCn;
        _DataAdapter = oCn.Provider.CreateDataAdapter(strSQL, oCn.Connection);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oCmd"></param>
    public cDataAdapter(cCommand oCmd)
    {
        this.oCn = oCmd.Connection;
        _DataAdapter = oCn.Provider.CreateDataAdapter(oCmd.Command);
    }
    /// <summary>
    /// 
    /// </summary>
    public cConnection Connection
    {
        get { return oCn; }
        set { oCn = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.IDbDataAdapter IDbDataAdapter
    {
        get { return _DataAdapter; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataSet"></param>
    /// <returns></returns>
    public System.Int32 Fill(System.Data.DataSet dataSet)
    {
        return _DataAdapter.Fill(dataSet);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataSet"></param>
    /// <param name="srcTable"></param>
    /// <returns></returns>
    public System.Int32 Fill(System.Data.DataSet dataSet, string srcTable)
    {
        //
        DataSet oDS = new DataSet();
        int result = _DataAdapter.Fill(oDS);
        oDS.Tables[0].TableName = srcTable;
        //
        if (!_DataAdapter.TableMappings.Contains(srcTable))
            _DataAdapter.TableMappings.Add(srcTable, srcTable);
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
        return _DataAdapter.FillSchema(dataSet, schemaType);
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
        if (!_DataAdapter.TableMappings.Contains(srcTable))
            _DataAdapter.TableMappings.Add(srcTable, srcTable + "DS");
        return _DataAdapter.FillSchema(dataSet, schemaType);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public System.Data.IDataParameter[] GetFillParameters()
    {
        return _DataAdapter.GetFillParameters();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataSet"></param>
    /// <returns></returns>
    public System.Int32 Update(System.Data.DataSet dataSet)
    {
        return _DataAdapter.Update(dataSet);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataSet"></param>
    /// <param name="srcTable"></param>
    /// <returns></returns>
    public System.Int32 Update(System.Data.DataSet dataSet, string srcTable)
    {
        if (!_DataAdapter.TableMappings.Contains(srcTable))
            _DataAdapter.TableMappings.Add(srcTable, srcTable + "DS");
        return _DataAdapter.Update(dataSet);
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.ITableMappingCollection TableMappings
    {
        get { return _DataAdapter.TableMappings; }
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.MissingSchemaAction MissingSchemaAction
    {
        get { return _DataAdapter.MissingSchemaAction; }
        set { _DataAdapter.MissingSchemaAction = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.MissingMappingAction MissingMappingAction
    {
        get { return _DataAdapter.MissingMappingAction; }
        set { _DataAdapter.MissingMappingAction = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.IDbCommand UpdateCommand
    {
        get { return _DataAdapter.UpdateCommand; }
        set { _DataAdapter.UpdateCommand = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.IDbCommand SelectCommand
    {
        get { return _DataAdapter.SelectCommand; }
        set { _DataAdapter.SelectCommand = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.IDbCommand InsertCommand
    {
        get { return _DataAdapter.InsertCommand; }
        set { _DataAdapter.InsertCommand = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.IDbCommand DeleteCommand
    {
        get { return _DataAdapter.DeleteCommand; }
        set { _DataAdapter.DeleteCommand = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.IDbDataAdapter DataAdapter
    {
        get { return _DataAdapter; }
    }
}
