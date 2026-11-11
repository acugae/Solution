namespace Solution.Data;
public class CRUDBase
{
    public CRUDBase(string sName) => Name = sName;
    public CRUDBase(string sName, Dictionary<string, object> Attributes)
    {
        Name = sName;
        oAttributes = Attributes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
    public string Name { get; set; }
    Dictionary<string, object> oAttributes = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
    public Dictionary<string, object> Attributes { get { return oAttributes; } set { oAttributes = value; } }
    public object this[string sAttributeName]
    {
        get { lock (this) { return oAttributes[sAttributeName]; } }
        set
        {
            lock (this)
            {
                if (oAttributes.ContainsKey(sAttributeName))
                    oAttributes[sAttributeName] = value;
                else
                    oAttributes.Add(sAttributeName, value);
            }
        }
    }
}

public class CRUDUpdate : CRUDBase
{
    public CRUDUpdate(string sName) : base(sName) { }
    public CRUDUpdate(string sName, Dictionary<string, object> Attributes) : base(sName, Attributes) { }
    public CRUDUpdate(string sName, DataRow dataRow) : base(sName, dataRow.ToDictionary()) { }

    private CRUDFilters oFilters = new CRUDFilters();
    public CRUDFilters Filters { get { return oFilters; } set { oFilters = value; } }
}

public class CRUDDelete
{
    public CRUDDelete(string sName)
    {
        Name = sName;
    }
    public string Name { get; set; }

    private CRUDFilters oFilters = new CRUDFilters();
    public CRUDFilters Filters { get { return oFilters; } set { oFilters = value; } }
}

public class CRUDProcedure
{
    public CRUDProcedure(string sName)
    {
        Name = sName;
    }
    public string Name { get; set; }

    private CRUDParameters oParameters = new CRUDParameters();
    public CRUDParameters Parameters { get { return oParameters; } set { oParameters = value; } }
}

public class CRUDParameters : List<CRUDParameter>
{
    public void AddParameter(string sName, object oValue)
    {
        this.Add(new CRUDParameter(sName, oValue));
    }

    //public string getParameters()
    //{
    //    if (this.Count == 0)
    //        return "";
    //    List<string> ovFilters = new List<string>();
    //    foreach (CRUDFilter filter in this)
    //        ovFilters.Add(filter.getFilter());
    //    return string.Join(" AND ", ovFilters);
    //}
}

public class CRUDParameter
{
    public CRUDParameter(string sName, object oValue)
    {
        Name = sName;
        Value = oValue;
        Type = "String";
        Direction = "Input";
    }
    public string Name { get; set; }
    public object Value { get; set; }
    public string Type { get; set; }
    public string Direction { get; set; }
}

public class CRUDFilters : List<CRUDFilter>
{
    public void AddFilter(string sName, string sOperator, object oValue)
    {
        this.Add(new CRUDFilter(sName, sOperator, oValue));
    }

    [Obsolete("Concatena i valori nella stringa SQL (rischio SQL injection). CRUD.Find usa ora query parametrizzate.")]
    public string getFilters()
    {
        if (this.Count == 0)
            return "";
        List<string> ovFilters = new List<string>();
        foreach (CRUDFilter filter in this)
            ovFilters.Add(filter.getFilter());
        return string.Join(" AND ", ovFilters);
    }

    /// <summary>
    /// Costruisce la condizione WHERE usando parametri al posto dei valori concatenati.
    /// I parametri creati vengono aggiunti a <paramref name="oParams"/>.
    /// </summary>
    internal string getFiltersParams(Connection oConnection, List<Parameter> oParams)
    {
        if (this.Count == 0)
            return "";
        List<string> ovFilters = new List<string>();
        foreach (CRUDFilter filter in this)
            ovFilters.Add(filter.getFilterParams(oConnection, oParams));
        return string.Join(" AND ", ovFilters);
    }
}

public class CRUDOrders : Dictionary<string, string>
{
    public void AddOrder(string sName, string sDirection = "asc")
    {
        this.Add(sName, sDirection);
    }

