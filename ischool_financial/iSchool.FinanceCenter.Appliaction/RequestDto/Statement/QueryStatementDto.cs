
using iSchool.FinanceCenter.Appliaction.ResponseDto.Statement;
using MediatR;
using System;
using System.Collections.Generic;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.Statement
{
    /// <summary>
    /// 账单流水查询条件类
    /// </summary>
    public class QueryStatementDto : IRequest<List<QueryStatementResult>>
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
    }
}
