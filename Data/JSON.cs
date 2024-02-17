namespace Solution.Data;

public class StringEnumConverter : Newtonsoft.Json.Converters.StringEnumConverter { public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { if (value is Action) { writer.WriteValue(Enum.GetName(typeof(Action), (Action)value)); } base.WriteJson(writer, value, serializer); } }

public class JSON
{
    static public string Serialize(object oObject, JsonSerializerSettings? oSetting = null)
    {
        return JsonConvert.SerializeObject(oObject, Newtonsoft.Json.Formatting.Indented, oSetting);
    }

    static public T Deserialize<T>(string sJson)
    {
        return JsonConvert.DeserializeObject<T>(sJson);
    }

    static public T Deserialize<T>(string sJson, string sPath, string? sDefault = null)
    {
        string ssJson = Parse(sJson, sPath, sDefault);
        return JsonConvert.DeserializeObject<T>(ssJson);
    }

    static public T Deserialize<T>(string sJson, JsonSerializerSettings oSetting)
    {

        return JsonConvert.DeserializeObject<T>(sJson, oSetting);
    }

    static public T Deserialize<T>(string sJson, params JsonConverter[] converter)
    {

        return JsonConvert.DeserializeObject<T>(sJson, converter);
    }

    static public void SerializeToFile(object oObject, string sFileName)
    {
        string sJSON = JsonConvert.SerializeObject(oObject, Newtonsoft.Json.Formatting.Indented);
        FileManager.SetFile(sFileName, sJSON);
    }

    static public T DeserializeFromFile<T>(string sFileName)
    {
        string sFileString = FileManager.GetFile(sFileName);
        return Deserialize<T>(sFileString);
    }

    static public string SerializeDataTable(DataTable oDTDati)
    {
        StringBuilder sResult = new StringBuilder();
        //
        sResult.Append("[");
        for (int i = 0; oDTDati != null && i < oDTDati.Rows.Count; i++)
        {
            sResult.Append((i > 0 ? "," : "") + "{");
            for (int col = 0; col < oDTDati.Columns.Count; col++)
            {
                sResult.Append("\"" + oDTDati.Columns[col].ColumnName.Replace(" ", "_") + "\":" + GetValue(oDTDati.Rows[i][col])); // (oDTDati.Rows[i][col] == DBNull.Value ? "null" : JsonConvert.SerializeObject(oDTDati.Rows[i][col])));
                                                                                                                                   //sResult.Append(oDTDati.Columns[col].ColumnName + ":" + JsonConvert.SerializeObject(oDTDati.Rows[i][col])); //GetValue(oDTDati.Rows[i][col]));
                sResult.Append((col < (oDTDati.Columns.Count - 1)) ? "," : "");
            }
            sResult.Append("}");
        }
        sResult.Append("]");
        return sResult.ToString();
    }

    static public string SerializeDataRow(DataRow oDRDati)
    {
        StringBuilder sResult = new StringBuilder();

        sResult.Append("{");
        for (int col = 0; col < oDRDati.ItemArray.Length; col++)
        {
            sResult.Append("\"" + oDRDati.Table.Columns[col].ColumnName.Replace(" ", "_") + "\":" + JsonConvert.SerializeObject(oDRDati[col] == DBNull.Value ? "" : oDRDati[col])); //GetValue(oDTDati.Rows[i][col]));
            sResult.Append((col < (oDRDati.Table.Columns.Count - 1)) ? "," : "");
        }
        sResult.Append("}");

        return sResult.ToString();
    }

    public static string SerializeJson<T>(T t)
    {
        MemoryStream stream = new MemoryStream();
        DataContractJsonSerializer ds = new DataContractJsonSerializer(typeof(T));
        //DataContractJsonSerializerSettings s = new DataContractJsonSerializerSettings();
        ds.WriteObject(stream, t);
        string jsonString = Encoding.UTF8.GetString(stream.ToArray());
        stream.Close();
        return jsonString;
    }

    public static T DeserializeJson<T>(string jsonString)
    {
        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
        MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
        T obj = (T)ser.ReadObject(stream);
        return obj;
    }


