﻿namespace Solution.Data;
public class DB
{
    DataManager oData = new DataManager();
    public Configuration Configuration { get; }
    ~DB(){}
    //
    public DB(Configuration oConfiguration)
    {
        //ModeConnection = enModeConnectionOpen.Whenever;
        Configuration = oConfiguration;
        foreach (var conn in Configuration.Connections)
        {
            oData.Connections.Add(conn.Key, conn.Value.Provider, conn.Value.Connection);
        }
    }
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
                    Logger.WriteLine("????????????? DB CLOSING EXCEPTION ????????? = " + oData.Connections[i].Key + "; error=" + ex.Message + "\n" + ex.StackTrace, Logger.TipoLog.Error);
                    Console.WriteLine(" Errore ");
                }
                finally
                {
                }
            }
        }
    }
    public DataTable GetSchema(string sEntityName)
    {
        return Get(Configuration.InfrastructureConnection, "SELECT * FROM syint_Map (NOLOCK) where ceic_contextCRM = '" + sEntityName + "'");
    }

    public GCollection<string, DataTable> GetSchemas()
    {
        string sSQL = "SELECT ceic_contextCRM FROM syint_Map GROUP BY ceic_contextCRM";
        DataTable oDT = Get(Configuration.InfrastructureConnection, sSQL);
        GCollection<string, DataTable> oS = new GCollection<string, DataTable>();
        for (int i = 0; i < oDT.Rows.Count; i++)
            oS.Add(oDT.Rows[i]["ceic_contextCRM"].ToString(), GetSchema(oDT.Rows[i][0].ToString()));
        return oS;
    }
    public DataTable GetQueryExecute(string sCode, Parameter[]? oParams = null, bool bOnlyVerified = true)
    {
        try
        {
            DataTable result = new DataTable();
            string sSQL = " SELECT top 1 qu_id,qu_name,qu_script,qu_connectionkey,qu_type,qu_active FROM syint_Query WHERE LTRIM(RTRIM(qu_codice)) = LTRIM(RTRIM('" + sCode + "')) order by qu_id desc";
            DataTable oDT = this.Get(Configuration.InfrastructureConnection, sSQL);
            DataRow row = oDT.Rows[0];
            string DbKey = row["qu_connectionkey"].ToString();
            string Query = row["qu_script"].ToString();
            int Type = int.Parse(row["qu_type"].ToString());
            if (Type == 0)
            {
                if (Query != null)
                {
                    for (int i = 0; i < oParams.Length; i++)
                        Query = Query.Replace(oParams[i].ParameterName, oParams[i].Value.ToString());
                    result = this.Get(DbKey, Query);
                }
            }
            else if (Type == 1)
            {
                List<Parameter> oParamsTmp = new List<Parameter>();
                if (bOnlyVerified && oParams != null)
                {
                    DataTable oDTParams = this.Get(DbKey, "SELECT p.name AS Parameter, t.name AS [Type] FROM sys.procedures sp JOIN sys.parameters p ON sp.object_id = p.object_id JOIN sys.types t ON p.system_type_id = t.system_type_id WHERE sp.name = 'sp_GEO_getEsiti_Agente_Target'");
                    for (int i = 0; i < oParams.Length; i++)
                    {
                        DataRow[] oDRParam = oDTParams.Select("Parameter = '" + oParams[i].ParameterName + "'");
                        if (oDRParam != null && oDRParam.Length > 0)
                        {
                            oParamsTmp.Add(oParams[i]);
                        }
                    }
                    result = this.Invoke(DbKey, Query, oParamsTmp.ToArray());
                }
                else
                {
                    result = this.Invoke(DbKey, Query, oParams);
                }

            }
            return result;
        }
        catch
        {
            return new DataTable();
        }
    }


    public Parameter CreateParameter(string sKey, DbType oType, ParameterDirection oParameterDirection, string sName, object oValue)
    {
        return DataManager.CreateParameter(sKey, oType, oParameterDirection, sName, oValue);
    }
    public int InsertLog(int idParent, int iReference, string sSource, string sCategory, string sMessage, string sQueue = null)
    {
        Console.WriteLine(sMessage);
        sCategory = sCategory.Replace("'", "''");
        sMessage = sMessage.Replace("'", "''");
        string sInsert = "";
        if (sQueue == null)
            sInsert = string.Format("insert into syint_Logs (log_idParent, log_reference, log_source, log_category, log_message) values ({0}, {1}, '{2}', '{3}', '{4}'); SELECT SCOPE_IDENTITY() AS [log_id];", idParent, iReference, sSource, sCategory, sMessage);
        else
            sInsert = string.Format("insert into syint_Logs (log_idParent, log_reference, log_source, log_category, log_message, log_queue) values ({0}, {1}, '{2}', '{3}', '{4}', '{5}'); SELECT SCOPE_IDENTITY() AS [log_id];", idParent, iReference, sSource, sCategory, sMessage, sQueue);
        //
        DataTable oDT = Get(Configuration.InfrastructureConnection, sInsert);
        return Convert.ToInt32(oDT.Rows[0]["log_id"]);
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
    public DataTable Get(string sKey, string sSQL, Cache<DataTable> DBCache, int cacheThreeshold)
    {
        CacheKey cacheKey = new CacheKey(sKey + "_" + sSQL);
        DataTable oDT = DBCache.get(cacheKey);
        if (oDT == null)
        {
            oDT = Get(sKey, sSQL);
            CacheValue<DataTable> cacheValue = new CacheValue<DataTable>(oDT, cacheThreeshold);
            DBCache.Add(cacheKey, cacheValue);
        }
        return oDT;
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



    public Dictionary<string, cMap> GetObjectMap(string sCode)
    {
        DataTable oDTMaps = Get(Configuration.InfrastructureConnection, "SELECT * FROM syint_ObjectMap WHERE om_code = '" + sCode + "'");
        if (oDTMaps != null)
            return oDTMaps.AsEnumerable().ToDictionary<DataRow, string, cMap>(row => row.Field<string>("om_source"), row => new cMap(row.Field<string>("om_target"), row.Field<string>("om_targetType")));
        return null;
    }


    public string GetValueString(string sValue)
    {
        return sValue == null ? "null" : "'" + sValue.Replace("'", "''") + "'";
    }
    public MemoryStream GetBytesByDB(string sAssemblyName)
    {
        DataTable oDT = Get(Configuration.InfrastructureConnection, "SELECT * FROM syint_Assembly WHERE as_active = 1 AND as_name = '" + sAssemblyName + "'");
        if (oDT.Rows.Count > 0)
            return new MemoryStream((byte[])oDT.Rows[0]["as_binary"]);
        return null;
    }
}

public class cMap
{
    public string sTarget = "";
    public string sType = "";
    public cMap(string target, string type) { sTarget = target; sType = type; }
}
