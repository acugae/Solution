using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.Data.SqlClient;
using Solution.DbOperations.Infrastracture;
using Solution.DbOperations.Models;
using OptimaLibray.Db.Operations.Infrastracture;


namespace Solution.DbOperations
{
    public static class DbOperations
    {
        #region Create Table

        public static IDbConnection CreateTable<TSource>(this IDbConnection connection,
            Action<ICreateTableOptions<TSource>> options)
        {
            var opts = new CreateTableOptions<TSource>(connection);
            options(opts);

            CreateTable(opts);
            return connection;
        }

        public static IDatabase<TContext> CreateTable<TContext, TSource>(this IDatabase<TContext> context, Action<ICreateTableOptions<TSource>> options)
        {
            var opts = new CreateTableOptions<TSource>((IDbConnection)context.Connection);
            options(opts);

            using (IDbConnection conn = opts.Connection)
            {
                CreateTable(opts);
                return context;
            }
        }

        public static void CreateTable<TSource>(Action<ICreateTableOptions<TSource>> action)
        {
            var opts = new CreateTableOptions<TSource>();
            action(opts);
            CreateTable(opts);
        }

        public static void CreateTable<TSource>(this IDbConnection connection, ICreateTableOptions<TSource> options)
        {
            options.Connection = connection;
            CreateTable(options);
        }



        public static void CreateTable<TSource>(ICreateTableOptions<TSource> options)
        {
            if (string.IsNullOrEmpty(options.tableName))
            {
                throw new ArgumentNullException(nameof(options.tableName));
            }

            if (options.Connection.State == ConnectionState.Closed) options.Connection.Open();
            bool internalTransaction = false;
            if (options.dropIfExists && options.Transaction == null)
            {
                internalTransaction = true;
                options.Transaction = options.Connection.BeginTransaction();
            }

            List<string> contentCreate = new List<string>();
            var fieldsCreate = GetProperties<TSource>(options.primaryKey, options.includeColumns, options.excludeColumns, options.excludePrimaryKeyColumns);

            try
            {
                //Drop Table If Exist
                if (options.dropIfExists)
                {
                    DropTable(opts =>
                    {
                        opts.tableName = options.tableName;
                        opts.Connection = options.Connection;
                        opts.Transaction = options.Transaction;
                        opts.useIfExist = true; //Forza se Esiste il Drop
                    });
                }

                //Creating temp table on database
                string pks = string.Join(",", fieldsCreate.OrderBy(p => p.Order).Where(p => p.IsPrimaryKey).Select(s => s.Name));

                contentCreate.AddRange(fieldsCreate.Select(e =>
                {
                    string name = e.Name;
                    string typeSql = e.TypeSql;
                    string autoIncrement = ((e.IsPrimaryKey && options.primaryKeyAutoIncrement
                                             && (e.TypeSql.ToLower() == "int" || e.TypeSql.ToLower() == "bigint"))
                            ? "IDENTITY(1,1)"
                            : "");
                    string isNullable = (e.IsNullable ? "NULL" : "NOT NULL");

                    return $"[{name}] {typeSql} {autoIncrement} {isNullable}";
                }));

                //UNIQUEIDENTIFIER And primaryKeyAutoIncrement And is Primary   
                string[] constraint = fieldsCreate.Where(p => options.primaryKeyAutoIncrement && p.IsPrimaryKey && p.TypeSql == "UNIQUEIDENTIFIER").Select(s =>
                   {
                       return
                           $"ALTER TABLE [{options.tableName}] ADD  CONSTRAINT [DF_{options.tableName}_{s.Name}]  DEFAULT (newsequentialid()) FOR [{s.Name}];";
                   }).ToArray();

                if (!string.IsNullOrEmpty(pks))
                {
                    contentCreate.Add($" PRIMARY KEY({pks})");
                }

                string commandCreate = $"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'{options.tableName}')" +
                                       $" CREATE TABLE [{options.tableName}] ({string.Join(", ", contentCreate)}); {string.Join(",", constraint)}";

                ExecuteCommand(options, commandCreate);

                if (internalTransaction) options.Transaction.Commit();
            }
            catch (Exception e)
            {
                if (internalTransaction) options.Transaction.Rollback();
                throw;
            }
        }

