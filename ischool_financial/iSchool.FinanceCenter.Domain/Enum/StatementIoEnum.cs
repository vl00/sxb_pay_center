using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Enum
{
    /// <summary>
    /// 流水支出/收入
    /// </summary>
    [Description("流水支出/收入")]
    public enum StatementIoEnum
    {
        /// <summary>
        /// 支出
        /// </summary>
        [Description("支出")]
        Out = 1,

        /// <summary>
        /// 收入
        /// </summary>
        [Description("收入")]
        In = 2,
    }
}
