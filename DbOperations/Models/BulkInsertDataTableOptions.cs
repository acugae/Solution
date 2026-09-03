using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Solution.DbOperations.Infrastracture;

namespace Solution.DbOperations.Models
{
    public class BulkInsertDataTableOptions : IBulkInsertDataTableOptions
    {
        public IDbConnection Connection { get; set; } = null;
        public IDbTransaction Transaction { get; set; } = null;
        public int ConnectionTimeout { get; set; } = 500000;
        public string tableName { get; set; } = string.Empty;
        public string[] primaryKeys { get; set; } = null;
        public string[] excludeprimaryKeys { get; set; } = null;
        public string[] includeColumns { get; set; } = null;
        public string[] excludeColumns { get; set; } = null;
        public string[] excludePrimaryKeyColumns { get; set; } = null;
        public bool dropTableIfExist { get; set; } = false;
        public bool createTableIfNotExist { get; set; } = false;
        public bool primaryKeyAutoIncrement { get; set; }
    }
}