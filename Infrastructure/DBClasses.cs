using NPOI.SS.Formula.Eval;
using Renci.SshNet.Messages;
using System.Reflection;

namespace Solution.Infrastructure;
public class DBClasses : DBCore
{
    public DBClasses(DB oDB) : base(oDB, "core_Classes") { }
    
    public void Set(Guid? id, string name, Guid idAssemblies, string className, string methodName, string descri)
    {
        Dictionary<string, object> Attributes = new();
        Attributes["id"] = id;
        Attributes["name"] = name;
        Attributes["idAssemblies"] = idAssemblies;
        Attributes["className"] = className;
        Attributes["methodName"] = methodName;
        Attributes["descri"] = descri;

        Set(Attributes);
    }
}
