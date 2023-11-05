using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.PayOrder;
using iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure;
using MediatR;
using Microsoft.Extensions.Options;
using Sxb.PayCenter.WechatPay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.PayOrder
{

    public class CheckOrderPayStatusHandler : IRequestHandler<CheckOrderPayStatus, OrderPayCheckResult>
    {
        private readonly IRepository<Domain.Entities.PayOrder> _payOrderRepo;
        private readonly IRepository<Domain.Entities.PayLog> _payLogRepo;
        private readonly IWeChatPayClient _client;
        private readonly WeChatPayOptions _wechatConfig;
        public CheckOrderPayStatusHandler(IRepository<Domain.Entities.PayOrder> payOrderRepo, IRepository<Domain.Entities.PayLog> payLogRepo, IWeChatPayClient client, IOptions<WeChatPayOptions> wechatPayConfig)
        {
            this._payOrderRepo = payOrderRepo;
            this._payLogRepo = payLogRepo;
            this._client = client;
            this._wechatConfig = wechatPayConfig.Value;
        }


        /// <summary>
        /// 查询支付订单
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<OrderPayCheckResult> Handle(CheckOrderPayStatus dto, CancellationToken cancellationToken)
        {
            var payorders = _payOrderRepo.GetAll(x => x.OrderId == dto.OrderId );
            if (null == payorders || payorders.Count()<=0) throw new CustomResponseException("财务系统不存在该订单");
            var result = new OrderPayCheckResult();
            if (payorders.Count(x => x.OrderStatus == (int)OrderStatus.PaySucess) > 0)
            {
                result.OrderStatus = (int)OrderStatus.PaySucess;

                //请求微信再确实是否支付成功
                var payLogs = _payLogRepo.GetAll(x => x.OrderId == dto.OrderId&&x.PayStatus==1).FirstOrDefault();
                if (null != payLogs)
                    result.PaySuccessTime = payLogs.SuccessTime;
                return result;
            }

            {
                //请求微信再确实是否支付成功
                var payLogs = _payLogRepo.GetAll(x => x.OrderId == dto.OrderId);
                if (null == payLogs||payLogs.Count()<=0) throw new CustomResponseException("财务系统不存在该订单支付记录");
                foreach (var payorder in payorders)
                {
                    var r = await WechatCheckOrderPayStatus(payorder.OrderNo);
                    if (r.trade_state == WeChatPayTradeState.Success)
                    {
                        result.OrderStatus = (int)OrderStatus.PaySucess;
                        result.WechatTradeState = r.trade_state;
                        result.PaySuccessTime = r.success_time;
                        return result;
                    }
                   
                }
               

            }
            result.OrderStatus = (int)OrderStatus.Wait;
            return result;

        }

        private async Task<WeChatPayOrderQueryResponse> WechatCheckOrderPayStatus(string orderid)
        {
            var request = new WeChatPayOrderQueryRequest
            {

                mchid = _wechatConfig.MchId,
                out_trade_no = orderid
            };
            return await _client.ExecuteAsync(request, _wechatConfig);

        }
    }
}
