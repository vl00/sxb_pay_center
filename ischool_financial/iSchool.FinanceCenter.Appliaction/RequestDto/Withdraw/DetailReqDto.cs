using iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw
{
    /// <summary>
    /// 提现详情参数
    /// </summary>
    public class DetailReqDto : IRequest<WithdrawDetailResult>
    {
        /// <summary>
        /// 提现编号
        /// </summary>
        public string No { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public Guid UserId { get; set; }
    }
}
