using System.Data;
using System.Data.SqlClient;

namespace iSchool.Infrastructure
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;
        private IDbConnection _connection;

        public SqlConnectionFactory(string connectionString)
        {
            this._connectionString = connectionString;
        }
        public IDbConnection GetOpenConnection()
        {
            if (this._connection == null || this._connection.State != ConnectionState.Open)
            {
                this._connection = new SqlConnection(_connectionString);
                this._connection.Open();
            }

            return this._connection;
        }

    }
}
