//namespace Solution.Data;
public class DB
{
    DataManager oData = new();
    public string connectionDefault { get; set; }
    public DB(){}
    public CRUD this[string sKeyDb]
    {
        get
        {
            lock (this)
            {
                return new CRUD(this, sKeyDb);
            }
        }
    }
    public enum enModeConnectionOpen
    {
        Always, Whenever
    }
    private enModeConnectionOpen _ModeConnection = enModeConnectionOpen.Always;
    public enModeConnectionOpen ModeConnection
    {
        get { return _ModeConnection; }
        set { _ModeConnection = value; }
    }

    public DataManager DataManager
    {
        get { return oData; }
    }

    //public cCommander Commander
    //{
    //    get { return oCommander; }
    //}

    public void Open()
    {
        for (int i = 0; i < oData.Connections.Count; i++)
        {
            if (oData.Connections[i].State != ConnectionState.Open)
            {
                try
                {
                    oData.Connections[i].Open();
                    Console.WriteLine(" Aperta ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Errore: " + ex.Message);
                }
                finally
                {
                }
            }
        }
    }
    public void Close()
    {
        for (int i = 0; i < oData.Connections.Count; i++)
        {
            if (oData.Connections[i].State == ConnectionState.Open)
            {
                try
                {
                    oData.Connections[i].Close();
                    Console.WriteLine(" Aperta ");
                }
                catch (Exception ex)
                {
                    //Logger.WriteLine("????????????? DB CLOSING EXCEPTION ????????? = " + oData.Connections[i].Key + "; error=" + ex.Message + "\n" + ex.StackTrace, Logger.TipoLog.Error);
                    Console.WriteLine(" Errore ");
                }
                finally
                {
                }
            }
        }
    }
    public Parameter CreateParameter(string sKey, DbType oType, ParameterDirection oParameterDirection, string sName, object oValue)
    {
        return DataManager.CreateParameter(sKey, oType, oParameterDirection, sName, oValue);
    }
    public DataTable InvokeSQL(string sSQL, params Parameter[] pParams)
    {
        return InvokeSQL(connectionDefault, sSQL, pParams.ToArray());
    }
    public DataTable InvokeSQL(string sKey, string sSQL, params Parameter[] pParams)
    {
        Exception exResult = null;
        DataTable oDT = null;
        Connection oConn = (_ModeConnection == enModeConnectionOpen.Whenever ? oData.Connections.Clone(sKey) : oData.Connections[sKey]);
        try
        {
            if (oConn.State == ConnectionState.Closed)
            {
                try
                {
                    oConn.Open();
                }
                catch (Exception ex)
                {
                    exResult = ex;
                }
                finally
                {
                }
                if (exResult != null)
                {
                    if (_ModeConnection == enModeConnectionOpen.Whenever)
                        oConn.Close();
                    throw (exResult);
                }
            }
            oDT = oData.Invoke(oConn, sSQL, pParams);
        }
        catch (Exception ex)
        {
            exResult = ex;
        }
        finally
        {
            if (_ModeConnection == enModeConnectionOpen.Whenever)
                oConn.Close();
        }
        //
        if (exResult != null)
            throw (exResult);
        return oDT;
    }
    public DataTable Invoke(string sSQL, params Parameter[] pParams)
    { 
        return Invoke(connectionDefault, sSQL, pParams.ToArray());
    }
    public DataTable Invoke(string sKey, string sSQL, params Parameter[] pParams)
    {
        Exception exResult = null;
        DataTable oDT = null;
        Connection oConn = (_ModeConnection == enModeConnectionOpen.Whenever ? oData.Connections.Clone(sKey) : oData.Connections[sKey]);
        try
        {
            if (oConn.State == ConnectionState.Closed)
            {
                try
                {
                    oConn.Open();
                }
                catch (Exception ex)
                {
                    exResult = ex;
                }
                finally
                {
                }
                if (exResult != null)
                {
                    if (_ModeConnection == enModeConnectionOpen.Whenever)
                        oConn.Close();
                    throw (exResult);
                }
            }
            oDT = oData.InvokeStore(oConn, sSQL, pParams);
        }
        catch (Exception ex)
        {
            exResult = ex;
        }
        finally
        {
            if (_ModeConnection == enModeConnectionOpen.Whenever)
                oConn.Close();
        }
        //
        if (exResult != null)
            throw (exResult);
        return oDT;
    }
    public DataTable Invoke(string sSQL)
    {
        return Invoke(connectionDefault, sSQL);
    }
    public DataTable Invoke(string sKey, string sSQL)
    {
        Exception exResult = null;
        DataTable oDT = null;
        Connection oConn = (_ModeConnection == enModeConnectionOpen.Whenever ? oData.Connections.Clone(sKey) : oData.Connections[sKey]);
        try
        {
            if (oConn.State == ConnectionState.Closed)
            {
                try
                {
                    oConn.Open();
                }
                catch (Exception ex)
                {
                    exResult = ex;
                }
                finally
                {
                }
                if (exResult != null)
                {
                    if (_ModeConnection == enModeConnectionOpen.Whenever)
                        oConn.Close();
                    throw (exResult);
                }
            }
            oDT = oData.InvokeStore(oConn, sSQL);
        }
        catch (Exception ex)
        {
            exResult = ex;
        }
        finally
        {
            if (_ModeConnection == enModeConnectionOpen.Whenever)
                oConn.Close();
        }
        //
        if (exResult != null)
            throw (exResult);
        return oDT;
    }
    public DataTable Get(string sSQL, Dictionary<string, object>? parameters = null)
    {
        return Get(connectionDefault, sSQL);
    }
    public DataTable Get(string sKey, string sSQL, Dictionary<string, object>? parameters = null)
    {
        Exception exResult = null;
        DataSet oDS = null;
        Connection oConn = (_ModeConnection == enModeConnectionOpen.Whenever ? oData.Connections.Clone(sKey) : oData.Connections[sKey]);
        try
        {
            if (oConn.State == ConnectionState.Closed)
            {
                try
                {
                    oConn.Open();
                }
                catch (Exception ex)
                {
                    exResult = ex;
                }
                finally
                {
                }
                if (exResult != null)
                {
                    if (_ModeConnection == enModeConnectionOpen.Whenever)
                        oConn.Close();
                    throw (exResult);
                }
            }
            oDS = oData.GetDS(oConn, sSQL, parameters);
        }
        catch (Exception ex)
        {
            exResult = ex;
        }
        finally
        {
            if (_ModeConnection == enModeConnectionOpen.Whenever)
                oConn.Close();
        }
        if (exResult != null)
            throw (exResult);
        if (oDS != null && oDS.Tables.Count > 0)
            return oDS.Tables[0];
        return null;
    }
    public int Execute(string sSQL)
    {
        return Execute(connectionDefault, sSQL);
    }
    public int Execute(string sKey, string sSQL)
    {
        Exception exResult = null;
        int iResult = -1;
        Connection oConn = (_ModeConnection == enModeConnectionOpen.Whenever ? oData.Connections.Clone(sKey) : oData.Connections[sKey]);
        try
        {
            if (oConn.State == ConnectionState.Closed)
            {
                try
                {
                    oConn.Open();
                }
                catch (Exception ex)
                {
                    exResult = ex;
                }
                finally
                {
                }
                if (exResult != null)
                {
                    if (_ModeConnection == enModeConnectionOpen.Whenever)
                        oConn.Close();
                    throw (exResult);
                }
            }
            DateTime before = DateTime.Now;
            iResult = oData.ExecuteNonQuery(oConn, sSQL, null);
            TimeSpan tsDuration = DateTime.Now.Subtract(before);
        }
        catch (Exception ex)
        {
            exResult = ex;
        }
        finally
        {
            if (_ModeConnection == enModeConnectionOpen.Whenever)
                oConn.Close();
        }
        if (exResult != null)
            throw (exResult);
        return iResult;
    }

    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < oData.Connections.Count; i++)
        {
            if (oData.Connections[i].State != ConnectionState.Open)
            {
                try
                {
                    await oData.Connections[i].OpenAsync(cancellationToken);
                    Console.WriteLine(" Aperta ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Errore: " + ex.Message);
                }
            }
        }
    }

    public async Task CloseAsync()
    {
        for (int i = 0; i < oData.Connections.Count; i++)
        {
            if (oData.Connections[i].State == ConnectionState.Open)
            {
                try
                {
                    await oData.Connections[i].CloseAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Errore: " + ex.Message);
                }
            }
        }
    }

    public Task<DataTable?> InvokeSQLAsync(string sSQL, params Parameter[] pParams)
        => InvokeSQLAsync(connectionDefault, sSQL, default, pParams);

    public async Task<DataTable?> InvokeSQLAsync(string sKey, string sSQL, CancellationToken cancellationToken = default, params Parameter[] pParams)
    {
        await using Connection oConn = oData.Connections.Clone(sKey);

        if (oConn.State == ConnectionState.Closed)
            await oConn.OpenAsync(cancellationToken);

        Command oCMD = oData.GetCM(oConn, sSQL);
        foreach (var p in pParams)
            oCMD.Parameters.Add(p.ParameterName, p.Value ?? DBNull.Value);

        await using DbDataReader reader = await oCMD.ExecuteReaderAsync();
        DataTable oDT = new DataTable();
        oDT.Load(reader);
        return oDT;
    }

    public Task<DataTable?> InvokeAsync(string sSQL, params Parameter[] pParams)
        => InvokeAsync(connectionDefault, sSQL, default, pParams);

    public async Task<DataTable?> InvokeAsync(string sKey, string sSQL, CancellationToken cancellationToken = default, params Parameter[] pParams)
    {
        await using Connection oConn = oData.Connections.Clone(sKey);

        if (oConn.State == ConnectionState.Closed)
            await oConn.OpenAsync(cancellationToken);

        Command oCMD = oData.GetCM(oConn, sSQL);
        oCMD.CommandType = CommandType.StoredProcedure;
        foreach (var p in pParams)
        {
            Parameter oP = oData.CreateParameter(oConn, p.DbType, p.Direction, p.ParameterName, p.Value);
            oCMD.Parameters.Add(oP.ParameterName, oP.Value ?? DBNull.Value);
        }

        await using DbDataReader reader = await oCMD.ExecuteReaderAsync();
        DataTable oDT = new DataTable();
        oDT.Load(reader);
        return oDT;
    }

    public Task<DataTable?> InvokeAsync(string sSQL)
        => InvokeAsync(connectionDefault, sSQL);

    public async Task<DataTable?> InvokeAsync(string sKey, string sSQL, CancellationToken cancellationToken = default)
    {
        await using Connection oConn = oData.Connections.Clone(sKey);

        if (oConn.State == ConnectionState.Closed)
            await oConn.OpenAsync(cancellationToken);

        Command oCMD = oData.GetCM(oConn, sSQL);
        oCMD.CommandType = CommandType.StoredProcedure;

        await using DbDataReader reader = await oCMD.ExecuteReaderAsync();
        DataTable oDT = new DataTable();
        oDT.Load(reader);
        return oDT;
    }

    public Task<DataTable?> GetAsync(string sSQL, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
        => GetAsync(connectionDefault, sSQL, parameters, cancellationToken);

    public async Task<DataTable?> GetAsync(string sKey, string sSQL, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        await using Connection oConn = oData.Connections.Clone(sKey);

        if (oConn.State == ConnectionState.Closed)
            await oConn.OpenAsync(cancellationToken);

        Command oCMD = oData.GetCM(oConn, sSQL, parameters);
        await using DbDataReader reader = await oCMD.ExecuteReaderAsync();
        DataTable oDT = new DataTable();
        oDT.Load(reader);
        return oDT;
    }

    public Task<int> ExecuteAsync(string sSQL, CancellationToken cancellationToken = default)
        => ExecuteAsync(connectionDefault, sSQL, cancellationToken);

    public async Task<int> ExecuteAsync(string sKey, string sSQL, CancellationToken cancellationToken = default)
    {
        await using Connection oConn = oData.Connections.Clone(sKey);

        if (oConn.State == ConnectionState.Closed)
            await oConn.OpenAsync(cancellationToken);

        Command oCMD = oData.GetCM(oConn, sSQL);
        return await oCMD.ExecuteNonQueryAsync(cancellationToken);
    }
}


