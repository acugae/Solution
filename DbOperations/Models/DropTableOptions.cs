using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Solution.DbOperations.Infrastracture;

namespace Solution.DbOperations.Models
{
    public class DropTableOptions : IDropTableOptions
    {
        public IDbConnection Connection { get; set; } = null;
        public IDbTransaction Transaction { get; set; } = null;
        public int ConnectionTimeout { get; set; } = 500000;
        public string tableName { get; set; } = string.Empty;
        public bool useIfExist { get; set; } = false;
    }
}
