using System;

namespace HitsSniffer.Model.Attrs
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DbTableNameAttribute : Attribute
    {
        public string Name { get; }
        public bool MainTable { get; }

        private DbTableNameAttribute()
        {
        }

        public DbTableNameAttribute(string name)
        {
            Name = name;
        }

        public DbTableNameAttribute(string name, bool mainTable)
        {
            Name = name;
            MainTable = mainTable;
        }
    }
}