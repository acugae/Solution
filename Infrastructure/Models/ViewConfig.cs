using MySqlX.XDevAPI.Common;

namespace Solution.Infrastructure.Models;

public class ViewConfig
{
    public List<ViewColumn>? Columns { get; set; } = null;
    public ViewEdit Edit { get; set; } = null;
    public List<OneParameter>? Parameters { get; set; } = null;
    public List<ViewExpand> Extensions { get; set; } = [];
}
public class ViewColumn
{
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; } = string.Empty;
    public bool IsPK { get; set; } = false;
    public bool IsEdit { get; set; } = false;
    public bool IsInsert { get; set; } = false;
}
public class ViewEdit
{
    public string Type { get; set; } = string.Empty; // stored, httproutes, control
    public string Value { get; set; } = string.Empty; // sp_prov_syintQuery_USR_BONIFICHE_CAPCOMUNE, /abc/gggg, CONTROL_UPDATE_CONTRATTI
}
public class ViewSource
{
    public string Type { get; set; } = string.Empty; // sql, list, syint_Query
    public string Value { get; set; } = string.Empty; // select * from, 0=Scegli,1=Si,2=No
    public string DBConnection { get; set; } = string.Empty; // dbMain, dbCRM, ...
}
public class ViewExpand
{
    public string Name { get; set; } = string.Empty; // 
    public string Type { get; set; } = string.Empty; // control, grid, ...
    public string Value { get; set; } = string.Empty; // USR_GESTCONTRSTTI, CONTROL_GEST_VENDITA
    public List<OneMaps> Maps { get; set; } = [];
}
public class OneMaps
{
    public Dictionary<string, OneMap> Sources { get; set; } = new();
    public Dictionary<string, OneMap> Targets { get; set; } = new();

    public OneMaps() { }
    // "source1=target1;source2=target2"
    public OneMaps(string maps)
    {
        if (string.IsNullOrEmpty(maps.Trim()))
            return;
        string[] oV = maps.Split(';');
        for (int i = 0; i < oV.Length; i++)
        {
            OneMap oM = new(oV[i]);
            Sources.Add(oM.Source, oM);
            Targets.Add(oM.Target, oM);
        }
    }

    public void Add(string source, string target, string? type = null) {
        OneMap oM = new();
        oM.Source = source; oM.Target = target; oM.Type = type;
        Sources.Add(oM.Source, oM);
        Targets.Add(oM.Target, oM);
    }
}
public class OneMap
{
    public OneMap() { }
    public OneMap(string map) {
        string[] oValues = map.Split('=');
        Source = oValues[0];
        Target = oValues[1];
        if(oValues.Length >= 3)
            Type = oValues[2];
    }

    public string Source { get; set; } = string.Empty; // co_id
    public string Target { get; set; } = string.Empty; // idContratto
    public string? Type { get; set; } = string.Empty; // string
}
public class OneParameter
{
    public string? Name { get; set; } = null; // idContratto
    public string? Type { get; set; } = null; // string, int
    public string? Descri { get; set; } = null; // idContratto Punto
    public int AllowNull { get; set; } = 0;
    public string? Default { get; set; } = null; // combo, text, file
    public string? Control { get; set; } = null; // combo, text, file
    public string? Source { get; set; } = null; // list;0=Scegli,1=Si,2=No | sql;dbCRMOptima;name;title;select name, title from sys_tTables  where state=1 order by name
    public object? Value { get; set; } = null;
}
