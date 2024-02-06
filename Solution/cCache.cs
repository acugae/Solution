namespace Solution;
public class cCacheValue<T>
{
    private T _value;
    public T Value
    {
        get
        {
            if (IsZipped)
                return DecompressAndDeserialize(ZippedValue);
            else
                return _value;
        }
        set
        {
            _value = value;
        }
    }

    private byte[] ZippedValue
    {
        get;
        set;
    }

    public Boolean IsUnitSize
    {
        get;
        set;
    }

    public long Size
    {
        get;
        set;
    }

    public DateTime ExpirationTime
    {
        get;
        private set;
    }

    public Boolean IsZipped
    { get; set; }

    public cCacheValue(T Value, int ExpirationThreshold, Boolean zip = false, bool isUnitSize = false)
    {
        this.IsZipped = zip;
        this.IsUnitSize = isUnitSize;
        if (this.IsZipped)
        {
            this._value = default(T);
            this.ZippedValue = SerializeAndCompress(Value);
            this.Size = this.ZippedValue.Length; //GetSize(ZippedValue);
        }
        else
        {
            this.ZippedValue = null;
            this._value = Value;
            this.Size = GetSize(_value);
        }
        if (ExpirationThreshold != 0)
            this.ExpirationTime = DateTime.Now.AddMinutes(ExpirationThreshold);
        else
            this.ExpirationTime = DateTime.MaxValue;
    }

    private long GetSize(T Value)
    {
        if (IsUnitSize)
            return 1;
        else
        {
            //MemoryStream ms = new MemoryStream();
            //BinaryFormatter bf = new BinaryFormatter();
            //bf.Serialize(ms, Value);
            //return ms.Length;
            return Binary.ObjectToByteArray(Value).Length;
        }
    }

    //private long GetSize(byte[] Value)
    //{
    //    if (IsUnitSize)
    //        return 1;
    //    else
    //    {
    //        MemoryStream ms = new MemoryStream();
    //        BinaryFormatter bf = new BinaryFormatter();
    //        bf.Serialize(ms, Value);
    //        return ms.Length;
    //    }
    //}

    private byte[] SerializeAndCompress(object obj)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (GZipStream zs = new GZipStream(ms, CompressionMode.Compress, true))
            {
                //BinaryFormatter bf = new BinaryFormatter();
                //bf.Serialize(zs, obj);
                return Binary.ObjectToByteArray(obj);
            }
            //return ms.ToArray();
        }
    }

    private T DecompressAndDeserialize(byte[] data)
    {
        using (MemoryStream ms = new MemoryStream(data))
        {
            using (GZipStream zs = new GZipStream(ms, CompressionMode.Decompress, true))
            {
                //BinaryFormatter bf = new BinaryFormatter();
                //return (T)bf.Deserialize(zs);
                return Binary.ByteArrayToObject<T>(data);
            }
        }
    }
}

public class cCacheKey
{
    public string QueryString
    {
        get;
        private set;
    }

    public DateTime LastUsedTime
    {
        get;
        private set;
    }

    public cCacheKey(string QueryString)
    {
        this.QueryString = GetHash(QueryString);
        this.LastUsedTime = DateTime.Now;
    }

    private static string GetHash(string inputString)
    {
        // step 1, calculate MD5 hash from input
        MD5 md5 = System.Security.Cryptography.MD5.Create();
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(inputString);
        byte[] hash = md5.ComputeHash(inputBytes);
        string str = BitConverter.ToString(hash);
        return str;
    }

    public override int GetHashCode()
    {
        return QueryString.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as cCacheKey);
    }
    public bool Equals(cCacheKey obj)
    {
        return this.QueryString.Equals(obj.QueryString);
    }
}

public class cCacheManager
{
    private Dictionary<string, object> caches
    { get; set; }
    public cCacheManager(cXMLManager oXML)
    {
        Init(oXML);
    }
    public cCacheManager(string sConfigXML)
    {
        cXMLManager oXML = new cXMLManager();
        oXML.Load(sConfigXML);
        Init(oXML);
    }