    static private string GetValue(object oValue)
    {
        if (oValue == null || oValue == DBNull.Value)
            return "null";
        //
        switch (oValue.GetType().ToString())
        {
            case "System.Single":
            case "System.Double":
            case "System.Decimal":
                {
                    return oValue.ToString().Replace(",", ".");
                }
            case "System.Byte":
            case "System.Int16":
            case "System.Int32":
            case "System.Int64":
                {
                    return oValue.ToString();
                }
            default:
                {
                    return "\"" + oValue.ToString().Replace(@"\", @"\\").Replace("\"", "\\\"") + "\"";
                }
        }
    }

    static private JToken ParseString(string sJson, string sPath)
    {
        //JObject oResult = JObject.Parse(sJson);
        string[] ovPath = sPath.Split('.');
        JToken oTmp = JToken.Parse(sJson);
        for (int i = 0; i < ovPath.Length; i++)
        {
            bool isLeaf = (i >= (ovPath.Length - 1)) ? true : false;
            try
            {
                string sElement = ovPath[i].Split('[')[0];
                int iIndex = (ovPath[i].Split('[').Length > 1 ? int.Parse(ovPath[i].Split('[')[1].Replace("]", "")) : -1);
                if (iIndex < 0)
                {
                    if (!isLeaf && (oTmp[sElement]).Type.Equals(Newtonsoft.Json.Linq.JTokenType.String))
                    {
                        oTmp = JObject.Parse(oTmp[sElement].ToString());
                    }
                    else
                    {
                        oTmp = oTmp[sElement];
                    }
                }
                else
                {
                    if (!isLeaf && (string.IsNullOrEmpty(sElement) ? oTmp[iIndex] : oTmp[sElement][iIndex]).Type.Equals(Newtonsoft.Json.Linq.JTokenType.String))
                    {
                        oTmp = JObject.Parse(oTmp[sElement].ToString());
                    }
                    else
                    {
                        oTmp = (string.IsNullOrEmpty(sElement) ? oTmp[iIndex] : oTmp[sElement][iIndex]);
                    }
                }
            }
            catch
            { return null; }
        }
        return oTmp;
    }

    static public string Set(string sJson, string sPath, string sValue, string? sType = null)
    {
        GenericObject oObj = new GenericObject();
        oObj.SetJSON(sJson);
        object oValue = sValue;
        //
        if (sType != null && sType.ToLower().Trim().Equals("number"))
        {
            sValue = sValue.Replace(",", ".");
            if (sValue.IndexOf('.') >= 0)
                oValue = float.Parse(sValue);
            else
                oValue = Convert.ToInt32(sValue);
        }
        //
        oObj.SetPathValue(sPath, oValue);
        return oObj.GetJSON();
    }

    static public string Set(string sJson, string sPath, object oValue)
    {
        GenericObject oObj = new GenericObject();
        oObj.SetJSON(sJson);
        oObj.SetPathValue(sPath, oValue);
        return oObj.GetJSON();
    }

    static public string? Parse(string sJson, string sPath, string? sDefault = null)
    {
        JToken oResult = ParseString(sJson, sPath);
        if (oResult == null)
            return sDefault;
        return oResult.ToString();
    }

    static public bool IsArray(string sJson, string sPath)
    {
        JToken oResult = ParseString(sJson, sPath);
        if (oResult.Type == JTokenType.Array)
            return true;
        return false;
    }

    static public int Count(string sJson, string sPath)
    {
        JToken oResult = ParseString(sJson, sPath);
        if (oResult == null)
            return 0;
        return oResult.Count();
    }

    static public bool IncludedInto(string sFull, string sPartial)
    {
        GenericObject oObjCondition = new GenericObject();
        oObjCondition.SetJSON(sPartial.Replace("*", ""));
        Dictionary<string, object> oPath = oObjCondition.GetAllKeys();
        foreach (var oItem in oPath)
        {
            string sValue = JSON.Parse(sFull, oItem.Key);
            if (sValue == null || !sValue.Equals(oItem.Value.ToString()))
                return false;
        }
        return true;
    }

