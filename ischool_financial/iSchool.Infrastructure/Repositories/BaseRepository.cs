using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Domain;
using iSchool.Domain.Repository.Interfaces;
using iSchool.Infrastructure.Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.Infrastructure.Repositories
{
    /// <summary>
    /// 备注delete还差修改MidifyDateTime跟Modifier
    /// 这里的查询，只有单表的。多表查询需要自己写
    /// 这里实现基础仓储的接口，提供访问数据库的方法
    /// </summary>
    /// <typeparam name="Tentiy"></typeparam>
    public class BaseRepository<Tentiy> : IRepository<Tentiy> where Tentiy : class
    {
        protected IDbConnection ReadConnection { get { return UnitOfWork.ReadDbConnection; } }
        protected IDbConnection Connection { get { return UnitOfWork.DbConnection; } }
        protected IDbTransaction Transaction { get { return UnitOfWork.DbTransaction; } }

        public UnitOfWork UnitOfWork { get; set; }

        public BaseRepository(IUnitOfWork IUnitOfWork)
        {
            UnitOfWork = (UnitOfWork)IUnitOfWork;
        }
        /// <summary>
        /// 根据主键 查询单个对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Tentiy Get(string id)
        {
            return ReadConnection.Get<Tentiy>(id, transaction: Transaction);
        }


        /// <summary>
        /// 查询全部
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tentiy> GetAll()
        {
            return ReadConnection.Query<Tentiy>(GetAllSql(), new { }, transaction: Transaction);
        }

        /// <summary>
        /// 根据条件查询内容
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public IEnumerable<Tentiy> GetAll(Expression<Func<Tentiy, bool>> expression)
        {
            DynamicParameters Parameters = new DynamicParameters();
            var sql = GetAllSql();
            if (expression != null)
            {
                WhereExpression whereExpression = new WhereExpression(expression, null);
                sql = $"{sql} and {whereExpression.SqlCmd}";
                Parameters = whereExpression.Param;
            }
            return ReadConnection.Query<Tentiy>(sql, Parameters, transaction: Transaction);
        }

        /// <summary>
        /// 根据主键int 查询单个对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Tentiy Get(int id)
        {
            return ReadConnection.Get<Tentiy>(id, transaction: Transaction);
        }
        /// <summary>
        /// 根据主键Guid 查询单个对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Tentiy Get(Guid id)
        {
            return ReadConnection.Get<Tentiy>(id, transaction: Transaction);
        }


        /// <summary>
        /// 根据条件判断是否已经存在记录
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public bool IsExist(Expression<Func<Tentiy, bool>> expression)
        {
            var query = GetCount();
            DynamicParameters Parameters = new DynamicParameters();
            if (expression != null)
            {
                WhereExpression whereExpression = new WhereExpression(expression, null);
                query = $"{query} and {whereExpression.SqlCmd}";
                Parameters = whereExpression.Param;
            }
            return ReadConnection.Query<int>(query, Parameters, transaction: Transaction).FirstOrDefault() > 0;
        }

        /// <summary>
        /// 根据where条件获取单个对象
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Tentiy Get(Expression<Func<Tentiy, bool>> expression)
        {
            var query = GetTopSql();
            DynamicParameters Parameters = new DynamicParameters();
            if (expression != null)
            {
                WhereExpression whereExpression = new WhereExpression(expression, null);
                query = $"{query} and {whereExpression.SqlCmd}";
                Parameters = whereExpression.Param;
            }
            return ReadConnection.QueryFirstOrDefault<Tentiy>(query, Parameters, transaction: Transaction);
        }

        /// <summary>
        /// 根据where条件获取有效的单个对象
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public Tentiy GetIsValid(Expression<Func<Tentiy, bool>> expression)
        {
            var query = $"{GetTopSql()} and {virtualDeleteFiled}=1";
            DynamicParameters Parameters = new DynamicParameters();
            if (expression != null)
            {
                WhereExpression whereExpression = new WhereExpression(expression, null);
                query = $"{query} and {whereExpression.SqlCmd}";
                Parameters = whereExpression.Param;
            }
            return ReadConnection.QueryFirstOrDefault<Tentiy>(query, Parameters, transaction: Transaction);
        }
        /// <summary>
        /// 根据Id查询单个有效对象
        /// </summary>
        /// <param name="objId"></param>
        /// <returns></returns>
        public Tentiy GetIsValid<T>(T objId)
        {
            return ReadConnection.Query<Tentiy>(IsValidSql(), new { objId = objId }, transaction: Transaction).FirstOrDefault();
        }


        /// <summary>
        /// 根据某个字段查询对象
        /// </summary>
        /// <param name="filed">字段</param>
        /// <param name="value">字段值</param>
        /// <param name="isValid">是否仅包含有些值</param>
        /// <returns></returns>
        public Tentiy GetByFiled(string filed, string value, bool isValid = true)
        {
            return ReadConnection.Query<Tentiy>(GetFiledSql(filed), new { filed = value }, transaction: Transaction).FirstOrDefault();
        }

        /// <summary>
        /// 根据某个字段查询对象
        /// </summary>
        /// <param name="filed">字段</param>
        /// <param name="value">字段值</param>
        /// <param name="isValid">是否仅包含有些值</param>
        /// <returns></returns>
        public IEnumerable<Tentiy> GetListByFiled(string filed, string value)
        {
            return ReadConnection.Query<Tentiy>(GetFiledListSql(filed), new { filed = value }, transaction: Transaction);
        }


        /// <summary>
        /// 根据多个条件查询
        /// </summary>
        /// <param name="fileds">字典集合</param>
        /// <param name="isValid">是否仅包含有些值</param>
        /// <returns></returns>
        public IEnumerable<Tentiy> GetListByFileds(Dictionary<string, string> fileds)
        {
            DynamicParameters parameters = ConvertParameters(fileds);
            return ReadConnection.Query<Tentiy>(GetFiledsListSql(fileds), parameters, transaction: Transaction);
        }


        /// <summary>
        /// 根据多参数查询对象
        /// </summary>
        /// <param name="array">in 的集合</param>
        /// <param name="field">in 的字段</param>
        /// <param name="isValid">是否仅包含有效字段</param>
        /// <returns>返回多个对象</returns>
        public IEnumerable<Tentiy> GetInArray(string[] array, string field = "Id")
        {
            return ReadConnection.Query<Tentiy>(InArraySql(field), new { array = array }, transaction: Transaction);
        }


        /// <summary>
        /// 新增单个对象
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>（自增ID，如果主键不是自增返回0）</returns>
        public int Insert(Tentiy entity)
        {
            return (int)Connection.Insert<Tentiy>(entity, transaction: Transaction);
        }

        /// <summary>
        /// 批量新增
        /// </summary>
        /// <param name="list"></param>
        /// <returns>影响的行数</returns>
        public int BatchInsert(List<Tentiy> list)
        {
            return (int)Connection.Insert<List<Tentiy>>(list, transaction: Transaction);
        }

        /// <summary>
        /// 修改对象
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Update(Tentiy entity)
        {
            return Connection.Update<Tentiy>(entity, transaction: Transaction);
        }
        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool BatchUpdate(List<Tentiy> list)
        {
            return Connection.Update<List<Tentiy>>(list, transaction: Transaction);
        }



        //public bool UpdateByFiled(string filed, object value, string objectID)
        //{
        //    return Connection.Execute(UpdateFiledSql(filed), new { filed = value, ObjId = objectID }, transaction: Transaction) > 0;
        //}
        /// <summary>
        /// 根据指定字段修改
        /// </summary>
        /// <param name="filed">要修改的字段</param>
        /// <param name="value">修改的值</param>
        /// <param name="objectID">where 唯一ID</param>
        /// <returns></returns>
        public bool UpdateByFiled(string filed, object value, Guid objectID)
        {
            return Connection.Execute(UpdateFiledSql(filed), new { filed = value, ObjId = objectID }, transaction: Transaction) > 0;
        }

        /// <summary>
        /// 根据主键删除对象（set IsValid =0）
        /// </summary>
        /// <param name="id">主键</param>
        /// <returns></returns>
        public int Delete(int id)
        {
            return Connection.Execute(DeleteByIdSql(), new { id = id }, transaction: Transaction);
        }

        /// <summary>
        /// 根据主键删除对象（set IsValid =0）
        /// </summary>
        /// <param name="objId">主键</param>
        /// <returns></returns>
        public int Delete(string objId)
        {
            return Connection.Execute(DeleteByObjIdSql(), new { objId = objId }, transaction: Transaction);
        }

        public int Delete(Guid id)
        {
            return Connection.Execute(DeleteByGuidSql(), new { id = id }, transaction: Transaction);
        }


        public Task<int> DelectAsync(Guid id)
        {
            return Connection.ExecuteAsync(DeleteByGuidSql(), new { id = id }, transaction: Transaction);
        }
        /// <summary>
        /// 根据指定字段删除
        /// </summary>
        /// <param name="filed"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int DeleteByFiled(string filed, string value)
        {
            return Connection.Execute(DeleteByFieldSql(filed), new { filed = value }, transaction: Transaction);
        }

        /// <summary>
        /// 根据指定字段集合进行删除(fileds 长度必须大于0)
        /// </summary>
        /// <param name="fileds"></param>
        /// <returns></returns>
        public int DeleteByFileds(Dictionary<string, string> fileds)
        {
            if (fileds != null && fileds.Count <= 0) return 0;
            DynamicParameters parameters = ConvertParameters(fileds);
            return Connection.Execute(DeleteByFieldsSql(fileds), parameters, transaction: Transaction);
        }

        /// <summary>
        /// 原始sql语句查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public IEnumerable<Tentiy> Query(string sql, object param = null)
        {

            var data = Connection.Query<Tentiy>(sql, param, transaction: Transaction);

            return data;
        }

        /// <summary>
        /// 原始sql语句查询总笔数
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<int> QueryCount(string sql, object param = null)
        {

            var data = await Connection.QueryFirstAsync<int>(sql, param, transaction: Transaction);

            return data;
        }

        /// <summary>
        /// 原始sql语句执行
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<bool> ExecuteAsync(string sql, object param = null)
        {
            var res = await Connection.ExecuteAsync(sql, param, transaction: Transaction);
            return res > 0;
        }

        /// <summary>
        /// 原始sql语句事务执行
        /// </summary>
        /// <param name="sqls"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<bool> Executes(List<string> sqls, List<object> param = null)
        {
            //事务的时候，需要自己手动打开
            Connection.Open();
            IDbTransaction transaction = Connection.BeginTransaction();
            try
            {
                var arrSql = sqls.ToArray();
                var arrpParam = param.ToArray();
                for (int i = 0; i < sqls.Count; i++)
                {
                    await Connection.ExecuteAsync(arrSql[i], arrpParam[i], transaction);
                }
                //提交事务 
                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                //回滚事务
                transaction.Rollback();
                return false;
            }
            finally
            {
                if (Connection != null)
                {
                    Connection.Close();//关闭连接
                }
                if (transaction != null)
                {
                    transaction.Dispose();//释放资源
                }
            }
            return true;
        }

        #region 私有方法，构造sql
        private readonly string virtualDeleteFiled = "IsValid";
        /// <summary>
        /// 用 根据ObjId查询
        /// </summary>
        /// <returns></returns>
        private string IsValidSql()
        {
            var sql = string.Format("SELECT * FROM  [{0}]  WHERE Id=@objId", GetTableName());
            return sql;
        }

        /// <summary>
        /// 根据指定字段查询数据,返回一个对象
        /// </summary>
        /// <returns></returns>
        private string GetFiledSql(string filed)
        {
            var sql = string.Format("SELECT top 1 * FROM  [{0}]  WHERE {1} = @filed", GetTableName(), filed);
            return sql;
        }

        /// <summary>
        /// 根据指定字段查询数据,返回一个对象
        /// </summary>
        /// <returns></returns>
        private string GetFiledListSql(string filed)
        {
            var sql = string.Format("SELECT * FROM  [{0}]  WHERE {1} = @filed", GetTableName(), filed);
            return sql;
        }

        private string GetFiledsListSql(Dictionary<string, string> fileds)
        {
            var sql = new StringBuilder()
                .Append(string.Format("SELECT * FROM  [{0}] where 1=1",
                GetTableName(), virtualDeleteFiled));
            foreach (var kvp in fileds)
            {
                sql.Append(string.Format(" and {0}=@{1} ", kvp.Key, kvp.Key));
            }
            return sql.ToString();
        }



        #region 单表查询
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string InArraySql(string filed)
        {
            var sql = string.Format("SELECT * FROM  [{0}]  WHERE {1} in @array ", GetTableName(), filed);
            return sql;
        }

        /// <summary>
        /// 用 根据guid查询
        /// </summary>
        /// <returns></returns>
        private string GetAllSql()
        {
            var sql = "SELECT * FROM  [" + GetTableName() + "]  WHERE 1=1 ";
            return sql;
        }
        /// <summary>
        /// 获取前几行数据
        /// </summary>
        /// <param name="top"></param>
        /// <returns></returns>
        private string GetTopSql(int top = 1)
        {
            var sql = $"SELECT TOP({top}) * FROM [{GetTableName()}] WHERE 1=1 ";
            return sql;
        }

        /// <summary>
        /// 获取满足条件行数
        /// </summary>
        /// <returns></returns>
        private string GetCount()
        {
            var sql = $"SELECT  Count(1) FROM [{GetTableName()}] WHERE 1=1 ";
            return sql;
        }
        #endregion

        #region 软删除

        private string UpdateFiledSql(string filed)
        {
            var sql = string.Format("UPDATE [{0}] SET {1}=@filed WHERE Id=@ObjId", GetTableName(), filed);
            return sql;
        }


        /// <summary>
        /// 用 update set isvalid 替换delete
        /// </summary>
        /// <returns></returns>
        private string DeleteByObjIdSql()
        {
            var sql = "UPDATE  [" + GetTableName() + "]  SET " + virtualDeleteFiled + " =0  WHERE ObjId=@objId";
            return sql;
        }

        private string DeleteByGuidSql()
        {
            //var sql = $"delete from {GetTableName()} where Id=@id";
            var sql = $"update {GetTableName()} set IsValid=0 where Id=@id ";
            return sql;
        }

        /// <summary>
        /// 用 update set isvalid 替换delete
        /// </summary>
        /// <returns></returns>
        private string DeleteByIdSql()
        {
            var sql = "UPDATE  [" + GetTableName() + "]  SET " + virtualDeleteFiled + " =0  WHERE ID=@id";
            return sql;
        }

        private string DeleteByFieldSql(string filed)
        {
            var sql = "UPDATE  [" + GetTableName() + "]  SET " + virtualDeleteFiled + " =0  WHERE " + filed + "=@filed";
            //var sql = $"delete from {GetTableName()}  where {filed}=@filed";
            return sql;
        }

        private string DeleteByFieldsSql(Dictionary<string, string> fileds)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append($"update {GetTableName()} set IsValid=0  where 1=1 ");
            foreach (var kvp in fileds)
            {
                sql.Append(string.Format(" and {0}=@{1} ", kvp.Key, kvp.Key));
            }
            return sql.ToString();
        }
        #endregion




        /// <summary>
        /// 根据Tentiy Attributes得到表名
        /// </summary>
        /// <returns></returns>
        private string GetTableName()
        {
            var type = typeof(Tentiy);
            string name;
            var tableattr = type.GetCustomAttributes(false).SingleOrDefault(attr => attr.GetType().Name == "TableAttribute") as dynamic;
            if (tableattr != null)
                name = tableattr.Name;
            else
            {
                name = type.Name;
                if (type.IsInterface && name.StartsWith("I"))
                    name = name.Substring(1);
            }
            return name;
        }


        /// <summary>
        /// 将Dictionary转换成dapper的参数
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        private DynamicParameters ConvertParameters(Dictionary<string, string> dic)
        {
            var dbArgs = new DynamicParameters();
            foreach (var item in dic)
                dbArgs.Add(item.Key, item.Value);
            return dbArgs;
        }











        #endregion
    }
      

}
