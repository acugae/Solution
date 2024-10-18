using Microsoft.AspNetCore.Components;

namespace Solution.Reflection;
/// <summary>
/// Classe utilizzata per il recuperano di informazioni su assembly, moduli, membri, parametri e altre entità nel codice gestito esaminandone i metadati. 
/// </summary>
public class ReflectionManager
{
    /// <summary>
    /// Chiama il metodo dell'oggetto specificato.
    /// </summary>
    /// <param name="oObject">Oggetto su cui chiamare il metodo.</param>
    /// <param name="strMethodName">Nome del metodo da chiamare.</param>
    /// <param name="oParmas">Parametri da passare al metodo.</param>
    /// <returns>Valore di ritorno.</returns>
    public object CallMethod(object oObject, string strMethodName, params object[] oParmas)
    {
        try
        {
            MethodInfo myMethod = GetMethod(oObject, strMethodName, oParmas);
            if (oParmas != null && oParmas.Length > 0)
                return myMethod.Invoke(oObject, oParmas);
            else
                return myMethod.Invoke(oObject, null);
        }
        catch (Exception err)
        {
            throw new Exception(err.Message);
        }
    }
    /// <summary>
    /// Ritorna i tipi dei parametri in input ad un metodo.
    /// </summary>
    /// <param name="oMethod">Metodo da analizzare.</param>
    /// <returns>Tipi in input al metodo.</returns>
    public Type[] GetInputParamsMethod(MethodInfo oMethod)
    {
        ParameterInfo[] ovPI = oMethod.GetParameters();
        if (ovPI == null)
            return null;
        Type[] oType = new Type[ovPI.Length];
        for (int i = 0; i < ovPI.Length; i++)
        {
            oType[i] = ovPI[i].ParameterType;
        }
        return oType;
    }

    public DataTable GetDataTable(Object oObject, string[] svColumn)
    {
        DataTable oDTResult = new DataTable();
        //
        for (int i = 0; i < svColumn.Length; i++)
        {
            oDTResult.Columns.Add(svColumn[i]);
        }
        //
        if (oObject.GetType().IsArray)
        {
            object[] arr = oObject as object[];
            //cGCollection<object, object> oValueResult = new cGCollection<object, object>();
            for (int i = 0; i < arr.Length; i++)
            {
                DataRow oDR = oDTResult.NewRow();
                for (int j = 0; j < svColumn.Length; j++)
                {
                    oDR[svColumn[j]] = GetPropertiesValue(arr[i])[svColumn[j]];
                }
                oDTResult.Rows.Add(oDR);
            }
        }
        else
        {
            DataRow oDR = oDTResult.NewRow();
            for (int j = 0; j < svColumn.Length; j++)
            {
                oDR[svColumn[j]] = GetPropertiesValue(oObject)[svColumn[j]];
            }
            oDTResult.Rows.Add(oDR);
        }
        return oDTResult;
    }

