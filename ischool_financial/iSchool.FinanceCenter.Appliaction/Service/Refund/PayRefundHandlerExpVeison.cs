using CSRedis;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Statement;
using iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder;
using iSchool.FinanceCenter.Appliaction.Service.Statement;
using iSchool.FinanceCenter.Domain;
using iSchool.FinanceCenter.Domain.Entities;
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
    public class PayRefundHandlerExpVeison : IRequestHandler<PayRefundCommandExpVeison, RefundResult>
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
        public PayRefundHandlerExpVeison(IFinanceCenterUnitOfWork financeUnitOfWork, IRepository<ProductOrderRelation> payOrderDetailRepo, IRepository<Domain.Entities.Statement> stateMentRepo, CSRedisClient redis, IRepository<Domain.Entities.RefundLog> refundLogRepo, IOptions<WeChatPayOptions> wechatPayConfig, IRepository<Domain.Entities.WxPayCallBackLog> payCallBackRepo, IRepository<Domain.Entities.RefundOrder> refundOrderRepo, IRepository<Domain.Entities.PayOrder> payOrderRepo, IWeChatPayClient client
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

        public async Task<RefundResult> Handle(PayRefundCommandExpVeison cmd, CancellationToken cancellationToken)
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
                case RefundTypeEnum.Freight:
                    return await FreightRefund(cmd);
            }
            return new RefundResult() { };

        }
        public async Task<RefundResult> AllRefund(PayRefundCommandExpVeison cmd)
        {
            if (null == cmd.AdvanceOrderId || Guid.Empty == cmd.AdvanceOrderId) throw new CustomResponseException("AdvanceOrderId 缺失");
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
                    payorder = _payOrderRepo.Get(x => x.UserId != expectUserId && x.OrderId == cmd.OrderId && (x.OrderStatus == (int)OrderStatus.PaySucess || x.OrderStatus == (int)OrderStatus.Refund || x.OrderStatus == (int)OrderStatus.PartRefund) && x.System == cmd.System);
                else if (cmd.System == (int)OrderSystem.Org)
                {
                    payorder = _payOrderRepo.Get(x => x.UserId != expectUserId && x.OrderId == cmd.AdvanceOrderId && (x.OrderStatus == (int)OrderStatus.PaySucess || x.OrderStatus == (int)OrderStatus.Refund || x.OrderStatus == (int)OrderStatus.PartRefund) && x.System == cmd.System);
                }
                if (null == payorder) throw new CustomResponseException("找不到该订单的支付订单，退款失败");

                var paydetail = _payCallBackRepo.Get(x => x.OutTradeNo == payorder.OrderNo);
                if (null == paydetail) throw new CustomResponseException("找不到该订单的支付回调记录，退款失败");

                //金额，状态逻辑验证
                if (cmd.RefundAmount / 100 > payorder.TotalAmount) throw new CustomResponseException("退款金额超过付款金额，退款失败");

                //获取退款总记录进行验证
                var list_refund = _refundOrderRepo.GetAll(x => x.PayOrderId == payorder.Id && x.Status == 1);
                if (null != list_refund && list_refund.Count() > 0)
                {
                    var test = list_refund.Sum(x => x.Amount) + cmd.RefundAmount;
                    if (test > payorder.TotalAmount)
                        throw new CustomResponseException("订单申请退款金额超过支付金额，退款失败");

                }

                var result = new RefundResult() { ApplySucess = false };
                //生成一个退款订单
                var refundOrderNo = $"FNC{_sxbGenerateNo.GetNumber()}";
                var refuundOrderId = Guid.NewGuid();
                var addResult = await CreateRefundOrder(cmd, refuundOrderId, payorder, refundOrderNo);
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

                            var r = await UpdateRefundOrderAll(RefundStatusEnum.ApplySuccess, refuundOrderId, response.RefundId, response.ResultCode, payorder, cmd.RefundAmount);
                            if (r)
                            {
                                await LogStatement(payorder, cmd.RefundAmount);
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
                            var r = await UpdateRefundOrderAll(RefundStatusEnum.Fail, refuundOrderId, response.RefundId, response.ErrCodeDes, payorder, cmd.RefundAmount);
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

                        var r = await UpdateRefundOrderAll(RefundStatusEnum.Fail, refuundOrderId, response.RefundId, $"请求微信退款业务接口返回失败:{response.ReturnMsg}", payorder, cmd.RefundAmount);

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
        public async Task<RefundResult> ChildRefund(PayRefundCommandExpVeison cmd)
        {

            string Refund0rderId = string.Empty;
            if (null == cmd.OrderDetailId || Guid.Empty == cmd.OrderDetailId) throw new CustomResponseException("退子单请传OrderDetailId");
            if (null == cmd.AdvanceOrderId || Guid.Empty == cmd.AdvanceOrderId) throw new CustomResponseException("AdvanceOrderId 缺失");
            Refund0rderId = cmd.OrderDetailId.ToString("N");
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

                payorder = _payOrderRepo.Get(x => x.UserId != expectUserId && x.OrderId == cmd.AdvanceOrderId && (x.OrderStatus == (int)OrderStatus.PaySucess || x.OrderStatus == (int)OrderStatus.Refund || x.OrderStatus == (int)OrderStatus.PartRefund) && x.System == cmd.System);

                if (null == payorder) throw new CustomResponseException("找不到该订单的支付订单，退款失败");

                var paydetail = _payCallBackRepo.Get(x => x.OutTradeNo == payorder.OrderNo);
                if (null == paydetail) throw new CustomResponseException("找不到该订单的支付回调记录，退款失败");

                //金额，状态逻辑验证
                if (cmd.RefundAmount / 100 > payorder.TotalAmount) throw new CustomResponseException("退款金额超过付款金额，退款失败");

                //获取退款总记录进行验证
                var list_refund = _refundOrderRepo.GetAll(x => x.PayOrderId == payorder.Id && x.Status == 1);
                if (null != list_refund && list_refund.Count() > 0)
                {
                    var test = list_refund.Sum(x => x.Amount) + cmd.RefundAmount;
                    if (test > payorder.TotalAmount)
                        throw new CustomResponseException("订单申请退款金额超过支付金额，退款失败");

                }
                var productList = _payOrderDetailRepo.GetAll(x => x.AdvanceOrderId == cmd.AdvanceOrderId && x.OrderId == cmd.OrderDetailId && x.Status != (int)OrderStatus.Refund);
                if (null == productList || productList.Count() <= 0) throw new CustomResponseException("找不到该子下单订单数据，退款失败");
                var remainRefundAmount = productList.Sum(x=>x.Amount*x.BuyNum);
                if(cmd.RefundAmount> remainRefundAmount)
                    throw new CustomResponseException("订单申请退款金额超过剩余未退款金额，退款失败");

                var result = new RefundResult() { ApplySucess = false };
                //生成一个退款订单
                var refundOrderNo = $"FNC{_sxbGenerateNo.GetNumber()}";
                var refuundOrderId = Guid.NewGuid();
                var addResult = await CreateRefundOrder(cmd, refuundOrderId, payorder, refundOrderNo);
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

                            var r = await UpdateRefundOrderChild(RefundStatusEnum.ApplySuccess, refuundOrderId, response.RefundId, response.ResultCode, payorder, cmd.RefundAmount,cmd.OrderDetailId);
                            if (r)
                            {
                                await LogStatement(payorder, cmd.RefundAmount);
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
                            var r = await UpdateRefundOrderChild(RefundStatusEnum.Fail, refuundOrderId, response.RefundId, response.ErrCodeDes, payorder, cmd.RefundAmount, cmd.OrderDetailId);
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

                        var r = await UpdateRefundOrderChild(RefundStatusEnum.Fail, refuundOrderId, response.RefundId, $"请求微信退款业务接口返回失败:{response.ReturnMsg}", payorder, cmd.RefundAmount, cmd.OrderDetailId);

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
        public async Task<RefundResult> ProductOrderRefund(PayRefundCommandExpVeison cmd)
        {
            string Refund0rderId = string.Empty;
            if (null == cmd.ProductId || Guid.Empty == cmd.ProductId) throw new CustomResponseException("退SKU商品请传ProductId");
            if (null == cmd.OrderDetailId || Guid.Empty == cmd.OrderDetailId) throw new CustomResponseException("退SKU商品请传OrderDetailId");
            if (cmd.RefundProductInfo.Count <= 0) throw new CustomResponseException("RefundProductInfo参数缺失");
            Refund0rderId = cmd.OrderDetailId.ToString("N");
            if (null == cmd.AdvanceOrderId || Guid.Empty == cmd.AdvanceOrderId) throw new CustomResponseException("AdvanceOrderId 缺失");
            //限制同一张订单进行多次退款
            var key = CacheKeys.WechatPayRefundOrder.FormatWith(Refund0rderId);
            var ishandle = await _redis.ExistsAsync(key);
            if (ishandle) throw new CustomResponseException("请勿重复发起退款申请");
            try
            {
              
                //防止高频调用
                await _redis.SetAsync(key, 1, TimeSpan.FromSeconds(10));
                //查找该订单的支付订单
                var expectUserId = Guid.Parse(ConfigHelper.GetConfigString("CompanyUserId"));
                var payorder = _payOrderRepo.Get(x => x.UserId != expectUserId && x.OrderId == cmd.AdvanceOrderId && (x.OrderStatus == (int)OrderStatus.PaySucess || x.OrderStatus == (int)OrderStatus.Refund || x.OrderStatus == (int)OrderStatus.PartRefund) && x.System == cmd.System);
                if (null == payorder) throw new CustomResponseException("找不到该订单的支付订单，退款失败");

                var paydetail = _payCallBackRepo.Get(x => x.OutTradeNo == payorder.OrderNo);
                if (null == paydetail) throw new CustomResponseException("找不到该订单的支付回调记录，退款失败");

                var productList = _payOrderDetailRepo.GetAll(x => x.AdvanceOrderId == cmd.AdvanceOrderId && x.ProductId == cmd.ProductId && x.OrderId == cmd.OrderId && x.Status != (int)OrderStatus.Refund);
               
                if (null == productList || productList.Count() <= 0) throw new CustomResponseException("找不到该SKU商品下单订单数据，退款失败");
                if (cmd.RefundProductInfo.Sum(x=>x.RefundProductNum) > productList.Count()) throw new CustomResponseException("要退得SKU商品数量有误，退款失败");
                //验证金额

                var doList = new List<ProductOrderRelation>();
             
                //找出能匹配的product,并验证金额
                foreach (var item in cmd.RefundProductInfo)
                {
                    if(item.Amount>item.RefundProductPrice) throw new CustomResponseException("要退商品的金额，不能大于商品原价");
                    var filterList=productList.Where(x => x.Price == item.RefundProductPrice);
                    if(filterList.Count()==0||filterList.Count()<item.RefundProductNum) throw new CustomResponseException("要退商品单价，数量不匹配");
                    var doItems = filterList.Take(item.RefundProductNum);
                    foreach (var doitem in doItems)
                    {
                        doitem.RefundAmount = item.Amount;
                    }
                    doList.AddRange(doItems);//根据传过来的价格和数量随机获取要退的
                }
                //上一步已经校验过了全为有效
                cmd.RefundAmount = doList.Sum(x=>x.RefundAmount);
               //金额，状态逻辑验证
                if (cmd.RefundAmount > payorder.TotalAmount) throw new CustomResponseException("退款金额超过付款金额，退款失败");
                if (cmd.RefundAmount<=0) throw new CustomResponseException("退款金额低于0元");
            
                var wechat_amout = Convert.ToInt32(cmd.RefundAmount * 100);
                //获取退款总记录进行验证
                var list_refund = _refundOrderRepo.GetAll(x => x.PayOrderId == payorder.Id && x.Status == 1);
                if (null != list_refund && list_refund.Count() > 0)
                {
                    var test = list_refund.Sum(x => x.Amount) + cmd.RefundAmount;
                    if (test > payorder.TotalAmount)
                        throw new CustomResponseException("订单申请退款金额超过支付金额，退款失败");

                }
              
                var result = new RefundResult() { ApplySucess = false };
                //生成一个退款订单
                var refundOrderNo = $"FNC{_sxbGenerateNo.GetNumber()}";
                var refuundOrderId = Guid.NewGuid();
                var addResult = await CreateRefundOrder(cmd, refuundOrderId, payorder, refundOrderNo);
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

                            var r = await UpdateRefundOrderSku(RefundStatusEnum.ApplySuccess, refuundOrderId, response.RefundId, response.ResultCode, payorder, cmd.RefundAmount, doList);

                            if (r)
                            {
                                await LogStatement(payorder, cmd.RefundAmount);
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
                            var r = await UpdateRefundOrderSku(RefundStatusEnum.Fail, refuundOrderId, response.RefundId, response.ErrCodeDes, payorder, cmd.RefundAmount, doList);
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

                        var r = await UpdateRefundOrderSku(RefundStatusEnum.Fail, refuundOrderId, response.RefundId, $"请求微信退款业务接口返回失败:{response.ReturnMsg}", payorder, cmd.RefundAmount, doList);

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
        public async Task<RefundResult> FreightRefund(PayRefundCommandExpVeison cmd)
        {
            if (null == cmd.AdvanceOrderId || Guid.Empty == cmd.AdvanceOrderId) throw new CustomResponseException("AdvanceOrderId 缺失");
            string Refund0rderId = string.Empty;
            if (cmd.System == (int)OrderSystem.Ask)
                throw new CustomResponseException("问答系统暂不支持退运费");
            else if (cmd.System == (int)OrderSystem.Org)
            {
                Refund0rderId = cmd.OrderId.ToString("N");
            }
            else
            {

                throw new CustomResponseException("参数有误");
            }

            //限制同一张订单进行多次退款
            var key = CacheKeys.WechatPayRefundOrderFreight.FormatWith(Refund0rderId);
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
                    payorder = _payOrderRepo.Get(x => x.UserId != expectUserId && x.OrderId == cmd.OrderId && (x.OrderStatus == (int)OrderStatus.PaySucess || x.OrderStatus == (int)OrderStatus.Refund || x.OrderStatus == (int)OrderStatus.PartRefund) && x.System == cmd.System);
                else if (cmd.System == (int)OrderSystem.Org)
                {
                    payorder = _payOrderRepo.Get(x => x.UserId != expectUserId && x.OrderId == cmd.AdvanceOrderId && (x.OrderStatus == (int)OrderStatus.PaySucess || x.OrderStatus == (int)OrderStatus.Refund || x.OrderStatus == (int)OrderStatus.PartRefund) && x.System == cmd.System);
                }
                if (null == payorder) throw new CustomResponseException("找不到该订单的支付订单，退款失败");

                var paydetail = _payCallBackRepo.Get(x => x.OutTradeNo == payorder.OrderNo);
                if (null == paydetail) throw new CustomResponseException("找不到该订单的支付回调记录，退款失败");

                //金额，状态逻辑验证
                if (cmd.RefundAmount / 100 > payorder.TotalAmount) throw new CustomResponseException("退款金额超过付款金额，退款失败");

                //获取退款总记录进行验证
                var list_refund = _refundOrderRepo.GetAll(x => x.PayOrderId == payorder.Id && x.Status == 1);
                if (null != list_refund && list_refund.Count() > 0)
                {
                    var test = list_refund.Sum(x => x.Amount) + cmd.RefundAmount;
                    if (test > payorder.TotalAmount)
                        throw new CustomResponseException("订单申请退款金额超过支付金额，退款失败");

                }

                var result = new RefundResult() { ApplySucess = false };
                //生成一个退款订单
                var refundOrderNo = $"FNC{_sxbGenerateNo.GetNumber()}";
                var refuundOrderId = Guid.NewGuid();
                var addResult = await CreateRefundOrder(cmd, refuundOrderId, payorder, refundOrderNo);
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

                            var r = await UpdateRefundOrderFreight(RefundStatusEnum.ApplySuccess, refuundOrderId, response.RefundId, response.ResultCode, payorder, cmd.RefundAmount,cmd.OrderId);
                            if (r)
                            {
                                await LogStatement(payorder, cmd.RefundAmount);
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
                            var r = await UpdateRefundOrderFreight(RefundStatusEnum.Fail, refuundOrderId, response.RefundId, response.ErrCodeDes, payorder, cmd.RefundAmount, cmd.OrderId);
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

                        var r = await UpdateRefundOrderFreight(RefundStatusEnum.Fail, refuundOrderId, response.RefundId, $"请求微信退款业务接口返回失败:{response.ReturnMsg}", payorder, cmd.RefundAmount, cmd.OrderId);

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
        private async Task<bool> LogStatement(iSchool.FinanceCenter.Domain.Entities.PayOrder order, decimal refundAmount)
        {
            var listSql = new List<iSchool.Domain.Modles.SqlSingle>();
            #region 公司流水添加
            var addStatmentRequest = new AddStatementDto()
            {

                Amount = order.TotalAmount,
                Io = StatementIoEnum.Out,
                StatementType = StatementTypeEnum.Outgoings,
                OrderId = order.OrderId,
                OrderType = OrderTypeEnum.OrgFx,
                Remark = "商城退款支出"

            };

            var r = AddStatementSql.AddStatement(addStatmentRequest);
            return await _stateMentRepo.ExecuteAsync(r.Sql, r.SqlParam);

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

        private Task<bool> UpdateRefundOrderAll(RefundStatusEnum status, Guid refundOrderId, string pay_platformid, string result, iSchool.FinanceCenter.Domain.Entities.PayOrder payOrder, decimal refund_amount)
        {

            var sqlBuld = new StringBuilder();
            if (status == RefundStatusEnum.ApplySuccess)
            {

                sqlBuld.Append($@"UPDATE [dbo].[PayOrder] SET  [OrderStatus] ={(int)OrderStatus.Refund}, [UpdateTime] =@UpdateTime,[RefundAmount]+=@RefundAmount  WHERE [ID] =@PayOrderId;");

                sqlBuld.Append($@"UPDATE [dbo].[ProductOrderRelation] SET  [Status] ={(int)OrderStatus.Refund}, [UpdateTime] =@UpdateTime  WHERE [AdvanceOrderId] = @AdvanceOrderId;");
            }
            sqlBuld.Append("Update dbo.RefundOrder Set Status=@Status,UpdateTime=@UpdateTime,PayPlatfomRefundId=@PayPlatfomRefundId,AapplyResultStr=@AapplyResultStr where Id=@ID; ");
            var param = new
            {
                ID = refundOrderId,
                UpdateTime = DateTime.Now,
                Status = (int)status,
                PayPlatfomRefundId = pay_platformid,
                AapplyResultStr = result,
                PayOrderId = payOrder.Id,
                RefundAmount = refund_amount,
                AdvanceOrderId = payOrder.OrderId

            };
            return _refundOrderRepo.ExecuteAsync(sqlBuld.ToString(), param);
        }

        private Task<bool> UpdateRefundOrderChild(RefundStatusEnum status, Guid refundOrderId, string pay_platformid, string result, iSchool.FinanceCenter.Domain.Entities.PayOrder payOrder, decimal refund_amount, Guid orderDetailId)
        {

            var sqlBuld = new StringBuilder();
            if (status == RefundStatusEnum.ApplySuccess)
            {

                sqlBuld.Append($@"UPDATE [dbo].[PayOrder] SET  [OrderStatus] ={(int)OrderStatus.PartRefund}, [UpdateTime] =@UpdateTime,[RefundAmount]+=@RefundAmount  WHERE [ID] =@PayOrderId;");

                sqlBuld.Append($@"UPDATE [dbo].[ProductOrderRelation] SET  [Status] ={(int)OrderStatus.Refund}, [UpdateTime] =@UpdateTime  WHERE [OrderId] in  @OrderDetailId;");
            }
            sqlBuld.Append("Update dbo.RefundOrder Set Status=@Status,UpdateTime=@UpdateTime,PayPlatfomRefundId=@PayPlatfomRefundId,AapplyResultStr=@AapplyResultStr where Id=@ID; ");
            var param = new
            {
                ID = refundOrderId,
                UpdateTime = DateTime.Now,
                Status = (int)status,
                PayPlatfomRefundId = pay_platformid,
                AapplyResultStr = result,
                PayOrderId = payOrder.Id,
                RefundAmount = refund_amount,
                OrderDetailId = orderDetailId

            };
            return _refundOrderRepo.ExecuteAsync(sqlBuld.ToString(), param);
        }

        private Task<bool> UpdateRefundOrderSku(RefundStatusEnum status, Guid refundOrderId, string pay_platformid, string result, iSchool.FinanceCenter.Domain.Entities.PayOrder payOrder, decimal refund_amount, IEnumerable<ProductOrderRelation> ProductOrderRelation)
        {

            var sqlBuld = new StringBuilder();
            if (status == RefundStatusEnum.ApplySuccess)
            {

                sqlBuld.Append($@"UPDATE [dbo].[PayOrder] SET  [OrderStatus] ={(int)OrderStatus.PartRefund}, [UpdateTime] =@UpdateTime,[RefundAmount]+=@RefundAmount  WHERE [ID] =@PayOrderId;");

                foreach (var item in ProductOrderRelation)
                {
                    sqlBuld.Append($@"UPDATE [dbo].[ProductOrderRelation] SET  [Status] ={(int)OrderStatus.Refund}, [UpdateTime] =@UpdateTime,[RefundAmount]={item.RefundAmount}  WHERE [Id]='{item.Id}';");
                }

            
            }
            sqlBuld.Append("Update dbo.RefundOrder Set Status=@Status,UpdateTime=@UpdateTime,PayPlatfomRefundId=@PayPlatfomRefundId,AapplyResultStr=@AapplyResultStr where Id=@ID; ");
            var param = new
            {
                ID = refundOrderId,
                UpdateTime = DateTime.Now,
                Status = (int)status,
                PayPlatfomRefundId = pay_platformid,
                AapplyResultStr = result,
                PayOrderId = payOrder.Id,
                RefundAmount = refund_amount,
              

            };
            return _refundOrderRepo.ExecuteAsync(sqlBuld.ToString(), param);
        }
        private Task<bool> UpdateRefundOrderFreight(RefundStatusEnum status, Guid refundOrderId, string pay_platformid, string result, iSchool.FinanceCenter.Domain.Entities.PayOrder payOrder, decimal refund_amount, Guid orderId)
        {

            var sqlBuld = new StringBuilder();
            if (status == RefundStatusEnum.ApplySuccess)
            {

                sqlBuld.Append($@"UPDATE [dbo].[PayOrder] SET  [OrderStatus] ={(int)OrderStatus.PartRefund}, [UpdateTime] =@UpdateTime,[RefundAmount]+=@RefundAmount  WHERE [ID] =@PayOrderId;");

                sqlBuld.Append($@"UPDATE [dbo].[ProductOrderRelation] SET  [Status] ={(int)OrderStatus.Refund}, [UpdateTime] =@UpdateTime  WHERE [OrderId]=@OrderId and ProductType={(int)ProductOrderType.Freight};");
            }
            sqlBuld.Append("Update dbo.RefundOrder Set Status=@Status,UpdateTime=@UpdateTime,PayPlatfomRefundId=@PayPlatfomRefundId,AapplyResultStr=@AapplyResultStr where Id=@ID; ");
            var param = new
            {
                ID = refundOrderId,
                UpdateTime = DateTime.Now,
                Status = (int)status,
                PayPlatfomRefundId = pay_platformid,
                AapplyResultStr = result,
                PayOrderId = payOrder.Id,
                RefundAmount = refund_amount,
                OrderId = orderId

            };
            return _refundOrderRepo.ExecuteAsync(sqlBuld.ToString(), param);
        }


        private Task<bool> CreateRefundOrder(PayRefundCommandExpVeison cmd, Guid refundOrderId, iSchool.FinanceCenter.Domain.Entities.PayOrder payorder, string no)
        {

            var orderid = cmd.OrderId;
            if (payorder.System == (int)OrderSystem.Org)
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
                    case RefundTypeEnum.Freight:
                        orderid = cmd.OrderId;
                        break;
                }

            }
            var sql = @"INSERT INTO dbo.RefundOrder
                        (ID,No,OrderId,CreateTime,Status,Remark,System,Amount,Type,PayOrderId)
                        VALUES
                         (@ID,@No,@OrderId,@CreateTime,@Status,@Remark,@System,@Amount,@Type,@PayOrderId)";
            var param = new
            {
                ID = refundOrderId,
                No = no,
                OrderId = orderid,
                CreateTime = DateTime.Now,
                Status = (int)RefundStatusEnum.InProcess,
                Remark = cmd.Remark,
                System = payorder.System,
                Amount = cmd.RefundAmount,
                type = (int)cmd.RefundType,
                PayOrderId=payorder.Id
            };
            return _refundOrderRepo.ExecuteAsync(sql, param);
        }
    }

}
