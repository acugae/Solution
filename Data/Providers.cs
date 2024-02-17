namespace Solution.Data;

/// <summary>
/// Contiene collezione di oggetti cProvider, ne gestisce il ciclo di vita.
/// </summary>
/// 
/// <remarks>
/// Provider inseriti di default in ambiente non Mobile:
///     "oledb", "System.Data.OleDb.OleDbFactory"
///     "sqldb", "System.Data.SqlClient.SqlClientFactory"
///     "odbdb", "System.Data.Odbc.OdbcFactory"
/// Provider inseriti di default in ambiente Mobile:
///     "oledb", "System.Data.OleDb.OleDbFactory"
///     "sqldb", "System.Data.SqlClient.SqlClientFactory"
///     "odbdb", "System.Data.Odbc.OdbcFactory"
/// </remarks>
public class Providers : System.Collections.Specialized.NameObjectCollectionBase
{
    /// <summary>
    /// Inizializza l'istanza.
    /// </summary>
    public Providers()
    {
        Add("sqldb", "System.Data.SqlClient.SqlClientFactory", null);
        Add("mysdb", "MySql.Data.MySqlClient.MySqlClientFactory", null);
        Add("pstdb", "Npgsql.NpgsqlFactory", null);
        //Add("odbdb", "System.Data.Odbc.OdbcFactory", null);
        //Add("oledb", "System.Data.OleDb.OleDbFactory", null);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public void Add(Provider.Provider value)
    {
        base.BaseAdd(value.Key, value);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyProvider"></param>
    /// <param name="sClassFactoryName"></param>
    /// <param name="sFilename"></param>
    public void Add(string sKeyProvider, string sClassFactoryName, string sFilename)
    {
        Provider.Provider oPM = new Provider.Provider(sKeyProvider, sClassFactoryName, sFilename);
        Add(oPM);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public void Set(Provider.Provider value)
    {
        base.BaseSet(value.Key, value);
    }
    /// <summary>
    /// 
    /// </summary>
    public void Clear()
    {
        base.BaseClear();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Contains(string key)
    {
        return base.BaseGet(key) != null;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Contains(int key)
    {
        return base.BaseGet(key) != null;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    public void Remove(string key)
    {
        base.BaseRemove(key);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Provider.Provider this[string key]
    {
        get { return (Provider.Provider)(base.BaseGet(key)); }
        set { base.BaseSet(key, value); }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Provider.Provider this[int key]
    {
        get { return (Provider.Provider)(base.BaseGet(key)); }
        set { base.BaseSet(key, value); }
    }

#if (!MOBILE)
    /// <summary>
    /// 
    /// </summary>
    public object[] Values
    {
        get { return base.BaseGetAllValues(); }
    }
#else
		public object[] Values
	    {
            get
            {
                int iLen = base.BaseGetAllKeys().Length;
                object[] obv = new object[iLen];
                for (int i = 0; i < iLen; i++)
                {
                    obv[i] = base.Keys[i];
                }
                return obv;
            }
        }
#endif
}
