namespace Solution.Persistence;
/// <summary>
/// Questa classe gestisce l'accesso ad una tabella.
/// </summary>
public class cTable
{
    private cDataManager _oData = null;
    private string _strNameTable = "";
    private cCollection _oFields = new cCollection();
    private cCollection _oFieldsKey = new cCollection();
    private cCollection _oTypes = new cCollection();
    private bool _bActiveNullValue = true;
    //
    // Questi membri vengono inseriti per velocizzare la computazione.
    private string soFileds = "";
    cReflectionManager oReflection = new cReflectionManager();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="oTable"></param>
    public cTable(cTable oTable)
    {
        _oData = oTable.DataManager;
        _strNameTable = oTable.TableName;
        _oFields = new cCollection(oTable.Fields);
        _oFieldsKey = new cCollection(oTable.FieldsKey);
        _oTypes = new cCollection(oTable.Types);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oDataManager"></param>
    public cTable(cDataManager oDataManager)
    {
        _oData = oDataManager;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oDataManager"></param>
    /// <param name="strNameTable"></param>
    /// <param name="oCCField"></param>
    /// <param name="oCCFieldKey"></param>
    public cTable(cDataManager oDataManager, string strNameTable, cGCollection<string, cDBField> oCCField, cGCollection<string, cDBField> oCCFieldKey)
    {
        _oData = oDataManager;
        _strNameTable = strNameTable;
        //
        ArrayList oAAField = new ArrayList();
        for (int i = 0; i < oCCField.Count; i++)
        {
            oAAField.Add(GetXMLToSQLType(((cDBField)oCCField.GetValue(i)).Type));
        }
        _oFields.AddRange(oCCField.Keys, oAAField.ToArray());
        soFileds = _concat(_oFields, ", ");
        //
        ArrayList oAAFieldKey = new ArrayList();
        for (int i = 0; i < oCCFieldKey.Count; i++)
        {
            oAAFieldKey.Add(GetXMLToSQLType(((cDBField)oCCFieldKey.GetValue(i)).Type));
        }
        _oFieldsKey.AddRange(oCCFieldKey.Keys, oAAFieldKey.ToArray());
    }
    /// <summary>
    /// 
    /// </summary>
    public bool ActiveNullValue
    {
        get { return _bActiveNullValue; }
        set { _bActiveNullValue = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="strType"></param>
    /// <param name="strNameMatchProperty">[nomecampodb]=[nomeproperty]&[nomecampodb]=[nomeproperty]&[nomecampodb]=[nomeproperty]</param>
    public void SetType(string sTypeName, string strNameMatchProperty)
    {
        cCollection oMatch = new cCollection();
        oMatch.SetCollectionFromString(strNameMatchProperty);
        if (_oTypes.ContainsKey(sTypeName))
            _oTypes[sTypeName] = oMatch;
        else
            _oTypes.Add(sTypeName, oMatch);
    }
    /// <summary>
    /// 
    /// </summary>
    public cDataManager DataManager
    {
        get { return _oData; }
        set { _oData = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public string TableName
    {
        get { return _strNameTable; }
    }
    /// <summary>
    /// 
    /// </summary>
    public cCollection Fields
    {
        get { return _oFields; }
    }
    /// <summary>
    /// 
    /// </summary>
    public cCollection FieldsKey
    {
        get { return _oFieldsKey; }
    }
    /// <summary>
    /// 
    /// </summary>
    public cCollection Types
    {
        get { return _oTypes; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <returns></returns>
    public DataTable Get(string sKeyConnection)
    {
        try
        {
            DataSet ds = null;
            ds = _oData.GetDS(sKeyConnection, "SELECT " + soFileds + " FROM " + _strNameTable);
            if (ds == null)
                return null;
            if (ds.Tables.Count == 0)
                return null;
            if (ds.Tables[0].Rows.Count == 0)
                return null;
            ds.Tables[0].TableName = _strNameTable;
            return ds.Tables[0];
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="oValuesKey"></param>
    /// <returns></returns>
    public DataTable Get(string sKeyConnection, params object[] oValuesKey)
    {
        try
        {
            //
            if (oValuesKey != null && oValuesKey[0] != null)
            {
                if (oValuesKey[0].GetType().Equals(Type.GetType("System.Object[]")))
                {
                    oValuesKey = (object[])oValuesKey[0];
                }
            }
            //
            DataSet ds = null;
            string strSQL = "SELECT " + soFileds + " FROM " + _strNameTable + " WHERE ";
            strSQL += _generateAND(_oFieldsKey, oValuesKey);
            ds = _oData.GetDS(sKeyConnection, strSQL);
            if (ds == null || ds.Tables == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return null;
            ds.Tables[0].TableName = _strNameTable;
            return ds.Tables[0];
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="sWhere"></param>
    /// <returns></returns>
    public DataTable GetWhere(string sKeyConnection, string sWhere)
    {
        try
        {
            if (sWhere.Trim().Length == 0)
                return Get(sKeyConnection);
            //
            DataSet ds = null;
            ds = _oData.GetDS(sKeyConnection, "SELECT " + soFileds + " FROM " + _strNameTable + " WHERE " + sWhere);
            if (ds == null)
                return null;
            if (ds.Tables.Count == 0)
                return null;
            if (ds.Tables[0].Rows.Count == 0)
                return null;
            return ds.Tables[0];
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="sWhere"></param>
    /// <param name="sNameFieldTarget"></param>
    /// <param name="sNameFieldValue"></param>
    /// <param name="svNameKeysRows"></param>
    /// <param name="svValueHorizontal"></param>
    /// <returns></returns>
    public DataTable GetHorizontalWhere(string sKeyConnection, string sWhere, string sNameFieldTarget, string sNameFieldValue, string[] svNameKeysRows, string[] svValueHorizontal)
    {
        try
        {
            DataSet ds = null;
            string strSQL = "SELECT " + soFileds + " FROM " + _strNameTable + " WHERE ";
            strSQL += (sWhere.Length > 0) ? " ( " + sWhere + " ) " + ((svValueHorizontal.Length > 0) ? " AND " : " ") : "";
            if (svValueHorizontal.Length > 0)
                strSQL += " ( " + _generateOR(sNameFieldTarget, svValueHorizontal) + " ) ";
            strSQL += " ORDER BY " + _concat(svNameKeysRows, ", ");
            //
            ds = _oData.GetDS(sKeyConnection, strSQL);
            if (ds == null)
                return null;
            if (ds.Tables.Count == 0)
                return null;
            if (ds.Tables[0].Rows.Count == 0)
                return null;

            DataTable oDT = new DataTable("Table");
            for (int i = 0; i < svNameKeysRows.Length; i++)
            {
                oDT.Columns.Add(svNameKeysRows[i]);
            }
            for (int i = 0; i < svValueHorizontal.Length; i++)
            {
                oDT.Columns.Add(svValueHorizontal[i]);
            }
            //
            object[] oPrevValues = null; //new object[svNameKeysRows.Length];
            object[] oCurrentValues = null;
            //
            DataRow oDR = oDT.NewRow();
            oPrevValues = _DataRowToObjects(svNameKeysRows, ds.Tables[0].Rows[0]);
            //int indexDT = 0; 
            //bool isEqual= true;
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                oCurrentValues = _DataRowToObjects(svNameKeysRows, ds.Tables[0].Rows[i]);
                if (!ArrayEquals(oPrevValues, oCurrentValues))
                {
                    for (int idKey = 0; idKey < svNameKeysRows.Length; idKey++)
                    {
                        oDR[svNameKeysRows[idKey]] = oPrevValues[idKey];
                    }
                    oDT.Rows.Add(oDR);
                    oDR = oDT.NewRow();
                    oPrevValues = oCurrentValues;
                }
                oDR[(string)ds.Tables[0].Rows[i][sNameFieldTarget]] = ds.Tables[0].Rows[i][sNameFieldValue];
            }
            for (int idKey = 0; idKey < svNameKeysRows.Length; idKey++)
            {
                oDR[svNameKeysRows[idKey]] = oPrevValues[idKey];
            }
            oDT.Rows.Add(oDR);
            return oDT;
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="sNameFieldTarget"></param>
    /// <param name="sNameFieldValue"></param>
    /// <param name="svNameKeysRows"></param>
    /// <param name="svValueHorizontal"></param>
    /// <returns></returns>
    public DataTable GetHorizontal(string sKeyConnection, string sNameFieldTarget, string sNameFieldValue, string[] svNameKeysRows, string[] svValueHorizontal)
    {
        try
        {
            DataSet ds = null;
            string strSQL = "SELECT " + soFileds + " FROM " + _strNameTable + " WHERE ";
            strSQL += _generateOR(sNameFieldTarget, svValueHorizontal);
            strSQL += " ORDER BY " + _concat(svNameKeysRows, ", ");
            //
            ds = _oData.GetDS(sKeyConnection, strSQL);
            if (ds == null)
                return null;
            if (ds.Tables.Count == 0)
                return null;
            if (ds.Tables[0].Rows.Count == 0)
                return null;
            //
            DataTable oDT = new DataTable("Table");
            for (int i = 0; i < svNameKeysRows.Length; i++)
            {
                oDT.Columns.Add(svNameKeysRows[i]);
            }
            for (int i = 0; i < svValueHorizontal.Length; i++)
            {
                oDT.Columns.Add(svValueHorizontal[i]);
            }
            //
            object[] oPrevValues = null;
            object[] oCurrentValues = null;
            //
            DataRow oDR = oDT.NewRow();
            oPrevValues = _DataRowToObjects(svNameKeysRows, ds.Tables[0].Rows[0]);
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                oCurrentValues = _DataRowToObjects(svNameKeysRows, ds.Tables[0].Rows[i]);
                if (!ArrayEquals(oPrevValues, oCurrentValues))
                {
                    for (int idKey = 0; idKey < svNameKeysRows.Length; idKey++)
                    {
                        oDR[svNameKeysRows[idKey]] = oPrevValues[idKey];
                    }
                    oDT.Rows.Add(oDR);
                    oDR = oDT.NewRow();
                    oPrevValues = oCurrentValues;
                }
                oDR[(string)ds.Tables[0].Rows[i][sNameFieldTarget]] = ds.Tables[0].Rows[i][sNameFieldValue];
            }
            for (int idKey = 0; idKey < svNameKeysRows.Length; idKey++)
            {
                oDR[svNameKeysRows[idKey]] = oPrevValues[idKey];
            }
            oDT.Rows.Add(oDR);
            return oDT;
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oObject"></param>
    /// <param name="oDR"></param>
    /// <param name="oMatch"></param>
    private void SetProperties(ref object oObject, DataRow oDR, cCollection oMatch)
    {
        try
        {
            if (oObject == null || oDR == null || oMatch == null)
            {
                throw (new Exception("oObject is null, or oDR is null, or oMatch is null"));
            }
            object ovalue = null;
            for (int i = 0; i < oMatch.Count; i++)
            {
                try
                {
                    ovalue = oDR[(string)oMatch.GetKey(i)];
                    if (ovalue != null && ovalue != DBNull.Value)
                        oReflection.CallPropertySet(oObject, (string)oMatch.GetValue(i), ovalue);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" Error " + ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oDataTable"></param>
    /// <param name="cType"></param>
    /// <returns></returns>
    public object[] ConvertDataTableToObjects(DataTable oDataTable, Type cType)
    {
        return ConvertDataTableToObjects(oDataTable, cType, "", "");
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oDataTable"></param>
    /// <param name="cType"></param>
    /// <param name="sFilter"></param>
    /// <param name="sSort"></param>
    /// <returns></returns>
    public object[] ConvertDataTableToObjects(DataTable oDataTable, Type cType, string sFilter, string sSort)
    {
        ArrayList oArr = new ArrayList();
        DataRow[] ovDR = oDataTable.Select(sFilter, sSort);
        for (int i = 0; ovDR != null && i < ovDR.Length; i++)
        {
            object oB = Activator.CreateInstance(cType);
            SetProperties(ref oB, ovDR[i], (cCollection)_oTypes[cType.ToString()]);
            oArr.Add(oB);
        }
        return oArr.ToArray();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="oObject"></param>
    /// <returns></returns>
    public DataTable GetDataTable(string sKeyConnection, object oObject)
    {
        DataTable oDT = null;
        if (!this._oTypes.ContainsKey(oObject.GetType().ToString()))
            throw (new Exception("Type not supported"));
        //
        object[] oValues = ObjectToArray(oObject);
        //
        if (oValues == null)
            return null;
        //
        oDT = GetFilter(sKeyConnection, oValues);
        return oDT;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="oObject"></param>
    /// <returns></returns>
    public object[] GetObject(string sKeyConnection, object oObject)
    {
        DataTable oDT = GetDataTable(sKeyConnection, oObject);
        if (oDT == null)
            return null;
        return ConvertDataTableToObjects(oDT, oObject.GetType());
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="oObject"></param>
    /// <param name="sFilter"></param>
    /// <param name="sSort"></param>
    /// <returns></returns>
    public object[] GetObject(string sKeyConnection, object oObject, string sFilter, string sSort)
    {
        DataTable oDT = GetDataTable(sKeyConnection, oObject);
        if (oDT == null)
            return null;
        return ConvertDataTableToObjects(oDT, oObject.GetType(), sFilter, sSort);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oVet1"></param>
    /// <param name="oVet2"></param>
    /// <returns></returns>
    protected bool ArrayEquals(object[] oVet1, object[] oVet2)
    {
        if (oVet1.Length != oVet2.Length)
            return false;
        for (int i = 0; i < oVet1.Length; i++)
        {
            if (!CompareObject(oVet1[i], oVet2[i]))
                return false;
        }
        return true;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKey"></param>
    /// <param name="oValuesKey"></param>
    /// <param name="strNameField"></param>
    /// <returns></returns>
    public object GetField(string sKey, object[] oValuesKey, string strNameField)
    {
        try
        {
            DataSet ds = null;
            string strSQL = "SELECT " + strNameField + " FROM " + _strNameTable + " WHERE ";
            strSQL += _generateAND(_oFieldsKey, oValuesKey);
            ds = _oData.GetDS(sKey, strSQL);
            if (ds == null)
                return null;
            if (ds.Tables.Count == 0)
                return null;
            if (ds.Tables[0].Rows.Count == 0)
                return null;
            return ds.Tables[0].Rows[0][0];
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="strNameField"></param>
    /// <param name="oValue"></param>
    /// <returns></returns>
    public DataTable Find(string sKeyConnection, string strNameField, object oValue)
    {
        try
        {
            DataSet ds = null;
            ds = _oData.GetDS(sKeyConnection, "SELECT " + soFileds + " FROM " + _strNameTable + " WHERE " + strNameField + " = " + GetSQLCampo(oValue, (string)_oFields[strNameField]));
            if (ds == null)
                return null;
            if (ds.Tables.Count == 0)
                return null;
            if (ds.Tables[0].Rows.Count == 0)
                return null;
            return ds.Tables[0];
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="strNameFields"></param>
    /// <param name="oValues"></param>
    /// <returns></returns>
    public DataTable Find(string sKeyConnection, string[] strNameFields, object[] oValues)
    {
        DataSet ds = null;
        try
        {
            if (strNameFields.Length != oValues.Length)
                return null;
            string strSQL = "SELECT " + soFileds + " FROM " + _strNameTable + " ";
            strSQL += " WHERE ";
            for (int i = 0; i < strNameFields.Length; i++)
            {
                string sType = (string)this._oFields[strNameFields[i]];
                if (sType.ToLower().Equals("string"))
                    strSQL += " " + strNameFields[i] + " LIKE " + GetSQLCampo(oValues[i], sType);
                else
                    strSQL += " " + strNameFields[i] + " = " + GetSQLCampo(oValues[i], sType);
                if (i < strNameFields.Length - 1)
                    strSQL += " AND ";
            }
            ds = _oData.GetDS(sKeyConnection, strSQL);
            if (ds == null)
                return null;
            if (ds.Tables.Count == 0)
                return null;
            if (ds.Tables[0].Rows.Count == 0)
                return null;
            return ds.Tables[0];
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    /// <summary>
    /// Viene utilizzato per la conversione die valori.
    /// </summary>
    /// <param name="oValue">Valore da interpretare</param>
    /// <returns>Stringa contenente in valore compatibile con istruzioni SQL</returns>
    public string GetSQLCampo(object oValue)
    {
        try
        {
            string strCampo = "";
            if (oValue.GetType() == Type.GetType("System.Int64"))
                strCampo += " " + oValue.ToString() + " ";
            if (oValue.GetType() == Type.GetType("System.Int32"))
                strCampo += " " + oValue.ToString() + " ";
            if (oValue.GetType() == Type.GetType("System.String"))
                strCampo += "'" + oValue.ToString() + "'";
            if (oValue.GetType() == Type.GetType("System.DateTime"))
            {
                int dd = DateTime.Parse(oValue.ToString()).Day;
                int mm = DateTime.Parse(oValue.ToString()).Month;
                int yyyy = DateTime.Parse(oValue.ToString()).Year;
                if (yyyy == 1)
                    yyyy = 1901;
                int hh = DateTime.Parse(oValue.ToString()).Hour;
                int min = DateTime.Parse(oValue.ToString()).Minute;
                int ss = DateTime.Parse(oValue.ToString()).Second;
                strCampo += "'" + yyyy + "-" + mm.ToString().PadLeft(2, '0') + "-" + dd.ToString().PadLeft(2, '0') + " " + hh.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0') + ":" + ss.ToString().PadLeft(2, '0') + "'";
            }
            if (oValue.GetType() == Type.GetType("System.Double"))
                strCampo += " " + oValue.ToString().Replace(",", ".") + " ";
            if (oValue.GetType() == Type.GetType("System.Boolean"))
                strCampo += "'" + oValue.ToString() + "'";
            if (oValue.GetType() == Type.GetType("System.Byte[]"))
            {
                strCampo += " 0x" + oReflection.BinaryToHex((byte[])oValue) + " ";
            }
            return strCampo;
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sType"></param>
    /// <returns></returns>
    public string GetXMLToSQLType(string sType)
    {
        try
        {
            //string strCampo = "";
            sType = sType.Trim().ToLower();
            if (sType.Equals("varchar") || sType.Equals("nvarchar"))
                return "string";
            if (sType.Equals("string"))
                return "string";
            if (sType.Equals("date") || sType.Equals("datetime"))
                return "date";
            if (sType.Equals("double") || sType.Equals("real") || sType.Equals("numeric"))
                return "double";
            if (sType.Equals("boolean") || sType.Equals("bool") || sType.Equals("bit"))
                return "bool";
            if (sType.Equals("byte") || sType.Equals("binary") || sType.Equals("image") || sType.Equals("blob"))
                return "blob";
            return sType;
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    /// <summary>
    /// Viene utilizzato all'interno di cTable per la mappatura dei tipi a valori in sql.
    /// </summary>
    /// <param name="oValue"></param>
    /// <param name="sType"></param>
    /// <returns></returns>
    public string GetSQLCampo(object oValue, string sType)
    {
        try
        {
            string strCampo = "";
            sType = sType.Trim().ToLower();
            if (sType.Equals("int") || sType.Equals("smallint"))
                strCampo += " " + oValue.ToString() + " ";
            if (sType.Equals("string") || sType.Equals("text") || sType.Equals("ntext") || sType.Equals("varchar") || sType.Equals("nvarchar"))
                strCampo += "'" + oValue.ToString().Replace("'", "''") + "'";
            if (sType.Equals("date") || sType.Equals("datetime"))
            {
                int dd = DateTime.Parse(oValue.ToString()).Day;
                int mm = DateTime.Parse(oValue.ToString()).Month;
                int yyyy = DateTime.Parse(oValue.ToString()).Year;
                if (yyyy == 1)
                    yyyy = 1901;
                int hh = DateTime.Parse(oValue.ToString()).Hour;
                int min = DateTime.Parse(oValue.ToString()).Minute;
                int ss = DateTime.Parse(oValue.ToString()).Second;
                strCampo += "'" + yyyy + "-" + mm.ToString().PadLeft(2, '0') + "-" + dd.ToString().PadLeft(2, '0') + "T" + hh.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0') + ":" + ss.ToString().PadLeft(2, '0') + "'";
            }
            if (sType.Equals("double") || sType.Equals("real") || sType.Equals("numeric"))
                strCampo += " " + oValue.ToString().Replace(",", ".") + " ";
            if (sType.Equals("boolean") || sType.Equals("bool") || sType.Equals("bit"))
                strCampo += "'" + oValue.ToString() + "'";
            if (sType.Equals("byte") || sType.Equals("binary") || sType.Equals("image") || sType.Equals("blob"))
            {
                strCampo += " 0x" + oReflection.BinaryToHex((byte[])oValue) + " ";
            }
            return strCampo;
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    //
    private bool CompareObject(object oObj1, object oObj2)
    {
        try
        {
            if (oObj1.Equals(oObj2))
                return true;
            return false;
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    //
    private object[] ObjectToArray(object oObject)
    {
        if (!this._oTypes.ContainsKey(oObject.GetType().ToString()))
            throw (new Exception("Type not supported"));
        cCollection oMatch = (cCollection)_oTypes[oObject.GetType().ToString()];
        ArrayList oArrValues = new ArrayList();
        for (int i = 0; i < _oFields.Count; i++)
        {
            if (oMatch.ContainsKey(_oFields.GetKey(i)))
                oArrValues.Add(oReflection.CallPropertyGet(oObject, (string)oMatch[_oFields.GetKey(i)]));
            else
                oArrValues.Add(null);
        }
        return oArrValues.ToArray();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="oObject"></param>
    public void SetObject(string sKeyConnection, object oObject)
    {
        if (!this._oTypes.ContainsKey(oObject.GetType().ToString()))
            throw (new Exception("Type not supported"));
        //
        Set(sKeyConnection, ObjectToArray(oObject));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="oDTable"></param>
    /// <returns></returns>
    public bool Set(string sKeyConnection, DataTable oDTable)
    {
        if (oDTable == null)
            return false;
        for (int i = 0; i < oDTable.Rows.Count; i++)
        {
            ArrayList oADD = new ArrayList();
            for (int j = 0; j < this._oFields.Count; j++)
            {
                if (oDTable.Rows[i].RowState != DataRowState.Deleted)
                    oADD.Add(oDTable.Rows[i][(string)_oFields.GetKey(j)]);
            }
            Set(sKeyConnection, oADD.ToArray());
        }
        return true;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="oValues"></param>
    /// <returns></returns>
    public DataTable GetFilter(string sKeyConnection, params object[] oValues)
    {
        try
        {
            if (oValues != null && oValues[0] != null && oValues[0] != DBNull.Value)
            {
                if (oValues[0].GetType().Equals(Type.GetType("System.Object[]")))
                {
                    oValues = (object[])oValues[0];
                }
            }
            //
            DataSet ds;
            string strSQL = "SELECT " + soFileds + " FROM " + _strNameTable + " WHERE ";
            cCollection oFileds = GetFields(oValues);
            object[] oValuesNN = GetValuesNoNull(oValues);
            //
            strSQL += _generateWHARE(oFileds, oValuesNN, "AND");
            ds = _oData.GetDS(sKeyConnection, strSQL, "Main");
            if (ds == null)
                return null;
            if (ds.Tables.Count == 0)
                return null;
            if (ds.Tables[0].Rows.Count == 0)
                return null;
            ds.Tables[0].TableName = _strNameTable;
            return ds.Tables[0];
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    //
    // Non viene ancora utilizzata, ma con il tempo potrebbe essere adottata in quei casi dove si rende
    // necessario il controllo sull'offset e il limite.
    public DataTable GetFilter(string sKeyConnection, int iOffSet, int iLimit, params object[] oValues)
    {
        try
        {
            if (oValues != null && oValues[0] != null && oValues[0] != DBNull.Value)
                if (oValues[0].GetType().Equals(Type.GetType("System.Object[]")))
                    oValues = (object[])oValues[0];
            DataSet ds;
            int iOffSet2 = iOffSet + iLimit;
            string sSQL = "SELECT " + soFileds + " FROM ( SELECT TOP " + iOffSet + " " + soFileds + " FROM ( SELECT TOP " + iOffSet2 + " " + soFileds + " FROM " + _strNameTable + " WHERE {0} ORDER BY key ASC ) AS FOO ORDER BY key DESC ) AS BAR ORDER BY key ASC";
            cCollection oFileds = GetFields(oValues);
            object[] oValuesNN = GetValuesNoNull(oValues);
            //
            sSQL = String.Format(sSQL, _generateWHARE(oFileds, oValuesNN, "AND"));
            ds = _oData.GetDS(sKeyConnection, sSQL, "Main");
            if (ds == null)
                return null;
            if (ds.Tables.Count == 0)
                return null;
            if (ds.Tables[0].Rows.Count == 0)
                return null;
            ds.Tables[0].TableName = _strNameTable;
            return ds.Tables[0];
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="oValues"></param>
    /// <returns></returns>
    public bool Set(string sKeyConnection, params object[] oValues)
    {
        try
        {
            if (oValues != null && oValues[0] != null && oValues[0] != DBNull.Value)
            {
                if (oValues[0].GetType().Equals(Type.GetType("System.Object[]")))
                {
                    oValues = (object[])oValues[0];
                }
            }
            //
            DataSet ds;
            string strSQL = "SELECT " + soFileds + " FROM " + _strNameTable + " WHERE ";
            //
            // Quando uno dei campi chiave è null è possibile
            // fare solo l'inserimento.
            if (IsKeyNull(oValues))
            {
                strSQL = "INSERT INTO " + _strNameTable + " ";
                strSQL += " ( ";
                strSQL += _generateMERGEFileds(_oFields, oValues, ", ");
                strSQL += " ) VALUES ( ";
                strSQL += _generateMERGEValues(_oFields, oValues, ", ");
                strSQL += " ) ";
                _oData.ExecuteNonQuery(sKeyConnection, strSQL, null);
                return true;
            }
            //
            object[] oValuesKey = GetValuesKey(oValues);
            strSQL += _generateAND(_oFieldsKey, oValuesKey);
            ds = _oData.GetDS(sKeyConnection, strSQL, "Main");
            //
            if (ds.Tables["Main"].Rows.Count > 0)
            {
                strSQL = "UPDATE " + _strNameTable + " SET ";
                object[] oValuesNoKey = GetValuesNoKey(oValues);
                if (oValuesNoKey.Length == 0)
                    return false;
                cCollection oFieldsNoKey = GetFieldsNoKey();
                strSQL += _generateWHARE(oFieldsNoKey, oValuesNoKey, ", ");
                strSQL += " WHERE ";
                strSQL += _generateAND(_oFieldsKey, oValuesKey);
            }
            else
            {
                strSQL = "INSERT INTO " + _strNameTable + " ";
                strSQL += " ( ";
                strSQL += _generateMERGEFileds(_oFields, oValues, ", ");
                strSQL += " ) VALUES ( ";
                strSQL += _generateMERGEValues(_oFields, oValues, ", ");
                strSQL += " ) ";
            }
            _oData.ExecuteNonQuery(sKeyConnection, strSQL, null);
        }
        catch (Exception e)
        {
            throw (e);
        }
        return true;
    }
    //
    protected void SetField(string sKeyConnection, object[] oValuesKey, string strNameField, object oValue)
    {
        try
        {
            string strSQL = "UPDATE " + _strNameTable + " SET " + strNameField + " = " + GetSQLCampo(oValue, (string)_oFields[strNameField]);
            string strWhere = " WHERE ";
            strWhere += _generateAND(_oFieldsKey, oValuesKey);
            _oData.ExecuteNonQuery(sKeyConnection, strSQL + strWhere, null);
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="oObject"></param>
    public void DelObject(string sKeyConnection, object oObject)
    {
        if (!this._oTypes.ContainsKey(oObject.GetType().ToString()))
            throw (new Exception("Type not supported"));
        //
        Del(sKeyConnection, ObjectToArray(oObject));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="oValuesKey"></param>
    public void Del(string sKeyConnection, params object[] oValuesKey)
    {
        try
        {
            if (oValuesKey != null && oValuesKey[0] != null && oValuesKey[0] != DBNull.Value)
            {
                if (oValuesKey[0].GetType().Equals(Type.GetType("System.Object[]")))
                {
                    oValuesKey = (object[])oValuesKey[0];
                }
            }
            string strSQL = "DELETE FROM " + _strNameTable + " WHERE ";
            strSQL += _generateAND(_oFieldsKey, oValuesKey);
            _oData.ExecuteNonQuery(sKeyConnection, strSQL, null);
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    //
    // Questa funzione costruisce una stringa pronta per la clausula WHERE
    // concatenando i campi con i rispettivi valori... tramite un "Separator"
    private string _generateWHARE(cCollection ovFileds, object[] oValoreCampi, string sSeparator)
    {
        string strSQL = "";
        //
        for (int i = 0; i < ovFileds.Count; i++)
        {
            if (i > (oValoreCampi.Length - 1))
                continue;
            if (oValoreCampi[i] == null || oValoreCampi[i] == DBNull.Value)
                continue;
            if (strSQL.Trim().Length > 0)
                strSQL += " " + sSeparator.Trim() + " ";
            strSQL += " " + ovFileds.GetKey(i) + " = " + GetSQLCampo(oValoreCampi.GetValue(i), (string)ovFileds.GetValue(i));
        }
        //
        return strSQL;
    }
    // Questa funzione costruisce una stringa pronta per la clausula SELECT, INSERT, UPDATE
    // concatenando i campi ma rispettando i valori null
    private string _generateMERGEFileds(cCollection ovFileds, object[] oValoreCampi, string sSeparator)
    {
        string strSQL = "";
        //
        for (int i = 0; i < ovFileds.Count; i++)
        {
            if (i > (oValoreCampi.Length - 1))
                continue;
            if (oValoreCampi[i] == null || oValoreCampi[i] == DBNull.Value)
                continue;
            if (strSQL.Trim().Length > 0)
                strSQL += " " + sSeparator.Trim() + " ";
            strSQL += " " + ovFileds.GetKey(i) + " ";
        }
        //
        return strSQL;
    }
    //
    private string _generateMERGEValues(cCollection ovFileds, object[] oValoreCampi, string sSeparator)
    {
        string strSQL = "";
        //
        for (int i = 0; i < oValoreCampi.Length; i++)
        {
            if (oValoreCampi[i] == null || oValoreCampi[i] == DBNull.Value)
                continue;
            if (strSQL.Trim().Length > 0)
                strSQL += " " + sSeparator.Trim() + " ";
            strSQL += " " + GetSQLCampo(oValoreCampi[i], (string)ovFileds.GetValue(i)) + " ";
        }
        //
        return strSQL;
    }
    //
    // Questa funzione costruisce una stringa pronta per la clausula WHERE
    // concatenando i campi con i rispettivi valori... tramite AND
    private string _generateAND(cCollection ovFileds, object[] oValoreCampi)
    {
        return _generateWHARE(ovFileds, oValoreCampi, "AND");
    }
    //
    // Questa funzione costruisce una stringa pronta per la clausula WHERE
    // concatenando il campo con i rispettivi valori... tramite AND
    private string _generateAND(string strNomeCampo, object[] oValoreCampi)
    {
        cCollection oFileds = new cCollection();
        for (int i = 0; i < oValoreCampi.Length; i++)
        {
            oFileds.Add(strNomeCampo, _oFields[strNomeCampo]);
        }
        return _generateWHARE(oFileds, oValoreCampi, "AND");
    }
    //
    // Questa funzione costruisce una stringa pronta per la clausula WHERE
    // concatenando il campo con i rispettivi valori... tramite OR
    private string _generateOR(string strNomeCampo, object[] oValoreCampi)
    {
        cCollection oFileds = new cCollection();
        for (int i = 0; i < oValoreCampi.Length; i++)
        {
            oFileds.Add(strNomeCampo, _oFields[strNomeCampo]);
        }
        return _generateWHARE(oFileds, oValoreCampi, "OR");
    }
    //
    // Questa funzione prende in ingresso tutti i valori di un record
    // e restituisce i vero se uno dei campi chiave è null
    private bool IsKeyNull(object[] oValoreCampi)
    {
        for (int i = 0; i < _oFieldsKey.Count; i++)
        {
            object oValue = oValoreCampi[_oFields.GetIndex(_oFieldsKey.GetKey(i))];
            if (oValue == null || oValue == DBNull.Value)
                return true;
        }
        return false;
    }
    //
    // Questa funzione prende in ingresso tutti i valori di un record
    // e restituisce i soli valori dei campi chiave
    private object[] GetValuesKey(object[] oValoreCampi)
    {
        object[] oTmpValuesKey = new object[_oFieldsKey.Count];
        int cont = 0;
        for (int i = 0; i < _oFieldsKey.Count; i++)
        {
            oTmpValuesKey.SetValue(oValoreCampi.GetValue(_oFields.GetIndex(_oFieldsKey.GetKey(i))), cont++);
        }
        return oTmpValuesKey;
    }
    //
    // Questa funzione prende in ingresso tutti i valori di un record
    // e restituisce i soli valori che non appartengono alla chiave
    private object[] GetValuesNoKey(object[] oValoreCampi)
    {
        ArrayList oA = new ArrayList();
        for (int i = 0; i < _oFields.Count; i++)
        {
            if (!_oFieldsKey.ContainsKey(_oFields.GetKey(i)))
                oA.Add(oValoreCampi[i]);
        }
        return oA.ToArray();
    }
    //
    // Questa funzione restituisce tutti i nomi di colonna non chiavi
    private cCollection GetFieldsNoKey()
    {
        cCollection oCC = new cCollection();
        for (int i = 0; i < _oFields.Count; i++)
        {
            if (!_oFieldsKey.ContainsKey(_oFields.GetKey(i)))
                oCC.Add(_oFields.GetKey(i), _oFields.GetValue(i));
        }
        return oCC;
    }
    //
    // Questa funzione prende in ingresso tutti i valori di un record
    // e restituisce i nomi di colonna valorizzati
    private cCollection GetFields(object[] oValoreCampi)
    {
        cCollection oCC = new cCollection();
        for (int i = 0; i < _oFields.Count; i++)
        {
            //if(i < oValoreCampi.Length && oValoreCampi[i] != null && oValoreCampi[i] != DBNull.Value)
            if (i < oValoreCampi.Length && !IsNullValue(oValoreCampi[i]))
                oCC.Add(_oFields.GetKey(i), _oFields.GetValue(i));
        }
        return oCC;
    }
    //
    // Questa funzione prende in ingresso tutti i valori di un record
    // e restituisce i soli valori non null
    private object[] GetValuesNoNull(object[] oValues)
    {
        ArrayList oA = new ArrayList();
        for (int i = 0; i < oValues.Length; i++)
        {
            //if(oValues[i] != null && oValues[i] != DBNull.Value)
            if (!IsNullValue(oValues[i]))
                oA.Add(oValues[i]);
        }
        return oA.ToArray();
    }

    private bool IsNullValue(object oValue)
    {
        if (oValue == null)
            return true;
        if (oValue == DBNull.Value)
            return true;
        if (_bActiveNullValue)
        {
            switch (oValue.GetType().ToString())
            {
                case "System.Int32":
                    return oValue.Equals(Int32.MinValue);
                case "System.DateTime":
                    return oValue.Equals(DateTime.MinValue);
                case "System.Int64":
                    return oValue.Equals(Int64.MinValue);
                case "System.Double":
                    return oValue.Equals(Double.MinValue);
                case "System.String":
                    return oValue.Equals("");
            }
        }
        return false;
    }

    //
    // Questa funzione costruisce una stringa pronta per la clausula SELECT
    // concatenando i campi tramite virgola
    private string _concat(string[] strNomeCampi, string sSymbol)
    {
        string strSQL = "";
        for (int i = 0; i < strNomeCampi.Length; i++)
        {
            strSQL += strNomeCampi[i];
            if (i < strNomeCampi.Length - 1)
                strSQL += sSymbol;
        }
        return strSQL;
    }

    private string _concat(cCollection oFields, string sSymbol)
    {
        string strSQL = "";
        for (int i = 0; i < oFields.Count; i++)
        {
            strSQL += oFields.GetKey(i);
            if (i < oFields.Count - 1)
                strSQL += sSymbol;
        }
        return strSQL;
    }
    //
    // Questa funzione costruisce una stringa pronta per la clausula SELECT
    // concatenando i campi tramite virgola
    private object[] _DataRowToObjects(string[] strNomeCampi, DataRow oDataRow)
    {
        object[] oVet = new object[strNomeCampi.Length];
        try
        {
            if (oDataRow != null)
            {
                for (int i = 0; i < strNomeCampi.Length; i++)
                {
                    oVet[i] = oDataRow[strNomeCampi[i]];
                }
            }
            return oVet;
        }
        catch (Exception ex)
        {
            throw (ex);
        }
    }
}

/// <summary>
/// Questa classe gestisce una relazione di tipo 1-N.
/// </summary>
public class cTable1N
{
    private string _strKey = null;
    private cDataManager _oBus = null;
    private cTable _oTParent = null;
    private cTable _oTChild = null;
    private cCollection _oRelation = null;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strKey"></param>
    /// <param name="oBus"></param>
    public cTable1N(string strKey, cDataManager oBus)
    {
        _strKey = strKey;
        _oBus = oBus;
    }
    //
    // oRelation = k1(nomecampoKeyParent),v1(nomecampoChild); k2(nomecampoKeyParent),v2(nomecampoChild);
    public cTable1N(cTable oTParent, cTable oTChild, cCollection oRelation)
    {
        _oTParent = oTParent;
        _oTChild = oTChild;
        _oRelation = oRelation;
    }
    //
    //sRelation = [nomecampoKeyParent]&[nomecampoChild]=[nomecampoKeyParent]&[nomecampoChild]
    protected void Init(cTable oTParent, cTable oTChild, string sRelation)
    {
        _oTParent = oTParent;
        _oTChild = oTChild;
        _oRelation = new cCollection();
        //
        // Caricamento Campi Chiave
        if (sRelation.Length > 0)
        {
            _oRelation.SetCollectionFromString(sRelation);
        }
    }
    //
    public DataTable Get()
    {
        DataSet ds = null;
        string sSQL = "";
        sSQL = "SELECT " + _generateKeyVIRGULE(_oTParent.Fields) + ", " + _generateKeyVIRGULE(_oTChild.Fields);
        sSQL += " FROM " + _oTParent.TableName + " JOIN " + _oTChild.TableName;
        sSQL += " ON " + _generateAND(_oRelation);
        ds = _oBus.GetDS(_strKey, sSQL);
        //
        if (ds == null)
            return null;
        if (ds.Tables.Count == 0)
            return null;
        if (ds.Tables[0].Rows.Count == 0)
            return null;
        ds.Tables[0].TableName = _oTParent.TableName + "#" + _oTChild.TableName;
        return ds.Tables[0];
    }
    //
    public DataTable Get(params object[] oValuesKey)
    {
        try
        {
            //
            if (oValuesKey != null && oValuesKey[0] != null)
            {
                if (oValuesKey[0].GetType().Equals(Type.GetType("System.Object[]")))
                {
                    oValuesKey = (object[])oValuesKey[0];
                }
            }
            //
            DataSet ds = null;
            string sSQL = "";
            sSQL = "SELECT " + _generateKeyVIRGULE(_oTParent.Fields) + ", " + _generateKeyVIRGULE(_oTChild.Fields);
            sSQL += " FROM " + _oTParent.TableName + " JOIN " + _oTChild.TableName;
            sSQL += " ON " + _generateAND(_oRelation);
            cCollection oCWhere = _oRelation;
            if (oCWhere.Count == oValuesKey.Length)
            {
                for (int i = 0; i < oCWhere.Count; i++)
                {
                    oCWhere[oCWhere.GetKey(i)] = oValuesKey[i];
                }
            }
            sSQL += " WHERE " + _generateAND(oCWhere);
            //
            ds = _oBus.GetDS(_strKey, sSQL);
            if (ds == null)
                return null;
            if (ds.Tables.Count == 0)
                return null;
            if (ds.Tables[0].Rows.Count == 0)
                return null;
            ds.Tables[0].TableName = _oTParent.TableName + "#" + _oTChild.TableName;
            return ds.Tables[0];
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    //
    public DataTable Get(string sWhere)
    {
        try
        {
            if (sWhere.Trim().Length == 0)
                return Get();
            //
            DataSet ds = null;
            string sSQL = "";
            sSQL = "SELECT " + _generateKeyVIRGULE(_oTParent.Fields) + ", " + _generateKeyVIRGULE(_oTChild.Fields);
            sSQL += " FROM " + _oTParent.TableName + " JOIN " + _oTChild.TableName;
            sSQL += " ON " + _generateAND(_oRelation);
            sSQL += " WHERE " + sWhere;
            //
            if (ds == null)
                return null;
            if (ds.Tables.Count == 0)
                return null;
            if (ds.Tables[0].Rows.Count == 0)
                return null;
            ds.Tables[0].TableName = _oTParent.TableName + "#" + _oTChild.TableName;
            return ds.Tables[0];
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    //
    public DataTable GetWhere(string sWhere)
    {
        try
        {
            if (sWhere.Trim().Length == 0)
                return Get();
            //
            DataSet ds = null;
            string sSQL = "";
            sSQL = "SELECT " + _generateKeyVIRGULE(_oTParent.Fields) + ", " + _generateKeyVIRGULE(_oTChild.Fields);
            sSQL += " FROM " + _oTParent.TableName + " JOIN " + _oTChild.TableName;
            sSQL += " ON " + _generateAND(_oRelation);
            sSQL += " WHERE " + sWhere;
            //
            if (ds == null)
                return null;
            if (ds.Tables.Count == 0)
                return null;
            if (ds.Tables[0].Rows.Count == 0)
                return null;
            ds.Tables[0].TableName = _oTParent.TableName + "#" + _oTChild.TableName;
            return ds.Tables[0];
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    //
    protected bool ArrayEquals(object[] oVet1, object[] oVet2)
    {
        if (oVet1.Length != oVet2.Length)
            return false;
        for (int i = 0; i < oVet1.Length; i++)
        {
            if (!CompareObject(oVet1[i], oVet2[i]))
                return false;
        }
        return true;
    }
    //
    public DataTable Find(string strNameField, object oValue)
    {
        DataSet ds = null;
        string sSQL = "";
        sSQL = "SELECT " + _generateKeyVIRGULE(_oTParent.Fields) + ", " + _generateKeyVIRGULE(_oTChild.Fields);
        sSQL += " FROM " + _oTParent.TableName + " JOIN " + _oTChild.TableName;
        sSQL += " ON " + _generateAND(_oRelation);
        sSQL += " WHERE " + strNameField + " = " + GetSQLCampo(oValue);
        ds = _oBus.GetDS(_strKey, sSQL);
        //
        if (ds == null)
            return null;
        if (ds.Tables.Count == 0)
            return null;
        if (ds.Tables[0].Rows.Count == 0)
            return null;
        ds.Tables[0].TableName = _oTParent.TableName + "#" + _oTChild.TableName;
        return ds.Tables[0];
    }
    //
    public DataTable Find(string[] strNameFields, object[] oValues)
    {
        if (strNameFields.Length != oValues.Length)
            return null;

        //
        DataSet ds = null;
        string sSQL = "";
        sSQL = "SELECT " + _generateKeyVIRGULE(_oTParent.Fields) + ", " + _generateKeyVIRGULE(_oTChild.Fields);
        sSQL += " FROM " + _oTParent.TableName + " JOIN " + _oTChild.TableName;
        sSQL += " ON " + _generateAND(_oRelation);
        sSQL += " WHERE ";
        for (int i = 0; i < strNameFields.Length; i++)
        {
            sSQL += " " + strNameFields[i] + " = " + GetSQLCampo(oValues[i]);
            if (i < strNameFields.Length - 1)
                sSQL += " AND ";
        }
        //
        ds = _oBus.GetDS(_strKey, sSQL);
        //
        if (ds == null)
            return null;
        if (ds.Tables.Count == 0)
            return null;
        if (ds.Tables[0].Rows.Count == 0)
            return null;
        ds.Tables[0].TableName = _oTParent.TableName + "#" + _oTChild.TableName;
        return ds.Tables[0];
    }
    //
    private string GetSQLCampo(object oValue)
    {
        try
        {
            string strCampo = "";
            if (oValue.GetType() == Type.GetType("System.Int64"))
                strCampo += " " + oValue.ToString() + " ";
            if (oValue.GetType() == Type.GetType("System.Int32"))
                strCampo += " " + oValue.ToString() + " ";
            if (oValue.GetType() == Type.GetType("System.String"))
                strCampo += "'" + oValue.ToString() + "'";
            if (oValue.GetType() == Type.GetType("System.DateTime"))
            {
                int dd = DateTime.Parse(oValue.ToString()).Day;
                int mm = DateTime.Parse(oValue.ToString()).Month;
                int yyyy = DateTime.Parse(oValue.ToString()).Year;
                int hh = DateTime.Parse(oValue.ToString()).Hour;
                int min = DateTime.Parse(oValue.ToString()).Minute;
                int ss = DateTime.Parse(oValue.ToString()).Second;
                strCampo += "'" + yyyy + mm.ToString().PadLeft(2, '0') + dd.ToString().PadLeft(2, '0') + " " + hh + ":" + min + ":" + ss + "'";
            }
            if (oValue.GetType() == Type.GetType("System.Double"))
                strCampo += " " + oValue.ToString().Replace(",", ".") + " ";
            return strCampo;
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    //
    private bool CompareObject(object oObj1, object oObj2)
    {
        try
        {
            if (oObj1.GetType() != oObj2.GetType())
                return false;
            if (oObj1.GetType() == Type.GetType("System.Int64"))
                return ((long)oObj1 == (long)oObj2);
            if (oObj1.GetType() == Type.GetType("System.Int32"))
                return ((int)oObj1 == (int)oObj2);
            if (oObj1.GetType() == Type.GetType("System.String"))
                return (((string)oObj1).Equals((string)oObj2));
            if (oObj1.GetType() == Type.GetType("System.Double"))
                return ((double)oObj1 == (double)oObj2);
            return false;
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    //
    public void Del(params object[] oValuesKey)
    {
        try
        {
            //
            if (oValuesKey != null && oValuesKey[0] != null && oValuesKey[0] != DBNull.Value)
            {
                if (oValuesKey[0].GetType().Equals(Type.GetType("System.Object[]")))
                {
                    oValuesKey = (object[])oValuesKey[0];
                }
            }
            //
            string strSQL = "DELETE FROM " + _oTParent.TableName;
            string strSQL2 = "DELETE FROM " + _oTChild.TableName;
            //
            cCollection oCWhere = _oRelation;
            cCollection oCWhere2 = new cCollection();
            if (oCWhere.Count == oValuesKey.Length)
            {
                for (int i = 0; i < oCWhere.Count; i++)
                {
                    oCWhere[oCWhere.GetKey(i)] = oValuesKey[i];
                    oCWhere2.Add(_oRelation.GetValue(i), oValuesKey[i]);
                }
            }
            strSQL += " WHERE " + _generateAND(oCWhere);
            strSQL2 += " WHERE " + _generateAND(oCWhere2);

            _oBus.ExecuteNonQuery(_strKey, strSQL, null);
            _oBus.ExecuteNonQuery(_strKey, strSQL2, null);
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    //
    // Questa funzione costruisce una stringa pronta per la clausula WHERE
    // concatenando i campi con i rispettivi valori... tramite AND
    private string _generateAND(string[] strNomeCampi, object[] oValoreCampi)
    {
        if (strNomeCampi.Length != oValoreCampi.Length)
            return null;
        string strSQL = "";
        //
        int contNULL = 0;
        for (int j = 0; j < oValoreCampi.Length; j++)
            if (oValoreCampi[j] == null)
                contNULL++;
        //
        for (int i = 0; i < strNomeCampi.Length; i++)
        {
            if (oValoreCampi[i] == null)
                continue;
            strSQL += " " + strNomeCampi[i] + " = " + GetSQLCampo(oValoreCampi.GetValue(i));
            if (i < strNomeCampi.Length - 1 - contNULL)
                strSQL += " AND ";
        }
        //
        return strSQL;
    }
    //
    // Questa funzione costruisce una stringa pronta per la clausula WHERE
    // concatenando il campo con i rispettivi valori... tramite AND
    private string _generateAND(string strNomeCampo, object[] oValoreCampi)
    {
        string strSQL = "";
        //
        int contNULL = 0;
        for (int j = 0; j < oValoreCampi.Length; j++)
            if (oValoreCampi[j] == null || oValoreCampi[j] == DBNull.Value)
                contNULL++;
        //
        int contFileValid = 0;
        for (int i = 0; i < oValoreCampi.Length; i++)
        {
            if (oValoreCampi[i] == null || oValoreCampi[i] == DBNull.Value)
                continue;
            contFileValid++;
            strSQL += " " + strNomeCampo + " = " + GetSQLCampo(oValoreCampi.GetValue(i));
            if (contFileValid < oValoreCampi.Length - contNULL)
                strSQL += " AND ";
        }
        return strSQL;
    }
    //
    // Questa funzione costruisce una stringa pronta per la clausula ON
    // concatenando i campi della tabella padre
    // con i relativi cami della tabella figlia, tramite AND
    private string _generateAND(cCollection oRelation)
    {
        string strSQL = "";
        //
        int contNULL = 0;
        for (int j = 0; j < oRelation.Count; j++)
            if (oRelation.GetValue(j) == null || oRelation.GetValue(j) == DBNull.Value)
                contNULL++;
        //
        for (int i = 0; i < oRelation.Count; i++)
        {
            if (oRelation.GetValue(i) == null || oRelation.GetValue(i) == DBNull.Value)
                continue;
            strSQL += " " + oRelation.GetKey(i) + " = " + GetSQLCampo(oRelation.GetValue(i));
            if (i < oRelation.Count - 1 - contNULL)
                strSQL += " AND ";
        }
        //
        return strSQL;
    }
    //
    // Questa funzione costruisce una stringa pronta per la clausula WHERE
    // concatenando il campo con i rispettivi valori... tramite OR
    private string _generateOR(string strNomeCampo, object[] oValoreCampi)
    {
        string strSQL = "";
        //
        int contNULL = 0;
        for (int j = 0; j < oValoreCampi.Length; j++)
            if (oValoreCampi[j] == null)
                contNULL++;
        //
        for (int i = 0; i < oValoreCampi.Length; i++)
        {
            if (oValoreCampi[i] == null)
                continue;
            strSQL += " " + strNomeCampo + " = " + GetSQLCampo(oValoreCampi.GetValue(i));
            if (i < oValoreCampi.Length - 1 - contNULL)
                strSQL += " OR ";
        }
        return strSQL;
    }
    //
    // Questa funzione costruisce una stringa pronta per la clausula SELECT
    // concatenando i campi tramite virgola
    private string _generateKeyVIRGULE(cCollection oCampi)
    {
        string strSQL = "";
        for (int i = 0; i < oCampi.Count; i++)
        {
            strSQL += oCampi.GetKey(i);
            if (i < oCampi.Count - 1)
                strSQL += ", ";
        }
        return strSQL;
    }
    //
    // Questa funzione costruisce una stringa pronta per la clausula SELECT
    // concatenando i campi tramite virgola
    private object[] _DataRowToObjects(string[] strNomeCampi, DataRow oDataRow)
    {
        object[] oVet = new object[strNomeCampi.Length];
        if (oDataRow != null)
        {
            for (int i = 0; i < strNomeCampi.Length; i++)
            {
                oVet[i] = oDataRow[strNomeCampi[i]];
            }
        }
        return oVet;
    }
}
