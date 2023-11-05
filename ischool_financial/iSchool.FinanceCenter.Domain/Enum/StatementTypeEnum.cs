using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Enum
{
    /// <summary>
    /// 流水类型枚举
    /// </summary>
    [Description("流水类型枚举")]
    public enum StatementTypeEnum
    {
        /// <summary>
        /// 充值
        /// </summary>
        [Description("充值")]
        Recharge = 1,

        /// <summary>
        /// 支出
        /// </summary>
        [Description("支出")]
        Outgoings = 2,

        /// <summary>
        /// 收入
        /// </summary>
        [Description("收入")] 
        Incomings = 3,

        /// <summary>
        /// 提现(结算)
        /// </summary>
        [Description("提现")]
        Settlement = 4,

        /// <summary>
        /// 服务费
        /// </summary>
        [Description("服务费")]
        ServiceFee = 5,

        /// <summary>
        /// 冻结
        /// </summary>
        [Description("冻结")] 
        Blocked = 6,

        /// <summary>
        /// 解冻
        /// </summary>
        [Description("解冻")]
        Unfreeze = 7,

        /// <summary>
        /// 扣费
        /// </summary>
        [Description("扣费")]
        Deduct = 8,

        /// <summary>
        /// 关闭(只针对冻结期的订单)
        /// </summary>
        [Description("关闭")]
        Close = 9,
    }
}
