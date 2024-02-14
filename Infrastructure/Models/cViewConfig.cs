namespace Solution.Infrastructure.Models;

public class cViewConfig
{
    public List<cViewColumn> columns { get; set; } = null;
    public cViewEdit edit { get; set; } = null;
    public List<cViewParameter> parameters { get; set; } = null;
}
public class cViewColumn
{
    public string name { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
    public bool? isPK { get; set; } = false;
    public bool? isEdit { get; set; } = false;
    public bool? isInsert { get; set; } = false;
}
public class cViewEdit
{
    public string typeUpdate { get; set; } = string.Empty; // stored, httproutes, control
    public string valueUpdate { get; set; } = string.Empty; // sp_prov_syintQuery_USR_BONIFICHE_CAPCOMUNE, /abc/gggg, CONTROL_UPDATE_CONTRATTI
    public string typeInsert { get; set; } = string.Empty; // stored, httproutes, control
    public string valueInsert { get; set; } = string.Empty; // sp_prov_syintQuery_USR_BONIFICHE_CAPCOMUNE, /abc/gggg, CONTROL_UPDATE_CONTRATTI
}
public class cViewSource
{
    public string type { get; set; } = string.Empty; // sql, list, syint_Query
    public string value { get; set; } = string.Empty; // select * from, 0=Scegli,1=Si,2=No
    public string dbConnection { get; set; } = string.Empty; // dbMain, dbCRM, ...
}
public class cViewExpand
{
    public string type { get; set; } = string.Empty; // control, grid, ...
    public string code { get; set; } = string.Empty; // USR_GESTCONTRSTTI, CONTROL_GEST_VENDITA
    public List<cViewMaps> maps { get; set; } = null;
}
public class cViewMaps
{
    public string nameSource { get; set; } = string.Empty; // co_id
    public string nameTarget { get; set; } = string.Empty; // idContratto
}
public class cViewParameter
{
    public string name { get; set; } = string.Empty; // idContratto
    public string type { get; set; } = string.Empty; // string, int
    public string descri { get; set; } = string.Empty; // idContratto Punto
    public int isRequired { get; set; } = 1;
}
