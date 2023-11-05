using MediatR;
using Sxb.PayCenter.WechatPay;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.Service.Refund
{
    public class WxPayReundCallBackCommand : IRequest<bool>
    {
        public WeChatPayRefundNotify notify { get; set; }


    }
}
