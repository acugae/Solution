using NPOI.SS.Formula.Eval;
using Renci.SshNet.Messages;

namespace Solution.Infrastructure;
public class DBQueues : DBCore
{
    public DBQueues(DB DB, string dbKey) : base(DB, dbKey, "core_Queues") { }
    public void Set(Guid? id, string name, string dbKey, string tableName, int isPianif, int isSystem)
    {
        Dictionary<string, object> Attributes = new();
        Attributes.Add("id", id);
        Attributes.Add("name", name);
        Attributes.Add("dbKey", dbKey);
        Attributes.Add("tableName", tableName);
        Attributes.Add("isPianif", isPianif);
        Attributes.Add("isSystem", isSystem);

        Set(Attributes);
    }
}
