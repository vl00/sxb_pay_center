using CSRedis;
using Dapper;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.PayOrder;
using iSchool.FinanceCenter.Appliaction.RequestDto.Statement;
using iSchool.FinanceCenter.Appliaction.RequestDto.Wallet;
using iSchool.FinanceCenter.Appliaction.Service.Statement;
using iSchool.FinanceCenter.Domain;
using iSchool.FinanceCenter.Domain.Entities;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.FinanceCenter.Domain.Redis;
using iSchool.Infrastructure;
using iSchool.Infrastructure.UoW;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    /// <summary>
    /// 企业发放到钱包
    /// </summary>
    public class CompanyWallletOperateHandler : IRequestHandler<CompanyWallletOperateCommand, bool>
    {
        private readonly IRepository<Domain.Entities.Wallet> _walletRepo;
        private readonly IRepository<Domain.Entities.PayOrder> repository;
        private readonly IRepository<Domain.Entities.PayLog> _payLogRepository;
        private readonly CSRedisClient _redisClient;
        private readonly ISxbGenerateNo _sxbGenerateNo;
        private readonly FinanceCenterUnitOfWork _financeUnitOfWork;
        private readonly IMediator _mediator;
        private readonly ILogger<CompanyWallletOperateHandler> _logger;
        public CompanyWallletOperateHandler(ILogger<CompanyWallletOperateHandler> logger, IMediator mediator, CSRedisClient redisClient, IRepository<Domain.Entities.PayLog> payLogRepository, IRepository<Domain.Entities.PayOrder> repository, IWeChatPayClient client, ISxbGenerateNo sxbGenerateNo, IFinanceCenterUnitOfWork financeUnitOfWork, IRepository<Domain.Entities.Wallet> walletRepo)
        {
            this.repository = repository;
            _payLogRepository = payLogRepository;
            _redisClient = redisClient;
            _sxbGenerateNo = sxbGenerateNo;
            this._financeUnitOfWork = (FinanceCenterUnitOfWork)financeUnitOfWork;
            _walletRepo = walletRepo;
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> Handle(CompanyWallletOperateCommand cmd, CancellationToken cancellationToken)
        {
            var amount = cmd.Amount + cmd.BlockedAmount;
            if (amount <= 0)
            {
                throw new CustomResponseException("金额参数有误");
            }
            if (Guid.Empty == cmd.OrderId)
            {
                throw new CustomResponseException("参数缺失，非法操作");
            }
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();

            try
            {   //新增公司支付订单
                var orderId = Guid.NewGuid();
                var orderNo = $"FNC{_sxbGenerateNo.GetNumber()}";
                var payOrderSlq = AddPayOrder(cmd, amount, orderId, orderNo);
                sqlBase.Sqls.AddRange(payOrderSlq.Sqls);
                sqlBase.SqlParams.AddRange(payOrderSlq.SqlParams);
                //添加公司流水
                var stateMentSql = AddCompanyCashLog(amount, orderId, cmd.OrderType);
                sqlBase.Sqls.AddRange(stateMentSql.Sqls);
                sqlBase.SqlParams.AddRange(stateMentSql.SqlParams);
                _financeUnitOfWork.BeginTransaction();
                //用户钱包入账
                var cmdWallet = new OperateWalletDto() { CompanyOperate = true, UserId = cmd.ToUserId, VirtualAmount = cmd.VirtualAmount, Amount = cmd.Amount, BlockedAmount = cmd.BlockedAmount, Io = StatementIoEnum.In, StatementType = StatementTypeEnum.Blocked, OrderId = orderId, Remark = cmd.Remark, OrderType = cmd.OrderType, OrderDetailId = cmd.OrderDetailId };
                var walletSql = await _mediator.Send(cmdWallet);
                sqlBase.Sqls.AddRange(walletSql.Sqls);
                sqlBase.SqlParams.AddRange(walletSql.SqlParams);
                var arrSql = sqlBase.Sqls.ToArray();
                var arrpParam = sqlBase.SqlParams.ToArray();
                for (int i = 0; i < arrSql.Count(); i++)
                {
                    await _financeUnitOfWork.DbConnection.ExecuteAsync(arrSql[i], arrpParam[i], _financeUnitOfWork.DbTransaction);
                }
                //提交事务 
                _financeUnitOfWork.DbTransaction.Commit();


            }
            catch (Exception ex)
            {

                //回滚事务
                _financeUnitOfWork.Rollback();
                _logger.LogError(new EventId(0), ex, ex.Message);
                throw ex;
            }



            return true;


        }





        private SqlBase AddPayOrder(CompanyWallletOperateCommand dto, decimal amount, Guid orderId, string orderNo)
        {

            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();

            var sql = @"INSERT INTO [dbo].[payOrder] (id,userId,orderId,orderType,orderStatus,totalAmount,payAmount,createTime,updateTime,remark,System,OrderNo)
            VALUES (@id,@userId,@orderId,@orderType,@orderStatus,@totalAmount,@payAmount,@createTime,@updateTime,@remark,@System,@OrderNo)";
            var param = new
            {
                id = orderId,
                userId = Guid.Parse(ConfigHelper.GetConfigString("CompanyUserId")),
                orderType = dto.OrderType,
                orderId = dto.OrderId,
                orderStatus = OrderStatus.PaySucess,
                totalAmount = amount,
                payAmount = amount,
                createTime = DateTime.Now,
                updateTime = DateTime.Now,
                remark = dto.Remark,
                System = OrderSystem.Org,
                OrderNo = orderNo,
            };
            sqlBase.Sqls.Add(sql);
            sqlBase.SqlParams.Add(param);
            return sqlBase;
        }

        private SqlBase AddCompanyCashLog(decimal amount, Guid orderid, OrderTypeEnum ordertype)
        {
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            #region 公司流水添加
            var addStatmentRequest = new AddStatementDto()
            {
                UserId = Guid.Parse(ConfigHelper.GetConfigString("CompanyUserId")),
                Amount = amount,
                Io = StatementIoEnum.In,
                StatementType = StatementTypeEnum.Recharge,
                OrderId = orderid,
                OrderType = ordertype,
                Remark = "用户支付成功"

            };
            var addSql = AddStatementSql.AddStatement(addStatmentRequest);
            sqlBase.Sqls.Add(addSql.Sql);
            sqlBase.SqlParams.Add(addSql.SqlParam);
            var subStractStatmentRequest = new AddStatementDto()
            {
                UserId = Guid.Parse(ConfigHelper.GetConfigString("CompanyUserId")),
                Amount = amount,
                Io = StatementIoEnum.Out,
                StatementType = StatementTypeEnum.Outgoings,
                OrderId = orderid,
                OrderType = ordertype,
                Remark = "用户支付成功"

            };
            var subStractSql = AddStatementSql.AddStatement(subStractStatmentRequest);
            sqlBase.Sqls.Add(subStractSql.Sql);
            sqlBase.SqlParams.Add(subStractSql.SqlParam);
            #endregion

            return sqlBase;
        }


    }
}
