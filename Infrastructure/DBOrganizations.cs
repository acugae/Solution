using NPOI.SS.Formula.Eval;
using Renci.SshNet.Messages;

namespace Solution.Infrastructure;
public class DBOrganizations : DBCore
{
    public DBOrganizations(DB DB, string dbKey) : base(DB, dbKey, "core_Organizations") { }
    public void Set(Guid? id, string name, string connection)
    {
        Dictionary<string, object> Attributes = new();
        Attributes.Add("id", id);
        Attributes.Add("name", name);
        Attributes.Add("connection", connection);

        Set(Attributes);
    }
}
