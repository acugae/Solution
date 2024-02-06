using NPOI.SS.Formula.Functions;

namespace Solution.Data;

public class cDB
{
    cDataManager oData = new cDataManager();
    public cModelConfiguration Configuration { get; }
    ~cDB(){}
    //
    public cDB(cModelConfiguration oConfiguration)
    {
        ModeConnection = enModeConnectionOpen.Whenever;
        Configuration = oConfiguration;
        foreach (var conn in Configuration.Connections)
        {
            oData.Connections.Add(conn.Key, conn.Value.Provider, conn.Value.Connection);
        }
    }
    public cCRUD this[string sKeyDb]
    {
        get
        {
            lock (this)
            {
                return new cCRUD(this, sKeyDb);
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

    public cDataManager DataManager
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
    //
    public void Close()
    {
        //cLogger.WriteLine("????????????? cDB TRY TO CLOSE CONNECTIONS ?????????", cLogger.TipoLog.Debug);
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
                    cLogger.WriteLine("????????????? cDB CLOSING EXCEPTION ????????? = " + oData.Connections[i].Key + "; error=" + ex.Message + "\n" + ex.StackTrace, cLogger.TipoLog.Error);
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

    public cGCollection<string, DataTable> GetSchemas()
    {
        string sSQL = "SELECT ceic_contextCRM FROM syint_Map GROUP BY ceic_contextCRM";
        DataTable oDT = Get(Configuration.InfrastructureConnection, sSQL);
        cGCollection<string, DataTable> oS = new cGCollection<string, DataTable>();
        for (int i = 0; i < oDT.Rows.Count; i++)
            oS.Add(oDT.Rows[i]["ceic_contextCRM"].ToString(), GetSchema(oDT.Rows[i][0].ToString()));
        return oS;
    }
    public DataTable GetQueryExecute(string sCode, cParameter[] oParams = null, bool bOnlyVerified = true)
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
                List<cParameter> oParamsTmp = new List<cParameter>();
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
    /// <summary>
    /// Esegue script presenti in syint_Query
    /// </summary>
    public DataTable GetQueryExecute(string sCode, cGCollection<string, object> oParams = null, bool bOnlyVerified = false)
    {
        try
        {
            DataTable result = new DataTable();
            //
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
                    for (int i = 0; oParams != null && i < oParams.Count; i++)
                        Query = Query.Replace(oParams.GetKey(i), oParams.GetValue(i) == null ? "null" : oParams.GetValue(i).ToString());
                    result = this.Get(DbKey, Query);
                }
            }
            else if (Type == 1)
            {
                List<cParameter> oParamsPar = new List<cParameter>();
                List<cParameter> oParamsTmp = new List<cParameter>();
                for (int i = 0; oParams != null && i < oParams.Count; i++)
                    oParamsPar.Add(CreateParameter(DbKey, DbType.String, ParameterDirection.Input, oParams.GetKey(i), oParams.GetValue(i) == null ? DBNull.Value : oParams.GetValue(i)));
                //
                if (bOnlyVerified)
                {
                    DataTable oDTParams = this.Get(DbKey, "SELECT p.name AS Parameter, t.name AS [Type] FROM sys.procedures sp JOIN sys.parameters p ON sp.object_id = p.object_id JOIN sys.types t ON p.system_type_id = t.system_type_id WHERE sp.name = '" + Query + "'");
                    for (int i = 0; i < oParamsPar.Count; i++)
                    {
                        DataRow[] oDRParam = oDTParams.Select("Parameter = '" + oParamsPar[i].ParameterName + "'");
                        if (oDRParam != null && oDRParam.Length > 0)
                        {
                            oParamsTmp.Add(oParamsPar[i]);
                        }
                    }
                    result = this.Invoke(DbKey, Query, oParamsTmp.ToArray());
                }
                else
                {
                    result = this.Invoke(DbKey, Query, oParamsTmp.ToArray());
                }

            }
            return result;
        }
        catch
        {
            return new DataTable();
        }
    }


    public DataTable GetQueryExecute(string sCode, params string[] svKey_Value)
    {
        try
        {
            DataTable result = new DataTable();
            if (svKey_Value != null && (svKey_Value.Length % 2 == 1))
                return null;
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
                    for (int i = 0; i < svKey_Value.Length; i += 2)
                    {
                        Query = Query.Replace(svKey_Value[i].ToString(), svKey_Value[i + 1]);
                    }
                    result = this.Get(DbKey, Query);
                }
            }
            else if (Type == 1)
            {
                result = Invoke(DbKey, Query);
                //result = DataManager.InvokeStore(DbKey, Query);
            }
            return result;
        }
        catch
        {
            return new DataTable();
        }
    }

