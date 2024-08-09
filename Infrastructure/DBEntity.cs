using NPOI.SS.Formula.Eval;
using Renci.SshNet.Messages;

namespace Solution.Infrastructure;
public class DBEntity
{
    readonly protected DB _DB;
    readonly protected string _entityName;
    readonly protected string _dbKey;
    public DBEntity(DB DB, string dbKey, string entityName) { _DB = DB; _dbKey = dbKey; _entityName = entityName; }
    public DataTable Get()
    {
        try
        {
            return _DB.Get(_dbKey, "SELECT * FROM [" + _entityName + "]");
        }
        catch { return null; }
    }
}

