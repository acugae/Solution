namespace Solution.Data;

/// <summary>
/// Query builder fluent per costruire query SELECT in modo intuitivo.
/// Versione "light" per query semplici senza join/subquery.
/// </summary>
/// <example>
/// var users = db["main"]
///     .From("Users")
///     .Where(new { Active = true, Role = "Admin" })
///     .OrderBy("Name")
///     .Take(10)
///     .Select&lt;User&gt;();
/// </example>
public class QueryBuilder
{
    private readonly CRUD _crud;
    private readonly DB _db;
    private readonly string _connectionKey;
    private string _tableName = string.Empty;
    private readonly List<QueryFilter> _filters = new();
    private readonly List<QueryOrder> _orders = new();
    private readonly List<string> _columns = new();
    private int? _take;
    private int? _skip;

    internal QueryBuilder(CRUD crud, DB db, string connectionKey)
    {
        _crud = crud;
        _db = db;
        _connectionKey = connectionKey;
    }

    /// <summary>
    /// Specifica la tabella da cui selezionare.
    /// </summary>
    /// <param name="tableName">Nome della tabella</param>
    /// <returns>QueryBuilder per chaining</returns>
    public QueryBuilder From(string tableName)
    {
        _tableName = tableName;
        return this;
    }

    /// <summary>
    /// Specifica le colonne da selezionare.
    /// </summary>
    /// <param name="columns">Nomi delle colonne</param>
    /// <returns>QueryBuilder per chaining</returns>
    /// <example>
    /// .Columns("Id", "Name", "Email")
    /// </example>
    public QueryBuilder Columns(params string[] columns)
    {
        _columns.AddRange(columns);
        return this;
    }

    /// <summary>
    /// Aggiunge condizioni WHERE usando un oggetto anonimo (uguaglianza).
    /// </summary>
    /// <param name="conditions">Oggetto con le condizioni</param>
    /// <returns>QueryBuilder per chaining</returns>
    /// <example>
    /// .Where(new { Active = true, Role = "Admin" })
    /// </example>
    public QueryBuilder Where(object conditions)
    {
        foreach (var prop in conditions.GetType().GetProperties())
        {
            _filters.Add(new QueryFilter(prop.Name, "=", prop.GetValue(conditions)));
        }
        return this;
    }

    /// <summary>
    /// Aggiunge una condizione WHERE con operatore specificato.
    /// </summary>
    /// <param name="column">Nome colonna</param>
    /// <param name="op">Operatore (=, >, <, >=, <=, <>, LIKE, IN)</param>
    /// <param name="value">Valore</param>
    /// <returns>QueryBuilder per chaining</returns>
    /// <example>
    /// .Where("Age", ">", 18)
    /// .Where("Name", "LIKE", "%John%")
    /// </example>
    public QueryBuilder Where(string column, string op, object value)
    {
        _filters.Add(new QueryFilter(column, op, value));
        return this;
    }

    /// <summary>
    /// Aggiunge una condizione WHERE con uguaglianza.
    /// </summary>
    /// <param name="column">Nome colonna</param>
    /// <param name="value">Valore</param>
    /// <returns>QueryBuilder per chaining</returns>
    public QueryBuilder Where(string column, object value)
    {
        return Where(column, "=", value);
    }

    /// <summary>
    /// Aggiunge condizione IS NULL.
    /// </summary>
    public QueryBuilder WhereNull(string column)
    {
        _filters.Add(new QueryFilter(column, "IS", null));
        return this;
    }

    /// <summary>
    /// Aggiunge condizione IS NOT NULL.
    /// </summary>
    public QueryBuilder WhereNotNull(string column)
    {
        _filters.Add(new QueryFilter(column, "IS NOT", null));
        return this;
    }

