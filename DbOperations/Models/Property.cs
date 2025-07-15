using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Solution.DbOperations.Models
{
    public class Property
    {
        public PropertyInfo fieldInfo { get; set; }
        public string Name { get; set; }
        public string TypeSql { get; set; }
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public int Order { get; set; }
    }
}
