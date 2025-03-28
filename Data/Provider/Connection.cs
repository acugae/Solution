namespace Solution.Data.Provider;

public class Connection 
{
    private Provider provider;
    private DbConnection connection;
    private DbTransaction transaction;
    private string _strKey;
    /// <summary>
    /// Ritona la transazione sulla connessione, altrimenti null.
    /// </summary>
    public DbTransaction Transaction
    {
        get { return transaction; }
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
    public Provider Provider
    {
        get { return provider; }
        set { provider = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    //[WebMethod(true)]
    public void Dispose()
    {
        _strKey = null;
        connection = null;
        return;
    }
    /// <summary>
    /// Costrutture di default.
    /// </summary>
    public Connection()
    {
    }
    /// <summary>
    /// Inizializza la connessione. 
    /// </summary>
    /// <param name="oProvider">Provider pel l'accesso ai dati.</param>
    /// <param name="sKey">Chiave di connessione.</param>
    public Connection(Provider oProvider, string sKey)
    {
        provider = oProvider;
        connection = oProvider.CreateConnection();
        _strKey = sKey;
    }
    /// <summary>
    /// Inizializza la connessione. 
    /// </summary>
    /// <param name="oProvider">Provider pel l'accesso ai dati.</param>
    /// <param name="sKey">Chiave di connessione.</param>
    /// <param name="connectionString">Stringa di connessione.</param>
    public Connection(Provider oProvider, string sKey, string connectionString)
    {
        provider = oProvider;
        _strKey = sKey;
        connection = oProvider.CreateConnection();
        connection.ConnectionString = connectionString;
    }
    /// <summary>
    /// Inizia una transazione sull'istanza specificando l'IsolationLevel.
    /// </summary>
    /// <param name="il"></param>
    /// <returns>Transazione risultante.</returns>
    public DbTransaction BeginTransaction(System.Data.IsolationLevel il)
    {
        transaction = connection.BeginTransaction(il);
        return transaction;
    }
    public DbTransaction BeginTransaction()
    {
        transaction = connection.BeginTransaction();
        return transaction;
    }
    /// <summary>
         /// 
         /// </summary>
         /// <param name="databaseName"></param>
    public void ChangeDatabase(string databaseName) => connection.ChangeDatabase(databaseName);
    public void Close() => connection.Close();
    public async Task CloseAsync() => await connection.CloseAsync();
    /// <summary>
    /// Crea e ritorna un comando associato alla connessione.
    /// </summary>
    /// <returns></returns>
    //[WebMethod(true)]
    public DbCommand CreateCommand()
    {
        return connection.CreateCommand();
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
            return connection.State == System.Data.ConnectionState.Open;
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
        connection.Open();
    }

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        await connection.OpenAsync(cancellationToken);
    }
    /// <summary>
    /// Stringa di connessione.
    /// </summary>
    public string ConnectionString
    {
        get { return connection.ConnectionString; }
        set { connection.ConnectionString = value; }
    }
    /// <summary>
    /// Tempo massimo per l'apertura della connessione.
    /// </summary>
    public int ConnectionTimeout
    {
        get { return connection.ConnectionTimeout; }
    }
    /// <summary>
    /// Database utilizzato dalla connessione.
    /// </summary>
    public string Database
    {
        get { return connection.Database; }
    }
    /// <summary>
    /// Stato della connessione.
    /// </summary>
    public System.Data.ConnectionState State
    {
        get { return connection.State; }
    }
    /// <summary>
    /// 
    /// </summary>
    public DbConnection DbConnection
    {
        get { return connection; }
    }
}


/*
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

public class Connection : DbConnection
{
    private readonly DbConnection _innerConnection;
    private bool _isDisposed;
    private string _connectionString;
    private ConnectionState _state;

    // Constructors
    public Connection(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _state = ConnectionState.Closed;

        // This is a placeholder - in a real implementation, you would need to
        // initialize _innerConnection with a specific provider connection
        // For example: _innerConnection = new SqlConnection(connectionString);
        // For now, we'll throw an exception in methods that require _innerConnection
    }

    public Connection(DbConnection connection)
    {
        _innerConnection = connection ?? throw new ArgumentNullException(nameof(connection));
        _connectionString = connection.ConnectionString;
        _state = connection.State;
    }

    // DbConnection abstract properties implementation
    public override string ConnectionString
    {
        get => _connectionString;
        set
        {
            ThrowIfDisposed();
            _connectionString = value;
            if (_innerConnection != null)
            {
                _innerConnection.ConnectionString = value;
            }
        }
    }

    public override string Database
    {
        get
        {
            ThrowIfDisposed();
            return _innerConnection?.Database ?? string.Empty;
        }
    }

    public override string DataSource
    {
        get
        {
            ThrowIfDisposed();
            return _innerConnection?.DataSource ?? string.Empty;
        }
    }

    public override string ServerVersion
    {
        get
        {
            ThrowIfDisposed();
            EnsureConnectionIsOpen();
            return _innerConnection?.ServerVersion ?? string.Empty;
        }
    }

    public override ConnectionState State
    {
        get
        {
            if (_isDisposed)
                return ConnectionState.Closed;

            return _innerConnection?.State ?? _state;
        }
    }

    // DbConnection abstract methods implementation
    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        ThrowIfDisposed();
        EnsureInnerConnectionExists();
        EnsureConnectionIsOpen();
        return new Transaction(_innerConnection.BeginTransaction(isolationLevel));
    }

    public override void ChangeDatabase(string databaseName)
    {
        ThrowIfDisposed();
        EnsureInnerConnectionExists();
        EnsureConnectionIsOpen();
        _innerConnection.ChangeDatabase(databaseName);
    }

    public override void Close()
    {
        if (!_isDisposed)
        {
            if (_innerConnection != null)
            {
                _innerConnection.Close();
            }
            _state = ConnectionState.Closed;
        }
    }

    public override void Open()
    {
        ThrowIfDisposed();
        EnsureInnerConnectionExists();

        if (State != ConnectionState.Open)
        {
            _innerConnection.Open();
            _state = ConnectionState.Open;
        }
    }

    public override async Task OpenAsync(CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        EnsureInnerConnectionExists();

        if (State != ConnectionState.Open)
        {
            await _innerConnection.OpenAsync(cancellationToken);
            _state = ConnectionState.Open;
        }
    }

    protected override DbCommand CreateDbCommand()
    {
        ThrowIfDisposed();
        EnsureInnerConnectionExists();
        return new Command(_innerConnection.CreateCommand());
    }

    // Convenience methods
    public Command CreateCommand()
    {
        ThrowIfDisposed();
        EnsureInnerConnectionExists();
        return new Command(_innerConnection.CreateCommand());
    }

    public Command CreateCommand(string commandText)
    {
        ThrowIfDisposed();
        EnsureInnerConnectionExists();
        var cmd = _innerConnection.CreateCommand();
        cmd.CommandText = commandText;
        return new Command(cmd);
    }

    public async Task<DbDataReader> ExecuteReaderAsync(string sql)
    {
        ThrowIfDisposed();
        EnsureInnerConnectionExists();
        EnsureConnectionIsOpen();

        using var command = CreateCommand(sql);
        return await command.ExecuteReaderAsync();
    }

    public async Task<object> ExecuteScalarAsync(string sql)
    {
        ThrowIfDisposed();
        EnsureInnerConnectionExists();
        EnsureConnectionIsOpen();

        using var command = CreateCommand(sql);
        return await command.ExecuteScalarAsync();
    }

    public async Task<int> ExecuteNonQueryAsync(string sql)
    {
        ThrowIfDisposed();
        EnsureInnerConnectionExists();
        EnsureConnectionIsOpen();

        using var command = CreateCommand(sql);
        return await command.ExecuteNonQueryAsync();
    }

    public DbTransaction BeginTransaction()
    {
        ThrowIfDisposed();
        EnsureInnerConnectionExists();
        EnsureConnectionIsOpen();
        return new Transaction(_innerConnection.BeginTransaction());
    }

    public DbTransaction BeginTransaction(IsolationLevel isolationLevel)
    {
        ThrowIfDisposed();
        EnsureInnerConnectionExists();
        EnsureConnectionIsOpen();
        return new Transaction(_innerConnection.BeginTransaction(isolationLevel));
    }

    public async Task<DbTransaction> BeginTransactionAsync()
    {
        ThrowIfDisposed();
        EnsureInnerConnectionExists();
        EnsureConnectionIsOpen();

        // Note: DbConnection doesn't have BeginTransactionAsync in all versions
        // This is a workaround using Task.Run for demonstration purposes
        return await Task.Run(() => BeginTransaction());
    }

    public async Task<DbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel)
    {
        ThrowIfDisposed();
        EnsureInnerConnectionExists();
        EnsureConnectionIsOpen();

        // Note: DbConnection doesn't have BeginTransactionAsync in all versions
        // This is a workaround using Task.Run for demonstration purposes
        return await Task.Run(() => BeginTransaction(isolationLevel));
    }

    // Helper methods
    private void EnsureInnerConnectionExists()
    {
        if (_innerConnection == null)
        {
            throw new InvalidOperationException("Connection not properly initialized with a specific database provider.");
        }
    }

    private void EnsureConnectionIsOpen()
    {
        if (State != ConnectionState.Open)
        {
            throw new InvalidOperationException("Connection is not open.");
        }
    }

    // Dispose pattern
    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                Close();
                _innerConnection?.Dispose();
            }

            _isDisposed = true;
            base.Dispose(disposing);
        }
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(Connection));
        }
    }
}
*/

