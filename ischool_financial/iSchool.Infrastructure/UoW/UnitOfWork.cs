using iSchool.Domain;
using System;
using System.Data;
using System.Data.SqlClient;

namespace iSchool.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {

        public IDbConnection DbConnection { get; set; }
        public IDbTransaction DbTransaction { get; set; }

        public IDbConnection ReadDbConnection { get; set; }

        public UnitOfWork(string connectionString)
        {
            //数据库链接
            var sb = new SqlConnectionStringBuilder(connectionString) { }.ToString();
            DbConnection = new SqlConnection(sb);
            ReadDbConnection = DbConnection;
        }


        //初始话读写分离connection
        public UnitOfWork(string connectionString, string readConnnectionString)
        {
            //数据库链接
            var sb = new SqlConnectionStringBuilder(connectionString) { }.ToString();
            DbConnection = new SqlConnection(sb);

            //只读数据库
            var readSb = new SqlConnectionStringBuilder(readConnnectionString) { }.ToString();
            ReadDbConnection = new SqlConnection(readSb);
        }



        /// <summary>
        /// 开始事务
        /// </summary>
        public void BeginTransaction()
        {
            if (DbTransaction != null)
            {
                return;
            }
            DbTransaction = DbConnection.TryOpen().BeginTransaction();
        }

        /// <summary>
        /// 
        /// </summary>
        public void CommitChanges()
        {
            if (DbTransaction != null)
            {
                DbTransaction.Commit();
                DbTransaction.Dispose();
                DbTransaction = null;
                DbConnection.Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            //请求系统不要调用指定对象的终结器。
            GC.SuppressFinalize(this);
        }

        public void Rollback()
        {
            if (DbTransaction != null)
            {
                DbTransaction.Rollback();
                DbTransaction.Dispose();
                DbTransaction = null;
                DbConnection.Close();
            }
        }


        /// <summary>
        /// 子类重写
        /// </summary>
        /// <param name="disposing"></param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (DbTransaction != null)
                {
                    DbTransaction.Dispose();
                    DbTransaction = null;
                }
                if (DbConnection != null)
                {
                    if (DbConnection.State == ConnectionState.Open) { DbConnection.Close(); }
                    DbConnection.Dispose();
                    DbConnection = null;
                }
            }
        }


        /// <summary>
        /// 析构函数
        /// 当客户端没有显示调用Dispose()时由GC完成资源回收功能
        /// </summary>
        ~UnitOfWork()
        {
            Dispose(false);
        }
    }
}