    private void Init(cXMLManager oXML)
    {
        caches = new Dictionary<string, object>();
        string sTarget = oXML.GetX("/registry/configurations/@target", "preproduzione");
        string[] configCaches = oXML.GetX("/registry/configurations/" + sTarget + "/cache/add/@key");
        for (int i = 0; configCaches != null && i < configCaches.Length; i++)
        {
            string sType = oXML.GetX("/registry/configurations/" + sTarget + "/cache/add[@key='" + configCaches[i] + "']/@type", "");
            string sPeriod = oXML.GetX("/registry/configurations/" + sTarget + "/cache/add[@key='" + configCaches[i] + "']/@minutesCleanPeriod", "");
            string sDim = oXML.GetX("/registry/configurations/" + sTarget + "/cache/add[@key='" + configCaches[i] + "']/@byteDimension", "");
            bool bIsZip = bool.Parse(oXML.GetX("/registry/configurations/" + sTarget + "/cache/add[@key='" + configCaches[i] + "']/@IsZip", "false"));
            long bytesDimension = 0;
            bool IsUnit = false;
            if (sDim.ToUpper().Contains("GB"))
            {
                int index = sDim.ToUpper().IndexOf("GB");
                string value = sDim.Substring(0, index);
                bytesDimension = long.Parse(value) * 1024 * 1024 * 1024;
            }
            else if (sDim.ToUpper().Contains("MB"))
            {
                int index = sDim.ToUpper().IndexOf("MB");
                string value = sDim.Substring(0, index);
                bytesDimension = long.Parse(value) * 1024 * 1024;
            }
            else if (sDim.ToUpper().Contains("KB"))
            {
                int index = sDim.ToUpper().IndexOf("KB");
                string value = sDim.Substring(0, index);
                bytesDimension = long.Parse(value) * 1024;
            }
            else if (sDim.ToUpper().Contains("UNIT"))
            {
                int index = sDim.ToUpper().IndexOf("UNIT");
                string value = sDim.Substring(0, index);
                bytesDimension = long.Parse(value);
                IsUnit = true;
            }
            Type type = Type.GetType(sType); //GetTypeEx(sType); // Type.GetType(sType);
            Type cacheGenericType = typeof(cCache<>);
            Type constructed = cacheGenericType.MakeGenericType(type);
            object o = Activator.CreateInstance(constructed, bytesDimension, int.Parse(sPeriod), bIsZip, IsUnit);
            caches.Add(configCaches[i], o);
        }
    }

    //private Type GetTypeEx(string fullTypeName)
    //{
    //    var type = Type.GetType(fullTypeName);
    //    if (type != null)
    //        return type;
    //    ICollection assemblies = System.Web.Compilation.BuildManager.GetReferencedAssemblies();
    //    foreach (Assembly a in assemblies)
    //    {
    //        type = a.GetType(fullTypeName);
    //        if (type != null)
    //            return type;
    //    }
    //    return null;

    //}

    public cCache<DataTable> getDataTableCache(string cacheName)
    {
        object cacheObject = caches[cacheName];
        if (cacheObject != null && cacheObject.GetType() == typeof(cCache<DataTable>))
            return (cCache<DataTable>)cacheObject;
        else
            return null;
    }

    public cCache<string> getStringCache(string cacheName)
    {
        object cacheObject = caches[cacheName];
        if (cacheObject != null && cacheObject.GetType() == typeof(cCache<string>))
            return (cCache<string>)cacheObject;
        else
            return null;
    }

    public object getGenericCache(string cacheName)
    {
        if (caches.ContainsKey(cacheName))
            return caches[cacheName];
        else
            return null;
    }

    public object Get(string cacheName, string sKey, object oObjectDefault = null)
    {
        if (caches.ContainsKey(cacheName))
        {
            object cacheObject = caches[cacheName];
            if (cacheObject != null)
            {
                Type myTypeObj = cacheObject.GetType();
                MethodInfo cleanMethod = myTypeObj.GetMethod("Get");
                return cleanMethod.Invoke(cacheObject, new object[1] { sKey });
            }
        }
        return oObjectDefault;
    }

    public bool Set(string cacheName, string sKey, object oValue)
    {
        if (caches.ContainsKey(cacheName))
        {
            object cacheObject = caches[cacheName];
            if (cacheObject != null)
            {
                Type myTypeObj = cacheObject.GetType();
                MethodInfo cleanMethod = myTypeObj.GetMethod("Set");
                cleanMethod.Invoke(cacheObject, new object[3] { sKey, oValue, 0 });
                return true;
            }
        }
        return false;
    }

