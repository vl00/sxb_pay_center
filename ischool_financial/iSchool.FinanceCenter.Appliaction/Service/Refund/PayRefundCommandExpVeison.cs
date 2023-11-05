using iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder;
using iSchool.FinanceCenter.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.Service.Refund
{
    public class PayRefundCommandExpVeison : IRequest<RefundResult>
    {
        /// <summary>
        /// 预支付订单ID
        /// </summary>
        public Guid AdvanceOrderId { get; set; }
        /// <summary>
        /// 订单ID  
        /// </summary>
        public Guid OrderId { get; set; }
        /// <summary>
        /// 订单单个产品Id
        /// </summary>
        public Guid OrderDetailId { get; set; }
        /// <summary>
        /// 退款金额--单位元
        /// </summary>
        public decimal RefundAmount { get; set; }

        /// <summary>
        /// 退款说明
        /// </summary>
        public string Remark { get; set; }
        public int System { get; set; }
        /// <summary>
        /// 产品ID
        /// </summary>
        public Guid ProductId { get; set; }
        /// <summary>
        /// 退款类型
        /// </summary>
        public RefundTypeEnum RefundType { get; set; }
        /// <summary>
        ///  退款的价格，数量.(因存在统一个商品不同价格问题。前端决定退具体哪个)
        /// </summary>
        public List<AppplyPrice> RefundProductInfo { get; set; }


    }
    public class AppplyPrice
    {
        /// <summary>
        /// 退SKU时，退的数量
        /// </summary>

        public int RefundProductNum { get; set; }
        /// <summary>
        ///   单个商品退的原价
        /// </summary>
        public decimal RefundProductPrice{ get; set; }
        /// <summary>
        ///  单个商品退的金额 
        /// </summary>
        public decimal Amount { get; set; }
    }
}
