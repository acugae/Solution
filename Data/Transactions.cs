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

    // Connessioni aperte da Begin, da richiudere a fine transazione.
    private readonly HashSet<string> _oOpenedHere = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    // Chiude la contabilità della transazione: la connessione non deve più riferirla.
    private void End(string sKeyConnection)
    {
        Remove(sKeyConnection);
        if (_oConnections is not null && _oConnections.Contains(sKeyConnection))
        {
            Connection oConnection = _oConnections[sKeyConnection];
            oConnection.ClearTransaction();
            if (_oOpenedHere.Remove(sKeyConnection) && oConnection.State == ConnectionState.Open)
                oConnection.Close();
        }
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
                    // La transazione vive sulla connessione condivisa della chiave: se è chiusa viene
                    // aperta qui e richiusa a fine transazione.
                    Connection oConnection = _oConnections[sKeyConnection];
                    if (oConnection.State == ConnectionState.Closed)
                    {
                        oConnection.Open();
                        _oOpenedHere.Add(sKeyConnection);
                    }
                    DbTransaction result = oConnection.BeginTransaction();
                    this[sKeyConnection] = result as Transaction ?? new Transaction(result);
                }
            }
            catch (Exception)
            {
                throw;
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
                End(sKeyConnection);
            }
            catch (Exception)
            {
                throw;
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
                End(sKeyConnection);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
