using CSRedis;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.PayOrder;
using iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder;
using iSchool.FinanceCenter.Domain.Entities;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.FinanceCenter.Domain.Redis;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Timing;
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
    /// 支付中心查询订单详情 
    /// </summary>
    public class PayInfoCheckHandler : IRequestHandler<PayInfoCheckCommand, PayInfoCheckResult>
    {
        private readonly IRepository<Domain.Entities.PayOrder> repository;
       
        private readonly CSRedisClient _redisClient;
     

        public PayInfoCheckHandler( IRepository<Domain.Entities.PayOrder> repository, CSRedisClient redisClient)
        {
            this.repository = repository;
            this._redisClient = redisClient;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<PayInfoCheckResult> Handle(PayInfoCheckCommand dto, CancellationToken cancellationToken)
        {
            var PayInfoCheckResult = new PayInfoCheckResult();
            var payorder = repository.Get(x => x.Id == dto.OrderId);
            if (null == payorder)
            {
                payorder = repository.Get(x => x.OrderNo == dto.OrderNo);
                if (null == payorder)
                {
                    throw new CustomResponseException("参数有误，不存在该订单");
                }
            }
            PayInfoCheckResult.PayStatus = payorder.OrderStatus;
            PayInfoCheckResult.PayOrderId = dto.OrderId;
            PayInfoCheckResult.PayOrderNo = payorder.OrderNo;
            PayInfoCheckResult.PayAmount = payorder.PayAmount;
            PayInfoCheckResult.Remark = payorder.Remark;
            if(null != payorder.OrderExpireTime)
            PayInfoCheckResult.OrderExpireTime = payorder.OrderExpireTime.Value.ToUnixTimestampByMilliseconds();
            return PayInfoCheckResult;


        }
   


     


    }
}
