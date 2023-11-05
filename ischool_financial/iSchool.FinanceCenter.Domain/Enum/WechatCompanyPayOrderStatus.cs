using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Enum
{
    /// <summary>
    /// 微信企业付款订单状态
    /// </summary>
    public enum WechatCompanyPayOrderStatus
    {
        /// <summary>
        /// 待支付
        /// </summary>
        [Description("待支付")]
        Wait =0,
        /// <summary>
        /// 支付成功
        /// </summary>
       [Description("支付成功")]
        Success = 1,
        /// <summary>
        /// 支付失败
        /// </summary>
        [Description("支付失败")]
        Fail =2
    }
}
