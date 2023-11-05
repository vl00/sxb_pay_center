using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace iSchool.Infrastructure
{
    public interface ISqlConnectionFactory
    {
        IDbConnection GetOpenConnection();
    }
}
