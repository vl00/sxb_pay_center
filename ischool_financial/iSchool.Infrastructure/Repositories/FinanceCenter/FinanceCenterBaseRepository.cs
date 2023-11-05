using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Domain;

namespace iSchool.Infrastructure.Repositories.FinanceCenter
{
    /// <summary>
    /// 备注delete还差修改MidifyDateTime跟Modifier
    /// 这里的查询，只有单表的。多表查询需要自己写
    /// 这里实现基础仓储的接口，提供访问数据库的方法
    /// </summary>
    /// <typeparam name="Tentiy"></typeparam>
    public class FinanceCenterBaseRepository<Tentiy> : BaseRepository<Tentiy>, IBaseRepository<Tentiy> where Tentiy : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="IUnitOfWork"></param>
        public FinanceCenterBaseRepository(IFinanceCenterUnitOfWork IUnitOfWork) : base(IUnitOfWork)
        {

        }
    }
}
