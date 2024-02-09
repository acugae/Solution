using System.Threading.Tasks;

namespace Solution.Infrastructure;
public class cDBHttpClients
{
    readonly cDB DB;
    public cDBHttpClients(cModelConfiguration oConfiguration) => DB = new(oConfiguration);
    public cDBHttpClients(cDB oDB) => DB = oDB;
    public int InsertHttpClient(string sCodice, int iActive, string sMethod, string sContextType, string sUrl, string sPayload, string sHeader = null)
    {
        string sQuery = "INSERT INTO[dbo].[syint_HttpClient] ([hc_codice],[hc_active],[hc_method],[hc_contextType],[hc_url],[hc_payload],[hc_header]) VALUES ";
        sQuery += " (" + DB.GetValueString(sCodice) + "," + iActive.ToString() + "," + DB.GetValueString(sMethod) + "," + DB.GetValueString(sContextType) + "," + DB.GetValueString(sUrl) + "," + DB.GetValueString(sPayload) + "," + DB.GetValueString(sHeader) + ") ";
        sQuery += "; select @@identity;";
        //
        DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, sQuery);
        return int.Parse(oDT.Rows[0][0].ToString());
    }
    public int UpdateHttpClient(string sCodice, int iActive, string sMethod, string sContextType, string sUrl, string sPayload, string sHeader = null)
    {
        string sQuery = "UPDATE [dbo].[syint_HttpClient] SET ";
        sQuery += " [hc_active] = " + iActive.ToString() + ", [hc_method] = " + DB.GetValueString(sMethod) + ", [hc_contextType] = " + DB.GetValueString(sContextType) + ", [hc_url] = " + DB.GetValueString(sUrl) + ", [hc_payload] = " + DB.GetValueString(sPayload) + ", [hc_header] = " + DB.GetValueString(sHeader);
        sQuery += " WHERE hc_codice = " + DB.GetValueString(sCodice);
        //
        return DB.Execute(DB.Configuration.InfrastructureConnection, sQuery);
    }
    public int UpdateHttpClientHistory(int idHttpClientHistory, string sResponse, int isError = 0)
    {
        string sQuery = "UPDATE [dbo].[syint_HttpClientHistory] SET ";
        sQuery += " [hch_response] = " + DB.GetValueString(sResponse) + ", [hch_responseDate] = getdate(), [hch_isError] = " + isError.ToString();
        sQuery += " WHERE hch_id = " + idHttpClientHistory.ToString();
        //
        return DB.Execute(DB.Configuration.InfrastructureConnection, sQuery);
    }
    public int InsertHttpClientHistory(int idHttpClient, string sMethod, string sContextType, string sUrl, string sPayload, string sHeader = null)
    {
        string sQuery = "INSERT INTO [dbo].[syint_HttpClientHistory] ";
        sQuery += " ([hch_idHttpClient],[hch_method],[hch_contextType],[hch_url],[hch_payload],[hch_requestDate], [hch_isError], [hch_header]) ";
        sQuery += " VALUES ";
        sQuery += " (" + idHttpClient.ToString() + "," + DB.GetValueString(sMethod) + "," + DB.GetValueString(sContextType) + "," + DB.GetValueString(sUrl) + "," + DB.GetValueString(sPayload) + ",getdate(), 0, " + DB.GetValueString(sHeader) + ") ";
        sQuery += "; select @@identity;";
        //
        DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, sQuery);
        return int.Parse(oDT.Rows[0][0].ToString());
    }
    public DataRow GetHttpClient(string sCodice)
    {
        string sQuery = "SELECT * FROM syint_HttpClient WHERE hc_codice = '" + sCodice + "'";
        DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, sQuery);
        if (oDT == null || oDT.Rows.Count == 0)
            return null;
        return oDT.Rows[0];
    }
    public DataRow GetHttpClient(int id)
    {
        string sQuery = "SELECT * FROM syint_HttpClient WHERE hc_id = '" + id.ToString() + "'";
        DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, sQuery);
        if (oDT == null || oDT.Rows.Count == 0)
            return null;
        return oDT.Rows[0];
    }
}
