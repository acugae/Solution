using NPOI.SS.Formula.Eval;
using Renci.SshNet.Messages;

namespace Solution.Infrastructure;
public class DBConnections : DBCore
{
    public DBConnections(DB DB, string dbKey) : base(DB, dbKey, "core_Connections") { }
    public void Set(Guid? id, string name, string connection, string provider, int isInfrastructure)
    {
        Dictionary<string, object> Attributes = new();
        Attributes.Add("id", id);
        Attributes.Add("name", name);
        Attributes.Add("connection", connection);
        Attributes.Add("provider", provider);
        Attributes.Add("isInfrastructure", isInfrastructure);

        Set(Attributes);
    }
}
