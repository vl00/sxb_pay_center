using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Enum
{
   /// <summary>
   /// 退款类型
   /// </summary>
    public enum RefundTypeEnum
    {
        /// <summary>
        /// 全部
        /// </summary>
        [Description("全部")]
        All = 1,

        /// <summary>
        /// 子单
        /// </summary>
        [Description("子单")]
        ChildOrder = 2,
        /// <summary>
        /// 子单里面单个商品
        /// </summary>
        [Description("子单里面单个商品")]
        ProductOrder = 3,
        /// <summary>
        /// 运费
        /// </summary>
        [Description("运费")]
        Freight =4
    }
}
