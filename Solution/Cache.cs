﻿namespace Solution;
public class CacheValue<T>
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

    public bool IsUnitSize
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

    public bool IsZipped
    { get; set; }

    public CacheValue(T Value, int ExpirationThreshold, bool zip = false, bool isUnitSize = false)
    {
        IsZipped = zip;
        IsUnitSize = isUnitSize;
        if (IsZipped)
        {
            _value = default;
            ZippedValue = SerializeAndCompress(Value);
            Size = ZippedValue.Length; //GetSize(ZippedValue);
        }
        else
        {
            ZippedValue = null;
            _value = Value;
            Size = GetSize(_value);
        }
        if (ExpirationThreshold != 0)
            ExpirationTime = DateTime.Now.AddMinutes(ExpirationThreshold);
        else
            ExpirationTime = DateTime.MaxValue;
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

public class CacheKey
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

    public CacheKey(string QueryString)
    {
        this.QueryString = GetHash(QueryString);
        LastUsedTime = DateTime.Now;
    }

    private static string GetHash(string inputString)
    {
        // step 1, calculate MD5 hash from input
        MD5 md5 = MD5.Create();
        byte[] inputBytes = Encoding.ASCII.GetBytes(inputString);
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
        return Equals(obj as CacheKey);
    }
    public bool Equals(CacheKey obj)
    {
        return QueryString.Equals(obj.QueryString);
    }
}

public class CacheManager
{
    private Dictionary<string, object> caches
    { get; set; }
    public CacheManager(XML oXML)
    {
        Init(oXML);
    }
    public CacheManager(string sConfigXML)
    {
        XML oXML = new XML();
        oXML.Load(sConfigXML);
        Init(oXML);
    }

    private void Init(XML oXML)
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
            Type cacheGenericType = typeof(Cache<>);
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

    public Cache<DataTable> getDataTableCache(string cacheName)
    {
        object cacheObject = caches[cacheName];
        if (cacheObject != null && cacheObject.GetType() == typeof(Cache<DataTable>))
            return (Cache<DataTable>)cacheObject;
        else
            return null;
    }

