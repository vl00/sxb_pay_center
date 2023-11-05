using CSRedis;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.PayOrder;
using iSchool.FinanceCenter.Domain.Entities;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.FinanceCenter.Domain.Redis;
using iSchool.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sxb.GenerateNo;
using Sxb.PayCenter.WechatPay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace iSchool.FinanceCenter.Appliaction.Service.PayOrder
{
    /// <summary>
    /// 新增支付订单控制器
    /// </summary>
    public class AddPayOrderHandler : IRequestHandler<AddPayOrderCommand, WeChatPayDictionary>
    {
        private readonly IRepository<Domain.Entities.PayOrder> repository;
        private readonly IRepository<Domain.Entities.PayLog> _payLogRepository;
        private readonly IRepository<Domain.Entities.WxPayCallBackLog> _wxPayCallBackRepo;
        private readonly IWeChatPayClient _client;
        private readonly WeChatPayOptions _wechatConfig;
        private readonly CSRedisClient _redisClient;
        private readonly ISxbGenerateNo _sxbGenerateNo;
        private readonly IHttpContextAccessor _accessor;

        public AddPayOrderHandler(IHttpContextAccessor accessor, IRepository<Domain.Entities.WxPayCallBackLog> wxPayCallBackRepo, CSRedisClient redisClient, IRepository<Domain.Entities.PayLog> payLogRepository, IRepository<Domain.Entities.PayOrder> repository, IWeChatPayClient client, IOptions<WeChatPayOptions> wechatPayConfig, ISxbGenerateNo sxbGenerateNo)
        {
            this.repository = repository;
            _client = client;
            _wechatConfig = wechatPayConfig.Value;
            _payLogRepository = payLogRepository;
            _redisClient = redisClient;
            _wxPayCallBackRepo = wxPayCallBackRepo;
            _sxbGenerateNo = sxbGenerateNo;
            _accessor = accessor;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<WeChatPayDictionary> Handle(AddPayOrderCommand cmd, CancellationToken cancellationToken)
        {        
            var needNew = 0;
            var prepay_id = "";
            var dto = cmd.Param;
            //参数验证
            if (string.IsNullOrEmpty(dto.OrderNo))
            {
                throw new CustomResponseException("OrderNo 为必填项");
            }
            var old = repository.Get(x=>x.OrderId==cmd.Param.OrderId&&x.System==(byte)cmd.Param.System);
            if (null != old && old.OrderStatus == (byte)OrderStatus.PaySucess)
            {
                throw new CustomResponseException("该订单已经支付过了。请勿重新支付");
            }
            
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            if (null != old&&dto.NoNeedPay==0)//重新支付，有可能换人
            {

                prepay_id = _redisClient.Get(CacheKeys.WechatPrePayId.FormatWith(dto.OrderId, dto.IsWechatMiniProgram, dto.OpenId));
                if (string.IsNullOrEmpty(prepay_id))//换人了,要重新下单
                {
                    throw new CustomResponseException("请使用原支付的微信账号继续支付");
                    //needNew = 1;
                }
                else {
                    dto.TradeNo = old.OrderNo;
                    dto.IsRepay = 1;

                }
                
              
            }
            else {
                needNew = 1;
            }
            var payorderid = Guid.NewGuid();
            if (1 == needNew)
            {
                //新增支付订单
                dto.TradeNo = $"FNC{_sxbGenerateNo.GetNumber()}";
                var payOrderSlq = AddPayOrder(dto, payorderid);
                sqlBase.Sqls.AddRange(payOrderSlq.Sqls);
                sqlBase.SqlParams.AddRange(payOrderSlq.SqlParams);
                //订单产品信息
                if (null != dto.OrderByProducts && dto.OrderByProducts.Count > 0 && (byte)cmd.Param.System == (int)OrderSystem.Org)
                {
                    //验证合单金额合理性
                    var amount = dto.OrderByProducts.Sum(x => x.Price * x.BuyNum);
                    if (dto.TotalAmount != amount)
                    {
                        throw new CustomResponseException("订单金额与产品总金额不匹配");
                    }
                    var productSql = AddOrderByProducts(dto.OrderByProducts);
                    sqlBase.Sqls.AddRange(productSql.Sqls);
                    sqlBase.SqlParams.AddRange(productSql.SqlParams);
                }

            }
         
            int wechat_amout = 0;
            if (dto.TotalAmount != dto.PayAmount + dto.DiscountAmount)
            {
                throw new CustomResponseException("金额参数有误");
            }
            wechat_amout = Convert.ToInt32(dto.PayAmount * 100);
          
            var freightFee = dto.FreightFee;
      
            if (0 == dto.NoNeedPay)
            {
                //请求微信支付
                var result = await WechtPay(dto, wechat_amout, cancellationToken);
                sqlBase.Sqls.AddRange(result.Item2.Sqls);
                sqlBase.SqlParams.AddRange(result.Item2.SqlParams);
                //事务执行，保证数据一致性
                var res = await repository.Executes(sqlBase.Sqls, sqlBase.SqlParams);
                if (!res) throw new CustomResponseException("内部订单下单执行事务失败");
                //订单对应的回调地址放redis
                _redisClient.Set(dto.TradeNo, dto.CallBackLink, TimeSpan.FromDays(1));
                return result.Item1;
            }
            else
            {
                if (sqlBase.Sqls.Count>0)
                {
                    //事务执行，保证数据一致性
                    var res = await repository.Executes(sqlBase.Sqls, sqlBase.SqlParams);
                    if (!res) throw new CustomResponseException("内部订单下单执行事务失败");
                }
                //订单对应的回调地址放redis
                _redisClient.Set(dto.TradeNo,dto.CallBackLink,TimeSpan.FromDays(1));
                return new WeChatPayDictionary
                {
                    { "result", "下单成功" },
                    {"orderid",payorderid.ToString() },
                    {"orderno",dto.TradeNo }
                };
            }

        }

        private string GetDate(DateTime DateTime)
        {
            DateTime UtcDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime);
            return XmlConvert.ToString(UtcDateTime, XmlDateTimeSerializationMode.Utc);
        }


        private async Task<(WeChatPayDictionary, SqlBase)> WechtPay(AddPayOrderDto dto, int wechat_amout, CancellationToken cancellationToken)
        {
            var appid = ConfigHelper.GetConfigs("WeChatPay", "AppId");
            var _result = new WeChatPayDictionary();
            var prepay_id = "";
            var post_json = "";
            var pay_type = (byte)PayTypeEnum.Recharge;
           //使用预支付ID重新支付
            if(1==dto.IsRepay)
            {

                //查找支付记录和回调记录
                var payLog = _payLogRepository.GetAll(x => x.OrderId == dto.OrderId).OrderByDescending(x => x.CreateTime).FirstOrDefault();
                if (null != payLog && payLog.PayStatus != (int)PayStatusEnum.InProcess) throw new CustomResponseException("该订单已经支付过了，请勿重复支付");
                var callBackLog = _payLogRepository.Get(x => x.OrderId == dto.OrderId);
                if (null != callBackLog)
                {
                    if (callBackLog.PayStatus != (int)PayStatusEnum.InProcess) throw new CustomResponseException("该订单已经支付过了，请勿重复支付");
                }
                //价格没修改重新支付
                if (null != payLog)
                {
                    //回调慢时。达人修改提问价格，重新用预支付请求微信核实是否已经支付成功。--待写


                    prepay_id = _redisClient.Get(CacheKeys.WechatPrePayId.FormatWith(dto.OrderId, dto.IsWechatMiniProgram,dto.OpenId));
                    pay_type = (byte)PayTypeEnum.RePay;

                }



            }
            #region h5支付
            if (2 == dto.IsWechatMiniProgram)
            {


                string ipString = _accessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                var h5RequestParam = new WeChatPayTransactionsH5Model
                {
                    AppId = appid,
                    MchId = ConfigHelper.GetConfigs("WeChatPay", "MchId"),
                    Amount = new Amount { Total = wechat_amout, Currency = "CNY" },
                    Description = dto.Remark,
                    NotifyUrl = ConfigHelper.GetConfigs("WeChatPay", "WeChatPayNotifyUrl"),
                    OutTradeNo = dto.TradeNo,
                    SceneInfo = new SceneInfo { PayerClientIp = ipString },
                    Attach = dto.Attach
                };
                if (null != dto.OrderExpireTime)
                {
                    h5RequestParam.TimeExpire = GetDate(dto.OrderExpireTime.Value);
                }
             
                var h5_request = new WeChatPayTransactionsH5Request();
                h5_request.SetQueryModel(h5RequestParam);
                var wechatConfigClone = _wechatConfig.Clone();
                wechatConfigClone.AppId = appid;
                var h5_response = await _client.ExecuteAsync(h5_request, wechatConfigClone);
                if (h5_response.StatusCode == 200)
                {

                    // h5_url为拉起微信支付收银台的中间页面，可通过访问该url来拉起微信客户端，完成支付,h5_url的有效期为5分钟。
                    // https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter3_3_4.shtml
                    _result = new WeChatPayDictionary
                    {
                        { "h5_url",h5_response.H5Url}
                    };
                }
                else
                { //支付失败

                    throw new CustomResponseException("微信H5支付预支付下单返回错误:" + h5_response.Detail + "|" + h5_response.Message);
                }


            }
            #endregion
            #region 小程序与jsapi支付
            else
            {
                var wechatConfigClone = _wechatConfig.Clone();
                if (1 == dto.IsWechatMiniProgram)//小程序支付
                {
                    if (!string.IsNullOrEmpty(dto.AppId)) appid = dto.AppId;
                    else appid = ConfigHelper.GetConfigs("WeChatPay", "MiniProgramAppId");

                }
                if (string.IsNullOrEmpty(prepay_id))
                {

                    wechatConfigClone.AppId = appid;
                    var model = new WeChatPayTransactionsJsApiModel
                    {
                        AppId = appid,
                        MchId = ConfigHelper.GetConfigs("WeChatPay", "MchId"),
                        Amount = new Amount { Total = wechat_amout, Currency = "CNY" },
                        Description = dto.Remark,
                        NotifyUrl = ConfigHelper.GetConfigs("WeChatPay", "WeChatPayNotifyUrl"),
                        OutTradeNo = dto.TradeNo,
                        Payer = new PayerInfo { OpenId = dto.OpenId },
                        Attach = dto.Attach,

                    };
                    if (null != dto.OrderExpireTime)
                    {
                        model.TimeExpire = GetDate(dto.OrderExpireTime.Value);
                    }
                    var request = new WeChatPayTransactionsJsApiRequest();
                    request.SetQueryModel(model);
                    var response = await _client.ExecuteAsync(request, wechatConfigClone);

                    if (response.StatusCode == 200)
                    {
                        prepay_id = response.PrepayId;
                        _redisClient.Set(CacheKeys.WechatPrePayId.FormatWith(dto.OrderId, dto.IsWechatMiniProgram,dto.OpenId), prepay_id, TimeSpan.FromHours(1.98)); //预支付ID有效期为2小时
                    }
                    else
                    { //支付失败

                        throw new CustomResponseException("微信预支付下单返回错误:" + response.Detail + "|" + response.Message);
                    }
                    post_json = JsonConvert.SerializeObject(model);
                }
                else
                {
                    wechatConfigClone.AppId = appid;

                }

                var reqest_sdk = new WeChatPayJsApiSdkRequest
                {
                    Package = "prepay_id=" + prepay_id
                };

                _result = await _client.ExecuteAsync(reqest_sdk, wechatConfigClone);

            }
            #endregion

            ////添加支付记录
            var addM = new PayLog()
            {
                UserId = dto.UserId,
                PrepayId = prepay_id,
                TradeNo = dto.TradeNo,
                OrderId = dto.OrderId,
                PayType = pay_type,
                PayWay = (byte)PayWayEnum.WeChatPay,
                PayStatus = (byte)PayStatusEnum.InProcess,
                TotalAmount = dto.PayAmount,
                PostJson = post_json,
                ProcedureKb = 6,
            };
            var addLogSql = AddPayLog(addM);
            return (_result, addLogSql);


        }
        /// <summary>
        /// 新增支付订单sql
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private SqlBase AddPayOrder(AddPayOrderDto dto,Guid id)
        {
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();

            var sql = @"INSERT INTO [dbo].[payOrder] (id,userId,orderId,orderType,orderStatus,totalAmount,payAmount,discountAmount,createTime,updateTime,remark,System,OrderNo,SourceOrderNo,FreightCost,OrderExpireTime)
            VALUES (@id,@userId,@orderId,@orderType,@orderStatus,@totalAmount,@payAmount,@discountAmount,@createTime,@updateTime,@remark,@System,@OrderNo,@SourceOrderNo,@FreightCost,@OrderExpireTime)";
            var param = new
            {
                id = id,
                userId = dto.UserId,
                orderId = dto.OrderId,
                orderType = dto.OrderType,
            
                totalAmount = dto.TotalAmount,
                payAmount = dto.PayAmount,
                discountAmount = dto.DiscountAmount,
             
                createTime = DateTime.Now,
                updateTime = DateTime.Now,
                remark = dto.Remark,
                System = dto.System,
                OrderNo = dto.TradeNo,
                SourceOrderNo = dto.OrderNo,
                FreightCost = dto.FreightFee,
                orderStatus = (byte)OrderStatus.Wait,
                OrderExpireTime = dto.OrderExpireTime,
            };
            sqlBase.Sqls.Add(sql);
            sqlBase.SqlParams.Add(param);



            return sqlBase;
        }

        /// <summary>
        /// 订单产品信息sql
        /// </summary>
        /// <param name="dtos"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private SqlBase AddOrderByProducts(List<OrderByProduct> dtos)
        {

            //orderId 对机构来说，这个是advanceorderid

            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            var sql = @"INSERT INTO dbo.productOrderRelation
                        (id,amount,status,orderId,productType,productId,remark,createTime,updateTime,AdvanceOrderId,OrderDetailId,Price)
                        VALUES
                        (@id,@amount,@status,@orderId,@productType,@productId,@remark,@createTime,@updateTime,@AdvanceOrderId,@OrderDetailId,@Price)";


            foreach (var item in dtos)
            {

                //买多个拆单
                if (item.BuyNum > 1)
                {
                    for (int i = 0; i < item.BuyNum; i++)
                    {

                        var param = new
                        {
                            id = Guid.NewGuid(),
                            amount = item.Amount,
                            status = item.Status,
                            orderId = item.OrderId,
                            productType = item.productType,
                            productId = item.productId,
                            remark = item.Remark,
                            createTime = DateTime.Now,
                            updateTime = DateTime.Now,
                            AdvanceOrderId = item.AdvanceOrderId,
                            OrderDetailId = item.OrderDetailId,
                            Price = item.Price


                        };

                        sqlBase.Sqls.Add(sql);
                        sqlBase.SqlParams.Add(param);
                    }


                }
                else
                {

                    var param = new
                    {
                        id = Guid.NewGuid(),
                        amount = item.Amount,
                        status = item.Status,
                        orderId = item.OrderId,
                        productType = item.productType,
                        productId = item.productId,
                        remark = item.Remark,
                        createTime = DateTime.Now,
                        updateTime = DateTime.Now,
                        AdvanceOrderId = item.AdvanceOrderId,
                        OrderDetailId = item.OrderDetailId,
                        Price = item.Price

                    };

                    sqlBase.Sqls.Add(sql);
                    sqlBase.SqlParams.Add(param);
                }


            }
            return sqlBase;
        }


        private SqlBase AddPayLog(PayLog dto)
        {
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            var sql = @"INSERT INTO dbo.PayLog
                        (ID,UserId,PrepayId,TradeNo,OrderId,PayType,PayWay,PayStatus,TotalAmount,PostJson,CreateTime,ProcedureKb)
                        VALUES
                         (@ID,@UserId,@PrepayId,@TradeNo,@OrderId,@PayType,@PayWay,@PayStatus,@TotalAmount,@PostJson,@CreateTime,@ProcedureKb)";

            var param = new
            {
                ID = Guid.NewGuid(),
                UserId = dto.UserId,
                PrepayId = dto.PrepayId,
                TradeNo = dto.TradeNo,
                OrderId = dto.OrderId,
                PayType = (byte)PayTypeEnum.Recharge,
                PayWay = (byte)PayWayEnum.AliPay,
                PayStatus = (byte)PayStatusEnum.InProcess,
                TotalAmount = dto.TotalAmount,
                PostJson = dto.PostJson,
                CreateTime = DateTime.Now,
                ProcedureKb = 6,

            };
            sqlBase.Sqls.Add(sql);
            sqlBase.SqlParams.Add(param);
            return sqlBase;
        }


    }
}
