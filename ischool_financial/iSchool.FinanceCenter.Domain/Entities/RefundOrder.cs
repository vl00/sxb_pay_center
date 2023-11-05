using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Entities
{ 
    ///<summary>
  ///退款订单表
  ///</summary>
    [Table("RefundOrder")]
    public partial class RefundOrder
    {
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [ExplicitKey]
        public Guid ID { get; set; }
        /// <summary>
        /// Desc:退库流水号
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string No { get; set; }
        /// <summary>
        /// Desc:订单ID
        /// Default:
        /// Nullable:False
        /// </summary>           
        public Guid OrderId { get; set; }
        /// <summary>
        /// Desc:创建时间
        /// Default:
        /// Nullable:False
        /// </summary>           
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// -1退款失败 0 待退款 1申请成功2退款成功
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 支付平台返回的退款订单号
        /// </summary>
        public string PayPlatfomRefundId { get; set; }
        /// <summary>
        /// 支付平台返回的退款请求结果
        /// </summary>
        public string AapplyResultStr { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 退款申请金额
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// 1全部 2子单 3子单得某个商品
        /// </summary>
        public int Type { get; set; }
        public Guid PayOrderId { get; set; }
    }
}
