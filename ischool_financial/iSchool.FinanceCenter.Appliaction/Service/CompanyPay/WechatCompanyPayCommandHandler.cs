using CSRedis;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.Http;
using iSchool.FinanceCenter.Appliaction.RequestDto;
using iSchool.FinanceCenter.Appliaction.ResponseDto.CompanyPay;
using iSchool.FinanceCenter.Appliaction.Service.WechatTemplateMsg;
using iSchool.FinanceCenter.Domain;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Extensions;
using iSchool.Infrastructure.UoW;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sxb.GenerateNo;
using Sxb.PayCenter.WechatPay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


namespace iSchool.FinanceCenter.Appliaction.Service.CompanyPay
{

    public class WechatCompanyPayCommandHandler : IRequestHandler<WechatCompanyPayCommand, PromotionTransfersResult>
    {
        private readonly IInsideHttpRepository _insideHttpRepo;
        private readonly IRepository<Domain.Entities.CompanyPayOrder> _compayPayOrderRepo;
        private readonly IWeChatPayClient _client;
        private readonly FinanceCenterUnitOfWork financeUnitOfWork;
        CSRedisClient _redis;
        IHttpClientFactory httpClientFactory;
        private readonly WeChatPayOptions _wechatConfig;
        private readonly IMediator _mediator;
        private readonly ILogger<WechatCompanyPayCommandHandler> _log;
        /// <summary>
        /// 提现
        /// </summary>
        /// <param name="client"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="financeUnitOfWork"></param>
        /// <param name="redis"></param>
        /// <param name="wechatPayConfig"></param>
        /// <param name=""></param>
        public WechatCompanyPayCommandHandler(IInsideHttpRepository insideHttpRepo, ILogger<WechatCompanyPayCommandHandler> log, IMediator mediator, IWeChatPayClient client, IHttpClientFactory httpClientFactory, IFinanceCenterUnitOfWork financeUnitOfWork, CSRedisClient redis, IOptions<WeChatPayOptions> wechatPayConfig, IRepository<Domain.Entities.CompanyPayOrder> compayPayOrderRepo)
        {
            _insideHttpRepo = insideHttpRepo;
            _mediator = mediator;
            this.financeUnitOfWork = (FinanceCenterUnitOfWork)financeUnitOfWork;
            this._redis = redis;
            this.httpClientFactory = httpClientFactory;
            _client = client;
            _wechatConfig = wechatPayConfig.Value;
            _compayPayOrderRepo = compayPayOrderRepo;
            this._log = log;

        }


