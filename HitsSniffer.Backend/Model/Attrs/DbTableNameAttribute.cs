using System;

namespace HitsSniffer.Model.Attrs
{
    public class DbTableNameAttribute : Attribute
    {
        public string Name { get; }

        private DbTableNameAttribute()
        {
        }

        public DbTableNameAttribute(string name)
        {
            Name = name;
        }
    }
}