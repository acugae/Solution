using NPOI.SS.Formula.Eval;
using Renci.SshNet.Messages;

namespace Solution.Infrastructure;
public class DBEntity
{
    readonly protected DB DB;
    readonly protected string entityName;
    readonly protected string dbKey;
    public DBEntity(DB DB, string entityName) { this.DB = DB; dbKey = DB.connectionDefault; this.entityName = entityName; }
    public DBEntity(DB DB, string dbKey, string entityName) { this.DB = DB; this.dbKey = dbKey; this.entityName = entityName; }
    public DataTable Get()
    {
        try
        {
            return DB.Get(dbKey, "SELECT * FROM [" + entityName + "]");
        }
        catch { return null; }
    }
    public string GetValueString(string sValue)
    {
        return sValue == null ? "null" : "'" + sValue.Replace("'", "''") + "'";
    }
}