    public int SetQuery(string sCode, string sName, string sScript, string sKeyDB, int iType, int iActive, string sOnLoad)
    {
        string sql = "";

        DataTable oDT = this.Get(Configuration.InfrastructureConnection, "SELECT qu_codice FROM syint_Query WHERE qu_codice = '" + sCode + "'");
        if (oDT == null || oDT.Rows.Count == 0)
        {
            sql = "INSERT INTO [syint_Query] ([qu_codice],[qu_name],[qu_script],[qu_connectionkey],[qu_type],[qu_active],[qu_eventLoad]) VALUES (";
            sql += "'" + sCode + "',";
            sql += "'" + sName + "',";
            sql += "'" + PreparaSql(sScript) + "',";
            sql += "'" + sKeyDB + "',";
            sql += iType.ToString() + ",";
            sql += iActive.ToString() + ",";
            sql += "'" + PreparaSql(sOnLoad) + "'";
            sql += ")";
        }
        else
        {
            sql = "UPDATE [syint_Query] " +
                    " SET [qu_name] = '" + PreparaSql(sName) + "' " +
                    "    ,[qu_script] = '" + PreparaSql(sScript) + "' " +
                    "    ,[qu_connectionkey] = '" + sKeyDB + "' " +
                    "    ,[qu_type] = " + iType.ToString() + " " +
                    "    ,[qu_active] = " + iActive.ToString() + " " +
                    "    ,[qu_eventLoad] = '" + PreparaSql(sOnLoad) + "' " +
                    "  WHERE qu_codice = '" + sCode + "'";
        }
        return this.Execute(Configuration.InfrastructureConnection, sql);
    }

    public int SetQuery(string sCode, string sName, string sScript, string sKeyDB, int iType, int iActive, string sCheckedKey, string sCheckedName, string sOnLoad)
    {
        string sql = "";

        DataTable oDT = this.Get(Configuration.InfrastructureConnection, "SELECT qu_codice FROM syint_Query WHERE qu_codice = '" + sCode + "'");
        if (oDT == null || oDT.Rows.Count == 0)
        {
            sql = "INSERT INTO [syint_Query] ([qu_codice],[qu_name],[qu_script],[qu_connectionkey],[qu_type],[qu_active],[qu_checkedkey],[qu_checkedname],[qu_eventLoad]) VALUES (";
            sql += "'" + sCode + "',";
            sql += "'" + sName + "',";
            sql += "'" + PreparaSql(sScript) + "',";
            sql += "'" + sKeyDB + "',";
            sql += iType.ToString() + ",";
            sql += iActive.ToString() + ",";
            sql += "'" + sCheckedKey + "',";
            sql += "'" + sCheckedName + "',";
            sql += "'" + PreparaSql(sOnLoad) + "'";
            sql += ")";
        }
        else
        {
            sql = "UPDATE [syint_Query] " +
                    " SET [qu_name] = '" + PreparaSql(sName) + "' " +
                    "    ,[qu_script] = '" + PreparaSql(sScript) + "' " +
                    "    ,[qu_connectionkey] = '" + sKeyDB + "' " +
                    "    ,[qu_type] = " + iType.ToString() + " " +
                    "    ,[qu_active] = " + iActive.ToString() + " " +
                    "    ,[qu_checkedkey] = '" + sCheckedKey + "' " +
                    "    ,[qu_checkedname] = '" + sCheckedName + "' " +
                    "    ,[qu_eventLoad] = '" + PreparaSql(sOnLoad) + "' " +
                    "  WHERE qu_codice = '" + sCode + "'";
        }
        return this.Execute(Configuration.InfrastructureConnection, sql);
    }