    public void Clean(string cacheName = null)
    {
        if (caches == null)
            return;
        if (cacheName != null)
        {
            object cacheObject = caches[cacheName];
            if (cacheObject != null)
            {
                Type myTypeObj = cacheObject.GetType();
                MethodInfo cleanMethod = myTypeObj.GetMethod("ExplicitClean");
                cleanMethod.Invoke(cacheObject, new object[0] { });
            }
        }
        else
        {
            foreach (string sKey in caches.Keys)
            {
                object cacheObject = caches[sKey];
                if (cacheObject != null)
                {
                    Type myTypeObj = cacheObject.GetType();
                    MethodInfo cleanMethod = myTypeObj.GetMethod("ExplicitClean");
                    cleanMethod.Invoke(cacheObject, new object[0] { });
                }
            }
        }
    }
}

public class cCache<T> : Dictionary<cCacheKey, cCacheValue<T>>
{
    public long MaximumSize
    { get; private set; }
    public long CurrentSize
    { get; private set; }
    public long Length
    { get; private set; }
    private object _lock = null;
    private static Timer stateTimer;
    public bool IsUnit { get; set; }
    public bool IsZip { get; set; }

    public cCache(long CacheSize, int TrashPeriod, bool IsZipped = false, bool IsUnitSize = false) : base()
    {
        cLogger.WriteLine("StartService:cCache: Creating new cache Object of maximum bytes=" + CacheSize, cLogger.TipoLog.Debug);
        _lock = new Object();
        MaximumSize = CacheSize;
        CurrentSize = 0;
        Length = 0;
        IsUnit = IsUnitSize;
        IsZip = IsZipped;
        TimerCallback tcb = this.DeleteExpired;
        Object timerState = new Object();
        cLogger.WriteLine("Creating timer of " + TrashPeriod + " minutes for ThrashPeriod", cLogger.TipoLog.Debug);
        stateTimer = new Timer(tcb, timerState, TrashPeriod * 60 * 1000, TrashPeriod * 60 * 1000);
    }

    public new List<cCacheKey> Keys()
    {
        return base.Keys.ToList();
    }

    public void Set(string sKey, T Value, int cacheThreeshold = 0)
    {
        Add(new cCacheKey(sKey), new cCacheValue<T>(Value, cacheThreeshold, IsZip, IsUnit));
    }

    public new void Add(cCacheKey Key, cCacheValue<T> Value)
    {
        cLogger.WriteLine("StartService:cCache: adding new value=" + Key.QueryString, cLogger.TipoLog.Debug);
        lock (_lock)
        {
            if (this.ContainsKey(Key))
            {
                cLogger.WriteLine("StartService:cCache: key already present. Removing the oldest", cLogger.TipoLog.Debug);
                Remove(Key);
            }
            long newSize = CurrentSize + Value.Size;
            cLogger.WriteLine("StartService:cCache: Number of Items = " + Length + ". Current size is " + CurrentSize + ". The new size will be =" + newSize, cLogger.TipoLog.Debug);
            if (CurrentSize + Value.Size > MaximumSize)
            {
                cLogger.WriteLine("StartService:cCache: size needed not sufficient. It will be cleaned", cLogger.TipoLog.Debug);
                if (!Recycler(Value.Size))
                {
                    cLogger.WriteLine("StartService:cCache: unable to store cache object. Current cache size is=" + CurrentSize + " while object size is=" + Value.Size, cLogger.TipoLog.Warn);
                    return;
                }
            }
            cLogger.WriteLine("StartService:cCache: Adding Value = " + Value.Value.ToString(), cLogger.TipoLog.Debug);
            base.Add(Key, Value);
            //still newSize computing because after recycler currentSize could be changed
            newSize = CurrentSize + Value.Size;
            Length++;
            CurrentSize = newSize;
        }
    }

    public bool Remove(string sKey)
    {
        return Remove(new cCacheKey(sKey));
    }

    public new bool Remove(cCacheKey Key)
    {
        cLogger.WriteLine("StartService:cCache: removing value=" + Key.QueryString, cLogger.TipoLog.Debug);
        Boolean bRet = false;
        lock (_lock)
        {
            if (this.ContainsKey(Key))
            {
                bRet = true;
                cCacheValue<T> Value = this[Key];
                long newSize = CurrentSize - Value.Size;
                cLogger.WriteLine("StartService:cCache: Number of Items = " + Length + ". Current size is " + CurrentSize + ". The new size will be =" + newSize, cLogger.TipoLog.Debug);
                base.Remove(Key);
                CurrentSize = newSize;
                Length--;
            }
        }
        return bRet;
    }

    public T Get(string sKey)
    {
        return get(new cCacheKey(sKey));
    }

