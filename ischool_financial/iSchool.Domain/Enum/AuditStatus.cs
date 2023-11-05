using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.Domain.Enum
{
    //public enum AuditStatus
    //{
    //    /// <summary>
    //    /// 初始状态
    //    /// </summary>
    //    Initial = 0,
    //    /// <summary>
    //    /// 未审核
    //    /// </summary>
    //    UnAudit = 1,
    //    /// <summary>
    //    /// 审核中
    //    /// </summary>
    //    InAudit = 2,
    //    /// <summary>
    //    /// 成功
    //    /// </summary>
    //    Succes = 3,
    //    /// <summary>
    //    /// 失败
    //    /// </summary>
    //    Failed = 4
    //}

    /// <summary>
    /// 学校审核状态
    /// </summary>
    public enum SchoolAuditStatus
    {
        /// <summary>
        /// 待审核
        /// </summary>
        [Description("待审核")]
        UnAudit = 1,
        /// <summary>
        /// 审核处理中
        /// </summary>
        [Description("审核处理中")]
        InAudit = 2,
        /// <summary>
        /// 审核数据提交中
        /// </summary>
        [Description("审核数据提交中")]
        Auditing = 3,
        /// <summary>
        /// 已发布
        /// </summary>
        [Description("已发布")]
        Success = 4,
        /// <summary>
        /// 需修改
        /// </summary>
        [Description("需修改")]
        Failed = 5,
    }
}
