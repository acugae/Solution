using Newtonsoft.Json.Linq;
using NPOI.XWPF.UserModel;
using Solution.Data;
using System.Dynamic;
using static iTextSharp.text.pdf.AcroFields;

namespace Solution;

public class Map
{
    public Map() { }
    public Map(string map)
    {
        string[] oValues = map.Split('=');
        Source = oValues[0];
        Target = oValues[1];
        if (oValues.Length >= 3)
            Type = oValues[2];
    }

    public string Source { get; set; } = string.Empty; // co_id
    public string Target { get; set; } = string.Empty; // idContratto
    public string? Type { get; set; } = string.Empty; // string
}

public class Maps
{
    public Dictionary<string, Map> Sources { get; set; } = new();
    public Dictionary<string, Map> Targets { get; set; } = new();

    public Maps() { }
    // "source1=target1;source2=target2"
    public Maps(string maps)
    {
        if (string.IsNullOrEmpty(maps.Trim()))
            return;
        string[] oV = maps.Split(';');
        for (int i = 0; i < oV.Length; i++)
        {
            Map oM = new(oV[i]);
            Sources.Add(oM.Source, oM);
            Targets.Add(oM.Target, oM);
        }
    }

    public void Add(string source, string target, string? type = null)
    {
        Map oM = new();
        oM.Source = source; oM.Target = target; oM.Type = type;
        Sources.Add(oM.Source, oM);
        Targets.Add(oM.Target, oM);
    }
}
public static class HttpContextExtensions
{
    public static async Task<string> GetBody(this HttpRequest oRequest)
    {
        try
        {
            return await oRequest.Body.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            return "{ \"errormessage\" : \"" + ex.Message.Replace("'", "''") + "\"  }";
        }
    }
}
public static class ExpandoObjectMethods
{
    public static T FirstOrDefault<T>(this ExpandoObject oExpandoObject, string key)
    {
        object r = oExpandoObject.FirstOrDefault(x => x.Key == key).Value;
        return (r is T) ? (T)r : default(T);
    }
}
public static class DataTableExtensions
{
    public static List<Dictionary<string, object>> ToKeyValue(this DataTable dt)
    {
        List<Dictionary<string, object>> oResult = new List<Dictionary<string, object>>();
        foreach (DataRow row in dt.Rows)
            oResult.Add(row.ToDictionary());
        return oResult;
    }

    public static List<dynamic> ToDynamic(this DataTable dt)
    {
        List<dynamic> oResult = new List<dynamic>();
        foreach (DataRow row in dt.Rows)
            oResult.Add(row.ToDynamic());
        return oResult;
    }
    public static List<ExpandoObject> ToExpando(this DataTable dt)
    {
        List<ExpandoObject> oResult = new List<ExpandoObject>();
        foreach (DataRow row in dt.Rows)
            oResult.Add(row.ToExpando());
        return oResult;
    }
    public static List<K> To<K>(this DataTable dt, Maps maps = null) where K : new()
    {
        List<K> oResult = new List<K>();
        foreach (DataRow row in dt.Rows)
            oResult.Add(row.To<K>(maps));
        return oResult;
    }
    public static CRUDBase[] ToCrud(this DataTable dataTable)
    {
        List<CRUDBase> oResult = new List<CRUDBase>();
        for (int i = 0; dataTable != null && i < dataTable.Rows.Count; i++)
        {
            DataRow oDR = dataTable.Rows[i];
            CRUDBase oCB = new CRUDBase(dataTable.TableName);
            oCB.Attributes = dataTable.Columns.Cast<DataColumn>().ToDictionary(column => column.ColumnName, column => oDR[column]);
            oResult.Add(oCB);
        }
        return oResult.ToArray();
    }
    public static string Serialize(this DataTable dataTable)
    {
        return JsonConvert.SerializeObject(dataTable, Newtonsoft.Json.Formatting.Indented);
    }
}

