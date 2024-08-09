using System.Runtime.CompilerServices;

namespace Solution.Infrastructure;
public static class Federation
{
    public static DB DB = null;
    public static Guid ID { get; } = Guid.NewGuid();
    public static string[] Args { get; set; }
    public static string Title { get { return ("Process-" + ID + "," + UserName + "," + ProcessName); } }
    public static string ProcessName { get; set; } = string.Empty;
    public static string Mode { get; set; } = string.Empty;
    public static bool IsPause { get; set; } = false;
    public static bool IsReload { get; set; } = false;
    public static bool IsExit { get; set; } = false;
    public static string ConfigFile { get { return Path.Combine(HomePath, "config.pxml"); } }
    public static string HomePath { get; } = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
    public static string AssemblyPath { get; } = Path.Combine(HomePath, "assembly");
    public static string HostName { get; } = Environment.MachineName;
    public static string UserName { get; } = Environment.UserName;
    private static FunctionsAssemblyManager _FunctionsAssemblyContext = null;
    public static FunctionsAssemblyManager FunctionsAssemblyContext 
    {
        get
        {
            _FunctionsAssemblyContext ??= new(DB, "default", AssemblyPath);
            return _FunctionsAssemblyContext;
        }
    }

    public static string Target { get; set; } = string.Empty;
    public static string Connection { get; set; } = string.Empty;
    public static Dictionary<string, ConfigurationOrganization> Organizations { get; set; } = [];

    public static void Start(string[] args, XML XMLManager)
    {
        Organizations.Clear();
        Args = args;
        ProcessName = (Args.Length > 0 ? Args[0] : "Default");
        Target = XMLManager.GetX("/registry/federations/@target", "preproduzione");
        Connection = XMLManager.GetX("/registry/federations/" + Target + "/connection/@value", "");
        //
        DB = new();
        DB.DataManager.Connections.Add("default", "sqldb", Connection);

        DBOrganizations DBOrganizations = new(DB, "default");
        DataTable dtOrganizations = DBOrganizations.Get();
        for (int i = 0; i < dtOrganizations?.Rows.Count; i++)
        {
            string keyOrg = dtOrganizations.Rows[i]["name"].ToString();
            string dbKeyOrg = dtOrganizations.Rows[i]["dbKey"].ToString();
            DB.DataManager.Connections.Add(keyOrg, "sqldb", dtOrganizations.Rows[i]["connection"].ToString());
            //
            DBConnections DBOrg = new(DB, keyOrg);
            DataTable OrgDT = DBOrg.Get();
            DB oDB = new();
            for (int c = 0; OrgDT != null && c < OrgDT.Rows.Count; c++)
            {
                string name = OrgDT.Rows[c]["name"].ToString();
                string connection = OrgDT.Rows[c]["connection"].ToString();
                string provider = OrgDT.Rows[c]["provider"].ToString().Equals("") ? "sqldb" : OrgDT.Rows[c]["provider"].ToString();
                //
                oDB.DataManager.Connections.Add(name, provider, connection);
            }
            //
            ConfigurationOrganization oOrg = new();
            oOrg.DB = oDB;
            oOrg.dbKey = dbKeyOrg;
            Organizations.Add(keyOrg, oOrg);
        }
    }

}
