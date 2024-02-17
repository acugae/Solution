namespace Solution.Data;

public enum enSQLFIELD { String, Date, Numeric, NumericNull, Bool, None };
/// <summary>
/// Gestisce il Data Layer.
/// </summary>
public class DataManager : MarshalByRefObject
{
    protected Providers _oProviders = new Providers();
    protected Connections _oCns = null;
    protected Transactions _oTrs = null;
    protected GCollection<string, string> _Inclusions = new GCollection<string, string>();

    protected int _iTimeoutCommand = -1;

    //public override object InitializeLifetimeService()
    //{
    //	return null;
    //}
    /// <summary>
    /// Costruttore di default della classe.
    /// </summary>
    public DataManager()
    {
        lock (this)
        {
            try
            {
                _oCns = new Connections(_oProviders);
                _oTrs = new Transactions(_oCns);
            }
            catch (Exception e)
            {
                throw (e);
            }
        }
    }
    /// <summary>
    /// Restituisce le connessioni.
    /// </summary>
    public Connections Connections
    {
        get { return _oCns; }
    }
    /// <summary>
    /// Imposta il timeout dei comandi.
    /// </summary>
    public int TimeoutCommand
    {
        set { _iTimeoutCommand = value; }
        get { return _iTimeoutCommand; }
    }
    /// <summary>
    /// Restituisce i providers.
    /// </summary>
    public Providers Providers
    {
        get { return _oProviders; }
    }
    /// <summary>
    /// Restituisce le transezioni.
    /// </summary>
    public Transactions Transactions
    {
        get { return _oTrs; }
    }
    /// <summary>
    /// Restituisce le inclusioni.
    /// </summary>
    public GCollection<string, string> Inclusions
    {
        get { return _Inclusions; }
    }

    // strParams = "[nomecampo]=[valuecampo]&[nomecampo]=[valuecampo]&[nomecampo]=[valuecampo]"
    private string[] ParseStringParmas(string strParams, int type)
    {
        lock (this)
        {
            int numCampi = 0;
            string[] tmpStrings = null;
            string[] _strName = null;
            //
            // Caricamento Campi Chiave
            if (strParams.Length > 0)
            {
                tmpStrings = strParams.Split('&');
                numCampi = tmpStrings.Length;
                _strName = new string[numCampi];
                for (int i = 0; i < numCampi; i++)
                {
                    _strName[i] = tmpStrings[i].Split('=')[type];
                }
            }
            return _strName;
        }
    }
    /// <summary>
    /// Esegue l'struzione SQL e restituisce il contenuto in un cDataReader. 
    /// </summary>
    /// <param name="sKeyConnection">Chiave di connessione</param>
    /// <param name="strSQL">Stringa SQL</param>
    /// <param name="strParams">Array di pametri di sostituzione nella forma (nomecampo=tipocampo&amp;nomecampo=tipocampo)</param>
    public DataReader ExecuteReader(string sKeyConnection, string strSQL, string strParams)
    {
        lock (this)
        {
            DataReader oDRE = null;

            Command oCMD = new Command(_oCns[sKeyConnection], strSQL);
            if (_iTimeoutCommand >= 0)
                oCMD.CommandTimeout = _iTimeoutCommand;
            oCMD.Parameters.AddParameterString(strParams);
            oDRE = new DataReader(oCMD.ExecuteReader());

            return oDRE;
        }
    }

    public DataReader ExecuteReader(Connection oConnection, string strSQL, string strParams)
    {
        lock (this)
        {
            DataReader oDRE = null;
            try
            {
                Command oCMD = new Command(oConnection, strSQL);
                if (_iTimeoutCommand >= 0)
                    oCMD.CommandTimeout = _iTimeoutCommand;
                oCMD.Parameters.AddParameterString(strParams);
                oDRE = new DataReader(oCMD.ExecuteReader());
            }
            catch (Exception e)
            {
                throw (e);
            }
            return oDRE;
        }
    }
    /// <summary>
    /// Esegue l'struzione SQL e restituisce il contenuto in uno scalare. 
    /// </summary>
    /// <param name="sKeyConnection">Chiave che identifica la connesione da usare</param>
    /// <param name="strSQL">Stringa sql da eseguire</param>
    /// <param name="strParams">Array di pametri di sostituzione nella forma (nomecampo=tipocampo&amp;nomecampo=tipocampo)</param>
    public object ExecuteScalar(string sKeyConnection, string strSQL, string strParams)
    {
        lock (this)
        {
            object oObject = null;
            try
            {
                Command oCMD = new Command(_oCns[sKeyConnection], strSQL);
                if (_iTimeoutCommand >= 0)
                    oCMD.CommandTimeout = _iTimeoutCommand;
                oCMD.Parameters.AddParameterString(strParams);
                oObject = oCMD.ExecuteScalar();
            }
            catch (Exception e)
            {
                throw e;
            }
            return oObject;
        }
    }