    static public string Merge(string sSource, string sDestination, Dictionary<string, cMap> oMap)
    {
        string sResult = string.Copy(sDestination);
        foreach (var item in oMap)
        {
            string sValueSource = JSON.Parse(sSource, item.Key);
            sResult = JSON.Set(sResult, item.Value.sTarget, sValueSource, item.Value.sType);
        }
        return sResult;
    }

    static public DataTable ToDataTablePrimitive(string json, string sPath)
    {
        bool columnsCreated = false;
        DataTable dt = new DataTable();

        Newtonsoft.Json.Linq.JObject root = Newtonsoft.Json.Linq.JObject.Parse(json);
        Newtonsoft.Json.Linq.JArray items = (Newtonsoft.Json.Linq.JArray)ParseString(json, sPath);   //(Newtonsoft.Json.Linq.JArray)root[tableName];

        Newtonsoft.Json.Linq.JObject item = default(Newtonsoft.Json.Linq.JObject);
        Newtonsoft.Json.Linq.JToken jtoken = default(Newtonsoft.Json.Linq.JToken);

        for (int i = 0; i <= items.Count - 1; i++)
        {
            // Create the columns once
            if (columnsCreated == false)
            {
                item = (Newtonsoft.Json.Linq.JObject)items[i];
                jtoken = item.First;

                while (jtoken != null)
                {
                    dt.Columns.Add(new DataColumn(((Newtonsoft.Json.Linq.JProperty)jtoken).Name.ToString()));
                    jtoken = jtoken.Next;
                }

                columnsCreated = true;
            }

            // Add each of the columns into a new row then put that new row into the DataTable
            item = (Newtonsoft.Json.Linq.JObject)items[i];
            jtoken = item.First;

            // Create the new row, put the values into the columns then add the row to the DataTable
            DataRow dr = dt.NewRow();

            while (jtoken != null)
            {
                if (((Newtonsoft.Json.Linq.JProperty)jtoken).Value.Type == JTokenType.Null)
                    dr[((Newtonsoft.Json.Linq.JProperty)jtoken).Name.ToString()] = DBNull.Value;
                else
                    dr[((Newtonsoft.Json.Linq.JProperty)jtoken).Name.ToString()] = ((Newtonsoft.Json.Linq.JProperty)jtoken).Value.ToString();
                jtoken = jtoken.Next;
            }
            dt.Rows.Add(dr);
        }

        return dt;

    }