    public T get(cCacheKey Key)
    {
        cLogger.WriteLine("StartService:cCache: getting value=" + Key.QueryString, cLogger.TipoLog.Debug);
        T Value = default(T);
        lock (_lock)
        {
            if (this.ContainsKey(Key))
            {
                cLogger.WriteLine("StartService:cCache: value is present", cLogger.TipoLog.Debug);
                cCacheValue<T> TmpValue = this[Key];
                if (DateTime.Now <= TmpValue.ExpirationTime)
                {
                    Value = TmpValue.Value;
                    cLogger.WriteLine("StartService:cCache: The values is not expired", cLogger.TipoLog.Debug);
                    //Remove key with the oldest timestamp
                    base.Remove(Key);
                    //Add the same key with the new timestamp
                    base.Add(Key, TmpValue);
                    cLogger.WriteLine("StartService:cCache: getting Value = " + Value.ToString(), cLogger.TipoLog.Debug);
                }
                else
                {
                    cLogger.WriteLine("StartService:cCache: The values is expired. I'll remove it.", cLogger.TipoLog.Debug);
                    Remove(Key);
                }
            }
        }
        return Value;
    }

    private Boolean Recycler(long Bytes)
    {
        cLogger.WriteLine("StartService:cCache: Recycled invoked to get " + Bytes + " bytes", cLogger.TipoLog.Debug);
        Boolean retValue = false;

        lock (_lock)
        {
            DeleteExpired(null);

            if (MaximumSize - CurrentSize >= Bytes)
            {
                retValue = true;
            }
            else
            {
                List<cCacheKey> keys = base.Keys.ToList();
                List<cCacheKey> orderedKeys = keys.OrderBy(x => x.LastUsedTime).ToList();
                foreach (cCacheKey key in orderedKeys)
                {
                    cCacheValue<T> Value = base[key];
                    Remove(key);
                    cLogger.WriteLine("StartService:cCache: Recycled " + Value.Size + " of bytes because oldest even if it isn't expired", cLogger.TipoLog.Debug);
                    if (MaximumSize - CurrentSize >= Bytes)
                    {
                        retValue = true;
                        break;
                    }
                }
            }
        }
        cLogger.WriteLine("StartService:cCache: Recycled invoked to get " + Bytes + " bytes=" + retValue, cLogger.TipoLog.Debug);
        return retValue;
    }

    public void DeleteExpired(Object stateInfo)
    {
        lock (_lock)
        {
            cLogger.WriteLine("StartService:cCache: Delete expired task when  Number of Items = " + Length + ". Current size is " + CurrentSize, cLogger.TipoLog.Debug);
            List<cCacheKey> keys = base.Keys.ToList();

            foreach (cCacheKey key in keys)
            {
                cCacheValue<T> Value = base[key];
                if (DateTime.Now > Value.ExpirationTime)
                {
                    Remove(key);
                    cLogger.WriteLine("StartService:cCache: DeleteExpired " + Value.Size + " of bytes", cLogger.TipoLog.Debug);
                }
            }
            cLogger.WriteLine("StartService:cCache: Delete expired task fineshed and  Number of Items = " + Length + ". Current size is" + CurrentSize, cLogger.TipoLog.Debug);
        }
    }

    public void ExplicitClean()
    {
        lock (_lock)
        {
            cLogger.WriteLine("StartService:cCache: ExplicitClean when  Number of Items = " + Length + ". Current size is " + CurrentSize, cLogger.TipoLog.Debug);
            List<cCacheKey> keys = base.Keys.ToList();

            foreach (cCacheKey key in keys)
            {
                cCacheValue<T> Value = base[key];
                Remove(key);
            }
            cLogger.WriteLine("StartService:cCache: ExplicitClean fineshed and  Number of Items = " + Length + ". Current size is" + CurrentSize, cLogger.TipoLog.Debug);
        }
    }
}

public static class Binary
{
    /// <summary>
    /// Convert an object to a Byte Array.
    /// </summary>
    public static byte[] ObjectToByteArray(object objData)
    {
        if (objData == null)
            return default;

        return Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(objData, GetJsonSerializerOptions()));
    }

    /// <summary>
    /// Convert a byte array to an Object of T.
    /// </summary>
    public static T ByteArrayToObject<T>(byte[] byteArray)
    {
        if (byteArray == null || !byteArray.Any())
            return default;

        return System.Text.Json.JsonSerializer.Deserialize<T>(byteArray, GetJsonSerializerOptions());
    }

    private static System.Text.Json.JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new System.Text.Json.JsonSerializerOptions()
        {
            PropertyNamingPolicy = null,
            WriteIndented = true,
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };
    }
}