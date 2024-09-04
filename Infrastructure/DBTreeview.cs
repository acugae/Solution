using static System.Net.Mime.MediaTypeNames;

namespace Solution.Infrastructure;
public class DBTreeview : DBEntity
{
    public DBTreeview(DB DB, string dbKey) : base(DB, dbKey, "syint_Treeview") { }
    public DataTable GetTreeview(string user)
    {
        string sSQL = " SELECT tv_id, coalesce(tv_idParent, '00000000-0000-0000-0000-000000000000') as parent, tv_object, tv_type, coalesce(tv_name, Name) as Name, tv_order FROM";
        sSQL += " (";
        sSQL += "   SELECT distinct tk_name Codice, tk_title Name, per_type as Type  FROM syint_Tasks INNER JOIN syint_Permissions ON per_object = tk_name OR per_object = '*' WHERE tk_active = 1 AND per_type = 'task' AND (  per_user = '@applicationuser'   OR   per_role in (@usergroups) )";
        sSQL += "   UNION ";
        sSQL += "   SELECT distinct co_codice Codice, co_name Name, per_type as type FROM syint_Controls INNER JOIN syint_Permissions ON per_object = co_codice OR per_object = '*' WHERE co_active = 1 AND per_type = 'control' AND (  per_user = '@applicationuser'   OR   per_role in (@usergroups) )";
        sSQL += "   UNION";
        sSQL += "   SELECT distinct qu_codice Codice, qu_name Name, per_type as type FROM syint_Query INNER JOIN syint_Permissions ON per_object = qu_codice OR per_object = '*' WHERE qu_active = 1 AND per_type = 'view' AND (  per_user = '@applicationuser'   OR   per_role in (@usergroups) )";
        sSQL += " ) as TMP INNER JOIN syint_Treeview ON Codice = tv_object AND Type = tv_type";
        sSQL += " UNION";
        sSQL += " SELECT tv_id, COALESCE(tv_idparent, '00000000-0000-0000-0000-000000000000') AS parent, tv_object, tv_type, tv_name AS Name ,tv_order FROM syint_treeview where tv_type = 'directory'";
        sSQL += " ORDER BY tv_order";

        sSQL = sSQL.Replace("@applicationuser", user);
        //sSQL = sSQL.Replace("@applicationuserid", cApplication.Profile.User.ID.ToString());
        sSQL = sSQL.Replace("@usergroups", "''");

        return _DB.Get(_dbKey, sSQL);
    }
}
