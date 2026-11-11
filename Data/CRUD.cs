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
    /// Restituisce il filtro usando parametri al posto del valore concatenato.
    /// Unica eccezione, per compatibilità: operatore "in" con una stringa già formattata
    /// (es. "1,2,3"), che resta concatenata come in passato. Passando una lista di valori
    /// all'operatore "in" viene creato un parametro per ogni elemento.
    /// </summary>
    internal string getFilterParams(Connection oConnection, List<Parameter> oParams)
    {
        // Come in passato, un valore nullo non è ammesso (prima generava NullReferenceException).
        if (Value == null)
            throw new ArgumentNullException(nameof(Value), "CRUDFilter: valore nullo per il filtro '" + Name + "'.");

        object oValue = SqlValue.Normalize(Value)!;

        if (Operator.ToLower() == "in")
        {
            // Stringa già formattata: comportamento storico invariato.
            if (oValue is string)
                return Name + " " + getValue(Operator, oValue);

            List<string> ovNames = new List<string>();
            if (oValue is IEnumerable oValues)
            {
                foreach (object? oItem in oValues)
                    ovNames.Add(AddParameter(oConnection, oParams, oItem));
            }
            else
            {
                ovNames.Add(AddParameter(oConnection, oParams, oValue));
            }
            if (ovNames.Count == 0)
                return "1 = 0";
            return Name + " in (" + string.Join(", ", ovNames) + ")";
        }

        return Name + " " + Operator + " " + AddParameter(oConnection, oParams, oValue);
    }

    private static string AddParameter(Connection oConnection, List<Parameter> oParams, object? oValue)
    {
        string sName = "@find" + oParams.Count;
        Parameter oParam = new Parameter(oConnection);
        oParam.ParameterName = sName;
        oParam.Value = SqlValue.Normalize(oValue) ?? DBNull.Value;
        oParams.Add(oParam);
        return sName;
    }
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
/// Converte i valori provenienti da JSON (System.Text.Json / Newtonsoft) nei tipi .NET
/// corrispondenti, così da poterli passare come parametri di una query.
/// </summary>
internal static class SqlValue
{
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
        string sSQL = "SELECT " + oFind.getFields() + " FROM [" + oFind.Name + "] ";
        string sSQLWhere = oFind.Filters.getFiltersParams(oDB.DataManager.Connections[_sKey], oParams);
        if (!string.IsNullOrEmpty(sSQLWhere))
            sSQL += " WHERE " + sSQLWhere;
        sSQL += " ORDER BY " + oFind.Orders.getOrders();
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
        StringBuilder oSql = new StringBuilder();
        StringBuilder oSqlName = new StringBuilder();
        StringBuilder oSqlValue = new StringBuilder();
        List<Parameter> oParams = new List<Parameter>();
        //
        foreach (KeyValuePair<string, object> oField in oItem.Attributes)
        {
            if (oSqlName.Length > 0)
                oSqlName.Append(", ");
            oSqlName.Append(oField.Key);
            if (oSqlValue.Length > 0)
                oSqlValue.Append(", ");
            oSqlValue.Append("@" + oField.Key);
            //
            Parameter oParam = new Parameter(oDB.DataManager.Connections[_sKey]);
            oParam.ParameterName = "@" + oField.Key;
            oParam.Value = (oField.Value == null ? DBNull.Value : oField.Value);
            oParams.Add(oParam);
        }
        //
        oSql.Append("INSERT INTO [" + oItem.Name + "] ( " + oSqlName + " ) VALUES ( " + oSqlValue + ");SELECT @@ROWCOUNT;");
        //
        DataTable oDT = oDB.InvokeSQL(_sKey, oSql.ToString(), oParams.ToArray());
        if (oDT == null || oDT.Rows.Count == 0)
            return 0;
        return System.Convert.ToInt32(oDT.Rows[0][0].ToString());
    }

    public long? InsertWithReturn(CRUDBase oItem)
    {
        StringBuilder oSql = new StringBuilder();
        StringBuilder oSqlName = new StringBuilder();
        StringBuilder oSqlValue = new StringBuilder();
        List<Parameter> oParams = new List<Parameter>();
        //
        foreach (KeyValuePair<string, object> oField in oItem.Attributes)
        {
            if (oSqlName.Length > 0)
                oSqlName.Append(", ");
            oSqlName.Append(oField.Key);
            if (oSqlValue.Length > 0)
                oSqlValue.Append(", ");
            oSqlValue.Append("@" + oField.Key);
            //
            Parameter oParam = new Parameter(oDB.DataManager.Connections[_sKey]);
            oParam.ParameterName = "@" + oField.Key;
            oParam.Value = (oField.Value == null ? DBNull.Value : oField.Value);
            oParams.Add(oParam);
        }
        //
        oSql.Append("INSERT INTO [" + oItem.Name + "] ( " + oSqlName + " ) VALUES ( " + oSqlValue + "); SELECT SCOPE_IDENTITY();");
        //
        DataTable oDT = oDB.InvokeSQL(_sKey, oSql.ToString(), oParams.ToArray());
        if (oDT == null || oDT.Rows.Count == 0)
            return null;
        return System.Convert.ToInt64(oDT.Rows[0][0].ToString());
    }

    public int Update(CRUDUpdate oItem)
    {
        StringBuilder oSql = new StringBuilder();
        StringBuilder oSqlValue = new StringBuilder();
        StringBuilder oSqlFilter = new StringBuilder();
        List<Parameter> oParams = new List<Parameter>();
        //
        if (oItem.Filters == null || oItem.Filters.Count == 0)
            throw new Exception("UPDATE: condizione di Where non presente.");
        //
        foreach (KeyValuePair<string, object> oField in oItem.Attributes)
        {
            if (oSqlValue.Length > 0)
                oSqlValue.Append(", ");
            oSqlValue.Append(" [" + oField.Key + "] = @" + oField.Key);

            Parameter oParam = new Parameter(oDB.DataManager.Connections[_sKey]);
            oParam.ParameterName = "@" + oField.Key;
            oParam.Value = (oField.Value == null ? DBNull.Value : oField.Value);
            oParams.Add(oParam);
        }

        foreach (CRUDFilter oFilter in oItem.Filters)
        {
            if (oSqlFilter.Length > 0)
                oSqlFilter.Append(" AND ");
            oSqlFilter.Append(oFilter.getFilterParams("filter"));
            //
            Parameter oParam = new Parameter(oDB.DataManager.Connections[_sKey]);
            oParam.ParameterName = "@filter" + oFilter.Name;
            oParam.Value = (oFilter.Value == null ? DBNull.Value : oFilter.Value);
            oParams.Add(oParam);
        }
        //
        oSql.Append("UPDATE [" + oItem.Name + "] SET " + oSqlValue + " WHERE " + oSqlFilter + ";SELECT @@ROWCOUNT;");
        //
        DataTable oDT = oDB.InvokeSQL(_sKey, oSql.ToString(), oParams.ToArray());
        if (oDT == null || oDT.Rows.Count == 0)
            return 0;
        return System.Convert.ToInt32(oDT.Rows[0][0].ToString());
    }

    public int Delete(CRUDDelete oItem)
    {
        StringBuilder oSql = new StringBuilder();
        StringBuilder oSqlFilter = new StringBuilder();
        List<Parameter> oParams = new List<Parameter>();
        //
        if (oItem.Filters == null || oItem.Filters.Count == 0)
            throw new Exception("DELETE: condizione di Where non presente.");

        foreach (CRUDFilter oFilter in oItem.Filters)
        {
            if (oSqlFilter.Length > 0)
                oSqlFilter.Append(" AND ");
            oSqlFilter.Append(oFilter.getFilterParams("filter"));
            //
            Parameter oParam = new Parameter(oDB.DataManager.Connections[_sKey]);
            oParam.ParameterName = "@filter" + oFilter.Name;
            oParam.Value = (oFilter.Value == null ? DBNull.Value : oFilter.Value);
            oParams.Add(oParam);
        }
        //
        oSql.Append("DELETE FROM [" + oItem.Name + "] WHERE " + oSqlFilter + ";SELECT @@ROWCOUNT;");
        //
        DataTable oDT = oDB.InvokeSQL(_sKey, oSql.ToString(), oParams.ToArray());
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

