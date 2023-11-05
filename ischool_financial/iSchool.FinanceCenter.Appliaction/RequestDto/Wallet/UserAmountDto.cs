using iSchool.FinanceCenter.Appliaction.ResponseDto.Wallet;
using MediatR;
using System;
using System.Collections.Generic;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.Wallet
{
    /// <summary>
    /// 用户金额
    /// </summary>
    public class UserAmountDto: IRequest<List<UserAmountResult>>
    {
        /// <summary>
        /// 多用户id
        /// </summary>
        public List<Guid> UserIds { get; set; }
    }
}
