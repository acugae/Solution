using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace Solution.DbOperations
{
    public class GenericListDataReader<T> : IDataReader
    {
        private IEnumerator<T> list = null;
        private List<PropertyInfo> fields;

        public GenericListDataReader(IEnumerable<T> listElements, List<PropertyInfo> fields)
        {
            list = listElements.GetEnumerator();
            this.fields = fields;
        }

        public void Dispose()
        {
            list.Dispose();
        }

        public string GetName(int i)
        {
            return fields[i].Name;
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            return fields[i].PropertyType;
        }

        public object GetValue(int i)
        {
            return fields[i].GetValue(list.Current, (object[])null);

        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public int GetOrdinal(string name)
        {
            return fields.FindIndex(p => p.Name == name);
        }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        public int FieldCount
        {
            get { return fields.Count; }
        }

        public object this[int i] => throw new NotImplementedException();

        public object this[string name] => throw new NotImplementedException();

        public void Close()
        {
            list.Dispose();
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public bool Read()
        {
            return list.MoveNext();
        }

        public int Depth { get; }
        public bool IsClosed { get; }
        public int RecordsAffected { get; }
    }
}