public static class DataRowExtensions
{
    public static Dictionary<string, object> ToDictionary(this DataRow dr)
    {
        if (dr == null)
            return [];
        Dictionary<string, object> oResult = new Dictionary<string, object>();
        foreach (DataColumn column in dr.Table.Columns)
            oResult[column.ColumnName] = (Convert.IsDBNull(dr[column]) ? null : dr[column]);
        return oResult;
    }
    public static List<KeyValuePair<string, object>> ToKeyValue(this DataRow dr)
    {
        if (dr == null)
            return [];
        var keyValuePairs = new List<KeyValuePair<string, object>>();
        foreach (DataColumn column in dr.Table.Columns)
            keyValuePairs.Add(new KeyValuePair<string, object>(column.ColumnName, (Convert.IsDBNull(dr[column]) ? null : dr[column])));
        return keyValuePairs;
    }
    public static dynamic ToDynamic(this DataRow dr)
    {
        return dr.ToDictionary().ToDynamic();
    }
    public static ExpandoObject ToExpando(this DataRow dr)
    {
        return dr.ToDictionary().ToExpando();
    }
    /// <summary>
    /// Converet un DataRow in un oggetto prestabilito.
    /// </summary>
    /// <typeparam name="T">Tipo di destinazione</typeparam>
    /// <param name="row"></param>
    /// <param name="maps"></param>
    /// <returns></returns>
    public static T To<T>(this DataRow row, Maps maps = null) where T : new()
    {
        PropertyInfo[] Properties = typeof(T).GetProperties();
        T obj = new();

        if (maps is null)
        {
            maps = new();
            foreach (PropertyInfo objProperty in Properties)
            {
                if (!row.Table.Columns.Contains(objProperty.Name))
                    continue;
                maps.Add(objProperty.Name, objProperty.Name);
            }
        }

        try
        {
            foreach (PropertyInfo objProperty in Properties)
            {
                if (!maps.Targets.ContainsKey(objProperty.Name))
                    continue;
                string sourceName = maps.Targets[objProperty.Name].Source;
                object? value = row[sourceName];
                //if (value is null || value == DBNull.Value)
                //    continue;
                if (!string.IsNullOrEmpty(value.ToString()))
                {
                    if (Nullable.GetUnderlyingType(objProperty.PropertyType) != null)
                    {
                        //value = row[sourceName].ToString().Replace("$", "").Replace(",", "");
                        objProperty.SetValue(obj, Convert.ChangeType(value, Type.GetType(Nullable.GetUnderlyingType(objProperty.PropertyType).ToString())), null);
                    }
                    else
                    {
                        //value = row[sourceName].ToString().Replace("%", "");
                        objProperty.SetValue(obj, Convert.ChangeType(value, Type.GetType(objProperty.PropertyType.ToString())), null);
                    }
                }
            }
            return obj;
        }
        catch(Exception ex)
        {
            return obj;
        }
    }

}

public static class DictionaryStringObjectExtensions
{
    public static dynamic ToDynamic(this Dictionary<string, object> dict)
    {
        var response = new ExpandoObject();
        var eoColl = (ICollection<KeyValuePair<string, object>>)response;
        foreach (var kvp in dict)
        {
            eoColl.Add(kvp);
        }
        return response;
    }
    public static ExpandoObject ToExpando(this Dictionary<string, object> dict)
    {
        var response = new ExpandoObject();
        var eoColl = (ICollection<KeyValuePair<string, object>>)response;
        foreach (var kvp in dict)
        {
            eoColl.Add(kvp);
        }
        return response;
    }
}
public static class RequestExtensions
{
    public static async Task<string> ReadAsStringAsync(this Stream requestBody, bool leaveOpen = false)
    {
        using StreamReader reader = new(requestBody, leaveOpen: leaveOpen);
        var bodyAsString = await reader.ReadToEndAsync();
        return bodyAsString;
    }
}

public static class StringExtensions
{
    public static string[] GetIntoTag(this string _string, string sTagBegin, string sTagEnd)
    {
        List<string> vString = new List<string>();
        Tuple<string?, int> item = new(null, 0);
        for (; item.Item2 != -1;)
        {
            item = _string.GetIntoTag(sTagBegin, sTagEnd, item.Item2);
            if (item.Item1 != null && item.Item2 != -1)
                vString.Add(item.Item1);
        }
        return vString.ToArray();
    }

    private static Tuple<string, int> GetIntoTag(this string _string, string sTagBegin, string sTagEnd, int index)
    {
        int iBBody = _string.IndexOf(sTagBegin, index);
        if (iBBody != -1)
        {
            int iEBody = _string.IndexOf(sTagEnd, iBBody + sTagBegin.Length + 1);
            if (iEBody != -1)
            {
                index = iEBody + 1;
                return new Tuple<string, int>(_string.Substring(iBBody + sTagBegin.Length, iEBody - (iBBody + sTagBegin.Length)), index);
            }
        }
        index = -1;
        return new Tuple<string,int>(_string, index);
    }

}