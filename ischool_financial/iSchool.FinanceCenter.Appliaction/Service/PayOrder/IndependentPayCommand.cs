using iSchool.FinanceCenter.Appliaction.RequestDto.PayOrder;
using MediatR;
using Sxb.PayCenter.WechatPay;
using System;

namespace iSchool.FinanceCenter.Appliaction.Service.PayOrder
{
    /// <summary>
    /// 发起支付
    /// </summary>
    public class IndependentPayCommand : IRequest<WeChatPayDictionary>
    {
        /// <summary>
        /// 订单Id
        /// </summary>
        public Guid OrderId { get; set; }
        /// <summary>
        /// 订单No
        /// </summary>
        public string OrderNo { get; set; }

        /// 0 h5jsapi支付 1小程序支付 2h5支付
        /// </summary>
        public int IsWechatMiniProgram { get; set; }
        /// <summary>
        /// 小程序appid
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// openid
        /// </summary>
        public string OpenId { get; set; }
       

    }
}