    public Cache<string> getStringCache(string cacheName)
    {
        object cacheObject = caches[cacheName];
        if (cacheObject != null && cacheObject.GetType() == typeof(Cache<string>))
            return (Cache<string>)cacheObject;
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

public class Cache<T> : Dictionary<CacheKey, CacheValue<T>>
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

    public Cache(long CacheSize, int TrashPeriod, bool IsZipped = false, bool IsUnitSize = false) : base()
    {
        Logger.WriteLine("StartService:cCache: Creating new cache Object of maximum bytes=" + CacheSize, Logger.TipoLog.Debug);
        _lock = new object();
        MaximumSize = CacheSize;
        CurrentSize = 0;
        Length = 0;
        IsUnit = IsUnitSize;
        IsZip = IsZipped;
        TimerCallback tcb = DeleteExpired;
        object timerState = new object();
        Logger.WriteLine("Creating timer of " + TrashPeriod + " minutes for ThrashPeriod", Logger.TipoLog.Debug);
        stateTimer = new Timer(tcb, timerState, TrashPeriod * 60 * 1000, TrashPeriod * 60 * 1000);
    }

    public new List<CacheKey> Keys()
    {
        return base.Keys.ToList();
    }

    public void Set(string sKey, T Value, int cacheThreeshold = 0)
    {
        Add(new CacheKey(sKey), new CacheValue<T>(Value, cacheThreeshold, IsZip, IsUnit));
    }

    public new void Add(CacheKey Key, CacheValue<T> Value)
    {
        Logger.WriteLine("StartService:cCache: adding new value=" + Key.QueryString, Logger.TipoLog.Debug);
        lock (_lock)
        {
            if (ContainsKey(Key))
            {
                Logger.WriteLine("StartService:cCache: key already present. Removing the oldest", Logger.TipoLog.Debug);
                Remove(Key);
            }
            long newSize = CurrentSize + Value.Size;
            Logger.WriteLine("StartService:cCache: Number of Items = " + Length + ". Current size is " + CurrentSize + ". The new size will be =" + newSize, Logger.TipoLog.Debug);
            if (CurrentSize + Value.Size > MaximumSize)
            {
                Logger.WriteLine("StartService:cCache: size needed not sufficient. It will be cleaned", Logger.TipoLog.Debug);
                if (!Recycler(Value.Size))
                {
                    Logger.WriteLine("StartService:cCache: unable to store cache object. Current cache size is=" + CurrentSize + " while object size is=" + Value.Size, Logger.TipoLog.Warn);
                    return;
                }
            }
            Logger.WriteLine("StartService:cCache: Adding Value = " + Value.Value.ToString(), Logger.TipoLog.Debug);
            base.Add(Key, Value);
            //still newSize computing because after recycler currentSize could be changed
            newSize = CurrentSize + Value.Size;
            Length++;
            CurrentSize = newSize;
        }
    }

    public bool Remove(string sKey)
    {
        return Remove(new CacheKey(sKey));
    }

    public new bool Remove(CacheKey Key)
    {
        Logger.WriteLine("StartService:cCache: removing value=" + Key.QueryString, Logger.TipoLog.Debug);
        bool bRet = false;
        lock (_lock)
        {
            if (ContainsKey(Key))
            {
                bRet = true;
                CacheValue<T> Value = this[Key];
                long newSize = CurrentSize - Value.Size;
                Logger.WriteLine("StartService:cCache: Number of Items = " + Length + ". Current size is " + CurrentSize + ". The new size will be =" + newSize, Logger.TipoLog.Debug);
                base.Remove(Key);
                CurrentSize = newSize;
                Length--;
            }
        }
        return bRet;
    }

    public T Get(string sKey)
    {
        return get(new CacheKey(sKey));
    }

    public T get(CacheKey Key)
    {
        Logger.WriteLine("StartService:cCache: getting value=" + Key.QueryString, Logger.TipoLog.Debug);
        T Value = default;
        lock (_lock)
        {
            if (ContainsKey(Key))
            {
                Logger.WriteLine("StartService:cCache: value is present", Logger.TipoLog.Debug);
                CacheValue<T> TmpValue = this[Key];
                if (DateTime.Now <= TmpValue.ExpirationTime)
                {
                    Value = TmpValue.Value;
                    Logger.WriteLine("StartService:cCache: The values is not expired", Logger.TipoLog.Debug);
                    //Remove key with the oldest timestamp
                    base.Remove(Key);
                    //Add the same key with the new timestamp
                    base.Add(Key, TmpValue);
                    Logger.WriteLine("StartService:cCache: getting Value = " + Value.ToString(), Logger.TipoLog.Debug);
                }
                else
                {
                    Logger.WriteLine("StartService:cCache: The values is expired. I'll remove it.", Logger.TipoLog.Debug);
                    Remove(Key);
                }
            }
        }
        return Value;
    }

    private bool Recycler(long Bytes)
    {
        Logger.WriteLine("StartService:cCache: Recycled invoked to get " + Bytes + " bytes", Logger.TipoLog.Debug);
        bool retValue = false;

        lock (_lock)
        {
            DeleteExpired(null);

            if (MaximumSize - CurrentSize >= Bytes)
            {
                retValue = true;
            }
            else
            {
                List<CacheKey> keys = base.Keys.ToList();
                List<CacheKey> orderedKeys = keys.OrderBy(x => x.LastUsedTime).ToList();
                foreach (CacheKey key in orderedKeys)
                {
                    CacheValue<T> Value = base[key];
                    Remove(key);
                    Logger.WriteLine("StartService:cCache: Recycled " + Value.Size + " of bytes because oldest even if it isn't expired", Logger.TipoLog.Debug);
                    if (MaximumSize - CurrentSize >= Bytes)
                    {
                        retValue = true;
                        break;
                    }
                }
            }
        }
        Logger.WriteLine("StartService:cCache: Recycled invoked to get " + Bytes + " bytes=" + retValue, Logger.TipoLog.Debug);
        return retValue;
    }

    public void DeleteExpired(object stateInfo)
    {
        lock (_lock)
        {
            Logger.WriteLine("StartService:cCache: Delete expired task when  Number of Items = " + Length + ". Current size is " + CurrentSize, Logger.TipoLog.Debug);
            List<CacheKey> keys = base.Keys.ToList();

            foreach (CacheKey key in keys)
            {
                CacheValue<T> Value = base[key];
                if (DateTime.Now > Value.ExpirationTime)
                {
                    Remove(key);
                    Logger.WriteLine("StartService:cCache: DeleteExpired " + Value.Size + " of bytes", Logger.TipoLog.Debug);
                }
            }
            Logger.WriteLine("StartService:cCache: Delete expired task fineshed and  Number of Items = " + Length + ". Current size is" + CurrentSize, Logger.TipoLog.Debug);
        }
    }

    public void ExplicitClean()
    {
        lock (_lock)
        {
            Logger.WriteLine("StartService:cCache: ExplicitClean when  Number of Items = " + Length + ". Current size is " + CurrentSize, Logger.TipoLog.Debug);
            List<CacheKey> keys = base.Keys.ToList();

            foreach (CacheKey key in keys)
            {
                CacheValue<T> Value = base[key];
                Remove(key);
            }
            Logger.WriteLine("StartService:cCache: ExplicitClean fineshed and  Number of Items = " + Length + ". Current size is" + CurrentSize, Logger.TipoLog.Debug);
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