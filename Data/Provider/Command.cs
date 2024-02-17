namespace Solution.Data.Provider;

/// <summary>
/// Classe per la gestione del Command.
/// </summary>
public class Command //: System.Data.IDbCommand
{
    private Connection _Connection;
    private IDbCommand _Command;
    private Parameters oParas;

    public Connection Connection
    {
        get { return _Connection; }
    }
    /// <summary>
    /// Inizializza l'istanza della classe.
    /// </summary>
    /// <param name="oConnection">Connessione su cui lavorare.</param>
    public Command(Connection oConnection)
    {
        _Connection = oConnection;
        _Command = oConnection.Provider.CreateCommand();
        _Command.CommandTimeout = oConnection.ConnectionTimeout;
        //_Command.Connection = (IDbConnection)this._Connection.Connection;
        _Command.Connection = this._Connection.IDbConnection;
        _Command.Transaction = this._Connection.Transaction;
        oParas = new Parameters(this._Connection, _Command);
    }
    /// <summary>
    /// Inizializza l'istanza della classe.
    /// </summary>
    /// <param name="oConnection">Connessione su cui lavorare.</param>
    /// <param name="cmdText">Comando SQL.</param>
    public Command(Connection oConnection, string cmdText)
    {
        _Connection = oConnection;
        _Command = oConnection.Provider.CreateCommand(cmdText, oConnection.IDbConnection);
        _Command.CommandTimeout = oConnection.ConnectionTimeout;
        _Command.Transaction = this._Connection.Transaction;
        oParas = new Parameters(this._Connection, _Command);
    }
    /// <summary>
    /// Inizializza l'istanza della classe.
    /// </summary>
    /// <param name="oConnection">Connessione su cui lavorare.</param>
    /// <param name="cmdText">Comando SQL.</param>
    /// <param name="oTransaction">Transazione su cui lavorare.</param>
    public Command(Connection oConnection, string cmdText, Transaction oTransaction)
    {
        _Connection = oConnection;
        _Command = oConnection.Provider.CreateCommand(cmdText, oConnection.IDbConnection, oTransaction.IDbTransaction);
        _Command.CommandTimeout = oConnection.ConnectionTimeout;
        oParas = new Parameters(this._Connection, _Command);
    }
    /// <summary>
    /// 
    /// </summary>
    public void Cancel()
    {
        _Command.Cancel();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public System.Data.IDbDataParameter CreateParameter()
    {
        return _Command.CreateParameter();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public System.Int32 ExecuteNonQuery()
    {
        try { return _Command.ExecuteNonQuery(); }
        catch (Exception e) { throw (e); }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="behavior"></param>
    /// <returns></returns>
    public System.Data.IDataReader ExecuteReader(System.Data.CommandBehavior behavior)
    {
        return _Command.ExecuteReader(behavior);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public System.Data.IDataReader ExecuteReader()
    {
        return _Command.ExecuteReader();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public object ExecuteScalar()
    {
        return _Command.ExecuteScalar();
    }
    /// <summary>
    /// 
    /// </summary>
    public void Prepare()
    {
        _Command.Prepare();
    }
    /// <summary>
    /// 
    /// </summary>
    public string CommandText
    {
        get { return _Command.CommandText; }
        set { _Command.CommandText = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public int CommandTimeout
    {
        get { return _Command.CommandTimeout; }
        set { _Command.CommandTimeout = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.CommandType CommandType
    {
        get { return _Command.CommandType; }
        set { _Command.CommandType = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.IDbCommand IDbCommand
    {
        get { return _Command; }
    }
    /// <summary>
    /// 
    /// </summary>
    public Parameters Parameters
    {
        get { return oParas; }
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.IDbTransaction Transaction
    {
        get { return _Command.Transaction; }
        set { _Command.Transaction = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.UpdateRowSource UpdatedRowSource
    {
        get { return _Command.UpdatedRowSource; }
        set { _Command.UpdatedRowSource = value; }
    }
}
