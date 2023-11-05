using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace iSchool
{
    public class SchFType0TypeHandler : Dapper.SqlMapper.ITypeHandler
    {
        public void SetValue(IDbDataParameter parameter, object value)
        {
            if (value is SchFType0 schFType)
            {
                parameter.Value = schFType.ToString();
            }
        }

        public object Parse(Type destinationType, object value)
        {
            if (value == null || value is DBNull) return null;
            if (destinationType == typeof(SchFType0))
            {
                return SchFType0.Parse(value.ToString());
            }
            throw new NotSupportedException();
        }
    }
}
