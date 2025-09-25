namespace Solution.Data;

public class cMap
{
    public string sTarget = "";
    public string sType = "";
    public cMap(string target, string type) { sTarget = target; sType = type; }
}

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
