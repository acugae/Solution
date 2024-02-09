using Renci.SshNet.Messages;

namespace Solution.Infrastructure;
public class cDBProcess
{
    readonly cDB DB;
    public cDBProcess(cModelConfiguration oConfiguration) => DB = new(oConfiguration);
    public cDBProcess(cDB oDB) => DB = oDB;
    public void StartProcess(Guid ID, string Mode, string HostName, string ProcessName, string UserName, string HomePath)
    {
        cCRUD oCrud = new(DB, DB.Configuration.InfrastructureConnection);
        CRUDBase tabService = new("core_Processes");
        tabService["pr_id"] = ID;
        tabService["pr_mode"] = Mode;
        tabService["pr_host"] = HostName;
        tabService["pr_name"] = ProcessName;
        tabService["pr_user"] = UserName;
        tabService["pr_path"] = HomePath;
        tabService["pr_config"] = cJson.Serialize(DB.Configuration);
        oCrud.Insert(tabService);
    }
    public DataRow ReadProcess(Guid ID)
    {
        try
        {
            return DB.Get(DB.Configuration.InfrastructureConnection, "SELECT * FROM core_Processes WHERE pr_id = '" + ID.ToString() + "'").Rows[0];
        }
        catch { return null; }
    }
    public void UpdateProcess(Guid ID, string sOutput)
    {
        cCRUD oCrud = new(DB, DB.Configuration.InfrastructureConnection);
        CRUDUpdate tabService = new("core_Processes");
        tabService["pr_dateLast"] = DateTime.Now;
        tabService["pr_output"] = sOutput;
        //
        tabService.Filters.Add(new CRUDFilter("pr_id", "=", ID.ToString()));
        oCrud.Update(tabService);
    }
}
