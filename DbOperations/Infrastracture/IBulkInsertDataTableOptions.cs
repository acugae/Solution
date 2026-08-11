using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Solution.DbOperations.Infrastracture
{
    public interface IBulkInsertDataTableOptions : IDbOperationOptions
    {
        string tableName { get; set; }
        string[] primaryKeys { get; set; }
        string[] includeColumns { get; set; }
        string[] excludeColumns { get; set; }
        string[] excludePrimaryKeyColumns { get; set; }
        bool dropTableIfExist { get; set; }
        bool createTableIfNotExist { get; set; }
        bool primaryKeyAutoIncrement { get; set; }
    }
}