    /// <summary>
    /// Ritorna "true" se l'oggetto selezionato contiene unmetodo chiamato "sNameMethod", altrimenti ritorna "false".
    /// E' necessario utilizzare quasto metodo quando sono presenti overloding.
    /// </summary>
    /// <param name="oObject">Oggetto da analizzare.</param>
    /// <param name="sNameMethod">Metodo specificato.</param>
    /// <returns>Variabile indicante la presenza del metodo.</returns>
    public bool ExistMethod(Object oObject, string sNameMethod)
    {
        try
        {
            MethodInfo[] myMs = GetMethods(oObject);
            for (int i = 0; myMs != null && i < myMs.Length; i++)
            {
                if (myMs[i].Name.Equals(sNameMethod))
                    return true;
            }
            return false;
        }
        catch (Exception err)
        {
            throw new Exception(err.Message);
        }
    }
    /// <summary>
    /// Restituisce i metodi (Statici e Pubblici) di un oggetto.
    /// </summary>
    /// <param name="oObject">Oggetto da analizzare.</param>
    /// <returns>Array contenente i metodi dell'oggetto.</returns>
    public MethodInfo[] GetMethods(Object oObject)
    {
        try
        {
            MethodInfo[] myMethods = oObject.GetType().GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance);
            return myMethods;
        }
        catch (Exception err)
        {
            throw new Exception(err.Message);
        }
    }
    /// <summary>
    /// Restituisce il metodo di un oggetto.
    /// </summary>
    /// <param name="oObject">Oggetto da analizzare.</param>
    /// <param name="sNameMethod">Nome del metodo da ricercare.</param>
    /// <param name="oParams">Paramatri in input.</param>
    /// <returns>Metodo dell'oggetto.</returns>
    public MethodInfo GetMethod(Object oObject, string sNameMethod, object[] oParams)
    {
        try
        {
            if (oParams != null && oParams.Length > 0)
            {
                Type[] oA = new Type[oParams.Length];
                for (int i = 0; i < oA.Length; i++)
                {
                    if (oParams[i] == null)
                        oA[i] = Missing.Value.GetType();
                    else
                        oA[i] = oParams[i].GetType();
                }
                return oObject.GetType().GetMethod(sNameMethod, oA);
            }
            else
            {
                return oObject.GetType().GetMethod(sNameMethod, BindingFlags.Public | BindingFlags.Instance);
            }
        }
        catch (Exception err)
        {
            throw new Exception(err.Message);
        }
    }

    /// <summary>
    /// Imposta un valore di una proprieta su di un oggetto specificato.
    /// </summary>
    /// <param name="oObject">Oggeto su cui impostare la proprietà.</param>
    /// <param name="strPropertyName">Nome della da impostare.</param>
    /// <param name="oValue">Valore da impostare.</param>
    public void CallPropertySet(Object oObject, string strPropertyName, object oValue)
    {
        try
        {
            PropertyInfo myProperty = oObject.GetType().GetProperty(strPropertyName);
            if (oValue == null || oValue == DBNull.Value)
                return;
            myProperty.SetValue(oObject, oValue, null);
        }
        catch (Exception err)
        {
            throw new Exception(err.Message);
        }
    }
    /// <summary>
    /// Restituisce il valore della proprietà spacificata.
    /// </summary>
    /// <param name="oObject">Oggetto di riferimento.</param>
    /// <param name="strPropertyName">Nome della proprieta.</param>
    /// <returns>Oggetto di ritorno della proprietà.</returns>
    public object CallPropertyGet(Object oObject, string strPropertyName)
    {
        try
        {
            PropertyInfo myProperty = oObject.GetType().GetProperty(strPropertyName);
            if (myProperty.CanRead)
                return myProperty.GetValue(oObject, null);
            return null;
        }
        catch (Exception err)
        {
            throw new Exception(err.Message);
        }
    }
    /// <summary>
    /// Restituisce le proprieta dell'oggetto specificato.
    /// </summary>
    /// <param name="oObject">Oggetto di riferimento.</param>
    /// <param name="bindingAttr">Specifica i flag che controllano l'associazione e il modo in cui è condotta la ricerca.</param>
    /// <returns>Collezione di PropertyInfo.</returns>
    public GCollection<string, PropertyInfo> GetProperties(Object oObject, BindingFlags bindingAttr)
    {
        try
        {
            PropertyInfo[] myProperties = oObject.GetType().GetProperties(bindingAttr);
            GCollection<string, PropertyInfo> oPs = new GCollection<string, PropertyInfo>();
            for (int i = 0; i < myProperties.Length; i++)
            {
                oPs.Add(myProperties[i].Name, myProperties[i]);
            }
            return oPs;
        }
        catch (Exception err)
        {
            throw new Exception(err.Message);
        }
    }
    /// <summary>
    /// Ritorna la proprietà di un oggetto.
    /// </summary>
    /// <param name="oObject">Oggeto da analizzare.</param>
    /// <param name="sPropertyName">Nome della proprietà.</param>
    /// <returns></returns>
    public PropertyInfo GetProperty(Object oObject, string sPropertyName)
    {
        try
        {
            PropertyInfo myProperty = oObject.GetType().GetProperty(sPropertyName);
            return myProperty;
        }
        catch (Exception err)
        {
            throw new Exception(err.Message);
        }
    }
    /// <summary>
    /// Verifica se la proprietà è un parametro.
    /// </summary>
    public bool IsParameter(PropertyInfo property)
    {
        // Verifica se la proprietà ha l'attributo [Parameter] o [CascadingParameter]
        return property.GetCustomAttributes(typeof(ParameterAttribute), true).Any() ||
               property.GetCustomAttributes(typeof(CascadingParameterAttribute), true).Any();
    }
    /// <summary>
    /// Ritorna gli eventi di un oggetto.
    /// </summary>
    /// <param name="oObject">Oggeto da analizzare.</param>
    /// <param name="bindingAttr">Specifica i flag che controllano l'associazione e il modo in cui è condotta la ricerca.</param>
    /// <returns>Collezione di EventInfo.</returns>
    public GCollection<string, EventInfo> GetEvents(Object oObject, BindingFlags bindingAttr)
    {
        try
        {
            EventInfo[] myEvents = oObject.GetType().GetEvents(bindingAttr);
            GCollection<string, EventInfo> oPs = new GCollection<string, EventInfo>();
            for (int i = 0; i < myEvents.Length; i++)
            {
                oPs.Add(myEvents[i].Name, myEvents[i]);
            }
            return oPs;
        }
        catch (Exception err)
        {
            throw new Exception(err.Message);
        }
    }
    /// <summary>
    /// Ritorna l'evento di un oggeto. 
    /// </summary>
    /// <param name="oObject">Oggetto specificato.</param>
    /// <param name="sNameEvent">Nome dell'evento.</param>
    /// <returns>EventInfo di riferimento.</returns>
    public EventInfo GetEvent(Object oObject, string sNameEvent)
    {
        try
        {
            EventInfo myEvents = oObject.GetType().GetEvent(sNameEvent);
            return myEvents;
        }
        catch (Exception err)
        {
            throw new Exception(err.Message);
        }
    }
    /// <summary>
    /// Ritorna gli eventi di un oggeto, saranno ritornato solo gli eventi di tipo "sTypeEvent".
    /// </summary>
    /// <param name="oObject">Oggetto specificato.</param>
    /// <param name="sTypeEvent">Nome del tipo di Eventi.</param>
    /// <returns>Array di EventInfo.</returns>
    public EventInfo[] GetEventsFromType(Object oObject, string sTypeEvent)
    {
        try
        {
            EventInfo[] myEvents = oObject.GetType().GetEvents();
            ArrayList oArr = new ArrayList();
            for (int i = 0; i < myEvents.Length; i++)
            {
                if (myEvents[i].EventHandlerType.Name.Equals(sTypeEvent))
                    oArr.Add((EventInfo)myEvents[i]);
            }
            //
            if (oArr.Count <= 0)
                return null;
            //
            myEvents = new EventInfo[oArr.Count];
            for (int i = 0; i < oArr.Count; i++)
            {
                myEvents[i] = (EventInfo)oArr[i];
            }
            return myEvents;
        }
        catch (Exception err)
        {
            throw new Exception(err.Message);
        }
    }
    /// <summary>
    /// Crea un delegato per la gestione di un evento.
    /// </summary>
    /// <param name="oObject">Oggetto contenente l'evento.</param>
    /// <param name="sEvent">Evento specificato.</param>
    /// <param name="oObjectManager">Oggetto contenente il metodo da eseguire allo scaturire dell'evento.</param>
    /// <param name="oMethodManager">Metodo specificato.</param>
    /// <returns>Delefato creato.</returns>
    public Delegate AddEventHandler(Object oObject, string sEvent, Object oObjectManager, string oMethodManager)
    {
        try
        {
            MethodInfo oMethodInfo = GetMethod(oObjectManager, oMethodManager, null);
            EventInfo myEvent = GetEvent(oObject, sEvent);
            Delegate oDelegate = Delegate.CreateDelegate(myEvent.EventHandlerType, oObjectManager, oMethodInfo);
            myEvent.AddEventHandler(oObject, oDelegate);
            return oDelegate;
        }
        catch (Exception err)
        {
            throw new Exception(err.Message);
        }
    }
    /// <summary>
    /// Rimuove il delegato per la gestione di un evento.
    /// </summary>
    /// <param name="oObject">Oggetto contenente l'evento.</param>
    /// <param name="sEvent">Evento specificato.</param>
    /// <param name="oDelegate">Delegato specificato.</param>
    public void RemoveEventHandler(Object oObject, string sEvent, Delegate oDelegate)
    {
        try
        {
            EventInfo myEvent = GetEvent(oObject, sEvent);
            myEvent.RemoveEventHandler(oObject, oDelegate);
        }
        catch (Exception err)
        {
            throw new Exception(err.Message);
        }
    }
    /// <summary>
    /// Ritorna l'istanza dell'assembli specificato.
    /// </summary>
    /// <param name="strAssemblyName">Nome del file Assembly.</param>
    /// <returns>Istanza dell'oggetto.</returns>
    public Assembly GetAssembly(string strAssemblyName)
    {
        if (!File.Exists(strAssemblyName))
            throw new Exception("DLL not found.");
        Assembly assembly = null;
        try
        {
            assembly = Assembly.LoadFrom(strAssemblyName);
        }
        catch
        {
            throw new Exception("Assembly.LoadFrom error.");
        }
        return assembly;
    }
    /// <summary>
    /// Imposta le proprietà di un oggetto con i valori specificati.
    /// </summary>
    /// <param name="oObject">Oggetto di riferimento.</param>
    /// <param name="PropertyListValue">Collezione di proprietà con valori associati.</param>
    public void SetControlProperties(object oObject, GCollection<string, object> PropertyListValue)
    {
        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(oObject);
        foreach (PropertyDescriptor myProperty in properties)
        {
            if (PropertyListValue.ContainsKey(myProperty.Name))
            {
                try
                {
                    myProperty.SetValue(oObject, PropertyListValue[myProperty.Name]);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
    /// <summary>
    /// Restituisce i valori di tuttle le proprietà dell'oggetto.
    /// </summary>
    /// <param name="oObject">Oggetto di riferimento.</param>
    /// <returns>Collezione di proprietà valori.</returns>
    public GCollection<string, object> GetPropertiesValue(object oObject)
    {
        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(oObject);
        GCollection<string, object> propertyList = new GCollection<string, object>();
        //
        foreach (PropertyDescriptor myProperty in properties)
        {
            try
            {
                if (myProperty.PropertyType.IsSerializable)
                    propertyList.Add(myProperty.Name, myProperty.GetValue(oObject));
            }
            catch (Exception)
            {
                throw;
            }
        }
        return propertyList;
    }
    /// <summary>
    /// Copia le proprieta con lo stesso nome, da un oggetto ad un altro oggetto.
    /// </summary>
    /// <param name="oSource">Oggetto sorgente.</param>
    /// <param name="oDestination">Oggetto destinazione.</param>
    /// <returns>Ritorna l'oggetto destrinazione modificato.</returns>
    public object CopyPropertiesObject(object oSource, object oDestination)
    {
        SetControlProperties(oDestination, GetPropertiesValue(oSource));
        return oDestination;
    }
    /// <summary>
    /// Ritorna l'istanza di classe spacificata. 
    /// </summary>
    /// <param name="strAssemblyName">Assembly di riferimento.</param>
    /// <param name="strClassName">Classe da selezionare.</param>
    /// <param name="oParamsConstructor">Paramentri in input al construttore.</param>
    /// <returns>Istanza di classe.</returns>
    public object GetObject(string strAssemblyName, string strClassName, params object[] oParamsConstructor)
    {
        Assembly assembly = GetAssembly(strAssemblyName);
        if (assembly == null)
            throw new Exception("Assembly not found.");
        try
        {
            Type oType = assembly.GetType(strClassName);
            if (oType.IsClass)
            {
                try
                {
#if (!MOBILE)
                    if (oParamsConstructor == null)
                        return Activator.CreateInstance(oType);
                    return Activator.CreateInstance(oType, oParamsConstructor);
#else
                        return Activator.CreateInstance(oType);
#endif
                }
                catch (Exception err)
                {
                    throw new Exception(err.Message);
                }
            }
            throw new Exception("Oggetto non riconosciuto");
        }
        catch (Exception err)
        {
            throw new Exception(err.Message);
        }
    }
#if (!MOBILE)
    /// <summary>
    /// Ritorna "True" se l'oggetto implemeta l'interfaccia strInterface, altrimente "false"
    /// </summary>
    /// <param name="oObject">Oggetto per il controllo</param>
    /// <param name="strInterface">Nome dell'interfaccia</param>
    /// <returns></returns>
    public bool IsImplemented(object oObject, string strInterface)
    {
        Type oType = oObject.GetType().GetInterface(strInterface, true);
        return oType == null ? false : true;
    }
#endif
    /// <summary>
    /// Ritorna tutte le interfacce contenute in un assembly.
    /// </summary>
    /// <param name="strAssemblyName">Assembly specificato.</param>
    /// <returns>Interfacce contenute nell'assembly.</returns>
    public object[] GetInterfaces(string strAssemblyName)
    {
        Assembly assembly = GetAssembly(strAssemblyName);
        if (assembly == null)
            throw new Exception("Assembly not found.");
        //
        try
        {
            Type[] oTypes = assembly.GetTypes();
            ArrayList ovObject = new ArrayList();
            for (int i = 0; i < oTypes.Length; i++)
            {
                if (oTypes[i].IsInterface)
                {
                    try
                    {
                        ovObject.Add(Activator.CreateInstance(oTypes[i]));
                    }
                    catch (Exception err)
                    {
                        throw new Exception(err.Message);
                    }
                }
            }
            if (ovObject.Count > 0)
                return ovObject.ToArray();
            return null;
        }
        catch (Exception err)
        {
            throw new Exception(err.Message);
        }
    }
    /// <summary>
    /// Converte un arrey di bytes in una scringa esadecimale.
    /// </summary>
    /// <param name="buff">Array di bytes.</param>
    /// <returns>Stringa esadecimale.</returns>
    public string BinaryToHex(byte[] buff)
    {
        string tmpstring = "";
        foreach (byte var in buff)
        {
            tmpstring += Convert.ToString(var, 16).PadLeft(2, '0');
        }
        return tmpstring.Trim().ToUpper();
    }
    /// <summary>
    /// Converte una stringa esadecimale in bytes.
    /// </summary>
    /// <param name="HexValue">Stringa esadecimale.</param>
    /// <returns>Arrey di bytes.</returns>
    public byte[] HexToBinary(string HexValue)
    {
        if ((HexValue.Length % 2) == 1)
            return null;
        string[] tmpary = new string[HexValue.Length / 2]; //HexValue.Trim().Split(' ');
        for (int i = 0, index = 0; i < HexValue.Length; i += 2, index++)
        {
            tmpary[index] = HexValue.Substring(i, 2);
        }
        byte[] buff = new byte[tmpary.Length];
        for (int i = 0; i < buff.Length; i++)
        {
            buff[i] = Convert.ToByte(tmpary[i], 16);
        }
        return buff;
    }
    /// <summary>
    /// Converte a Byte Array of Unicode values (UTF-8 encoded) to a complete String.
    /// </summary>
    /// <param name="characters">Unicode Byte Array to be converted to String</param>
    /// <returns>String converted from Unicode Byte Array</returns>
    private String UTF8ByteArrayToString(Byte[] characters)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        String constructedString = encoding.GetString(characters, 0, characters.Length);
        return (constructedString);
    }
    /// <summary>
    /// Converts the String to UTF8 Byte array and is used in De serialization
    /// </summary>
    /// <param name="pXmlString"></param>
    /// <returns></returns>
    private Byte[] StringToUTF8ByteArray(String pXmlString)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        Byte[] byteArray = encoding.GetBytes(pXmlString);
        return byteArray;
    }
    /// <summary>
    /// Converte l'oggetto specificato in una stringa XML.
    /// </summary>
    /// <param name="pObject">Oggetto che deve essere serializzato.</param>
    /// <returns>XML string</returns>
    public String XMLSerialize(Object oObject)
    {
        try
        {
            String XmlizedString = null;
            MemoryStream memoryStream = new MemoryStream();
            XmlSerializer xs = new XmlSerializer(oObject.GetType());
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xs.Serialize(xmlTextWriter, oObject);
            memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
            XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());
            return XmlizedString;
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e);
            return null;
        }
    }
    /// <summary>
    /// Ricostruisce l'oggetto dalla stringa XML specificata.
    /// </summary>
    /// <param name="sXmlSerialized">Stringa da deserializzare.</param>
    /// <param name="oType">Tipo dell'oggetto da ritornare.</param>
    public Object DeserializeObject(String sXmlSerialized, Type oType)
    {
        XmlSerializer xs = new XmlSerializer(oType);
        MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(sXmlSerialized));
        XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
        return xs.Deserialize(memoryStream);
    }
    /// <summary>
    /// Serializza un oggeto il un buffer di bytes.
    /// </summary>
    /// <param name="oObject">Oggetto da deserializzare.</param>
    /// <returns>Buffer contenente l'oggetto serializzato.</returns>
    public byte[] XMLSerializeToBytes(object oObject)
    {
        try
        {
            MemoryStream writer = new MemoryStream();
            XmlSerializer serializer = new XmlSerializer(oObject.GetType());
            serializer.Serialize(writer, oObject);
            writer.Close();
            return writer.ToArray();
        }
        catch (Exception err)
        {
            throw (err);
        }
    }
    /// <summary>
    /// Deserializza un oggetto in XML.
    /// </summary>
    /// <param name="bytsStream">Buffer contenente l'oggeto serializzato.</param>
    /// <param name="oType">Tipo della deserializzazione.</param>
    /// <returns>Oggetto deserializzato.</returns>
    public object XMLDeserializeFromBytes(byte[] bytsStream, Type oType)
    {
        try
        {
            object oObject = null;
            MemoryStream reader = new MemoryStream(bytsStream);
            XmlSerializer serializer = new XmlSerializer(oType);
            oObject = serializer.Deserialize(reader);
            reader.Close();
            return oObject;
        }
        catch (Exception err)
        {
            throw (err);
        }
    }
    /// <summary>
    /// Serializza un oggetto su di un file.
    /// </summary>
    /// <param name="strFileName">File su cui effettuare la serializzazione.</param>
    /// <param name="oObject">Oggeto da serializzare.</param>
    public void XMLSerialize(string strFileName, object oObject)
    {
        try
        {
            StreamWriter writer = new StreamWriter(strFileName);
            XmlSerializer serializer = new XmlSerializer(oObject.GetType());
            serializer.Serialize(writer, oObject);
            writer.Close();
        }
        catch (Exception err)
        {
            throw (err);
        }
    }
    /// <summary>
    /// Deserializza un oggetto da un file.
    /// </summary>
    /// <param name="strFileName">File specificato.</param>
    /// <param name="oType">Tipo da deserializzare.</param>
    /// <returns>Oggeto deserializzato.</returns>
    public object XMLDeserialize(string strFileName, Type oType)
    {
        try
        {
            StreamReader reader = new StreamReader(strFileName);
            XmlSerializer serializer = new XmlSerializer(oType);
            object oObject = serializer.Deserialize(reader);
            reader.Close();
            return oObject;
        }
        catch (Exception err)
        {
            throw (err);
        }
    }
