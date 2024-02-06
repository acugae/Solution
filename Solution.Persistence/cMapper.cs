namespace Solution.Persistence;
/// <summary>
/// Questa classe gestisce il meccanismo di persistenza tra XML e database.
/// </summary>
public class cMapper
{
    private cDataManager _oData = null;
    private cCollection _oTypes = new cCollection();
    private cCollection _ocTables = new cCollection();

    /// <summary>
    /// Crea l'istanza del Mapper, carica le informazioni di persistenza dai file inclusi nel file di configurazione.
    /// </summary>
    /// <param name="oDataManager"></param>
    public cMapper(cDataManager oDataManager)
    {
        _oData = oDataManager;
        for (int i = 0; oDataManager.Inclusions != null && i < oDataManager.Inclusions.Count; i++)
        {
            this.Load(oDataManager.Inclusions.GetKey(i));
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sTableName"></param>
    /// <param name="oTable"></param>
    public void SetTable(string sTableName, cTable oTable)
    {
        if (!_ocTables.ContainsKey(sTableName))
        {
            _ocTables.Add(sTableName, oTable);
        }
        else
        {
            _ocTables[sTableName] = oTable;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sTableName"></param>
    public void DelTable(string sTableName)
    {
        if (_ocTables.ContainsKey(sTableName))
        {
            _ocTables.Remove(sTableName);
        }
    }

    /// <summary>
    /// Carica la mappatura delle tabelle sui tipi di dato.
    /// </summary>
    /// <param name="sFile">File di configurazione.</param>
    public void Load(string sFile)
    {
        XMLDBRead oRegSchema = new XMLDBRead();
        sFile = cFileManager.GetPathRoot(cFileManager.NormalizePath(sFile));
        oRegSchema.Load(sFile);
        Load(oRegSchema);
    }
    /// <summary>
    /// Carica la mappatura delle tabelle sui tipi di dato.
    /// </summary>
    /// <param name="oXMLManager">Manager del file di configurazione.</param>
    public void Load(XMLDBRead oXMLManager)
    {
        //
        cGCollection<string, string> oTables = oXMLManager.getTables();
        for (int j = 0; j < oTables.Count; j++)
        {
            string sAlias = (string)oTables.GetKey(j);
            string sTable = (oTables.GetValue(j) == null ? (string)oTables.GetKey(j) : (string)oTables.GetValue(j));
            cGCollection<string, cDBField> oFields = oXMLManager.getFields(sAlias);
            cGCollection<string, cDBField> oFieldsKey = oXMLManager.getFieldsKey(sAlias);
            this.SetTable(sAlias, new cTable(_oData, sTable, oFields, oFieldsKey));
        }
        //
        // Mapper
        //
        string[] sMaps = oXMLManager.getMapTypes();
        for (int i = 0; sMaps != null && i < sMaps.Length; i++)
        {
            string[] sTables = oXMLManager.getMapTables(sMaps[i]);
            for (int j = 0; sTables != null && j < sTables.Length; j++)
            {
                string[] sName = oXMLManager.getMapProperty(sMaps[i]);
                string[] sColumnName = oXMLManager.getMapColumn(sMaps[i]);
                cCollection oCC = new cCollection();
                oCC.AddRange(sColumnName, sName);
                this.SetMapperType(sMaps[i], sTables[j], oCC.GetStringFromCollection());
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sClassNameObject"></param>
    /// <param name="sTableName"></param>
    /// <param name="strNameMatchProperty">[nomecampodb]=[nomeproperty]&[nomecampodb]=[nomeproperty]&[nomecampodb]=[nomeproperty]</param>
    public void SetMapperType(string sClassNameObject, string sTableName, string strNameMatchProperty)
    {
        if (!_ocTables.ContainsKey(sTableName))
            throw (new Exception());
        //
        cTable oTable = (cTable)_ocTables[sTableName];
        oTable.SetType(sClassNameObject, strNameMatchProperty);
        //
        if (_oTypes.ContainsKey(sClassNameObject))
        {
            cCollection oMatch = (cCollection)_oTypes[sClassNameObject];
            cCollection oProperies = new cCollection();
            oProperies.SetCollectionFromString(strNameMatchProperty);
            if (oMatch.ContainsKey(oTable))
                oMatch[oTable] = oProperies;
            else
                oMatch.Add(oTable, oProperies);
            _oTypes[sClassNameObject] = oMatch;
        }
        else
        {
            cCollection oMatch = new cCollection();
            cCollection oProperies = new cCollection();
            oProperies.SetCollectionFromString(strNameMatchProperty);
            oMatch.Add(oTable, oProperies);
            _oTypes.Add(sClassNameObject, oMatch);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sClassNameObject"></param>
    /// <param name="oTable"></param>
    public void DelMapperType(string sClassNameObject, cTable oTable)
    {
        if (_oTypes.ContainsKey(sClassNameObject))
        {
            cCollection oMatch = (cCollection)_oTypes[sClassNameObject];
            if (oMatch.ContainsKey(oTable))
            {
                oMatch.Remove(oTable);
            }
            _oTypes[sClassNameObject] = oMatch;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sClassNameObject"></param>
    public void DelMapperType(string sClassNameObject)
    {
        if (_oTypes.ContainsKey(sClassNameObject))
        {
            _oTypes.Remove(sClassNameObject);
        }
    }
    /// <summary>
    /// Filtra le inforazioni con le proprietà dell'oggetto specificato per effettuare un filtro sulla tabella mappata.
    /// </summary>
    /// <param name="sKeyConnection">Chiave di connessione</param>
    /// <param name="oObject">Oggetto </param>
    /// <returns>Lista di oggetti di tipo identico al tipo dell'oggetto specificato.</returns>
    public object[] Get(string sKeyConnection, object oObject)
    {
        if (!this._oTypes.ContainsKey(oObject.GetType().ToString()))
            throw (new Exception("Object type not supported"));
        //
        cTable oT = (cTable)((cCollection)_oTypes[oObject.GetType().ToString()]).GetKey(0);
        return oT.GetObject(sKeyConnection, oObject);
    }
    /// <summary>
    /// Filtra le inforazioni con le proprietà dell'oggetto specificato per effettuare un filtro sulla tabella mappata.
    /// </summary>
    /// <param name="sKeyConnection">Chiave di connessione</param>
    /// <param name="oObject"></param>
    /// <returns>Ritorna un DataTable.</returns>
    public DataTable GetDataTable(string sKeyConnection, object oObject)
    {
        if (!this._oTypes.ContainsKey(oObject.GetType().ToString()))
            throw (new Exception("Object type not supported"));
        //
        cTable oT = (cTable)((cCollection)_oTypes[oObject.GetType().ToString()]).GetKey(0);
        return oT.GetDataTable(sKeyConnection, oObject);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection">Chiave di connessione</param>
    /// <param name="oObject"></param>
    /// <returns></returns>
    public object GetFirst(string sKeyConnection, object oObject)
    {
        object[] oResult = Get(sKeyConnection, oObject);
        if (oResult == null || oResult.Length == 0)
            return null;
        return oResult[0];
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="oObject"></param>
    public void Set(string sKeyConnection, object oObject)
    {
        if (!this._oTypes.ContainsKey(oObject.GetType().ToString()))
            throw (new Exception("Object type not supported"));
        //
        cReflectionManager oR = new cReflectionManager();
        cCollection oTables = (cCollection)_oTypes[oObject.GetType().ToString()];
        //
        for (int j = 0; j < oTables.Count; j++)
        {
            cTable oTable = (cTable)oTables.GetKey(j);
            cCollection oMatch = (cCollection)oTables.GetValue(j);
            ArrayList oArrValues = new ArrayList();
            for (int i = 0; i < oTable.Fields.Count; i++)
            {
                if (oMatch.ContainsKey(oTable.Fields.GetKey(i)))
                    oArrValues.Add(oR.CallPropertyGet(oObject, (string)oMatch[oTable.Fields.GetKey(i)]));
                else
                    oArrValues.Add(null);
            }
            oTable.Set(sKeyConnection, oArrValues.ToArray());
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="oObject"></param>
    public void Del(string sKeyConnection, object oObject)
    {
        if (!this._oTypes.ContainsKey(oObject.GetType().ToString()))
            throw (new Exception("Object type not supported"));
        //
        cReflectionManager oR = new cReflectionManager();
        cCollection oTables = (cCollection)_oTypes[oObject.GetType().ToString()];
        //
        for (int j = 0; j < oTables.Count; j++)
        {
            cTable oTable = (cTable)oTables.GetKey(j);
            cCollection oMatch = (cCollection)oTables.GetValue(j);
            ArrayList oArrValues = new ArrayList();
            for (int i = 0; i < oTable.Fields.Count; i++)
            {
                if (oMatch.ContainsKey(oTable.Fields.GetKey(i)))
                    oArrValues.Add(oR.CallPropertyGet(oObject, (string)oMatch[oTable.Fields.GetKey(i)]));
                else
                    oArrValues.Add(null);
            }
            object[] oObjects = oTable.GetObject(sKeyConnection, oObject);
            for (int r = 0; oObjects != null && r < oObjects.Length; r++)
            {
                oTable.DelObject(sKeyConnection, oObjects[r]);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public cCollection Tables
    {
        get
        {
            return _ocTables;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sTableName"></param>
    /// <returns></returns>
    public cTable this[string sTableName]
    {
        get
        {
            if (_ocTables.ContainsKey(sTableName))
                return (cTable)_ocTables[sTableName];
            return null;
        }
    }
}