    /// <summary>
    /// Aggiunge condizione IN.
    /// </summary>
    /// <param name="column">Nome colonna</param>
    /// <param name="values">Valori</param>
    /// <returns>QueryBuilder per chaining</returns>
    /// <example>
    /// .WhereIn("Status", "Active", "Pending", "Review")
    /// </example>
    public QueryBuilder WhereIn(string column, params object[] values)
    {
        _filters.Add(new QueryFilter(column, "IN", values));
        return this;
    }

    /// <summary>
    /// Aggiunge condizione BETWEEN.
    /// </summary>
    /// <param name="column">Nome colonna</param>
    /// <param name="from">Valore minimo</param>
    /// <param name="to">Valore massimo</param>
    /// <returns>QueryBuilder per chaining</returns>
    public QueryBuilder WhereBetween(string column, object from, object to)
    {
        _filters.Add(new QueryFilter(column, "BETWEEN", new[] { from, to }));
        return this;
    }

    /// <summary>
    /// Ordina per colonna (ascendente).
    /// </summary>
    /// <param name="column">Nome colonna</param>
    /// <returns>QueryBuilder per chaining</returns>
    public QueryBuilder OrderBy(string column)
    {
        _orders.Add(new QueryOrder(column, "ASC"));
        return this;
    }

    /// <summary>
    /// Ordina per colonna con direzione specificata.
    /// </summary>
    /// <param name="column">Nome colonna</param>
    /// <param name="direction">Direzione (asc/desc)</param>
    /// <returns>QueryBuilder per chaining</returns>
    public QueryBuilder OrderBy(string column, string direction)
    {
        _orders.Add(new QueryOrder(column, direction.ToUpper()));
        return this;
    }

    /// <summary>
    /// Ordina per colonna (discendente).
    /// </summary>
    /// <param name="column">Nome colonna</param>
    /// <returns>QueryBuilder per chaining</returns>
    public QueryBuilder OrderByDesc(string column)
    {
        _orders.Add(new QueryOrder(column, "DESC"));
        return this;
    }

    /// <summary>
    /// Limita il numero di risultati.
    /// </summary>
    /// <param name="count">Numero massimo di righe</param>
    /// <returns>QueryBuilder per chaining</returns>
    public QueryBuilder Take(int count)
    {
        _take = count;
        return this;
    }

    /// <summary>
    /// Salta un numero di risultati (per paginazione).
    /// </summary>
    /// <param name="count">Numero di righe da saltare</param>
    /// <returns>QueryBuilder per chaining</returns>
    public QueryBuilder Skip(int count)
    {
        _skip = count;
        return this;
    }

    /// <summary>
    /// Paginazione: imposta pagina e dimensione.
    /// </summary>
    /// <param name="page">Numero pagina (1-based)</param>
    /// <param name="pageSize">Dimensione pagina</param>
    /// <returns>QueryBuilder per chaining</returns>
    /// <example>
    /// .Page(2, 10) // Seconda pagina, 10 elementi per pagina
    /// </example>
    public QueryBuilder Page(int page, int pageSize)
    {
        _skip = (page - 1) * pageSize;
        _take = pageSize;
        return this;
    }

    /// <summary>
    /// Esegue la query e restituisce DataTable.
    /// </summary>
    /// <returns>DataTable con i risultati</returns>
    public DataTable Select()
    {
        var (sql, parameters) = BuildParameterizedSql();
        return parameters.Count > 0 ? _db.Get(_connectionKey, sql, parameters) : _db.Get(_connectionKey, sql);
    }

    /// <summary>
    /// Esegue la query e mappa i risultati a oggetti tipizzati.
    /// </summary>
    /// <typeparam name="T">Tipo di destinazione</typeparam>
    /// <returns>Lista di oggetti tipizzati</returns>
    public List<T> Select<T>() where T : new()
    {
        var dt = Select();
        return dt?.To<T>() ?? new List<T>();
    }

    /// <summary>
    /// Esegue la query e restituisce il primo risultato.
    /// </summary>
    /// <typeparam name="T">Tipo di destinazione</typeparam>
    /// <returns>Primo oggetto o null</returns>
    public T? SelectFirst<T>() where T : class, new()
    {
        _take = 1;
        var results = Select<T>();
        return results.FirstOrDefault();
    }

