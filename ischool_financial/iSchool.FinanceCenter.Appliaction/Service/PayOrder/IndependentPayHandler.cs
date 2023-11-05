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
    /// 独立支付控制器
    /// </summary>
    public class IndependentPayHandler : IRequestHandler<IndependentPayCommand, WeChatPayDictionary>
    {
        private readonly IRepository<Domain.Entities.PayOrder> repository;
        private readonly IRepository<Domain.Entities.PayLog> _payLogRepository;
        private readonly IRepository<Domain.Entities.WxPayCallBackLog> _wxPayCallBackRepo;
        private readonly IWeChatPayClient _client;
        private readonly WeChatPayOptions _wechatConfig;
        private readonly CSRedisClient _redisClient;
        private readonly ISxbGenerateNo _sxbGenerateNo;
        private readonly IHttpContextAccessor _accessor;

        public IndependentPayHandler(IHttpContextAccessor accessor, IRepository<Domain.Entities.WxPayCallBackLog> wxPayCallBackRepo, CSRedisClient redisClient, IRepository<Domain.Entities.PayLog> payLogRepository, IRepository<Domain.Entities.PayOrder> repository, IWeChatPayClient client, IOptions<WeChatPayOptions> wechatPayConfig, ISxbGenerateNo sxbGenerateNo)
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
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<WeChatPayDictionary> Handle(IndependentPayCommand dto, CancellationToken cancellationToken)
        {
            var payorder = repository.Get(x => x.Id == dto.OrderId);
            if (null == payorder)
            {
                payorder = repository.Get(x => x.OrderNo == dto.OrderNo);
                if (null == payorder)
                {
                    throw new CustomResponseException("参数有误，不存在该订单");
                }
            }
            if (payorder.OrderStatus != (byte)OrderStatus.Wait)
            {
                throw new CustomResponseException("订单已支付或已退款，请核实");
            }
            if (dto.OpenId.IsNullOrEmpty()) throw new CustomResponseException("openid 缺失");
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            var prepay_id = _redisClient.Get(CacheKeys.WechatPrePayId.FormatWith(dto.OrderId, dto.IsWechatMiniProgram, dto.OpenId));

         


            var wechat_amout = Convert.ToInt32(payorder.PayAmount * 100);
            var appid = ConfigHelper.GetConfigs("WeChatPay", "AppId");
            var _result = new WeChatPayDictionary();

            var post_json = "";
            var pay_type = (byte)PayTypeEnum.Recharge;
            //使用预支付ID重新支付
            if (!prepay_id.IsNullOrEmpty())
            {

                //查找支付记录和回调记录
                var payLog = _payLogRepository.GetAll(x => x.OrderId == payorder.OrderId).OrderByDescending(x => x.CreateTime).FirstOrDefault();
                if (null != payLog && payLog.PayStatus != (int)PayStatusEnum.InProcess) throw new CustomResponseException("该订单已经支付过了，请勿重复支付");
                var callBackLog = _wxPayCallBackRepo.Get(x => x.OutTradeNo == payorder.OrderNo);
                if (null != callBackLog)
                {
                    if (callBackLog.TradeState == "SUCCESS") throw new CustomResponseException("该订单已经支付过了，请勿重复支付");
                }
                //价格没修改重新支付
                if (null != payLog)
                {
                    //回调慢时。达人修改提问价格，重新用预支付请求微信核实是否已经支付成功。--待写


          
                    pay_type = (byte)PayTypeEnum.RePay;

                }



            }

            #region 小程序与jsapi支付

            var wechatConfigClone = _wechatConfig.Clone();
            if (1 == dto.IsWechatMiniProgram)//小程序支付
            {
                if (!string.IsNullOrEmpty(dto.AppId)) appid = dto.AppId;
                else appid = ConfigHelper.GetConfigs("WeChatPay", "MiniProgramAppId");

            }
            wechatConfigClone.AppId = appid;
            if (string.IsNullOrEmpty(prepay_id))
            {


                var model = new WeChatPayTransactionsJsApiModel
                {
                    AppId = appid,
                    MchId = ConfigHelper.GetConfigs("WeChatPay", "MchId"),
                    Amount = new Amount { Total = wechat_amout, Currency = "CNY" },
                    Description = payorder.Remark,
                    NotifyUrl = ConfigHelper.GetConfigs("WeChatPay", "WeChatPayNotifyUrl"),
                    OutTradeNo = payorder.OrderNo,
                    Payer = new PayerInfo { OpenId = dto.OpenId },
                    Attach = string.Empty,


                };
                if (null != payorder.OrderExpireTime)
                {
                    model.TimeExpire = GetDate(payorder.OrderExpireTime.Value);
                }

                var request = new WeChatPayTransactionsJsApiRequest();
                request.SetQueryModel(model);
                var response = await _client.ExecuteAsync(request, wechatConfigClone);

                if (response.StatusCode == 200)
                {
                    prepay_id = response.PrepayId;
                    _redisClient.Set(CacheKeys.WechatPrePayId.FormatWith(dto.OrderId, dto.IsWechatMiniProgram, dto.OpenId), prepay_id, TimeSpan.FromHours(1.98)); //预支付ID有效期为2小时
                }
                else
                { //支付失败

                    throw new CustomResponseException("微信预支付下单返回错误:" + response.Detail + "|" + response.Message);
                }
                post_json = JsonConvert.SerializeObject(model);
            }

            var reqest_sdk = new WeChatPayJsApiSdkRequest
            {
                Package = "prepay_id=" + prepay_id
            };

            _result = await _client.ExecuteAsync(reqest_sdk, wechatConfigClone);


            #endregion

            ////添加支付记录
            var addM = new PayLog()
            {
                UserId = payorder.UserId,
                PrepayId = prepay_id,
                TradeNo = payorder.OrderNo,
                OrderId = payorder.OrderId,
                PayType = pay_type,
                PayWay = (byte)PayWayEnum.WeChatPay,
                PayStatus = (byte)PayStatusEnum.InProcess,
                TotalAmount = payorder.PayAmount,
                PostJson = post_json,
                ProcedureKb = 6,
            };
            var addLogSql = AddPayLog(addM);




            sqlBase.Sqls.AddRange(addLogSql.Sqls);
            sqlBase.SqlParams.AddRange(addLogSql.SqlParams);
            //事务执行，保证数据一致性
            var res = await repository.Executes(sqlBase.Sqls, sqlBase.SqlParams);
            if (!res) throw new CustomResponseException("内部订单下单执行事务失败");
            return _result;


        }
        private string GetDate(DateTime DateTime)
        {
            DateTime UtcDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime);
            return XmlConvert.ToString(UtcDateTime, XmlDateTimeSerializationMode.Utc);
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
