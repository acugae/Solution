namespace Solution.Infrastructure;
public class DBHttpRoutes
{
    readonly DB DB;
    public DBHttpRoutes(Configuration oConfiguration) => DB = new(oConfiguration);
    public DBHttpRoutes(DB oDB) => DB = oDB;
    public DataTable GetRoutes(string sMachineName, string sServiceName)
    {
        string sSQL = "SELECT [hr_id] [ID],[hr_name] ,[hr_route] [Pattern], [ht_type]  ,[ht_assembly] [Assembly], [ht_controller] [Class], [ht_method] [Function], [ht_allowedMethods] [httpMethods] FROM core_HttpRoutes WHERE hr_isactive = 1 AND ( ( (hr_host LIKE '{0},%') OR (hr_host LIKE '%,{0}') OR (hr_host LIKE '%,{0},%') OR (hr_host = '{0}') OR (hr_host = '*') ) AND ( (hr_service LIKE '{1},%') OR (hr_service LIKE '%,{1}') OR (hr_service LIKE '%,{1},%') OR (hr_service = '{1}') OR (hr_service = '*') ) ) ";
        return DB.Get(DB.Configuration.InfrastructureConnection, string.Format(sSQL, sMachineName, sServiceName));
    }
    public int UpdateHttpRoutesHistory(int idHttpRoutesHistory, string sResponse, int isError = 0)
    {
        string sQuery = string.Concat("UPDATE core_HttpRoutesHistory SET [hrh_response] = ", DB.GetValueString(sResponse), ", [hrh_responseDate] = getdate(), [hrh_isError] = ", isError.ToString(), " WHERE hrh_id = " , idHttpRoutesHistory.ToString());
        return DB.Execute(DB.Configuration.InfrastructureConnection, sQuery);
    }
    public int InsertHttpRoutesHistory(int idHttpRoutes, string sController, string sMethod, string sRequest, string sOthers = null)
    {
        string sQuery = " INSERT INTO core_HttpRoutesHistory ([hrh_idHttpRoutes],[hrh_controller],[hrh_method],[hrh_request],[hrh_other]) VALUES (" + idHttpRoutes + " ," + DB.GetValueString(sController) + " ," + DB.GetValueString(sMethod) + "," + DB.GetValueString(sRequest) + "," + DB.GetValueString(sOthers) + " ); select @@identity;";
        DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, sQuery);
        return int.Parse(oDT.Rows[0][0].ToString());
    }
    public int InsertHttpRoutesHistory(Route oRoute, string sBody, HttpRequest oRequest)
    {
        string sQuery = " INSERT INTO core_HttpRoutesHistory ([hrh_idHttpRoutes],[hrh_controller],[hrh_method],[hrh_request],[hrh_other]) VALUES (" + oRoute.ID + " ," + DB.GetValueString(oRoute.Class) + " ," + DB.GetValueString(oRoute.Function) + "," + DB.GetValueString(sBody) + "," + DB.GetValueString(GetOthers(oRequest)) + " ); select @@identity;";
        DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, sQuery);
        return int.Parse(oDT.Rows[0][0].ToString());
    }
    private string GetOthers(HttpRequest oRequest)
    {
        try
        {
            Dictionary<string, string> dOther = new Dictionary<string, string>();
            dOther["HttpMethod"] = oRequest.Method;
            dOther["Route"] = oRequest.Path;
            if (oRequest.Headers == null || oRequest.Headers.Count == 0)
                return JSON.Serialize(dOther);
            Dictionary<string, string> dHeaders = oRequest.Headers.ToDictionary(k => k.Key, h => h.Value.ToString());
            foreach (var kvp in dHeaders)
                dOther[kvp.Key] = kvp.Value;
            return JSON.Serialize(dOther);
        }
        catch (Exception ex)
        {
            return "{ \"errormessage\" : \"" + ex.Message.Replace("'", "''") + "\"  }";
        }
    }
}