    /// <summary>
    /// Esegue la query e restituisce il primo risultato o default.
    /// </summary>
    /// <typeparam name="T">Tipo di destinazione</typeparam>
    /// <returns>Primo oggetto o default(T)</returns>
    public T SelectFirstOrDefault<T>() where T : new()
    {
        _take = 1;
        var results = Select<T>();
        return results.FirstOrDefault() ?? new T();
    }

    /// <summary>
    /// Conta i record che corrispondono ai filtri.
    /// </summary>
    /// <returns>Numero di record</returns>
    public int Count()
    {
        var (sql, parameters) = BuildParameterizedCountSql();
        var dt = parameters.Count > 0 ? _db.Get(_connectionKey, sql, parameters) : _db.Get(_connectionKey, sql);
        if (dt == null || dt.Rows.Count == 0)
            return 0;
        return Convert.ToInt32(dt.Rows[0][0]);
    }

    /// <summary>
    /// Verifica se esistono record che corrispondono ai filtri.
    /// </summary>
    /// <returns>true se esistono record</returns>
    public bool Exists()
    {
        _take = 1;
        _columns.Clear();
        _columns.Add("1");
        var dt = Select();
        return dt != null && dt.Rows.Count > 0;
    }

    /// <summary>
    /// Restituisce la query SQL generata (utile per debug).
    /// </summary>
    /// <returns>Query SQL</returns>
    public string ToSql() => BuildParameterizedSql().Sql;

    #region SQL Building

    private (string Sql, Dictionary<string, object> Parameters) BuildParameterizedSql()
    {
        if (string.IsNullOrEmpty(_tableName))
            throw new InvalidOperationException("Table name not specified. Use From() method.");

        string safeTable = SqlSecurityHelper.QuoteIdentifier(_tableName, nameof(_tableName));
        var sb = new StringBuilder();
        var parameters = new Dictionary<string, object>();
        int paramIndex = 0;

        // SELECT
        sb.Append("SELECT ");
        if (_columns.Count > 0)
            sb.Append(string.Join(", ", _columns.Select(c => SqlSecurityHelper.ValidateSelectField(c, nameof(_columns)))));
        else
            sb.Append("*");

        // FROM
        sb.Append($" FROM {safeTable}");

        // WHERE
        if (_filters.Count > 0)
        {
            sb.Append(" WHERE ");
            sb.Append(string.Join(" AND ", _filters.Select(f => f.ToParameterizedSql(parameters, ref paramIndex))));
        }

        // ORDER BY
        if (_orders.Count > 0)
        {
            sb.Append(" ORDER BY ");
            sb.Append(string.Join(", ", _orders.Select(o => o.ToSql())));
        }
        else if (_skip.HasValue || _take.HasValue)
        {
            // ORDER BY richiesto per OFFSET/FETCH
            sb.Append(" ORDER BY (SELECT NULL)");
        }

        // OFFSET/FETCH (SQL Server syntax)
        if (_skip.HasValue || _take.HasValue)
        {
            sb.Append($" OFFSET {Math.Max(0, _skip ?? 0)} ROWS");
            if (_take.HasValue)
                sb.Append($" FETCH NEXT {Math.Max(0, _take.Value)} ROWS ONLY");
        }

        return (sb.ToString(), parameters);
    }

    private (string Sql, Dictionary<string, object> Parameters) BuildParameterizedCountSql()
    {
        if (string.IsNullOrEmpty(_tableName))
            throw new InvalidOperationException("Table name not specified. Use From() method.");

        string safeTable = SqlSecurityHelper.QuoteIdentifier(_tableName, nameof(_tableName));
        var sb = new StringBuilder();
        var parameters = new Dictionary<string, object>();
        int paramIndex = 0;

        sb.Append($"SELECT COUNT(*) FROM {safeTable}");

        if (_filters.Count > 0)
        {
            sb.Append(" WHERE ");
            sb.Append(string.Join(" AND ", _filters.Select(f => f.ToParameterizedSql(parameters, ref paramIndex))));
        }

        return (sb.ToString(), parameters);
    }

