using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.ResponseDto.Wallet
{
    /// <summary>
    /// 我的钱包结果类
    /// </summary>
    public class MyWalletResult
    {
        /// <summary>
        /// 总收益
        /// </summary>
        public decimal TotalIncomes { get; set; }

        /// <summary>
        /// 可提现金额
        /// </summary>
        public decimal WithdrawalAmount { get; set; }

        /// <summary>
        /// 待结算金额
        /// </summary>
        public decimal WaitSettlementAmount { get; set; }
    }

    
}
