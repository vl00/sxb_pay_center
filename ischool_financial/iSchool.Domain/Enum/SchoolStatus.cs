using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Domain.Enum
{
    /// <summary>
    /// 学校状态 
    /// </summary>
    public enum SchoolStatus
    {
        /// <summary>
        /// 初始状态
        /// </summary>
        [Description("初始状态")]
        Initial = 0,
        /// <summary>
        /// 编辑中
        /// </summary>
        [Description("编辑中")]
        Edit = 1,
        /// <summary>
        /// 审核中
        /// </summary>
        [Description("审核中")]
        InAudit = 2,
        /// <summary>
        /// 成功
        /// </summary>
        [Description("成功")]
        Success = 3,
        /// <summary>
        /// 失败
        /// </summary>
        [Description("需修改")]
        Failed = 4,
    }
}
