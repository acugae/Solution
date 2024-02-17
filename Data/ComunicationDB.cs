namespace Solution.Data;

/// <summary>
/// 
/// </summary>
public interface IReader
{
    GCollection<string, string> getTables();
    GCollection<string, DBField> getFields(string strTable);
    GCollection<string, DBField> getFieldsKey(string strTable);
}
/// <summary>
/// 
/// </summary>
public class DBField
{
    string sName = "";
    string sType = "";
    string sDefault = "";
    int iLength = 0;
    bool bIsNull = false;
    bool bPrimaryKey = false;
    bool bIsIdentity = false;
    int iIdentityStart = 0;
    int iIdentityStep = 0;
    /// <summary>
    /// 
    /// </summary>
    public DBField()
    {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oField"></param>
    public DBField(DBField oField)
    {
        sName = oField.sName;
        sType = oField.sType;
        sDefault = oField.sDefault;
        iLength = oField.iLength;
        bIsNull = oField.bIsNull;
        bPrimaryKey = oField.bPrimaryKey;
        bIsIdentity = oField.bIsIdentity;
        iIdentityStart = oField.iIdentityStart;
        iIdentityStep = oField.iIdentityStep;
    }
    /// <summary>
    /// 
    /// </summary>
    public string Name
    {
        get { return sName; }
        set { sName = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public string Type
    {
        get { return sType; }
        set { sType = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public string Default
    {
        get { return sDefault; }
        set { sDefault = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public int Length
    {
        get { return iLength; }
        set { iLength = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public bool IsNull
    {
        get { return bIsNull; }
        set { bIsNull = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public bool PrimaryKey
    {
        get { return bPrimaryKey; }
        set { bPrimaryKey = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public bool IsIdentity
    {
        get { return bIsIdentity; }
        set { bIsIdentity = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public int IdentityStart
    {
        get { return iIdentityStart; }
        set { iIdentityStart = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public int IdentityStep
    {
        get { return iIdentityStep; }
        set { iIdentityStep = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetStringColumn()
    {
        bool bIsString = false;
        string strValue = "";
        //
        if (!sType.Equals(""))
        {
            //
            // Questi "if" eliminano la grandezza del campi per questi tipi
            if (sType.ToUpper().IndexOf("INT") >= 0
                || sType.ToUpper().IndexOf("DATETIME") >= 0
                || sType.ToUpper().IndexOf("BIT") >= 0
                || sType.ToUpper().IndexOf("UNIQUEIDENTIFIER") >= 0
                || sType.ToUpper().IndexOf("NTEXT") >= 0
                || sType.ToUpper().IndexOf("IMAGE") >= 0
            )
            {
                strValue = sType;
            }
            else if (sType.ToUpper().IndexOf("VARCHAR") >= 0 || sType.ToUpper().IndexOf("VARBINARY") >= 0)
            {
                strValue = sType + "(" + (iLength == -1 ? "MAX" : iLength.ToString()) + ")";
                bIsString = true;
            }
            /*
            if(sType.ToUpper().IndexOf("TEXT") >= 0)
            {
                int a = sType.IndexOf("(");
                int b = sType.IndexOf(")");
                if(a == -1)
                    strValue = sType;
                else
                    strValue = sType.Replace(sType.Substring(a, b-a+1),"");
            }
            if(sType.ToUpper().IndexOf("DATETIME") >= 0)
            {
                int a = sType.IndexOf("(");
                int b = sType.IndexOf(")");
                if(a == -1)
                    strValue = sType;
                else
                    strValue = sType.Replace(sType.Substring(a, b-a+1),"");
            }
            */
            /*
            int a = sType.IndexOf("(");
            int b = sType.IndexOf(")");
            if(a == -1)
                strValue = sType;
            else
                strValue = sType.Replace(sType.Substring(a, b-a+1),"");
            */
            else
                strValue = sType + "(" + iLength.ToString() + ")";
            //
        }
        //
        if (!this.Default.Equals(""))
            strValue += " DEFAULT " + (bIsString ? "'" + this.Default + "'" : this.Default) + " ";
        strValue += " " + (bIsNull ? "Null" : "Not Null");
        //strValue += " " + (bPrimaryKey ? "Primary Key" : "" );
        //
        return strValue;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetStringXML()
    {
        string sFields = @"\column&name=" + this.Name;
        sFields += "&type=" + this.Type;
        sFields += "&len=" + this.Length.ToString();
        sFields += "&isnull=" + this.IsNull.ToString();
        sFields += "&primarykey=" + this.PrimaryKey.ToString();
        if (this.IsIdentity)
        {
            sFields += "&isidentity=" + this.IsIdentity.ToString();
            sFields += "&identitystart=" + this.IdentityStart.ToString();
            sFields += "&identitystep=" + this.IdentityStep.ToString();
        }
        sFields += "&default=" + this.Default;
        return sFields;
    }

}


public class SQLDBRead : IReader
{
    private DataManager _oBUS;
    private string strConn;

    public SQLDBRead(string strKey, DataManager oBUS)
    {
        _oBUS = oBUS;
        strConn = strKey;
    }
    public GCollection<string, string> getTables()
    {
        DataSet ds;
        ds = _oBUS.GetDS(strConn, "Select name From sysobjects Where xtype = 'U' and name <> 'dtproperties' ORDER BY name", "Tables");
        GCollection<string, string> oC = new GCollection<string, string>();
        if (ds != null)
        {
            string[] myStrColl = new string[ds.Tables["Tables"].Rows.Count];
            for (int i = 0; i < ds.Tables["Tables"].Rows.Count; i++)
            {
                //myStrColl[i] = (string)ds.Tables["Tables"].DefaultView[i][0];
                oC.Add((string)ds.Tables["Tables"].DefaultView[i][0], null);
            }
            //oC.AddRange(myStrColl, null);
            return oC;
        }
        else
        {
            return null;
        }
    }
    //
    public GCollection<string, DBField> getFields(string strTable)
    {
        GCollection<string, DBField> myStrColl = new GCollection<string, DBField>();
        DataSet ds;
        //
        string sSQL = "SELECT syscolumns.name, systypes.name, syscolumns.length, syscolumns.isnullable, syscolumns.colstat, syscolumns.printfmt ";
        sSQL += "FROM sysobjects ";
        sSQL += "JOIN syscolumns ON sysobjects.id = syscolumns.id ";
        sSQL += "JOIN systypes ON systypes.xtype = syscolumns.xtype ";
        sSQL += "WHERE sysobjects.name = '" + strTable + "' AND systypes.status = 0 ORDER BY colorder";
        ds = _oBUS.GetDS(strConn, sSQL, "Fields");
        //
        string sSQLKey = "SELECT syscolumns.name ";
        sSQLKey += "FROM sysobjects ";
        sSQLKey += "JOIN syscolumns ON sysobjects.id = syscolumns.id ";
        sSQLKey += "JOIN sysindexkeys ON sysindexkeys.id = syscolumns.id ";
        sSQLKey += "WHERE sysobjects.name = '" + strTable + "' AND sysindexkeys.indid = 1 AND sysindexkeys.colid = syscolumns.colid";
        ds = _oBUS.GetDS(strConn, sSQLKey, "FieldsKey", ds);
        //
        GCollection<string, DBField> oCC = new GCollection<string, DBField>();
        for (int i = 0; ds.Tables["Fields"] != null && i < ds.Tables["Fields"].Rows.Count; i++)
        {
            DBField oDBF = new DBField();
            oDBF.Name = Convert.ToString(ds.Tables["Fields"].DefaultView[i][0]);
            oDBF.Type = Convert.ToString(ds.Tables["Fields"].DefaultView[i][1]);
            oDBF.Length = Convert.ToInt32(ds.Tables["Fields"].DefaultView[i][2]);
            oDBF.IsNull = Convert.ToBoolean(ds.Tables["Fields"].DefaultView[i][3]);
            oDBF.IsIdentity = (Convert.ToInt32(ds.Tables["Fields"].DefaultView[i][4]) == 1 ? true : false);
            //
            if (ds.Tables["Fields"].DefaultView[0][5] != DBNull.Value && ds.Tables["Fields"].DefaultView[0][5] != null)
            {
                byte[] autoval = ASCIIEncoding.ASCII.GetBytes((string)ds.Tables["Fields"].DefaultView[0][5]);
                if (autoval != null)
                {
                    oDBF.IdentityStart = Convert.ToInt32(autoval.GetValue(8));
                    oDBF.IdentityStep = Convert.ToInt32(autoval.GetValue(4));
                }
            }
            oCC.Add(oDBF.Name, oDBF);
        }
        for (int i = 0; ds.Tables["FieldsKey"] != null && i < ds.Tables["FieldsKey"].Rows.Count; i++)
        {
            DBField oDBF = (DBField)oCC[Convert.ToString(ds.Tables["Fields"].DefaultView[i][0])];
            oDBF.PrimaryKey = true;
            oCC[Convert.ToString(ds.Tables["Fields"].DefaultView[i][0])] = oDBF;
        }
        return oCC;
    }
    //
    public StringDictionary getFieldsValue(string strTable)
    {
        //
        StringDictionary myStrDic = new StringDictionary();
        GCollection<string, DBField> oFields;
        string myValue = "";
        string myType = "";
        //
        oFields = getFields(strTable);
        for (int i = 0; i < oFields.Count; i++)
        {
            DBField oDBF = (DBField)oFields.GetValue(i);
            myType = oDBF.Type;
            myValue = "[" + myType + "] ";
            myValue += "(" + oDBF.Length + ") ";

            if (myType.Equals("int"))
            {
                if (oDBF.IsIdentity)
                {
                    myValue += "IDENTITY {" + oDBF.IdentityStart + ", " + oDBF.IdentityStep + "} ";
                }
            }

            if (myType.Equals("nvarchar") && myType.Equals("varchar"))
            {
                string myCollation = getCollation(strTable, oFields.GetKey(i).ToString());
                if (!myCollation.Equals(""))
                {
                    myValue += "COLLATION " + myCollation + " ";
                }
            }

            if (oDBF.IsNull)
                myValue += "NULL";
            else
                myValue += "NOT NULL";
            //
            myStrDic.Add(oFields.GetKey(i).ToString(), myValue);
            //
        }
        return myStrDic;
    }

    public string getCollation(string strTable, string strField)
    {
        string collation;
        DataSet ds;
        ds = _oBUS.GetDS(strConn, "SELECT id From sysobjects Where name = '" + strTable + "'", "Tables");
        ds = _oBUS.GetDS(strConn, "SELECT collation FROM syscolumns WHERE id = " + ds.Tables["Tables"].DefaultView[0][0] + " and name ='" + strField + "'", "Collat", ds);
        collation = (string)ds.Tables["Collat"].DefaultView[0]["collation"];
        return collation;
    }

    public GCollection<string, DBField> getFieldsKey(string strTable)
    {
        GCollection<string, DBField> oCC = getFields(strTable);
        GCollection<string, DBField> oReturn = new GCollection<string, DBField>();
        for (int i = 0; i < oCC.Count; i++)
        {
            DBField oDBF = (DBField)oCC.GetValue(i);
            if (oDBF.PrimaryKey)
                oReturn.Add(oDBF.Name, oDBF);
        }
        return oReturn;
    }

    public string getFieldDefaultValue(string strTable, string strField)
    {
        DataSet ds;
        ds = _oBUS.GetDS(strConn, "Select id From sysobjects Where name = 'DF_" + strTable + "_" + strField + "'", "Tables");
        if (ds.Tables["Tables"].Rows.Count > 0)
        {
            ds = _oBUS.GetDS(strConn, "Select text From syscomments Where id = " + ds.Tables["Tables"].DefaultView[0][0].ToString(), "Fields", ds);
            if (ds.Tables["Fields"].Rows.Count > 0)
                return ds.Tables["Fields"].DefaultView[0][0].ToString();
        }
        return null;
    }

    public Collection GetStoreProcedures()
    {
        Collection myStrStore = new Collection();
        DataSet ds, ds2;
        ds = _oBUS.GetDS(strConn, "Select name, id From sysobjects Where xtype = 'P' and status >= 0", "Stored");
        for (int i = 0; i < ds.Tables["Stored"].Rows.Count; i++)
        {
            ds2 = _oBUS.GetDS(strConn, "Select text From syscomments Where id = " + ds.Tables["Stored"].DefaultView[i]["id"].ToString(), "Dettagli");
            myStrStore.Add((string)ds.Tables["Stored"].DefaultView[i]["name"], (string)ds2.Tables["Dettagli"].DefaultView[0]["text"]);
        }
        return myStrStore;
    }

}

public class SQLDBWrite
{
    private DataManager _oBUS;
    private string strConn;
    private SQLDBRead oSQLRead;
    //
    public SQLDBWrite(string strKey, DataManager oData)
    {
        _oBUS = oData;
        strConn = strKey;
        oSQLRead = new SQLDBRead(strConn, oData);
    }
    //
    private Collection Init(SQLDBRead oSQLRead, XMLDBRead oXMLRead)
    {
        Collection oCCResult = new Collection();
        Collection oCCTablesDB = new Collection();
        Collection oCCTablesXML = new Collection();
        //
        oCCTablesXML.AddRange(oXMLRead.getTables().Keys, null);
        oCCTablesDB.AddRange(oSQLRead.getTables().Keys, null);
        //
        for (int i = 0; i < oCCTablesXML.Count; i++)
        {
            if (oCCTablesDB.ContainsKey(oCCTablesXML.GetKey(i)))
            {
                oCCResult.Add(oCCTablesXML.GetKey(i), "mod"); // da modificare
            }
            else
            {
                oCCResult.Add(oCCTablesXML.GetKey(i), "add"); // da aggiungere
            }
        }
        //
        for (int i = 0; i < oCCTablesDB.Count; i++)
        {
            if (!oCCTablesXML.ContainsKey(oCCTablesDB.GetKey(i)))
            {
                oCCResult.Add(oCCTablesDB.GetKey(i), "del"); // da cancellare
            }
        }
        return oCCResult;
    }
    //
    public void Write(XMLDBRead oXMLRead)
    {
        Write(oXMLRead, false);
    }
    //
    public void Write(XMLDBRead oXMLRead, bool Deleted)
    {
        GCollection<string, DBField> ColumsXML;
        Collection ColumsKey;
        Collection ColumsDefaultValue;
        //
        Collection oCCTables = Init(oSQLRead, oXMLRead);
        //
        string sTableName = "";
        string sOperation = "";
        for (int i = 0; i < oCCTables.Count; i++)
        {
            sTableName = (string)oCCTables.GetKey(i);
            sOperation = (string)oCCTables.GetValue(i);
            //
            if (sOperation.ToLower().Equals("del"))
            {
                if (Deleted)
                {
                    delTable(sTableName);
                }
                continue;
            }
            //
            ColumsXML = oXMLRead.getFields(sTableName);
            if (sOperation.ToLower().Equals("add"))
                CreateTable(sTableName, ColumsXML);
            else if (sOperation.ToLower().Equals("mod"))
                AlterTable(sTableName, ColumsXML);
            //
            ColumsKey = oXMLRead.getFieldsPrimaryKey(sTableName);
            setPrimaryKey(sTableName, ColumsKey);
            //
            ColumsDefaultValue = oXMLRead.getFieldsNoDefaultValues(sTableName);
            delDefaultValue(sTableName, ColumsDefaultValue);
        }
        //
        Collection oCStore = oXMLRead.getStoreProcedures();
        for (int j = 0; j < oCStore.Count; j++)
        {
            setStoredProcedure((string)oCStore.GetKey(j), (string)oCStore.GetValue(j));
        }
        //
        // Imposta le stored procedure
        //setStoredProcedures();
    }

    public bool CreateTable(string strTable, GCollection<string, DBField> oFields)
    {
        string mySQL = "CREATE TABLE " + strTable;
        bool virgola = false;
        mySQL += " ( ";
        //
        for (int i = 0; i < oFields.Count; i++)
        {
            //foreach(string mykey in oFields.Keys)
            //{
            string mykey = (string)oFields.GetKey(i);
            if (virgola)
                mySQL += ", ";
            mySQL += " [" + mykey + "] ";
            mySQL += " " + ((DBField)oFields[mykey]).GetStringColumn() + " ";
            virgola = true;
        }
        //
        mySQL += " ) ";
        //
        try
        {
            _oBUS.ExecuteNonQuery(strConn, mySQL, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
        return true;
    }

    public bool AlterTable(string strTable, GCollection<string, DBField> Colums)
    {
        //StringDictionary ColumsDB;
        //ColumsDB  = oSQLRead.getFieldsValue(strTable);
        GCollection<string, DBField> ColumsDB = oSQLRead.getFields(strTable);
        //
        // Modifica e Aggiunta di campi nella Tabella
        for (int i = 0; i < Colums.Count; i++)
        {
            string mykey = (string)Colums.GetKey(i);
            try
            {
                if (ColumsDB.ContainsKey(mykey))
                {
                    AlterColumnTable(strTable, (DBField)Colums[mykey]);
                }
                else
                {
                    CreateColumnTable(strTable, (DBField)Colums[mykey]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        //
        // Cancellazione di campi nella Tabella
        foreach (string mykey in ColumsDB.Keys)
        {
            try
            {
                if (!Colums.ContainsKey(mykey))
                {
                    delColumnTable(strTable, mykey);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        return true;
    }
    //
    public bool delColumnTable(string strTable, string strColumn)
    {
        string mySQL = "ALTER TABLE " + strTable + " ";
        try
        {
            mySQL += "DROP COLUMN " + strColumn;
            _oBUS.ExecuteNonQuery(strConn, mySQL, null);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    public bool CreateColumnTable(string strTable, DBField oField)
    {
        string mySQL = "ALTER TABLE " + strTable + " ";
        try
        {
            mySQL += "ADD " + oField.Name + " " + oField.GetStringColumn();
            _oBUS.ExecuteNonQuery(strConn, mySQL, null);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    public bool AlterColumnTable(string strTable, DBField oField)
    {
        string mySQL = "ALTER TABLE " + strTable + " ";
        try
        {
            mySQL += "ALTER COLUMN " + oField.Name + " " + oField.GetStringColumn();
            _oBUS.ExecuteNonQuery(strConn, mySQL, null);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
    }

    public bool delTable(string strTable)
    {
        try
        {
            _oBUS.ExecuteNonQuery(this.strConn, "DROP TABLE " + strTable, null);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool delStoredProcedures()
    {
        Collection ostrDir = oSQLRead.GetStoreProcedures();
        //
        if (ostrDir == null)
            return false;
        //
        try
        {
            foreach (string strKey in ostrDir.Keys)
            {
                string mySQL = "DROP PROCEDURE " + strKey;
                try
                {
                    _oBUS.ExecuteNonQuery(strConn, mySQL, null);
                }
                catch
                {
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
        return true;
    }

    public bool delStoredProcedure(string strStoreProcedure)
    {
        string mySQL = "DROP PROCEDURE " + strStoreProcedure;
        try
        {
            _oBUS.ExecuteNonQuery(strConn, mySQL, null);
        }
        catch
        {
            return false;
        }
        return true;
    }
    /*
    public bool setStoredProcedures()
    {
        string mySQL = "";
        string[] Procedure = oXMLRead.getStoreProcedures();
        try
        {
            delStoredProcedures();
            for(int i = 0; i < Procedure.Length; i++)
            {
                mySQL = oXMLRead.getStoreProcedure( (string)Procedure[i] );
                _oBUS.ExecuteNonQuery(strConn, mySQL);
            }
        }
        catch
        {
            return false;
        }
        return true;
    }
    */
    public bool setStoredProcedure(string strNameStoredProcedure, string strValueStoredProcedure)
    {
        try
        {
            delStoredProcedure(strNameStoredProcedure);
            _oBUS.ExecuteNonQuery(strConn, strValueStoredProcedure, null);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex.Message);
            return false;
        }
        return true;
    }

    public bool CreatePrimaryKey(string strTable, string[] ColumsKey)
    {
        string mySQL = "";
        bool virgola = false;
        //
        mySQL += "ALTER TABLE " + strTable + " WITH NOCHECK ADD ";
        mySQL += "CONSTRAINT [PK_" + strTable + "] PRIMARY KEY CLUSTERED ";
        mySQL += " ( ";
        for (int i = 0; i < ColumsKey.Length; i++)
        {
            if (virgola)
                mySQL += ", ";
            mySQL += " [" + ColumsKey[i] + "] ";
            virgola = true;
        }
        mySQL += " ) ";
        //
        try
        {
            _oBUS.ExecuteNonQuery(strConn, mySQL, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
        return true;
    }

    public bool setPrimaryKey(string strTable, Collection ColumsKey)
    {
        string mySQL = "";
        bool virgola = false;
        //
        if (ColumsKey == null)
            return false;
        //
        mySQL += "ALTER TABLE " + strTable + " WITH NOCHECK ADD ";
        mySQL += "CONSTRAINT [PK_" + strTable + "] PRIMARY KEY CLUSTERED ";
        mySQL += " ( ";
        for (int i = 0; i < ColumsKey.Count; i++)
        {
            if (virgola)
                mySQL += ", ";
            mySQL += " [" + ((string)ColumsKey.GetKey(i)) + "] ";
            virgola = true;
        }
        mySQL += " ) ";
        //
        try
        {
            delPrimaryKey(strTable);
            _oBUS.ExecuteNonQuery(strConn, mySQL, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
        return true;
    }

    public bool delPrimaryKey(string strTable)
    {
        string mySQLDrop = "ALTER TABLE " + strTable + " DROP PK_" + strTable;
        //
        try
        {
            _oBUS.ExecuteNonQuery(strConn, mySQLDrop, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
        return true;
    }

    public bool delPrimaryKeys()
    {
        string mySQLDrop = "";
        string[] ostrColl = (string[])oSQLRead.getTables().Keys;
        //
        if (ostrColl == null)
            return false;
        for (int i = 0; i < ostrColl.Length; i++)
        {
            try
            {
                mySQLDrop = "ALTER TABLE " + ostrColl[i] + " DROP PK_" + ostrColl[i];
                _oBUS.ExecuteNonQuery(strConn, mySQLDrop, null);
            }
            catch
            {
            }
        }

        return true;
    }
    public bool setDefaultValue(string strTable, Collection Colums)
    {
        string mySQL = "";
        bool virgola = false;
        //
        GCollection<string, DBField> strcFields = oSQLRead.getFields(strTable);
        for (int i = 0; i < strcFields.Count; i++)
        {
            delDefaultValue(strTable, ((DBField)strcFields.GetValue(i)).Name);
        }
        //
        mySQL += "ALTER TABLE " + strTable + " WITH NOCHECK ADD ";
        //
        try
        {
            //
            foreach (string mykey in Colums.Keys)
            {
                if (virgola)
                    mySQL += ", ";
                mySQL += "CONSTRAINT [DF_" + strTable + "_" + mykey + "] DEFAULT " + Colums[mykey] + " FOR [" + mykey + "]";
                //
                virgola = true;
            }
            //
            _oBUS.ExecuteNonQuery(strConn, mySQL, null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }
        return true;
    }

    public void delDefaultValue(string strTable, Collection oCFields)
    {
        for (int i = 0; i < oCFields.Count; i++)
        {
            string sField = (string)oCFields.GetKey(i);
            delDefaultValue(strTable, sField);
        }
    }

    public bool delDefaultValue(string strTable, string strField)
    {
        string mySQLDrop = "ALTER TABLE " + strTable + " ALTER COLUMN " + strField + " DROP DEFAULT ";
        try
        {
            _oBUS.ExecuteNonQuery(strConn, mySQLDrop, null);
        }
        catch (Exception err)
        {
            Console.WriteLine(err.Message);
            return false;
        }
        return true;
    }
    //
    // Questa funzione prende in ingresso il tipo del campo
    // in formato XML e ne restituisce uno compatibile con il
    // data base SQLDB
    private string AdattaString(string strFieldValue)
    {
        string strValue = "";
        //
        if (!strFieldValue.Equals(""))
        {
            //
            // Questi "if" eliminano la grandezza del campi per questi tipi
            if (strFieldValue.ToUpper().IndexOf("INT") >= 0)
            {
                int a = strFieldValue.IndexOf("(");
                int b = strFieldValue.IndexOf(")");
                if (a == -1)
                    strValue = strFieldValue;
                else
                    strValue = strFieldValue.Replace(strFieldValue.Substring(a, b - a + 1), "");
                return strValue;
            }
            if (strFieldValue.ToUpper().IndexOf("IMAGE") >= 0)
            {
                int a = strFieldValue.IndexOf("(");
                int b = strFieldValue.IndexOf(")");
                if (a == -1)
                    strValue = strFieldValue;
                else
                    strValue = strFieldValue.Replace(strFieldValue.Substring(a, b - a + 1), "");
                return strValue;
            }
            if (strFieldValue.ToUpper().IndexOf("TEXT") >= 0)
            {
                int a = strFieldValue.IndexOf("(");
                int b = strFieldValue.IndexOf(")");
                if (a == -1)
                    strValue = strFieldValue;
                else
                    strValue = strFieldValue.Replace(strFieldValue.Substring(a, b - a + 1), "");
                return strValue;
            }
            if (strFieldValue.ToUpper().IndexOf("DATETIME") >= 0)
            {
                int a = strFieldValue.IndexOf("(");
                int b = strFieldValue.IndexOf(")");
                if (a == -1)
                    strValue = strFieldValue;
                else
                    strValue = strFieldValue.Replace(strFieldValue.Substring(a, b - a + 1), "");
                return strValue;
            }
        }
        //
        return strFieldValue;
        //
    }
}

public class XMLDBWrite : XML
{
    private XMLDBRead oXMLRead;
    //
    private string FIRSTNODEXML = "registry";
    string TABLES = "tables";
    string TABLE = "table";
    string COLUMN = "column";
    string PROCEDURES = "procedures";
    string PROCEDURE = "procedure";
    //
    public XMLDBWrite(string sFileName)
    {
        Load(sFileName);
        oXMLRead = new XMLDBRead();
        oXMLRead.Load(sFileName);
    }
    //
    // Inizializza le strutture per la procedura "WRITE"
    private Collection Init(SQLDBRead oSQLRead, XMLDBRead oXMLRead)
    {
        Collection oCCResult = new Collection();
        Collection oCCTablesDB = new Collection();
        Collection oCCTablesXML = new Collection();
        //
        oCCTablesXML.AddRange(oXMLRead.getTables().Keys, null);
        oCCTablesDB.AddRange(oSQLRead.getTables().Keys, null);
        //
        for (int i = 0; i < oCCTablesDB.Count; i++)
        {
            if (oCCTablesXML.ContainsKey(oCCTablesDB.GetKey(i)))
            {
                oCCResult.Add(oCCTablesDB.GetKey(i), "mod"); // da modificare
            }
            else
            {
                oCCResult.Add(oCCTablesDB.GetKey(i), "add"); // da aggiungere
            }
        }
        //
        for (int i = 0; i < oCCTablesXML.Count; i++)
        {
            if (!oCCTablesDB.ContainsKey(oCCTablesXML.GetKey(i)))
            {
                oCCResult.Add(oCCTablesXML.GetKey(i), "del"); // da cancellare
            }
        }
        return oCCResult;
    }
    //
    //
    public void WriteFromSQL(SQLDBRead oSQLRead)
    {
        GCollection<string, DBField> ColumsDB;
        Collection ColumsKey;
        //
        Collection oCCTables = Init(oSQLRead, oXMLRead);
        //
        string sTableName = "";
        string sOperation = "";
        for (int i = 0; i < oCCTables.Count; i++)
        {
            sTableName = (string)oCCTables.GetKey(i);
            sOperation = (string)oCCTables.GetValue(i);
            //
            ColumsDB = oSQLRead.getFields(sTableName);
            //
            if (sOperation.ToLower().Equals("add"))
                CreateTable(sTableName, ColumsDB);
            else if (sOperation.ToLower().Equals("mod"))
                AlterTable(sTableName, ColumsDB);
            else if (sOperation.ToLower().Equals("del"))
                delTable(sTableName);
            //
            ColumsKey = oXMLRead.getFieldsPrimaryKey(sTableName);
            setPrimaryKey(sTableName, ColumsKey);
            //
            //ColumsDefaultValue = oXMLRead.getFieldsDefaultValues(sTableName);
            //setDefaultValue(sTableName, ColumsDefaultValue);
            //
        }
        Collection oCStore = oSQLRead.GetStoreProcedures();
        for (int j = 0; j < oCStore.Count; j++)
        {
            setStoreProcedure((string)oCStore.GetKey(j), (string)oCStore.GetValue(j));
        }
        //setStoreProcedures();
        Save();
    }
    //
    public bool delTables()
    {
        return DelX("/" + FIRSTNODEXML + "/" + TABLES);
    }
    //
    public bool delTable(string strTable)
    {
        return DelX("/" + FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']");
    }
    //
    public bool delStoredProcedures()
    {
        //return oReg.delGroup(GroupProcedures);
        return false;
    }
    //
    public bool delStoredProcedure(string strName)
    {
        return DelX("/" + FIRSTNODEXML + "/" + PROCEDURES + "/" + PROCEDURE + "[@name='" + strName + "']");
    }
    //
    public void CreateColumn(string strTable, DBField oField)
    {
        if (GetX("/" + FIRSTNODEXML + "/" + TABLES) == null)
            InsertX("/" + FIRSTNODEXML, "<" + TABLES + "></" + TABLES + ">");
        //
        if (GetX("/" + FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']") == null)
            InsertX("/" + FIRSTNODEXML + "/" + TABLES, "<" + TABLE + " name='" + strTable + "'></" + TABLE + ">");
        //
        if (GetX("/" + FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@name='" + oField.Name + "']") == null)
            InsertX("/" + FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']", "<" + COLUMN + " name='" + oField.Name + "' />");
        //
        InsertX("/" + FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@name='" + oField.Name + "']", "type", oField.Type);
        InsertX("/" + FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@name='" + oField.Name + "']", "isnull", oField.IsNull.ToString());
        InsertX("/" + FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@name='" + oField.Name + "']", "len", oField.Length.ToString());
        InsertX("/" + FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@name='" + oField.Name + "']", "primarykey", oField.PrimaryKey.ToString());
        InsertX("/" + FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@name='" + oField.Name + "']", "default", oField.Default);
    }
    //
    public bool CreateTable(string strTable, GCollection<string, DBField> Columns)
    {

        for (int i = 0; i < Columns.Count; i++)
        {
            DBField oF = Columns.GetValue(i);
            CreateColumn(strTable, oF);
        }

        return true;
    }
    public bool AlterTable(string strTable, GCollection<string, DBField> Columns)
    {
        DelX("/" + FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN);
        try
        {
            for (int i = 0; i < Columns.Count; i++)
            {
                DBField oF = Columns.GetValue(i);
                CreateColumn(strTable, oF);
            }
        }
        catch
        {
            return false;
        }
        return true;
    }
    //
    public bool setStoreProcedure(string strProcedureName, string strProcedureBody)
    {
        delStoredProcedure(strProcedureName);
        if (GetX("/" + FIRSTNODEXML + "/" + PROCEDURES) == null)
            InsertX("/" + FIRSTNODEXML, "<" + PROCEDURES + "></" + PROCEDURES + ">");
        //
        if (GetX("/" + FIRSTNODEXML + "/" + PROCEDURES + "/" + PROCEDURE + "[@name='" + strProcedureName + "']") == null)
        {
            InsertX("/" + FIRSTNODEXML + "/" + PROCEDURES, "<" + PROCEDURE + " name='" + strProcedureName + "' />");
            InsertX("/" + FIRSTNODEXML + "/" + PROCEDURES + "/" + PROCEDURE + "[@name='" + strProcedureName + "']", strProcedureBody);
        }
        /*
        if(oReg.GetX("/" + FIRSTNODEXML + "/" + PROCEDURES + "/" + PROCEDURE + "[@name='" + strProcedureName + "']") != null)
            oReg.SetX("/" + FIRSTNODEXML + "/" + PROCEDURES + "/" + PROCEDURE, "<" + PROCEDURE + " name='" + strProcedureName + "'>" + strProcedureBody + "</" + PROCEDURE + ">");
        */
        return true;
    }
    //
    public void delDefaultValue(string strTable, string strField)
    {
        DelX("/" + FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@name='" + strField + "']/@default");
    }
    //
    public void delPrimaryKey(string strTable)
    {
        DelX("/" + FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@primarykey='True']/@primarykey");
    }
    //
    public bool setPrimaryKey(string strTable, Collection oFields)
    {
        if (oFields == null)
            return false;
        delPrimaryKey(strTable);
        for (int i = 0; i < oFields.Count; i++)
        {
            if (GetX("/" + FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@name='" + ((DBField)oFields.GetValue(i)).Name + "']") != null)
                InsertX("/" + FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@name='" + ((DBField)oFields.GetValue(i)).Name + "']", "primarykey", "True");
        }
        return true;
    }
    //
    private bool setDefaultValue(string strTable, string strField, string strValue)
    {
        delDefaultValue(strTable, strField);
        if (GetX("/" + FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@name='" + strField + "']") != null)
            InsertX("/" + FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@name='" + strField + "']", "default", strValue);
        return true;
    }
}
/// <summary>
/// Classe per la lettura di file XML contenete le specifiche per la persistenza dei dati.
/// </summary>
public class XMLDBRead : XML, IReader
{
    private string FIRSTNODEXML = "registry";
    string MAPS = "maps";
    string TYPE = "type";
    string TABLES = "tables";
    string TABLE = "table";
    string COLUMN = "column";
    string COMMANDS = "commands";
    string COMMAND = "command";
    string PROCEDURES = "procedures";
    string PROCEDURE = "procedure";
    /// <summary>
    /// 
    /// </summary>
    ///         
    public XMLDBRead()
    {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public GCollection<string, string> getTables()
    {
        string[] sNames = GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[not(@alias) or @alias='']/@name");
        string[] sAlias = GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[(@alias) and @alias!='']/@alias");
        //
        GCollection<string, string> oAlias = getAlias();
        GCollection<string, string> oTables = new GCollection<string, string>();
        for (int i = 0; sNames != null && i < sNames.Length; i++)
        {
            oTables.Add(sNames[i], null);
        }
        for (int i = 0; sAlias != null && i < sAlias.Length; i++)
        {
            if (sAlias[i].Equals(""))
                continue;
            oTables.Add(sAlias[i], oAlias[sAlias[i]]);
        }
        return oTables;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string[] getMapTypes()
    {
        string[] sNames = GetX(FIRSTNODEXML + "/" + MAPS + "/" + TYPE + "/@name");
        return sNames;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sType"></param>
    /// <returns></returns>
    public string[] getMapTables(string sType)
    {
        string[] sNames = GetX(FIRSTNODEXML + "/" + MAPS + "/" + TYPE + "[@name='" + sType + "']/@tablename");
        return sNames;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sType"></param>
    /// <returns></returns>
    public string[] getMapProperty(string sType)
    {
        string[] sNames = GetX(FIRSTNODEXML + "/" + MAPS + "/" + TYPE + "[@name='" + sType + "']/property/@name");
        return sNames;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sType"></param>
    /// <returns></returns>
    public string[] getMapColumn(string sType)
    {
        string[] sNames = GetX(FIRSTNODEXML + "/" + MAPS + "/" + TYPE + "[@name='" + sType + "']/property/@columnname");
        return sNames;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string[] getCommands()
    {
        string[] sNames = GetX(FIRSTNODEXML + "/" + COMMANDS + "/" + COMMAND + "/@name");
        return sNames;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sCommand"></param>
    /// <returns></returns>
    public string[] getCommand(string sCommand)
    {
        string[] sNames = GetX(FIRSTNODEXML + "/" + COMMANDS + "/" + COMMAND + "[@name='" + sCommand + "']");
        return sNames;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public GCollection<string, string> getAlias()
    {
        GCollection<string, string> ovAlias = new GCollection<string, string>();
        string[] osAlias = GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[(@alias) and @alias!='']/@alias");
        for (int i = 0; osAlias != null && i < osAlias.Length; i++)
        {
            string[] oS = GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@alias='" + osAlias[i] + "']/@name");
            ovAlias.Add(osAlias[i], oS[0]);
        }
        if (ovAlias.Count == 0)
            return null;
        return ovAlias;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sTableName"></param>
    /// <returns></returns>
    public string[] getAlias(string sTableName)
    {
        return GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + sTableName + "']/@alias");
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sTableName"></param>
    /// <param name="strField"></param>
    /// <returns></returns>
    public DBField getField(string sTableName, string strField)
    {
        string[] sNames = null;
        sNames = GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@alias='" + sTableName + "']/" + COLUMN + "[@name='" + strField + "']/@name");
        sNames ??= GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + sTableName + "' and (not(@alias) or @alias='')]/" + COLUMN + "[@name='" + strField + "']/@name");
        if (sNames == null)
            return null;
        //
        DBField oF = new DBField();
        oF.Name = strField;
        oF.IsNull = getIsNull(sTableName, strField);
        oF.Length = getFieldLength(sTableName, strField);
        oF.Type = getFieldType(sTableName, strField);
        oF.PrimaryKey = getFieldPrimaryKey(sTableName, strField);
        oF.IsIdentity = getFieldIsIdentity(sTableName, strField);
        //
        return oF;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strTable"></param>
    /// <returns></returns>
    public GCollection<string, DBField> getFields(string strTable)
    {
        GCollection<string, DBField> oCC = new GCollection<string, DBField>();
        string[] sNames = null;
        sNames = GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@alias='" + strTable + "']/" + COLUMN + "/@name");
        sNames ??= GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "' and (not(@alias) or @alias='')]/" + COLUMN + "/@name");
        if (sNames == null)
            return null;
        for (int i = 0; i < sNames.Length; i++)
        {
            DBField oF = new DBField();
            oF.Name = sNames[i];
            oF.IsNull = getIsNull(strTable, sNames[i]);
            oF.Length = getFieldLength(strTable, sNames[i]);
            oF.Type = getFieldType(strTable, sNames[i]);
            oF.PrimaryKey = getFieldPrimaryKey(strTable, sNames[i]);
            oF.Default = getFieldDefaultValue(strTable, sNames[i]);
            oF.IsIdentity = getFieldIsIdentity(strTable, sNames[i]);
            oCC.Add(sNames[i], oF);
        }
        return oCC;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strTable"></param>
    /// <returns></returns>
    public GCollection<string, DBField> getFieldsKey(string strTable)
    {
        GCollection<string, DBField> oCC = new GCollection<string, DBField>();
        string[] sNames = null;
        sNames = GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@alias='" + strTable + "']/" + COLUMN + "[@primarykey='True']/@name");
        sNames ??= GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "' and (not(@alias) or @alias='')]/" + COLUMN + "[@primarykey='True']/@name");
        if (sNames == null)
            return null;
        //
        for (int i = 0; i < sNames.Length; i++)
        {
            DBField oF = new DBField();
            oF.Name = sNames[i];
            oF.IsNull = getIsNull(strTable, sNames[i]);
            oF.Length = getFieldLength(strTable, sNames[i]);
            oF.Type = getFieldType(strTable, sNames[i]);
            oF.PrimaryKey = getFieldPrimaryKey(strTable, sNames[i]);
            oF.IsIdentity = getFieldIsIdentity(strTable, sNames[i]);
            oCC.Add(sNames[i], oF);
        }
        return oCC;
    }

    //
    /*
    public cCollection getFieldsValue(string strTable)
    {
        return oReg.getKeys(GroupTables,strTable);
    }
    */
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Collection getStoreProcedures()
    {
        Collection oCC = new Collection();
        string[] sNames = GetX(FIRSTNODEXML + "/" + PROCEDURES + "/" + PROCEDURE + "/@name");
        for (int i = 0; sNames != null && i < sNames.Length; i++)
        {
            string sProc = getStoreProcedure(sNames[i]);
            oCC.Add(sNames[i], sProc);
        }
        return oCC;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sProcedureName"></param>
    /// <returns></returns>
    public string getStoreProcedure(string sProcedureName)
    {
        return GetX(FIRSTNODEXML + "/" + PROCEDURES + "/" + PROCEDURE + "[@name='" + sProcedureName + "']")[0];
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strTable"></param>
    /// <param name="strField"></param>
    /// <returns></returns>
    public string getFieldType(string strTable, string strField)
    {
        string[] sNames = null;
        sNames = GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@alias='" + strTable + "']/" + COLUMN + "[@name='" + strField + "']/@type");
        sNames ??= GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "' and (not(@alias) or @alias='')]/" + COLUMN + "[@name='" + strField + "']/@type");
        if (sNames == null)
            return null;
        return sNames[0];
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strTable"></param>
    /// <param name="strField"></param>
    /// <returns></returns>
    public int getFieldLength(string strTable, string strField)
    {
        string[] sNames = null;
        sNames = GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@alias='" + strTable + "']/" + COLUMN + "[@name='" + strField + "']/@len");
        sNames ??= GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "' and (not(@alias) or @alias='')]/" + COLUMN + "[@name='" + strField + "']/@len");
        if (sNames == null)
            return -1;
        return Convert.ToInt32(sNames[0]);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strTable"></param>
    /// <param name="strField"></param>
    /// <returns></returns>
    public bool getIsNull(string strTable, string strField)
    {
        string[] sNames = null;
        sNames = GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@alias='" + strTable + "']/" + COLUMN + "[@name='" + strField + "']/@isnull");
        sNames ??= GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "' and (not(@alias) or @alias='')]/" + COLUMN + "[@name='" + strField + "']/@isnull");
        if (sNames == null)
            return false;
        return Convert.ToBoolean(sNames[0]);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strTable"></param>
    /// <returns></returns>
    public Collection getFieldsPrimaryKey(string strTable)
    {
        Collection oCC = new Collection();
        string[] sValues = null;
        sValues = GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@alias='" + strTable + "']/" + COLUMN + "[@primarykey='True']/@name");
        sValues ??= GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "' and (not(@alias) or @alias='')]/" + COLUMN + "[@primarykey='True']/@name");
        if (sValues == null)
            return null;
        for (int i = 0; i < sValues.Length; i++)
        {
            oCC.Add(sValues[i], getField(strTable, sValues[i]));
        }
        return oCC;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strTable"></param>
    /// <param name="strField"></param>
    /// <returns></returns>
    public bool getFieldPrimaryKey(string strTable, string strField)
    {
        string[] sValues = null;
        sValues = GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@alias='" + strTable + "']/" + COLUMN + "[@primarykey='True']/@primarykey");
        if (sValues == null)
            GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "' and (not(@alias) or @alias='')]/" + COLUMN + "[@primarykey='True']/@primarykey");
        return sValues == null ? false : Convert.ToBoolean(sValues[0]);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strTable"></param>
    /// <param name="strField"></param>
    /// <returns></returns>
    public string getFieldDefaultValue(string strTable, string strField)
    {
        string[] sValues = null;
        sValues = GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@alias='" + strTable + "']/" + COLUMN + "[@name='" + strField + "']/@default");
        if (sValues == null)
            GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "' and (not(@alias) or @alias='')]/" + COLUMN + "[@name='" + strField + "']/@default");
        return sValues == null ? "" : sValues[0];
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strTable"></param>
    /// <returns></returns>
    public Collection getFieldsDefaultValues(string strTable)
    {
        string[] oFields = GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@default!='']/@name");
        string[] oValues = GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@default!='']/@default");
        Collection oCC = new Collection();
        if (oFields != null)
            oCC.AddRange(oFields, oValues);
        return oCC;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strTable"></param>
    /// <returns></returns>
    public Collection getFieldsNoDefaultValues(string strTable)
    {
        string[] oFields = GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@default='']/@name");
        string[] oValues = GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@default='']/@default");
        Collection oCC = new Collection();
        if (oFields != null)
            oCC.AddRange(oFields, oValues);
        return oCC;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strTable"></param>
    /// <param name="strField"></param>
    /// <returns></returns>
    public bool getFieldIsIdentity(string strTable, string strField)
    {
        if (GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@name='" + strField + "']/@isidentity") != null)
            return Convert.ToBoolean(GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@name='" + strField + "']/@isidentity")[0]);
        else
            return false;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strTable"></param>
    /// <param name="strField"></param>
    /// <returns></returns>
    public string getFieldIdentityNumber(string strTable, string strField)
    {
        string sResult = "";
        string iIDStart = GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@name='" + strField + "']/@identitystart")[0];
        string iIDStep = GetX(FIRSTNODEXML + "/" + TABLES + "/" + TABLE + "[@name='" + strTable + "']/" + COLUMN + "[@name='" + strField + "']/@identitystep")[0];
        if (iIDStart != null && iIDStep != null)
            sResult = "{" + iIDStart + ", " + iIDStep + "}";
        return sResult;
    }
}