    public string getOrders()
    {
        if (this.Count == 0)
            return "1";
        List<string> ovOrders = new List<string>();
        foreach (string order in this.Keys)
            ovOrders.Add(order + " " + this[order]);
        return string.Join(",", ovOrders);
    }
}

public class CRUDFind
{
    public CRUDFind(string sName) { Name = sName; }
    public string Name { get; set; }
    public List<string> Fields { get; set; }
    private CRUDPagination oPagination = new CRUDPagination();
    public CRUDPagination Pagination
    {
        get
        {
            oPagination ??= new CRUDPagination();
            return oPagination;
        }
        set { oPagination = value; }
    }
    private CRUDOrders oOrders = new CRUDOrders();
    public CRUDOrders Orders
    {
        get
        {
            oOrders ??= new CRUDOrders();
            return oOrders;
        }
        set { oOrders = value; }
    }
    private CRUDFilters oFilters = new CRUDFilters();
    public CRUDFilters Filters
    {
        get
        {
            oFilters ??= new CRUDFilters();
            return oFilters;
        }
        set { oFilters = value; }
    }
    public string getFields() { return ((Fields == null || Fields.Count == 0) ? "*" : string.Join(",", Fields)); }
}

public class CRUDPagination
{
    public int? size { get; set; }
    public int? page { get; set; }
    public int? offset { get; set; }

    public string getPagination()
    {
        if (offset != null)
            return "OFFSET " + offset.ToString() + " ROWS FETCH NEXT " + size.ToString() + " ROWS ONLY";
        else if (page != null)
            return "OFFSET " + (size * (page - 1)).ToString() + " ROWS FETCH NEXT " + size.ToString() + " ROWS ONLY";
        else
            return "";
    }
}

public class CRUDOrder
{
    public string Column { get; set; }
    public string Direction { get; set; }
    public string getOrder() { return Column + " " + Direction; }
}

public class CRUDFilter
{
    public CRUDFilter(string sName, string sOperator, object oValue)
    {
        Name = sName;
        Operator = sOperator;
        Value = oValue;
    }
    public string Name { get; set; }
    public string Operator { get; set; }
    public object Value { get; set; }

    [Obsolete("Concatena il valore nella stringa SQL (rischio SQL injection). CRUD.Find usa ora query parametrizzate.")]
    public string getFilter()
    {
        return Name + " " + getValue(Operator, Value);
    }

    /// <summary>
    /// Restituisce il filtro con nome di colonna e operatore validati e i valori passati come parametri.
    /// Operatori ammessi: = &lt;&gt; != &lt; &gt; &lt;= &gt;= LIKE, NOT LIKE, IN, NOT IN, IS, IS NOT, BETWEEN.
    /// IN / NOT IN accettano una lista oppure un testo come "1,2,3" o "'a','b'";
    /// IS / IS NOT si usano solo per il confronto con NULL; BETWEEN richiede due valori.
    /// </summary>
    internal string getFilterParams(Connection oConnection, List<Parameter> oParams)
    {
        string sColumn = SqlIdentifier.Quote(Name);
        string sOperator = SqlIdentifier.Operator(Operator);
        object? oValue = SqlValue.Normalize(Value);

        if (sOperator == "IS" || sOperator == "IS NOT")
        {
            bool bIsNull = oValue == null || oValue is DBNull || (oValue is string sNull && sNull.Trim().Equals("null", StringComparison.OrdinalIgnoreCase));
            if (!bIsNull)
                throw new ArgumentException("CRUDFilter: l'operatore " + sOperator + " è ammesso solo con valore null (filtro '" + Name + "').");
            return sColumn + " " + sOperator + " NULL";
        }

        // Un valore nullo con gli altri operatori non troverebbe mai righe: meglio un errore esplicito.
        if (oValue == null)
            throw new ArgumentNullException(nameof(Value), "CRUDFilter: valore nullo per il filtro '" + Name + "'. Per cercare i NULL usare l'operatore IS.");

        if (sOperator == "IN" || sOperator == "NOT IN" || sOperator == "BETWEEN")
        {
            IEnumerable oValues;
            if (oValue is string sList)
                oValues = SqlValue.SplitList(sList);
            else if (oValue is IEnumerable oEnumerable)
                oValues = oEnumerable;
            else
                oValues = new object[] { oValue };

            List<string> ovNames = new List<string>();
            foreach (object? oItem in oValues)
                ovNames.Add(AddParameter(oConnection, oParams, oItem));

            if (sOperator == "BETWEEN")
            {
                if (ovNames.Count != 2)
                    throw new ArgumentException("CRUDFilter: BETWEEN richiede due valori (filtro '" + Name + "').");
                return sColumn + " BETWEEN " + ovNames[0] + " AND " + ovNames[1];
            }
            if (ovNames.Count == 0)
                return sOperator == "IN" ? "1 = 0" : "1 = 1";
            return sColumn + " " + sOperator + " (" + string.Join(", ", ovNames) + ")";
        }

        return sColumn + " " + sOperator + " " + AddParameter(oConnection, oParams, oValue);
    }

