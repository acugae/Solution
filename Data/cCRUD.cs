namespace Solution.Data;
public class CRUDBase
{
    public CRUDBase(string sName)
    {
        Name = sName;
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

    public string getFilter()
    {
        return Name + " " + getValue(Operator, Value);
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
                return sOperator + " " + (bIsString ? "'" + oValue.ToString() + "'" : oValue.ToString());
        }
    }
}

public class cCRUD
{
    cDB oDB = null;
    string _sKey;
    public cCRUD(cDB _oDB, string sKey) { oDB = _oDB; _sKey = sKey; }

    public DataTable Find(CRUDFind oFind)
    {
        string sSQL = "SELECT " + oFind.getFields() + " FROM [" + oFind.Name + "] ";
        string sSQLWhere = oFind.Filters.getFilters();
        if (!string.IsNullOrEmpty(sSQLWhere))
            sSQL += " WHERE " + sSQLWhere;
        sSQL += " ORDER BY " + oFind.Orders.getOrders();
        sSQL += " " + oFind.Pagination.getPagination();
        return oDB.InvokeSQL(_sKey, sSQL);
    }

    public int Insert(CRUDBase oItem)
    {
        StringBuilder oSql = new StringBuilder();
        StringBuilder oSqlName = new StringBuilder();
        StringBuilder oSqlValue = new StringBuilder();
        List<cParameter> oParams = new List<cParameter>();
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
            cParameter oParam = new cParameter(oDB.DataManager.Connections[_sKey]);
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
        return Convert.ToInt32(oDT.Rows[0][0].ToString());
    }

    public long? InsertWithReturn(CRUDBase oItem)
    {
        StringBuilder oSql = new StringBuilder();
        StringBuilder oSqlName = new StringBuilder();
        StringBuilder oSqlValue = new StringBuilder();
        List<cParameter> oParams = new List<cParameter>();
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
            cParameter oParam = new cParameter(oDB.DataManager.Connections[_sKey]);
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
        return Convert.ToInt64(oDT.Rows[0][0].ToString());
    }

    public int Update(CRUDUpdate oItem)
    {
        StringBuilder oSql = new StringBuilder();
        StringBuilder oSqlValue = new StringBuilder();
        StringBuilder oSqlFilter = new StringBuilder();
        List<cParameter> oParams = new List<cParameter>();
        //
        if (oItem.Filters == null || oItem.Filters.Count == 0)
            throw new Exception("UPDATE: condizione di Where non presente.");
        //
        foreach (KeyValuePair<string, object> oField in oItem.Attributes)
        {
            if (oSqlValue.Length > 0)
                oSqlValue.Append(", ");
            oSqlValue.Append(" [" + oField.Key + "] = @" + oField.Key);

            cParameter oParam = new cParameter(oDB.DataManager.Connections[_sKey]);
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
            cParameter oParam = new cParameter(oDB.DataManager.Connections[_sKey]);
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
        return Convert.ToInt32(oDT.Rows[0][0].ToString());
    }

    public int Delete(CRUDDelete oItem)
    {
        StringBuilder oSql = new StringBuilder();
        StringBuilder oSqlFilter = new StringBuilder();
        List<cParameter> oParams = new List<cParameter>();
        //
        if (oItem.Filters == null || oItem.Filters.Count == 0)
            throw new Exception("DELETE: condizione di Where non presente.");

        foreach (CRUDFilter oFilter in oItem.Filters)
        {
            if (oSqlFilter.Length > 0)
                oSqlFilter.Append(" AND ");
            oSqlFilter.Append(oFilter.getFilterParams("filter"));
            //
            cParameter oParam = new cParameter(oDB.DataManager.Connections[_sKey]);
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
        return Convert.ToInt32(oDT.Rows[0][0].ToString());
    }

    public DataTable Invoke(CRUDProcedure oItem)
    {
        List<cParameter> oParams = new List<cParameter>();
        //
        if (string.IsNullOrEmpty(oItem.Name))
            throw new Exception("INVOKE: Nome della procedura non presente.");
        //
        foreach (CRUDParameter oParameter in oItem.Parameters)
        {
            cParameter oParam = new cParameter(oDB.DataManager.Connections[_sKey]);
            oParam.ParameterName = oParameter.Name;
            oParam.Value = (oParameter.Value == null ? DBNull.Value : oParameter.Value);
            oParam.DbType = (DbType)Enum.Parse(typeof(DbType), oParameter.Type, true);
            oParam.Direction = (ParameterDirection)Enum.Parse(typeof(ParameterDirection), oParameter.Direction, true);
            oParams.Add(oParam);
        }
        //
        return oDB.Invoke(_sKey, oItem.Name, oParams.ToArray());
    }
}

