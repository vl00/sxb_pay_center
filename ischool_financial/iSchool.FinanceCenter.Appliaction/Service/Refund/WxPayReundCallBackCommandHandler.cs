using CSRedis;
using Dapper.Contrib.Extensions;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.Service.MessageQueue;
using iSchool.FinanceCenter.Domain;
using iSchool.FinanceCenter.Domain.Entities;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.FinanceCenter.Domain.Redis;
using iSchool.Infrastructure;
using iSchool.Infrastructure.UoW;
using MediatR;
using Newtonsoft.Json;
using ProductManagement.API.Http.HttpExtend;
using Sxb.PayCenter.WechatPay;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Refund
{
    /// <summary>
    /// 微信退款回调记录添加
    /// </summary>
    public class WxPayReundCallBackCommandHandler : IRequestHandler<WxPayReundCallBackCommand, bool>
    {

        private readonly IRepository<WxRefundCallBackLog> _wechatRefundCallBackRepository;
        private readonly FinanceCenterUnitOfWork financeUnitOfWork;
        CSRedisClient _redis;
        IHttpClientFactory httpClientFactory;
        IWechatCallBackMQService _wechatPayCallBackService;

        public WxPayReundCallBackCommandHandler(IWechatCallBackMQService wechatPayCallBackService, IRepository<WxRefundCallBackLog> wechatRefundCallBackRepository, IHttpClientFactory httpClientFactory, IFinanceCenterUnitOfWork financeUnitOfWork, CSRedisClient redis)
        {
            this._wechatRefundCallBackRepository = wechatRefundCallBackRepository;
            this.financeUnitOfWork = (FinanceCenterUnitOfWork)financeUnitOfWork;
            this._redis = redis;
            this.httpClientFactory = httpClientFactory;
            this._wechatPayCallBackService = wechatPayCallBackService;
        }


        public async Task<bool> Handle(WxPayReundCallBackCommand request, CancellationToken cancellationToken)
        {

            var notify = request.notify;
            var refundStatus = notify.RefundStatus == WeChatPayTradeState.Success ? (byte)RefundStatusEnum.Sucess : (int)RefundStatusEnum.Fail;
            //防止重复处理
            var key = CacheKeys.WechatPayRefundCallBackIdentity.FormatWith(notify.OutTradeNo);
            var ishandle = await _redis.ExistsAsync(key);
            if (!ishandle)
            {   //数据库防重复--考虑加唯一键索引
                var dataBaseRepeat = _wechatRefundCallBackRepository.IsExist(x => x.OutTradeNo == notify.OutTradeNo);
                if (!dataBaseRepeat)
                {
                    var addResult = await AddCallBackLog(notify);
                    if (addResult) await _redis.SetAsync(key, 1, TimeSpan.FromDays(1));
                    //退款回调通知

                    //代写
                    return true;

                }
                return false;

            }
            return false;
        }

        private async Task<bool> AddCallBackLog(WeChatPayRefundNotify notify)
        {
            var addM = new WxRefundCallBackLog()
            {
                Id = Guid.NewGuid(),
                OutTradeNo = notify.OutTradeNo,
                TransactionId = notify.TransactionId,
                ReturnCode = notify.ReturnCode,
                SuccessTime = notify.SuccessTime,
                ReturnMsg = notify.ReturnMsg,
                AppId = notify.AppId,
                MchId = notify.MchId,
                RefundId = notify.RefundId,
                OutRefundNo = notify.OutRefundNo,
                TotalFee = notify.TotalFee,
                RefundFee = notify.RefundFee,
                SettlementRefundFee = notify.SettlementRefundFee,
                RefundStatus = notify.RefundStatus,
                CreateTime = DateTime.Now
            };
            await financeUnitOfWork.DbConnection.InsertAsync(addM, financeUnitOfWork.DbTransaction);
            return true;
        }
    }
}
