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
    public class PayRefundHandler : IRequestHandler<PayRefundCommand, RefundResult>
    {
        private readonly IWeChatPayClient _client;
        private readonly IRepository<Domain.Entities.PayOrder> _payOrderRepo;
        private readonly IRepository<ProductOrderRelation> _payOrderDetailRepo;
        private readonly IRepository<Domain.Entities.WxPayCallBackLog> _payCallBackRepo;
        private readonly IRepository<Domain.Entities.RefundOrder> _refundOrderRepo;
        private readonly IRepository<Domain.Entities.RefundLog> _refundLogRepo;
        private readonly IRepository<Domain.Entities.Statement> _stateMentRepo;
        private readonly WeChatPayOptions _wechatConfig;
        private readonly FinanceCenterUnitOfWork financeUnitOfWork;
        CSRedisClient _redis;
        private readonly ISxbGenerateNo _sxbGenerateNo;
        public PayRefundHandler(IFinanceCenterUnitOfWork financeUnitOfWork, IRepository<ProductOrderRelation> payOrderDetailRepo, IRepository<Domain.Entities.Statement> stateMentRepo, CSRedisClient redis, IRepository<Domain.Entities.RefundLog> refundLogRepo, IOptions<WeChatPayOptions> wechatPayConfig, IRepository<Domain.Entities.WxPayCallBackLog> payCallBackRepo, IRepository<Domain.Entities.RefundOrder> refundOrderRepo, IRepository<Domain.Entities.PayOrder> payOrderRepo, IWeChatPayClient client
            , ISxbGenerateNo sxbGenerateNo)
        {
            _redis = redis;
            _payCallBackRepo = payCallBackRepo;
            _refundOrderRepo = refundOrderRepo;
            _payOrderRepo = payOrderRepo;
            _client = client;
            _wechatConfig = wechatPayConfig.Value;
            _refundLogRepo = refundLogRepo;
            _stateMentRepo = stateMentRepo;
            _sxbGenerateNo = sxbGenerateNo;
            _payOrderDetailRepo = payOrderDetailRepo;
            this.financeUnitOfWork = (FinanceCenterUnitOfWork)financeUnitOfWork;
        }

        public async Task<RefundResult> Handle(PayRefundCommand cmd, CancellationToken cancellationToken)
        {
            if (cmd.System == (int)OrderSystem.Ask) cmd.RefundType = RefundTypeEnum.All;

            switch (cmd.RefundType)
            {
                case RefundTypeEnum.All:
                    return await AllRefund(cmd);
                case RefundTypeEnum.ChildOrder:
                    return await ChildRefund(cmd);
                case RefundTypeEnum.ProductOrder:
                    return await ProductOrderRefund(cmd);

            }
            return new RefundResult() { };

        }
        public async Task<RefundResult> AllRefund(PayRefundCommand cmd)
        {

            string Refund0rderId = string.Empty;

            if (cmd.System == (int)OrderSystem.Ask)
                Refund0rderId = cmd.OrderId.ToString("N");
            else if (cmd.System == (int)OrderSystem.Org)
            {
                Refund0rderId = cmd.AdvanceOrderId.ToString("N");
            }
            else
            {

                throw new CustomResponseException("参数有误");
            }

            //限制同一张订单进行多次退款
            var key = CacheKeys.WechatPayRefundOrder.FormatWith(Refund0rderId);
            var ishandle = await _redis.ExistsAsync(key);
            if (ishandle) throw new CustomResponseException("请勿重复发起退款申请");
            try
            {

                var wechat_amout = Convert.ToInt32(cmd.RefundAmount * 100);
                //防止高频调用
                await _redis.SetAsync(key, 1, TimeSpan.FromMinutes(1));
                //查找该订单的支付订单
                var expectUserId = Guid.Parse(ConfigHelper.GetConfigString("CompanyUserId"));
                Domain.Entities.PayOrder payorder = null;
                if (cmd.System == (int)OrderSystem.Ask)
                    payorder = _payOrderRepo.Get(x => x.UserId != expectUserId && x.OrderId == cmd.OrderId && (x.OrderStatus == (int)OrderStatus.PaySucess || x.OrderStatus == (int)OrderStatus.Refund || x.OrderStatus == (int)OrderStatus.PartRefund) );
                else if (cmd.System == (int)OrderSystem.Org)
                {
                    payorder = _payOrderRepo.Get(x => x.UserId != expectUserId && x.OrderId == cmd.AdvanceOrderId && (x.OrderStatus == (int)OrderStatus.PaySucess || x.OrderStatus == (int)OrderStatus.Refund || x.OrderStatus == (int)OrderStatus.PartRefund) && x.System == cmd.System);
                }
                if (null == payorder) throw new CustomResponseException("找不到该订单的支付订单，退款失败");

                var paydetail = _payCallBackRepo.Get(x => x.OutTradeNo == payorder.OrderNo);
                if (null == paydetail) throw new CustomResponseException("找不到该订单的支付回调记录，退款失败");

                //金额，状态逻辑验证
                if (cmd.RefundAmount / 100 > payorder.TotalAmount) throw new CustomResponseException("退款金额超过付款金额，退款失败");
                bool IsAllRefund = true;//是否全部退款 
                if (cmd.RefundAmount != payorder.TotalAmount) IsAllRefund = false;
                //获取退款总记录进行验证
                var list_refund = _refundOrderRepo.GetAll(x => x.No == payorder.OrderNo && x.Status == 1);
                if (null != list_refund && list_refund.Count() > 0)
                {
                    var test = list_refund.Sum(x => x.Amount) + cmd.RefundAmount;
                    if (test > payorder.TotalAmount)
                        throw new CustomResponseException("订单申请退款金额超过支付金额，退款失败");
                    if (test == payorder.TotalAmount) IsAllRefund = true;
                }

                var result = new RefundResult() { ApplySucess = false };
                //生成一个退款订单
                var refundOrderNo = $"FNC{_sxbGenerateNo.GetNumber()}";
                var refuundOrderId = Guid.NewGuid();
                var addResult = await CreateRefundOrder(cmd, refuundOrderId, payorder.System, refundOrderNo);
                if (addResult)
                {
                    var request = new WeChatPayRefundRequest
                    {
                        OutRefundNo = refundOrderNo,
                        TransactionId = paydetail.TransactionId,
                        OutTradeNo = paydetail.OutTradeNo,
                        TotalFee = Convert.ToInt32(payorder.PayAmount * 100),
                        RefundFee = wechat_amout,
                        RefundDesc = cmd.Remark,
                        NotifyUrl = ConfigHelper.GetConfigs("WeChatPay", "WeChatRefundNotifyUrl")
                    };
                    var response = await _client.ExecuteAsync(request, _wechatConfig);
                    var requestJson = JsonConvert.SerializeObject(request);
                    await AddRefundLog(refuundOrderId, requestJson);
                    if (response.ReturnCode == WeChatPayTradeState.Success)//业务申请成功
                    {
                        if (response.ResultCode == WeChatPayTradeState.Success)//退款成功
                        {

                            var r = await UpdateRefundOrder(RefundStatusEnum.ApplySuccess, refuundOrderId, response.RefundId, response.ResultCode, payorder.Id, cmd.RefundAmount, IsAllRefund);
                            if (r)
                            {
                                //记录流水
                                var logSqls = await LogStatement(payorder, cmd.RefundAmount);
                                //事务执行，保证数据一致性
                                var res = await _stateMentRepo.Executes(logSqls.Sqls, logSqls.SqlParams);
                                if (!res)
                                {
                                    //记录日志
                                }

                                result.ApplySucess = true;
                                result.AapplyDesc = "退款申请已经提交成功啦";
                                return result;
                            }
                            result.ApplySucess = true;
                            result.AapplyDesc = "退款申请已经成功,支付中心修改退款订单失败";
                            return result;
                        }
                        else
                        {
                            var r = await UpdateRefundOrder(RefundStatusEnum.Fail, refuundOrderId, response.RefundId, response.ErrCodeDes, payorder.Id, cmd.RefundAmount, IsAllRefund);
                            if (r)
                            {

                                result.AapplyDesc = response.ErrCodeDes; ;
                                return result;
                            }
                            result.AapplyDesc = "支付中心修改退款订单失败";
                            return result;

                        }

                    }
                    else
                    {

                        var r = await UpdateRefundOrder(RefundStatusEnum.Fail, refuundOrderId, response.RefundId, $"请求微信退款业务接口返回失败:{response.ReturnMsg}", payorder.Id, cmd.RefundAmount, IsAllRefund);

                        if (r)
                        {
                            result.ApplySucess = false;
                            result.AapplyDesc = " 请求微信退款业务接口返回失败";
                            return result;
                        }
                        result.AapplyDesc = "请求微信退款业务接口返回失败&&支付中心修改退款订单状态失败";
                        return result;

                    }
                }
                result.AapplyDesc = "支付中心创建退款订单失败";
                return result;
            }
            catch (Exception ex)
            {
                await _redis.DelAsync(key);
                throw;
            }
        }
        public async Task<RefundResult> ProductOrderRefund(PayRefundCommand cmd)
        {
            string Refund0rderId = string.Empty;
            if (null == cmd.OrderDetailId || Guid.Empty == cmd.OrderDetailId) throw new CustomResponseException("产品订单退款参数请传子单Id");
            Refund0rderId = cmd.OrderDetailId.ToString("N");
            if (null == cmd.AdvanceOrderId || Guid.Empty == cmd.AdvanceOrderId) throw new CustomResponseException("AdvanceOrderId 缺失");
            //限制同一张订单进行多次退款
            var key = CacheKeys.WechatPayRefundOrder.FormatWith(Refund0rderId);
            var ishandle = await _redis.ExistsAsync(key);
            if (ishandle) throw new CustomResponseException("请勿重复发起退款申请");
            try
            {
                var wechat_amout = Convert.ToInt32(cmd.RefundAmount * 100);
                //防止高频调用
                await _redis.SetAsync(key, 1, TimeSpan.FromMinutes(1));
                //查找该订单的支付订单
                var expectUserId = Guid.Parse(ConfigHelper.GetConfigString("CompanyUserId"));
                var payorder = _payOrderRepo.Get(x => x.UserId != expectUserId && x.OrderId == cmd.AdvanceOrderId && (x.OrderStatus == (int)OrderStatus.PaySucess || x.OrderStatus == (int)OrderStatus.Refund || x.OrderStatus == (int)OrderStatus.PartRefund) && x.System == cmd.System);
                if (null == payorder) throw new CustomResponseException("找不到该订单的支付订单，退款失败");

                var paydetail = _payCallBackRepo.Get(x => x.OutTradeNo == payorder.OrderNo);
                if (null == paydetail) throw new CustomResponseException("找不到该订单的支付回调记录，退款失败");

                var orderdetail = _payOrderDetailRepo.Get(x => x.OrderId == cmd.OrderDetailId);
                if (null == orderdetail) throw new CustomResponseException("找不到该产品订单记录，退款失败");
                if (orderdetail.Status != (int)PayStatusEnum.Success) throw new CustomResponseException("当前产品订单状态不支持退款，退款失败");
                if (cmd.RefundAmount / 100 > orderdetail.Amount) throw new CustomResponseException("申请金额与当前产品订单金额不匹配，退款失败");
                //金额，状态逻辑验证
                if (cmd.RefundAmount / 100 > payorder.TotalAmount) throw new CustomResponseException("退款金额超过付款金额，退款失败");
                var IsAllRefund = false;
                //获取退款总记录进行验证
                var list_refund = _refundOrderRepo.GetAll(x => x.No == payorder.OrderNo && x.Status == 1);
                if (null != list_refund && list_refund.Count() > 0)
                {
                    var test = list_refund.Sum(x => x.Amount) + cmd.RefundAmount;
                    if (test > payorder.TotalAmount)
                        throw new CustomResponseException("订单申请退款金额超过支付金额，退款失败");
                    if (test == payorder.TotalAmount) IsAllRefund = true;
                }

                var result = new RefundResult() { ApplySucess = false };
                //生成一个退款订单
                var refundOrderNo = $"FNC{_sxbGenerateNo.GetNumber()}";
                var refuundOrderId = Guid.NewGuid();
                var addResult = await CreateRefundOrder(cmd, refuundOrderId, payorder.System, refundOrderNo);
                if (addResult)
                {
                    var request = new WeChatPayRefundRequest
                    {
                        OutRefundNo = refundOrderNo,
                        TransactionId = paydetail.TransactionId,
                        OutTradeNo = paydetail.OutTradeNo,
                        TotalFee = Convert.ToInt32(payorder.PayAmount * 100),
                        RefundFee = wechat_amout,
                        RefundDesc = cmd.Remark,
                        NotifyUrl = ConfigHelper.GetConfigs("WeChatPay", "WeChatRefundNotifyUrl")
                    };
                    var response = await _client.ExecuteAsync(request, _wechatConfig);
                    var requestJson = JsonConvert.SerializeObject(request);
                    await AddRefundLog(refuundOrderId, requestJson);
                    if (response.ReturnCode == WeChatPayTradeState.Success)//业务申请成功
                    {
                        if (response.ResultCode == WeChatPayTradeState.Success)//退款成功
                        {

                            var r = await UpdateRefundOrder(RefundStatusEnum.ApplySuccess, refuundOrderId, response.RefundId, response.ResultCode, payorder.Id, cmd.RefundAmount, IsAllRefund);
                            var dr = await UpdateRefundOrderDetail(RefundStatusEnum.ApplySuccess,cmd.OrderDetailId);
                            if (r)
                            {
                                //记录流水
                                var logSqls = await LogStatement(payorder, cmd.RefundAmount);
                                //事务执行，保证数据一致性
                                var res = await _stateMentRepo.Executes(logSqls.Sqls, logSqls.SqlParams);
                                if (!res)
                                {
                                    //记录日志
                                }

                                result.ApplySucess = true;
                                result.AapplyDesc = "退款申请已经提交成功啦";
                                return result;
                            }
                            result.ApplySucess = true;
                            result.AapplyDesc = "退款申请已经成功,支付中心修改退款订单失败";
                            return result;
                        }
                        else
                        {
                            var r = await UpdateRefundOrder(RefundStatusEnum.Fail, refuundOrderId, response.RefundId, response.ErrCodeDes, payorder.Id, cmd.RefundAmount, IsAllRefund);
                            if (r)
                            {

                                result.AapplyDesc = response.ErrCodeDes; ;
                                return result;
                            }
                            result.AapplyDesc = "支付中心修改退款订单失败";
                            return result;

                        }

                    }
                    else
                    {

                        var r = await UpdateRefundOrder(RefundStatusEnum.Fail, refuundOrderId, response.RefundId, $"请求微信退款业务接口返回失败:{response.ReturnMsg}", payorder.Id, cmd.RefundAmount, IsAllRefund);

                        if (r)
                        {
                            result.ApplySucess = false;
                            result.AapplyDesc = " 请求微信退款业务接口返回失败";
                            return result;
                        }
                        result.AapplyDesc = "请求微信退款业务接口返回失败&&支付中心修改退款订单状态失败";
                        return result;

                    }
                }
                result.AapplyDesc = "支付中心创建退款订单失败";
                return result;
            }
            catch (Exception ex)
            {
                await _redis.DelAsync(key);
                throw;
            }
        }
        public async Task<RefundResult> ChildRefund(PayRefundCommand cmd)
        {
            string Refund0rderId = string.Empty;
            if (null == cmd.OrderId || Guid.Empty == cmd.OrderId) throw new CustomResponseException("子单退款参数请传子单Id");
            Refund0rderId = cmd.OrderId.ToString("N");
            if (null == cmd.AdvanceOrderId || Guid.Empty == cmd.AdvanceOrderId) throw new CustomResponseException("AdvanceOrderId 缺失");

            //限制同一张订单进行多次退款
            var key = CacheKeys.WechatPayRefundOrder.FormatWith(Refund0rderId);
            var ishandle = await _redis.ExistsAsync(key);
            if (ishandle) throw new CustomResponseException("请勿重复发起退款申请");
            try
            {

                var wechat_amout = Convert.ToInt32(cmd.RefundAmount * 100);
                //防止高频调用
                await _redis.SetAsync(key, 1, TimeSpan.FromMinutes(1));
                //查找该订单的支付订单
                var expectUserId = Guid.Parse(ConfigHelper.GetConfigString("CompanyUserId"));


                var payorder = _payOrderRepo.Get(x => x.UserId != expectUserId && x.OrderId == cmd.AdvanceOrderId && (x.OrderStatus == (int)OrderStatus.PaySucess || x.OrderStatus == (int)OrderStatus.Refund || x.OrderStatus == (int)OrderStatus.PartRefund) && x.System == cmd.System);
                if (null == payorder) throw new CustomResponseException("找不到该订单的支付订单，退款失败");

                var paydetail = _payCallBackRepo.Get(x => x.OutTradeNo == payorder.OrderNo);
                if (null == paydetail) throw new CustomResponseException("找不到该订单的支付回调记录，退款失败");

                var subOrder = _payOrderRepo.Get(x => x.OrderId == cmd.OrderId );
                if (null == subOrder) throw new CustomResponseException("找不到该子订单，退款失败");
                if (subOrder.OrderStatus != (int)PayStatusEnum.Success) throw new CustomResponseException("当前子订单状态不支持退款，退款失败");
                if (cmd.RefundAmount > subOrder.PayAmount) throw new CustomResponseException("退款金额超过付款金额，退款失败");
                //金额，状态逻辑验证
                if (cmd.RefundAmount / 100 > payorder.TotalAmount) throw new CustomResponseException("退款金额超过付款金额，退款失败");
                bool IsAllRefund = true;//是否全部退款 
                if (cmd.RefundAmount != payorder.TotalAmount) IsAllRefund = false;
                //获取退款总记录进行验证
                var list_refund = _refundOrderRepo.GetAll(x => x.No == payorder.OrderNo && x.Status == 1);
                if (null != list_refund && list_refund.Count() > 0)
                {
                    var test = list_refund.Sum(x => x.Amount) + cmd.RefundAmount;
                    if (test > payorder.TotalAmount)
                        throw new CustomResponseException("订单申请退款金额超过支付金额，退款失败");
                    if (test == payorder.TotalAmount) IsAllRefund = true;
                }

                var result = new RefundResult() { ApplySucess = false };
                //生成一个退款订单
                var refundOrderNo = $"FNC{_sxbGenerateNo.GetNumber()}";
                var refuundOrderId = Guid.NewGuid();
                var addResult = await CreateRefundOrder(cmd, refuundOrderId, payorder.System, refundOrderNo);
                if (addResult)
                {
                    var request = new WeChatPayRefundRequest
                    {
                        OutRefundNo = refundOrderNo,
                        TransactionId = paydetail.TransactionId,
                        OutTradeNo = paydetail.OutTradeNo,
                        TotalFee = Convert.ToInt32(payorder.PayAmount * 100),
                        RefundFee = wechat_amout,
                        RefundDesc = cmd.Remark,
                        NotifyUrl = ConfigHelper.GetConfigs("WeChatPay", "WeChatRefundNotifyUrl")
                    };
                    var response = await _client.ExecuteAsync(request, _wechatConfig);
                    var requestJson = JsonConvert.SerializeObject(request);
                    await AddRefundLog(refuundOrderId, requestJson);
                    if (response.ReturnCode == WeChatPayTradeState.Success)//业务申请成功
                    {
                        if (response.ResultCode == WeChatPayTradeState.Success)//退款成功
                        {
                            //缺乏事务
                            var r = await UpdateRefundOrder(RefundStatusEnum.ApplySuccess, refuundOrderId, response.RefundId, response.ResultCode, payorder.Id, cmd.RefundAmount, IsAllRefund);
                       
                            if (r)
                            {
                                //记录流水
                                var logSqls = await LogStatement(payorder, cmd.RefundAmount);
                                //事务执行，保证数据一致性
                                var res = await _stateMentRepo.Executes(logSqls.Sqls, logSqls.SqlParams);
                                if (!res)
                                {
                                    //记录日志
                                }

                                result.ApplySucess = true;
                                result.AapplyDesc = "退款申请已经提交成功啦";
                                return result;
                            }
                            result.ApplySucess = true;
                            result.AapplyDesc = "退款申请已经成功,支付中心修改退款订单失败";
                            return result;
                        }
                        else
                        {
                            var r = await UpdateRefundOrder(RefundStatusEnum.Fail, refuundOrderId, response.RefundId, response.ErrCodeDes, payorder.Id, cmd.RefundAmount, IsAllRefund);
                            if (r)
                            {

                                result.AapplyDesc = response.ErrCodeDes; ;
                                return result;
                            }
                            result.AapplyDesc = "支付中心修改退款订单失败";
                            return result;

                        }

                    }
                    else
                    {

                        var r = await UpdateRefundOrder(RefundStatusEnum.Fail, refuundOrderId, response.RefundId, $"请求微信退款业务接口返回失败:{response.ReturnMsg}", payorder.Id, cmd.RefundAmount, IsAllRefund);

                        if (r)
                        {
                            result.ApplySucess = false;
                            result.AapplyDesc = " 请求微信退款业务接口返回失败";
                            return result;
                        }
                        result.AapplyDesc = "请求微信退款业务接口返回失败&&支付中心修改退款订单状态失败";
                        return result;

                    }
                }
                result.AapplyDesc = "支付中心创建退款订单失败";
                return result;
            }
            catch (Exception ex)
            {
                await _redis.DelAsync(key);
                throw;
            }
        }
        private async Task<SqlBase> LogStatement(iSchool.FinanceCenter.Domain.Entities.PayOrder order, decimal refundAmount)
        {
            var listSql = new List<iSchool.Domain.Modles.SqlSingle>();
            #region 公司流水添加
            var addStatmentRequest = new AddStatementDto()
            {

                Amount = order.TotalAmount,
                Io = StatementIoEnum.In,
                StatementType = StatementTypeEnum.Recharge,
                OrderId = order.OrderId,
                OrderType = OrderTypeEnum.Ask,
                Remark = "转单退到公司账号"

            };

            listSql.Add(AddStatementSql.AddStatement(addStatmentRequest));

            //差价
            var delta = order.TotalAmount - refundAmount;
            if (0 != delta)
            {

                var subStractStatmentRequest = new AddStatementDto()
                {
                    Amount = refundAmount,
                    Io = StatementIoEnum.Out,
                    StatementType = StatementTypeEnum.Outgoings,
                    OrderId = order.OrderId,
                    OrderType = OrderTypeEnum.Ask,
                    Remark = "上学问退款支出（实际金额）"

                };
                listSql.Add(AddStatementSql.AddStatement(subStractStatmentRequest));

                var subStractStatmentRequestRemain = new AddStatementDto()
                {
                    Amount = delta,
                    Io = StatementIoEnum.Out,
                    StatementType = StatementTypeEnum.Outgoings,
                    OrderId = order.OrderId,
                    OrderType = OrderTypeEnum.Ask,
                    Remark = "上学问退款支出(剩余金额)"

                };
                listSql.Add(AddStatementSql.AddStatement(subStractStatmentRequestRemain));
                var addStatmentRequestRemain = new AddStatementDto()
                {
                    Amount = delta,
                    Io = StatementIoEnum.In,
                    StatementType = StatementTypeEnum.Incomings,
                    OrderId = order.OrderId,
                    OrderType = OrderTypeEnum.Ask,
                    Remark = "上学问退款(剩余金额)重新入账"

                };
                listSql.Add(AddStatementSql.AddStatement(addStatmentRequestRemain));
            }
            else
            { //全额退
                var subStractStatmentRequest = new AddStatementDto()
                {
                    Amount = refundAmount,
                    Io = StatementIoEnum.Out,
                    StatementType = StatementTypeEnum.Outgoings,
                    OrderId = order.OrderId,
                    OrderType = OrderTypeEnum.Ask,
                    Remark = "上学问全额退款支出"

                };
                listSql.Add(AddStatementSql.AddStatement(subStractStatmentRequest));

            }
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            foreach (var item in listSql)
            {
                sqlBase.Sqls.Add(item.Sql);
                sqlBase.SqlParams.Add(item.Sql);
            }
            return sqlBase;

            #endregion

        }

        private Task<bool> AddRefundLog(Guid orderid, string requestJson)
        {
            var sql = @"INSERT INTO dbo.RefundLog
                        (ID,RufundOrderId,CreateTime,PostJson)
                        VALUES
                           (@ID,@RufundOrderId,@CreateTime,@PostJson)";

            var param = new
            {

                Id = Guid.NewGuid(),
                RufundOrderId = orderid,
                CreateTime = DateTime.Now,
                PostJson = requestJson,
            };
            return _refundLogRepo.ExecuteAsync(sql, param);
        }

        private Task<bool> UpdateRefundOrderDetail(RefundStatusEnum status, Guid OrderDetailId)

        {
            var sqlBuld = new StringBuilder();
            if (status == RefundStatusEnum.ApplySuccess)
            {
                sqlBuld.Append($@"UPDATE [dbo].[ProductOrderRelation] SET  [Status] ={(int)OrderStatus.Refund} ,[UpdateTime] =@UpdateTime  WHERE [OrderId] = @OrderDetailId ;");
                var param = new
                {
                    OrderDetailId = OrderDetailId,
                    UpdateTime = DateTime.Now,


                };
                return _refundOrderRepo.ExecuteAsync(sqlBuld.ToString(), param);
            }
            return Task.FromResult(false);
        }

     

        private Task<bool> UpdateRefundOrder(RefundStatusEnum status, Guid orderid, string pay_platformid, string result, Guid Refund0rderId, decimal refund_amount, bool IsAllRefund)
        {

            var sqlBuld = new StringBuilder();
            if (status == RefundStatusEnum.ApplySuccess)
            {

                sqlBuld.Append($@"UPDATE [dbo].[PayOrder] SET  [OrderStatus] ={(IsAllRefund ? (int)OrderStatus.Refund : (int)OrderStatus.PartRefund)}, [UpdateTime] =@UpdateTime,[RefundAmount]+=@RefundAmount  WHERE [ID] = @Refund0rderId;");


            }
            sqlBuld.Append("Update dbo.RefundOrder Set Status=@Status,UpdateTime=@UpdateTime,PayPlatfomRefundId=@PayPlatfomRefundId,AapplyResultStr=@AapplyResultStr where Id=@ID; ");
            var param = new
            {
                ID = orderid,
                UpdateTime = DateTime.Now,
                Status = (int)status,
                PayPlatfomRefundId = pay_platformid,
                AapplyResultStr = result,

                Refund0rderId = Refund0rderId,

                RefundAmount = refund_amount

            };
            return _refundOrderRepo.ExecuteAsync(sqlBuld.ToString(), param);
        }

        private Task<bool> CreateRefundOrder(PayRefundCommand cmd, Guid refundOrderId, int system, string no)
        {

            var orderid = cmd.OrderId;
            if (system == (int)OrderSystem.Org)
            {
                switch (cmd.RefundType)
                {
                    case RefundTypeEnum.All:
                        orderid = cmd.AdvanceOrderId;
                        break;
                    case RefundTypeEnum.ChildOrder:
                        orderid = cmd.OrderId;
                        break;
                    case RefundTypeEnum.ProductOrder:
                        orderid = cmd.OrderDetailId;
                        break;

                }

            }

            var sql = @"INSERT INTO dbo.RefundOrder
                        (ID,No,OrderId,CreateTime,Status,Remark,System,Amount,Type)
                        VALUES
                         (@ID,@No,@OrderId,@CreateTime,@Status,@Remark,@System,@Amount,@Type)";
            var param = new
            {
                ID = refundOrderId,
                No = no,
                OrderId = orderid,
                CreateTime = DateTime.Now,
                Status = (int)RefundStatusEnum.InProcess,
                Remark = cmd.Remark,
                System = system,
                Amount = cmd.RefundAmount,
                type = (int)cmd.RefundType
            };
            return _refundOrderRepo.ExecuteAsync(sql, param);
        }
    }
}
