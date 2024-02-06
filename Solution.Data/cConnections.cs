namespace Solution.Data;

/// <summary>
/// Contiene la collezione di oggetti cConnection, ne gestisce il ciclo di vita.
/// </summary>
public class cConnections : System.Collections.Specialized.NameObjectCollectionBase
{
    cProviders _oProviders = null;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="oProviders"></param>
    public cConnections(cProviders oProviders)
    {
        _oProviders = oProviders;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public void Add(cConnection value)
    {
        base.BaseAdd(value.Key, value);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <param name="sKeyProvider"></param>
    /// <param name="connectionString"></param>
    public void Add(string sKeyConnection, string sKeyProvider, string connectionString)
    {
        cConnection oCn = new cConnection(_oProviders[sKeyProvider], sKeyConnection, connectionString);
        Add(oCn);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sKeyConnection"></param>
    /// <returns></returns>
    public cConnection Clone(string sKeyConnection)
    {
        cConnection oConnectionTmp = (cConnection)(base.BaseGet(sKeyConnection));
        return new cConnection(oConnectionTmp.Provider, sKeyConnection, oConnectionTmp.ConnectionString);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public void Set(cConnection value)
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
    public cConnection this[string key]
    {
        get { return (cConnection)(base.BaseGet(key)); }
        set { base.BaseSet(key, value); }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public cConnection this[int key]
    {
        get { return (cConnection)(base.BaseGet(key)); }
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
    /// <summary>
    /// Apre tutte le connessioni.
    /// </summary>
    /// <returns></returns>
    public string[] OpenAll()
    {
        ArrayList oStrConn = new ArrayList();
        Thread[] lThreads = new Thread[this.Count];
        int cont = 0;

        foreach (string iCn in this)
        {
            try
            {
                lThreads[cont] = new Thread(new ThreadStart(((cConnection)this[iCn]).Open));
                lThreads[cont].Start();
                ++cont;
            }
            catch
            {
            }
        }
        //
        // Blocca la chiamata al Thread fino a quando termina.
        for (int i = 0; i < lThreads.Length; i++)
            lThreads[i].Join();
        for (int i = 0; i < this.Count; i++)
        {
            if (this[i].IsOpen())
                oStrConn.Add(this.Keys[i]);
        }
        string[] oResult = (string[])oStrConn.ToArray(Type.GetType("System.String"));
        return oResult;
    }

    /// <summary>
    /// Chiude tutte le connessioni.
    /// </summary>
    public void CloseAll()
    {
        try
        {
            foreach (string iCn in this)
            {
                try
                {
                    this[iCn].Close();
                }
                catch
                {
                }
            }
        }
        catch (Exception e)
        {
            throw e;
        }
    }
    /// <summary>
    /// Ritorna tutte le connessioni aperte.
    /// </summary>
    /// <returns></returns>
    public System.Collections.Hashtable GetHashConnectionsOpen()
    {
        System.Collections.Hashtable oEnumConnOpen = new System.Collections.Hashtable();
        int i = 0;
        try
        {
            foreach (string iCn in this)
            {
                if (this[iCn].State == ConnectionState.Open)
                {
                    oEnumConnOpen.Add((int)i, (string)iCn);
                }
                i++;
            }
            return oEnumConnOpen;
        }
        catch (Exception e)
        {
            throw e;
        }

    }
}
