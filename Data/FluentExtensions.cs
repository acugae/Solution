using Solution.Data.Provider;

namespace Solution.Data;

#region Connection Builder

/// <summary>
/// Builder fluent per configurare connessioni database.
/// </summary>
/// <example>
/// var db = new DB()
///     .AddConnection("main", c => c
///         .UseSqlServer()
///         .WithConnectionString("Server=localhost;Database=mydb;..."))
///     .AddConnection("logs", c => c
///         .UsePostgreSQL()
///         .WithConnectionString("Host=localhost;Database=logs;..."));
/// </example>
public class ConnectionBuilder
{
    private string _providerKey = "sqldb";
    private string _connectionString = string.Empty;
    private readonly DataManager _dataManager;
    private readonly string _connectionKey;

    internal ConnectionBuilder(DataManager dataManager, string connectionKey)
    {
        _dataManager = dataManager;
        _connectionKey = connectionKey;
    }

    /// <summary>
    /// Configura per SQL Server.
    /// </summary>
    public ConnectionBuilder UseSqlServer()
    {
        _providerKey = "sqldb";
        return this;
    }

    /// <summary>
    /// Configura per MySQL.
    /// </summary>
    public ConnectionBuilder UseMySQL()
    {
        _providerKey = "mysdb";
        return this;
    }

    /// <summary>
    /// Configura per PostgreSQL.
    /// </summary>
    public ConnectionBuilder UsePostgreSQL()
    {
        _providerKey = "pstdb";
        return this;
    }

    /// <summary>
    /// Configura un provider custom.
    /// </summary>
    /// <param name="providerKey">Chiave del provider (sqldb, mysdb, pstdb)</param>
    public ConnectionBuilder UseProvider(string providerKey)
    {
        _providerKey = providerKey;
        return this;
    }

    /// <summary>
    /// Imposta la stringa di connessione.
    /// </summary>
    public ConnectionBuilder WithConnectionString(string connectionString)
    {
        _connectionString = connectionString;
        return this;
    }

    /// <summary>
    /// Costruisce la connessione con i parametri SQL Server.
    /// </summary>
    /// <param name="server">Nome server</param>
    /// <param name="database">Nome database</param>
    /// <param name="integratedSecurity">Usa Windows Authentication</param>
    public ConnectionBuilder ForSqlServer(string server, string database, bool integratedSecurity = true)
    {
        _providerKey = "sqldb";
        _connectionString = integratedSecurity
            ? $"Server={server};Database={database};Integrated Security=True;TrustServerCertificate=True;"
            : $"Server={server};Database={database};TrustServerCertificate=True;";
        return this;
    }

    /// <summary>
    /// Costruisce la connessione con i parametri SQL Server (user/password).
    /// </summary>
    public ConnectionBuilder ForSqlServer(string server, string database, string user, string password)
    {
        _providerKey = "sqldb";
        _connectionString = $"Server={server};Database={database};User Id={user};Password={password};TrustServerCertificate=True;";
        return this;
    }

    /// <summary>
    /// Costruisce la connessione con i parametri MySQL.
    /// </summary>
    public ConnectionBuilder ForMySQL(string server, string database, string user, string password, int port = 3306)
    {
        _providerKey = "mysdb";
        _connectionString = $"Server={server};Port={port};Database={database};User={user};Password={password};";
        return this;
    }

    /// <summary>
    /// Costruisce la connessione con i parametri PostgreSQL.
    /// </summary>
    public ConnectionBuilder ForPostgreSQL(string host, string database, string user, string password, int port = 5432)
    {
        _providerKey = "pstdb";
        _connectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password};";
        return this;
    }

    /// <summary>
    /// Applica la configurazione e crea la connessione.
    /// </summary>
    internal void Build()
    {
        if (string.IsNullOrEmpty(_connectionString))
            throw new InvalidOperationException($"Connection string not specified for '{_connectionKey}'");

        _dataManager.Connections.Add(_connectionKey, _providerKey, _connectionString);
    }
}

/// <summary>
/// Extension methods per configurazione fluent di DB.
/// </summary>
public static class DBConnectionExtensions
{
    /// <summary>
    /// Aggiunge una connessione usando il builder fluent.
    /// </summary>
    /// <param name="db">Istanza DB</param>
    /// <param name="connectionKey">Chiave della connessione</param>
    /// <param name="configure">Azione di configurazione</param>
    /// <returns>Istanza DB per chaining</returns>
    /// <example>
    /// var db = new DB()
    ///     .AddConnection("main", c => c.UseSqlServer().WithConnectionString("..."))
    ///     .AddConnection("logs", c => c.UsePostgreSQL().WithConnectionString("..."));
    /// </example>
    public static DB AddConnection(this DB db, string connectionKey, Action<ConnectionBuilder> configure)
    {
        var builder = new ConnectionBuilder(db.DataManager, connectionKey);
        configure(builder);
        builder.Build();
        return db;
    }