    public int SetQueryLink(string sCode, string sName, string sType, string sUrl, string sParams, int iOrder, int iVisible, int iChecked)
    {
        string sql = "";
        DataTable oDT = this.Get(Configuration.InfrastructureConnection, "SELECT qu_id FROM [syint_Query] WHERE qu_codice = '" + sCode + "'");
        if (oDT == null || oDT.Rows.Count == 0)
            throw new Exception("Query non trovata");
        //
        DataTable oDTLinks = this.Get(Configuration.InfrastructureConnection, "SELECT * FROM [syint_QueryLinks] WHERE ql_idQuery = " + oDT.Rows[0]["qu_id"].ToString() + " AND ql_name = '" + sName + "'");
        DataRow oDR = oDT.Rows[0];
        if (oDTLinks == null || oDTLinks.Rows.Count == 0)
        {
            sql = "INSERT INTO [syint_QueryLinks] ([ql_idQuery],[ql_type],[ql_name],[ql_url],[ql_params],[ql_order],[ql_visible],[ql_checked]) VALUES (";
            sql += oDR["qu_id"].ToString() + ",";
            sql += "'" + sType + "',";
            sql += "'" + sName + "',";
            sql += sUrl != null ? "'" + PreparaSql(sUrl) + "'," : "null,";
            sql += sParams != null ? "'" + sParams + "'," : "null,";
            sql += iOrder.ToString() + ",";
            sql += iVisible.ToString() + ",";
            sql += iChecked.ToString();
            sql += ")";
        }
        else
        {
            sql = "UPDATE [syint_QueryLinks] " +
                    " SET [ql_type] = '" + sType + "' " +
                     (sUrl != null ? " ,[ql_url] = '" + PreparaSql(sUrl) + "' " : " ,[ql_url] = null ") +
                     (sParams != null ? " ,[ql_params] = '" + sParams + "' " : " ,[ql_params] = null ") +
                    "    ,[ql_order] = " + iOrder.ToString() + " " +
                    "    ,[ql_visible] = " + iVisible.ToString() + " " +
                    "    ,[ql_checked] = " + iChecked.ToString() + " " +
                    "    ,[ql_date] = getdate()" +
                    "  WHERE ql_idQuery = " + oDR["qu_id"].ToString() + " AND ql_name = '" + sName + "'";
        }
        return this.Execute(Configuration.InfrastructureConnection, sql);
    }

