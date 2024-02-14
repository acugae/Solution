using System.Dynamic;

namespace Solution;
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
public static class DataTableExtensions
{
    public static List<Dictionary<string, object>> ToKeyValue(this DataTable dt)
    {
        List<Dictionary<string, object>> oResult = new List<Dictionary<string, object>>();
        foreach (DataRow row in dt.Rows)
            oResult.Add(row.ToKeyValue());
        return oResult;
    }

    public static List<dynamic> ToDynamic(this DataTable dt)
    {
        List<dynamic> oResult = new List<dynamic>();
        foreach (DataRow row in dt.Rows)
            oResult.Add(row.ToDynamic());
        return oResult;
    }
    public static List<K> To<K>(this DataTable dt) where K : new()
    {
        List<K> oResult = new List<K>();
        foreach (DataRow row in dt.Rows)
            oResult.Add(row.To<K>());
        return oResult;
    }
}

public static class DataRowExtensions
{
    public static Dictionary<string, object> ToKeyValue(this DataRow dr)
    {
        Dictionary<string, object> oResult = new Dictionary<string, object>();
        foreach (DataColumn column in dr.Table.Columns)
            oResult[column.ColumnName] = dr[column];
        return oResult;
    }
    public static dynamic ToDynamic(this DataRow dr)
    {
        return dr.ToKeyValue().ToDynamic();
    }
    public static T To<T>(this DataRow row) where T : new()
    {
        T obj = new();
        try
        {
            string columnname = "";
            string value = "";
            PropertyInfo[] Properties;
            Properties = typeof(T).GetProperties();
            foreach (PropertyInfo objProperty in Properties)
            {
                //columnname = columnsName.Find(name => name.ToLower() == objProperty.Name.ToLower());
                if (!row.Table.Columns.Contains(objProperty.Name))
                    continue;
                columnname = row.Table.Columns[objProperty.Name].ColumnName;
                if (!string.IsNullOrEmpty(columnname))
                {
                    value = row[columnname].ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (Nullable.GetUnderlyingType(objProperty.PropertyType) != null)
                        {
                            value = row[columnname].ToString().Replace("$", "").Replace(",", "");
                            objProperty.SetValue(obj, Convert.ChangeType(value, Type.GetType(Nullable.GetUnderlyingType(objProperty.PropertyType).ToString())), null);
                        }
                        else
                        {
                            value = row[columnname].ToString().Replace("%", "");
                            objProperty.SetValue(obj, Convert.ChangeType(value, Type.GetType(objProperty.PropertyType.ToString())), null);
                        }
                    }
                }
            }
            return obj;
        }
        catch
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
