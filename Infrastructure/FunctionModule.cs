using Solution.Communication;

namespace Solution.Infrastructure;
abstract public class FunctionModule : ControllerBase 
{
    public DB? db = null;
    public string dbKey = null;
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
            _Service ??= new(db, dbKey);
            return _Service;
        }
    }
    public DBConfig DBConfig
    {
        get
        {
            _DBConfig ??= new(db, dbKey);
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
    public void Load(DB oDB, string sKey, FunctionParameters oParams)
    {
        if (oParams != null)
            Parameters = oParams;
        db = oDB;
        dbKey = sKey;
    }
    public void WriteLogDebug(string sMessage) => Logger.WriteLine(TaskName + ": " + sMessage, Logger.TipoLog.Debug);

    //public abstract void Execute();
}