    public static void ToDataTableRecursiveMethod(JToken JToken__, bool DeserializeInnerFields__, DataTable dt, DataRow dr, int Level__)
    {
        if (JToken__.Type == JTokenType.Property)
        {
            JProperty prop = (JProperty)JToken__;
            string columnName = prop.Name;
            string columnValue = prop.Value.ToString();

            if (prop.Parent.Type == JTokenType.Object)
            {
                //columnName=((JObject)prop.Parent.First()).Na.P
            }
            if (dt.Columns.Contains(columnName) == false)
            {
                dt.Columns.Add(new DataColumn(columnName));
            }
            if (columnValue != null)
            {
                dr[columnName] = columnValue;
            }
            if (DeserializeInnerFields__)
            {
                if (columnValue.Trim().StartsWith("{") == true && columnValue.Trim().EndsWith("}") == true)
                {
                    if (Level__ >= 2)
                    {
                        JObject propNew = (JObject)prop.Value;
                        ToDataTableRecursiveMethod(propNew, DeserializeInnerFields__, dt, dr, Level__ + 1);
                    }
                }

                if (columnValue.Trim().StartsWith("[") == true && columnValue.Trim().EndsWith("]") == true)
                {
                    JArray jaIn = (JArray)prop.Value;
                    JObject propNew = (JObject)jaIn[0];
                    ToDataTableRecursiveMethod(propNew, DeserializeInnerFields__, dt, dr, Level__ + 1);
                }
            }
        }
        else if (JToken__.Type == JTokenType.Array)
        {

            Newtonsoft.Json.Linq.JArray items = (JArray)JToken__;
            for (int i = 0; i <= items.Count - 1; i++)
            {
                DataRow newRow = dt.NewRow();
                dt.Rows.Add(newRow);

                JToken obj = (JToken)items[i];

                ToDataTableRecursiveMethod(obj, DeserializeInnerFields__, dt, newRow, Level__ + 1);//,null);
                                                                                                   //// Newtonsoft.Json.Linq.JObject item = default(Newtonsoft.Json.Linq.JObject);
                                                                                                   //// item = (Newtonsoft.Json.Linq.JObject)items[i];
                                                                                                   //// objectList.Add(item);

                //// rObj.ObjectList = objectList;
            }
        }


        else if (JToken__.Type == JTokenType.Object)
        {
            List<JToken> jList = JToken__.Children().ToList();
            if (jList.Count > 0)
            {
                DataRow drRow = null;
                if (dr != null)
                {
                    drRow = dr;
                }
                else
                {
                    drRow = dt.NewRow();
                    dt.Rows.Add(drRow);
                }



                for (int i = 0; i <= jList.Count - 1; i++)
                {




                    JToken obj = jList[i];


                    //if (Level__ <= 1 || (Level__ >= 2 && DeserializeInnerFields__ == true))
                    //{
                    ToDataTableRecursiveMethod(obj, DeserializeInnerFields__, dt, drRow, Level__ + 1);//,null);
                                                                                                      //}



                    /*
                    if (dr != null)
                    {

                    }
                    else
                    {

                        ToDataTableRecursiveMethod(obj, true, dt, rowNew);//,null);
                    }
                    */

                    //// Newtonsoft.Json.Linq.JObject item = default(Newtonsoft.Json.Linq.JObject);
                    //// item = (Newtonsoft.Json.Linq.JObject)items[i];
                    //// objectList.Add(item);

                    //// rObj.ObjectList = objectList;
                }
            }


        }

        /*
    else  (JToken__.Type == JTokenType.String && JToken__.ToString().StartsWith("{") == true  && JToken__.ToString().EndsWith("}")
    {

    }
    */

        //else if (JToken__.Type == JTokenType.Object)
        //{

        /*
        if (TokenLabel__.Length > 0)
        {


        }


        foreach (JProperty jp in JToken__. .Properties())
        {
            string name = jp.Name.ToString();
            string value = jp.Value.ToString();
        }
        */
        //Newtonsoft.Json.Linq.JProperty items = JObject__.Properties();


        //for (int i = 0; i <= items.Count - 1; i++)
        // {
        //  JObject obj = (JObject)items[i];

        // ToDataTableRecursiveMethod(obj, "", true, dt);//,null);
        // // Newtonsoft.Json.Linq.JObject item = default(Newtonsoft.Json.Linq.JObject);
        //// item = (Newtonsoft.Json.Linq.JObject)items[i];
        //// objectList.Add(item);

        // // rObj.ObjectList = objectList;
        //}
        //}




        // return rList;
    }

    static public DataTable ToDataTable(string json, string sPath, bool deserializeInnerFields)
    {
        if (!deserializeInnerFields)
            return ToDataTablePrimitive(json, sPath);

        DataTable dt = new DataTable();
        Newtonsoft.Json.Linq.JObject root = Newtonsoft.Json.Linq.JObject.Parse(json);
        Newtonsoft.Json.Linq.JToken jtk = root.SelectToken(sPath);


        if (jtk.Type == JTokenType.String)
        {
            Newtonsoft.Json.Linq.JObject objApp = Newtonsoft.Json.Linq.JObject.Parse(jtk.ToString());
            jtk = objApp.SelectToken("");
        }

        ToDataTableRecursiveMethod(jtk, deserializeInnerFields, dt, null, 0);
        return dt;

    }



}

public static class MergeExtensions
{
    /// <summary>
    /// <para>Creates a new token which is the merge of the passed tokens</para>
    /// </summary>
    /// <param name="left">Token</param>
    /// <param name="right">Token to merge, overwriting the left</param>
    /// <param name="options">Options for merge</param>
    /// <returns>A new merged token</returns>
    public static JToken Merge(this JToken left, JToken right, object options)
    {
        if (left.Type != JTokenType.Object)
            return right.DeepClone();

        var leftClone = (JContainer)left.DeepClone();
        MergeInto(leftClone, right, options);

        return leftClone;
    }

