namespace Solution.Data.Provider;

/// <summary>
/// Classe contenete collezione di oggetti cParameter.
/// </summary>
public class Parameters
{
    private IDbCommand _Command;
    private Connection oCn;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oCn"></param>
    /// <param name="oCmd"></param>
    public Parameters(Connection oCn, IDbCommand oCmd)
    {
        this.oCn = oCn;
        _Command = oCmd;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oCn"></param>
    /// <param name="oCmd"></param>
    public Parameters(Connection oCn, Command oCmd)
    {
        this.oCn = oCn;
        _Command = oCmd.IDbCommand;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="dbType"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public Parameter Add(string name, System.Data.DbType dbType, int size)
    {
        Parameter oP = new Parameter(oCn);
        oP.ParameterName = name;
        oP.DbType = dbType;
        oP.Size = size;
        _Command.Parameters.Add(oP.IDataParameter);
        return oP;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="dbType"></param>
    /// <param name="size"></param>
    /// <param name="sourceColumn"></param>
    /// <returns></returns>
    public Parameter Add(string name, System.Data.DbType dbType, int size, string sourceColumn)
    {
        Parameter oP = new Parameter(oCn);
        oP.ParameterName = name;
        oP.DbType = dbType;
        oP.Size = size;
        oP.SourceColumn = sourceColumn;
        _Command.Parameters.Add(oP.IDataParameter);
        return oP;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="dbType"></param>
    /// <returns></returns>
    public Parameter Add(string name, System.Data.DbType dbType)
    {
        Parameter oP = new Parameter(oCn);
        oP.ParameterName = name;
        oP.DbType = dbType;
        oP.Size = -1;
        oP.SourceColumn = "";
        _Command.Parameters.Add(oP);
        return oP;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public Parameter Add(string name, object value)
    {
        try
        {
            Parameter oP = new Parameter(oCn);
            oP.ParameterName = name;
            oP.Value = value;
            oP.Size = -1;
            oP.SourceColumn = "";
            _Command.Parameters.Add(oP.IDataParameter);
            return oP;
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oParam"></param>
    /// <returns></returns>
    public Parameter Add(Parameter oParam)
    {
        try
        {
            _Command.Parameters.Add(oParam.IDataParameter);
            return oParam;
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strParameterString">[nomecampo]=[valuecampo]&[nomecampo]=[valuecampo]&[nomecampo]=[valuecampo]</param>
    public void AddParameterString(string strParameterString)
    {
        try
        {
            if (strParameterString == null || strParameterString.Trim().Equals(""))
                return;
            //
            string[] strNameFileds = ParseStringParmas(strParameterString, 0);
            string[] strValuesFileds = ParseStringParmas(strParameterString, 1);
            //
            for (int j = 0; j < strNameFileds.Length; j++)
            {
                Parameter oP = new Parameter(oCn);
                oP.ParameterName = strNameFileds[j];
                oP.Size = -1;
                oP.SourceColumn = "";
                if (strValuesFileds[j].Equals("#NULL#"))
                    oP.Value = "";
                else
                    oP.Value = strValuesFileds[j];
                _Command.Parameters.Add(oP.IDataParameter);
            }
        }
        catch (Exception e)
        {
            throw (e);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strParams">[nomecampo]=[valuecampo]&[nomecampo]=[valuecampo]&[nomecampo]=[valuecampo]</param>
    /// <param name="type"></param>
    /// <returns></returns>
    private string[] ParseStringParmas(string strParams, int type)
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
    /// <summary>
    /// 
    /// </summary>
    public void Clear()
    {
        _Command.Parameters.Clear();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    public void Insert(int index, object value)
    {
        _Command.Parameters.Insert(index, value);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public void Remove(object value)
    {
        _Command.Parameters.Remove(value);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    public void RemoveAt(int index)
    {
        _Command.Parameters.RemoveAt(index);
    }
    /// <summary>
    /// 
    /// </summary>
    public int Count
    {
        get { return _Command.Parameters.Count; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameterName"></param>
    /// <returns></returns>
    public System.Boolean Contains(System.String parameterName)
    {
        return _Command.Parameters.Contains(parameterName);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameterName"></param>
    /// <returns></returns>
    public System.Int32 IndexOf(System.String parameterName)
    {
        return _Command.Parameters.IndexOf(parameterName);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameterName"></param>
    public void RemoveAt(System.String parameterName)
    {
        _Command.Parameters.RemoveAt(parameterName);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public object this[int index]
    {
        get { return _Command.Parameters[index]; }
        set { _Command.Parameters[index] = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="parameterName"></param>
    /// <returns></returns>
    public System.Data.IDataParameter this[string parameterName]
    {
        get { return (IDataParameter)_Command.Parameters[parameterName]; }
        set { _Command.Parameters[parameterName] = value; }
    }
    /// <summary>
    /// 
    /// </summary>
    public System.Data.IDataParameterCollection IDataParameterCollection
    {
        get { return _Command.Parameters; }
    }
}
