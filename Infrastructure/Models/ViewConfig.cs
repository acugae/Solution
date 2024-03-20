namespace Solution.Infrastructure.Models;

public class ViewConfig
{
    public List<ViewColumn> Columns { get; set; } = null;
    public ViewEdit Edit { get; set; } = null;
    public List<ViewParameter> Parameters { get; set; } = [];
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
    public string TypeUpdate { get; set; } = string.Empty; // stored, httproutes, control
    public string ValueUpdate { get; set; } = string.Empty; // sp_prov_syintQuery_USR_BONIFICHE_CAPCOMUNE, /abc/gggg, CONTROL_UPDATE_CONTRATTI
    public string TypeInsert { get; set; } = string.Empty; // stored, httproutes, control
    public string ValueInsert { get; set; } = string.Empty; // sp_prov_syintQuery_USR_BONIFICHE_CAPCOMUNE, /abc/gggg, CONTROL_UPDATE_CONTRATTI
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
    public List<ViewMaps> Maps { get; set; } = [];
}
public class ViewMaps
{
    public string NameSource { get; set; } = string.Empty; // co_id
    public string NameTarget { get; set; } = string.Empty; // idContratto
}
public class ViewParameter
{
    public string Name { get; set; } = string.Empty; // idContratto
    public string Type { get; set; } = string.Empty; // string, int
    public string Descri { get; set; } = string.Empty; // idContratto Punto
    public int IsRequired { get; set; } = 1;
}
