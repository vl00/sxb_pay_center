using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Enum
{
    /// <summary>
    /// 订单组合查询
    /// </summary>
    public enum OrderTypeGroupEnum
    {
        /// <summary>
        /// 机构
        /// </summary>
        [Description("机构")]
        Org = 1,

        /// <summary>
        /// 上学问
        /// </summary>
        [Description("上学问")]
        Ask = 2,
            
        /// <summary>
        /// 分销收益
        /// </summary>
        [Description("分销收益")]
        Fx = 3,

        /// <summary>
        /// 提现
        /// </summary>
        [Description("提现")]
        Withdraw = 4,

         /// <summary>
        /// 种草收益 
        /// </summary>
        [Description("种草收益")]
        OrgZc = 5
    }
}
