namespace Solution.Persistence;
/// <summary>
/// Questa classe gestisce comandi SQL.
/// </summary>
public class cCommander : cGCollection<string, string>
{
    private cDataManager _oData = null;
    //private cGCollection<string, string> _ocSQL = new cGCollection<string, string>();
    private cGCollection<string, cGCollection<string, object>> _ocParams = new cGCollection<string, cGCollection<string, object>>();

    /// <summary>
    /// Crea l'istanza del Commander, carica le informazioni di persistenza dai file inclusi nel file di configurazione.
    /// </summary>
    /// <param name="oDataManager"></param>
    public cCommander(cDataManager oDataManager)
    {
        _oData = oDataManager;
        for (int i = 0; oDataManager.Inclusions != null && i < oDataManager.Inclusions.Count; i++)
        {
            this.Load(oDataManager.Inclusions.GetKey(i));
        }
    }
    //
    /// <summary>
    /// Carica le istruzioni SQL.
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
    /// Carica le istruzioni SQL.
    /// </summary>
    /// <param name="oXML"></param>
    public void Load(cXMLManager oXML)
    {
        XMLDBRead oRegSchema = new XMLDBRead(); //(oXML as XMLDBRead);
        oRegSchema.Load(oXML);
        Load(oRegSchema);
    }
    /// <summary>
    /// Carica le istruzioni SQL.
    /// </summary>
    /// <param name="oXMLManager">Manager del file di configurazione.</param>
    public void Load(XMLDBRead oXMLManager)
    {
        string[] sCommands = oXMLManager.getCommands();
        for (int i = 0; sCommands != null && i < sCommands.Length; i++)
        {
            string[] sSQLs = oXMLManager.getCommand(sCommands[i]);
            if (sSQLs.Length > 0)
            {
                sSQLs[0] = sSQLs[0].Replace("\n", "");
                sSQLs[0] = sSQLs[0].Replace("\r", "");
                sSQLs[0] = sSQLs[0].Replace("\t", "").Trim();
                this.SetSQL(sCommands[i], sSQLs[0]);
            }
        }
    }
    private cGCollection<string, object> GetParams(string sSQL)
    {
        cGCollection<string, object> oResult = new cGCollection<string, object>();
        string[] sParams = sSQL.Split('#');
        for (int i = 0; sParams != null && i < sParams.Length; i++)
        {
            if (i % 2 == 1)
                oResult.Set("#" + sParams[i] + "#", null);
        }
        return oResult;
    }
    /// <summary>
    /// Imposta l'istruzione.
    /// </summary>
    /// <param name="sSQLName">Nome dell'istruzione</param>
    /// <param name="sSQL">Istruzione</param>
    public void SetSQL(string sSQLName, string sSQL)
    {
        Set(sSQLName, sSQL);
        _ocParams.Set(sSQLName, GetParams(sSQL));
    }
    /// <summary>
    /// Ritorna i parametri dei comandi.
    /// </summary>
    public cGCollection<string, cGCollection<string, object>> Params
    {
        get { return _ocParams; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sSQLName"></param>
    public void DelSQL(string sSQLName)
    {
        if (ContainsKey(sSQLName))
        {
            Remove(sSQLName);
            _ocParams.Remove(sSQLName);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKey"></param>
    /// <param name="sSQLName"></param>
    /// <returns></returns>
    public DataTable Execute(string sKey, string sSQLName)
    {
        if (!ContainsKey(sSQLName))
            throw (new Exception("SQL name not exsist"));
        //
        DataSet oDS = this._oData.GetDS(sKey, this[sSQLName]);
        if (oDS == null)
            return null;
        if (oDS.Tables.Count == 0)
            return null;
        return oDS.Tables[0];
    }

    public DataTable Execute(cConnection oConn, string sSQLName)
    {
        if (!ContainsKey(sSQLName))
            throw (new Exception("SQL name not exsist"));
        //
        DataSet oDS = this._oData.GetDS(oConn, this[sSQLName]);
        if (oDS == null)
            return null;
        if (oDS.Tables.Count == 0)
            return null;
        return oDS.Tables[0];
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKey"></param>
    /// <param name="sSQLName"></param>
    /// <param name="oParams"></param>
    /// <returns></returns>
    public DataTable Execute(string sKey, string sSQLName, cGCollection<string, string> oParams)
    {
        if (!ContainsKey(sSQLName))
            throw (new Exception("SQL name not exsist"));
        //
        string sSQL = (string)this[sSQLName];
        //
        for (int i = 0; i < oParams.Count; i++)
        {
            sSQL = sSQL.Replace(oParams.GetKey(i), oParams.GetValue(i));
        }
        //
        DataSet oDS = this._oData.GetDS(sKey, sSQL);
        if (oDS == null)
            return null;
        if (oDS.Tables.Count == 0)
            return null;
        return oDS.Tables[0];
    }

    public DataTable Execute(cConnection oConn, string sSQLName, cGCollection<string, string> oParams)
    {
        if (!ContainsKey(sSQLName))
            throw (new Exception("SQL name not exsist"));
        //
        string sSQL = (string)this[sSQLName];
        //
        for (int i = 0; i < oParams.Count; i++)
        {
            sSQL = sSQL.Replace(oParams.GetKey(i), oParams.GetValue(i));
        }
        //
        DataSet oDS = this._oData.GetDS(oConn, sSQL);
        if (oDS == null)
            return null;
        if (oDS.Tables.Count == 0)
            return null;
        return oDS.Tables[0];
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKey"></param>
    /// <param name="sSQLName"></param>
    /// <param name="oParams"></param>
    /// <returns></returns>
    public DataTable Execute(string sKey, string sSQLName, params string[] oParams)
    {
        if (!ContainsKey(sSQLName))
            throw (new Exception("SQL name not exsist"));
        //
        string sSQL = (string)this[sSQLName];
        cGCollection<string, object> oAPrams = _ocParams[sSQLName];
        //
        for (int i = 0; i < oAPrams.Count; i++)
        {
            sSQL = sSQL.Replace(oAPrams.GetKey(i), oParams[i]);
        }
        //
        DataSet oDS = this._oData.GetDS(sKey, sSQL);
        if (oDS == null)
            return null;
        if (oDS.Tables.Count == 0)
            return null;
        return oDS.Tables[0];
    }

    public DataTable Execute(cConnection oConn, string sSQLName, params string[] oParams)
    {
        if (!ContainsKey(sSQLName))
            throw (new Exception("SQL name not exsist"));
        //
        string sSQL = (string)this[sSQLName];
        cGCollection<string, object> oAPrams = _ocParams[sSQLName];
        //
        for (int i = 0; i < oAPrams.Count; i++)
        {
            sSQL = sSQL.Replace(oAPrams.GetKey(i), oParams[i]);
        }
        //
        DataSet oDS = this._oData.GetDS(oConn, sSQL);
        if (oDS == null)
            return null;
        if (oDS.Tables.Count == 0)
            return null;
        return oDS.Tables[0];
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKey"></param>
    /// <param name="sSQLName"></param>
    /// <param name="oObject"></param>
    /// <returns></returns>
    public DataTable Execute(string sKey, string sSQLName, object oObject)
    {
        if (!ContainsKey(sSQLName))
            throw (new Exception("SQL name not exsist"));
        //
        cReflectionManager oR = new cReflectionManager();
        string sSQL = (string)this[sSQLName];
        //
        cGCollection<string, PropertyInfo> oPropetry = oR.GetProperties(oObject, BindingFlags.Public | BindingFlags.Instance);
        for (int i = 0; i < oPropetry.Count; i++)
        {
            sSQL = sSQL.Replace("#" + oPropetry.GetKey(i) + "#", oR.CallPropertyGet(oObject, oPropetry.GetKey(i)).ToString());
        }
        //
        DataSet oDS = this._oData.GetDS(sKey, sSQL);
        if (oDS == null)
            return null;
        if (oDS.Tables.Count == 0)
            return null;
        return oDS.Tables[0];
    }

    public DataTable Execute(cConnection oConn, string sSQLName, object oObject)
    {
        if (!ContainsKey(sSQLName))
            throw (new Exception("SQL name not exsist"));
        //
        cReflectionManager oR = new cReflectionManager();
        string sSQL = (string)this[sSQLName];
        //
        cGCollection<string, PropertyInfo> oPropetry = oR.GetProperties(oObject, BindingFlags.Public | BindingFlags.Instance);
        for (int i = 0; i < oPropetry.Count; i++)
        {
            sSQL = sSQL.Replace("#" + oPropetry.GetKey(i) + "#", oR.CallPropertyGet(oObject, oPropetry.GetKey(i)).ToString());
        }
        //
        DataSet oDS = this._oData.GetDS(oConn, sSQL);
        if (oDS == null)
            return null;
        if (oDS.Tables.Count == 0)
            return null;
        return oDS.Tables[0];
    }
}
