using NPOI.SS.Formula.Eval;
using Renci.SshNet.Messages;

namespace Solution.Infrastructure;
public class DBCore
{
    readonly protected DB DB;
    readonly protected string entityName;
    public DBCore(DB oDB, string entityName) { DB = oDB; this.entityName = entityName; }
    //public void Set(Guid? ID, string name, string type, byte[] executable)
    public void Set(Dictionary<string, object> Attributes)
    {
        CRUD oCrud = new(DB, DB.Configuration.InfrastructureConnection);
        CRUDBase tabAssemblies = new(entityName, Attributes);

        Guid? ID = (Attributes.ContainsKey("id") ? Guid.Parse( Attributes["id"].ToString() ) : null);
        if (Attributes.ContainsKey("id") && Attributes["id"] is not null)
            oCrud.Insert(tabAssemblies);
        else
        {
            CRUDUpdate tabConnectionUP = (CRUDUpdate)tabAssemblies;
            tabAssemblies["modifiedOn"] = DateTime.Now;
            tabConnectionUP.Filters.Add(new CRUDFilter("id", "=", ID));
            oCrud.Update(tabConnectionUP);
        }
    }
    public void SetField(Guid? ID, string nameField, object valueField)
    {
        CRUD oCrud = new(DB, DB.Configuration.InfrastructureConnection);
        CRUDBase tabAssemblies = new(entityName);
        tabAssemblies[nameField] = valueField;
        tabAssemblies["modifiedOn"] = DateTime.Now;
        if (ID is null)
            oCrud.Insert(tabAssemblies);
        else
        {
            CRUDUpdate tabConnectionUP = new( tabAssemblies.Name , tabAssemblies.Attributes );
            tabConnectionUP.Filters.Add(new CRUDFilter("id", "=", ID));
            oCrud.Update(tabConnectionUP);
        }
    }
    public DataTable Get()
    {
        try
        {
            return DB.Get(DB.Configuration.InfrastructureConnection, "SELECT * FROM " + entityName + " WHERE deletionStateCode = 0");
        }
        catch { return null; }
    }
    public DataRow Get(string name)
    {
        try
        {
            return DB.Get(DB.Configuration.InfrastructureConnection, "SELECT * FROM " + entityName + " WHERE name = '" + name + "' and deletionStateCode = 0").Rows[0];
        }
        catch { return null; }
    }
    public DataRow Get(Guid id)
    {
        try
        {
            return DB.Get(DB.Configuration.InfrastructureConnection, "SELECT * FROM " + entityName + " WHERE id = '" + id.ToString() + "'").Rows[0];
        }
        catch { return null; }
    }

}

