using NPOI.SS.Formula.Eval;
using Renci.SshNet.Messages;

namespace Solution.Infrastructure;
public class DBControls : DBCore
{
    public DBControls(DB oDB) : base(oDB, "core_Controls") { }
    public void Set(Guid? id, string name, string nameClasses, string descri)
    {
        Dictionary<string, object> Attributes = new();
        Attributes["id"] = id;
        Attributes["name"] = name;
        Attributes["nameClasses"] = nameClasses;
        Attributes["descri"] = descri;

        Set(Attributes);
    }
}
