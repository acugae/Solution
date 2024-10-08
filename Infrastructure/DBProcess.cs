using Renci.SshNet.Messages;

namespace Solution.Infrastructure;
public class DBProcess : DBEntity
{
    public DBProcess(DB DB, string dbKey) : base(DB, dbKey, "core_Processes") { }
    public void StartProcess(Guid ID, string Mode, string HostName, string ProcessName, string UserName, string HomePath)
    {
        CRUD oCrud = new(_DB, _dbKey);
        CRUDBase tabService = new(_entityName);
        tabService["pr_id"] = ID;
        tabService["pr_mode"] = Mode;
        tabService["pr_host"] = HostName;
        tabService["pr_name"] = ProcessName;
        tabService["pr_user"] = UserName;
        tabService["pr_path"] = HomePath;
        //tabService["pr_config"] = JSON.Serialize(_DB.Configuration);
        oCrud.Insert(tabService);
    }
    public DataRow ReadProcess(Guid ID)
    {
        try
        {
            return _DB.Get(_dbKey, "SELECT * FROM core_Processes WHERE pr_id = '" + ID.ToString() + "'").Rows[0];
        }
        catch { return null; }
    }
    public void UpdateProcess(Guid ID, string sOutput)
    {
        CRUD oCrud = new(_DB, _dbKey);
        CRUDUpdate tabService = new(_entityName);
        tabService["pr_dateLast"] = DateTime.Now;
        tabService["pr_output"] = sOutput;
        //
        tabService.Filters.Add(new CRUDFilter("pr_id", "=", ID.ToString()));
        oCrud.Update(tabService);
    }
}
