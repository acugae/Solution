namespace Solution.Infrastructure.Models;

public class cViewConfig
{
    public List<cViewColumn> Columns { get; set; } = null;
    public cViewEdit Edit { get; set; } = null;
    public List<cViewParameter> Parameters { get; set; } = null;
}
public class cViewColumn
{
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; } = string.Empty;
    public bool IsPK { get; set; } = false;
    public bool IsEdit { get; set; } = false;
    public bool IsInsert { get; set; } = false;
    public bool StickyLeft { get; set; } = false;
}
public class cViewEdit
{
    public string TypeUpdate { get; set; } = string.Empty; // stored, httproutes, control
    public string ValueUpdate { get; set; } = string.Empty; // sp_prov_syintQuery_USR_BONIFICHE_CAPCOMUNE, /abc/gggg, CONTROL_UPDATE_CONTRATTI
    public string TypeInsert { get; set; } = string.Empty; // stored, httproutes, control
    public string ValueInsert { get; set; } = string.Empty; // sp_prov_syintQuery_USR_BONIFICHE_CAPCOMUNE, /abc/gggg, CONTROL_UPDATE_CONTRATTI
}
public class cViewSource
{
    public string Type { get; set; } = string.Empty; // sql, list, syint_Query
    public string Value { get; set; } = string.Empty; // select * from, 0=Scegli,1=Si,2=No
    public string DBConnection { get; set; } = string.Empty; // dbMain, dbCRM, ...
}
public class cViewExpand
{
    public string Type { get; set; } = string.Empty; // control, grid, ...
    public string Code { get; set; } = string.Empty; // USR_GESTCONTRSTTI, CONTROL_GEST_VENDITA
    public List<cViewMaps> Maps { get; set; } = null;
}
public class cViewMaps
{
    public string NameSource { get; set; } = string.Empty; // co_id
    public string NameTarget { get; set; } = string.Empty; // idContratto
}
public class cViewParameter
{
    public string Name { get; set; } = string.Empty; // idContratto
    public string Type { get; set; } = string.Empty; // string, int
    public string Descri { get; set; } = string.Empty; // idContratto Punto
    public int IsRequired { get; set; } = 1;
}
