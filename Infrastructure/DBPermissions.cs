using iTextSharp.text.pdf;
using System.Data;

namespace Solution.Infrastructure;
public class DBPermissions : DBEntity
{
    public DBPermissions(DB DB, string dbKey) : base(DB, dbKey, "syint_Permissions") { }
    public int Insert(string user, string role, string objectRif, string typeRif)
    {
        try
        {
            return DB.Execute("INSERT INTO [dbo].[syint_Permissions] ([per_user],[per_role],[per_object],[per_type]) VALUES (" + (user == null ? "null" : "'" + user + "'") + ", " + (role == null ? "null" : "'" + role + "'") + ", '" + objectRif + "', '" + typeRif + "')");
        }
        catch
        {
            return -1;
        }
    }
}
