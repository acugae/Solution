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

    public string getFilters()
    {
        if (this.Count == 0)
            return "";
        List<string> ovFilters = new List<string>();
        foreach (CRUDFilter filter in this)
            ovFilters.Add(filter.getFilter());
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
            ovOrders.Add(SqlSecurityHelper.QuoteIdentifier(order, nameof(order)) + " " + SqlSecurityHelper.ValidateSortDirection(this[order], "direction"));
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
    public string getFields()
    {
        if (Fields == null || Fields.Count == 0)
            return "*";
        return string.Join(",", Fields.Select(f => SqlSecurityHelper.ValidateSelectField(f, nameof(Fields))));
    }
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
    public string getOrder() => SqlSecurityHelper.QuoteIdentifier(Column, nameof(Column)) + " " + SqlSecurityHelper.ValidateSortDirection(Direction, nameof(Direction));
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

    public string getFilter()
    {
        return SqlSecurityHelper.QuoteIdentifier(Name, nameof(Name)) + " " + getValue(Operator, Value);
    }
    public string getFilterParams(string sPrefix)
    {
        string safeCol = SqlSecurityHelper.QuoteIdentifier(Name, nameof(Name));
        string safeOp = SqlSecurityHelper.ValidateOperator(Operator, nameof(Operator));
        return safeCol + " " + safeOp + " @" + sPrefix + Name;
    }

    internal string BuildParameterizedClause(Connection conn, string prefix, ref int paramIndex, List<Parameter> parameters)
    {
        string safeCol = SqlSecurityHelper.QuoteIdentifier(Name, nameof(Name));
        string safeOp = SqlSecurityHelper.ValidateOperator(Operator, nameof(Operator));

        if (safeOp is "IS" or "IS NOT")
        {
            if (Value == null || Value == DBNull.Value || Value.ToString()!.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                return $"{safeCol} {safeOp} NULL";
            throw new ArgumentException($"Operator '{safeOp}' only supports NULL value.", nameof(Value));
        }

        if (safeOp is "IN" or "NOT IN")
        {
            var items = new List<object>();
            if (Value is System.Collections.IEnumerable enumerable && Value is not string)
            {
                foreach (var item in enumerable)
                    items.Add(item ?? DBNull.Value);
            }
            else if (Value is string rawList)
            {
                foreach (var token in rawList.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    string cleaned = token.Length >= 2 && token.StartsWith('\'') && token.EndsWith('\'')
                        ? token[1..^1].Replace("''", "'")
                        : token;
                    items.Add(cleaned);
                }
            }
            else if (Value != null)
            {
                items.Add(Value);
            }

            if (items.Count == 0)
                return safeOp == "IN" ? "1 = 0" : "1 = 1";

            var placeholders = new List<string>(items.Count);
            foreach (var item in items)
            {
                string paramName = SqlSecurityHelper.MakeParamName(prefix, paramIndex++);
                placeholders.Add(paramName);
                var p = new Parameter(conn)
                {
                    ParameterName = paramName,
                    Value = item ?? DBNull.Value
                };
                parameters.Add(p);
            }

            return $"{safeCol} {safeOp} ({string.Join(", ", placeholders)})";
        }

        if (safeOp == "BETWEEN")
        {
            if (Value is object[] range && range.Length == 2)
            {
                string pFrom = SqlSecurityHelper.MakeParamName(prefix, paramIndex++);
                string pTo = SqlSecurityHelper.MakeParamName(prefix, paramIndex++);
                parameters.Add(new Parameter(conn) { ParameterName = pFrom, Value = range[0] ?? DBNull.Value });
                parameters.Add(new Parameter(conn) { ParameterName = pTo, Value = range[1] ?? DBNull.Value });
                return $"{safeCol} BETWEEN {pFrom} AND {pTo}";
            }
            throw new ArgumentException("BETWEEN operator requires an object[2] range.", nameof(Value));
        }

        string pName = SqlSecurityHelper.MakeParamName(prefix, paramIndex++);
        parameters.Add(new Parameter(conn)
        {
            ParameterName = pName,
            Value = Value ?? DBNull.Value
        });
        return $"{safeCol} {safeOp} {pName}";
    }

    public string getValue(string sOperator, object oValue)
    {
        string safeOp = SqlSecurityHelper.ValidateOperator(sOperator, nameof(sOperator));
        if (oValue == null || oValue == DBNull.Value)
            return safeOp + " NULL";

        bool bIsString = oValue is string;
        switch (safeOp)
        {
            case "IN":
            case "NOT IN":
                {
                    var parts = oValue.ToString()!
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(v =>
                        {
                            string cleaned = v.Length >= 2 && v.StartsWith('\'') && v.EndsWith('\'')
                                ? v[1..^1].Replace("''", "'")
                                : v;
                            return decimal.TryParse(cleaned, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _)
                                ? cleaned
                                : "'" + EscapeSqlString(cleaned) + "'";
                        });
                    return safeOp + " (" + string.Join(", ", parts) + ")";
                }
            default:
                // Escape single quotes to prevent SQL injection
                string escapedValue = bIsString ? EscapeSqlString(oValue.ToString()!) : Convert.ToString(oValue, System.Globalization.CultureInfo.InvariantCulture)!;
                return safeOp + " " + (bIsString ? "'" + escapedValue + "'" : escapedValue);
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

public class CRUD
{
    DB oDB = null;
    string _sKey;
    public CRUD(DB _oDB, string sKey) { oDB = _oDB; _sKey = sKey; }

    public DataTable Find(CRUDFind oFind)
    {
        string safeTable = SqlSecurityHelper.QuoteIdentifier(oFind.Name, nameof(oFind.Name));
        string sSQL = "SELECT " + oFind.getFields() + " FROM " + safeTable + " ";
        List<Parameter> oParams = new List<Parameter>();

        if (oFind.Filters != null && oFind.Filters.Count > 0)
        {
            Connection conn = oDB.DataManager.Connections[_sKey];
            int paramIndex = 0;
            var clauses = new List<string>(oFind.Filters.Count);
            foreach (CRUDFilter filter in oFind.Filters)
                clauses.Add(filter.BuildParameterizedClause(conn, "f", ref paramIndex, oParams));
            sSQL += " WHERE " + string.Join(" AND ", clauses);
        }

        if (oFind.Orders != null && oFind.Orders.Count > 0)
            sSQL += " ORDER BY " + oFind.Orders.getOrders();
        else if (!string.IsNullOrEmpty(oFind.Pagination.getPagination()))
            sSQL += " ORDER BY 1";

        string pagination = oFind.Pagination.getPagination();
        if (!string.IsNullOrEmpty(pagination))
            sSQL += " " + pagination;

        return oParams.Count > 0 ? oDB.InvokeSQL(_sKey, sSQL, oParams.ToArray()) : oDB.InvokeSQL(_sKey, sSQL);
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
        Connection conn = oDB.DataManager.Connections[_sKey];
        string safeTable = SqlSecurityHelper.QuoteIdentifier(oItem.Name, nameof(oItem.Name));
        int idx = 0;
        //
        foreach (KeyValuePair<string, object> oField in oItem.Attributes)
        {
            string safeCol = SqlSecurityHelper.QuoteIdentifier(oField.Key, nameof(oItem.Attributes));
            string pName = SqlSecurityHelper.MakeParamName("p", idx++);
            if (oSqlName.Length > 0)
                oSqlName.Append(", ");
            oSqlName.Append(safeCol);
            if (oSqlValue.Length > 0)
                oSqlValue.Append(", ");
            oSqlValue.Append(pName);
            //
            Parameter oParam = new Parameter(conn);
            oParam.ParameterName = pName;
            oParam.Value = (oField.Value == null ? DBNull.Value : oField.Value);
            oParams.Add(oParam);
        }
        //
        oSql.Append("INSERT INTO " + safeTable + " ( " + oSqlName + " ) VALUES ( " + oSqlValue + ");SELECT @@ROWCOUNT;");
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
        Connection conn = oDB.DataManager.Connections[_sKey];
        string safeTable = SqlSecurityHelper.QuoteIdentifier(oItem.Name, nameof(oItem.Name));
        int idx = 0;
        //
        foreach (KeyValuePair<string, object> oField in oItem.Attributes)
        {
            string safeCol = SqlSecurityHelper.QuoteIdentifier(oField.Key, nameof(oItem.Attributes));
            string pName = SqlSecurityHelper.MakeParamName("p", idx++);
            if (oSqlName.Length > 0)
                oSqlName.Append(", ");
            oSqlName.Append(safeCol);
            if (oSqlValue.Length > 0)
                oSqlValue.Append(", ");
            oSqlValue.Append(pName);
            //
            Parameter oParam = new Parameter(conn);
            oParam.ParameterName = pName;
            oParam.Value = (oField.Value == null ? DBNull.Value : oField.Value);
            oParams.Add(oParam);
        }
        //
        oSql.Append("INSERT INTO " + safeTable + " ( " + oSqlName + " ) VALUES ( " + oSqlValue + "); SELECT SCOPE_IDENTITY();");
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
        Connection conn = oDB.DataManager.Connections[_sKey];
        string safeTable = SqlSecurityHelper.QuoteIdentifier(oItem.Name, nameof(oItem.Name));
        int idx = 0;
        foreach (KeyValuePair<string, object> oField in oItem.Attributes)
        {
            string safeCol = SqlSecurityHelper.QuoteIdentifier(oField.Key, nameof(oItem.Attributes));
            string pName = SqlSecurityHelper.MakeParamName("p", idx++);
            if (oSqlValue.Length > 0)
                oSqlValue.Append(", ");
            oSqlValue.Append(" " + safeCol + " = " + pName);

            Parameter oParam = new Parameter(conn);
            oParam.ParameterName = pName;
            oParam.Value = (oField.Value == null ? DBNull.Value : oField.Value);
            oParams.Add(oParam);
        }

        int filterIdx = 0;
        foreach (CRUDFilter oFilter in oItem.Filters)
        {
            if (oSqlFilter.Length > 0)
                oSqlFilter.Append(" AND ");
            oSqlFilter.Append(oFilter.BuildParameterizedClause(conn, "f", ref filterIdx, oParams));
        }
        //
        oSql.Append("UPDATE " + safeTable + " SET " + oSqlValue + " WHERE " + oSqlFilter + ";SELECT @@ROWCOUNT;");
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

        Connection conn = oDB.DataManager.Connections[_sKey];
        string safeTable = SqlSecurityHelper.QuoteIdentifier(oItem.Name, nameof(oItem.Name));
        int filterIdx = 0;
        foreach (CRUDFilter oFilter in oItem.Filters)
        {
            if (oSqlFilter.Length > 0)
                oSqlFilter.Append(" AND ");
            oSqlFilter.Append(oFilter.BuildParameterizedClause(conn, "f", ref filterIdx, oParams));
        }
        //
        oSql.Append("DELETE FROM " + safeTable + " WHERE " + oSqlFilter + ";SELECT @@ROWCOUNT;");
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