        public async Task<PromotionTransfersResult> Handle(WechatCompanyPayCommand request, CancellationToken cancellationToken)
        {
            var result = new PromotionTransfersResult() { OperateResult = false };
            try
            {


                if (Guid.Empty == request.UserId || request.OpenId.IsNullOrEmpty()) throw new CustomResponseException($"参数缺失");
                var wechat_amout = Convert.ToInt32(request.Amount * 100);
                //金额限制
                var limitMin = ConfigHelper.GetConfigInt("WithDrawMinMoney");
                if (wechat_amout < limitMin) throw new CustomResponseException($"提现最小申请{limitMin}元");

                string OrderNo = request.CompanyPayOrderNo;
                Guid OrderId = request.CompanyPayOrderId;
                if (Guid.Empty == request.CompanyPayOrderId)//首次支付
                {
                    //创建提现订单
                    OrderNo = $"FNC{new SxbGenerateNo().GetNumber()}";
                    OrderId = Guid.NewGuid();
                    var addResult = await CreateCompanyPayOrder(request, OrderNo, OrderId);
                    if (!addResult)
                    {
                        result.AapplyDesc = "支付中心创建企业支付订单失败";
                        return result;
                    }


                }

                var wechatRequest = new WeChatPayPromotionTransfersRequest
                {
                    PartnerTradeNo = OrderNo,
                    OpenId = request.OpenId,
                    CheckName = "NO_CHECK",//是否校验真实姓名，FORCE_CHECK为强制校验
                    ReUserName = "",
                    Amount = wechat_amout,
                    Desc = request.Remark,

                };

                var wechatConfig = _wechatConfig.Clone();
                if (!request.AppId.IsNullOrEmpty())
                {
                    wechatConfig.AppId = request.AppId;//如果是小程序用户的openid，appid要跟着
                }

                _log.LogInformation(wechatConfig.ToJsonString());
                _log.LogInformation(wechatRequest.ToJsonString());
                var response = await _client.ExecuteAsync(wechatRequest, wechatConfig);
                if (response.ReturnCode == WeChatPayTradeState.Success)//业务申请成功,通信标志状态
                {
                    if (response.ResultCode == WeChatPayTradeState.Success)//支付成功
                    {
                        var r = await UpdateCompanyPayOrder(WechatCompanyPayOrderStatus.Success, OrderId, response.PaymentNo, response.ResultCode, response.ReturnMsg + ":" + response.ErrCodeDes);
                        if (r)
                        {

                            result.OperateResult = true;
                            result.AapplyDesc = "企业支付成功";
                            #region 微信通知
                            var msgOpenId = request.OpenId;
                            //小程序兼容处理
                            if (!request.AppId.IsNullOrEmpty())
                            {
                                var listOpenIds = await _insideHttpRepo.GetUserOpenIds(new List<Guid>() { request.UserId });
                                var modelOpenId = listOpenIds.FirstOrDefault(x => x.AppId == request.AppId);
                                if (null != modelOpenId)
                                    msgOpenId = modelOpenId.OpenId;

                            }

                            await Task.Factory.StartNew(() =>
                            {


                                var msgReq = new WechatTemplateSendCommand()
                                {
                                    OpenId = request.OpenId,
                                    KeyWord1 = $"您申请提现（{request.Amount.ToString("#0.00")}元）已审批到账。",
                                    KeyWord2 = DateTime.Now.ToDateTimeString(),
                                    Remark = "点击更多查看详情",
                                    MsyType = WechatMessageType.提现到账通知,

                                };

                                _mediator.Send(msgReq);
                            });
                            #endregion
                            result.CompanyPayOrderId = OrderNo;
                            return result;

                        }
                        result.OperateResult = true;
                        result.AapplyDesc = "企业支付成功已经成功,支付中心修改企业支付订单失败";
                        return result;
                    }
                    else
                    { //错误码信息，注意：出现未明确的错误码时（SYSTEMERROR等），请务必用原商户订单号重试，或通过查询接口确认此次付款的结果。
                        var r = await UpdateCompanyPayOrder(WechatCompanyPayOrderStatus.Fail, OrderId, response.PaymentNo, response.ResultCode, response.ReturnMsg + ":" + response.ErrCodeDes);
                        if (r)
                        {

                            result.AapplyDesc = response.ReturnMsg + ":" + response.ErrCodeDes;
                            return result;
                        }
                        result.AapplyDesc = "支付中心修改企业支付订单状态失败";
                        return result;

                    }

                }
                else
                {
                    //判断errcode
                    var r = await UpdateCompanyPayOrder(WechatCompanyPayOrderStatus.Fail, OrderId, response.PaymentNo, response.ResultCode, $"请求企业支付成功业务接口返回失败:{response.ReturnMsg}");
                    if (r)
                    {

                        result.AapplyDesc = " 请求微信企业支付成功业务接口返回失败";
                        return result;
                    }
                    result.AapplyDesc = "请求微信企业支付接口返回失败&&支付中心修改企业支付订单状态失败";
                    return result;

                }
            }
            catch (Exception ex)
            {

                return result;
            }





        }
        private Task<bool> CreateCompanyPayOrder(WechatCompanyPayCommand cmd, string no, Guid orderid)
        {
            var sql = @"INSERT INTO dbo.CompanyPayOrder
                        (ID,No,CreateTime,Status,Remark,Amount,UserId,WithDrawNo)
                        VALUES
                         (@ID,@No,@CreateTime,@Status,@Remark,@Amount,@UserId,@WithDrawNo)";
            var param = new
            {
                ID = orderid,
                No = no,
                CreateTime = DateTime.Now,
                Status = (int)WechatCompanyPayOrderStatus.Wait,
                Remark = cmd.Remark,
                Amount = cmd.Amount,
                UserId = cmd.UserId,
                WithDrawNo = cmd.WithDrawNo
            };
            return _compayPayOrderRepo.ExecuteAsync(sql, param);
        }
        private Task<bool> UpdateCompanyPayOrder(WechatCompanyPayOrderStatus status, Guid orderid, string pay_platformid, string resultCode, string resultDesc)
        {



            var sql = "Update dbo.CompanyPayOrder Set Status=@Status,UpdateTime=@UpdateTime,PayPlatfomPayId=@PayPlatfomPayId,AapplyResultStr=@AapplyResultStr,ApplyResultCode=@ApplyResultCode where Id=@ID;";
            var param = new
            {
                ID = orderid,
                UpdateTime = DateTime.Now,
                Status = (int)status,
                PayPlatfomPayId = pay_platformid,
                AapplyResultStr = resultDesc,
                ApplyResultCode = resultCode,



            };
            return _compayPayOrderRepo.ExecuteAsync(sql, param);
        }

    }
}
