using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Solution.DbOperations.Infrastracture
{
    public interface IBulkUpdateOptions<TSource> : IDbOperationOptions
    {
        string tableName { get; set; }
        Expression<Func<TSource, object>> primaryKeys { get; set; }
        Expression<Func<TSource, object>> excludeprimaryKeys { get; set; }
        Expression<Func<TSource, object>> includeColumns { get; set; }
        Expression<Func<TSource, object>> excludeColumns { get; set; }
        Expression<Func<TSource, object>> fieldsToUpdate { get; set; }
        Expression<Func<TSource, object>> joinColumns { get; set; }
        bool dropTableIfExist { get; set; }
        bool createTableIfNotExist { get; set; }
        string otherSqlWhereCondition { get; set; }
        string joinSql { get; set; }
    }
}
