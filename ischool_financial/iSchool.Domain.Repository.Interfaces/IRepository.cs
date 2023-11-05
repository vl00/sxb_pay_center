using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.Domain.Repository.Interfaces
{
    /// <summary>
    ///基础仓储的接口
    /// </summary>
    /// <typeparam name="Tentiy"></typeparam>
    public interface IRepository<Tentiy> : IDependency where Tentiy : class
    {
        /// <summary>
        /// 根据条件查询内容
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        IEnumerable<Tentiy> GetAll(Expression<Func<Tentiy, bool>> expression);


        /// <summary>
        /// 查询全部
        /// </summary>
        /// <returns></returns>
        IEnumerable<Tentiy> GetAll();

        /// <summary>
        /// 根据主键查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Tentiy Get(int id);

        /// <summary>
        /// 根据主键查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Tentiy Get(string id);

        /// <summary>
        /// 根据主键Guid 查询单个对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Tentiy Get(Guid id);

        /// <summary>
        /// 根据where条件获取单个对象
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        Tentiy Get(Expression<Func<Tentiy, bool>> expression);
        /// <summary>
        /// 根据where 条件获取有效值
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        Tentiy GetIsValid(Expression<Func<Tentiy, bool>> expression);

        /// <summary>
        /// 根据主键查询有效数据
        /// </summary>
        /// <param name="id">主键</param>
        /// <returns></returns>
        Tentiy GetIsValid<T>(T id);

        /// <summary>
        /// 根据指定字段查询
        /// </summary>
        /// <param name="filed"></param>
        /// <param name="value"></param>
        /// <param name="isValid">是否仅包含有些值</param>
        /// <returns></returns>
        Tentiy GetByFiled(string filed, string value, bool isValid = true);


        /// <summary>
        /// 根据指定字段查询
        /// </summary>
        /// <param name="filed"></param>
        /// <param name="value"></param>
        /// <param name="isValid">是否仅包含有些值</param>
        /// <returns></returns>
        IEnumerable<Tentiy> GetListByFiled(string filed, string value);

        /// <summary>
        ///根据key（字段）/value字典查询List
        /// </summary>
        /// <param name="fileds">字典集合</param>
        /// <param name="isValid"></param>
        /// <returns></returns>
        IEnumerable<Tentiy> GetListByFileds(Dictionary<string, string> fileds);



        /// <summary>
        /// in 查询
        /// </summary>
        /// <param name="array">数组</param>
        /// <param name="field">查询字段</param>
        /// <param name="isValid">是否仅包含有些值</param>
        /// <returns>返回多个对象</returns>
        IEnumerable<Tentiy> GetInArray(string[] array, string field = "Id");

        /// <summary>
        /// 新增单个对象
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        int Insert(Tentiy entity);

        /// <summary>
        /// 批量新增
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        int BatchInsert(List<Tentiy> list);

        /// <summary>
        /// 修改对象
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool Update(Tentiy entity);

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        bool BatchUpdate(List<Tentiy> list);

        /// <summary>
        /// 修改指定字段
        /// </summary>
        /// <param name="filed">要修改的字段</param>
        /// <param name="value">修改的值</param>
        /// <param name="objectID">where 唯一ID</param>
        /// <returns></returns>
        bool UpdateByFiled(string filed, object value, Guid objectID);


        /// <summary>
        /// 根据主键删除对象（set IsValid =0）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        int Delete(int id);

        /// <summary>
        /// 根据主键删除对象（set IsValid =0）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        int Delete(string id);


        /// <summary>
        /// 根据主键删除对象（set IsValid =0）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        int Delete(Guid id);



        /// <summary>
        /// 异步删除 根据主键删除对象（set IsValid =0）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<int> DelectAsync(Guid id);
        /// <summary>
        /// 根据指定字段删除
        /// </summary>
        /// <param name="filed"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        int DeleteByFiled(string filed, string value);

        /// <summary>
        /// 根据指定字段集删除
        /// </summary>
        /// <returns></returns>
        int DeleteByFileds(Dictionary<string, string> fileds);

        /// <summary>
        /// 是否存在某条件的元素
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        bool IsExist(Expression<Func<Tentiy, bool>> expression);

        /// <summary>
        /// 原始sql语句查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        IEnumerable<Tentiy> Query(string sql, object param = null);

        /// <summary>
        /// 原始sql语句查询总笔数
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<int> QueryCount(string sql, object param = null);

        /// <summary>
        /// 原始sql语句执行
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<bool> ExecuteAsync(string sql, object param = null);

        /// <summary>
        /// 原始sql语句事务执行
        /// </summary>
        /// <param name="sqls"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        Task<bool> Executes(List<string> sqls, List<object> param = null);

    }
}
