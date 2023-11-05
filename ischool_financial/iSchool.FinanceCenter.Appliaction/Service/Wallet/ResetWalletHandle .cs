using CSRedis;
using Dapper;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Wallet;
using iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder;
using iSchool.Infrastructure;
using MediatR;
using Newtonsoft.Json;
using Sxb.GenerateNo;
using Sxb.PayCenter.WechatPay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace iSchool.FinanceCenter.Appliaction.Service.Wallet
{
    public class ResetWalletHandle : IRequestHandler<WalletResetDto, bool>
    {
        private readonly IRepository<Domain.Entities.Wallet> _repository;
        private readonly CSRedisClient _redis;
        private readonly IMediator _mediator;


        public ResetWalletHandle(IRepository<Domain.Entities.Wallet> repository, IRepository<CheckOrderResult> payOrderRepository, CSRedisClient redis, ISxbGenerateNo sxbGenerateNo, IMediator mediator)

        {
            _repository = repository;
            _redis = redis;
            _mediator = mediator;
        }

        public async Task<bool> Handle(WalletResetDto dto, CancellationToken cancellationToken)
        {


            if (dto.UserId == Guid.Empty)
            {
                var data = _repository.Query("SELECT UserId, TotalAmount, BlockedAmount, RemainAmount, UpdateTime, VirtualTotalAmount, VirtualRemainAmount, CheckSign FROM dbo.Wallet ", new { });
                var sqlbase = new SqlBase();
                sqlbase.Sqls = new List<string>();
                sqlbase.SqlParams = new List<object>();
                foreach (var item in data)
                {
                    var _CheckSign = CheckSign(item);
                    var sql = "update Wallet set CheckSign=@CheckSign where UserId=@UserId";
                    sqlbase.Sqls.Add(sql);
                    var param = new DynamicParameters();
                    param.Add("CheckSign", _CheckSign);
                    param.Add("UserId", item.UserId);
                    sqlbase.SqlParams.Add(param);

                }
                return await _repository.Executes(sqlbase.Sqls, sqlbase.SqlParams);
            }
            else
            {
                var data = _repository.Query("SELECT UserId, TotalAmount, BlockedAmount, RemainAmount, UpdateTime, VirtualTotalAmount, VirtualRemainAmount, CheckSign FROM dbo.Wallet WHERE UserId = @UserId ", new { UserId = dto.UserId })?.FirstOrDefault();
                var _CheckSign = CheckSign(data);
                if (null != data)
                {
                    var sql = "update Wallet set CheckSign=@CheckSign where UserId=@UserId";
                    return await _repository.ExecuteAsync(sql, new { CheckSign = _CheckSign, UserId = data.UserId });
                }
            }
            return false;
        }
        /// <summary>
        /// 查询数据生成用户信息认证密钥
        /// </summary>
        /// <param name="data"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        private  string CheckSign(Domain.Entities.Wallet data)
        {
            var paramJson = new
            {
                userId = data.UserId,
                totalAmount = (int)(data.TotalAmount * 10000),
                blockedAmount = (int)(data.BlockedAmount * 10000),
                remainAmount = (int)(data.RemainAmount * 10000),
                updateTime = data.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                virtualTotalAmount = (int)(data.VirtualTotalAmount * 10000),
                virtualRemainAmount = (int)(data.VirtualRemainAmount * 10000)
            };
            var json = JsonConvert.SerializeObject(paramJson).Trim();
            var paramMd5 = MD5.Compute(json).ToLowerInvariant();
            return paramMd5;
        }
    }
}
