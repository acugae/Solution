using NPOI.SS.Formula.Eval;
using Renci.SshNet.Messages;

namespace Solution.Infrastructure;
public class DBCore
{
    readonly protected DB _DB;
    readonly protected string _entityName;
    readonly protected string _dbKey;
    public DBCore(DB DB, string dbKey, string entityName) { _DB = DB; _dbKey = dbKey; _entityName = entityName; }
    public void Set(Dictionary<string, object> Attributes)
    {
        CRUD oCrud = new(_DB, _dbKey);
        CRUDBase tabAssemblies = new(_entityName, Attributes);

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
        CRUD oCrud = new(_DB, _dbKey);
        CRUDBase tabAssemblies = new(_entityName);
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
            return _DB.Get(_dbKey, "SELECT * FROM " + _entityName + " WHERE deletionStateCode = 0");
        }
        catch { return null; }
    }
    public DataRow Get(string name)
    {
        try
        {
            return _DB.Get(_dbKey, "SELECT * FROM " + _entityName + " WHERE name = '" + name + "' and deletionStateCode = 0").Rows[0];
        }
        catch { return null; }
    }
    public DataRow Get(Guid id)
    {
        try
        {
            return _DB.Get(_dbKey, "SELECT * FROM " + _entityName + " WHERE id = '" + id.ToString() + "'").Rows[0];
        }
        catch { return null; }
    }

}