    public int SetQueryParams(string sCode, string sName, string sDescri, int iOrder, string sType, string sControl, string sSource, int iAllowNull, string sReplace)
    {
        string sql = "";
        DataTable oDT = this.Get(Configuration.InfrastructureConnection, "SELECT qu_id FROM [syint_Query] WHERE qu_codice = '" + sCode + "'");
        if (oDT == null || oDT.Rows.Count == 0)
            throw new Exception("Query non trovata");
        //
        DataTable oDTParams = this.Get(Configuration.InfrastructureConnection, "SELECT * FROM [syint_QueryParams] WHERE qp_idQuery = " + oDT.Rows[0]["qu_id"].ToString() + " AND qp_name = '" + sName + "'");
        DataRow oDR = oDT.Rows[0];
        if (oDTParams == null || oDTParams.Rows.Count == 0)
        {
            sql = "INSERT INTO [syint_QueryParams] ([qp_idQuery] ,[qp_name] ,[qp_descri] ,[qp_order] ,[qp_type] ,[qp_control] ,[qp_source] ,[qp_allownull] ,[qp_replace]) VALUES (";
            sql += oDR["qu_id"].ToString() + ",";
            sql += "'" + sName + "',";
            sql += "'" + PreparaSql(sDescri) + "',";
            sql += iOrder.ToString() + ",";
            sql += "'" + sType + "',";
            sql += "'" + sControl + "',";
            sql += sSource != null ? "'" + sSource + "'," : "null,";
            sql += iAllowNull.ToString() + ",";
            sql += sReplace != null ? "'" + sReplace + "'" : "null";
            sql += ")";
        }
        else
        {
            sql = "UPDATE [syint_QueryParams] " +
                    " SET [qp_name] = '" + sName + "' " +
                    "    ,[qp_descri] = '" + PreparaSql(sDescri) + "' " +
                    "    ,[qp_order] = " + iOrder.ToString() + " " +
                    "    ,[qp_type] = '" + sType + "' " +
                    "    ,[qp_control] = '" + sControl + "' " +
                    (sSource != null ? " ,[qp_source] = '" + sSource + "' " : " ,[qp_source] = null ") +
                    "    ,[qp_allownull] = " + iAllowNull.ToString() + " " +
                    "    ,[qp_date] = getdate()" +
                    (sReplace != null ? " ,[qp_replace] = '" + sReplace + "' " : " , [qp_replace] = null ") +
                    "  WHERE qp_idQuery = " + oDR["qu_id"].ToString() + " AND qp_name = '" + sName + "'";
        }
        return this.Execute(Configuration.InfrastructureConnection, sql);
    }

    public int DelQuery(string sCode)
    {
        DataTable oDT = this.Get(Configuration.InfrastructureConnection, "SELECT qu_id FROM [syint_Query] WHERE qu_codice = '" + sCode + "'");
        if (oDT != null && oDT.Rows.Count > 0)
        {
            this.Execute(Configuration.InfrastructureConnection, "DELETE FROM [syint_QueryParams] WHERE qp_idQuery = " + oDT.Rows[0]["qu_id"].ToString());
            this.Execute(Configuration.InfrastructureConnection, "DELETE FROM [syint_QueryLinks]  WHERE ql_idQuery = " + oDT.Rows[0]["qu_id"].ToString());
            return this.Execute(Configuration.InfrastructureConnection, "DELETE FROM [syint_Query] WHERE qu_id = " + oDT.Rows[0]["qu_id"].ToString());
        }

        return 0;

    }

    public int DelQueryParams(string sCode, string sName)
    {
        DataTable oDT = this.Get(Configuration.InfrastructureConnection, "SELECT qu_id FROM [syint_Query] WHERE qu_codice = '" + sCode + "'");
        if (oDT != null && oDT.Rows.Count > 0)
        {
            return this.Execute(Configuration.InfrastructureConnection, "DELETE FROM [syint_QueryParams] WHERE qp_idQuery = " + oDT.Rows[0]["qu_id"].ToString() + " AND qp_name = '" + sName + "'");
        }
        return 0;
    }

    public string PreparaSql(string sql)
    {
        string sqlPreparato = sql;
        try
        {
            sqlPreparato = sqlPreparato.Replace("'", "''");
            string nl = System.Environment.NewLine;
            sqlPreparato = sqlPreparato.Replace(nl, "\r\n");
        }
        catch
        {

        }
        return sqlPreparato;
    }

    public DataRow GetQuery(string sCode)
    {
        try
        {
            DataTable result = new DataTable();
            string sSQL = " SELECT top 1 qu_id,qu_name,qu_script,qu_connectionkey,qu_type,qu_active FROM syint_Query " +
                          " where LTRIM(RTRIM(qu_codice)) = LTRIM(RTRIM('" + sCode + "')) order by qu_id desc";
            DataTable oDT = this.Get(Configuration.InfrastructureConnection, sSQL);
            return oDT.Rows[0];
        }
        catch
        {
            return null;
        }
    }

