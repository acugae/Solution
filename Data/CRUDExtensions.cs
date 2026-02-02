using Solution.Data.Provider;

namespace Solution.Data;

/// <summary>
/// Extension methods per semplificare le operazioni CRUD.
/// Fornisce API intuitive mantenendo la retrocompatibilità con le API esistenti.
/// </summary>
public static class CRUDExtensions
{
    #region Insert

    /// <summary>
    /// Inserisce un record usando un oggetto anonimo o tipizzato.
    /// </summary>
    /// <param name="crud">Istanza CRUD</param>
    /// <param name="tableName">Nome della tabella</param>
    /// <param name="data">Oggetto con le proprietà da inserire</param>
    /// <returns>Numero di righe inserite</returns>
    /// <example>
    /// db["main"].Insert("Users", new { Name = "John", Email = "john@mail.com" });
    /// </example>
    public static int Insert(this CRUD crud, string tableName, object data)
    {
        var item = new CRUDBase(tableName, data.ToDictionary());
        return crud.Insert(item);
    }

    /// <summary>
    /// Inserisce un record e restituisce l'ID generato (SCOPE_IDENTITY).
    /// </summary>
    /// <param name="crud">Istanza CRUD</param>
    /// <param name="tableName">Nome della tabella</param>
    /// <param name="data">Oggetto con le proprietà da inserire</param>
    /// <returns>ID generato o null</returns>
    /// <example>
    /// long? id = db["main"].InsertWithReturn("Users", new { Name = "John" });
    /// </example>
    public static long? InsertWithReturn(this CRUD crud, string tableName, object data)
    {
        var item = new CRUDBase(tableName, data.ToDictionary());
        return crud.InsertWithReturn(item);
    }

    #endregion

    #region Update

    /// <summary>
    /// Aggiorna record usando oggetti anonimi per dati e condizione WHERE.
    /// </summary>
    /// <param name="crud">Istanza CRUD</param>
    /// <param name="tableName">Nome della tabella</param>
    /// <param name="data">Oggetto con le proprietà da aggiornare</param>
    /// <param name="where">Oggetto con le condizioni WHERE (uguaglianza)</param>
    /// <returns>Numero di righe aggiornate</returns>
    /// <example>
    /// db["main"].Update("Users", new { Name = "John Doe" }, new { Id = 1 });
    /// </example>
    public static int Update(this CRUD crud, string tableName, object data, object where)
    {
        var item = new CRUDUpdate(tableName, data.ToDictionary());
        foreach (var kvp in where.ToDictionary())
        {
            item.Filters.AddFilter(kvp.Key, "=", kvp.Value);
        }
        return crud.Update(item);
    }

    #endregion

    #region Delete

    /// <summary>
    /// Elimina record usando un oggetto anonimo per la condizione WHERE.
    /// </summary>
    /// <param name="crud">Istanza CRUD</param>
    /// <param name="tableName">Nome della tabella</param>
    /// <param name="where">Oggetto con le condizioni WHERE (uguaglianza)</param>
    /// <returns>Numero di righe eliminate</returns>
    /// <example>
    /// db["main"].Delete("Users", new { Id = 1 });
    /// </example>
    public static int Delete(this CRUD crud, string tableName, object where)
    {
        var item = new CRUDDelete(tableName);
        foreach (var kvp in where.ToDictionary())
        {
            item.Filters.AddFilter(kvp.Key, "=", kvp.Value);
        }
        return crud.Delete(item);
    }

    #endregion

    #region Query

    /// <summary>
    /// Esegue una query SELECT e mappa i risultati a una lista di oggetti tipizzati.
    /// </summary>
    /// <typeparam name="T">Tipo di destinazione</typeparam>
    /// <param name="crud">Istanza CRUD</param>
    /// <param name="tableName">Nome della tabella</param>
    /// <param name="where">Condizioni WHERE (opzionale)</param>
    /// <returns>Lista di oggetti tipizzati</returns>
    /// <example>
    /// List&lt;User&gt; users = db["main"].Query&lt;User&gt;("Users");
    /// List&lt;User&gt; activeUsers = db["main"].Query&lt;User&gt;("Users", new { Active = true });
    /// </example>
    public static List<T> Query<T>(this CRUD crud, string tableName, object? where = null) where T : new()
    {
        var find = new CRUDFind(tableName);
        if (where != null)
        {
            foreach (var kvp in where.ToDictionary())
            {
                find.Filters.AddFilter(kvp.Key, "=", kvp.Value);
            }
        }
        DataTable dt = crud.Find(find);
        return dt?.To<T>() ?? new List<T>();
    }

