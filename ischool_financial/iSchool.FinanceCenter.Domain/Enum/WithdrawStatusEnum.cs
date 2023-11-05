using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Enum
{
    /// <summary>
    /// 处理状态
    /// </summary>
    public enum WithdrawStatusEnum
    {
        /// <summary>
        /// 发起申请
        /// </summary>
        [Description("待审核")]
        Apply = 1,

        /// <summary>
        /// 审核不通过
        /// </summary>
        [Description("不通过")]
        Refuse = 2,

        /// <summary>
        /// 审核通过
        /// </summary>
        [Description("通过")]
        Pass = 3,
        /// <summary>
        /// 同步第三方,目前是交给高登处理
        /// </summary>
        [Description("同步第三方")]
        SyncThirdParty=4
    }
}
