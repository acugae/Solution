using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Solution.Data;

/// <summary>
/// Extension methods per la registrazione di DB nel container di Dependency Injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra DB come Scoped service con configurazione fluent.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Azione di configurazione</param>
    /// <returns>Service collection per chaining</returns>
    /// <example>
    /// services.AddSolutionDB(db => db
    ///     .AddSqlServer("main", "Server=localhost;Database=mydb;..."));
    /// </example>
    public static IServiceCollection AddSolutionDB(this IServiceCollection services, Action<DB> configure)
    {
        return services.AddSolutionDB(configure, ServiceLifetime.Scoped);
    }

    /// <summary>
    /// Registra DB con ciclo di vita specificato.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Azione di configurazione</param>
    /// <param name="lifetime">Ciclo di vita del servizio</param>
    /// <returns>Service collection per chaining</returns>
    /// <example>
    /// // Singleton (una istanza per tutta l'applicazione)
    /// services.AddSolutionDB(db => db
    ///     .AddSqlServer("main", connectionString), ServiceLifetime.Singleton);
    /// 
    /// // Scoped (una istanza per request - default, consigliato per web)
    /// services.AddSolutionDB(db => db
    ///     .AddSqlServer("main", connectionString), ServiceLifetime.Scoped);
    /// 
    /// // Transient (nuova istanza ogni volta)
    /// services.AddSolutionDB(db => db
    ///     .AddSqlServer("main", connectionString), ServiceLifetime.Transient);
    /// </example>
    public static IServiceCollection AddSolutionDB(this IServiceCollection services, Action<DB> configure, ServiceLifetime lifetime)
    {
        var descriptor = new ServiceDescriptor(typeof(DB), sp =>
        {
            var db = new DB();
            configure(db);
            return db;
        }, lifetime);

        services.Add(descriptor);
        return services;
    }

    /// <summary>
    /// Registra DB come Scoped con connection string da configuration.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="connectionName">Nome della connessione</param>
    /// <param name="connectionString">Stringa di connessione</param>
    /// <param name="providerType">Tipo di database</param>
    /// <returns>Service collection per chaining</returns>
    /// <example>
    /// services.AddSolutionDB("main", Configuration.GetConnectionString("DefaultConnection"), DatabaseProvider.SqlServer);
    /// </example>
    public static IServiceCollection AddSolutionDB(this IServiceCollection services, string connectionName, string connectionString, DatabaseProvider providerType = DatabaseProvider.SqlServer)
    {
        return services.AddSolutionDB(db =>
        {
            switch (providerType)
            {
                case DatabaseProvider.SqlServer:
                    db.AddSqlServer(connectionName, connectionString);
                    break;
                case DatabaseProvider.MySQL:
                    db.AddMySQL(connectionName, connectionString);
                    break;
                case DatabaseProvider.PostgreSQL:
                    db.AddPostgreSQL(connectionName, connectionString);
                    break;
            }
        });
    }

    /// <summary>
    /// Registra DB leggendo la configurazione da IConfiguration.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Opzioni di configurazione</param>
    /// <returns>Service collection per chaining</returns>
    /// <example>
    /// services.AddSolutionDB(options => {
    ///     options.Connections.Add(new ConnectionOptions {
    ///         Name = "main",
    ///         ConnectionString = "Server=...",
    ///         Provider = DatabaseProvider.SqlServer
    ///     });
    /// });
    /// </example>
    public static IServiceCollection AddSolutionDB(this IServiceCollection services, Action<SolutionDBOptions> configureOptions)
    {
        var options = new SolutionDBOptions();
        configureOptions(options);

        return services.AddSolutionDB(db =>
        {
            foreach (var conn in options.Connections)
            {
                switch (conn.Provider)
                {
                    case DatabaseProvider.SqlServer:
                        db.AddSqlServer(conn.Name, conn.ConnectionString);
                        break;
                    case DatabaseProvider.MySQL:
                        db.AddMySQL(conn.Name, conn.ConnectionString);
                        break;
                    case DatabaseProvider.PostgreSQL:
                        db.AddPostgreSQL(conn.Name, conn.ConnectionString);
                        break;
                }
            }
        }, options.Lifetime);
    }
}

/// <summary>
/// Tipo di database provider.
/// </summary>
public enum DatabaseProvider
{
    SqlServer,
    MySQL,
    PostgreSQL
}

/// <summary>
/// Opzioni di configurazione per SolutionDB.
/// </summary>
public class SolutionDBOptions
{
    /// <summary>
    /// Lista delle connessioni da configurare.
    /// </summary>
    public List<ConnectionOptions> Connections { get; set; } = new();

    /// <summary>
    /// Ciclo di vita del servizio (default: Scoped).
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;
}

/// <summary>
/// Opzioni per una singola connessione.
/// </summary>
public class ConnectionOptions
{
    /// <summary>
    /// Nome/chiave della connessione.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Stringa di connessione.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Tipo di database.
    /// </summary>
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.SqlServer;
}

#region Configuration Binding Extensions

