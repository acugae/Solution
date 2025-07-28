namespace Solution.Data.Provider;

/// <summary>
/// Classe per la gestione del parametro in un command.
/// </summary>
public class Parameter 
{
    private DbParameter parameter;
    private int _size;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oCn"></param>
    public Parameter(Connection oCn)
    {
        parameter = oCn.Provider.CreateDataParameter();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oCn"></param>
    public Parameter(Connection oCn, System.Data.DbType oDbType, System.Data.ParameterDirection oDirection, string sName, object oValue)
    {
        parameter = oCn.Provider.CreateDataParameter();
        this.DbType = oDbType;
        this.Direction = oDirection;
        this.ParameterName = sName;
        this.Value = oValue;
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.DbType DbType
    {
        get { return parameter.DbType; }
        set { parameter.DbType = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.ParameterDirection Direction
    {
        get { return parameter.Direction; }
        set { parameter.Direction = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public bool IsNullable
    {
        get { return parameter.IsNullable; }
    }
    /// <summary>
    /// 
    /// </summary>
    public int Size
    {
        get { return _size; }
        set { _size = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public string ParameterName
    {
        get { return parameter.ParameterName; }
        set { parameter.ParameterName = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public string SourceColumn
    {
        get { return parameter.SourceColumn; }
        set { parameter.SourceColumn = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.DataRowVersion SourceVersion
    {
        get { return parameter.SourceVersion; }
        set { parameter.SourceVersion = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public object Value
    {
        get { return parameter.Value; }
        set { parameter.Value = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public DbParameter DbParameter
    {
        get { return parameter; }
        set { parameter = value; }
    }
}
