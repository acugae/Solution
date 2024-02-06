namespace Solution.Collections;

/// <summary>
/// Consente di gestire collezione di oggetti.
/// </summary>
/// <see cref="Solution.Collections.cGCollection"/>
[Serializable]
public class cCollection : ISerializable
{
    public event ParamEventHandler OnAdd;
    public event ParamEventHandler OnRemove;
    public event ParamEventHandler OnRemoveFromIndex;
    ArrayList arKeys;
    Hashtable htColl;
    /// <summary>
    /// Ottiene il numero di coppie contenute nella collezione.
    /// </summary>
    public int Count
    {
        get
        {
            lock (this)
            {
                if (arKeys == null || htColl == null)
                    return 0;
                if (arKeys.Count == htColl.Count)
                    return arKeys.Count;
                return -1;
            }
        }
    }
    //
    private cCollection(SerializationInfo info, StreamingContext context)
    {
        /*
        htColl = (Hashtable)info.GetValue("htColl", typeof(Hashtable));
        */
        cReflectionManager oR = new cReflectionManager();
        byte[] bhtColl = new byte[2];
        bhtColl = (byte[])info.GetValue("htColl", bhtColl.GetType());
        htColl = (Hashtable)oR.BinaryDeserialize(bhtColl);
        arKeys = (ArrayList)info.GetValue("arKeys", typeof(ArrayList));
    }
    /// <summary>
    /// Serializza l'istenza con i dati necessari.
    /// </summary>
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        /*
        info.AddValue("htColl", htColl);
        */
        cReflectionManager oR = new cReflectionManager();
        byte[] bhtColl = oR.BinarySerialize(htColl);
        info.AddValue("htColl", bhtColl);
        info.AddValue("arKeys", arKeys);
    }
    /// <summary>
    /// Viene restituita la chiave per l'indice specificato.
    /// </summary>
    /// <param name="index">Indice per cui è necessario che sia restituita una chiave.</param>
    /// <returns>La chiave selezionata.</returns>
    public object GetKey(int index)
    {
        return arKeys[index];
    }
    /// <summary>
    /// Viene restituito il valore per l'indice specificato.
    /// </summary>
    /// <param name="index">Indice per cui è necessario che sia restituito un valore.</param>
    /// <returns>Il valore selezionato.</returns>
    public object GetValue(int index)
    {
        return this[arKeys[index]];
    }
    /// <summary>
    /// Viene restituito l'indice per la chiave specificata.
    /// </summary>
    /// <param name="key">Chiave per cui è necessario che sia restituito l'indice.</param>
    /// <returns>L'indice selezionato.</returns>
    public int GetIndex(object key)
    {
        return arKeys.IndexOf(key);
    }
    /// <summary>
    /// Viene restituito il valore per la chiave specificata.
    /// </summary>
    /// <param name="key">Chiave per cui è necessario che sia restituito il valore.</param>
    /// <returns>Valore selezionato.</returns>
    public object this[object Key]
    {
        get
        {
            lock (this)
            {
                return htColl[Key];
            }
        }
        set
        {
            lock (this)
            {
                htColl[Key] = value;
            }
        }

    }
    /// <summary>
    /// Inizializza una nuova istanza vuota della classe.
    /// </summary>
    public cCollection()
    {
        lock (this)
        {
            htColl = new Hashtable();
            arKeys = new ArrayList();
        }
    }
    /// <summary>
    /// Inizializza una nuova istanza vuota della classe, con la capacità iniziale specificata.
    /// </summary>
    public cCollection(int capacity)
    {
        lock (this)
        {
            htColl = new Hashtable(capacity);
            arKeys = new ArrayList(capacity);
        }
    }
    /// <summary>
    /// Inizializza una nuova istanza della classe, copiando il contenuto della collezione specificata.
    /// </summary>
    public cCollection(cCollection oCollection)
    {
        lock (this)
        {
            htColl = new Hashtable();
            Array oK = Array.CreateInstance(typeof(Object), oCollection.htColl.Count);
            Array oV = Array.CreateInstance(typeof(Object), oCollection.htColl.Count);
            oCollection.htColl.Keys.CopyTo(oK, 0);
            oCollection.htColl.Values.CopyTo(oV, 0);
            for (int i = 0; i < oK.Length; i++)
            {
                this.htColl.Add(oK.GetValue(i), oV.GetValue(i));
            }
            //
            arKeys = new ArrayList();
            arKeys.AddRange(oCollection.arKeys.ToArray());
        }
    }
    /// <summary>
    /// Effettua operazione sulla collezione.
    /// </summary>
    /// <param name="Key">Chivae</param>
    /// <param name="Obj">Valore</param>
    /// <param name="sOperation">Operazioni possibili "add, remove"</param>
    /// <returns>True se l'operazione è andata a buon fine, False altrimenti.</returns>
    public bool AdavancedOperator(object Key, object Obj, string sOperation)
    {
        lock (this)
        {
            try
            {
                if (htColl.Count != arKeys.Count)
                {
                    Console.WriteLine(" Solution : AdavancedOperator Errore indici disallineati");
                }
                if (sOperation.ToLower().Equals("add"))
                {
                    Add(Key, Obj);
                    return true;
                }
                if (sOperation.ToLower().Equals("remove"))
                {
                    Remove(Key);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore AdavancedOperator in cCollection : (" + ex.Message + ")");
                return false;
            }
        }
    }
    /// <summary>
    /// Scambia l'elemento in posizione index1 con l'elemento in posizione index2.
    /// </summary>
    public void Swap(int index1, int index2)
    {
        if (index1 > this.Count || index2 > this.Count || index1 == index2)
            return;
        //
        if (index1 > index2)
        {
            int k = index1;
            index1 = index2;
            index2 = k;
        }
        object objTmpkey = this.GetKey(index1);
        object objTmpValue = this.GetValue(index1);
        object objTmp2key = this.GetKey(index2);
        object objTmp2Value = this.GetValue(index2);
        this.Remove(this.GetKey(index2));
        this.Insert(objTmp2key, index1, objTmp2Value);
        this.Remove(this.GetKey(index1 + 1));
        this.Insert(objTmpkey, index2, objTmpValue);
    }
    /// <summary>
    /// Inserisce nell'istanza la coppia Key, Obj alla fine della collezione.
    /// </summary>
    public void Add(object Key, object Obj)
    {
        lock (this)
        {
            if (htColl.ContainsKey(Key))
                throw new Exception("Chiave già presente");
            htColl.Add(Key, Obj);
            arKeys.Add(Key);
            if (OnAdd != null)
                OnAdd(this, Key);
        }
    }
    /// <summary>
    /// Inserisce nell'istanza le collezioni di chiavi, valori.
    /// </summary>
    public void AddRange(ICollection oCollectionKey, ICollection oCollectionValue)
    {
        lock (this)
        {
            if (oCollectionKey == null)
                return;
            foreach (object o in oCollectionKey)
            {
                this.Add(o, null);
            }
            //
            if (oCollectionValue != null)
            {
                int i = 0;
                foreach (object o in oCollectionValue)
                {
                    this[this.GetKey(i++)] = o;
                }
            }
        }
    }
    /// <summary>
    /// Se la chiave esiste sovrascrive il valore, altrimenti inserisce la coppia Key, Obj alla fine della collezione.
    /// </summary>
    public void Set(object Key, object Obj)
    {
        lock (this)
        {
            if (htColl.ContainsKey(Key))
                htColl[Key] = Obj;
            else
                this.Add(Key, Obj);
        }
    }
    /// <summary>
    /// Ritorna la collezione di chiavi.
    /// </summary>
    public ICollection Keys
    {
        get { return arKeys.ToArray() /* htColl.Keys */; }
    }
    //
    public string GetStringFromCollection()
    {
        string sValue = "";
        for (int i = 0; i < htColl.Keys.Count; i++)
        {
            sValue += this.GetKey(i).ToString() + "=" + this.GetValue(i).ToString();
            if (i < htColl.Keys.Count - 1)
                sValue += "&";
        }
        return sValue;
    }
    //
    public void SetCollectionFromString(string sStringCollection)
    {
        if (sStringCollection == null || sStringCollection.Length <= 0)
            return;
        string[] sItems = sStringCollection.Split('&');
        if (sItems == null)
            return;
        for (int i = 0; i < sItems.Length; i++)
        {
            string[] sValues = sItems[i].Split('=');
            if (sValues.Length == 2)
            {
                if (this.ContainsKey(sValues[0]))
                    this[sValues[0]] = sValues[1];
                else
                    this.Add(sValues[0], sValues[1]);
            }
        }
    }
    /// <summary>
    /// Inserisce nell'istanza la coppia Key, Obj in posizione index.
    /// </summary>
    public void Insert(object Key, int index, object Obj)
    {
        lock (this)
        {
            if (htColl.ContainsKey(Key))
                throw new Exception("Chiave già presente");
            htColl.Add(Key, Obj);
            arKeys.Insert(index, Key);
            if (OnAdd != null)
                OnAdd(this, Key);
        }
    }
    /// <summary>
    /// Rimuove l'elemento con chiave Key.
    /// </summary>
    public void Remove(object Key)
    {
        lock (this)
        {
            if (!htColl.ContainsKey(Key))
                throw new Exception("Chiave non presente");
            arKeys.Remove(Key);
            htColl.Remove(Key);
            if (OnRemove != null)
                OnRemove(this, Key);
        }
    }
    /// <summary>
    /// Rimuove l'elemento in posizione index.
    /// </summary>
    public void RemoveFromIndex(int index)
    {
        lock (this)
        {
            htColl.Remove(arKeys[index]);
            arKeys.RemoveAt(index);
            if (OnRemoveFromIndex != null)
                OnRemoveFromIndex(this, index);
        }
    }
    /// <summary>
    /// Controlla se esiste un elemento con chiave specificata.
    /// </summary>
    public bool ContainsKey(object key)
    {
        return htColl.ContainsKey(key);
    }
    /// <summary>
    /// Controlla se esiste un elemento con valore specificato.
    /// </summary>
    public bool ContainsValue(object oValue)
    {
        return htColl.ContainsValue(oValue);
    }
    /// <summary>
    /// Cancella tutti gli elementi dell'istanza.
    /// </summary>
    public void Clear()
    {
        htColl.Clear();
        arKeys.Clear();
    }
    //
    private object[] ToArray()
    {
        if (arKeys == null)
            return null;
        return arKeys.ToArray();
    }
}
