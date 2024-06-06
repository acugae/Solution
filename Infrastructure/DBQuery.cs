using SkiaSharp;

namespace Solution.Infrastructure;
public class DBQuery
{
    readonly DB DB;
    public DB Base => DB;
    public DBQuery(Configuration oConfiguration) => DB = new(oConfiguration);
    public DBQuery(DB oDB) => DB = oDB;

    public DataTable GetQueryExecute(string sCode, Dictionary<string, object> oParams = null)
    {
        try
        {
            DataTable result = new DataTable();
            //
            DataRow row = GetQuery(sCode);
            string DbKey = row["qu_connectionkey"].ToString();
            string Query = row["qu_script"].ToString();
            int Type = int.Parse(row["qu_type"].ToString());
            if (Type == 0) // Query
            {
                if (Query != null)
                {
                    foreach(var param in oParams)
                        Query = Query.Replace(param.Key, param.Value == null ? "null" : param.Value.ToString());
                    result = DB.Get(DbKey, Query);
                }
            }
            else if (Type == 1) // Stored procedure
            {
                List<Parameter> oParamsTmp = new List<Parameter>();
                foreach (var param in oParams)
                    oParamsTmp.Add(DB.CreateParameter(DbKey, DbType.String, ParameterDirection.Input, param.Key, param.Value ?? DBNull.Value));
                result = DB.Invoke(DbKey, Query, oParamsTmp.ToArray());
            }
            return result;
        }
        catch
        {
            return new DataTable();
        }
    }
    public DataTable GetQueryExecute(string sCode, GCollection<string, object> oParams = null, bool bOnlyVerified = false)
    {
        try
        {
            DataTable result = new DataTable();
            //
            string sSQL = " SELECT top 1 qu_id,qu_name,qu_script,qu_connectionkey,qu_type,qu_active FROM syint_Query WHERE LTRIM(RTRIM(qu_codice)) = LTRIM(RTRIM('" + sCode + "')) order by qu_id desc";
            DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, sSQL);
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
                    result = DB.Get(DbKey, Query);
                }
            }
            else if (Type == 1)
            {
                List<Parameter> oParamsPar = new List<Parameter>();
                List<Parameter> oParamsTmp = new List<Parameter>();
                for (int i = 0; oParams != null && i < oParams.Count; i++)
                    oParamsPar.Add(DB.CreateParameter(DbKey, DbType.String, ParameterDirection.Input, oParams.GetKey(i), oParams.GetValue(i) == null ? DBNull.Value : oParams.GetValue(i)));
                //
                if (bOnlyVerified)
                {
                    DataTable oDTParams = DB.Get(DbKey, "SELECT p.name AS Parameter, t.name AS [Type] FROM sys.procedures sp JOIN sys.parameters p ON sp.object_id = p.object_id JOIN sys.types t ON p.system_type_id = t.system_type_id WHERE sp.name = '" + Query + "'");
                    for (int i = 0; i < oParamsPar.Count; i++)
                    {
                        DataRow[] oDRParam = oDTParams.Select("Parameter = '" + oParamsPar[i].ParameterName + "'");
                        if (oDRParam != null && oDRParam.Length > 0)
                        {
                            oParamsTmp.Add(oParamsPar[i]);
                        }
                    }
                    result = DB.Invoke(DbKey, Query, oParamsTmp.ToArray());
                }
                else
                {
                    result = DB.Invoke(DbKey, Query, oParamsTmp.ToArray());
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
            DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, sSQL);
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
                    result = DB.Get(DbKey, Query);
                }
            }
            else if (Type == 1)
            {
                result = DB.Invoke(DbKey, Query);
                //result = DataManager.InvokeStore(DbKey, Query);
            }
            return result;
        }
        catch
        {
            return new DataTable();
        }
    }
    public DataTable GetQueryLinks(string sCode)
    {
        return DB.Get(DB.Configuration.InfrastructureConnection, "select syint_QueryLinks.* from syint_QueryLinks inner join syint_Query ON qu_id = ql_idQuery where qu_codice = '" + sCode + "'");
    }
    public DataTable GetQueryParams(string sCode)
    {
        return DB.Get(DB.Configuration.InfrastructureConnection, "select syint_QueryParams.* from syint_QueryParams inner join syint_Query ON qu_id = qp_idQuery where qu_codice = '" + sCode + "'");
    }
    public int DelQuery(string sCode)
    {
        DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, "SELECT qu_id FROM [syint_Query] WHERE qu_codice = '" + sCode + "'");
        if (oDT != null && oDT.Rows.Count > 0)
        {
            DB.Execute(DB.Configuration.InfrastructureConnection, "DELETE FROM [syint_QueryParams] WHERE qp_idQuery = " + oDT.Rows[0]["qu_id"].ToString());
            DB.Execute(DB.Configuration.InfrastructureConnection, "DELETE FROM [syint_QueryLinks]  WHERE ql_idQuery = " + oDT.Rows[0]["qu_id"].ToString());
            return DB.Execute(DB.Configuration.InfrastructureConnection, "DELETE FROM [syint_Query] WHERE qu_id = " + oDT.Rows[0]["qu_id"].ToString());
        }
        return 0;
    }
    public int DelQueryParams(string sCode, string sName)
    {
        DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, "SELECT qu_id FROM [syint_Query] WHERE qu_codice = '" + sCode + "'");
        if (oDT != null && oDT.Rows.Count > 0)
        {
            return DB.Execute(DB.Configuration.InfrastructureConnection, "DELETE FROM [syint_QueryParams] WHERE qp_idQuery = " + oDT.Rows[0]["qu_id"].ToString() + " AND qp_name = '" + sName + "'");
        }
        return 0;
    }
    public DataRow GetQuery(string sCode)
    {
        try
        {
            DataTable result = new DataTable();
            string sSQL = " SELECT top 1 * FROM syint_Query " +
                          " where LTRIM(RTRIM(qu_codice)) = LTRIM(RTRIM('" + sCode + "')) order by qu_id desc";
            DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, sSQL);
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
                    result = DB.Get(DbKey, Query);
                }
            }
            else if (Type == 1)
            {
                //result = DataManager.InvokeStore(DbKey, Query);
                result = DB.Invoke(DbKey, Query);
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
        DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, sSQL);
        DataRow row = oDT.Rows[0];
        string DbKey = row["qu_connectionkey"].ToString();
        string Query = row["qu_script"].ToString();
        int Type = int.Parse(row["qu_type"].ToString());
        if (Type == 0)
        {
            if (Query != null)
            {
                result = DB.Get(DbKey, Query);
            }
        }
        else if (Type == 1)
        {
            //result = DataManager.InvokeStore(DbKey, Query);
            result = DB.Invoke(DbKey, Query);
        }
        return result;
        //}
        //catch
        //{
        //    return new DataTable();
        //}
    }
    public int SetQuery(string sCode, string sName, string sScript, string sKeyDB, int iType, int iActive, string sOnLoad)
    {
        string sql = "";

        DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, "SELECT qu_codice FROM syint_Query WHERE qu_codice = '" + sCode + "'");
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
        return DB.Execute(DB.Configuration.InfrastructureConnection, sql);
    }
    public int SetQuery(string sCode, string sName, string sScript, string sKeyDB, int iType, int iActive, string sCheckedKey, string sCheckedName, string sOnLoad)
    {
        string sql = "";

        DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, "SELECT qu_codice FROM syint_Query WHERE qu_codice = '" + sCode + "'");
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
        return DB.Execute(DB.Configuration.InfrastructureConnection, sql);
    }
    public int SetQueryLink(string sCode, string sName, string sType, string sUrl, string sParams, int iOrder, int iVisible, int iChecked)
    {
        string sql = "";
        DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, "SELECT qu_id FROM [syint_Query] WHERE qu_codice = '" + sCode + "'");
        if (oDT == null || oDT.Rows.Count == 0)
            throw new Exception("Query non trovata");
        //
        DataTable oDTLinks = DB.Get(DB.Configuration.InfrastructureConnection, "SELECT * FROM [syint_QueryLinks] WHERE ql_idQuery = " + oDT.Rows[0]["qu_id"].ToString() + " AND ql_name = '" + sName + "'");
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
        return DB.Execute(DB.Configuration.InfrastructureConnection, sql);
    }
    public int SetQueryParams(string sCode, string sName, string sDescri, int iOrder, string sType, string sControl, string sSource, int iAllowNull, string sReplace)
    {
        string sql = "";
        DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, "SELECT qu_id FROM [syint_Query] WHERE qu_codice = '" + sCode + "'");
        if (oDT == null || oDT.Rows.Count == 0)
            throw new Exception("Query non trovata");
        //
        DataTable oDTParams = DB.Get(DB.Configuration.InfrastructureConnection, "SELECT * FROM [syint_QueryParams] WHERE qp_idQuery = " + oDT.Rows[0]["qu_id"].ToString() + " AND qp_name = '" + sName + "'");
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
        return DB.Execute(DB.Configuration.InfrastructureConnection, sql);
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
}