        #endregion

        #region  Drop Table

        public static void DropTable<TContext, TSource>(this IDatabase<TContext> context, Action<IDropTableOptions> action)
        {
            var options = new DropTableOptions();
            action(options);

            using (var conn = (IDbConnection)context.Connection)
            {
                options.Connection = conn;
                DropTable(options);
            }
        }

        public static void DropTable(this IDbConnection connection, Action<IDropTableOptions> action)
        {
            var options = new DropTableOptions();
            options.Connection = connection;
            action(options);
            DropTable(options);
        }


        public static void DropTable(Action<IDropTableOptions> action)
        {
            var options = new DropTableOptions();
            action(options);
            DropTable(options);
        }

        public static void DropTable(IDropTableOptions options)
        {
            if (string.IsNullOrEmpty(options.tableName))
            {
                throw new ArgumentNullException(nameof(options.tableName));
            }

            string command = (options.useIfExist ? $"IF OBJECT_ID(N'{options.tableName}', N'U') IS NOT NULL" : "") +
                             $" DROP TABLE [{options.tableName}];";

            ExecuteCommand(options, command);
        }

        private static void ExecuteCommand(IDbOperationOptions options, string command)
        {
            if (options.Connection.State == ConnectionState.Closed) options.Connection.Open();

            bool internalTransaction = false;
            if (options.Transaction == null)
            {
                internalTransaction = true;
                options.Transaction = options.Connection.BeginTransaction();
            }

            try
            {
                using (IDbCommand cmd = options.Connection.CreateCommand())
                {
                    cmd.Connection = options.Connection;
                    cmd.Transaction = options.Transaction;
                    cmd.CommandText = command;
                    cmd.ExecuteNonQuery();
                }

                if (internalTransaction)
                {
                    options.Transaction.Commit();
                }
            }
            catch (Exception e)
            {
                if (internalTransaction)
                {
                    options.Transaction.Rollback();
                }

                throw;
            }
        }

        public static IEnumerable<dynamic> DataReaderMapToDynamic(this IDataReader dr)
        {
            var names = Enumerable.Range(0, dr.FieldCount).Select(dr.GetName).ToList();
            foreach (IDataRecord record in dr as IEnumerable)
            {
                var expando = new ExpandoObject() as IDictionary<string, object>;
                foreach (var name in names)
                    expando[name] = record[name];

                yield return expando;
            }
        }


