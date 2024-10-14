using System.Threading.Tasks;

namespace Solution.Infrastructure;
public class DBHttpClients : DBEntity
{
    //readonly DB DB;
    //readonly string dbKey;
    //public DBHttpClients(DB oDB, string sKey) { DB = oDB; dbKey = sKey; }
    public DBHttpClients(DB DB, string dbKey) : base(DB, dbKey, "syint_HttpClient") { }
    public int InsertHttpClient(string sCodice, int iActive, string sMethod, string sContextType, string sUrl, string sPayload, string? sHeader = null)
    {
        string sQuery = "INSERT INTO[dbo].[syint_HttpClient] ([hc_codice],[hc_active],[hc_method],[hc_contextType],[hc_url],[hc_payload],[hc_header]) VALUES ";
        sQuery += " (" + GetValueString(sCodice) + "," + iActive.ToString() + "," + GetValueString(sMethod) + "," + GetValueString(sContextType) + "," + GetValueString(sUrl) + "," + GetValueString(sPayload) + "," + GetValueString(sHeader) + ") ";
        sQuery += "; select @@identity;";
        //
        DataTable oDT = DB.Get(dbKey, sQuery);
        return int.Parse(oDT.Rows[0][0].ToString());
    }
    public int UpdateHttpClient(string sCodice, int iActive, string sMethod, string sContextType, string sUrl, string sPayload, string? sHeader = null)
    {
        string sQuery = "UPDATE [dbo].[syint_HttpClient] SET ";
        sQuery += " [hc_active] = " + iActive.ToString() + ", [hc_method] = " + GetValueString(sMethod) + ", [hc_contextType] = " + GetValueString(sContextType) + ", [hc_url] = " + GetValueString(sUrl) + ", [hc_payload] = " + GetValueString(sPayload) + ", [hc_header] = " + GetValueString(sHeader);
        sQuery += " WHERE hc_codice = " + GetValueString(sCodice);
        //
        return DB.Execute(dbKey, sQuery);
    }
    public int UpdateHttpClientHistory(int idHttpClientHistory, string sResponse, int isError = 0)
    {
        string sQuery = "UPDATE [dbo].[syint_HttpClientHistory] SET ";
        sQuery += " [hch_response] = " + GetValueString(sResponse) + ", [hch_responseDate] = getdate(), [hch_isError] = " + isError.ToString();
        sQuery += " WHERE hch_id = " + idHttpClientHistory.ToString();
        //
        return DB.Execute(dbKey, sQuery);
    }
    public int InsertHttpClientHistory(int idHttpClient, string sMethod, string sContextType, string sUrl, string sPayload, string? sHeader = null)
    {
        string sQuery = "INSERT INTO [dbo].[syint_HttpClientHistory] ";
        sQuery += " ([hch_idHttpClient],[hch_method],[hch_contextType],[hch_url],[hch_payload],[hch_requestDate], [hch_isError], [hch_header]) ";
        sQuery += " VALUES ";
        sQuery += " (" + idHttpClient.ToString() + "," + GetValueString(sMethod) + "," + GetValueString(sContextType) + "," + GetValueString(sUrl) + "," + GetValueString(sPayload) + ",getdate(), 0, " + GetValueString(sHeader) + ") ";
        sQuery += "; select @@identity;";
        //
        DataTable oDT = DB.Get(dbKey, sQuery);
        return int.Parse(oDT.Rows[0][0].ToString());
    }
    public DataRow GetHttpClient(string sCodice)
    {
        string sQuery = "SELECT * FROM syint_HttpClient WHERE hc_codice = '" + sCodice + "'";
        DataTable oDT = DB.Get(dbKey, sQuery);
        if (oDT == null || oDT.Rows.Count == 0)
            return null;
        return oDT.Rows[0];
    }
    public DataRow GetHttpClient(int id)
    {
        string sQuery = "SELECT * FROM syint_HttpClient WHERE hc_id = '" + id.ToString() + "'";
        DataTable oDT = DB.Get(dbKey, sQuery);
        if (oDT == null || oDT.Rows.Count == 0)
            return null;
        return oDT.Rows[0];
    }
}
