namespace Solution.Data.Provider;


public class cProvider
{
    private string _strKey;
    private object _Instance;

    public string Key
    {
        get { return _strKey; }
        set { _strKey = value; }
    }

#if (!MOBILE)

    public cProvider() { }

    public cProvider(string sKeyProvider, string sClassFactoryName, string sFilename)
    {
        try
        {
            _strKey = sKeyProvider;
            if (sFilename == null)
            {
                if (sKeyProvider.ToLower().Equals("sqldb"))
                    _Instance = SqlClientFactory.Instance;
                if (sKeyProvider.ToLower().Equals("mysdb"))
                    _Instance = MySql.Data.MySqlClient.MySqlClientFactory.Instance;
                if (sKeyProvider.ToLower().Equals("pstdb"))
                    _Instance = Npgsql.NpgsqlFactory.Instance;
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
                Type oType = oAss.GetType(sClassFactoryName);
            }
        }
        catch (TargetInvocationException e)
        {
            throw new SystemException(e.InnerException.Message, e.InnerException);
        }
    }
#else
        int indexProvider = -1;
        public cProvider(string sKeyProvider, string sClassFactoryName, string sFilename)
        {
            try
            {
                _strKey = sKeyProvider;
                if (sKeyProvider.ToLower().Trim().Equals("sqlce"))
                    indexProvider = 0;
                else if (sKeyProvider.ToLower().Trim().Equals("sqldb"))
                    indexProvider = 1;
            }
            catch (TargetInvocationException e)
            {
                throw new SystemException(e.InnerException.Message, e.InnerException);
            }
        }
#endif

    #region IDbConnection methods
    public IDbConnection CreateConnection()
    {
        try
        {
            return (IDbConnection)((DbProviderFactory)_Instance).CreateConnection();
        }
        catch (TargetInvocationException e)
        {
            throw new SystemException(e.InnerException.Message, e.InnerException);
        }
    }
    #endregion

    #region IDbCommand methods
    public IDbCommand CreateCommand()
    {
        try
        {
            return (IDbCommand)((DbProviderFactory)_Instance).CreateCommand();
        }
        catch (TargetInvocationException e)
        {
            throw new SystemException(e.InnerException.Message, e.InnerException);
        }
    }

    public IDbCommand CreateCommand(string cmdText)
    {
        try
        {
            IDbCommand cmd = CreateCommand();
            cmd.CommandText = cmdText;
            return cmd;
        }
        catch (TargetInvocationException e)
        {
            throw new SystemException(e.InnerException.Message, e.InnerException);
        }
    }
    //
    public IDbCommand CreateCommand(string cmdText, IDbConnection connection)
    {
        try
        {
            IDbCommand cmd = CreateCommand();
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
    public IDbCommand CreateCommand(string cmdText, IDbConnection connection, IDbTransaction transaction)
    {
        try
        {
            IDbCommand cmd = CreateCommand();
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
    public IDbDataAdapter CreateDataAdapter()
    {
        try
        {
            return (IDbDataAdapter)((DbProviderFactory)_Instance).CreateDataAdapter();
        }
        catch (TargetInvocationException e)
        {
            throw new SystemException(e.InnerException.Message, e.InnerException);
        }
    }
    //
    public IDbDataAdapter CreateDataAdapter(IDbCommand selectCommand)
    {
        try
        {
            IDbDataAdapter da = CreateDataAdapter();
            da.SelectCommand = selectCommand;
            return da;
        }
        catch (TargetInvocationException e)
        {
            throw new SystemException(e.InnerException.Message, e.InnerException);
        }
    }
    //
    public IDbDataAdapter CreateDataAdapter(string selectCommandText, IDbConnection selectConnection)
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

    #region IDbDataParameter methods
    public IDbDataParameter CreateDataParameter()
    {
        try
        {
#if (!MOBILE)
            return (IDbDataParameter)((DbProviderFactory)_Instance).CreateParameter();
#else
                return (IDbDataParameter)Activator.CreateInstance(_dataParameterTypes[indexProvider]);
#endif
        }
        catch (TargetInvocationException e)
        {
            throw new SystemException(e.InnerException.Message, e.InnerException);
        }
        return null;
    }
    public IDbDataParameter CreateDataParameter(string parameterName, object value)
    {
        try
        {
            IDbDataParameter param = CreateDataParameter();
            param.ParameterName = parameterName;
            param.Value = value;
            return param;
        }
        catch (TargetInvocationException e)
        {
            throw new SystemException(e.InnerException.Message, e.InnerException);
        }
    }
    public IDbDataParameter CreateDataParameter(string parameterName, DbType dataType)
    {
        IDbDataParameter param = CreateDataParameter();
        if (param != null)
        {
            param.ParameterName = parameterName;
            param.DbType = dataType;
        }
        return param;
    }
    public IDbDataParameter CreateDataParameter(string parameterName, DbType dataType, int size)
    {
        IDbDataParameter param = CreateDataParameter();
        if (param != null)
        {
            param.ParameterName = parameterName;
            param.DbType = dataType;
            param.Size = size;
        }
        return param;
    }
    public IDbDataParameter CreateDataParameter(string parameterName, DbType dataType, int size, string sourceColumn)
    {
        IDbDataParameter param = CreateDataParameter();
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
