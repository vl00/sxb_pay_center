using iSchool.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Dapper;
using System.Threading.Tasks;
using static Dapper.SqlMapper;
using Dapper.Contrib.Extensions;

namespace iSchool.Infrastructure.UoW
{
    public static class DBContext
    {
        #region ------------------------Read---------------------------------

        #region Query

        [Obsolete]
        public static IEnumerable<T> Query<T, K>(this K uow, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null) where K : UnitOfWork
        {
            return uow.ReadDbConnection.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType);
        }

        public static IEnumerable<T> Query<T>(this UnitOfWork uow, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType);
        }

        public static Task<IEnumerable<T>> QueryAsync<T>(this UnitOfWork uow, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType);
        }

        public static IEnumerable<dynamic> Query(this UnitOfWork uow, string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.Query(sql, param, transaction, buffered, commandTimeout, commandType);
        }

        public static Task<IEnumerable<dynamic>> QueryAsync(this UnitOfWork uow, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.QueryAsync(sql, param, transaction, commandTimeout, commandType);
        }

        #endregion Query

        public static GridReader QueryMultiple(this UnitOfWork uow, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.QueryMultiple(sql, param, transaction, commandTimeout, commandType);
        }

        public static Task<GridReader> QueryMultipleAsync(this UnitOfWork uow, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.QueryMultipleAsync(sql, param, transaction, commandTimeout, commandType);
        }

        #region QueryFirstOrDefault

        public static T QueryFirstOrDefault<T>(this UnitOfWork uow, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.QueryFirstOrDefault<T>(sql, param, transaction, commandTimeout, commandType);
        }

        public static Task<T> QueryFirstOrDefaultAsync<T>(this UnitOfWork uow, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.QueryFirstOrDefaultAsync<T>(sql, param, transaction, commandTimeout, commandType);
        }

        public static dynamic QueryFirstOrDefault(this UnitOfWork uow, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.QueryFirstOrDefault(sql, param, transaction, commandTimeout, commandType);
        }

        public static Task<dynamic> QueryFirstOrDefaultAsync(this UnitOfWork uow, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.QueryFirstOrDefaultAsync(sql, param, transaction, commandTimeout, commandType);
        }

        #endregion QueryFirstOrDefault

        #region QueryFirst

        public static T QueryFirst<T>(this UnitOfWork uow, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.QueryFirst<T>(sql, param, transaction, commandTimeout, commandType);
        }

        public static Task<T> QueryFirstAsync<T>(this UnitOfWork uow, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.QueryFirstAsync<T>(sql, param, transaction, commandTimeout, commandType);
        }

        public static dynamic QueryFirst(this UnitOfWork uow, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.QueryFirst(sql, param, transaction, commandTimeout, commandType);
        }

        public static Task<dynamic> QueryFirstAsync(this UnitOfWork uow, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.QueryFirstAsync(sql, param, transaction, commandTimeout, commandType);
        }

        #endregion QueryFirst

        #region Query splitOn map

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(this UnitOfWork uow, string sql, Func<TFirst, TSecond, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
        }
        public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(this UnitOfWork uow, string sql, Func<TFirst, TSecond, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
        }

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(this UnitOfWork uow, string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
        }
        public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(this UnitOfWork uow, string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
        }

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(this UnitOfWork uow, string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
        }
        public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(this UnitOfWork uow, string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
        }

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(this UnitOfWork uow, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
        }
        public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(this UnitOfWork uow, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
        }

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(this UnitOfWork uow, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
        }
        public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(this UnitOfWork uow, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
        }

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(this UnitOfWork uow, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.Query(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
        }
        public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(this UnitOfWork uow, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.ReadDbConnection.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType);
        }

        #endregion Query splitOn map

        #endregion ------------------------Read---------------------------------

        #region ------------------------Write---------------------------------

        public static int Execute(this UnitOfWork uow, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.DbConnection.Execute(sql, param, transaction, commandTimeout, commandType);
        }

        public static Task<int> ExecuteAsync(this UnitOfWork uow, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.DbConnection.ExecuteAsync(sql, param, transaction, commandTimeout, commandType);
        }

        [Obsolete]
        public static Task<T> ExecuteScalarAsync<T, K>(this K uow, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
            where K : UnitOfWork
        {
            return uow.DbConnection.ExecuteScalarAsync<T>(sql, param, transaction, commandTimeout, commandType);
        }

        public static T ExecuteScalar<T>(this UnitOfWork uow, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.DbConnection.ExecuteScalar<T>(sql, param, transaction, commandTimeout, commandType);
        }

        public static Task<T> ExecuteScalarAsync<T>(this UnitOfWork uow, string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return uow.DbConnection.ExecuteScalarAsync<T>(sql, param, transaction, commandTimeout, commandType);
        }

        #endregion ------------------------Write---------------------------------
    }
}
