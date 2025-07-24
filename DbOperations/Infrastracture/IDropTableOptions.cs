using System;
using System.Collections.Generic;
using System.Text;

namespace Solution.DbOperations.Infrastracture
{
    public interface IDropTableOptions : IDbOperationOptions
    {
        string tableName { get; set; }
        bool useIfExist { get; set; }
    }
}
