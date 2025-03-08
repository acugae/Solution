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
            DateTime before = DateTime.Now;
            oDT = oData.Invoke(oConn, sSQL, pParams);
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
            DateTime before = DateTime.Now;
            oDT = oData.InvokeStore(oConn, sSQL, pParams);
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
            DateTime before = DateTime.Now;
            oDT = oData.InvokeStore(oConn, sSQL);
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
            DateTime before = DateTime.Now;
            oDS = oData.GetDS(oConn, sSQL, parameters);
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
}