/// <summary>
/// Extension methods per configurazione da IConfiguration (appsettings.json).
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Registra DB leggendo la configurazione da una sezione IConfiguration.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Sezione di configurazione</param>
    /// <returns>Service collection per chaining</returns>
    /// <example>
    /// // In Program.cs
    /// services.AddSolutionDB(Configuration.GetSection("SolutionDB"));
    /// 
    /// // In appsettings.json
    /// {
    ///   "SolutionDB": {
    ///     "Connections": [
    ///       {
    ///         "Name": "main",
    ///         "Provider": "SqlServer",
    ///         "ConnectionString": "Server=localhost;Database=mydb;..."
    ///       },
    ///       {
    ///         "Name": "logs",
    ///         "Provider": "PostgreSQL",
    ///         "ConnectionString": "Host=localhost;Database=logs;..."
    ///       }
    ///     ]
    ///   }
    /// }
    /// </example>
    public static IServiceCollection AddSolutionDB(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new SolutionDBOptions();
        configuration.Bind(options);

        // Parsing del provider da stringa (per supportare JSON)
        var connectionsSection = configuration.GetSection("Connections");
        if (connectionsSection.Exists())
        {
            options.Connections.Clear();
            foreach (var connSection in connectionsSection.GetChildren())
            {
                var conn = new ConnectionOptions
                {
                    Name = connSection["Name"] ?? string.Empty,
                    ConnectionString = connSection["ConnectionString"] ?? string.Empty,
                    Provider = ParseProvider(connSection["Provider"])
                };
                options.Connections.Add(conn);
            }
        }
        
        var lifetimeStr = configuration["Lifetime"];
        if (!string.IsNullOrEmpty(lifetimeStr) && Enum.TryParse<ServiceLifetime>(lifetimeStr, true, out var lifetime))
        {
            options.Lifetime = lifetime;
        }

        return services.AddSolutionDB(db =>
        {
            foreach (var conn in options.Connections)
            {
                switch (conn.Provider)
                {
                    case DatabaseProvider.SqlServer:
                        db.AddSqlServer(conn.Name, conn.ConnectionString);
                        break;
                    case DatabaseProvider.MySQL:
                        db.AddMySQL(conn.Name, conn.ConnectionString);
                        break;
                    case DatabaseProvider.PostgreSQL:
                        db.AddPostgreSQL(conn.Name, conn.ConnectionString);
                        break;
                }
            }
        }, options.Lifetime);
    }

    /// <summary>
    /// Registra DB leggendo le ConnectionStrings standard e mappandole.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configurazione root</param>
    /// <param name="connectionMappings">Mappatura nome → provider</param>
    /// <returns>Service collection per chaining</returns>
    /// <example>
    /// // In Program.cs
    /// services.AddSolutionDBFromConnectionStrings(Configuration, 
    ///     new Dictionary&lt;string, DatabaseProvider&gt; {
    ///         { "Main", DatabaseProvider.SqlServer },
    ///         { "Logs", DatabaseProvider.PostgreSQL }
    ///     });
    /// 
    /// // In appsettings.json (formato standard ASP.NET Core)
    /// {
    ///   "ConnectionStrings": {
    ///     "Main": "Server=localhost;Database=mydb;...",
    ///     "Logs": "Host=localhost;Database=logs;..."
    ///   }
    /// }
    /// </example>
    public static IServiceCollection AddSolutionDBFromConnectionStrings(
        this IServiceCollection services, 
        IConfiguration configuration,
        Dictionary<string, DatabaseProvider> connectionMappings)
    {
        return services.AddSolutionDB(db =>
        {
            foreach (var mapping in connectionMappings)
            {
                var connectionString = configuration.GetConnectionString(mapping.Key);
                if (string.IsNullOrEmpty(connectionString))
                    continue;

                switch (mapping.Value)
                {
                    case DatabaseProvider.SqlServer:
                        db.AddSqlServer(mapping.Key, connectionString);
                        break;
                    case DatabaseProvider.MySQL:
                        db.AddMySQL(mapping.Key, connectionString);
                        break;
                    case DatabaseProvider.PostgreSQL:
                        db.AddPostgreSQL(mapping.Key, connectionString);
                        break;
                }
            }
        });
    }

    /// <summary>
    /// Registra DB con una singola ConnectionString dal formato standard.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configurazione root</param>
    /// <param name="connectionStringName">Nome della connection string</param>
    /// <param name="provider">Provider database (default: SqlServer)</param>
    /// <returns>Service collection per chaining</returns>
    /// <example>
    /// // Usa una connection string specifica
    /// services.AddSolutionDBFromConnectionString(Configuration, "Main");
    /// services.AddSolutionDBFromConnectionString(Configuration, "Logs", DatabaseProvider.PostgreSQL);
    /// </example>
    public static IServiceCollection AddSolutionDBFromConnectionString(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName,
        DatabaseProvider provider = DatabaseProvider.SqlServer)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException($"Connection string '{connectionStringName}' not found in configuration.");

        return services.AddSolutionDB(connectionStringName, connectionString, provider);
    }

    private static DatabaseProvider ParseProvider(string? provider)
    {
        if (string.IsNullOrEmpty(provider))
            return DatabaseProvider.SqlServer;

        return provider.ToLowerInvariant() switch
        {
            "sqlserver" or "mssql" or "sql" => DatabaseProvider.SqlServer,
            "mysql" or "mariadb" => DatabaseProvider.MySQL,
            "postgresql" or "postgres" or "pgsql" => DatabaseProvider.PostgreSQL,
            _ => Enum.TryParse<DatabaseProvider>(provider, true, out var result) 
                ? result 
                : DatabaseProvider.SqlServer
        };
    }
}

#endregion
