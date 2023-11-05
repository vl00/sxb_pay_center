using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.PayOrder
{
    /// <summary>
    /// 修改支付订单类
    /// </summary>
    public class UpdatePayOrderDto : IRequest<bool>
    {
        /// <summary>
        /// 订单编号
        /// </summary>
        public Guid OrderId { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public int OrderStatus { get; set; }

        /// <summary>
        /// 修改订单产品信息
        /// </summary>
        public List<UpdateProduct> UpdateProducts { get; set; }
    }

    /// <summary>
    /// 修改订单产品信息类
    /// </summary>
    public class UpdateProduct
    {
        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 产品id
        /// </summary>
        public Guid productId { get; set; }
    }
}
