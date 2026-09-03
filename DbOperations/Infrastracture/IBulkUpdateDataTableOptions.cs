using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Solution.DbOperations.Infrastracture
{
    public interface IBulkUpdateDataTableOptions : IDbOperationOptions
    {
        string tableName { get; set; }
        string[] primaryKeys { get; set; }
        string[] excludeprimaryKeys { get; set; }
        string[] includeColumns { get; set; }
        string[] excludeColumns { get; set; }
        string[] fieldsToUpdate { get; set; }
        string[] joinColumns { get; set; }
        bool dropTableIfExist { get; set; }
        bool createTableIfNotExist { get; set; }
        string otherSqlWhereCondition { get; set; }
        string joinSql { get; set; }
    }
}