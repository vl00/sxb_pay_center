using Dapper.Contrib.Extensions;
using iSchool.FinanceCenter.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Entities
{
    /// <summary>
    /// 支付订单表实体类
    /// </summary>
    [Table("payOrder")]
    public partial class PayOrder
    {
        /// <summary>
        /// id
        /// </summary>
        [ExplicitKey]
        public Guid Id { get; set; }
        /// <summary>
        /// 支付订单ID
        /// </summary>
        public Guid OrderId { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        public OrderTypeEnum OrderType { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public int OrderStatus { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// 支付金额
        /// </summary>
        public decimal PayAmount { get; set; }

        /// <summary>
        /// 折扣金额
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// 已退款金额
        /// </summary>
        public decimal RefundAmount { get; set; }


        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 区别来自哪个系统
        /// </summary>
        public int System { get; set; }
        /// <summary>
        /// 软删除
        /// </summary>
        public int IsDelete { get; set; } = 0;
        public string SourceOrderNo { get; set; }

        /// <summary>
        /// 订单过期时间 订单失效时间
        /// </summary>
        public DateTime? OrderExpireTime { get; set; }

    }
}