        public static List<T> DataReaderMapTo<T>(this IDataReader dr) where T : class, new()
        {
            List<T> list = new List<T>();
            T obj = new T();
            List<PropertyInfo> props = obj.GetType().GetProperties().AsEnumerable()
                .Where(p =>
                    Enumerable.Range(0, dr.FieldCount).Any(i =>
                        string.Equals(dr.GetName(i), p.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            while (dr.Read())
            {
                obj = new T();
                foreach (PropertyInfo prop in props)
                {
                    if (!object.Equals(dr[prop.Name], DBNull.Value))
                    {
                        prop.SetValue(obj, dr[prop.Name], null);
                    }
                }
                list.Add(obj);
            }
            return list;
        }

        private static IEnumerable<T> ExecuteCommand<T>(IDbOperationOptions options, string command) where T : class, new()
        {
            var result = Enumerable.Empty<T>();
            if (options.Connection.State == ConnectionState.Closed) options.Connection.Open();

            bool internalTransaction = false;
            if (options.Transaction == null)
            {
                internalTransaction = true;
                options.Transaction = options.Connection.BeginTransaction();
            }

            try
            {
                using (IDbCommand cmd = options.Connection.CreateCommand())
                {
                    cmd.Connection = options.Connection;
                    cmd.Transaction = options.Transaction;
                    cmd.CommandText = command;
                    using (var reader = cmd.ExecuteReader())
                    {
                        result = reader.DataReaderMapTo<T>();
                    }
                }

                if (internalTransaction)
                {
                    options.Transaction.Commit();
                }

                return result;
            }
            catch (Exception e)
            {
                if (internalTransaction)
                {
                    options.Transaction.Rollback();
                }

                throw;
            }
        }

        #endregion

        #region BulkInsert
        public static IDbConnection BulkInsert<TContext, TSource>(this IDatabase<TContext> context, IEnumerable<TSource> data, Action<IBulkInsertOptions<TSource>> action)
        {
            var opts = new BulkInsertOptions<TSource>();
            action(opts);


            using (IDbConnection conn = (IDbConnection)context.Connection)
            {
                if (conn.State == ConnectionState.Closed) conn.Open();
                IDbTransaction trans = conn.BeginTransaction();
                try
                {
                    BulkInsert<TSource>(data, opts);
                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
            return (IDbConnection)context.Connection;
        }

        public static void BulkInsert<TSource>(this IDbConnection conn, IEnumerable<TSource> data, Action<IBulkInsertOptions<TSource>> action)
        {
            var opts = new BulkInsertOptions<TSource>();
            if (conn.State == ConnectionState.Closed) conn.Open();

            opts.Connection = conn;
            action(opts);

            bool internalTransaction = false;
            if (opts.Transaction == null)
            {
                opts.Transaction = opts.Connection.BeginTransaction();
                internalTransaction = true;
            }

            try
            {
                BulkInsert(data, opts);
                if (internalTransaction) opts.Transaction.Commit();
            }
            catch (Exception)
            {
                if (internalTransaction) opts.Transaction.Rollback();
                throw;
            }
        }

        public static void BulkInsert<TSource>(IEnumerable<TSource> data, IBulkInsertOptions<TSource> options)
        {
            if (string.IsNullOrEmpty(options.tableName))
            {
                throw new ArgumentNullException(nameof(options.tableName));
            }

            var fieldsCreate = GetProperties<TSource>(options.primaryKeys, options.includeColumns, options.excludeColumns);

            if (options.Connection.State == ConnectionState.Closed) options.Connection.Open();
            bool internalTransaction = false;

            if (options.Transaction == null)
            {
                internalTransaction = true;
                options.Transaction = options.Connection.BeginTransaction();
            }

            if (options.dropTableIfExist)
            {
                DropTable(opt =>
                {
                    opt.useIfExist = options.dropTableIfExist;
                    opt.tableName = options.tableName;
                    opt.Connection = options.Connection;
                    opt.Transaction = options.Transaction;
                });
            }

            if (options.createTableIfNotExist)
            {
                CreateTable<TSource>(opt =>
                {
                    opt.tableName = options.tableName;
                    opt.primaryKey = options.primaryKeys;
                    opt.primaryKeyAutoIncrement = options.primaryKeyAutoIncrement;
                    opt.excludeColumns = options.excludeColumns;
                    opt.includeColumns = options.includeColumns;
                    opt.dropIfExists = options.dropTableIfExist;
                    opt.excludePrimaryKeyColumns = options.excludePrimaryKeyColumns;
                    opt.Connection = options.Connection;
                    opt.Transaction = options.Transaction;
                });
            }

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(options.Connection as SqlConnection, SqlBulkCopyOptions.KeepNulls & SqlBulkCopyOptions.KeepIdentity, options.Transaction as SqlTransaction))
            {
                bulkCopy.DestinationTableName = $"{options.tableName}";
                bulkCopy.BulkCopyTimeout = options.ConnectionTimeout;
                bulkCopy.ColumnMappings.Clear();
                fieldsCreate.ForEach(e => bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(e.Name, e.Name)));

                using (GenericListDataReader<TSource> dataReader = new GenericListDataReader<TSource>(data, fieldsCreate.Select(s => s.fieldInfo).ToList()))
                {
                    bulkCopy.WriteToServer(dataReader);
                }

                //bulkCopy.Close();
            }
        }

        public static IEnumerable<TSource> BulkInsertWithReturn<TSource>(this IDbConnection connection, IEnumerable<TSource> data, Action<IBulkInsertOptions<TSource>> action) where TSource : class, new()
        {
            var opts = new BulkInsertOptions<TSource>();
            opts.Connection = connection;
            action(opts);


            if (opts.Connection.State == ConnectionState.Closed) opts.Connection.Open();

            bool internalTransaction = false;
            if (opts.Transaction == null)
            {
                opts.Transaction = opts.Connection.BeginTransaction();
                internalTransaction = true;
            }

            IEnumerable<TSource> result;
            try
            {
                result = BulkInsertWithResult<TSource>(data, opts);
                if (internalTransaction) opts.Transaction.Commit();
                return result;
            }
            catch (Exception)
            {
                if (internalTransaction) opts.Transaction.Rollback();
                throw;
            }
        }

        private static IEnumerable<TSource> BulkInsertWithResult<TSource>(IEnumerable<TSource> data, IBulkInsertOptions<TSource> options) where TSource : class, new()
        {
            if (string.IsNullOrEmpty(options.tableName))
            {
                throw new ArgumentNullException(nameof(options.tableName));
            }

            //Bulk in Temp Table
            string tempTable = $"{options.tableName}_{DateTime.Now:yyyyMMddHHmmss}";

            BulkInsert(data, new BulkInsertOptions<TSource>()
            {
                tableName = tempTable,
                createTableIfNotExist = true,
                dropTableIfExist = true,
                Connection = options.Connection,
                Transaction = options.Transaction,
                includeColumns = options.includeColumns,
                excludeColumns = options.excludeColumns,
                ConnectionTimeout = options.ConnectionTimeout,
                primaryKeys = null
            });


            if (options.dropTableIfExist)
            {
                DropTable(opt =>
                {
                    opt.useIfExist = options.dropTableIfExist;
                    opt.tableName = options.tableName;
                    opt.Connection = options.Connection;
                    opt.Transaction = options.Transaction;
                });
            }

            if (options.createTableIfNotExist)
            {
                CreateTable<TSource>(opt =>
                {
                    opt.tableName = options.tableName;
                    opt.primaryKey = options.primaryKeys;
                    opt.excludeColumns = options.excludeColumns;
                    opt.includeColumns = options.includeColumns;
                    opt.dropIfExists = options.dropTableIfExist;
                    opt.Connection = options.Connection;
                    opt.Transaction = options.Transaction;
                });
            }

            //Bulk to Real Table
            var props = GetProperties(options.primaryKeys, options.includeColumns, options.excludeColumns);
            var pk = props.Where(p => p.IsPrimaryKey);
            var columns = props.Where(p => !p.IsPrimaryKey)
                .Select(s => s.Name);

            if (!pk.Any())
            {
                throw new ArgumentNullException(nameof(options.primaryKeys));
            }

            string pkCreateStr = string.Join(",", pk.Select(s => $"{s.Name} {s.TypeSql} NOT NULL "));
            string pkStr = string.Join(",", pk.Select(s => $"{s.Name}"));
            string pkStrFromInserted = string.Join(",", pk.Select(s => $" INSERTED.{s.Name} "));
            string joinField = string.Join(" AND ", pk.Select(s => $" tp.{s.Name} = tr.{s.Name} "));
            string columnsStr = string.Join(", ", columns);

            string sql = $" DECLARE @tmpTbl as TABLE(intID bigint not null identity(1,1), {pkCreateStr} ); " +
                         $"INSERT [{options.tableName}] " +
                         $"({columnsStr}) " +
                         $"OUTPUT {pkStrFromInserted} INTO @tmpTbl({pkStr}) " +
                         $"SELECT {columnsStr} FROM {tempTable}; " +
                         $"SELECT tr.* FROM {options.tableName} tr " +
                         $" JOIN @tmpTbl tp on {joinField}; " +
                         $"DROP TABLE {tempTable} ;";

            return ExecuteCommand<TSource>(options, sql);
        }

        #endregion

        #region BulkUpdate
        public static IDbConnection BulkUpdate<TContext, TSource>(this IDatabase<TContext> context, IEnumerable<TSource> data, Action<IBulkUpdateOptions<TSource>> action)
        {
            var opts = new BulkUpdateOptions<TSource>();
            action(opts);

            using (IDbConnection conn = (IDbConnection)context.Connection)
            {
                if (conn.State == ConnectionState.Closed) conn.Open();
                IDbTransaction trans = conn.BeginTransaction();
                try
                {
                    BulkUpdate<TSource>(data, opts);
                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
            return (IDbConnection)context.Connection;
        }

        public static void BulkUpdate<TSource>(this IDbConnection conn, IEnumerable<TSource> data, Action<IBulkUpdateOptions<TSource>> action)
        {
            var opts = new BulkUpdateOptions<TSource>();
            opts.Connection = conn;
            action(opts);

            if (opts.Connection.State == ConnectionState.Closed) opts.Connection.Open();
            bool internalTransaction = false;
            if (opts.Transaction == null)
            {
                opts.Transaction = opts.Connection.BeginTransaction();
                internalTransaction = true;
            }

            try
            {
                BulkUpdate(data, opts);
                if (internalTransaction) opts.Transaction.Commit();
            }
            catch (Exception)
            {
                if (internalTransaction) opts.Transaction.Rollback();
                throw;
            }
        }
        public static void BulkUpdate<TSource>(IEnumerable<TSource> data, IBulkUpdateOptions<TSource> options)
        {
            string tempTable = $"{options.tableName}_{DateTime.Now:yyyyMMddHHmmss}";
            string joinParams = "";

            if (string.IsNullOrEmpty(options.tableName))
            {
                throw new ArgumentNullException(nameof(options.tableName));
            }

            if (options.joinColumns == null)
            {
                throw new ArgumentNullException(nameof(options.joinColumns));
            }

            if (options.Connection.State == ConnectionState.Closed) options.Connection.Open();
            bool internalTransaction = false;

            if (options.Transaction == null)
            {
                internalTransaction = true;
                options.Transaction = options.Connection.BeginTransaction();
            }

            BulkInsert(data, new BulkInsertOptions<TSource>()
            {
                Connection = options.Connection,
                Transaction = options.Transaction,
                tableName = tempTable,
                primaryKeys = options.joinColumns,
                includeColumns = options.includeColumns,
                excludeColumns = options.excludeColumns,
                dropTableIfExist = true,
                createTableIfNotExist = true,
                excludeprimaryKeys = options.excludeprimaryKeys
            });



            var fieldsCreate = GetProperties<TSource>(options.joinColumns, null, options.excludeColumns, null);
            joinParams = string.Join(" AND ", fieldsCreate.OrderBy(p => p.Order).Where(p => p.IsPrimaryKey).Select(s => $" [ExtendedRes].{s.Name} = [ExtendedTemp].{s.Name}"));

            if (string.IsNullOrEmpty(joinParams))
            {
                throw new Exception("Nessuna chiave di confronto impostata,la tabella non ha una PrimaryKey, popolare il metodo 'joinColumns'");
            }

            var fieldsUpdatelist = GetFields(options.fieldsToUpdate);
            var fieldsUpdate = "";
            if (fieldsUpdatelist.Any())
            {
                fieldsUpdate = string.Join(" , ", fieldsUpdatelist.Select(s => $" {s} = [ExtendedTemp].{s}"));
            }

            string query = $"UPDATE [ExtendedRes] " +
                           $" SET {fieldsUpdate} " +
                           $" FROM [{tempTable}] as [ExtendedTemp] " +
                           $" INNER JOIN [{options.tableName}] as [ExtendedRes] on {joinParams} {options.joinSql} " +
                           $"{(!string.IsNullOrEmpty(options.otherSqlWhereCondition) ? " WHERE " + options.otherSqlWhereCondition : "")}; " +
                           $"DROP TABLE [{tempTable}];";

            ExecuteCommand(options, query);
        }

        public static IEnumerable<TRes> BulkUpdateWithResult<TContext, TSource, TRes>(this IDatabase<TContext> context, IEnumerable<TSource> data, Action<IBulkUpdateOptions<TSource>> action) where TSource : class, new() where TRes : class, new()
        {
            var opts = new BulkUpdateOptions<TSource>();
            action(opts);


            IEnumerable<TRes> result = new List<TRes>();
            using (IDbConnection conn = (IDbConnection)context.Connection)
            {
                if (conn.State == ConnectionState.Closed) conn.Open();
                IDbTransaction trans = conn.BeginTransaction();
                try
                {
                    result = BulkUpdateWithResult<TSource, TRes>(data, opts);
                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
                }
            }
            return result;
        }

        public static IEnumerable<TRes> BulkUpdateWithResult<TSource, TRes>(this IDbConnection conn, IEnumerable<TSource> data, Action<IBulkUpdateOptions<TSource>> action) where TSource : class, new() where TRes : class, new()
        {
            var opts = new BulkUpdateOptions<TSource>();
            opts.Connection = conn;

            action(opts);
            IEnumerable<TRes> result = Enumerable.Empty<TRes>();
            if (opts.Connection.State == ConnectionState.Closed) opts.Connection.Open();

            bool internalTransaction = false;
            if (opts.Transaction == null)
            {
                opts.Transaction = opts.Connection.BeginTransaction();
                internalTransaction = true;
            }

            try
            {
                result = BulkUpdateWithResult<TSource, TRes>(data, opts);
                if (internalTransaction) opts.Transaction.Commit();
                return result;
            }
            catch (Exception)
            {
                if (internalTransaction) opts.Transaction.Rollback();
                throw;
            }
        }
        public static IEnumerable<TRes> BulkUpdateWithResult<TSource, TRes>(IEnumerable<TSource> data, IBulkUpdateOptions<TSource> options) where TSource : class, new() where TRes : class, new()
        {
            string tempTable = $"{options.tableName}_{DateTime.Now:yyyyMMddHHmmss}";
            string joinParams = "";

            if (string.IsNullOrEmpty(options.tableName))
            {
                throw new ArgumentNullException(nameof(options.tableName));
            }

            if (options.joinColumns == null)
            {
                throw new ArgumentNullException(nameof(options.joinColumns));
            }

            if (options.primaryKeys == null)
            {
                throw new ArgumentNullException(nameof(options.primaryKeys));
            }

            if (options.Connection.State == ConnectionState.Closed) options.Connection.Open();
            bool internalTransaction = false;

            if (options.Transaction == null)
            {
                internalTransaction = true;
                options.Transaction = options.Connection.BeginTransaction();
            }

            CreateTable<TSource>(opt =>
            {
                opt.Connection = options.Connection;
                opt.Transaction = options.Transaction;
                opt.tableName = tempTable;
                opt.primaryKey = options.joinColumns;
                opt.excludeColumns = options.excludeColumns;
                opt.includeColumns = options.includeColumns;
                opt.dropIfExists = true;
                opt.primaryKeyAutoIncrement = false;
            });

            BulkInsert(data, new BulkInsertOptions<TSource>()
            {
                Connection = options.Connection,
                Transaction = options.Transaction,
                tableName = tempTable,
                primaryKeys = options.joinColumns,
                includeColumns = options.includeColumns,
                excludeColumns = options.excludeColumns,
                dropTableIfExist = false,
                createTableIfNotExist = false,
                excludeprimaryKeys = options.excludeprimaryKeys
            });

            var fieldsCreate = GetProperties<TSource>(options.joinColumns, null, null, null);
            joinParams = string.Join(" AND ", fieldsCreate.OrderBy(p => p.Order).Where(p => p.IsPrimaryKey).Select(s => $" [ExtendedRes].{s.Name} = [ExtendedTemp].{s.Name}"));

            if (string.IsNullOrEmpty(joinParams))
            {
                throw new Exception("Nessuna chiave di confronto impostata,la tabella non ha una PrimaryKey, popolare il metodo 'joinColumns'");
            }

            var fieldsUpdatelist = GetFields(options.fieldsToUpdate);
            var fieldsUpdate = "";
            if (fieldsUpdatelist.Any())
            {
                fieldsUpdate = string.Join(" , ", fieldsUpdatelist.Select(s => $" {s} = [ExtendedTemp].{s}"));
            }

            var props = GetProperties(options.primaryKeys, options.includeColumns, options.excludeColumns);
            var pk = props.Where(p => p.IsPrimaryKey);
            var joinProps = GetProperties(options.joinColumns);
            var columns = props.Where(p => !p.IsPrimaryKey)
                .Select(s => s.Name);

            if (!pk.Any())
            {
                throw new ArgumentNullException(nameof(options.primaryKeys));
            }

            string pkCreateStr = string.Join(",", pk.Select(s => $"{s.Name} {s.TypeSql} NOT NULL "));
            string pkStr = string.Join(",", pk.Select(s => $"{s.Name}"));
            string pkStrFromInserted = string.Join(",", pk.Select(s => $" INSERTED.{s.Name} "));
            string joinField = string.Join(" AND ", pk.Select(s => $" tp.{s.Name} = tr.{s.Name} "));

            string query = $"DECLARE @tmpTbl as TABLE(intID bigint not null identity(1,1), {pkCreateStr} ); " +
                           $"UPDATE [ExtendedRes] " +
                           $" SET {fieldsUpdate} " +
                           $" OUTPUT {pkStrFromInserted} INTO @tmpTbl({pkStr}) " +
                           $" FROM [{tempTable}] as [ExtendedTemp] " +
                           $" INNER JOIN [{options.tableName}] as [ExtendedRes] on {joinParams} {options.joinSql} " +
                           $" {(!string.IsNullOrEmpty(options.otherSqlWhereCondition) ? " WHERE " + options.otherSqlWhereCondition : "")}; " +
                           $" SELECT tr.* FROM {options.tableName} tr " +
                           $" JOIN @tmpTbl tp on {joinField}; " +
                           $" DROP TABLE [{tempTable}];";

            return ExecuteCommand<TRes>(options, query);
        }

        #endregion

        #region Private Methods

        private static List<Property> GetProperties<TSource>(
        Expression<Func<TSource, object>> primaryKeys = null,
        Expression<Func<TSource, object>> includeColumns = null,
        Expression<Func<TSource, object>> excludeColumns = null,
        Expression<Func<TSource, object>> excludePrimaryKeys = null
            )
        {
            List<string> primaryKeysFields = new List<string>();
            List<string> excludeFields = new List<string>();
            List<string> includeFields = new List<string>();
            List<string> exludeprimaryKeysFields = new List<string>();
            if (primaryKeys != null)
            {
                primaryKeysFields = GetFields<TSource>(primaryKeys).ToList();
            }

            if (includeColumns != null)
            {
                includeFields = GetFields<TSource>(includeColumns).ToList();
            }

            if (excludeColumns != null)
            {
                excludeFields = GetFields<TSource>(excludeColumns).ToList();
            }
            if (excludePrimaryKeys != null)
            {
                exludeprimaryKeysFields = GetFields<TSource>(excludePrimaryKeys).ToList();
            }

            List<Property> fieldsCreate = new List<Property>();
            int indexPK = 0;
            foreach (PropertyInfo fieldinfo in typeof(TSource).GetProperties(
                BindingFlags.Instance |
                BindingFlags.Public)
                .Where(p => !p.GetGetMethod().IsVirtual))
            {

                bool pk = false;
                int order = 0;
                bool skipPK = false;

                //Skip Fields non contenute
                if (excludeColumns != null || includeColumns != null || primaryKeysFields != null)
                {
                    if (primaryKeysFields.Any() && primaryKeysFields.Contains(fieldinfo.Name) && !exludeprimaryKeysFields.Contains(fieldinfo.Name))
                    {
                        pk = true;
                        order = indexPK;
                        indexPK += 1;
                        skipPK = true;
                    }
                    else if (excludeFields.Any() && excludeFields.Contains(fieldinfo.Name)) continue;
                    else if (includeFields.Any() && !includeFields.Contains(fieldinfo.Name)) continue;
                }

                //PrimaryKey Attribute 
                if (!skipPK && GetAttributeFrom<KeyAttribute>(fieldinfo, fieldinfo.Name).FirstOrDefault() != null)
                {
                    pk = true;
                    //var orderAttr = GetAttributeFrom<ColumnAttribute>(fieldinfo, "Order").FirstOrDefault(); //TODO for (EntityFramework)
                    order = indexPK;
                    indexPK = order;
                    indexPK += 1;
                }

                //Fields da creare
                var tp = GetTypeSqlType(fieldinfo.GetGetMethod().ReturnType);
                fieldsCreate.Add(new Property()
                {
                    fieldInfo = fieldinfo,
                    Name = fieldinfo.Name,
                    TypeSql = tp,
                    IsNullable = GetAttributeFrom<Nullable>(fieldinfo, fieldinfo.Name).FirstOrDefault() != null || (fieldinfo.GetGetMethod().ReturnType.Name.ToLower() == "string" || IsNullable(fieldinfo.GetType()) || IsNullable(fieldinfo.GetGetMethod().ReturnType)) && !pk,
                    IsPrimaryKey = pk,
                    Order = order
                });
            }

            return fieldsCreate;
        }

        private static T[] GetAttributeFrom<T>(PropertyInfo property, string propertyName) where T : Attribute
        {
            var attrType = typeof(T);
            return (T[])property.GetCustomAttributes(attrType, false);
        }

        public static List<string> GetFields<T>(Expression<Func<T, object>> exp)
        {
            MemberExpression body = exp.Body as MemberExpression;
            var fields = new List<string>();
            if (body == null)
            {
                NewExpression ubody = exp.Body as NewExpression;
                if (ubody != null)
                    foreach (var arg in ubody.Arguments)
                    {
                        if (arg is MemberExpression me)
                            fields.Add(me.Member.Name);
                    }

                if (ubody == null)
                {
                    UnaryExpression ubody2 = exp.Body as UnaryExpression;
                    if (ubody2 != null && ubody2.Operand is MemberExpression operand)
                    {
                        fields.Add(operand.Member.Name);
                    }
                }
            }
            else
            {
                fields.Add(body.Member.Name);
            }

            return fields;
        }

        public class Nullable : Attribute { }

        private static string GetTypeSqlType(Type t)
        {
            switch (t.Name.ToUpper())
            {
                case "INT64":
                    return "BIGINT";
                case "BYTE":
                    return "TINYINT";
                case "GUID":
                    return "UNIQUEIDENTIFIER";
                case "TIMESPAN":
                    return "TIME";
                case "BYTE[]":
                    return "VARBINARY(MAX)";
                case "BOOLEAN":
                    return "BIT";
                case "DATETIMEOFFSET":
                    return "DATETIMEOFFSET";
                case "DECIMAL":
                    return "DECIMAL(18,8)";
                case "DOUBLE":
                    return "FLOAT";
                case "CHAR[]":
                    return "NVARCHAR(MAX)";
                case "SINGLE":
                    return "REAL";
                case "INT16":
                    return "SMALLINT";
                case "INT32":
                    return "INT";
                case "STRING":
                    //CASE "DBNULL":
                    return "VARCHAR(MAX)";
                case "DATETIME":
                    return "DATETIME";
                case "XML":
                    return "XML";
                case "NULLABLE`1":
                    return GetTypeSqlType(GetUnderlyingType(t));
                default:
                    throw new Exception("Type non valido");
            }
        }
        public static bool IsNullable(Type nullableType)
        {
            return GetUnderlyingType(nullableType) != null;
        }

        private static Type GetUnderlyingType(Type nullableType)
        {
            return (nullableType.IsGenericType
                    && !nullableType.IsGenericTypeDefinition
                    && (object)nullableType.GetGenericTypeDefinition() == (object)typeof(Nullable<>)
                ? nullableType.GetGenericArguments()[0]
                : (Type)null);
        }



        #endregion
    }
}
