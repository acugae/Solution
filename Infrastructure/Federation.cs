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
    public static Dictionary<string, Configuration> Organizations { get; set; } = [];

    //public static DB GetDB(string organization) { 
    //    return new(Organizations[organization]);
    //}
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
            Configuration organization = new();
            
            string keyOrg = dtOrganizations.Rows[i]["name"].ToString();
            DB.DataManager.Connections.Add(keyOrg, "sqldb", dtOrganizations.Rows[i]["connection"].ToString());
            //
            DBConnections DBConnections = new(DB, keyOrg);
            DataTable dtConnections = DBConnections.Get();
            for (int c = 0; dtConnections != null && c < dtConnections.Rows.Count; c++)
            {
                string name = dtConnections.Rows[c]["name"].ToString();
                string connection = dtConnections.Rows[c]["connection"].ToString();
                string provider = dtConnections.Rows[c]["provider"].ToString().Equals("") ? "sqldb" : dtConnections.Rows[c]["provider"].ToString();
                int isInfrastructure = dtConnections.Rows[c]["isInfrastructure"].ToString().Equals("1") ? 1 : 0;
                if (isInfrastructure == 1)
                    organization.InfrastructureConnection = name;
                organization.Connections.Add(name, new ConfigurationConnection(name, connection, provider));
            }
            //
            DBQueues DBQueues = new(DB, keyOrg);
            DataTable dtQueues = DBQueues.Get();
            for (int c = 0; dtQueues != null && c < dtQueues.Rows.Count; c++)
            {
                string name = dtQueues.Rows[c]["name"].ToString();
                string dbKey = dtQueues.Rows[c]["dbKey"].ToString();
                string tableName = dtQueues.Rows[c]["tableName"].ToString();
                int isPianif = dtQueues.Rows[c]["isPianif"].ToString().Equals("1") ? 1 : 0;
                if (isPianif == 1)
                    organization.PianifQueue = name;
                int isSystem = dtQueues.Rows[c]["isSystem"].ToString().Equals("1") ? 1 : 0;
                if (isSystem == 1)
                    organization.SystemQueue = name;
                organization.Queues.Add(name, new(name, dbKey, tableName));
            }

            //organization.Connections.Add("default", new ConfigurationConnection("default", Connection, "sqldb"));
            Organizations.Add(keyOrg, organization);
        }
    }

}