    /// <summary>
    /// <para>Creates a new token which is the merge of the passed tokens</para>
    /// <para>Default options are used</para>
    /// </summary>
    /// <param name="left">Token</param>
    /// <param name="right">Token to merge, overwriting the left</param>
    /// <returns>A new merged token</returns>
    public static JToken Merge(this JToken left, JToken right)
    {
        return Merge(left, right, null);
    }

    /// <summary>
    /// <para>Merge the right token into the left</para>
    /// </summary>
    /// <param name="left">Token to be merged into</param>
    /// <param name="right">Token to merge, overwriting the left</param>
    /// <param name="options">Options for merge</param>
    public static void MergeInto(this JContainer left, JToken right, object oV)
    {
        foreach (var rightChild in right.Children<JProperty>())
        {
            var rightChildProperty = rightChild;
            var leftPropertyValue = left.SelectToken(rightChildProperty.Name);

            if (leftPropertyValue == null)
            {
                // no matching property, just add 
                left.Add(rightChild);
            }
            else
            {
                var leftProperty = (JProperty)leftPropertyValue.Parent;

                var leftArray = leftPropertyValue as JArray;
                var rightArray = rightChildProperty.Value as JArray;
                if (leftArray != null && rightArray != null)
                {
                    //switch (options.ArrayHandling)
                    //{
                    //    case MergeOptionArrayHandling.Concat:

                    //        foreach (var rightValue in rightArray)
                    //        {
                    //            leftArray.Add(rightValue);
                    //        }

                    //        break;
                    //    case MergeOptionArrayHandling.Overwrite:

                    leftProperty.Value = rightChildProperty.Value;
                    //        break;
                    //}
                }

                else
                {
                    var leftObject = leftPropertyValue as JObject;
                    if (leftObject == null)
                    {
                        // replace value
                        leftProperty.Value = rightChildProperty.Value;
                    }

                    else
                        // recurse object
                        MergeInto(leftObject, rightChildProperty.Value, null);
                }
            }
        }
    }

    /// <summary>
    /// <para>Merge the right token into the left</para>
    /// <para>Default options are used</para>
    /// </summary>
    /// <param name="left">Token to be merged into</param>
    /// <param name="right">Token to merge, overwriting the left</param>
    public static void MergeInto(this JContainer left, JToken right)
    {
        MergeInto(left, right, null);
    }
}

public class GenericObject
{
    private JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All,
        TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full 
    };

    Dictionary<string, object> oAttribute = new Dictionary<string, object>();
    //
    public GenericObject() { }
    //
    public object this[string sAttributeName]
    {
        get { lock (this) { return oAttribute[sAttributeName]; } }
        set
        {
            lock (this)
            {
                if (oAttribute.ContainsKey(sAttributeName))
                    oAttribute[sAttributeName] = value;
                else
                    oAttribute.Add(sAttributeName, value);
            }
        }

    }

    public void SetPathValue(string sPath, object sValue)
    {
        string[] osValues = sPath.Split('.');
        Dictionary<string, object> obTmp = oAttribute;
        for (int i = 0; i < (osValues.Length - 1); i++)
        {
            if (!obTmp.ContainsKey(osValues[i]))
                obTmp.Add(osValues[i], new Dictionary<string, object>());
            obTmp = (Dictionary<string, object>)obTmp[osValues[i]];
        }
        if (!obTmp.ContainsKey(osValues[osValues.Length - 1]))
            obTmp.Add(osValues[osValues.Length - 1], sValue);
        else
            obTmp[osValues[osValues.Length - 1]] = sValue;
    }

    public Dictionary<string, object> GetAllKeys()
    {
        Dictionary<string, object> oResult = new Dictionary<string, object>();

        var stackDictionariesToVisit = new Stack<Tuple<string, Dictionary<string, object>>>();
        stackDictionariesToVisit.Push(new Tuple<string, Dictionary<string, object>>("", oAttribute));
        while (stackDictionariesToVisit.Count > 0)
        {
            var nextDictionary = stackDictionariesToVisit.Pop();
            string sParent = nextDictionary.Item1;
            foreach (var keyValuePair in nextDictionary.Item2)
            {
                string sPath = sParent.Length == 0 ? keyValuePair.Key : (keyValuePair.Key[0] == '[' ? sParent + keyValuePair.Key : sParent + "." + keyValuePair.Key);
                if (keyValuePair.Value is Dictionary<string, object>)
                {
                    stackDictionariesToVisit.Push(new Tuple<string, Dictionary<string, object>>(sPath, (Dictionary<string, object>)keyValuePair.Value));
                }
                else if (keyValuePair.Value is List<object>)
                {
                    for (int i = 0; i < ((List<object>)keyValuePair.Value).Count; i++)
                        stackDictionariesToVisit.Push(new Tuple<string, Dictionary<string, object>>(sPath, new Dictionary<string, object>() { { "[" + i + "]", ((List<object>)keyValuePair.Value)[i] } }));
                }
                else
                {
                    oResult.Add(sPath, keyValuePair.Value);
                }
            }
        }
        return oResult;
    }

    public void SetJSON(string sJSON)
    {
        oAttribute = JsonConvert.DeserializeObject<Dictionary<string, object>>(string.IsNullOrEmpty(sJSON) ? "{}" : sJSON, new DictionaryConverter());
    }

    public string GetJSON(JsonSerializerSettings oSetting = null)
    {
        return JsonConvert.SerializeObject(this.oAttribute, Newtonsoft.Json.Formatting.Indented, oSetting);
    }
}