    /// <summary>
    /// Esegue una query SELECT e restituisce il primo risultato tipizzato.
    /// </summary>
    /// <typeparam name="T">Tipo di destinazione</typeparam>
    /// <param name="crud">Istanza CRUD</param>
    /// <param name="tableName">Nome della tabella</param>
    /// <param name="where">Condizioni WHERE</param>
    /// <returns>Primo oggetto o default(T)</returns>
    /// <example>
    /// User? user = db["main"].QueryFirst&lt;User&gt;("Users", new { Id = 1 });
    /// </example>
    public static T? QueryFirst<T>(this CRUD crud, string tableName, object where) where T : class, new()
    {
        var results = crud.Query<T>(tableName, where);
        return results.FirstOrDefault();
    }

    /// <summary>
    /// Esegue una query SELECT e restituisce il primo risultato tipizzato (value type).
    /// </summary>
    public static T QueryFirstOrDefault<T>(this CRUD crud, string tableName, object where) where T : struct
    {
        var find = new CRUDFind(tableName);
        foreach (var kvp in where.ToDictionary())
        {
            find.Filters.AddFilter(kvp.Key, "=", kvp.Value);
        }
        DataTable dt = crud.Find(find);
        if (dt == null || dt.Rows.Count == 0)
            return default;
        
        // Per value types, restituisci il valore della prima colonna della prima riga
        return (T)Convert.ChangeType(dt.Rows[0][0], typeof(T));
    }

    #endregion

    #region Exists

    /// <summary>
    /// Verifica se esistono record che corrispondono alle condizioni.
    /// </summary>
    /// <param name="crud">Istanza CRUD</param>
    /// <param name="tableName">Nome della tabella</param>
    /// <param name="where">Condizioni WHERE</param>
    /// <returns>true se esistono record, false altrimenti</returns>
    /// <example>
    /// bool exists = db["main"].Exists("Users", new { Email = "john@mail.com" });
    /// </example>
    public static bool Exists(this CRUD crud, string tableName, object where)
    {
        var find = new CRUDFind(tableName);
        find.Pagination.size = 1;
        foreach (var kvp in where.ToDictionary())
        {
            find.Filters.AddFilter(kvp.Key, "=", kvp.Value);
        }
        DataTable dt = crud.Find(find);
        return dt != null && dt.Rows.Count > 0;
    }

    #endregion

    #region Count

    /// <summary>
    /// Conta i record che corrispondono alle condizioni.
    /// </summary>
    /// <param name="crud">Istanza CRUD</param>
    /// <param name="tableName">Nome della tabella</param>
    /// <param name="where">Condizioni WHERE (opzionale)</param>
    /// <returns>Numero di record</returns>
    /// <example>
    /// int count = db["main"].Count("Users");
    /// int activeCount = db["main"].Count("Users", new { Active = true });
    /// </example>
    public static int Count(this CRUD crud, string tableName, object? where = null)
    {
        var find = new CRUDFind(tableName);
        find.Fields = new List<string> { "COUNT(*)" };
        if (where != null)
        {
            foreach (var kvp in where.ToDictionary())
            {
                find.Filters.AddFilter(kvp.Key, "=", kvp.Value);
            }
        }
        DataTable dt = crud.Find(find);
        if (dt == null || dt.Rows.Count == 0)
            return 0;
        return Convert.ToInt32(dt.Rows[0][0]);
    }

    #endregion

    #region Upsert

    /// <summary>
    /// Inserisce o aggiorna un record (Insert se non esiste, Update se esiste).
    /// </summary>
    /// <param name="crud">Istanza CRUD</param>
    /// <param name="tableName">Nome della tabella</param>
    /// <param name="data">Oggetto con tutte le proprietà</param>
    /// <param name="keyColumns">Nomi delle colonne chiave per identificare il record</param>
    /// <returns>Numero di righe interessate</returns>
    /// <example>
    /// db["main"].Upsert("Users", new { Id = 1, Name = "John", Email = "john@mail.com" }, "Id");
    /// </example>
    public static int Upsert(this CRUD crud, string tableName, object data, params string[] keyColumns)
    {
        var item = new CRUDBase(tableName, data.ToDictionary());
        string operation;
        return crud.Set(item, keyColumns, out operation);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Converte un oggetto (anonimo o tipizzato) in Dictionary.
    /// </summary>
    private static Dictionary<string, object> ToDictionary(this object obj)
    {
        if (obj == null)
            return new Dictionary<string, object>();

        if (obj is Dictionary<string, object> dict)
            return dict;

        var result = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.CanRead)
            {
                result[prop.Name] = prop.GetValue(obj);
            }
        }
        return result;
    }

    #endregion
}

/// <summary>
/// Extension methods per DB per query parametrizzate semplificate.
/// </summary>
public static class DBExtensions
{
    /// <summary>
    /// Esegue una query SQL con parametri da oggetto anonimo.
    /// </summary>
    /// <param name="db">Istanza DB</param>
    /// <param name="connectionKey">Chiave connessione</param>
    /// <param name="sql">Query SQL con parametri @NomeParametro</param>
    /// <param name="parameters">Oggetto anonimo con i parametri</param>
    /// <returns>DataTable con i risultati</returns>
    /// <example>
    /// var dt = db.Query("main", "SELECT * FROM Users WHERE Id = @Id AND Active = @Active", new { Id = 1, Active = true });
    /// </example>
    public static DataTable Query(this DB db, string connectionKey, string sql, object? parameters = null)
    {
        var paramDict = parameters?.ToParameterDictionary();
        return db.Get(connectionKey, sql, paramDict);
    }

