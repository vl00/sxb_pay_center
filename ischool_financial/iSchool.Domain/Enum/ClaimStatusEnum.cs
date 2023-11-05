using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Domain.Enum
{
  
    /// <summary>
    /// 认领状态
    /// </summary>
    public enum ClaimStatusEnum
    {
        /// <summary>
        /// 待确定
        /// </summary>
        [Description("待确定")]
        ToBeConfirmed = 1,

        /// <summary>
        /// 已认领
        /// </summary>
        [Description("已认领")]
        Claimed = 2,

        /// <summary>
        /// 拒绝
        /// </summary>
        [Description("拒绝")]
        Rejected = 3,

        /// <summary>
        /// 已取消
        /// </summary>
        [Description("已取消")]
        Cancelled = 4,

    }
}
