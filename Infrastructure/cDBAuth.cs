using Renci.SshNet.Messages;

namespace Solution.Infrastructure;
public class cDBAuth
{
    readonly cDB DB;
    public cDBAuth(cModelConfiguration oConfiguration) => DB = new(oConfiguration);
    public cDBAuth(cDB oDB) => DB = oDB;
    public cUser Authentication(string sDomain, string sUsername, string sPassword)
    {
        if (sUsername.Split('\\').Length != 2)
            sUsername = @"local\" + sUsername;

        string _sDominio = (sDomain ?? sUsername.Split('\\')[0]);
        string _sUsername = sUsername.Split('\\')[1];

        List<cParameter> oParams = new List<cParameter>();
        oParams.Add(DB.CreateParameter(cApplication.Configuration.InfrastructureConnection, DbType.String, ParameterDirection.Input, "@domain", _sDominio));
        oParams.Add(DB.CreateParameter(cApplication.Configuration.InfrastructureConnection, DbType.String, ParameterDirection.Input, "@username", _sUsername));
        oParams.Add(DB.CreateParameter(cApplication.Configuration.InfrastructureConnection, DbType.String, ParameterDirection.Input, "@password", sPassword));
        DataTable oDT = DB.InvokeSQL(cApplication.Configuration.InfrastructureConnection, "SELECT TOP 1 * FROM core_Users WHERE deletionStateCode = 0 AND [domain] = @domain AND [userName] = @username AND [password] = @password", oParams.ToArray());
        if (oDT.Rows.Count > 0)
            return new cUser { Id = Guid.NewGuid().ToString(), Domain = _sDominio, UserName = _sUsername, FullName = oDT.Rows[0]["fullName"].ToString() };
        else
            return null;
    }
}
