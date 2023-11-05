using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.ResponseDto.Wallet
{
    /// <summary>
    /// 用户金额
    /// </summary>
    public class UserAmountResult
    {
        /// <summary>
        /// 待结算金额
        /// </summary>
        public decimal BlockedAmount { get; set; }

        /// <summary>
        /// 可提现金额
        /// </summary>
        public decimal RemainAmount { get; set; }

        /// <summary>
        /// 已提现金额
        /// </summary>
        public decimal WithdrawAmount { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public Guid UserId { get; set; }
    }
}
