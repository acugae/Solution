namespace Solution.Data.Provider;

/// <summary>
/// Classe per la gestione della connessione.
/// </summary>
public class cConnection : System.Data.IDbConnection
{
    private cProvider _Provider;
    private System.Data.IDbConnection _Connection;
    private System.Data.IDbTransaction _Transaction;
    private string _strKey;
    /// <summary>
    /// Ritona la transazione sulla connessione, altrimenti null.
    /// </summary>
    public System.Data.IDbTransaction Transaction
    {
        get { return _Transaction; }
    }
    /// <summary>
    /// Chiave di connessione.
    /// </summary>
    public string Key
    {
        get { return _strKey; }
        set { _strKey = value; }
    }
    /// <summary>
    /// Provider utilizzato dalla connessione.
    /// </summary>
    public cProvider Provider
    {
        get { return _Provider; }
        set { _Provider = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    //[WebMethod(true)]
    public void Dispose()
    {
        _strKey = null;
        _Connection = null;
        return;
    }
    /// <summary>
    /// Costrutture di default.
    /// </summary>
    public cConnection()
    {
    }
    /// <summary>
    /// Inizializza la connessione. 
    /// </summary>
    /// <param name="oProvider">Provider pel l'accesso ai dati.</param>
    /// <param name="sKey">Chiave di connessione.</param>
    public cConnection(cProvider oProvider, string sKey)
    {
        _Provider = oProvider;
        _Connection = oProvider.CreateConnection();
        _strKey = sKey;
    }
    /// <summary>
    /// Inizializza la connessione. 
    /// </summary>
    /// <param name="oProvider">Provider pel l'accesso ai dati.</param>
    /// <param name="sKey">Chiave di connessione.</param>
    /// <param name="connectionString">Stringa di connessione.</param>
    public cConnection(cProvider oProvider, string sKey, string connectionString)
    {
        _Provider = oProvider;
        _strKey = sKey;
        _Connection = oProvider.CreateConnection();
        _Connection.ConnectionString = connectionString;
    }
    /// <summary>
    /// Inizia una transazione sull'istanza specificando l'IsolationLevel.
    /// </summary>
    /// <param name="il"></param>
    /// <returns>Transazione risultante.</returns>
    public System.Data.IDbTransaction BeginTransaction(System.Data.IsolationLevel il)
    {
        _Transaction = _Connection.BeginTransaction(il);
        return _Transaction;
    }
    /// <summary>
    /// Inizia una transazione sull'istanza.
    /// </summary>
    /// <returns>Transazione risultante.</returns>
    public System.Data.IDbTransaction BeginTransaction()
    {
        _Transaction = _Connection.BeginTransaction();
        return _Transaction;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="databaseName"></param>
    public void ChangeDatabase(System.String databaseName)
    {
        _Connection.ChangeDatabase(databaseName);
    }
    /// <summary>
    /// 
    /// </summary>
    //[WebMethod(true)]
    public void Close()
    {
        _Connection.Close();
    }
    /// <summary>
    /// Crea e ritorna un comando associato alla connessione.
    /// </summary>
    /// <returns></returns>
    //[WebMethod(true)]
    public System.Data.IDbCommand CreateCommand()
    {
        return _Connection.CreateCommand();
    }
    /// <summary>
    /// Controlla se la connessione è aperta.
    /// </summary>
    /// <returns></returns>
    //[WebMethod(true)]
    public bool IsOpen()
    {
        try
        {
            return _Connection.State == System.Data.ConnectionState.Open;
        }
        catch { }
        return false;
    }
    /// <summary>
    /// Apre la connessione così come specificato nella stringa di connessione.
    /// </summary>
    //[WebMethod(true)]
    public void Open()
    {
        try
        {
            _Connection.Open();
        }
        catch (Exception ex)
        {
            throw (ex);
        }
    }
    /// <summary>
    /// Stringa di connessione.
    /// </summary>
    public string ConnectionString
    {
        get { return _Connection.ConnectionString; }
        set { _Connection.ConnectionString = value; }
    }
    /// <summary>
    /// Tempo massimo per l'apertura della connessione.
    /// </summary>
    public int ConnectionTimeout
    {
        get { return _Connection.ConnectionTimeout; }
    }
    /// <summary>
    /// Database utilizzato dalla connessione.
    /// </summary>
    public string Database
    {
        get { return _Connection.Database; }
    }
    /// <summary>
    /// Stato della connessione.
    /// </summary>
    public System.Data.ConnectionState State
    {
        get { return _Connection.State; }
    }
    /// <summary>
    /// 
    /// </summary>
    public IDbConnection Connection
    {
        get { return _Connection; }
    }
}