    public object ExecuteScalar(Connection oConnection, string strSQL, string strParams)
    {
        lock (this)
        {
            object oObject = null;
            try
            {
                Command oCMD = new Command(oConnection, strSQL);
                if (_iTimeoutCommand >= 0)
                    oCMD.CommandTimeout = _iTimeoutCommand;
                oCMD.Parameters.AddParameterString(strParams);
                oObject = oCMD.ExecuteScalar();
            }
            catch (Exception e)
            {
                throw e;
            }
            return oObject;
        }
    }
    /// <summary>
    /// Esegue una stringa SQL, viene utilizzato per istruzioni quali: UPDATE, INSERT e DELETE.
    /// </summary>
    /// <param name="sKeyConnection">Chiave che identifica la connesione da usare</param>
    /// <param name="strSQL">Stringa sql da eseguire</param>
    /// <param name="strParams">Array di pametri di sostituzione nella forma (nomecampo=tipocampo&amp;nomecampo=tipocampo)</param>
    public int ExecuteNonQuery(string sKeyConnection, string strSQL, string strParams)
    {
        lock (this)
        {

            Command oCMD = new Command(_oCns[sKeyConnection], strSQL);
            if (_iTimeoutCommand >= 0)
                oCMD.CommandTimeout = _iTimeoutCommand;
            oCMD.Parameters.AddParameterString(strParams);
            return oCMD.ExecuteNonQuery();
        }
    }

    public int ExecuteNonQuery(Connection oConnection, string strSQL, string strParams)
    {
        lock (this)
        {
            Command oCMD = new Command(oConnection, strSQL);
            if (_iTimeoutCommand >= 0)
                oCMD.CommandTimeout = _iTimeoutCommand;
            oCMD.Parameters.AddParameterString(strParams);
            return oCMD.ExecuteNonQuery();
        }
    }