    /// <summary>
    /// Aggiunge una connessione SQL Server.
    /// </summary>
    public static DB AddSqlServer(this DB db, string connectionKey, string connectionString)
    {
        db.DataManager.Connections.Add(connectionKey, "sqldb", connectionString);
        return db;
    }

    /// <summary>
    /// Aggiunge una connessione MySQL.
    /// </summary>
    public static DB AddMySQL(this DB db, string connectionKey, string connectionString)
    {
        db.DataManager.Connections.Add(connectionKey, "mysdb", connectionString);
        return db;
    }

    /// <summary>
    /// Aggiunge una connessione PostgreSQL.
    /// </summary>
    public static DB AddPostgreSQL(this DB db, string connectionKey, string connectionString)
    {
        db.DataManager.Connections.Add(connectionKey, "pstdb", connectionString);
        return db;
    }
}

#endregion

#region Transaction Scope

/// <summary>
/// Gestisce una transazione con pattern using/Dispose.
/// Rollback automatico se non viene chiamato Commit().
/// </summary>
/// <example>
/// using (var tx = db.BeginTransaction("main"))
/// {
///     db["main"].Insert("Users", new { Name = "John" });
///     db["main"].Update("Users", new { Status = "Active" }, new { Name = "John" });
///     tx.Commit(); // Se non chiamato, rollback automatico
/// }
/// </example>
public class TransactionScope : IDisposable
{
    private readonly DB _db;
    private readonly string _connectionKey;
    private bool _committed;
    private bool _disposed;

    internal TransactionScope(DB db, string connectionKey)
    {
        _db = db;
        _connectionKey = connectionKey;
        _committed = false;
        _disposed = false;
        
        _db.DataManager.Transactions.Begin(_connectionKey);
    }

    /// <summary>
    /// Conferma la transazione.
    /// </summary>
    public void Commit()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TransactionScope));
        
        if (_committed)
            throw new InvalidOperationException("Transaction already committed");

        _db.DataManager.Transactions.Commit(_connectionKey);
        _committed = true;
    }

    /// <summary>
    /// Annulla la transazione esplicitamente.
    /// </summary>
    public void Rollback()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TransactionScope));
        
        if (_committed)
            throw new InvalidOperationException("Cannot rollback a committed transaction");

        _db.DataManager.Transactions.Rollback(_connectionKey);
        _committed = true; // Prevent double rollback
    }

    /// <summary>
    /// Dispose: esegue rollback se non è stato fatto commit.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (!_committed)
        {
            try
            {
                _db.DataManager.Transactions.Rollback(_connectionKey);
            }
            catch
            {
                // Ignore errors during rollback in dispose
            }
        }
    }
}

/// <summary>
/// Extension methods per transazioni con pattern using.
/// </summary>
public static class DBTransactionExtensions
{
    /// <summary>
    /// Inizia una transazione con pattern using/Dispose.
    /// Rollback automatico se Commit() non viene chiamato.
    /// </summary>
    /// <param name="db">Istanza DB</param>
    /// <param name="connectionKey">Chiave connessione</param>
    /// <returns>TransactionScope da usare con using</returns>
    /// <example>
    /// using (var tx = db.BeginTransaction("main"))
    /// {
    ///     db["main"].Insert("Users", new { Name = "John" });
    ///     tx.Commit();
    /// }
    /// </example>
    public static TransactionScope BeginTransaction(this DB db, string connectionKey)
    {
        return new TransactionScope(db, connectionKey);
    }

    /// <summary>
    /// Esegue un'azione all'interno di una transazione.
    /// Commit automatico se l'azione completa senza eccezioni, rollback altrimenti.
    /// </summary>
    /// <param name="db">Istanza DB</param>
    /// <param name="connectionKey">Chiave connessione</param>
    /// <param name="action">Azione da eseguire</param>
    /// <example>
    /// db.InTransaction("main", () => {
    ///     db["main"].Insert("Users", new { Name = "John" });
    ///     db["main"].Insert("Logs", new { Action = "UserCreated" });
    /// });
    /// </example>
    public static void InTransaction(this DB db, string connectionKey, Action action)
    {
        using var tx = db.BeginTransaction(connectionKey);
        action();
        tx.Commit();
    }

    /// <summary>
    /// Esegue un'azione all'interno di una transazione e restituisce un risultato.
    /// </summary>
    /// <typeparam name="T">Tipo del risultato</typeparam>
    /// <param name="db">Istanza DB</param>
    /// <param name="connectionKey">Chiave connessione</param>
    /// <param name="func">Funzione da eseguire</param>
    /// <returns>Risultato della funzione</returns>
    /// <example>
    /// long? userId = db.InTransaction("main", () => {
    ///     return db["main"].InsertWithReturn("Users", new { Name = "John" });
    /// });
    /// </example>
    public static T InTransaction<T>(this DB db, string connectionKey, Func<T> func)
    {
        using var tx = db.BeginTransaction(connectionKey);
        var result = func();
        tx.Commit();
        return result;
    }
}

#endregion