    private static string AddParameter(Connection oConnection, List<Parameter> oParams, object? oValue)
    {
        string sName = "@__p" + oParams.Count;
        Parameter oParam = new Parameter(oConnection);
        oParam.ParameterName = sName;
        oParam.Value = SqlValue.Normalize(oValue) ?? DBNull.Value;
        oParams.Add(oParam);
        return sName;
    }

    [Obsolete("Non valida nome di colonna e operatore. Update e Delete costruiscono ora i filtri internamente.")]
    public string getFilterParams(string sPrefix)
    {
        return "[" + Name + "] " + Operator + " @" + sPrefix + Name;
    }

    public string getValue(string sOperator, object oValue)
    {
        bool bIsString = oValue.GetType().ToString().Equals("System.String");
        switch (sOperator.ToLower())
        {
            case "in":
                {
                    return "in (" + oValue.ToString() + ")";
                }
            default:
                // Escape single quotes to prevent SQL injection
                string escapedValue = bIsString ? EscapeSqlString(oValue.ToString()) : oValue.ToString();
                return sOperator + " " + (bIsString ? "'" + escapedValue + "'" : escapedValue);
        }
    }
    
    /// <summary>
    /// Escape caratteri speciali per prevenire SQL injection.
    /// NOTA: Preferire sempre l'uso di query parametrizzate tramite getFilterParams().
    /// </summary>
    private static string EscapeSqlString(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;
        
        return value
            .Replace("'", "''")      // Escape single quotes
            .Replace("\\", "\\\\")   // Escape backslashes
            .Replace("\0", "")       // Remove null bytes
            .Replace("\n", "\\n")    // Escape newlines
            .Replace("\r", "\\r");   // Escape carriage returns
    }
}

/// <summary>
/// Validazione di nomi (tabelle, colonne), operatori e direzioni di ordinamento
/// prima di inserirli in una stringa SQL. Tutto ciò che non è riconosciuto viene rifiutato.
/// </summary>
internal static class SqlIdentifier
{
    private static readonly System.Text.RegularExpressions.Regex _oPart =
        new System.Text.RegularExpressions.Regex(@"^[^\[\]'""`;\p{Cc}]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

    private static readonly System.Text.RegularExpressions.Regex _oSafe =
        new System.Text.RegularExpressions.Regex(@"^[\p{L}\p{N}_@#$\[\]\. ]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>
    /// Per i punti in cui il nome viene scritto nella stringa SQL senza parentesi quadre (DDL):
    /// restituisce il nome invariato se contiene solo lettere, cifre, spazio e _ @ # $ . [ ], altrimenti eccezione.
    /// </summary>
    public static string Safe(string? sName)
    {
        if (string.IsNullOrWhiteSpace(sName) || !_oSafe.IsMatch(sName))
            throw new ArgumentException("Nome SQL non valido: '" + sName + "'.");
        return sName;
    }

    private static readonly HashSet<string> _oOperators = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "=", "<>", "!=", "<", ">", "<=", ">=", "LIKE", "NOT LIKE", "IN", "NOT IN", "IS", "IS NOT", "BETWEEN"
    };

    /// <summary>Restituisce il nome tra parentesi quadre ([schema].[tabella]); eccezione se non è un nome valido.</summary>
    public static string Quote(string? sName)
    {
        if (string.IsNullOrWhiteSpace(sName))
            throw new ArgumentException("Nome SQL non valido: vuoto.");
        string[] ovParts = sName.Trim().Split('.');
        for (int i = 0; i < ovParts.Length; i++)
        {
            string sPart = ovParts[i].Trim();
            if (sPart.Length >= 2 && sPart[0] == '[' && sPart[sPart.Length - 1] == ']')
                sPart = sPart.Substring(1, sPart.Length - 2);
            if (!_oPart.IsMatch(sPart))
                throw new ArgumentException("Nome SQL non valido: '" + sName + "'. Non sono ammessi [ ] ' \" ` ; e caratteri di controllo.");
            ovParts[i] = "[" + sPart + "]";
        }
        return string.Join(".", ovParts);
    }

    /// <summary>Come Quote, ma accetta anche *, 1 e COUNT(*) nell'elenco dei campi.</summary>
    public static string QuoteField(string? sName)
    {
        string sTrim = (sName ?? "").Trim();
        if (sTrim == "*" || sTrim == "1")
            return sTrim;
        if (sTrim.Replace(" ", "").Equals("COUNT(*)", StringComparison.OrdinalIgnoreCase))
            return "COUNT(*)";
        return Quote(sName);
    }

    public static string QuoteFields(IEnumerable<string>? oFields)
    {
        if (oFields == null)
            return "*";
        List<string> ovFields = new List<string>();
        foreach (string sField in oFields)
            ovFields.Add(QuoteField(sField));
        return ovFields.Count == 0 ? "*" : string.Join(",", ovFields);
    }

    public static string QuoteOrders(IEnumerable<KeyValuePair<string, string>>? oOrders)
    {
        List<string> ovOrders = new List<string>();
        if (oOrders != null)
        {
            foreach (KeyValuePair<string, string> oOrder in oOrders)
                ovOrders.Add(Quote(oOrder.Key) + " " + Direction(oOrder.Value));
        }
        return ovOrders.Count == 0 ? "1" : string.Join(",", ovOrders);
    }

    /// <summary>Restituisce l'operatore in maiuscolo; eccezione se non è tra quelli ammessi.</summary>
    public static string Operator(string? sOperator)
    {
        string sNormalized = string.Join(" ", (sOperator ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();
        if (!_oOperators.Contains(sNormalized))
            throw new ArgumentException("Operatore SQL non ammesso: '" + sOperator + "'.");
        return sNormalized;
    }

    public static string Direction(string? sDirection)
    {
        string sNormalized = (sDirection ?? "").Trim().ToUpperInvariant();
        if (sNormalized == "" || sNormalized == "ASC")
            return "ASC";
        if (sNormalized == "DESC")
            return "DESC";
        throw new ArgumentException("Direzione di ordinamento non ammessa: '" + sDirection + "'.");
    }
}

/// <summary>
/// Converte i valori provenienti da JSON (System.Text.Json / Newtonsoft) nei tipi .NET
/// corrispondenti, così da poterli passare come parametri di una query.
/// </summary>
internal static class SqlValue
{
    /// <summary>
    /// Scompone un elenco scritto come testo ("1,2,3" oppure "'a','b'") nei singoli valori,
    /// così da poterli passare come parametri all'operatore IN.
    /// </summary>
    public static List<object> SplitList(string sList)
    {
        List<object> oResult = new List<object>();
        StringBuilder oCurrent = new StringBuilder();
        bool bInQuotes = false;
        bool bQuoted = false;
        for (int i = 0; i <= sList.Length; i++)
        {
            bool bEnd = i == sList.Length;
            char c = bEnd ? ',' : sList[i];
            if (bInQuotes && !bEnd)
            {
                if (c != '\'')
                    oCurrent.Append(c);
                else if (i + 1 < sList.Length && sList[i + 1] == '\'')
                {
                    oCurrent.Append('\'');
                    i++;
                }
                else
                    bInQuotes = false;
            }
            else if (c == '\'' && !bEnd)
            {
                // inizio di un valore tra apici: si scarta quanto letto prima (spazi, prefisso N)
                oCurrent.Clear();
                bInQuotes = true;
                bQuoted = true;
            }
            else if (c == ',')
            {
                string sItem = bQuoted ? oCurrent.ToString() : oCurrent.ToString().Trim();
                if (bQuoted || sItem.Length > 0)
                    oResult.Add(sItem);
                oCurrent.Clear();
                bQuoted = false;
            }
            else if (!bQuoted)
                oCurrent.Append(c);
        }
        return oResult;
    }
    public static object? Normalize(object? oValue)
    {
        if (oValue is System.Text.Json.JsonElement oElement)
        {
            switch (oElement.ValueKind)
            {
                case System.Text.Json.JsonValueKind.Null:
                case System.Text.Json.JsonValueKind.Undefined:
                    return null;
                case System.Text.Json.JsonValueKind.String:
                    return oElement.GetString();
                case System.Text.Json.JsonValueKind.True:
                    return true;
                case System.Text.Json.JsonValueKind.False:
                    return false;
                case System.Text.Json.JsonValueKind.Number:
                    if (oElement.TryGetInt64(out long lValue))
                        return lValue;
                    if (oElement.TryGetDecimal(out decimal dValue))
                        return dValue;
                    return oElement.GetDouble();
                case System.Text.Json.JsonValueKind.Array:
                    List<object?> oItems = new List<object?>();
                    foreach (System.Text.Json.JsonElement oItem in oElement.EnumerateArray())
                        oItems.Add(Normalize(oItem));
                    return oItems;
                default:
                    return oElement.GetRawText();
            }
        }
        if (oValue is JValue oJValue)
            return oJValue.Value;
        return oValue;
    }
}

public class CRUD
{
    DB oDB = null;
    string _sKey;
    public CRUD(DB _oDB, string sKey) { oDB = _oDB; _sKey = sKey; }

    public DataTable Find(CRUDFind oFind)
    {
        List<Parameter> oParams = new List<Parameter>();
        string sSQL = "SELECT " + SqlIdentifier.QuoteFields(oFind.Fields) + " FROM " + SqlIdentifier.Quote(oFind.Name) + " ";
        string sSQLWhere = oFind.Filters.getFiltersParams(oDB.DataManager.Connections[_sKey], oParams);
        if (!string.IsNullOrEmpty(sSQLWhere))
            sSQL += " WHERE " + sSQLWhere;
        sSQL += " ORDER BY " + SqlIdentifier.QuoteOrders(oFind.Orders);
        sSQL += " " + oFind.Pagination.getPagination();
        return oDB.InvokeSQL(_sKey, sSQL, oParams.ToArray());
    }

    public DataRow Find0(string sEntityName, params object[] ovKeyValue)
    {
        DataTable dataTable = Find(sEntityName, ovKeyValue);
        if (dataTable == null || dataTable.Rows.Count == 0)
            return null;
        return dataTable.Rows[0];
    }

    public DataTable Find(string sEntityName, params object[] ovKeyValue)
    {
        CRUDFind cRUDFind = new CRUDFind(sEntityName);
        if (ovKeyValue == null || ovKeyValue.Length % 2 == 1)
            return null;
        for (int i = 0; i < ovKeyValue.Length; i += 2)
            cRUDFind.Filters.AddFilter(ovKeyValue[i].ToString(), "=", ovKeyValue[i + 1].ToString());
        DataTable dataTable = oDB[_sKey].Find(cRUDFind);
        dataTable.TableName = sEntityName;
        return dataTable;
    }

    public int Insert(CRUDBase oItem)
    {
        List<Parameter> oParams = new List<Parameter>();
        string sSQL = BuildInsert(oItem, oParams) + ";SELECT @@ROWCOUNT;";
        //
        DataTable oDT = oDB.InvokeSQL(_sKey, sSQL, oParams.ToArray());
        if (oDT == null || oDT.Rows.Count == 0)
            return 0;
        return System.Convert.ToInt32(oDT.Rows[0][0].ToString());
    }

    public long? InsertWithReturn(CRUDBase oItem)
    {
        List<Parameter> oParams = new List<Parameter>();
        string sSQL = BuildInsert(oItem, oParams) + "; SELECT SCOPE_IDENTITY();";
        //
        DataTable oDT = oDB.InvokeSQL(_sKey, sSQL, oParams.ToArray());
        if (oDT == null || oDT.Rows.Count == 0)
            return null;
        return System.Convert.ToInt64(oDT.Rows[0][0].ToString());
    }

    // Nomi di tabella e colonna validati e racchiusi tra parentesi quadre, valori sempre come parametri.
    private string BuildInsert(CRUDBase oItem, List<Parameter> oParams)
    {
        List<string> ovNames = new List<string>();
        List<string> ovValues = new List<string>();
        foreach (KeyValuePair<string, object> oField in oItem.Attributes)
        {
            ovNames.Add(SqlIdentifier.Quote(oField.Key));
            ovValues.Add(AddValue(oParams, oField.Value));
        }
        return "INSERT INTO " + SqlIdentifier.Quote(oItem.Name) + " ( " + string.Join(", ", ovNames) + " ) VALUES ( " + string.Join(", ", ovValues) + ")";
    }

    private string AddValue(List<Parameter> oParams, object? oValue)
    {
        string sName = "@__v" + oParams.Count;
        Parameter oParam = new Parameter(oDB.DataManager.Connections[_sKey]);
        oParam.ParameterName = sName;
        oParam.Value = SqlValue.Normalize(oValue) ?? DBNull.Value;
        oParams.Add(oParam);
        return sName;
    }

    public int Update(CRUDUpdate oItem)
    {
        List<Parameter> oParams = new List<Parameter>();
        //
        if (oItem.Filters == null || oItem.Filters.Count == 0)
            throw new Exception("UPDATE: condizione di Where non presente.");
        //
        List<string> ovSet = new List<string>();
        foreach (KeyValuePair<string, object> oField in oItem.Attributes)
            ovSet.Add(SqlIdentifier.Quote(oField.Key) + " = " + AddValue(oParams, oField.Value));
        string sFilter = oItem.Filters.getFiltersParams(oDB.DataManager.Connections[_sKey], oParams);
        //
        string sSQL = "UPDATE " + SqlIdentifier.Quote(oItem.Name) + " SET " + string.Join(", ", ovSet) + " WHERE " + sFilter + ";SELECT @@ROWCOUNT;";
        //
        DataTable oDT = oDB.InvokeSQL(_sKey, sSQL, oParams.ToArray());
        if (oDT == null || oDT.Rows.Count == 0)
            return 0;
        return System.Convert.ToInt32(oDT.Rows[0][0].ToString());
    }

    public int Delete(CRUDDelete oItem)
    {
        List<Parameter> oParams = new List<Parameter>();
        //
        if (oItem.Filters == null || oItem.Filters.Count == 0)
            throw new Exception("DELETE: condizione di Where non presente.");
        //
        string sFilter = oItem.Filters.getFiltersParams(oDB.DataManager.Connections[_sKey], oParams);
        string sSQL = "DELETE FROM " + SqlIdentifier.Quote(oItem.Name) + " WHERE " + sFilter + ";SELECT @@ROWCOUNT;";
        //
        DataTable oDT = oDB.InvokeSQL(_sKey, sSQL, oParams.ToArray());
        if (oDT == null || oDT.Rows.Count == 0)
            return 0;
        return System.Convert.ToInt32(oDT.Rows[0][0].ToString());
    }

    public DataTable Invoke(CRUDProcedure oItem)
    {
        List<Parameter> oParams = new List<Parameter>();
        //
        if (string.IsNullOrEmpty(oItem.Name))
            throw new Exception("INVOKE: Nome della procedura non presente.");
        //
        foreach (CRUDParameter oParameter in oItem.Parameters)
        {
            Parameter oParam = new Parameter(oDB.DataManager.Connections[_sKey]);
            oParam.ParameterName = oParameter.Name;
            oParam.Value = (oParameter.Value == null ? DBNull.Value : oParameter.Value);
            oParam.DbType = (DbType)Enum.Parse(typeof(DbType), oParameter.Type, true);
            oParam.Direction = (ParameterDirection)Enum.Parse(typeof(ParameterDirection), oParameter.Direction, true);
            oParams.Add(oParam);
        }
        //
        return oDB.Invoke(_sKey, oItem.Name, oParams.ToArray());
    }

    public int Set(CRUDBase oItem, string[] svAttrubutesKey, out string sOperation)
    {
        CRUDFind oF = new CRUDFind(oItem.Name);
        CRUDFilters oFs = new CRUDFilters();
        //
        for (int i = 0; svAttrubutesKey != null && i < svAttrubutesKey.Length; i++)
            oFs.Add(new CRUDFilter(svAttrubutesKey[i], "=", oItem[svAttrubutesKey[i]]));
        //
        DataTable oDT = null;
        if (oFs.Count > 0)
        {
            oF.Filters = oFs;
            oDT = this.Find(oF);
        }
        if (oDT != null && oDT.Rows.Count > 0)
        {
            CRUDUpdate oU = new CRUDUpdate(oItem.Name);
            oU.Filters = oFs;
            oU.Attributes = oItem.Attributes;
            sOperation = "UPDATE";
            return this.Update(oU);
        }
        else
        {
            sOperation = "INSERT";
            return this.Insert(oItem);
        }
    }

    public int Merge(CRUDBase[] oItems, string sDestinationTableName, string[] svAttributesKey)
    {
        int iResult = 0;
        string sOperation = "";
        for (int i = 0; oItems != null && i < oItems.Length; i++)
        {
            oItems[i].Name = sDestinationTableName;
            iResult += this.Set(oItems[i], svAttributesKey, out sOperation);
        }
        return iResult;
    }
}

