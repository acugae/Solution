namespace Solution.Data.Provider;


public class  Provider
{
    private DbProviderFactory? instance;
    public string Key { get; }
    public Provider() { }
    public Provider(string sKeyProvider, string sClassFactoryName, string sFilename)
    {
        try
        {
            Key = sKeyProvider;
            if (sFilename == null)
            {
                if (sKeyProvider.ToLower().Equals("sqldb"))
                    instance = Microsoft.Data.SqlClient.SqlClientFactory.Instance;
                if (sKeyProvider.ToLower().Equals("mysdb"))
                    instance = MySql.Data.MySqlClient.MySqlClientFactory.Instance;
                if (sKeyProvider.ToLower().Equals("pstdb"))
                    instance = Npgsql.NpgsqlFactory.Instance;
                //else if (sKeyProvider.ToLower().Equals("oledb"))
                //    _Instance = OleDbFactory.Instance;
                //else if (sKeyProvider.ToLower().Equals("odbdb"))
                //    _Instance = OdbcFactory.Instance;
                else
                {
                }
            }
            else
            {
                Assembly oAss = Assembly.LoadFile(sFilename);
                Type? oType = oAss.GetType(sClassFactoryName);
            }
        }
        catch (TargetInvocationException e)
        {
            throw new SystemException(e.InnerException.Message, e.InnerException);
        }
    }

    #region IDbConnection methods
    public DbConnection? CreateConnection()
    {
        try
        {
            return instance.CreateConnection();
        }
        catch (TargetInvocationException e)
        {
            throw new SystemException(e.InnerException.Message, e.InnerException);
        }
    }
    #endregion

    #region IDbCommand methods
    public DbCommand? CreateCommand()
    {
        try
        {
            return instance.CreateCommand();
        }
        catch (TargetInvocationException e)
        {
            throw new SystemException(e.InnerException.Message, e.InnerException);
        }
    }

    public DbCommand CreateCommand(string cmdText)
    {
        try
        {
            DbCommand cmd = CreateCommand();
            cmd.CommandText = cmdText;
            return cmd;
        }
        catch (TargetInvocationException e)
        {
            throw new SystemException(e.InnerException.Message, e.InnerException);
        }
    }
    //
    public DbCommand CreateCommand(string cmdText, DbConnection connection)
    {
        try
        {
            DbCommand? cmd = CreateCommand();
            if (cmd is null)
                return null;
            cmd.CommandText = cmdText;
            cmd.Connection = connection;
            return cmd;
        }
        catch (TargetInvocationException e)
        {
            throw new SystemException(e.InnerException.Message, e.InnerException);
        }
    }
    //
    public DbCommand? CreateCommand(string cmdText, DbConnection connection, DbTransaction transaction)
    {
        try
        {
            DbCommand? cmd = CreateCommand();
            if (cmd is null)
                return null;
            cmd.CommandText = cmdText;
            cmd.Connection = connection;
            cmd.Transaction = transaction;
            return cmd;
        }
        catch (TargetInvocationException e)
        {
            throw new SystemException(e.InnerException.Message, e.InnerException);
        }
    }
    #endregion

    #region IDbDataAdapter methods
    public DbDataAdapter? CreateDataAdapter()
    {
        try
        {
            return instance.CreateDataAdapter();
        }
        catch (TargetInvocationException e)
        {
            throw new SystemException(e.InnerException.Message, e.InnerException);
        }
    }
    //
    public DbDataAdapter? CreateDataAdapter(DbCommand selectCommand)
    {
        try
        {
            DbDataAdapter? da = CreateDataAdapter();
            da.SelectCommand = selectCommand;
            return da;
        }
        catch (TargetInvocationException e)
        {
            throw new SystemException(e.InnerException.Message, e.InnerException);
        }
    }
    //
    public DbDataAdapter? CreateDataAdapter(string selectCommandText, DbConnection selectConnection)
    {
        try
        {
            return CreateDataAdapter(CreateCommand(selectCommandText, selectConnection));
        }
        catch (TargetInvocationException e)
        {
            throw new SystemException(e.InnerException.Message, e.InnerException);
        }
    }
    #endregion

    #region DbParameter methods
    public DbParameter? CreateDataParameter()
    {
        try
        {
            return instance.CreateParameter();
        }
        catch (TargetInvocationException e)
        {
            throw new SystemException(e.InnerException.Message, e.InnerException);
        }
        return null;
    }
    public DbParameter CreateDataParameter(string parameterName, object value)
    {
        try
        {
            DbParameter? param = CreateDataParameter();
            param.ParameterName = parameterName;
            param.Value = value;
            return param;
        }
        catch (TargetInvocationException e)
        {
            throw new SystemException(e.InnerException.Message, e.InnerException);
        }
    }
    public DbParameter? CreateDataParameter(string parameterName, DbType dataType)
    {
        DbParameter? param = CreateDataParameter();
        if (param != null)
        {
            param.ParameterName = parameterName;
            param.DbType = dataType;
        }
        return param;
    }
    public DbParameter? CreateDataParameter(string parameterName, DbType dataType, int size)
    {
        DbParameter? param = CreateDataParameter();
        if (param != null)
        {
            param.ParameterName = parameterName;
            param.DbType = dataType;
            param.Size = size;
        }
        return param;
    }
    public DbParameter? CreateDataParameter(string parameterName, DbType dataType, int size, string sourceColumn)
    {
        DbParameter? param = CreateDataParameter();
        if (param != null)
        {
            param.ParameterName = parameterName;
            param.DbType = dataType;
            param.Size = size;
            param.SourceColumn = sourceColumn;
        }
        return param;
    }
    #endregion
}
