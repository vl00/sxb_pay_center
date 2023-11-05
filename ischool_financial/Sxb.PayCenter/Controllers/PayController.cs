using iSchool.FinanceCenter.Domain.Enum;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sxb.PayCenter.Application.Dto;
using Sxb.PayCenter.Application.ResponseModel;
using Sxb.PayCenter.Application.Services;
using Sxb.PayCenter.Models;
using Sxb.PayCenter.WechatPay;
using System;

using System.Threading.Tasks;

namespace Sxb.PayCenter.Controllers
{
    [Route("payment")]
    public class PayController : Controller
    {
        private readonly IWeChatPayClient _client;
        private readonly IOptions<WeChatPayOptions> _optionsAccessor;
        private readonly IMediator _mediator;
        public PayController(IWeChatPayClient client, IOptions<WeChatPayOptions> optionsAccessor, IMediator mediator)
        {
            _client = client;
            _optionsAccessor = optionsAccessor;
            _mediator = mediator;
        }
        /// <summary>
        /// 公众号支付-JSAPI下单
        /// </summary>
        //[HttpGet("pubpay")]
        //public IActionResult PubPay()
        //{
        //    return View();
        //}
  
        /// <summary>
        /// 公众号支付-JSAPI下单
        /// </summary>
        /// <param name="viewModel"></param>
        [ProducesResponseType(typeof(JsApiPayResponse), 200)]
        [HttpPost("weixin/jsapi")]
        public async Task<ResponseResult> PubPay(WeChatPayPubPayV3ViewModel viewModel)
        {
            var model = new WeChatPayTransactionsJsApiModel
            {
                AppId = _optionsAccessor.Value.AppId,
                MchId = _optionsAccessor.Value.MchId,
                Amount = new Amount { Total = viewModel.Total, Currency = "CNY" },
                Description = viewModel.Description,
                NotifyUrl = _optionsAccessor.Value.WeChatPayNotifyUrl,
                OutTradeNo = viewModel.OrderId.ToString(),
                Payer = new PayerInfo { OpenId = viewModel.OpenId },
                Attach=viewModel.Attach
            };

            var request = new WeChatPayTransactionsJsApiRequest();
            request.SetQueryModel(model);

            var response = await _client.ExecuteAsync(request, _optionsAccessor.Value);

            if (response.StatusCode == 200)
            {

                var req = new WeChatPayJsApiSdkRequest
                {
                    Package = "prepay_id=" + response.PrepayId
                };

                var parameter = await _client.ExecuteAsync(req, _optionsAccessor.Value);


                //添加支付记录
                var paLogAddM = new AddPayLogCommand()
                {
                    UserId = viewModel.UserId,
                    PrepayId = response.PrepayId,
                    OrderId = viewModel.OrderId,
                    PayType = (byte)PayTypeEnum.Recharge,
                    PayWay = (byte)PayWayEnum.AliPay,
                    PayStatus = (byte)PayStatusEnum.InProcess,
                    TotalAmount = viewModel.Total,
                    PostJson = JsonConvert.SerializeObject(model),
                    CreateTime = DateTime.Now,
                    ProcedureKb = 6,
                };
                var res = await _mediator.Send(paLogAddM);

                // 将参数(parameter)给 公众号前端
                // https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter3_1_4.shtml
                return ResponseResult.Success(parameter);
            }
            else//下预支付单失败
            {
            
                return ResponseResult.Failed($"微信下单错误码:{response.Code}-描述:{response.Detail}");
            }
            
        }

        /// <summary>
        /// H5支付-H5下单API
        /// </summary>
        //[HttpGet("h5pay")]
        //public IActionResult H5Pay()
        //{
        //    return View();
        //}

        /// <summary>
        /// H5支付-H5下单API
        /// </summary>
        /// <param name="viewModel"></param>
        [HttpPost("weixin/h5pay")]
        public async Task<IActionResult> H5Pay(WeChatPayH5PayV3ViewModel viewModel)
        {
            var model = new WeChatPayTransactionsH5Model
            {
                AppId = _optionsAccessor.Value.AppId,
                MchId = _optionsAccessor.Value.MchId,
                Amount = new Amount { Total = viewModel.Total, Currency = "CNY" },
                Description = viewModel.Description,
                NotifyUrl = _optionsAccessor.Value.WeChatPayNotifyUrl,
                OutTradeNo = viewModel.OutTradeNo,
                //SceneInfo = new SceneInfo { PayerClientIp = "127.0.0.1" }
            };

            var request = new WeChatPayTransactionsH5Request();
            request.SetQueryModel(model);

            var response = await _client.ExecuteAsync(request, _optionsAccessor.Value);

            // h5_url为拉起微信支付收银台的中间页面，可通过访问该url来拉起微信客户端，完成支付,h5_url的有效期为5分钟。
            // https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter3_3_4.shtml
            ViewData["response"] = response.Body;
            return View();
        }

        /// <summary>
        /// 申请退款
        /// </summary>
        //[HttpGet("refund")]
        //public IActionResult Refund()
        //{
        //    return View();
        //}

        /// <summary>
        /// 申请退款
        /// </summary>
        /// <param name="viewModel"></param>
        [HttpPost("weixin/refund")]
        public async Task<IActionResult> Refund(WeChatPayRefundViewModel viewModel)
        {
            var request = new WeChatPayRefundRequest
            {
                OutRefundNo = viewModel.OutRefundNo,
                TransactionId = viewModel.TransactionId,
                OutTradeNo = viewModel.OutTradeNo,
                TotalFee = viewModel.TotalFee,
                RefundFee = viewModel.RefundFee,
                RefundDesc = viewModel.RefundDesc,
                NotifyUrl = viewModel.NotifyUrl
            };
            var response = await _client.ExecuteAsync(request, _optionsAccessor.Value);
            ViewData["response"] = response.Body;
            return View();
        }
    }
}
