using iSchool.FinanceCenter.Appliaction.Service.PayOrder;
using iSchool.FinanceCenter.Appliaction.Service.Refund;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sxb.PayCenter.WechatPay;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Api.Controllers
{
    [Route("weixin")]
    public class WeChatPayNotifyController : Controller
    {
        private readonly IWeChatPayNotifyClient _client;
        private readonly IOptions<WeChatPayOptions> _optionsAccessor;
        private readonly ILogger<WeChatPayNotifyController> _logger;
        private readonly IMediator _mediator;
        public WeChatPayNotifyController(ILogger<WeChatPayNotifyController> logger, IWeChatPayNotifyClient client, IOptions<WeChatPayOptions> optionsAccessor, IMediator mediator)
        {
            _client = client;
            _optionsAccessor = optionsAccessor;
            _logger = logger;
            _mediator = mediator;
        }

        /// <summary>
        /// 统一下单支付结果通知
        /// </summary>      
        [HttpPost("callback")]
        public async Task<IActionResult> Unifiedorder()
        {
            string bodyStr = null;
            try
            {
                var request = new WxPayCallBackCommand();
                Request.EnableBuffering();
                Request.Body.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = await reader.ReadToEndAsync();
                    //request.ReturnJson = bodyStr;
                    _logger.LogInformation("\n统一下单支付结果通知:{bodyStr}", bodyStr);
                }
                Request.Body.Seek(0, SeekOrigin.Begin);
                var notify = await _client.ExecuteAsync<WeChatPayTransactionsNotify>(Request, _optionsAccessor.Value);
                request.notify = notify;
                var res = await _mediator.Send(request);
                if (res)
                {
                    return WeChatPayNotifyResult.Success;
                }
                return WeChatPayNotifyResult.Failure;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "body=`{bodystr}`", bodyStr);
                return WeChatPayNotifyResult.Failure;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_0">退款使用了v2的接口，这里接收到的body为xml</param>
        /// <returns></returns>
        [HttpPost("refundcallback")]
        public async Task<IActionResult> RefundOrder(string _0)
        {
            string bodyStr = null;
            try
            {
                Request.EnableBuffering();
                Request.Body.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = await reader.ReadToEndAsync();
                }
                Request.Body.Seek(0, SeekOrigin.Begin);
                var notify = await _client.ExecuteRefundAsync<WeChatPayRefundNotify>(Request, _optionsAccessor.Value);
                var request = new WxPayReundCallBackCommand { notify = notify };
                var res = await _mediator.Send(request);
                if (res)
                {
                    return WeChatPayNotifyResult.Success;

                }
                return WeChatPayNotifyResult.Failure;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "body=`{bodystr}`", bodyStr);
                return WeChatPayNotifyResult.Failure;
            }
        }

    }

}
