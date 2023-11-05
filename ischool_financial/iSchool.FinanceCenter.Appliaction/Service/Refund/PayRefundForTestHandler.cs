using CSRedis;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Statement;
using iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder;
using iSchool.FinanceCenter.Appliaction.Service.Statement;
using iSchool.FinanceCenter.Domain;
using iSchool.FinanceCenter.Domain.Entities.iSchool.FinanceCenter.Domain.Entities;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.FinanceCenter.Domain.Redis;
using iSchool.Infrastructure;
using iSchool.Infrastructure.UoW;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sxb.GenerateNo;
using Sxb.PayCenter.WechatPay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Refund
{
    public class PayRefundForTestHandler : IRequestHandler<PayRefundForTestCommand, RefundResult>
    {
        private readonly IWeChatPayClient _client;
        private readonly IRepository<Domain.Entities.PayOrder> _payOrderRepo;
        private readonly IRepository<Domain.Entities.WxPayCallBackLog> _payCallBackRepo;
        private readonly WeChatPayOptions _wechatConfig;
        private readonly IRepository<Domain.Entities.RefundOrder> _refundOrderRepo;
        CSRedisClient _redis;
        private readonly ISxbGenerateNo _sxbGenerateNo;
        public PayRefundForTestHandler(CSRedisClient redis, IFinanceCenterUnitOfWork financeUnitOfWork, IRepository<Domain.Entities.RefundOrder> refundOrderRepo, IOptions<WeChatPayOptions> wechatPayConfig, IRepository<Domain.Entities.PayOrder> payOrderRepo, IWeChatPayClient client
            , ISxbGenerateNo sxbGenerateNo, IRepository<Domain.Entities.WxPayCallBackLog> payCallBackRepo)
        {
            _payCallBackRepo = payCallBackRepo;
            _payOrderRepo = payOrderRepo;
            _client = client;
            _wechatConfig = wechatPayConfig.Value;
            _refundOrderRepo = refundOrderRepo;
            _redis = redis;
            _sxbGenerateNo = sxbGenerateNo;

        }

        public async Task<RefundResult> Handle(PayRefundForTestCommand cmd, CancellationToken cancellationToken)
        {

            return await AllRefund(cmd);



        }
        public async Task<RefundResult> AllRefund(PayRefundForTestCommand cmd)
        {

            //限制同一张订单进行多次退款
            var key = CacheKeys.WechatPayRefundOrder.FormatWith(cmd.OrderId);
            var ishandle = await _redis.ExistsAsync(key);
            if (ishandle) throw new CustomResponseException("请勿重复发起退款申请");
            try
            {
                var payorder = _payOrderRepo.Get(x => x.OrderId == cmd.OrderId && x.OrderStatus == (int)OrderStatus.PaySucess);

                if (null == payorder) throw new CustomResponseException("找不到该订单的支付订单，退款失败");

                var paydetail = _payCallBackRepo.Get(x => x.OutTradeNo == payorder.OrderNo);
                if (null == paydetail) throw new CustomResponseException("找不到该订单的支付回调记录，退款失败");
                var wechat_amout = Convert.ToInt32(payorder.PayAmount * 100);
                //防止高频调用
                await _redis.SetAsync(key, 1, TimeSpan.FromMinutes(1));

                //获取退款总记录进行验证
                var list_refund = _refundOrderRepo.GetAll(x => x.No == payorder.OrderNo && x.Status == 1);
                if (null != list_refund && list_refund.Count() > 0)
                {
                    var test = list_refund.Sum(x => x.Amount) + payorder.PayAmount;
                    if (test > payorder.TotalAmount)
                        throw new CustomResponseException("订单申请退款金额超过支付金额，退款失败");

                }

                var result = new RefundResult() { ApplySucess = false };
                //生成一个退款订单
                var refundOrderNo = $"FNC{_sxbGenerateNo.GetNumber()}";
                var refuundOrderId = Guid.NewGuid();

                var request = new WeChatPayRefundRequest
                {
                    OutRefundNo = refundOrderNo,
                    TransactionId = paydetail.TransactionId,
                    OutTradeNo = paydetail.OutTradeNo,
                    TotalFee = Convert.ToInt32(payorder.PayAmount * 100),
                    RefundFee = wechat_amout,
                    RefundDesc = "技术手动退款",
                    NotifyUrl = ConfigHelper.GetConfigs("WeChatPay", "WeChatRefundNotifyUrl")
                };
                var response = await _client.ExecuteAsync(request, _wechatConfig);
                var requestJson = JsonConvert.SerializeObject(request);
                //
                // 测试退款不记录日志，不改payorder的orderstatus
                // 日后如要记录，参考 PayRefundHandlerExpVeison.AllRefund()
                //
                if (response.ReturnCode == WeChatPayTradeState.Success)//业务申请成功
                {
                    if (response.ResultCode == WeChatPayTradeState.Success)//退款成功
                    {


                        result.ApplySucess = true;
                        result.AapplyDesc = "退款申请已经成功";
                        return result;
                    }
                    else
                    {

                        result.AapplyDesc = response.ReturnMsg;
                        return result;

                    }

                }
                else
                {


                    result.AapplyDesc = "请求微信退款业务接口返回失败";
                    return result;

                }



            }
            catch (Exception ex)
            {
                await _redis.DelAsync(key);
                throw;
            }
        }

    }
}
