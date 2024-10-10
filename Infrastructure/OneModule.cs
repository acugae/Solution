using Solution.Communication;

namespace Solution.Infrastructure;
abstract public class OneModule  
{
    public DB? db = null;
    public string dbKey = null;
    public DBConfig? _DBConfig = null;
    public Service? _Service = null;
    private XLS? _XLS = null;
    private Email? _Communication = null;
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
    public DB DB { get { return db; } }
    public XLS XLS
    {
        get
        {
            _XLS ??= new();
            return _XLS;
        }
    }
    public void Load(DB oDB, string sKey)
    {
        db = oDB;
        dbKey = sKey;
    }
}
