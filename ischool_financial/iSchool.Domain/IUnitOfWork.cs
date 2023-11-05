using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace iSchool.Domain
{
    /// <summary>
    ///工作单元接口
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// 开始事务
        /// </summary>
        void BeginTransaction();
        /// <summary>
        /// 提交更改
        /// </summary>
        void CommitChanges();

        /// <summary>
        ///回滚
        /// </summary>
        void Rollback();
    }
}
