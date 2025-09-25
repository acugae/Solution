using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using Solution.DbOperations.Infrastracture;

namespace Solution.DbOperations.Models
{
    public class BulkUpdateOptions<TSource> : IBulkUpdateOptions<TSource>
    {
        public IDbConnection Connection { get; set; } = null;
        public IDbTransaction Transaction { get; set; } =null;
        public int ConnectionTimeout { get; set; } = 500000;
        public string tableName { get; set; } = string.Empty;
        public Expression<Func<TSource, object>> primaryKeys { get; set; } = null;
        public Expression<Func<TSource, object>> excludeprimaryKeys { get; set; } = null;
        public Expression<Func<TSource, object>> includeColumns { get; set; } = null;
        public Expression<Func<TSource, object>> excludeColumns { get; set; } = null;
        public Expression<Func<TSource, object>> fieldsToUpdate { get; set; } = null;
        public Expression<Func<TSource, object>> joinColumns { get; set; } = null;
        public bool dropTableIfExist { get; set; } = false;
        public bool createTableIfNotExist { get; set; } = false;
        public string otherSqlWhereCondition { get; set; } = null;
        public string joinSql { get; set; } = null;
    }
}
