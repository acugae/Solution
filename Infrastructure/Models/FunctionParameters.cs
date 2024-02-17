namespace Solution.Infrastructure.Models;

public class FunctionParameters
{
    IDictionary<string, object?> _Items { get; set; } = new Dictionary<string, object?>(); // (StringComparer.InvariantCultureIgnoreCase);
    public object? this[string sName]
    {
        get { return _Items[sName]; }
        set { _Items[sName] = value; }
    }
    public FunctionParameters() { }
    public FunctionParameters(IDictionary<string, object?> Items) { _Items = (IDictionary<string, object?>)Items; }
    public void Add(string Key, object Value) { _Items.Add(Key, Value); }
    public void Remove(string Key) { _Items.Remove(Key); }
    public bool ContainsKey(string Key)
    { 
        return _Items.ContainsKey(Key); 
    }  
    public string? ToString(string sName)
    {
        if (!_Items.TryGetValue(sName, out object? value))
            return null;
        return value?.ToString();
    }
    public int ToInt(string sName)
    {
        if (!_Items.TryGetValue(sName, out object? value))
            return 0;
        return Convert.ToInt32(value);
    }
    public DateTime? ToDateTime(string sName)
    {
        if (!_Items.TryGetValue(sName, out object? value))
            return null;
        return Convert.ToDateTime(value);
    }
}