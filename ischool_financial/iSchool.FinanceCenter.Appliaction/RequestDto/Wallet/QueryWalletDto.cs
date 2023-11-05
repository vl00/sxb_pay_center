using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.Wallet
{
    /// <summary>
    /// 钱包参数类
    /// </summary>
    public class QueryWalletDto : IRequest<List<Domain.Entities.Wallet>>
    {
        /// <summary>
        /// 用户
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