    public DataTable GetQueryExecute(DataRow drQuery)
    {
        try
        {
            DataTable result = new DataTable();
            string DbKey = drQuery["qu_connectionkey"].ToString();
            string Query = drQuery["qu_script"].ToString();
            int Type = int.Parse(drQuery["qu_type"].ToString());
            if (Type == 0)
            {
                if (Query != null)
                {
                    result = this.Get(DbKey, Query);
                }
            }
            else if (Type == 1)
            {
                //result = DataManager.InvokeStore(DbKey, Query);
                result = Invoke(DbKey, Query);
            }
            return result;
        }
        catch
        {
            return new DataTable();
        }
    }


    public DataTable GetQueryExecute(string sCode)
    {
        //try
        //{
        DataTable result = new DataTable();
        string sSQL = " SELECT top 1 qu_id,qu_name,qu_script,qu_connectionkey,qu_type,qu_active FROM syint_Query " +
                      " where LTRIM(RTRIM(qu_codice)) = LTRIM(RTRIM('" + sCode + "')) order by qu_id desc";
        DataTable oDT = this.Get(Configuration.InfrastructureConnection, sSQL);
        DataRow row = oDT.Rows[0];
        string DbKey = row["qu_connectionkey"].ToString();
        string Query = row["qu_script"].ToString();
        int Type = int.Parse(row["qu_type"].ToString());
        if (Type == 0)
        {
            if (Query != null)
            {
                result = this.Get(DbKey, Query);
            }
        }
        else if (Type == 1)
        {
            //result = DataManager.InvokeStore(DbKey, Query);
            result = Invoke(DbKey, Query);
        }
        return result;
        //}
        //catch
        //{
        //    return new DataTable();
        //}
    }
    public cParameter CreateParameter(string sKey, DbType oType, ParameterDirection oParameterDirection, string sName, object oValue)
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

    public DataTable InvokeSQL(string sKey, string sSQL, params cParameter[] pParams)
    {
        Exception exResult = null;
        DataTable oDT = null;
        cConnection oConn = (_ModeConnection == enModeConnectionOpen.Whenever ? oData.Connections.Clone(sKey) : oData.Connections[sKey]);
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

    public DataTable Invoke(string sKey, string sSQL, params cParameter[] pParams)
    {
        Exception exResult = null;
        DataTable oDT = null;
        cConnection oConn = (_ModeConnection == enModeConnectionOpen.Whenever ? oData.Connections.Clone(sKey) : oData.Connections[sKey]);
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
        cConnection oConn = (_ModeConnection == enModeConnectionOpen.Whenever ? oData.Connections.Clone(sKey) : oData.Connections[sKey]);
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
    public DataTable Get(string sKey, string sSQL, cCache<DataTable> DBCache, int cacheThreeshold)
    {
        cCacheKey cacheKey = new cCacheKey(sKey + "_" + sSQL);
        DataTable oDT = DBCache.get(cacheKey);
        if (oDT == null)
        {
            oDT = Get(sKey, sSQL);
            cCacheValue<DataTable> cacheValue = new cCacheValue<DataTable>(oDT, cacheThreeshold);
            DBCache.Add(cacheKey, cacheValue);
        }
        return oDT;
    }
    public DataTable Get(string sKey, string sSQL)
    {
        Exception exResult = null;
        DataSet oDS = null;
        cConnection oConn = (_ModeConnection == enModeConnectionOpen.Whenever ? oData.Connections.Clone(sKey) : oData.Connections[sKey]);
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
        cConnection oConn = (_ModeConnection == enModeConnectionOpen.Whenever ? oData.Connections.Clone(sKey) : oData.Connections[sKey]);
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
