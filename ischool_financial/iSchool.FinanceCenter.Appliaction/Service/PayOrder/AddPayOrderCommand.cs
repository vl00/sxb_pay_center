using iSchool.FinanceCenter.Appliaction.RequestDto.PayOrder;
using MediatR;
using Sxb.PayCenter.WechatPay;

namespace iSchool.FinanceCenter.Appliaction.Service.PayOrder
{
    /// <summary>
    /// 发起支付
    /// </summary>
    public class AddPayOrderCommand : IRequest<WeChatPayDictionary>
    {
        /// <summary>
        /// 参数
        /// </summary>
        public AddPayOrderDto Param { get; set; }
    }
}
