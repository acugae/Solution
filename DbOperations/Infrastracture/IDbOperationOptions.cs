using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Solution.DbOperations.Infrastracture
{
    public interface IDbOperationOptions
    {
         IDbConnection Connection { get; set; }
         IDbTransaction Transaction { get; set; }
        int ConnectionTimeout { get; set; }
    }
}
