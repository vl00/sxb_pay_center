using CSRedis;
using Dapper;
using Dapper.Contrib.Extensions;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Statement;
using iSchool.FinanceCenter.Appliaction.Service.MessageQueue;
using iSchool.FinanceCenter.Appliaction.Service.Statement;
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
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.PayOrder
{
    /// <summary>
    /// 微信支付回调记录添加
    /// </summary>
    public class WxPayCallBackCommandHandler : IRequestHandler<WxPayCallBackCommand, bool>
    {
        private readonly IRepository<iSchool.FinanceCenter.Domain.Entities.PayOrder> _orderRepository;
        private readonly IRepository<WxPayCallBackLog> _wechatPayCallBackRepository;
        private readonly IRepository<Domain.Entities.Statement> _stateMentrepository;
        private readonly FinanceCenterUnitOfWork financeUnitOfWork;
        CSRedisClient _redis;
        IHttpClientFactory httpClientFactory;
        IWechatCallBackMQService _wechatPayCallBackService;
        private readonly IMediator _mediator;

        public WxPayCallBackCommandHandler(IRepository<Domain.Entities.Statement> stateMentrepository, IRepository<iSchool.FinanceCenter.Domain.Entities.PayOrder> orderRepository, IMediator mediator, IWechatCallBackMQService wechatPayCallBackService, IRepository<WxPayCallBackLog> wechatPayCallBackRepository, IHttpClientFactory httpClientFactory, IFinanceCenterUnitOfWork financeUnitOfWork, CSRedisClient redis)
        {
            this._wechatPayCallBackRepository = wechatPayCallBackRepository;
            this.financeUnitOfWork = (FinanceCenterUnitOfWork)financeUnitOfWork;
            this._redis = redis;
            this.httpClientFactory = httpClientFactory;
            this._wechatPayCallBackService = wechatPayCallBackService;
            this._mediator = mediator;
            this._orderRepository = orderRepository;
            this._stateMentrepository = stateMentrepository;
        }


        public async Task<bool> Handle(WxPayCallBackCommand request, CancellationToken cancellationToken)
        {

            var notify = request.notify;
            var payStatus = notify.TradeState == WeChatPayTradeState.Success ? (int)PayStatusEnum.Success : (int)PayStatusEnum.Fail;
            //防止重复处理
            var key = CacheKeys.WechatPayCallBackIdentity.FormatWith(notify.OutTradeNo);
            var ishandle = await _redis.ExistsAsync(key);
            if (!ishandle)
            {   //数据库防重复--考虑加唯一键索引
                var dataBaseRepeat = _wechatPayCallBackRepository.IsExist(x => x.OutTradeNo == notify.OutTradeNo);
                if (!dataBaseRepeat)
                {
                    var order_no = notify.OutTradeNo;
                    var order = _orderRepository.Get(x => x.OrderNo == order_no);
                    try
                    {
                        financeUnitOfWork.BeginTransaction();
                        var addResult = await AddCallBackLog(notify);

                        if (addResult) await _redis.SetAsync(key, 1, TimeSpan.FromDays(1));
                    
                     
                        if (null == order) throw new CustomResponseException($"支付回调处理异常，找不到该订单{order_no}");

                        //更新paylog&&payorder
                        var updateResult = await UpdatePayLogPayOrder(order_no, payStatus, notify.TradeState, notify.TradeStateDesc, request.ReturnJson, notify.SuccessTime,order.System, order.OrderId);
                        //公司流水添加
                        var tempR = await AddCompanyCashLog(order);
                        financeUnitOfWork.CommitChanges();
                    }
                    catch (Exception ex)
                    {
                        financeUnitOfWork.Rollback();
                        throw new CustomResponseException($"支付回调事务执行失败{ex.Message}");

                    }
                    var callBackUrl = _redis.Get<string>(order_no);
                    if (!string.IsNullOrEmpty(callBackUrl))
                    {
                        _redis.Del(order_no);
                        using var httpClient = httpClientFactory.CreateClient("ask");
                        await httpClient.SxbPost2Async(callBackUrl, JsonConvert.SerializeObject(new { orderid = order.OrderId, paystatus = payStatus }));

                    }
                    else {
                        if (notify.Attach.Contains("from=ask"))//上学问系统的回调
                        {
                            using var httpClient = httpClientFactory.CreateClient("ask");
                            await httpClient.SxbPostAsync($"{ConfigHelper.GetConfigString("AskSysytemUrlDomain")}/paidqa/order/paycallback", JsonConvert.SerializeObject(new { orderid = order.OrderId, paystatus = payStatus }));
                            //加入rabbitmq 
                            _wechatPayCallBackService.Notify(new Messeage.QueueEntity.PayCallBackNotifyMessage()
                            {
                                OrderId = order.OrderId,
                                AddTime = notify.SuccessTime,
                                PayStatus = payStatus
                            }, "asksystem");

                        }
                        if (notify.Attach.Contains("from=releaseask"))//上学问系统的回调
                        {


                            using var httpClient = httpClientFactory.CreateClient("ask");
                            await httpClient.SxbPostAsync($"{ConfigHelper.GetConfigString("ReleaseAskSysytemUrlDomain")}/paidqa/order/paycallback", JsonConvert.SerializeObject(new { orderid = order.OrderId, paystatus = payStatus }));
                            //加入rabbitmq 
                            _wechatPayCallBackService.Notify(new Messeage.QueueEntity.PayCallBackNotifyMessage()
                            {
                                OrderId = order.OrderId,
                                AddTime = notify.SuccessTime,
                                PayStatus = payStatus
                            }, "asksystem");


                        }
                        else if (notify.Attach.Contains("from=org"))//机构系统回调
                        {
                            //加入rabbitmq 
                            _wechatPayCallBackService.Notify(new Messeage.QueueEntity.PayCallBackNotifyMessage()
                            {
                                OrderNo = order.SourceOrderNo,
                                OrderId = order.OrderId,
                                AddTime = notify.SuccessTime,
                                PayStatus = payStatus
                            }, "orgsystem");
                        }
                    }

                    return true;

                }
                return false;

            }
            return false;


        }

        private async Task<bool> AddCompanyCashLog(iSchool.FinanceCenter.Domain.Entities.PayOrder order)
        {
 


            if (null != order)
            {

                #region 公司流水添加
                var addStatmentRequest = new AddStatementDto()
                {
                    UserId = Guid.Parse(ConfigHelper.GetConfigString("CompanyUserId")),
                    Amount = order.TotalAmount,
                    Io = StatementIoEnum.In,
                    StatementType = StatementTypeEnum.Recharge,
                    OrderId = order.OrderId,
                    OrderType = order.OrderType,
                Remark = "用户支付成功"

                };
                var addSql = AddStatementSql.AddStatement(addStatmentRequest);
                await financeUnitOfWork.DbConnection.ExecuteAsync(addSql.Sql, addSql.SqlParam, financeUnitOfWork.DbTransaction);
                var subStractStatmentRequest = new AddStatementDto()
                {
                    UserId = Guid.Parse(ConfigHelper.GetConfigString("CompanyUserId")),
                    Amount = order.TotalAmount,
                    Io = StatementIoEnum.Out,
                    StatementType = StatementTypeEnum.Outgoings,
                    OrderId = order.OrderId,
                    OrderType = order.OrderType,
                    Remark = "用户支付成功"

                };
                var subStractSql = AddStatementSql.AddStatement(subStractStatmentRequest);
                await financeUnitOfWork.DbConnection.ExecuteAsync(subStractSql.Sql, subStractSql.SqlParam, financeUnitOfWork.DbTransaction);
                #endregion
            }
            return true;
        }

        private async Task<bool> UpdatePayLogPayOrder(string order_no, int pay_status, string result_code, string result_err_msg, string return_json, DateTime sucess_time,int system,Guid orderid)
        {
            var order_status = pay_status == (int)PayStatusEnum.Success ? OrderStatus.PaySucess : OrderStatus.PayFaile;
            var sql = @"Update dbo.PayLog set PayStatus=@PayStatus,ResultCode=@ResultCode,ErrCodeStr=@ErrCodeStr,ReturnJson=@ReturnJson,UpdateTime=@UpdateTime,SuccessTime=@SuccessTime
where TradeNo=@TradeNo;
Update dbo.PayOrder  set OrderStatus=@OrderStatus,UpdateTime=@UpdateTime where OrderNo=@TradeNo;
";
            if (system == (int)OrderSystem.Org)
            {
                sql += "Update dbo.ProductOrderRelation set Status=@OrderStatus,UpdateTime=@UpdateTime where AdvanceOrderId=@OrderId";
            }
            var param = new
            {
                TradeNo = order_no,
                UpdateTime = DateTime.Now,
                PayStatus = pay_status,
                ResultCode = result_code,
                ErrCodeStr = result_err_msg,
                ReturnJson = return_json,
                SuccessTime = sucess_time,
                OrderStatus = order_status,
                OrderId=orderid
            };
            return await financeUnitOfWork.DbConnection.ExecuteAsync(sql, param, financeUnitOfWork.DbTransaction) > 0;
        }

        private async Task<bool> AddCallBackLog(WeChatPayTransactionsNotify notify)
        {
          ;
            var addM = new WxPayCallBackLog()
            {
                ID = Guid.NewGuid(),
                OutTradeNo = notify.OutTradeNo,
                TransactionId = notify.TransactionId,
                TradeType = notify.TradeType,
                TradeState = notify.TradeState,
                TradeStateDesc = notify.TradeStateDesc,
                BankType = notify.BankType,
                Attach = notify.Attach,
                OpenId = notify.CombinePayerInfo?.OpenId,
                SuccessTime = notify.SuccessTime,
                Amount = notify.Amount.Total,
                CreateTime = DateTime.Now
            };
            return await financeUnitOfWork.DbConnection.InsertAsync(addM, financeUnitOfWork.DbTransaction) > 0;

        }
    }
}
