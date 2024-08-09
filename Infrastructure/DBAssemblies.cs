using NPOI.SS.Formula.Eval;
using Renci.SshNet.Messages;

namespace Solution.Infrastructure;
public class DBAssemblies : DBCore
{
    public DBAssemblies(DB DB, string dbKey) : base(DB, dbKey, "core_Assemblies") { }
    
    public void Set(Guid? id, string name, string type, byte[] executable)
    {
        Dictionary<string, object> Attributes = new();
        Attributes.Add("id", id);
        Attributes.Add("name", name);
        Attributes.Add("type", type);
        Attributes.Add("executable", executable);

        Set(Attributes);
    }
}
