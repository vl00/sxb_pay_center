using MediatR;
using Sxb.PayCenter.WechatPay;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.Service.PayOrder
{
    public class WxPayCallBackCommand : IRequest<bool>
    {
        public WeChatPayTransactionsNotify notify { get; set; }
        public string ReturnJson { get; set; }

    }
}
