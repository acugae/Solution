namespace Solution.Data.Provider;
public class Command
{
    private Connection _Connection;
    private DbCommand _Command;
    private Parameters oParas;

    public Connection Connection
    {
        get { return _Connection; }
    }
    public DbCommand DbCommand
    {
        get { return _Command; }
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
        _Command.Connection = _Connection.IDbConnection;
        _Command.Transaction = _Connection.Transaction;
        oParas = new Parameters(_Connection, _Command);
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
    public void Cancel()
    {
        _Command.Cancel();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public DbParameter CreateParameter()
    {
        return _Command.CreateParameter();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int ExecuteNonQuery()
    {
        try { return _Command.ExecuteNonQuery(); }
        catch (Exception e) { throw (e); }
    }
    public async Task<int> ExecuteNonQueryAsync()
    {
        return await _Command.ExecuteNonQueryAsync();
    }
    public async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        return await _Command.ExecuteNonQueryAsync(cancellationToken);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="behavior"></param>
    /// <returns></returns>
    public DbDataReader ExecuteReader(System.Data.CommandBehavior behavior)
    {
        return _Command.ExecuteReader(behavior);
    }
    public DbDataReader ExecuteReader()
    {
        return _Command.ExecuteReader();
    }
    public async Task<DbDataReader> ExecuteReaderAsync()
    {
        return await _Command.ExecuteReaderAsync();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public object? ExecuteScalar()
    {
        return _Command.ExecuteScalar();
    }
    public async Task<object?> ExecuteScalarAsync()
    {
        return await _Command.ExecuteScalarAsync();
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
    public CommandType CommandType
    {
        get { return _Command.CommandType; }
        set { _Command.CommandType = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public DbCommand IDbCommand
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
    public DbTransaction? Transaction
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


/*
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

public class Command : DbCommand
{
    private DbCommand _innerCommand;
    private bool _isDisposed;

    // Constructors
    public Command(DbConnection connection, string commandText)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        _innerCommand = connection.CreateCommand();
        _innerCommand.CommandText = commandText;
        _innerCommand.Connection = connection;
    }

    public Command(DbCommand command)
    {
        _innerCommand = command ?? throw new ArgumentNullException(nameof(command));
    }

    // DbCommand abstract properties implementation
    public override string CommandText
    {
        get => _innerCommand.CommandText;
        set => _innerCommand.CommandText = value;
    }

    public override int CommandTimeout
    {
        get => _innerCommand.CommandTimeout;
        set => _innerCommand.CommandTimeout = value;
    }

    public override CommandType CommandType
    {
        get => _innerCommand.CommandType;
        set => _innerCommand.CommandType = value;
    }

    public override UpdateRowSource UpdatedRowSource
    {
        get => _innerCommand.UpdatedRowSource;
        set => _innerCommand.UpdatedRowSource = value;
    }

    protected override DbConnection DbConnection
    {
        get => _innerCommand.Connection;
        set => _innerCommand.Connection = value;
    }

    protected override DbParameterCollection DbParameterCollection => _innerCommand.Parameters;

    protected override DbTransaction DbTransaction
    {
        get => _innerCommand.Transaction;
        set => _innerCommand.Transaction = value;
    }

    public override bool DesignTimeVisible
    {
        get => _innerCommand.DesignTimeVisible;
        set => _innerCommand.DesignTimeVisible = value;
    }

    // DbCommand abstract methods implementation
    public override void Cancel()
    {
        ThrowIfDisposed();
        _innerCommand.Cancel();
    }

    public override int ExecuteNonQuery()
    {
        ThrowIfDisposed();
        return _innerCommand.ExecuteNonQuery();
    }

    public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        return await _innerCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    public override object ExecuteScalar()
    {
        ThrowIfDisposed();
        return _innerCommand.ExecuteScalar();
    }

    public override async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        return await _innerCommand.ExecuteScalarAsync(cancellationToken);
    }

    public override void Prepare()
    {
        ThrowIfDisposed();
        _innerCommand.Prepare();
    }

    protected override DbParameter CreateDbParameter()
    {
        ThrowIfDisposed();
        return _innerCommand.CreateParameter();
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        ThrowIfDisposed();
        return _innerCommand.ExecuteReader(behavior);
    }

    protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        return await _innerCommand.ExecuteReaderAsync(behavior, cancellationToken);
    }

    // Convenience methods
    public DbDataReader ExecuteReader()
    {
        ThrowIfDisposed();
        return _innerCommand.ExecuteReader();
    }

    public async Task<DbDataReader> ExecuteReaderAsync()
    {
        ThrowIfDisposed();
        return await _innerCommand.ExecuteReaderAsync();
    }

    public DbDataReader ExecuteReader(CommandBehavior behavior)
    {
        ThrowIfDisposed();
        return _innerCommand.ExecuteReader(behavior);
    }

    public async Task<DbDataReader> ExecuteReaderAsync(CommandBehavior behavior)
    {
        ThrowIfDisposed();
        return await _innerCommand.ExecuteReaderAsync(behavior);
    }

    public async Task<DbDataReader> ExecuteReaderAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        return await _innerCommand.ExecuteReaderAsync(cancellationToken);
    }

    // Dispose pattern
    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _innerCommand.Dispose();
            }

            _isDisposed = true;
            base.Dispose(disposing);
        }
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(Command));
        }
    }
}
*/
