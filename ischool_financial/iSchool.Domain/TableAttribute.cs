using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.Domain
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public TableAttribute(string tableName)
        {
            Name = tableName;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string Name { get; set; }
    }
}
