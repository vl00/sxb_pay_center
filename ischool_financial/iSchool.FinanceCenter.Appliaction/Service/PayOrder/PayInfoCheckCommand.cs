using iSchool.FinanceCenter.Appliaction.RequestDto.PayOrder;
using iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder;
using MediatR;
using Sxb.PayCenter.WechatPay;
using System;

namespace iSchool.FinanceCenter.Appliaction.Service.PayOrder
{
    /// <summary>
    /// 支付中心获取订单信息
    /// </summary>
    public class PayInfoCheckCommand : IRequest<PayInfoCheckResult>
    {
        /// <summary>
        /// 订单Id
        /// </summary>
        public Guid OrderId { get; set; }
        /// <summary>
        /// 订单No
        /// </summary>
        public string OrderNo { get; set; }
      

    }
}