public class DictionaryConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { this.WriteValue(writer, value); }

    private void WriteValue(JsonWriter writer, object value)
    {
        var t = JToken.FromObject(value);
        switch (t.Type)
        {
            case JTokenType.Object:
                this.WriteObject(writer, value);
                break;
            case JTokenType.Array:
                this.WriteArray(writer, value);
                break;
            default:
                writer.WriteValue(value);
                break;
        }
    }

    private void WriteObject(JsonWriter writer, object value)
    {
        writer.WriteStartObject();
        var obj = value as IDictionary<string, object>;
        foreach (var kvp in obj)
        {
            writer.WritePropertyName(kvp.Key);
            this.WriteValue(writer, kvp.Value);
        }
        writer.WriteEndObject();
    }

    private void WriteArray(JsonWriter writer, object value)
    {
        writer.WriteStartArray();
        var array = value as IEnumerable<object>;
        foreach (var o in array)
        {
            this.WriteValue(writer, o);
        }
        writer.WriteEndArray();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return ReadValue(reader);
    }

    private object ReadValue(JsonReader reader)
    {
        while (reader.TokenType == JsonToken.Comment)
        {
            if (!reader.Read()) throw new JsonSerializationException("Unexpected Token when converting IDictionary<string, object>");
        }

        switch (reader.TokenType)
        {
            case JsonToken.StartObject:
                return ReadObject(reader);
            case JsonToken.StartArray:
                return this.ReadArray(reader);
            case JsonToken.Integer:
            case JsonToken.Float:
            case JsonToken.String:
            case JsonToken.Boolean:
            case JsonToken.Undefined:
            case JsonToken.Null:
            case JsonToken.Date:
            case JsonToken.Bytes:
                return reader.Value;
            default:
                throw new JsonSerializationException
                    (string.Format("Unexpected token when converting IDictionary<string, object>: {0}", reader.TokenType));
        }
    }

    private object ReadArray(JsonReader reader)
    {
        IList<object> list = new List<object>();

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonToken.Comment:
                    break;
                default:
                    var v = ReadValue(reader);

                    list.Add(v);
                    break;
                case JsonToken.EndArray:
                    return list;
            }
        }

        throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
    }

    private object ReadObject(JsonReader reader)
    {
        var obj = new Dictionary<string, object>();

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonToken.PropertyName:
                    var propertyName = reader.Value.ToString();

                    if (!reader.Read())
                    {
                        throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
                    }

                    var v = ReadValue(reader);

                    obj[propertyName] = v;
                    break;
                case JsonToken.Comment:
                    break;
                case JsonToken.EndObject:
                    return obj;
            }
        }

        throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
    }

    public override bool CanConvert(Type objectType) { return typeof(IDictionary<string, object>).IsAssignableFrom(objectType); }
}
