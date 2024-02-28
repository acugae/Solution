namespace Solution.Infrastructure;
abstract public class FunctionModule : ControllerBase 
{
    public DB? db = null;
    public DBConfig? _DBConfig = null;
    public Service? _Service = null;
    private XLS? _XLS = null;
    private Email? _Communication = null;
    public FunctionParameters Parameters { get; set; } = new FunctionParameters();
    public int ID { get { return Parameters.ToInt("msg_id"); } }
    public int State { get { return Parameters.ToInt("msg_state"); } }
    public string Value { get { return Parameters.ToString("value"); } } 
    public string TaskName { get { return Parameters.ToString("msg_taskname"); } }
    public string Message { get { return Parameters.ToString("message"); } set { Parameters["message"] = value; } }
    public string Queue { get { return Parameters.ToString("queue"); } }
    public string User { get { return Parameters.ToString("user"); } }
    public Service Service
    {
        get
        {
            _Service ??= new(db);
            return _Service;
        }
    }
    public DBConfig DBConfig
    {
        get
        {
            _DBConfig ??= new(db);
            return _DBConfig;
        }
    }
    public Email Communication
    {
        get {
            _Communication ??= new(DBConfig.GetConfig(@"\system\mail\normal\smtpserver", ""));
            return _Communication;
        }
    }
    //public FunctionModule()
    //{
    //    db = new(Application.Configuration);
    //}
    public DB DB { get { return db; } }
    public XLS XLS
    {
        get
        {
            _XLS ??= new();
            return _XLS;
        }
    }
    public void Load(DB oDB, FunctionParameters oParams)
    {
        if (oParams != null)
            Parameters = oParams;
        db = oDB;
    }
    public void WriteLogDebug(string sMessage) => Logger.WriteLine(TaskName + ": " + sMessage, Logger.TipoLog.Debug);
    public void WriteProgress(string sMessage, int iValueCurrent = 0, int iValueTotal = 0)
    {
        ConfigurationQueue Queue = DB.Configuration.Queues[this.Queue];
        DataTable oDT = DB.Get(Queue.Connection, "SELECT * FROM " + Queue.Table + "Extend WHERE msg_id = " + ID.ToString());
        if (oDT.Rows.Count == 0)
        {
            DB.Execute(Queue.Connection, string.Format("INSERT INTO " + Queue.Table + "Extend (msg_id, msg_valuecurrent, msg_valuetotal, msg_message) VALUES ({0}, {1}, {2}, '{3}')", ID, iValueCurrent, iValueTotal, sMessage.Replace("'", "''")));
        }
        else
        {
            DB.Execute(Queue.Connection, string.Format("UPDATE " + Queue.Table + "Extend SET msg_valuecurrent = {0}, msg_valuetotal = {1}, msg_message = '{2}' WHERE msg_id = {3}", iValueCurrent, iValueTotal, sMessage.Replace("'", "''"), ID));
        }
    }
    //public abstract void Execute();
}
