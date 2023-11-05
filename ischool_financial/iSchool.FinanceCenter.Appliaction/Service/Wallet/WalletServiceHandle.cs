using CSRedis;
using Dapper;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Statement;
using iSchool.FinanceCenter.Appliaction.RequestDto.Wallet;
using iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.Service.Statement;
using iSchool.FinanceCenter.Appliaction.Service.Wallet;
using iSchool.FinanceCenter.Domain;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.FinanceCenter.Domain.Redis;
using iSchool.Infrastructure;
using iSchool.Infrastructure.UoW;
using MediatR;
using Microsoft.Extensions.Logging;
using NLog;
using Sxb.GenerateNo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Wallet
{
    public class WalletServiceHandle : IRequestHandler<WalletServiceDto, FreeszeAmountDto>
    {
        private readonly IRepository<Domain.Entities.Statement> _repositoryStatement;
        private readonly IRepository<Domain.Entities.Wallet> _repository;
        private readonly CSRedisClient _redis;
        private readonly IMediator _mediator;
        private readonly ILogger<WalletServiceHandle> _logger;
        private readonly FinanceCenterUnitOfWork _financeUnitOfWork;

        public WalletServiceHandle(IRepository<Domain.Entities.Statement> repositoryStatement, IFinanceCenterUnitOfWork financeUnitOfWork,ILogger<WalletServiceHandle> logger,IRepository<Domain.Entities.Wallet> repository, IRepository<CheckOrderResult> payOrderRepository, CSRedisClient redis, ISxbGenerateNo sxbGenerateNo, IMediator mediator)


        {
            _logger = logger;
            _repository = repository;
            _redis = redis;
            _mediator = mediator;
            _financeUnitOfWork = (FinanceCenterUnitOfWork)financeUnitOfWork;
            _repositoryStatement = repositoryStatement;
        }

        public async Task<FreeszeAmountDto> Handle(WalletServiceDto dto, CancellationToken cancellationToken)
        {
            //防并发，锁定订单，订单做唯一操作
            var orderIdKey = CacheKeys.WalletOrderId.FormatWith(dto.OrderId.ToString());
            var orderIdLock = _redis.Lock(orderIdKey, dto.UserId, 60);
            if (!orderIdLock)

                return new FreeszeAmountDto { OrderId = dto.OrderId, Success = false, RefuseContent = "此订单正在操作，请稍后", BonusOrderId = dto.BonusOrderId };


            var userIdKey = CacheKeys.WalletUserId.FormatWith(dto.UserId.ToString());
            var userIdLock = _redis.Lock(userIdKey, dto.UserId, 60);
            if (!userIdLock)

                return new FreeszeAmountDto { OrderId = dto.OrderId, Success = false, RefuseContent = "用户钱包锁定，请稍后", BonusOrderId = dto.BonusOrderId };
            // 防止重复解冻
            var doneBefor = _repositoryStatement.GetAll(x => x.StatementType == (byte)dto.StatementType && x.OrderDetailId == dto.BonusOrderId && x.UserId == dto.UserId && x.OrderId == dto.OrderId).FirstOrDefault();
            if (null != doneBefor)
            {
                return new FreeszeAmountDto { OrderId = dto.OrderId, Success = false, RefuseContent = "该单已经解冻过了，请勿重复处理", BonusOrderId = dto.BonusOrderId };
            }

            try
            {
                _financeUnitOfWork.BeginTransaction();
                var model = new OperateWalletDto
                {
                    Amount = dto.Amount,
                    BlockedAmount = dto.BlockedAmount,
                    Io = dto.Io,
                    OrderId = dto.OrderId,
                    OrderDetailId = dto.BonusOrderId,
                    OrderType = dto.OrderType,
                    Remark = dto.Remark,
                    StatementType = dto.StatementType,
                    UserId = dto.UserId,
                    VirtualAmount = dto.VirtualAmount,
                    FixTime = dto.FixTime
                };
                var data = await _mediator.Send(model);
                try
                {
                    var arrSql = data.Sqls.ToArray();
                    var arrpParam = data.SqlParams.ToArray();
                    for (int i = 0; i < arrSql.Count(); i++)
                    {
                        await _financeUnitOfWork.DbConnection.ExecuteAsync(arrSql[i], arrpParam[i], _financeUnitOfWork.DbTransaction);
                    }
                    //提交事务 
                    _financeUnitOfWork.DbTransaction.Commit();
                    //订单完成操作，释放订单
                    await _redis.DelAsync(orderIdKey);
                    await _redis.DelAsync(userIdKey);
                    return new FreeszeAmountDto { OrderId = dto.OrderId, Success = true, RefuseContent = "", BonusOrderId = dto.BonusOrderId };
                }
                catch (Exception ex)
                {

                    //回滚事务
                    _financeUnitOfWork.Rollback();
                    _logger.LogError(new EventId(0), ex,ex.Message);
                    return new FreeszeAmountDto { OrderId = dto.OrderId, Success = false, RefuseContent = "事务执行失败", BonusOrderId = dto.BonusOrderId };
                }
          

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                //操作异常，释放订单
                await _redis.DelAsync(orderIdKey);
                await _redis.DelAsync(userIdKey);
                return new FreeszeAmountDto { OrderId = dto.OrderId, Success = false, RefuseContent = ex.Message, BonusOrderId = dto.BonusOrderId };
            }
        }
    }
}
