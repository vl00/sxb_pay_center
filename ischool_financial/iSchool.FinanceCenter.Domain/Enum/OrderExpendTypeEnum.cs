using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Enum
{
    /// <summary>
    /// 订单支出类型
    /// </summary>
    [Description("订单支出类型")]
    public enum OrderExpendTypeEnum
    {
        /// <summary>
        /// 冻结
        /// </summary>
        [Description("冻结")]
        Blocked = 1,

        /// <summary>
        /// 剩余
        /// </summary>
        [Description("剩余")]
        Remain = 2,

    }
}
