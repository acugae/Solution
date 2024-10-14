namespace Solution.Infrastructure;
public class DBConfig : DBEntity
{
    public DBConfig(DB DB, string dbKey) : base(DB, dbKey, "syint_Config") { }
    public string GetConfig(string sKeyConfig, string sValueDefault)
    {
        string sQuery = "SELECT co_value FROM syint_Config WHERE co_key = '" + sKeyConfig + "'";
        DataTable oDT = DB.Get(dbKey, sQuery);
        if (oDT == null || oDT.Rows.Count == 0)
            return sValueDefault;
        return oDT.Rows[0]["co_value"].ToString();
    }
    public int SetConfig(string sKeyConfig, string sValue)
    {
        string sQuery = "SELECT co_value FROM syint_Config WHERE co_key = '" + sKeyConfig + "'";
        DataTable oDT = DB.Get(dbKey, sQuery);
        try
        {
            if (oDT == null || oDT.Rows.Count == 0)
            {
                string sInsert = string.Format("insert into syint_Config (co_key, co_value, co_date, co_dateModified) Values ('{0}', '{1}', getdate(), getdate())", sKeyConfig, sValue.Replace("'", "''"));
                DB.Execute(dbKey, sInsert);
                return 0;
            }
            else
            {
                string sUpdate = string.Format("update syint_Config set co_value = '{1}', co_dateModified = getdate() where co_key = '{0}'", sKeyConfig, sValue.Replace("'", "''"));
                DB.Execute(dbKey, sUpdate);
                return 1;
            }
        }
        catch
        {
            return -1;
        }
    }
}