    /// <summary>
    /// Esegue una query SQL e mappa i risultati a oggetti tipizzati.
    /// </summary>
    /// <typeparam name="T">Tipo di destinazione</typeparam>
    /// <param name="db">Istanza DB</param>
    /// <param name="connectionKey">Chiave connessione</param>
    /// <param name="sql">Query SQL</param>
    /// <param name="parameters">Parametri (opzionale)</param>
    /// <returns>Lista di oggetti tipizzati</returns>
    /// <example>
    /// var users = db.Query&lt;User&gt;("main", "SELECT * FROM Users WHERE Active = @Active", new { Active = true });
    /// </example>
    public static List<T> Query<T>(this DB db, string connectionKey, string sql, object? parameters = null) where T : new()
    {
        var dt = db.Query(connectionKey, sql, parameters);
        return dt?.To<T>() ?? new List<T>();
    }

    /// <summary>
    /// Esegue una query SQL e restituisce il primo risultato tipizzato.
    /// </summary>
    /// <typeparam name="T">Tipo di destinazione</typeparam>
    /// <param name="db">Istanza DB</param>
    /// <param name="connectionKey">Chiave connessione</param>
    /// <param name="sql">Query SQL</param>
    /// <param name="parameters">Parametri (opzionale)</param>
    /// <returns>Primo oggetto o null</returns>
    /// <example>
    /// var user = db.QueryFirst&lt;User&gt;("main", "SELECT * FROM Users WHERE Id = @Id", new { Id = 1 });
    /// </example>
    public static T? QueryFirst<T>(this DB db, string connectionKey, string sql, object? parameters = null) where T : class, new()
    {
        var results = db.Query<T>(connectionKey, sql, parameters);
        return results.FirstOrDefault();
    }

    /// <summary>
    /// Esegue una query SQL e restituisce un valore scalare.
    /// </summary>
    /// <typeparam name="T">Tipo del valore</typeparam>
    /// <param name="db">Istanza DB</param>
    /// <param name="connectionKey">Chiave connessione</param>
    /// <param name="sql">Query SQL</param>
    /// <param name="parameters">Parametri (opzionale)</param>
    /// <returns>Valore scalare o default</returns>
    /// <example>
    /// int count = db.Scalar&lt;int&gt;("main", "SELECT COUNT(*) FROM Users WHERE Active = @Active", new { Active = true });
    /// </example>
    public static T Scalar<T>(this DB db, string connectionKey, string sql, object? parameters = null)
    {
        var dt = db.Query(connectionKey, sql, parameters);
        if (dt == null || dt.Rows.Count == 0 || dt.Columns.Count == 0)
            return default!;
        
        var value = dt.Rows[0][0];
        if (value == null || value == DBNull.Value)
            return default!;
        
        return (T)Convert.ChangeType(value, typeof(T));
    }

    /// <summary>
    /// Esegue un comando SQL (INSERT, UPDATE, DELETE) con parametri da oggetto anonimo.
    /// </summary>
    /// <param name="db">Istanza DB</param>
    /// <param name="connectionKey">Chiave connessione</param>
    /// <param name="sql">Comando SQL</param>
    /// <param name="parameters">Oggetto anonimo con i parametri</param>
    /// <returns>Numero di righe interessate</returns>
    /// <example>
    /// int affected = db.Execute("main", "UPDATE Users SET Name = @Name WHERE Id = @Id", new { Name = "John", Id = 1 });
    /// </example>
    public static int Execute(this DB db, string connectionKey, string sql, object parameters)
    {
        // Costruisce la query con i parametri inline in modo sicuro
        var paramList = new List<Parameter>();
        var conn = db.DataManager.Connections[connectionKey];
        
        foreach (var prop in parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.CanRead)
            {
                var param = new Parameter(conn)
                {
                    ParameterName = "@" + prop.Name,
                    Value = prop.GetValue(parameters) ?? DBNull.Value
                };
                paramList.Add(param);
            }
        }
        
        return db.InvokeSQL(connectionKey, sql, paramList.ToArray()).Rows.Count > 0 ? 
            Convert.ToInt32(db.InvokeSQL(connectionKey, sql + ";SELECT @@ROWCOUNT;", paramList.ToArray()).Rows[0][0]) : 0;
    }

    /// <summary>
    /// Converte un oggetto in Dictionary per parametri.
    /// </summary>
    private static Dictionary<string, object>? ToParameterDictionary(this object obj)
    {
        if (obj == null)
            return null;

        if (obj is Dictionary<string, object> dict)
            return dict;

        var result = new Dictionary<string, object>();
        foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.CanRead)
            {
                result[prop.Name] = prop.GetValue(obj);
            }
        }
        return result;
    }
}
