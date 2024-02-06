namespace Solution.Collections;

/// <summary>
/// Classe per gestire collezione di oggetti, supporta Generics.
/// </summary>
/// <see cref="Solution.Collections.cCollection"/>
[Serializable]
public class cGCollection<K, V> : ISerializable
{
    public event ParamEventHandler OnAdd;
    public event ParamEventHandler OnRemove;
    public event ParamEventHandler OnRemoveFromIndex;
    List<K> arKeys = new List<K>();
    Dictionary<K, V> htColl = new Dictionary<K, V>();
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
    private cGCollection(SerializationInfo info, StreamingContext context)
    {
        cReflectionManager oR = new cReflectionManager();
        //byte[] bhtColl = new byte[2];
        //byte[] bhtColl = (byte[])info.GetValue("htColl", bhtColl.GetType());
        byte[] bhtColl = (byte[])info.GetValue("htColl", typeof(byte[]));
        htColl = (Dictionary<K, V>)oR.BinaryDeserialize(bhtColl);
        //arKeys = (List<K>)info.GetValue("arKeys", typeof(List<K>));
        byte[] barKeys = (byte[])info.GetValue("arKeys", typeof(byte[]));
        arKeys = (List<K>)oR.BinaryDeserialize(barKeys);
    }
    /// <summary>
    /// Serializza l'istenza con i dati necessari.
    /// </summary>
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        cReflectionManager oR = new cReflectionManager();
        byte[] bhtColl = oR.BinarySerialize(htColl);
        byte[] barKeys = oR.BinarySerialize(arKeys);
        info.AddValue("htColl", bhtColl);
        info.AddValue("arKeys", barKeys);
    }
    /// <summary>
    /// Viene restituita la chiave per l'indice specificato.
    /// </summary>
    /// <param name="index">Indice per cui è necessario che sia restituita una chiave.</param>
    /// <returns>La chiave selezionata.</returns>
    public K GetKey(int index)
    {
        return arKeys[index];
    }
    /// <summary>
    /// Viene restituito il valore per l'indice specificato.
    /// </summary>
    /// <param name="index">Indice per cui è necessario che sia restituito un valore.</param>
    /// <returns>Il valore selezionato.</returns>
    public V GetValue(int index)
    {
        return this[arKeys[index]];
    }
    /// <summary>
    /// Viene restituito l'indice per la chiave specificata.
    /// </summary>
    /// <param name="key">Chiave per cui è necessario che sia restituito l'indice.</param>
    /// <returns>L'indice selezionato.</returns>
    public int GetIndex(K key)
    {
        return arKeys.IndexOf(key);
    }
    /// <summary>
    /// Viene restituito il valore per la chiave specificata.
    /// </summary>
    /// <param name="key">Chiave per cui è necessario che sia restituito il valore.</param>
    /// <returns>Valore selezionato.</returns>
    public V this[K Key]
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
    public cGCollection()
    {
        lock (this)
        {
            htColl = new Dictionary<K, V>();
            arKeys = new List<K>();
        }
    }
    /// <summary>
    /// Inizializza una nuova istanza vuota della classe, con la capacità iniziale specificata.
    /// </summary>
    public cGCollection(int capacity)
    {
        lock (this)
        {
            htColl = new Dictionary<K, V>(capacity);
            arKeys = new List<K>(capacity);
        }
    }
    /// <summary>
    /// Inizializza una nuova istanza della classe, copiando il contenuto della collezione specificata.
    /// </summary>
    public cGCollection(cGCollection<K, V> oCollection)
    {
        lock (this)
        {
            htColl = new Dictionary<K, V>(oCollection.htColl);
            arKeys = new List<K>(oCollection.htColl.Keys);
        }
    }
    /// <summary>
    /// Effettua operazione sulla collezione.
    /// </summary>
    /// <param name="Key">Chivae</param>
    /// <param name="Obj">Valore</param>
    /// <param name="sOperation">Operazioni possibili "add, remove"</param>
    /// <returns>True se l'operazione è andata a buon fine, False altrimenti.</returns>
    public bool AdavancedOperator(K Key, V Obj, string sOperation)
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
        K objTmpkey = this.GetKey(index1);
        V objTmpValue = this.GetValue(index1);
        K objTmp2key = this.GetKey(index2);
        V objTmp2Value = this.GetValue(index2);
        this.Remove(this.GetKey(index2));
        this.Insert(objTmp2key, objTmp2Value, index1);
        this.Remove(this.GetKey(index1 + 1));
        this.Insert(objTmpkey, objTmpValue, index2);
    }
    /// <summary>
    /// Inserisce nell'istanza la coppia Key, Obj alla fine della collezione.
    /// </summary>
    public void Add(K Key, V Obj)
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
    /// Se la chiave esiste sovrascrive il valore, altrimenti inserisce la coppia Key, Obj alla fine della collezione.
    /// </summary>
    public void Set(K Key, V Obj)
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
        get { return arKeys.ToArray(); }
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
    /// <summary>
    /// Inserisce nell'istanza la coppia Key, Obj in posizione index.
    /// </summary>
    public void Insert(K Key, V Obj, int index)
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
    public void Remove(K Key)
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
    public bool ContainsKey(K key)
    {
        return htColl.ContainsKey(key);
    }
    /// <summary>
    /// Controlla se esiste un elemento con valore specificato.
    /// </summary>
    public bool ContainsValue(V oValue)
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
    private K[] ToArray()
    {
        if (arKeys == null)
            return null;
        return arKeys.ToArray();
    }
}
