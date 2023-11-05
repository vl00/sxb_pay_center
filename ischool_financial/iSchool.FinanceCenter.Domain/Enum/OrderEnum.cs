using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Enum
{
    /// <summary>
    /// 订单系统
    /// </summary>
    public enum OrderSystem
    {
        /// <summary>
        /// 问答系统
        /// </summary>
        [Description("问答系统")]
        Ask = 1,

        /// <summary>
        /// 机构
        /// </summary>
        [Description("机构")]
        Org = 2,
        /// <summary>
        /// 分销
        /// </summary>
        [Description("分销")]
        Fx = 3,
        /// <summary>
        ///学校
        /// </summary>
        [Description("学校")]
        School =4,
    }

    /// <summary>
    /// 订单状态
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// 待支付
        /// </summary>
        [Description("待支付")]
        Wait = 1,
        /// <summary>
        /// 进行中
        /// </summary>
        [Description("进行中")]
        Process = 2,

        /// <summary>
        /// 已全额退款
        /// </summary>
        [Description("已全额退款")]
        Refund = 5,

        /// <summary>
        /// 支付成功
        /// </summary>
        [Description("支付成功")]
        PaySucess = 6,

        /// <summary>
        /// 支付失败
        /// </summary>
        [Description("支付失败")]
        PayFaile = 7,
        /// <summary>
        /// 订单撤销
        /// </summary>
        [Description("订单撤销")]
        Cancel =8,
        /// <summary>
        /// 已部分退款
        /// </summary>
        [Description("已部分退款")]
        PartRefund = 9,
    }
    /// <summary>
    ///  ProductrderStatus
    /// </summary>

    public enum ProductOrderType
    {  /// <summary>
       /// 课程
       /// </summary>
        [Description("课程")]
        Course = 1,
        /// <summary>
        /// 商品
        /// </summary>
        [Description("商品")]
        Good = 2,
        /// <summary>
        /// 运费
        /// </summary>
        [Description("运费")]
        Freight =3,
        /// <summary>
        /// 学校付费查阅
        /// </summary>
        [Description("学校付费查阅")]
        SchoolInfoPayRead = 4


    }

}
