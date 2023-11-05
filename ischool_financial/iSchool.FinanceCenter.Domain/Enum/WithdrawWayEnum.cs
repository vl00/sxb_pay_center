using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Enum
{
    /// <summary>
    /// 提现方式 
    /// </summary>
    public enum WithdrawWayEnum
    {
        /// <summary>
        /// 公司结账
        /// </summary>
        [Description("公司结账")]
        Company = 1,

        /// <summary>
        /// 微信提现
        /// </summary>
        [Description("微信提现")]
        WeChat = 2,

        /// <summary>
        /// 银行卡
        /// </summary>
        [Description("银行卡")]
        BankCard = 3,
    }
}
