namespace Solution.Infrastructure;
//public class cGenericServiceCRUD
//{
//    string dbKey = cApplication.KeyDefault;
//    cDB oDB = null;

//    public cGenericServiceCRUD()
//    {
//        oDB = new cDB(cApplication.Configuration);
//        oDB.ModeConnection = cDB.enModeConnectionOpen.Whenever;
//    }

//    public GenericResponse<DataTable> Find(string dbKey, CRUDFind oFind)
//    {
//        try
//        {
//            DataTable oDT = oDB[dbKey].Find(oFind);
//            return new GenericResponse<DataTable>("200", null, oDT);
//        }
//        catch (Exception ex)
//        {
//            return new GenericResponse<DataTable>("500", ex.Message, null);
//        }
//    }

//    public GenericResponse<DataTable> FindDefault(CRUDFind oFind)
//    {
//        try
//        {
//            DataTable oDT = oDB[dbKey].Find(oFind);
//            return new GenericResponse<DataTable>("200", null, oDT);
//        }
//        catch (Exception ex)
//        {
//            return new GenericResponse<DataTable>("500", ex.Message, null);
//        }
//    }

//    public GenericResponse<int> Insert(string dbKey, CRUDBase oInsert)
//    {
//        try
//        {
//            int iResult = oDB[dbKey].Insert(oInsert);
//            return new GenericResponse<int>("200", null, iResult);
//        }
//        catch (Exception ex)
//        {
//            return new GenericResponse<int>("500", ex.Message, -1);
//        }
//    }

//    public GenericResponse<int> InsertDefault(CRUDBase oInsert)
//    {
//        try
//        {
//            int iResult = oDB[dbKey].Insert(oInsert);
//            return new GenericResponse<int>("200", null, iResult);
//        }
//        catch (Exception ex)
//        {
//            return new GenericResponse<int>("500", ex.Message, -1);
//        }
//    }

//    public GenericResponse<int> Update(string dbKey, CRUDUpdate oUpdate)
//    {
//        try
//        {
//            int iResult = oDB[dbKey].Update(oUpdate);
//            return new GenericResponse<int>("200", null, iResult);
//        }
//        catch (Exception ex)
//        {
//            return new GenericResponse<int>("500", ex.Message, -1);
//        }
//    }

//    public GenericResponse<int> UpdateDefault(CRUDUpdate oUpdate)
//    {
//        try
//        {
//            int iResult = oDB[dbKey].Update(oUpdate);
//            return new GenericResponse<int>("200", null, iResult);
//        }
//        catch (Exception ex)
//        {
//            return new GenericResponse<int>("500", ex.Message, -1);
//        }
//    }

//    public GenericResponse<int> Delete(string dbKey, CRUDDelete oDelete)
//    {
//        try
//        {
//            int iResult = oDB[dbKey].Delete(oDelete);
//            return new GenericResponse<int>("200", null, iResult);
//        }
//        catch (Exception ex)
//        {
//            return new GenericResponse<int>("500", ex.Message, -1);
//        }
//    }

//    public GenericResponse<int> DeleteDefault(CRUDDelete oDelete)
//    {
//        try
//        {
//            int iResult = oDB[dbKey].Delete(oDelete);
//            return new GenericResponse<int>("200", null, iResult);
//        }
//        catch (Exception ex)
//        {
//            return new GenericResponse<int>("500", ex.Message, -1);
//        }
//    }

//    public GenericResponse<long?> InsertWithReturn(string dbKey, CRUDBase oInsert)
//    {
//        try
//        {
//            long? iResult = oDB[dbKey].InsertWithReturn(oInsert);
//            return new GenericResponse<long?>("200", null, iResult);
//        }
//        catch (Exception ex)
//        {
//            return new GenericResponse<long?>("500", ex.Message, -1);
//        }
//    }

//    public GenericResponse<long?> InsertWithReturnDefault(CRUDBase oInsert)
//    {
//        try
//        {
//            long? iResult = oDB[dbKey].InsertWithReturn(oInsert);
//            return new GenericResponse<long?>("200", null, iResult);
//        }
//        catch (Exception ex)
//        {
//            return new GenericResponse<long?>("500", ex.Message, -1);
//        }
//    }

//    public GenericResponse<DataTable> Invoke(string dbKey, CRUDProcedure oProcedure)
//    {
//        try
//        {
//            DataTable oResult = oDB[dbKey].Invoke(oProcedure);
//            return new GenericResponse<DataTable>("200", null, oResult);
//        }
//        catch (Exception ex)
//        {
//            return new GenericResponse<DataTable>("500", ex.Message, null);
//        }
//    }

//    public GenericResponse<DataTable> InvokeDefault(CRUDProcedure oProcedure)
//    {
//        try
//        {
//            DataTable oResult = oDB[dbKey].Invoke(oProcedure);
//            return new GenericResponse<DataTable>("200", null, oResult);
//        }
//        catch (Exception ex)
//        {
//            return new GenericResponse<DataTable>("500", ex.Message, null);
//        }
//    }
//}
