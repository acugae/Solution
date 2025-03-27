namespace Solution.Data.Provider;

/// <summary>
/// Classe per la gestione del parametro in un command.
/// </summary>
public class Parameter 
{
    private DbParameter _Parameter;
    private int _size;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oCn"></param>
    public Parameter(Connection oCn)
    {
        _Parameter = oCn.Provider.CreateDataParameter();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oCn"></param>
    public Parameter(Connection oCn, System.Data.DbType oDbType, System.Data.ParameterDirection oDirection, string sName, object oValue)
    {
        _Parameter = oCn.Provider.CreateDataParameter();
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
        get { return _Parameter.DbType; }
        set { _Parameter.DbType = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.ParameterDirection Direction
    {
        get { return _Parameter.Direction; }
        set { _Parameter.Direction = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public bool IsNullable
    {
        get { return _Parameter.IsNullable; }
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
        get { return _Parameter.ParameterName; }
        set { _Parameter.ParameterName = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public string SourceColumn
    {
        get { return _Parameter.SourceColumn; }
        set { _Parameter.SourceColumn = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.DataRowVersion SourceVersion
    {
        get { return _Parameter.SourceVersion; }
        set { _Parameter.SourceVersion = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public object Value
    {
        get { return _Parameter.Value; }
        set { _Parameter.Value = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public DbParameter DbParameter
    {
        get { return _Parameter; }
        set { _Parameter = value; }
    }
}
