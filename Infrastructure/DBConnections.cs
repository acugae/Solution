using NPOI.SS.Formula.Eval;
using Renci.SshNet.Messages;

namespace Solution.Infrastructure;
public class DBConnections
{
    readonly DB DB;
    public DBConnections(Configuration oConfiguration) => DB = new(oConfiguration);
    public DBConnections(DB oDB) => DB = oDB;
    public void Set(Guid? ID, string name, string connection, string provider)
    {
        CRUD oCrud = new(DB, DB.Configuration.InfrastructureConnection);
        CRUDBase tabConnection = new("core_Connections");
        tabConnection["name"] = name;
        tabConnection["connection"] = connection;
        tabConnection["provider"] = provider;
        if (ID is null)
            oCrud.Insert(tabConnection);
        else
        {
            CRUDUpdate tabConnectionUP = (CRUDUpdate)tabConnection;
            tabConnectionUP.Filters.Add(new CRUDFilter("id", "=", ID));
            oCrud.Update(tabConnectionUP);
        }
    }
    public DataTable Get()
    {
        try
        {
            return DB.Get(DB.Configuration.InfrastructureConnection, "SELECT * FROM core_Connections WHERE deletionStateCode = 0");
        }
        catch { return null; }
    }
    public DataRow Get(string name)
    {
        try
        {
            return DB.Get(DB.Configuration.InfrastructureConnection, "SELECT * FROM core_Connections WHERE name = '" + name + "' and deletionStateCode = 0").Rows[0];
        }
        catch { return null; }
    }
}
