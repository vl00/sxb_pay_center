using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw
{
    /// <summary>
    /// 查询结算类
    /// </summary>
    public class QueryWithdrawDto : IRequest<List<Domain.Entities.Withdraw>>
    {
        /// <summary>
        /// 用户
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 结算方式
        /// </summary>
        public int? WithdrawWay { get; set; }

        /// <summary>
        /// 结算编号
        /// </summary>
        public string WithdrawNo { get; set; }

        /// <summary>
        /// 审核人
        /// </summary>
        public Guid VerifyUserId { get; set; }
    }
}
