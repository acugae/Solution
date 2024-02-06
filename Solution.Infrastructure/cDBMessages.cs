namespace Solution.Infrastructure;
public class cDBMessages 
{
    readonly cDB DB;
    public cDBMessages(cModelConfiguration oConfiguration) => DB = new(oConfiguration);
    public cDBMessages(cDB oDB) => DB = oDB;
    public string GetTasks(string sHost, string sName, int isParallel, string ExclusiveMessages = null)
    {
        string sTasks = "";
        string sSQL = "SELECT tk_name FROM core_Tasks WHERE tk_parallelexec = " + isParallel.ToString() + " and tk_active = 1 AND ( ( (tk_hosts LIKE '{0},%') OR (tk_hosts LIKE '%,{0}') OR (tk_hosts LIKE '%,{0},%') OR (tk_hosts = '{0}') OR (tk_hosts = '*') ) AND ( (tk_services LIKE '{1},%') OR (tk_services LIKE '%,{1}') OR (tk_services LIKE '%,{1},%') OR (tk_services = '{1}') OR (tk_services = '*') ) ) ";
        sSQL += (ExclusiveMessages == null || ExclusiveMessages.Trim().Equals("") ? "" : " AND tk_name in (" + ExclusiveMessages + ")");
        //
        DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, string.Format(sSQL, sHost, sName));
        for (int i = 0; oDT != null && i < oDT.Rows.Count; i++)
        {
            if (i == 0)
                sTasks += string.Format("'{0}'", oDT.Rows[i]["tk_name"].ToString());
            else
                sTasks += string.Format(",'{0}'", oDT.Rows[i]["tk_name"].ToString());
        }
        return sTasks;
    }
    public DataTable GetMessageByState(string sQueue, string sTasksParallel, string sTasksSerial)
    {
        cModelConfigurationQueue Queue = DB.Configuration.Queues[sQueue];
        string sTable = Queue.Table;
        //
        string sSerial = sTasksSerial.Trim().Equals("") ? "null" : sTasksSerial.Trim();
        string sParallel = sTasksParallel.Trim().Equals("") ? "null" : sTasksParallel.Trim();
        string sSQL = @"
              SELECT * FROM
              (
	              SELECT top 100 * FROM " + sTable + @" 
	              WHERE msg_state = 0 AND (getdate() between coalesce(msg_dateIniVal, '01/01/1900') and coalesce(msg_dateFinVal, '01/01/2888')) and msg_class in (" + sParallel + @")
	              ORDER BY msg_id
	              UNION ALL
	              SELECT TOP 100 * FROM " + sTable + @" WHERE msg_id IN
	              (  
		              SELECT A.msg_id FROM 
		              (
		                SELECT MIN(msg_id) as msg_id, msg_class FROM " + sTable + @"
		                WHERE msg_state = 0 and (getdate() between coalesce(msg_dateIniVal, '01/01/1900') and coalesce(msg_dateFinVal, '01/01/2888')) and msg_class in (" + sSerial + @")
		                group by msg_class 
		              ) AS A
		              LEFT JOIN 
		              (
		                SELECT MIN(msg_id) as msg_id, msg_class FROM " + sTable + @"
		                WHERE msg_state = 1 and (getdate() between coalesce(msg_dateIniVal, '01/01/1900') and coalesce(msg_dateFinVal, '01/01/2888')) and msg_class in (" + sSerial + @")
		                group by msg_class 
		              ) AS B ON A.msg_class = b.msg_class
		              WHERE B.msg_class is null
	              ) 
	              ORDER BY msg_id
              ) AS tmp
              ORDER BY msg_id
        ";
        return DB.Get(Queue.Connection, sSQL);
    }

    public DataRow GetMessageSystem(string sQueue, Guid ID)
    {
        cModelConfigurationQueue Queue = DB.Configuration.Queues[sQueue];
        string sTable = Queue.Table;
        //
        string sSQL = @"
	        SELECT top 1 * FROM " + sTable + @" 
	        WHERE msg_state = 0 AND (getdate() between coalesce(msg_dateIniVal, '01/01/1900') and coalesce(msg_dateFinVal, '01/01/2888')) and msg_class = 'system." + ID.ToString() + @"'
	        ORDER BY msg_id
        ";
        DataTable oDTResult = DB.Get(Queue.Connection, sSQL);
        if(oDTResult == null || oDTResult.Rows.Count == 0)
            return null;
        return oDTResult.Rows[0];
    }
    public int GetDeQueue(string sQueue, int IDMsg, Guid GuidService)
    {
        cModelConfigurationQueue Queue = DB.Configuration.Queues[sQueue];
        string sConnection = Queue.Connection;
        cParameter oPIn0 = DB.DataManager.CreateParameter(sConnection, DbType.String, ParameterDirection.Input, "p_nTableName", Queue.Table);
        cParameter oPIn1 = DB.DataManager.CreateParameter(sConnection, DbType.Int32, ParameterDirection.Input, "@p_nIDMsg", IDMsg);
        cParameter oPIn2 = DB.DataManager.CreateParameter(sConnection, DbType.Guid, ParameterDirection.Input, "@p_nidService", GuidService);
        cParameter oPOut = DB.DataManager.CreateParameter(sConnection, DbType.Int32, ParameterDirection.Output, "@p_nRowsAffected", 0);
        DB.Invoke(sConnection, "core_DeQueue", oPIn0, oPIn1, oPIn2, oPOut);
        if (oPOut.Value.Equals(1))
            return 1;
        return 0;
    }
    public void UpdateMessage(string sQueue, int id, int state, string message)
    {
        cModelConfigurationQueue Queue = DB.Configuration.Queues[sQueue];
        string sUpdate = string.Format("update " + Queue.Table + " SET msg_state = {0}, msg_message = {1}, msg_dateEnd = getdate() where msg_id = {2}", state, (message == null ? "null" : ("'" + message.Replace("'", "''") + "'")), id);
        DB.Execute(Queue.Connection, sUpdate);
    }
    public int InsertQueue(string sQueue, string value, string taskname, int state = 0, string message = null, string inival = null, string finval = null, object IDTaskPianif = null, string user = "system.integration")
    {
        cModelConfigurationQueue Queue = DB.Configuration.Queues[sQueue];
        string sConnection = Queue.Connection;
        cParameter oPIn0 = DB.DataManager.CreateParameter(sConnection, DbType.String, ParameterDirection.Input, "p_nTableName", Queue.Table);
        cParameter oPIn1 = DB.DataManager.CreateParameter(sConnection, DbType.String, ParameterDirection.Input, "p_nValue", value);
        cParameter oPIn2 = DB.DataManager.CreateParameter(sConnection, DbType.String, ParameterDirection.Input, "p_nTaskname", taskname);
        cParameter oPIn3 = DB.DataManager.CreateParameter(sConnection, DbType.Int32, ParameterDirection.Input, "p_nState", state);
        cParameter oPIn4 = DB.DataManager.CreateParameter(sConnection, DbType.DateTime, ParameterDirection.Input, "p_nInival", inival);
        cParameter oPIn5 = DB.DataManager.CreateParameter(sConnection, DbType.DateTime, ParameterDirection.Input, "p_nFinval", finval);
        cParameter oPIn6 = DB.DataManager.CreateParameter(sConnection, DbType.Int32, ParameterDirection.Input, "p_nIDTaskPianif", IDTaskPianif);
        cParameter oPIn7 = DB.DataManager.CreateParameter(sConnection, DbType.String, ParameterDirection.Input, "p_nUser", user);
        //
        cParameter oPOut = DB.DataManager.CreateParameter(sConnection, DbType.Int32, ParameterDirection.Output, "p_nIDMsg", 0);
        //
        DB.Invoke(sConnection, "core_InsertQueue", oPIn0, oPIn1, oPIn2, oPIn3, oPIn4, oPIn5, oPIn6, oPIn7, oPOut);
        return Convert.ToInt32(oPOut.Value);
    }
    public void ExecPianif()
    {
        DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, "SELECT * FROM syint_TasksPianif INNER JOIN core_Tasks ON tk_id = tkp_idTasks WHERE tkp_interval > 0 and tkp_active = 1 and getdate() between coalesce(tkp_inival, '01/01/1900') and coalesce(tkp_finval, '01/01/2900')  and tkp_expired <= GETDATE()");
        if (oDT == null || oDT.Rows.Count == 0)
            return;
        //
        for (int i = 0; i < oDT.Rows.Count; i++)
        {
            try
            {
                DateTime oDateExpired = Convert.ToDateTime(oDT.Rows[i]["tkp_expired"]);
                int iInterval = Convert.ToInt32(oDT.Rows[i]["tkp_interval"]);
                int iTemp = 0;
                for (iTemp = iInterval; oDateExpired.AddMinutes(iTemp) < DateTime.Now; iTemp += iInterval) ;
                //
                int count = DB.Execute(DB.Configuration.InfrastructureConnection, "UPDATE syint_TasksPianif set tkp_expired = DATEADD( MINUTE, " + iTemp + " ,tkp_expired), tkp_lastexec = GETDATE() where tkp_active = 1 and tkp_expired <= GETDATE() and tkp_id = " + oDT.Rows[i]["tkp_id"].ToString());
                if (count > 0)
                {
                    int idMsg = InsertQueue(DB.Configuration.PianifQueue, oDT.Rows[i]["tkp_value"].ToString(), oDT.Rows[i]["tk_name"].ToString(), 0, null, null, null, Convert.ToInt32(oDT.Rows[i]["tkp_id"]));
                }
            }
            catch
            { }
        }
    }
    public DataRow GetTask(string sTaskName)
    {
        DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, "SELECT * FROM core_Tasks WHERE tk_name = '" + sTaskName + "'");
        if (oDT != null && oDT.Rows.Count == 1)
            return oDT.Rows[0];
        return null;
    }
    public DataRow GetTask(int iIDTask)
    {
        DataTable oDT = DB.Get(DB.Configuration.InfrastructureConnection, "SELECT * FROM core_Tasks WHERE tk_id = " + iIDTask);
        if (oDT != null && oDT.Rows.Count == 1)
            return oDT.Rows[0];
        return null;
    }
    //public int Resubmit(int iMinutes, int iIDMsgRef = 0)
    //{
    //    return InsertQueue(Queue, Value, TaskName, 0, null, DateTime.Now.AddMinutes(iMinutes).ToString("yyyy-MM-ddTHH:mm:ss.000"), null, null, "system.integration.message." + (iIDMsgRef == 0 ? ID.ToString() : "rif." + iIDMsgRef.ToString()));
    //}
}
