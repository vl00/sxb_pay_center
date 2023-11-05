using iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder;
using iSchool.FinanceCenter.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.Service.Refund
{
    public class PayRefundCommand : IRequest<RefundResult>
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
        /// 退款类型
        /// </summary>
        public RefundTypeEnum RefundType { get; set; }
    }
}
