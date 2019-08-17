using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace HitsSniffer.Model.Attrs
{
    public class DbColumnNameAttribute : Attribute
    {
        public string Name { get; }
        public int Order { get; } = -1;
        public MySqlDbType? MySqlDbType { get; }
        public DbType? DbType { get; }

        private DbColumnNameAttribute()
        {
        }

        public DbColumnNameAttribute(string name)
        {
            Name = name;
        }

        public DbColumnNameAttribute(string name, int order)
        {
            Name = name;
            Order = order;
        }

        public DbColumnNameAttribute(string name, int order, MySqlDbType mySqlDbType, DbType dbType)
        {
            Name = name;
            Order = order;
            MySqlDbType = mySqlDbType;
            DbType = dbType;
        }
    }
}