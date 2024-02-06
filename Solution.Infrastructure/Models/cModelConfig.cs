namespace Solution.Infrastructure.Models;

public class cModelConfig
{
    public List<cModelColumn> columns { get; set; } = null;
    public cModelEdit edit { get; set; } = null;
    public List<cModelParameter> parameters { get; set; } = null;
}
public class cModelColumn
{
    public string name { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty;
    public bool? isPK { get; set; } = false;
    public bool? isEdit { get; set; } = false;
    public bool? isInsert { get; set; } = false;
}
public class cModelEdit
{
    public string typeUpdate { get; set; } = string.Empty; // stored, httproutes, control
    public string valueUpdate { get; set; } = string.Empty; // sp_prov_syintQuery_USR_BONIFICHE_CAPCOMUNE, /abc/gggg, CONTROL_UPDATE_CONTRATTI
    public string typeInsert { get; set; } = string.Empty; // stored, httproutes, control
    public string valueInsert { get; set; } = string.Empty; // sp_prov_syintQuery_USR_BONIFICHE_CAPCOMUNE, /abc/gggg, CONTROL_UPDATE_CONTRATTI
}
public class cModelSource
{
    public string type { get; set; } = string.Empty; // sql, list, syint_Query
    public string value { get; set; } = string.Empty; // select * from, 0=Scegli,1=Si,2=No
    public string dbConnection { get; set; } = string.Empty; // dbMain, dbCRM, ...
}
public class cModelExpand
{
    public string type { get; set; } = string.Empty; // control, grid, ...
    public string code { get; set; } = string.Empty; // USR_GESTCONTRSTTI, CONTROL_GEST_VENDITA
    public List<cModelMaps> maps { get; set; } = null;
}
public class cModelMaps
{
    public string nameSource { get; set; } = string.Empty; // co_id
    public string nameTarget { get; set; } = string.Empty; // idContratto
}
public class cModelParameter
{
    public string name { get; set; } = string.Empty; // idContratto
    public string type { get; set; } = string.Empty; // string, int
    public string descri { get; set; } = string.Empty; // idContratto Punto
    public int isRequired { get; set; } = 1;
}
