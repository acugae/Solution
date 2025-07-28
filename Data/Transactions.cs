namespace Solution.Data;

/// <summary>
/// Contenete collezione di oggetti cTransaction.
/// </summary>
public class Transactions : System.Collections.Specialized.NameObjectCollectionBase
{
    Connections _oConnections = null;
    /// <summary>
    /// Inizializza l'istanza.
    /// </summary>
    /// <param name="oConnections"></param>
    public Transactions(Connections oConnections)
    {
        _oConnections = oConnections;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Add(string key, Transaction value)
    {
        base.BaseAdd(key, value);
    }
    /// <summary>
    /// 
    /// </summary>
    public void Clear()
    {
        base.BaseClear();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Contains(string key)
    {
        return base.BaseGet(key) != null;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Contains(int key)
    {
        return base.BaseGet(key) != null;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    public void Remove(string key)
    {
        base.BaseRemove(key);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Transaction this[string key]
    {
        get { return (Transaction)(base.BaseGet(key)); }
        set { base.BaseSet(key, value); }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Transaction this[int key]
    {
        get { return (Transaction)(base.BaseGet(key)); }
        set { base.BaseSet(key, value); }
    }
    public object[] Values
    {
        get { return base.BaseGetAllValues(); }
    }
    /// <summary>
    ///	Viene utilizzata per creare una transazione sulla connessione spacificata.
    /// </summary>
    /// <param name="sKeyConnection">Chiave della connessione.</param>
    public void Begin(string sKeyConnection)
    {
        lock (this)
        {
            try
            {
                if (_oConnections is not null && _oConnections.Contains(sKeyConnection))
                {
                    var result = _oConnections[sKeyConnection].BeginTransaction();
                    this[sKeyConnection] = (Transaction)result;
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
        }
    }
    /// <summary>
    ///	Chiude la transazione con esito positivo, conferma le oparazioni effettuare sulla connessione dall'ultima TransactionBegin.
    /// </summary>
    /// <param name="sKeyConnection">Chiave della connessione.</param>
    public void Commit(string sKeyConnection)
    {
        lock (this)
        {
            try
            {
                this[sKeyConnection].Commit();
                Remove(sKeyConnection);
            }
            catch (Exception e)
            {
                throw (e);
            }
        }
    }
    /// <summary>
    ///	Chiude la transazione con esito negativo, annulla le oparazioni effettuare sulla connessione dall'ultima TransactionBegin.
    /// </summary>
    /// <param name="sKeyConnection">Chiave della connessione.</param>
    public void Rollback(string sKeyConnection)
    {
        lock (this)
        {
            try
            {
                this[sKeyConnection].Rollback();
                Remove(sKeyConnection);
            }
            catch (Exception e)
            {
                throw (e);
            }
        }
    }
}
