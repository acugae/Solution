using NPOI.SS.Formula.Eval;
using Renci.SshNet.Messages;

namespace Solution.Infrastructure;
public class DBConnections : DBCore
{
    public DBConnections(DB oDB) : base(oDB, "core_Connections") { }
    public void Set(Guid? id, string name, string connection, string provider)
    {
        Dictionary<string, object> Attributes = new();
        Attributes.Add("id", id);
        Attributes.Add("name", name);
        Attributes.Add("connection", connection);
        Attributes.Add("provider", provider);

        Set(Attributes);
    }
}
