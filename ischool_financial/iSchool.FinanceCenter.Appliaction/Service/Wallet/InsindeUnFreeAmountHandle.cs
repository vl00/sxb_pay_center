using CSRedis;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Statement;
using iSchool.FinanceCenter.Appliaction.RequestDto.Wallet;
using iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Wallet;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.Service.Statement;
using iSchool.FinanceCenter.Appliaction.Service.Wallet;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.FinanceCenter.Domain.Redis;
using iSchool.Infrastructure;
using MediatR;
using Sxb.GenerateNo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Wallet
{
    public class InsindeUnFreeAmountHandle : IRequestHandler<UnFreeAmountDto, bool>
    {
        private readonly IRepository<Domain.Entities.FreezeMoneyInLog> _repository;
        private readonly CSRedisClient _redis;
        private readonly IMediator _mediator;

        public InsindeUnFreeAmountHandle(IRepository<Domain.Entities.FreezeMoneyInLog> repository, CSRedisClient redis,  IMediator mediator)
        {
            _repository = repository;
            _redis = redis;
            _mediator = mediator;
        }

        public async Task<bool> Handle(UnFreeAmountDto dto, CancellationToken cancellationToken)
        {
            if (dto.Type == FreezeMoneyInLogTypeEnum.OrgGoodNewReward)
            {
                var log = _repository.Get(x => x.Id == dto.FreezeMoneyInLogId);
                if (null == log) throw new CustomResponseException("参数有误，找不到待解冻记录");
                if (log.Status != 0) throw new CustomResponseException("该记录已经处理，无需重复操作");
                var userIdKey = CacheKeys.WalletUserId.FormatWith(log.UserId.ToString());
                var userIdLock = _redis.Lock(userIdKey, log.UserId, 60);
                if (!userIdLock)
                    throw new CustomResponseException("用户钱包锁定，请稍后再试");

                try
                {
                    var model = new OperateWalletDto
                    {
                        BlockedAmount = log.BlockAmount,
                        Io = StatementIoEnum.In,
                        OrderType = OrderTypeEnum.OrgNewReward,
                        // Remark = $"解冻内部冻结金额FreezeMoneyInLogId:{dto.FreezeMoneyInLogId}",
                        Remark = "购物新人立返奖励结算",
                        StatementType = StatementTypeEnum.Unfreeze,
                        UserId = log.UserId,
                        CompanyOperate = true

                    };
                    var data = await _mediator.Send(model);

                    var logR = UpdateFreezeMoneyInLog(log.Id);
                    data.Sqls.AddRange(logR.Sqls);
                    data.SqlParams.AddRange(logR.SqlParams);
                    //事务执行，保证数据一致性
                    var res = await _repository.Executes(data.Sqls, data.SqlParams);
                    //订单完成操作，释放订单

                    await _redis.DelAsync(userIdKey);
                    return res;
                }
                catch (Exception ex)
                {
                    //操作异常，释放订单

                    await _redis.DelAsync(userIdKey);
                    throw new CustomResponseException(ex.Message);
                }
            }
            else if (dto.Type == FreezeMoneyInLogTypeEnum.SignIn)
            {
                var log = _repository.Get(x => x.Id == dto.FreezeMoneyInLogId);
                if (null == log) throw new CustomResponseException("参数有误，找不到待解冻记录");
                if (log.Status != 0) throw new CustomResponseException("该记录已经处理，无需重复操作");
                var userIdKey = CacheKeys.WalletUserId.FormatWith(log.UserId.ToString());
                var userIdLock = _redis.Lock(userIdKey, log.UserId, 60);
                if (!userIdLock)
                    throw new CustomResponseException("用户钱包锁定，请稍后再试");

                try
                {
                    var model = new OperateWalletDto
                    {

                        BlockedAmount = log.BlockAmount,
                        Io = StatementIoEnum.In,
                        OrderType = OrderTypeEnum.OrgFx,
                        // Remark = $"解冻内部冻结金额FreezeMoneyInLogId:{dto.FreezeMoneyInLogId}",
                        Remark = "签到活动奖励结算",
                        StatementType = StatementTypeEnum.Unfreeze,
                        UserId = log.UserId,
                        CompanyOperate = true

                    };
                    var data = await _mediator.Send(model);

                    var logR = UpdateFreezeMoneyInLog(log.Id);
                    data.Sqls.AddRange(logR.Sqls);
                    data.SqlParams.AddRange(logR.SqlParams);
                    //事务执行，保证数据一致性
                    var res = await _repository.Executes(data.Sqls, data.SqlParams);
                    //订单完成操作，释放订单

                    await _redis.DelAsync(userIdKey);
                    return res;
                }
                catch (Exception ex)
                {
                    //操作异常，释放订单

                    await _redis.DelAsync(userIdKey);
                    throw new CustomResponseException(ex.Message);
                }

            }
            return false;
        }

        private SqlBase UpdateFreezeMoneyInLog(Guid freezeMoneyInLogId)
        {
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            var sql = @"Update dbo.FreezeMoneyInLog set Status=@Status,ModifyTime=@ModifyTime
                        where Id=@Id
                      ";
            var param = new
            {
                Id = freezeMoneyInLogId,
                ModifyTime=DateTime.Now,
                Status=1

            };
            sqlBase.Sqls.Add(sql);
            sqlBase.SqlParams.Add(param);
            return sqlBase;
        }
    }
}