    protected string DBFieldToString(object oField)
    {
        lock (this)
        {
            if (oField.GetType() == System.DBNull.Value.GetType())
            {
                return " null ";
            }
            else
            {
                if (oField.GetType() == DbType.AnsiString.GetType() ||
                    oField.GetType() == DbType.AnsiStringFixedLength.GetType() ||
                    oField.GetType() == DbType.String.GetType() ||
                    oField.GetType() == DbType.StringFixedLength.GetType() ||
                    oField.GetType() == System.Type.GetType("System.String")
                    )
                {
                    return FieldToString(oField, enSQLFIELD.String);
                }
                else
                {
                    if (oField.GetType() == DbType.Boolean.GetType() ||
                        oField.GetType() == DbType.Byte.GetType() ||
                        oField.GetType() == DbType.Currency.GetType() ||
                        oField.GetType() == DbType.Decimal.GetType() ||
                        oField.GetType() == DbType.Double.GetType() ||
                        oField.GetType() == DbType.Int16.GetType() ||
                        oField.GetType() == DbType.Int32.GetType() ||
                        oField.GetType() == DbType.Int64.GetType() ||
                        oField.GetType() == DbType.SByte.GetType() ||
                        oField.GetType() == DbType.Single.GetType() ||
                        oField.GetType() == DbType.UInt16.GetType() ||
                        oField.GetType() == DbType.UInt32.GetType() ||
                        oField.GetType() == DbType.UInt64.GetType() ||
                        oField.GetType() == DbType.VarNumeric.GetType() ||
                        oField.GetType() == System.Type.GetType("System.Int16") ||
                        oField.GetType() == System.Type.GetType("System.Int32") ||
                        oField.GetType() == System.Type.GetType("System.Int64") ||
                        oField.GetType() == System.Type.GetType("System.Double") ||
                        oField.GetType() == System.Type.GetType("System.Single") ||
                        oField.GetType() == System.Type.GetType("System.Byte")
                        )
                    {
                        return FieldToString(oField, enSQLFIELD.NumericNull);
                    }
                    else
                    {
                        if (oField.GetType() == DbType.Date.GetType() ||
                            oField.GetType() == DbType.DateTime.GetType() ||
                            oField.GetType() == System.Type.GetType("System.DateTime")
                            )
                        {
                            return FieldToString(oField, enSQLFIELD.Date);
                        }
                        else
                            return oField.ToString();
                    }
                }
            }
        }
    }
    /// <summary>
    /// Viene utilizzata per creare un cCommand sulla connessione specificata.
    /// </summary>
    /// <param name="sKeyConnection">Chiave della connessione</param>
    /// <returns></returns>
    public Command GetCM(string sKeyConnection)
    {
        lock (this)
        {
            try
            {
                Command oCMD = new Command(_oCns[sKeyConnection]);
                if (_iTimeoutCommand >= 0)
                    oCMD.CommandTimeout = _iTimeoutCommand;
                return oCMD;
                //return new cCommand(_oCns[sKeyConnection]);
            }
            catch (Exception e)
            {
                if (_oCns[sKeyConnection].State == ConnectionState.Open) _oCns[sKeyConnection].Close();
                throw (e);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oConnection"></param>
    /// <returns></returns>
    public Command GetCM(Connection oConnection)
    {
        lock (this)
        {
            try
            {
                Command oCMD = new Command(oConnection);
                if (_iTimeoutCommand >= 0)
                    oCMD.CommandTimeout = _iTimeoutCommand;
                return oCMD;
                //return new cCommand(oConnection);
            }
            catch (Exception e)
            {
                //if (_oCns[sKeyConnection].State == ConnectionState.Open) _oCns[sKeyConnection].Close();
                throw (e);
            }
        }
    }
    /// <summary>
    /// Viene utilizzata per creare un cCommand sulla connessione specificata.
    /// </summary>
    /// <param name="sKeyConnection">Chiave della connessione</param>
    /// <param name="strSQL">Stringa SQL</param>
    /// <returns></returns>
    public Command GetCM(string sKeyConnection, string strSQL)
    {
        lock (this)
        {
            try
            {
                Command oCMD = new Command(_oCns[sKeyConnection], strSQL);
                if (_iTimeoutCommand >= 0)
                    oCMD.CommandTimeout = _iTimeoutCommand;
                return oCMD;
            }
            catch (Exception e)
            {
                //if (_oCns[sKeyConnection].State == ConnectionState.Open) _oCns[sKeyConnection].Close();
                throw (e);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oConnection"></param>
    /// <param name="strSQL"></param>
    /// <returns></returns>
    public Command GetCM(Connection oConnection, string strSQL)
    {
        lock (this)
        {
            try
            {
                Command oCMD = new Command(oConnection, strSQL);
                if (_iTimeoutCommand >= 0)
                    oCMD.CommandTimeout = _iTimeoutCommand;
                return oCMD;
                //return new cCommand(oConnection, strSQL);
            }
            catch (Exception e)
            {
                //if (_oCns[sKeyConnection].State == ConnectionState.Open) _oCns[sKeyConnection].Close();
                throw (e);
            }
        }
    }
    /// <summary>
    /// Viene utilizzata per creare un cDataAdapter sulla connessione specificata.
    /// </summary>
    /// <param name="sKeyConnection">Chiave di connessione</param>
    /// <param name="strSQL">stringa SQL</param>
    public Provider.DataAdapter GetDA(string sKeyConnection, string strSQL)
    {
        lock (this)
        {
            try
            {
                return new Provider.DataAdapter(GetCM(sKeyConnection, strSQL));
            }
            catch (Exception e)
            {
                throw (e);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="strSQL"></param>
    /// <returns></returns>
    public Provider.DataAdapter GetDA(Connection oConnection, string strSQL)
    {
        lock (this)
        {
            try
            {
                return new Provider.DataAdapter(GetCM(oConnection, strSQL));
            }
            catch (Exception e)
            {
                throw (e);
            }
        }
    }
    /// <summary>
    /// Viene utilizzata per creare un cDataAdapter sulla connessione specificata.
    /// </summary>
    /// <param name="sKeyConnection">Chiave di connessione</param>
    public Provider.DataAdapter GetDA(string sKeyConnection)
    {
        lock (this)
        {
            return new Provider.DataAdapter(GetCM(sKeyConnection));
        }
    }

    public Provider.DataAdapter GetDA(Connection oConnection)
    {
        lock (this)
        {
            return new Provider.DataAdapter(GetCM(oConnection));
        }
    }
    /// <summary>
    /// Restituisce la cConnection specificata.
    /// </summary>
    /// <param name="sKeyConnection">Chiave di connessione</param>
    public Connection GetCO(string sKeyConnection)
    {
        lock (this)
        {
            return _oCns[sKeyConnection];
        }
    }
    /// <summary>
    /// Resitituisce un DataSet contenente le informazioni selezionate con l'istruzione specificata.
    /// </summary>
    /// <param name="sKeyConnection">Chiave di connessione.</param>
    /// <param name="strSQL">Istruzione SQL.</param>
    public DataSet GetDS(string sKeyConnection, string strSQL)
    {
        lock (this)
        {
            if (_oCns.Contains(sKeyConnection))
            {
                DataSet odsTemp = new DataSet();
                GetDA(sKeyConnection, strSQL).Fill(odsTemp);
                return odsTemp;
            }
            else
                return null;
        }
    }

    public DataSet GetDS(Connection oConnection, string strSQL)
    {
        lock (this)
        {
            DataSet odsTemp = new DataSet();
            GetDA(oConnection, strSQL).Fill(odsTemp);
            return odsTemp;
        }
    }
    /// <summary>
    /// Resitituisce un DataTable contenente le informazioni selezionate con l'istruzione specificata.
    /// </summary>
    /// <param name="sKeyConnection">Chiave di connessione.</param>
    /// <param name="strSQL">Istruzione SQL.</param>
    public DataTable GetDT(string sKeyConnection, string strSQL)
    {
        lock (this)
        {
            DataSet oDS = GetDS(sKeyConnection, strSQL);
            if (oDS == null)
                return null;
            if (oDS.Tables.Count == 0)
                return null;
            return oDS.Tables[0];
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oConnection"></param>
    /// <param name="strSQL"></param>
    /// <returns></returns>
    public DataTable GetDT(Connection oConnection, string strSQL)
    {
        lock (this)
        {
            DataSet oDS = GetDS(oConnection, strSQL);
            if (oDS == null)
                return null;
            if (oDS.Tables.Count == 0)
                return null;
            return oDS.Tables[0];
        }
    }
    /// <summary>
    /// Resitituisce un DataSet contenente le informazioni selezionate con l'istruzione specificata. Il DataSet conterrà una tabela denominata con il valore di MappingTableName.
    /// </summary>
    /// <param name="sKeyConnection">Chiave di connessione.</param>
    /// <param name="strSQL">Istruzione SQL.</param>
    /// <param name="MappingTableName">Nome della tabella all'interno del DataSet.</param>
    public DataSet GetDS(string sKeyConnection, string strSQL, string MappingTableName)
    {
        lock (this)
        {
            if (_oCns.Contains(sKeyConnection))
            {
                DataSet odsTemp = new DataSet(MappingTableName);
                GetDA(sKeyConnection, strSQL).Fill(odsTemp, MappingTableName);
                return odsTemp;
            }
            else
                return null;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="strSQL"></param>
    /// <param name="MappingTableName"></param>
    /// <returns></returns>
    public DataSet GetDS(Connection oConnection, string strSQL, string MappingTableName)
    {
        lock (this)
        {
            DataSet odsTemp = new DataSet(MappingTableName);
            GetDA(oConnection, strSQL).Fill(odsTemp, MappingTableName);
            return odsTemp;
        }
    }
    /// <summary>
    /// Resitituisce un DataSet contenente le informazioni selezionate con l'istruzione specificata. Il DataSet conterrà una tabela denominata con il valore di MappingTableName.
    /// </summary>
    /// <param name="sKeyConnection">Chiave di connessione.</param>
    /// <param name="strSQL">Istruzione SQL.</param>
    /// <param name="MappingTableName">Nome della tabella all'interno del DataSet.</param>
    /// <param name="dsSource">DataSet a cui aggiungere la tabella.</param>
    public DataSet GetDS(string sKeyConnection, string strSQL, string MappingTableName, DataSet dsSource)
    {
        lock (this)
        {
            if (_oCns.Contains(sKeyConnection))
            {
                dsSource ??= new DataSet();
                GetDA(sKeyConnection, strSQL).Fill(dsSource, MappingTableName);
                return dsSource;
            }
            else
                return null;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oConnection"></param>
    /// <param name="strSQL"></param>
    /// <param name="MappingTableName"></param>
    /// <param name="dsSource"></param>
    /// <returns></returns>
    public DataSet GetDS(Connection oConnection, string strSQL, string MappingTableName, DataSet dsSource)
    {
        lock (this)
        {
            dsSource ??= new DataSet();
            GetDA(oConnection, strSQL).Fill(dsSource, MappingTableName);

            return dsSource;
        }
    }

    protected string FieldToString(object value, enSQLFIELD tField)
    {
        lock (this)
        {
            string cTMP = "";

            switch (tField)
            {
                case enSQLFIELD.Date:
                    if (value == null)
                    {
                        cTMP = "NULL";
                    }
                    else
                    {
                        cTMP = "#dsep#" + ((DateTime)value).ToString("dd/MM/yyyy") + "#dsep#";
                    }
                    break;

                case enSQLFIELD.None:
                    cTMP = value.ToString();
                    break;

                case enSQLFIELD.Numeric:
                    cTMP = value.ToString().Replace(",", ".");
                    break;

                case enSQLFIELD.NumericNull:
                    if (value == null)
                    {
                        cTMP = "NULL";
                    }
                    else
                    {
                        cTMP = value.ToString().Replace(",", ".");
                    }
                    break;

                case enSQLFIELD.String:
                    cTMP = "'" + ((string)value).Replace("'", "''") + "'";
                    break;

            }

            return cTMP;
        }
    }
    /// <summary>
    /// Invoca una Store Procedure.
    /// </summary>
    /// <param name="sKeyConnection">Chiave di connessione.</param>
    /// <param name="sNameStore">Nome della procedura.</param>
    /// <param name="ovNameFields">Lista di parametri da valorizzare.</param>
    /// <param name="ovValueFields">Lista di valori.</param>
    public object InvokeStore(string sKeyConnection, string sNameStore, string[] ovNameFields, object[] ovValueFields)
    {
        lock (this)
        {
            object oReturn = null;
            if (ovNameFields.Length != ovValueFields.Length)
                return null;

            Command oCM = new Command(_oCns[sKeyConnection], sNameStore);
            if (_iTimeoutCommand >= 0)
                oCM.CommandTimeout = _iTimeoutCommand;
            oCM.CommandType = CommandType.StoredProcedure;
            Parameters oPS = new Parameters(_oCns[sKeyConnection], oCM);
            for (int i = 0; i < ovNameFields.Length; i++)
            {
                oPS.Add(ovNameFields[i], ovValueFields[i]);
            }
            //
            Parameter oP = new Parameter(_oCns[sKeyConnection]);
            oP.ParameterName = "Return_Value";
            oP.Direction = ParameterDirection.ReturnValue;
            oP.DbType = DbType.Object;
            oPS.Add(oP);
            //
            oCM.ExecuteNonQuery();
            oReturn = oP.Value;

            return oReturn;
        }
    }

    public object InvokeStore(Connection oConnection, string sNameStore, string[] ovNameFields, object[] ovValueFields)
    {
        lock (this)
        {
            object oReturn = null;
            if (ovNameFields.Length != ovValueFields.Length)
                return null;
            Command oCM = new Command(oConnection, sNameStore);
            if (_iTimeoutCommand >= 0)
                oCM.CommandTimeout = _iTimeoutCommand;
            oCM.CommandType = CommandType.StoredProcedure;
            Parameters oPS = new Parameters(oConnection, oCM);
            for (int i = 0; i < ovNameFields.Length; i++)
            {
                oPS.Add(ovNameFields[i], ovValueFields[i]);
            }
            //
            Parameter oP = new Parameter(oConnection);
            oP.ParameterName = "Return_Value";
            oP.Direction = ParameterDirection.ReturnValue;
            oP.DbType = DbType.Object;
            oPS.Add(oP);
            //
            oCM.ExecuteNonQuery();
            oReturn = oP.Value;

            return oReturn;
        }
    }

    /// <summary>
    /// Invoca una Store Procedure.
    /// </summary>
    /// <param name="sKeyConnection">Chiave di connessione.</param>
    /// <param name="sNameStore">Nome della procedura.</param>
    /// <param name="oParams">Lista di parametri.</param>
    public DataTable InvokeStore(string sKeyConnection, string sNameStore, params Parameter[] oParams)
    {
        lock (this)
        {
            IDataReader oDataReader;
            DataTable _oDt = null;

            Command oCM = new Command(_oCns[sKeyConnection], sNameStore);
            if (_iTimeoutCommand >= 0)
                oCM.CommandTimeout = _iTimeoutCommand;
            oCM.CommandType = CommandType.StoredProcedure;
            Parameters oPS = new Parameters(_oCns[sKeyConnection], oCM);
            for (int i = 0; i < oParams.Length; i++)
            {
                Parameter oP = new Parameter(_oCns[sKeyConnection]);
                oP.DbType = oParams[i].DbType;
                oP.Direction = oParams[i].Direction;
                oP.ParameterName = oParams[i].ParameterName;
                oP.Value = oParams[i].Value;
                oPS.Add(oP);
            }
            //
            oDataReader = oCM.ExecuteReader();
            if (oDataReader != null)
            {
                _oDt = new DataTable();
                _oDt.Load(oDataReader);
                oDataReader.Close();
            }
            for (int i = 0; oCM.Parameters != null && i < oCM.Parameters.Count; i++)
                oParams[i].IDataParameter = oCM.Parameters[i] as IDataParameter;

            return _oDt;
        }
    }

    public DataTable Invoke(Connection oConnection, string sSQL, params Parameter[] oParams)
    {
        lock (this)
        {
            IDataReader oDataReader;
            DataTable _oDt = null;

            Command oCM = new Command(oConnection, sSQL);
            if (_iTimeoutCommand >= 0)
                oCM.CommandTimeout = _iTimeoutCommand;
            //oCM.CommandType = oCommandType;
            Parameters oPS = new Parameters(oConnection, oCM);
            for (int i = 0; i < oParams.Length; i++)
            {
                Parameter oP = new Parameter(oConnection);
                oP.DbType = oParams[i].DbType;
                oP.Direction = oParams[i].Direction;
                oP.ParameterName = oParams[i].ParameterName;
                oP.Value = oParams[i].Value;
                oPS.Add(oP);
            }
            //
            oDataReader = oCM.ExecuteReader();
            if (oDataReader != null)
            {
                try
                {
                    _oDt = new DataTable();
                    _oDt.Load(oDataReader);
                }
                catch
                { }
                finally
                {
                    oDataReader.Close();
                }
            }
            //
            for (int i = 0; oCM.Parameters != null && i < oCM.Parameters.Count; i++)
                oParams[i].IDataParameter = oCM.Parameters[i] as IDataParameter;

            return _oDt;
        }
    }


    public DataTable InvokeStore(Connection oConnection, string sNameStore, params Parameter[] oParams)
    {
        lock (this)
        {
            IDataReader oDataReader;
            DataTable _oDt = null;

            Command oCM = new Command(oConnection, sNameStore);
            if (_iTimeoutCommand >= 0)
                oCM.CommandTimeout = _iTimeoutCommand;
            oCM.CommandType = CommandType.StoredProcedure;
            Parameters oPS = new Parameters(oConnection, oCM);
            for (int i = 0; i < oParams.Length; i++)
            {
                Parameter oP = new Parameter(oConnection);
                oP.DbType = oParams[i].DbType;
                oP.Direction = oParams[i].Direction;
                oP.ParameterName = oParams[i].ParameterName;
                oP.Value = oParams[i].Value;
                oPS.Add(oP);
            }
            //
            oDataReader = oCM.ExecuteReader();
            if (oDataReader != null)
            {
                try
                {
                    _oDt = new DataTable();
                    _oDt.Load(oDataReader);
                }
                catch
                { }
                finally
                {
                    oDataReader.Close();
                }
            }
            //
            for (int i = 0; oCM.Parameters != null && i < oCM.Parameters.Count; i++)
                oParams[i].IDataParameter = oCM.Parameters[i] as IDataParameter;

            return _oDt;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="oDbType"></param>
    /// <param name="oDirection"></param>
    /// <param name="sName"></param>
    /// <param name="oValue"></param>
    /// <returns></returns>
    public Parameter CreateParameter(string sKeyConnection, System.Data.DbType oDbType, System.Data.ParameterDirection oDirection, string sName, object oValue)
    {
        return new Parameter(Connections[sKeyConnection], oDbType, oDirection, sName, oValue);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oConnection"></param>
    /// <param name="oDbType"></param>
    /// <param name="oDirection"></param>
    /// <param name="sName"></param>
    /// <param name="oValue"></param>
    /// <returns></returns>
    public Parameter CreateParameter(Connection oConnection, System.Data.DbType oDbType, System.Data.ParameterDirection oDirection, string sName, object oValue)
    {
        return new Parameter(oConnection, oDbType, oDirection, sName, oValue);
    }

    /// <summary>
    /// Carica i providers con le relative connessioni.
    /// </summary>
    /// <param name="sFile">File di configurazione.</param>
    public void Load(string sFile)
    {
        XML oRegSchema = new XML();
        sFile = FileManager.GetPathRoot(FileManager.NormalizePath(sFile));
        //
        oRegSchema.Load(sFile);
        //
        Load(oRegSchema);
    }
    /// <summary>
    /// Carica i providers con le relative connessioni.
    /// </summary>
    /// <param name="oXMLManager">Manager del file di configurazione.</param>
    public void Load(XML oXMLManager)
    {
        string[] sProvidersKey = oXMLManager.GetX("/registry/providers/provider/@name");
        string[] sConnectionsKey = oXMLManager.GetX("/registry/connections/connection/@name");
        string[] sInclusions = oXMLManager.GetX("/registry/inclusions/include/@filename");
        //
        // Providers inclusi nel file di configurazione.
        for (int i = 0; sProvidersKey != null && i < sProvidersKey.Length; i++)
        {
            string[] sClassName = oXMLManager.GetX("/registry/providers/provider[@name='" + sProvidersKey[i] + "']/@classname");
            string[] sAssembly = oXMLManager.GetX("/registry/providers/provider[@name='" + sProvidersKey[i] + "']/@assemblyname");
            //
            sAssembly[0] = FileManager.GetPathRoot(FileManager.NormalizePath(sAssembly[0]));
            this.Providers.Add(sProvidersKey[i], sClassName[0], sAssembly[0]);
        }
        //
        // Connessioni incluse nel file di configurazione.
        for (int i = 0; sConnectionsKey != null && i < sConnectionsKey.Length; i++)
        {
            string[] sString = oXMLManager.GetX("/registry/connections/connection[@name='" + sConnectionsKey[i] + "']/@string");
            string[] sType = oXMLManager.GetX("/registry/connections/connection[@name='" + sConnectionsKey[i] + "']/@type");
            string[] sOpenType = oXMLManager.GetX("/registry/connections/connection[@name='" + sConnectionsKey[i] + "']/@opentype");
            if (sString == null || sType == null)
                throw (new Exception());
            this.Connections.Add(sConnectionsKey[i], sType[0].ToLower(), sString[0]);
        }
        //
        // Fine inclusi nel file di configurazione.
        for (int i = 0; sInclusions != null && i < sInclusions.Length; i++)
        {
            this._Inclusions.Set(sInclusions[i], null);
        }
    }
}