    #endregion
}

#region Internal Classes

internal class QueryFilter
{
    public string Column { get; }
    public string Operator { get; }
    public object? Value { get; }

    public QueryFilter(string column, string op, object? value)
    {
        Column = SqlSecurityHelper.QuoteIdentifier(column, nameof(column));
        Operator = SqlSecurityHelper.ValidateOperator(op, nameof(op));
        Value = value;
    }

    public string ToParameterizedSql(Dictionary<string, object> parameters, ref int paramIndex)
    {
        switch (Operator)
        {
            case "IS":
                return $"{Column} IS NULL";
            case "IS NOT":
                return $"{Column} IS NOT NULL";
            case "IN":
            case "NOT IN":
                if (Value is object[] values)
                {
                    if (values.Length == 0)
                        return Operator == "IN" ? "1 = 0" : "1 = 1";
                    var pNames = new List<string>(values.Length);
                    foreach (var v in values)
                    {
                        string pName = $"@qb_{paramIndex++}";
                        pNames.Add(pName);
                        parameters[pName] = v ?? DBNull.Value;
                    }
                    return $"{Column} {Operator} ({string.Join(", ", pNames)})";
                }
                {
                    string pName = $"@qb_{paramIndex++}";
                    parameters[pName] = Value ?? DBNull.Value;
                    return $"{Column} {Operator} ({pName})";
                }
            case "BETWEEN":
                if (Value is object[] range && range.Length == 2)
                {
                    string p1 = $"@qb_{paramIndex++}";
                    string p2 = $"@qb_{paramIndex++}";
                    parameters[p1] = range[0] ?? DBNull.Value;
                    parameters[p2] = range[1] ?? DBNull.Value;
                    return $"{Column} BETWEEN {p1} AND {p2}";
                }
                throw new InvalidOperationException("BETWEEN requires two values");
            default:
                {
                    string pName = $"@qb_{paramIndex++}";
                    parameters[pName] = Value ?? DBNull.Value;
                    return $"{Column} {Operator} {pName}";
                }
        }
    }
}

internal class QueryOrder
{
    public string Column { get; }
    public string Direction { get; }

    public QueryOrder(string column, string direction)
    {
        Column = SqlSecurityHelper.QuoteIdentifier(column, nameof(column));
        Direction = SqlSecurityHelper.ValidateSortDirection(direction, nameof(direction));
    }

    public string ToSql() => $"{Column} {Direction}";
}

#endregion

/// <summary>
/// Extension methods per aggiungere QueryBuilder a CRUD.
/// </summary>
public static class QueryBuilderExtensions
{
    /// <summary>
    /// Inizia una query fluent sulla tabella specificata.
    /// </summary>
    /// <param name="crud">Istanza CRUD</param>
    /// <param name="tableName">Nome della tabella</param>
    /// <returns>QueryBuilder per costruire la query</returns>
    /// <example>
    /// var users = db["main"]
    ///     .From("Users")
    ///     .Where(new { Active = true })
    ///     .OrderBy("Name")
    ///     .Take(10)
    ///     .Select&lt;User&gt;();
    /// </example>
    public static QueryBuilder From(this CRUD crud, string tableName)
    {
        // Otteniamo DB e connection key tramite reflection (necessario per mantenere retrocompatibilità)
        var dbField = typeof(CRUD).GetField("oDB", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var keyField = typeof(CRUD).GetField("_sKey", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var db = (DB)dbField?.GetValue(crud)!;
        var connectionKey = (string)keyField?.GetValue(crud)!;

        return new QueryBuilder(crud, db, connectionKey).From(tableName);
    }
}
