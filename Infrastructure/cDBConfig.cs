namespace Solution.Infrastructure;
public class cDBConfig
{
    readonly cDB DB;
    public cDBConfig(cModelConfiguration oConfiguration) => DB = new(oConfiguration);
    public cDBConfig(cDB oDB) => DB = oDB;
    public string GetConfig(string sKeyConfig, string sValueDefault)
    {
        string sQuery = "SELECT co_value FROM syint_Config WHERE co_key = '" + sKeyConfig + "'";
        DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, sQuery);
        if (oDT == null || oDT.Rows.Count == 0)
            return sValueDefault;
        return oDT.Rows[0]["co_value"].ToString();
    }
    public int SetConfig(string sKeyConfig, string sValue)
    {
        string sQuery = "SELECT co_value FROM syint_Config WHERE co_key = '" + sKeyConfig + "'";
        DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, sQuery);
        try
        {
            if (oDT == null || oDT.Rows.Count == 0)
            {
                string sInsert = string.Format("insert into syint_Config (co_key, co_value, co_date, co_dateModified) Values ('{0}', '{1}', getdate(), getdate())", sKeyConfig, sValue.Replace("'", "''"));
                DB.Execute(DB.Configuration.InfrastructureConnection, sInsert);
                return 0;
            }
            else
            {
                string sUpdate = string.Format("update syint_Config set co_value = '{1}', co_dateModified = getdate() where co_key = '{0}'", sKeyConfig, sValue.Replace("'", "''"));
                DB.Execute(DB.Configuration.InfrastructureConnection, sUpdate);
                return 1;
            }
        }
        catch
        {
            return -1;
        }
    }
}
