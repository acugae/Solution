using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Solution.DbOperations.Infrastracture
{
    public interface ICreateTableOptions<TSource> : IDbOperationOptions
    {
        string tableName { get; set; }
        bool dropIfExists { get; set; }
        Expression<Func<TSource, object>> primaryKey { get; set; }
        Expression<Func<TSource, object>> includeColumns { get; set; }
        Expression<Func<TSource, object>> excludeColumns { get; set; }
        Expression<Func<TSource, object>> excludePrimaryKeyColumns { get; set; }
        bool createTableIfNotExist { get; set; }
        bool primaryKeyAutoIncrement { get; set; } 
    }
}
