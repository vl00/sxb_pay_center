using iSchool.FinanceCenter.Appliaction.ResponseDto.CompanyPay;
using MediatR;
using System;

namespace iSchool.FinanceCenter.Appliaction.Service.CompanyPay
{
    public class WechatCompanyPayCommand : IRequest<PromotionTransfersResult>
    {
        /// <summary>
        /// 提现金额(单位元)
        /// </summary>
        public decimal Amount { get; set; }
       /// <summary>
       /// 到账的微信用户openid
       /// </summary>
        public string  OpenId { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; } = "提现成功";
        /// <summary>
        /// 用户ID
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// 提现单号
        /// </summary>
        public string WithDrawNo { get; set; }
        /// <summary>
        /// 商户付款订单Id----目前是打款失败重新进行打款使用
        /// </summary>
        public Guid CompanyPayOrderId { get; set; } = Guid.Empty;
        /// <summary>
        /// 商户付款订单No----目前是打款失败重新进行打款使用
        /// </summary>
        public string CompanyPayOrderNo { get; set; }
        /// <summary>
        /// 申请提现终端的appid
        /// </summary>
        public string  AppId { get; set; }
    }
}
