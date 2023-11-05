using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace System.Data
{
    public static class DbExtension
    {
        public static IDbConnection TryOpen(this IDbConnection connection)
        {
            if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
            {
                connection.Open();
            }

            return connection;
        }

        public static DbConnection TryOpen(this DbConnection connection)
        {
            if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
                connection.Open();

            return connection;
        }

        public static async Task<IDbConnection> TryOpenAsync(this IDbConnection connection)
        {
            if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
            {
                if (connection is DbConnection conn)
                    await conn.OpenAsync().ConfigureAwait(false);
                else
                    connection.Open();
            }

            return connection;
        }

        public static async Task<DbConnection> TryOpenAsync(this DbConnection connection)
        {
            if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
                await connection.OpenAsync().ConfigureAwait(false);

            return connection;
        }
    }
}
