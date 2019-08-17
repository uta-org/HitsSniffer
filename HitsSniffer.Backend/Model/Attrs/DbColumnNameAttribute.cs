using System;

namespace HitsSniffer.Model.Attrs
{
    public class DbColumnNameAttribute : Attribute
    {
        public string Name { get; }

        private DbColumnNameAttribute()
        {
        }

        public DbColumnNameAttribute(string name)
        {
            Name = name;
        }
    }
}