#if (!MOBILE)
    /// <summary>
    /// Serializza l'oggeto specificato in un buffer.
    /// </summary>
    /// <param name="oObjectType">Oggeto da serializzare.</param>
    /// <returns>Buffer contenente la serializzazione dell'oggeto.</returns>
    public byte[] BinarySerialize(object oObjectType)
    {
        try
        {
            //MemoryStream writer = new MemoryStream();
            //BinaryFormatter serializer = new BinaryFormatter();
            //serializer.Serialize(writer, oObjectType);
            //byte[] ovBuffer = writer.ToArray();
            //writer.Close();
            //return ovBuffer;
            return Binary.ObjectToByteArray(oObjectType);
        }
        catch (Exception err)
        {
            throw (err);
        }
    }
    /// <summary>
    /// Deserializza un buffer in un oggetto.
    /// </summary>
    /// <param name="bBuffer">Buffer in input.</param>
    /// <returns>Oggetto deserializzato.</returns>
    public object BinaryDeserialize(byte[] bBuffer)
    {
        try
        {
            //object oObject = null;
            //MemoryStream reader = new MemoryStream(bBuffer);
            //BinaryFormatter serializer = new BinaryFormatter();
            //oObject = serializer.Deserialize(reader);
            //reader.Close();
            //return oObject;
            return Binary.ByteArrayToObject<object>(bBuffer);
        }
        catch (Exception err)
        {
            throw (err);
        }
    }
#endif
}
