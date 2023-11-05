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
    public class FreeAmounAddToWalletHandle : IRequestHandler<FreeAmounAddToWalletDto, FreeAmounAddToWalletResult>
    {
        private readonly IRepository<Domain.Entities.Wallet> _repository;
        private readonly CSRedisClient _redis;
        private readonly IMediator _mediator;

        public FreeAmounAddToWalletHandle(IRepository<Domain.Entities.Wallet> repository, IRepository<CheckOrderResult> payOrderRepository, CSRedisClient redis, ISxbGenerateNo sxbGenerateNo, IMediator mediator)
        {
            _repository = repository;
            _redis = redis;
            _mediator = mediator;
        }

        public async Task<FreeAmounAddToWalletResult> Handle(FreeAmounAddToWalletDto dto, CancellationToken cancellationToken)
        {


            var userIdKey = CacheKeys.WalletUserId.FormatWith(dto.UserId.ToString());
            if(null==dto.OrderId&&dto.Type== FreezeMoneyInLogTypeEnum.OrgGoodNewReward) return new FreeAmounAddToWalletResult { ErrorDesc = "orderid 缺失" };
            var userIdLock = _redis.Lock(userIdKey, dto.UserId, 60);
            if (!userIdLock)
                return new FreeAmounAddToWalletResult { ErrorDesc = "用户钱包锁定，请稍后再试" };
            try
            {
                var model = new OperateWalletDto
                {
                    BlockedAmount = dto.BlockedAmount,
                    Io = StatementIoEnum.In,
                    OrderType = OrderTypeEnum.OrgFx,
                    Remark = dto.Remark,
                    StatementType = StatementTypeEnum.Blocked,
                    UserId = dto.UserId,
                    CompanyOperate=true
                 

                };
                if (dto.Type == FreezeMoneyInLogTypeEnum.OrgGoodNewReward)
                {
                    model.OrderId = dto.OrderId.Value;
                    model.OrderType = OrderTypeEnum.OrgNewReward;

                }
                var data = await _mediator.Send(model);
                var returnId = Guid.NewGuid();
                var logR=AddFreeAmounAddToWalletLog(dto,returnId);
                data.Sqls.AddRange(logR.Sqls);
                data.SqlParams.AddRange(logR.SqlParams);
                //事务执行，保证数据一致性
                var res = await _repository.Executes(data.Sqls, data.SqlParams);
                //订单完成操作，释放订单

                await _redis.DelAsync(userIdKey);
                return res ? new FreeAmounAddToWalletResult { FreezeMoneyInLogId= returnId,Success=true } : new FreeAmounAddToWalletResult { ErrorDesc = "事务执行失败", Success = false };
            }
            catch (Exception ex)
            {
                //操作异常，释放订单
              
                await _redis.DelAsync(userIdKey);
                return new FreeAmounAddToWalletResult { ErrorDesc = ex.Message, Success = false };
            }
        }

        private SqlBase AddFreeAmounAddToWalletLog(FreeAmounAddToWalletDto dto,Guid freezeMoneyInLogId)
        {
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            var sql = @"INSERT INTO dbo.FreezeMoneyInLog
                        (Id,UserId,BlockAmount,CreateTime,ModifyTime,Status,OrderId,Type)
                        VALUES
                        (@Id,@UserId,@BlockAmount,@CreateTime,@ModifyTime,@Status,@OrderId,@Type)";
            var param = new
            {
                ID = freezeMoneyInLogId,
                UserId = dto.UserId,
                BlockAmount=dto.BlockedAmount,
                CreateTime=DateTime.Now,
                ModifyTime=DateTime.Now,
                Status=0,
                OrderId=dto.OrderId,
                Type=dto.Type

            };
            sqlBase.Sqls.Add(sql);
            sqlBase.SqlParams.Add(param);
            return sqlBase;
        }
    }
}
