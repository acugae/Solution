namespace Solution.Infrastructure;
public static class Application
{
    //public static cCacheManager CacheManager;
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
            _FunctionsAssemblyContext ??= new(DB, AssemblyPath);
            return _FunctionsAssemblyContext;
        }
    }

    public static Configuration Configuration { get; set; } = new Configuration();
    public static void Start(string[] args, XML XMLManager)
    {
        Args = args;
        ProcessName = (Args.Length > 0 ? Args[0] : "Default");
        //
        //CacheManager = new cCacheManager(XMLManager);
        //
        Configuration.Target = XMLManager.GetX("/registry/configurations/@target", "preproduzione");
        Configuration.InfrastructureConnection = XMLManager.GetX("/registry/configurations/" + Configuration.Target + "/infrastructure/@connection", "");
        Configuration.IsStandalone = bool.Parse(XMLManager.GetX("/registry/standalone/@enabled", "false"));
        Configuration.IsLocalAssembly = Convert.ToBoolean(XMLManager.GetX("/registry/configurations/" + Configuration.Target + "/add[@key='IsLocalAssembly']/@value", "true"));
        Configuration.PianifQueue = XMLManager.GetX("/registry/configurations/" + Configuration.Target + "/infrastructure/@pianifQueue", "");
        Configuration.SystemQueue = XMLManager.GetX("/registry/configurations/" + Configuration.Target + "/infrastructure/@systemQueue", "");
        //
        string[] connections = XMLManager.GetX("/registry/configurations/" + Configuration.Target + "/connections/add/@key");
        Configuration.Connections.Clear();
        for (int i = 0; connections != null && i < connections.Length; i++)
        {
            string sKey = connections[i];
            string sProvider = XMLManager.GetX("/registry/configurations/" + Configuration.Target + "/connections/add[@key='" + connections[i] + "']/@provider", "sqldb");
            string sConnection = XMLManager.GetX("/registry/configurations/" + Configuration.Target + "/connections/add[@key='" + connections[i] + "']/@value", "");
            Configuration.Connections.Add(sKey, new ConfigurationConnection(sKey, sConnection, sProvider));
        }
        //
        string[] queues = XMLManager.GetX("/registry/configurations/" + Configuration.Target + "/queues/queue/@key");
        string[] connectionsQueue = XMLManager.GetX("/registry/configurations/" + Configuration.Target + "/queues/queue/@connection");
        string[] tables = XMLManager.GetX("/registry/configurations/" + Configuration.Target + "/queues/queue/@table");
        Configuration.Queues.Clear();
        for (int i = 0; queues != null && i < queues.Length; i++)
        {
            string sQueue = queues[i];
            string sConnection = connectionsQueue[i];
            string sTable = tables[i];
            Configuration.Queues.Add(queues[i], new ConfigurationQueue(sQueue, sConnection, sTable));
        }
        //
        string[] exclusivemessages = XMLManager.GetX("/registry/exclusivemessages[@enabled='true']/message/@name");
        string sExclusiveMessagesTmp = "";
        for (int i = 0; exclusivemessages != null && i < exclusivemessages.Length; i++)
            sExclusiveMessagesTmp += "'" + exclusivemessages[i] + "'" + ((i < (exclusivemessages.Length - 1)) ? "," : "");
        Configuration.ExclusiveMessages = (sExclusiveMessagesTmp.Trim().Equals("") ? "" : sExclusiveMessagesTmp);
        //
        if (Configuration.IsStandalone)
        {
            string[] svAssembly = XMLManager.GetX("/registry/standalone[@enabled='true']/message[@enabled='true']/@msg_assembly");
            string[] svClass = XMLManager.GetX("/registry/standalone[@enabled='true']/message[@enabled='true']/@msg_class");
            string[] svValue = XMLManager.GetX("/registry/standalone[@enabled='true']/message[@enabled='true']/@msg_value");
            string[] svFunction = XMLManager.GetX("/registry/standalone[@enabled='true']/message[@enabled='true']/@msg_function");
            //
            if (svAssembly.Length != svClass.Length || svAssembly.Length != svValue.Length || svAssembly.Length != svFunction.Length)
            {
                Log.WriteLine("Errore nella configurazione del servizio in modalità Standalone.");
                return;
            }
            //
            for(int i = 0; i < svAssembly.Length; i++)
            {
                ConfigurationMessage oMessage = new(svAssembly[i], svClass[i], svFunction[i], svValue[i]);
                Configuration.Messages.Add(oMessage);
            }
        }
        DB = new(Configuration);
        Log.WriteLine("Process:" + Application.ID, "HostName: " + Application.HostName, "ProcessName: " + Application.ProcessName, "Mode: " + Application.Mode, "Standalone: " + Application.Configuration.IsStandalone.ToString());
    }

}
