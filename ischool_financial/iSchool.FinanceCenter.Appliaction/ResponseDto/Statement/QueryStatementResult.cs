using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.ResponseDto.Statement
{
    /// <summary>
    /// 流水查询结果类
    /// </summary>
    public class QueryStatementResult
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 流水
        /// </summary>
        public decimal SumAmount { get; set; }

    }
}
