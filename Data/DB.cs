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
    //public DataTable GetSchema(string sEntityName)
    //{
    //    return Get(Configuration.InfrastructureConnection, "SELECT * FROM syint_Map (NOLOCK) where ceic_contextCRM = '" + sEntityName + "'");
    //}

    //public GCollection<string, DataTable> GetSchemas()
    //{
    //    string sSQL = "SELECT ceic_contextCRM FROM syint_Map GROUP BY ceic_contextCRM";
    //    DataTable oDT = Get(Configuration.InfrastructureConnection, sSQL);
    //    GCollection<string, DataTable> oS = new GCollection<string, DataTable>();
    //    for (int i = 0; i < oDT.Rows.Count; i++)
    //        oS.Add(oDT.Rows[i]["ceic_contextCRM"].ToString(), GetSchema(oDT.Rows[i][0].ToString()));
    //    return oS;
    //}
    public Parameter CreateParameter(string sKey, DbType oType, ParameterDirection oParameterDirection, string sName, object oValue)
    {
        return DataManager.CreateParameter(sKey, oType, oParameterDirection, sName, oValue);
    }
    //public int InsertLog(int idParent, int iReference, string sSource, string sCategory, string sMessage, string sQueue = null)
    //{
    //    Console.WriteLine(sMessage);
    //    sCategory = sCategory.Replace("'", "''");
    //    sMessage = sMessage.Replace("'", "''");
    //    string sInsert = "";
    //    if (sQueue == null)
    //        sInsert = string.Format("insert into syint_Logs (log_idParent, log_reference, log_source, log_category, log_message) values ({0}, {1}, '{2}', '{3}', '{4}'); SELECT SCOPE_IDENTITY() AS [log_id];", idParent, iReference, sSource, sCategory, sMessage);
    //    else
    //        sInsert = string.Format("insert into syint_Logs (log_idParent, log_reference, log_source, log_category, log_message, log_queue) values ({0}, {1}, '{2}', '{3}', '{4}', '{5}'); SELECT SCOPE_IDENTITY() AS [log_id];", idParent, iReference, sSource, sCategory, sMessage, sQueue);
    //    //
    //    DataTable oDT = Get(Configuration.InfrastructureConnection, sInsert);
    //    return Convert.ToInt32(oDT.Rows[0]["log_id"]);
    //}
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
    //public DataTable Get(string sKey, string sSQL, Cache<DataTable> DBCache, int cacheThreeshold)
    //{
    //    CacheKey cacheKey = new CacheKey(sKey + "_" + sSQL);
    //    DataTable oDT = DBCache.get(cacheKey);
    //    if (oDT == null)
    //    {
    //        oDT = Get(sKey, sSQL);
    //        CacheValue<DataTable> cacheValue = new CacheValue<DataTable>(oDT, cacheThreeshold);
    //        DBCache.Add(cacheKey, cacheValue);
    //    }
    //    return oDT;
    //}
    public DataTable Get(string sSQL)
    {
        return Get(connectionDefault, sSQL);
    }
    public DataTable Get(string sKey, string sSQL)
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
            oDS = oData.GetDS(oConn, sSQL);
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

    //public DataTable ExecuteCommand(string sKey, string sSQLName, params string[] oParams)
    //{
    //    Exception exResult = null;
    //    DataTable oDT = null;
    //    cConnection oConn = (_ModeConnection == enModeConnectionOpen.Whenever ? oData.Connections.Clone(sKey) : oData.Connections[sKey]);
    //    try
    //    {
    //        if (oData.Connections[sKey].State == ConnectionState.Closed)
    //        {
    //            try
    //            {
    //                oConn.Open();
    //            }
    //            catch (Exception ex)
    //            {
    //                exResult = ex;
    //            }
    //            finally
    //            {
    //            }
    //            if (exResult != null)
    //            {
    //                if (_ModeConnection == enModeConnectionOpen.Whenever)
    //                    oConn.Close();
    //                throw (exResult);
    //            }
    //        }
    //        if (oParams.Length > 0)
    //            oDT = Commander.Execute(oConn, sSQLName, oParams.ToArray());
    //        else
    //            oDT = Commander.Execute(oConn, sSQLName);
    //    }
    //    catch (Exception ex)
    //    {
    //        exResult = ex;
    //    }
    //    if (_ModeConnection == enModeConnectionOpen.Whenever)
    //        oConn.Close();
    //    if (exResult != null)
    //        throw exResult;
    //    if (oDT != null && oDT.Rows.Count > 0)
    //        return oDT;
    //    return null;
    //}



    //public Dictionary<string, cMap> GetObjectMap(string sCode)
    //{
    //    DataTable oDTMaps = Get(Configuration.InfrastructureConnection, "SELECT * FROM syint_ObjectMap WHERE om_code = '" + sCode + "'");
    //    if (oDTMaps != null)
    //        return oDTMaps.AsEnumerable().ToDictionary<DataRow, string, cMap>(row => row.Field<string>("om_source"), row => new cMap(row.Field<string>("om_target"), row.Field<string>("om_targetType")));
    //    return null;
    //}



    //public MemoryStream GetBytesByDB(string sAssemblyName)
    //{
    //    DataTable oDT = Get(Configuration.InfrastructureConnection, "SELECT * FROM syint_Assembly WHERE as_active = 1 AND as_name = '" + sAssemblyName + "'");
    //    if (oDT.Rows.Count > 0)
    //        return new MemoryStream((byte[])oDT.Rows[0]["as_binary"]);
    //    return null;
    //}
}


