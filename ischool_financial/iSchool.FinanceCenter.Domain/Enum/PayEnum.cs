using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Enum
{
    /// <summary>
    /// 支付类型
    /// </summary>
    public enum PayTypeEnum
    {

        [Description("充值")]
        Recharge = 1,
        [Description("重新支付")]
        RePay = 1
    }
    /// <summary>
    /// 支付方式
    /// </summary>
    public enum PayWayEnum
    {
        [Description("微信支")]
        WeChatPay = 1,
        [Description("支付宝")]
        AliPay = 2,

    }
    /// <summary>
    /// 支付状态
    /// </summary>
    public enum PayStatusEnum
    {

        [Description("待支付")]
        InProcess = 0,
        [Description("成功")]
        Success = 1,
        [Description("失败")]
        Fail = -1
    }
    public enum RefundStatusEnum
    {

        [Description("待退")]
        InProcess = 0,
        [Description("退款申请成功")]
        ApplySuccess = 1,
        [Description("退款成功")]
        Sucess =2,
        [Description("失败")]
        Fail = -1
    }

    /// <summary>
    /// 公司支付状态
    /// </summary>
    public enum CompanyPayStatusEnum
    {
        /// <summary>
        /// 申请
        /// </summary>
        [Description("申请")]
        Apply = 0,

        /// <summary>
        /// 成功
        /// </summary>
        [Description("成功")]
        Success = 1,

        /// <summary>
        /// 失败
        /// </summary>
        [Description("失败")]
        Fail = 2,

        /// <summary>
        /// 支付中
        /// </summary>
        [Description("支付中")]
        Payping = 3,
    }
}
