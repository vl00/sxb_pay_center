using MediatR;
using System;
using System.Collections.Generic;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.PayOrder
{
    /// <summary>
    /// 支付订单查询类
    /// </summary>
    public class QueryPayOrderDto : IRequest<List<Domain.Entities.PayOrder>>
    {
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
        public int OrderType { get; set; }

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
    }
}
