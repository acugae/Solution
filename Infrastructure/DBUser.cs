using Renci.SshNet.Messages;

namespace Solution.Infrastructure;
public class DBUser : DBCore
{
    public DBUser(DB DB, string dbKey) : base(DB, dbKey, "core_Users") { }
    public void Set(Guid? id, string name, string password, string fullName, string organizationDefault)
    {
        Dictionary<string, object> Attributes = new();
        Attributes["id"] = id;
        Attributes["name"] = name;
        Attributes["password"] = password;
        Attributes["fullName"] = fullName;
        Attributes["organizationDefault"] = organizationDefault;

        Set(Attributes);
    }
    public User Authentication(string sDomain, string sUsername, string sPassword)
    {
        if (sUsername.Split('\\').Length != 2)
            sUsername = @"local\" + sUsername;

        string _sDominio = (sDomain ?? sUsername.Split('\\')[0]);
        string _sUsername = sUsername.Split('\\')[1];

        List<Parameter> oParams = [];
        oParams.Add(db.CreateParameter(Application.Configuration.InfrastructureConnection, DbType.String, ParameterDirection.Input, "@domain", _sDominio));
        oParams.Add(db.CreateParameter(Application.Configuration.InfrastructureConnection, DbType.String, ParameterDirection.Input, "@username", _sUsername));
        oParams.Add(db.CreateParameter(Application.Configuration.InfrastructureConnection, DbType.String, ParameterDirection.Input, "@password", sPassword));
        DataTable oDT = db.InvokeSQL(Application.Configuration.InfrastructureConnection, "SELECT TOP 1 * FROM core_Users WHERE deletionStateCode = 0 AND [domain] = @domain AND [userName] = @username AND [password] = @password", oParams.ToArray());
        if (oDT.Rows.Count > 0)
            return new User { Id = Guid.NewGuid().ToString(), Domain = _sDominio, UserName = _sUsername, FullName = oDT.Rows[0]["fullName"].ToString() };
        else
            return null;
    }
}
