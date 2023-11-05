using CSRedis;
using Dapper;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Statement;
using iSchool.FinanceCenter.Appliaction.RequestDto.Wallet;
using iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder;
using iSchool.FinanceCenter.Appliaction.Service.Statement;
using iSchool.FinanceCenter.Appliaction.Service.Withdraw;
using iSchool.FinanceCenter.Domain;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.FinanceCenter.Domain.Redis;
using iSchool.Infrastructure;
using iSchool.Infrastructure.UoW;
using MediatR;
using Newtonsoft.Json;
using NLog;
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
    /// 操作钱包控制器
    /// </summary>
    public class OperateWalletHandler : IRequestHandler<OperateWalletDto, SqlBase>
    {
        private readonly IRepository<Domain.Entities.Wallet> _repository;

        private readonly IRepository<CheckOrderResult> _payOrderRepository;

        private readonly CSRedisClient _redis;

        private readonly IMediator _mediator;


        private readonly ISxbGenerateNo _sxbGenerateNo;
        private readonly FinanceCenterUnitOfWork _financeUnitOfWork;
        /// <summary>
        /// 操作钱包控制器构造函数
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="payOrderRepository"></param>
        /// <param name="redis"></param>
        /// <param name="mediator"></param>
        /// <param name="sxbGenerateNo"></param>
        public OperateWalletHandler(IRepository<Domain.Entities.Wallet> repository, IFinanceCenterUnitOfWork financeUnitOfWork, IRepository<CheckOrderResult> payOrderRepository, CSRedisClient redis, IMediator mediator, ISxbGenerateNo sxbGenerateNo)
        {
            _repository = repository;
            _payOrderRepository = payOrderRepository;
            _redis = redis;
            _mediator = mediator;
            _sxbGenerateNo = sxbGenerateNo;
            this._financeUnitOfWork = (FinanceCenterUnitOfWork)financeUnitOfWork;
        }

        /// <summary>
        /// 钱包变动
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SqlBase> Handle(OperateWalletDto dto, CancellationToken cancellationToken)
        {
            return await WalletChange(dto);
        }


        /// <summary>
        /// 钱包变动事件
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<SqlBase> WalletChange(OperateWalletDto dto)
        {
            var sqlBase = new SqlBase();
            var finishKey = CacheKeys.FinishOrderId.FormatWith(dto.OrderId.ToString());
            if (Guid.Empty != dto.OrderId && null != dto.OrderId)
            {

                //防止刷（防止客户端密集调用接口）
                if (await _redis.ExistsAsync(finishKey)) throw new CustomResponseException("此订单已经完成操作");
            }

            try
            {
                //分步计算
                switch (dto.StatementType)
                {
                    //解冻
                    case StatementTypeEnum.Unfreeze:
                        sqlBase = Unfreeze(dto);
                        break;
                    //冻结
                    case StatementTypeEnum.Blocked:
                        sqlBase = await Blocked(dto, finishKey);
                        break;
                    //关闭
                    case StatementTypeEnum.Close:
                        sqlBase = Close(dto);
                        break;
                    //提现
                    case StatementTypeEnum.Settlement:
                        sqlBase = Settlement(dto);
                        break;
                    //扣费
                    case StatementTypeEnum.Deduct:
                        sqlBase = Deduct(dto);
                        break;
                    //收入
                    case StatementTypeEnum.Incomings:
                    //充值
                    case StatementTypeEnum.Recharge:
                        sqlBase = await Incomings(dto, finishKey);
                        break;
                    //支出
                    case StatementTypeEnum.Outgoings:
                        //操作金额为钱包剩余金额
                        sqlBase = await OperateRemainAmount(dto, finishKey);
                        break;
                    default:
                        break;
                }
                return sqlBase;
            }
            catch (Exception ex)
            {
                throw new CustomResponseException(ex.Message);
            }
        }

        /// <summary>
        /// 冻结
        /// 钱包冻结字段入账冻结金额
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="finishKey"></param>
        /// <returns></returns>
        private async Task<SqlBase> Blocked(OperateWalletDto dto, string finishKey)
        {
            if (dto.Amount > 0 && dto.BlockedAmount > 0) { throw new CustomResponseException("变动金额和冻结金额不能都大于0"); }
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            //用户钱包信息
            var data = UserWallet(dto.UserId);
            //没有冻结期的订单类似充值(不需要检验的充值)
            if (dto.BlockedAmount == 0)
            {
                //更改流水类型
                dto.StatementType = StatementTypeEnum.Incomings;
                //1、收入流水
                //钱包类型
                dto.Io = StatementIoEnum.In;
                var statement = AddStatement(dto, dto.Amount, StatementTypeEnum.Incomings);
                sqlBase.Sqls.Add(statement.Sql);
                sqlBase.SqlParams.Add(statement.SqlParam);
                //冻结钱包
                var walletSql = ModifyWalletSql(dto, data, dto.Amount, StatementIoEnum.In, dto.BlockedAmount);
                sqlBase.Sqls.AddRange(walletSql.Sqls);
                sqlBase.SqlParams.AddRange(walletSql.SqlParams);
                return sqlBase;
            }
            //只查询有冻结期订单
            var expendType = (int)OrderExpendTypeEnum.Blocked;
            //检查操作金额的合法性
            if (!dto.CompanyOperate) await CheckAmount(finishKey, dto.OrderId, dto.BlockedAmount, expendType);
            //1、收入流水
            //钱包类型
            dto.Io = StatementIoEnum.In;
            var statementSql = AddStatement(dto, dto.BlockedAmount, StatementTypeEnum.Incomings);
            sqlBase.Sqls.Add(statementSql.Sql);
            sqlBase.SqlParams.Add(statementSql.SqlParam);
            //2、冻结流水
            dto.Remark += "待结算";
            //钱包类型
            dto.Io = StatementIoEnum.Out;
            statementSql = AddStatement(dto, dto.BlockedAmount * -1, StatementTypeEnum.Blocked);
            sqlBase.Sqls.Add(statementSql.Sql);
            sqlBase.SqlParams.Add(statementSql.SqlParam);
            //更改钱包
            var wallet = ModifyWalletSql(dto, data, dto.Amount, StatementIoEnum.Out, dto.BlockedAmount);
            sqlBase.Sqls.AddRange(wallet.Sqls);
            sqlBase.SqlParams.AddRange(wallet.SqlParams);
            //操作为冻结金额时，增加订单入账金额
            if (!dto.CompanyOperate)
            {
                var checkOrderSql = AddCheckOrder(dto, expendType, dto.BlockedAmount);
                sqlBase.Sqls.Add(checkOrderSql.Sql);
                sqlBase.SqlParams.Add(checkOrderSql.SqlParam);
            }
            return sqlBase;
        }

        /// <summary>
        /// 解冻
        /// 钱包冻结字段扣去解冻金额
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private SqlBase Unfreeze(OperateWalletDto dto)
        {
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            //用户钱包信息
            var data = UserWallet(dto.UserId);
            if (dto.BlockedAmount > data.BlockedAmount) { throw new CustomResponseException("解冻金额大于冻结金额"); }
            //1、解冻流水
            //钱包类型
            dto.Io = StatementIoEnum.In;
            dto.Remark += "已结算";
            var statementSql = AddStatement(dto, dto.BlockedAmount, StatementTypeEnum.Unfreeze);
            sqlBase.Sqls.Add(statementSql.Sql);
            sqlBase.SqlParams.Add(statementSql.SqlParam);
            //钱包收入等于解冻金额
            dto.Amount = dto.BlockedAmount;
            //更改钱包
            var wallet = ModifyWalletSql(dto, data, dto.Amount, StatementIoEnum.In, dto.BlockedAmount * -1);
            sqlBase.Sqls.AddRange(wallet.Sqls);
            sqlBase.SqlParams.AddRange(wallet.SqlParams);
            return sqlBase;
        }

        /// <summary>
        /// 关闭
        /// 钱包冻结字段扣去解冻金额
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private SqlBase Close(OperateWalletDto dto)
        {
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            // 撤销冻结金额，增加一条金额为0得流水
            dto.Io = StatementIoEnum.Out;
            var statementSql = AddStatement(dto, 0, StatementTypeEnum.Close);
            sqlBase.Sqls.Add(statementSql.Sql);
            sqlBase.SqlParams.Add(statementSql.SqlParam);
            //用户钱包信息
            var data = UserWallet(dto.UserId);
            if (dto.BlockedAmount > data.BlockedAmount) { throw new CustomResponseException("撤销金额大于总待结算金额"); }
            //更改钱包，冻结金额扣除解冻金额
            var wallet = ModifyWalletSql(dto, data, dto.Amount, StatementIoEnum.Out, dto.BlockedAmount * -1);
            sqlBase.Sqls.AddRange(wallet.Sqls);
            sqlBase.SqlParams.AddRange(wallet.SqlParams);
            return sqlBase;
        }

        /// <summary>
        /// 扣费
        /// 扣除钱包剩余字段金额 
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private SqlBase Deduct(OperateWalletDto dto)
        {
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            //更改钱包
            var data = UserWallet(dto.UserId);
            if (dto.Amount > data.RemainAmount) { throw new CustomResponseException("扣费金额大于剩余金额"); }
            var wallet = ModifyWalletSql(dto, data, dto.Amount * -1, StatementIoEnum.Out, dto.BlockedAmount);
            sqlBase.Sqls.AddRange(wallet.Sqls);
            sqlBase.SqlParams.AddRange(wallet.SqlParams);
            //触动钱包必定增加流水SQL
            //钱包类型
            dto.Io = StatementIoEnum.Out;
            var statementSql = AddStatement(dto, dto.Amount * -1, dto.StatementType);
            sqlBase.Sqls.Add(statementSql.Sql);
            sqlBase.SqlParams.Add(statementSql.SqlParam);
            return sqlBase;
        }

        /// <summary>
        /// 提现分三种情况处理
        /// 申请   直接扣除钱包金额
        /// 通过   只是更改提现的状态
        /// 不通过 直接退回扣除金额到钱包
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        private SqlBase Settlement(OperateWalletDto dto)
        {
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            //更改钱包
            var data = UserWallet(dto.UserId);
            decimal amount = dto.Amount;
            //提现审核
            if (dto.WithdrawStatus == WithdrawStatusEnum.Apply)
            {
                amount = amount * -1;
                if (dto.Amount > data.RemainAmount) { throw new CustomResponseException("提现金额大于剩余金额"); }
            }
            var wallet = ModifyWalletSql(dto, data, amount, StatementIoEnum.Out, dto.BlockedAmount);
            sqlBase.Sqls.AddRange(wallet.Sqls);
            sqlBase.SqlParams.AddRange(wallet.SqlParams);
            return sqlBase;
        }

        /// <summary>
        /// 收入/充值
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="finishKey"></param>
        /// <returns></returns>
        private async Task<SqlBase> Incomings(OperateWalletDto dto, string finishKey)
        {
            var expendType = (int)OrderExpendTypeEnum.Remain;
            //检验订单金额
            await CheckAmount(finishKey, dto.OrderId, dto.Amount, expendType);
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            //更改钱包，前端传参一定是正数
            decimal blockedAmount = dto.BlockedAmount;
            decimal amount = dto.Amount;
            var data = UserWallet(dto.UserId);
            var wallet = ModifyWalletSql(dto, data, amount, dto.Io, blockedAmount);
            sqlBase.Sqls.AddRange(wallet.Sqls);
            sqlBase.SqlParams.AddRange(wallet.SqlParams);
            //触动钱包必定增加流水SQL
            var statementSql = AddStatement(dto, dto.Amount, dto.StatementType);
            sqlBase.Sqls.Add(statementSql.Sql);
            sqlBase.SqlParams.Add(statementSql.SqlParam);
            //操作金额为收入时，增加订单入账金额
            if (dto.Io == StatementIoEnum.In)
            {
                var checkOrderSql = AddCheckOrder(dto, expendType, dto.Amount);
                sqlBase.Sqls.Add(checkOrderSql.Sql);
                sqlBase.SqlParams.Add(checkOrderSql.SqlParam);
            }
            return sqlBase;
        }

        /// <summary>
        /// 操作金额为钱包剩余金额
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="finishKey"></param>
        /// <returns></returns>
        private async Task<SqlBase> OperateRemainAmount(OperateWalletDto dto, string finishKey)
        {
            try
            {
                var expendType = (int)OrderExpendTypeEnum.Remain;
                if (dto.Io == StatementIoEnum.In && dto.StatementType != StatementTypeEnum.Blocked)
                {
                    await CheckAmount(finishKey, dto.OrderId, dto.Amount, expendType);
                }
                var sqlBase = new SqlBase();
                sqlBase.Sqls = new List<string>();
                sqlBase.SqlParams = new List<object>();
                //更改钱包
                decimal blockedAmount = dto.BlockedAmount;
                if (dto.StatementType == StatementTypeEnum.Unfreeze || dto.StatementType == StatementTypeEnum.Close) { blockedAmount = blockedAmount * -1; }
                decimal amount = dto.Amount;
                var data = UserWallet(dto.UserId);
                if (data.RemainAmount < dto.Amount) throw new CustomResponseException("金额不能大于剩余金额");
                if (dto.Io == StatementIoEnum.Out) { amount = amount * -1; }
                var wallet = ModifyWalletSql(dto, data, amount, dto.Io, blockedAmount);
                sqlBase.Sqls.AddRange(wallet.Sqls);
                sqlBase.SqlParams.AddRange(wallet.SqlParams);
                //触动钱包必定增加流水SQL
                var statementSql = AddStatement(dto, dto.Io == StatementIoEnum.Out ? dto.Amount * -1 : dto.Amount, dto.StatementType);
                sqlBase.Sqls.Add(statementSql.Sql);
                sqlBase.SqlParams.Add(statementSql.SqlParam);

                //操作金额为收入时，增加订单入账金额
                if (dto.Io == StatementIoEnum.In)
                {
                    var checkOrderSql = AddCheckOrder(dto, expendType, dto.Amount);
                    sqlBase.Sqls.Add(checkOrderSql.Sql);
                    sqlBase.SqlParams.Add(checkOrderSql.SqlParam);
                }
                return sqlBase;
            }
            catch (Exception ex)
            {
                throw new CustomResponseException("异常：" + ex.Message);
            }
        }

        /// <summary>
        /// 用户钱包信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private Domain.Entities.Wallet UserWallet(Guid userId)
        {

            var data = _financeUnitOfWork.DbConnection.QueryFirstOrDefault<Domain.Entities.Wallet>("SELECT UserId, TotalAmount, BlockedAmount, RemainAmount, UpdateTime, VirtualTotalAmount, VirtualRemainAmount, CheckSign FROM dbo.Wallet WHERE UserId = @UserId ", new { UserId = userId }, _financeUnitOfWork.DbTransaction);
            return data;
        }

        /// <summary>
        /// 用户是否已经有钱包
        /// 有，修改钱包
        /// 没有，新增钱包
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="data"></param>
        /// <param name="amount"></param>
        /// <param name="statementIo"></param>
        /// <param name="blockedAmount"></param>
        /// <returns></returns>
        private SqlBase ModifyWalletSql(OperateWalletDto dto, Domain.Entities.Wallet data, decimal amount, StatementIoEnum statementIo, decimal blockedAmount)
        {
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            if (null == data)
            {
                //新增钱包
                var walletSql = WalletSql.AddWalletSql(dto);
                if (null == walletSql) throw new CustomResponseException("新增钱包错误");
                sqlBase.Sqls.Add(walletSql.Sql);
                sqlBase.SqlParams.Add(walletSql.SqlParam);
            }
            else
            {
                var walletSql = WalletSql.UpdateWalletSql(data, dto, amount, statementIo, blockedAmount);
                if (null == walletSql) throw new CustomResponseException("修改钱包错误");
                sqlBase.Sqls.Add(walletSql.Sql);
                sqlBase.SqlParams.Add(walletSql.SqlParam);
            }
            return sqlBase;
        }

        /// <summary>
        /// 检查操作金额的合法性
        /// </summary>
        /// <param name="finishKey"></param>
        /// <param name="orderId"></param>
        /// <param name="amount"></param>
        /// <param name="expendType"></param>
        /// <returns></returns>
        private async Task<bool> CheckAmount(string finishKey, Guid orderId, decimal amount, int expendType)
        {
            //检测订单的合法性
            var sqlCheck = @"SELECT a.SumOutAmount,b.TotalAmount FROM
                            (SELECT SUM(OutAmount) AS SumOutAmount FROM OrderWithdraw 
                            WHERE OrderId = @OrderId AND ExpendType = @ExpendType) AS a,
                            (SELECT TotalAmount FROM PayOrder 
                            WHERE OrderId = @OrderId AND OrderStatus = 6) AS b";
            var checkOrder = _payOrderRepository.Query(sqlCheck, new { OrderId = orderId, ExpendType = expendType })?.FirstOrDefault();
            //没有订单，订单不存在或冻结金额大于订单总金额
            //订单入账钱包金额  等于  订单总金额
            if (null == checkOrder || checkOrder?.SumOutAmount + amount > checkOrder?.TotalAmount)
            {
                await _redis.SetAsync(finishKey, checkOrder?.TotalAmount, 600);
                throw new CustomResponseException("订单不存在或金额大于订单总金额");
            }

            //防止刷，金额等于总金额，订单完成，设置redis key
            if (checkOrder?.SumOutAmount + amount == checkOrder?.TotalAmount)
            {
                await _redis.SetAsync(finishKey, checkOrder?.TotalAmount, 600);
            }
            return true;
        }

        /// <summary>
        /// 新增流水SQL
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="amount"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public SqlSingle AddStatement(OperateWalletDto dto, decimal amount, StatementTypeEnum type)
        {
            var statementDto = new AddStatementDto
            {
                UserId = dto.UserId,
                Amount = amount,
                StatementType = type,
                Io = dto.Io,
                OrderId = dto.OrderId,
                OrderType = dto.OrderType,
                Remark = dto.Remark,
                OrderDetailId = dto.OrderDetailId,
                FixTime = dto.FixTime
            };
            var statementSql = AddStatementSql.AddStatement(statementDto);
            return statementSql;
        }

        /// <summary>
        /// 新增订单结算金额SQL
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="expendType"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public SqlSingle AddCheckOrder(OperateWalletDto dto, int expendType, decimal amount)
        {
            //新增金额不能为负数
            if (amount < 0) return null;
            var sql = @"INSERT INTO dbo.OrderWithdraw (ID, No, OrderId, UserId, OrderType, ExpendType, OutAmount, CreateTime) VALUES (@id, @no, @orderid, @userid, @ordertype,@expendType, @outamount, @createtime)";
            var param = new
            {
                @id = Guid.NewGuid(),
                @no = "OWN" + _sxbGenerateNo.GetNumber(),
                @orderid = dto.OrderId,
                @userid = dto.UserId,
                @ordertype = dto.OrderType,
                @expendType = expendType,
                @outamount = amount,
                @createtime = DateTime.Now
            };
            var sqlSingle = new SqlSingle();
            sqlSingle.Sql = sql;
            sqlSingle.SqlParam = param;
            return sqlSingle;
        }

    }

    public static class WalletSql
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 新增钱包
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static SqlSingle AddWalletSql(OperateWalletDto dto)
        {
            if (dto.Amount < 0 || dto.BlockedAmount < 0)
            {
                throw new CustomResponseException("新增金额不能小于0");
            }
            var sqlSingle = new SqlSingle();
            var sql = @"INSERT INTO dbo.Wallet 
                        (userId,totalAmount,blockedAmount,remainAmount,updateTime,virtualTotalAmount,virtualRemainAmount,checkSign)
                        VALUES
                        (@userId,@totalAmount,@blockedAmount,@remainAmount,@updateTime,@virtualTotalAmount,@virtualRemainAmount,@checkSign)";
            sqlSingle.Sql = sql;
            var updateTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            var checkSignParams = CheckSignParams(dto, updateTime);
            var param = new { userId = dto.UserId, totalAmount = dto.Amount, blockedAmount = dto.BlockedAmount, remainAmount = dto.Amount, updateTime = updateTime, virtualTotalAmount = dto.VirtualAmount, virtualRemainAmount = dto.VirtualAmount, checkSign = checkSignParams };
            sqlSingle.SqlParam = param;
            return sqlSingle;
        }

        /// <summary>
        /// 修改钱包
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static SqlSingle UpdateWalletSqlOld(Domain.Entities.Wallet data, OperateWalletDto dto)
        {
            //CheckSign  验证不通过，不进行操作
            if (data.CheckSign != CheckSign(data)) throw new CustomResponseException("CheckSign验证不通过");
            var amount = dto.Amount;
            if (dto.StatementType == StatementTypeEnum.Deduct
                || dto.StatementType == StatementTypeEnum.Outgoings
                || dto.StatementType == StatementTypeEnum.ServiceFee
                || dto.StatementType == StatementTypeEnum.Settlement)
            {
                //审核拒绝不校验金额
                if (dto.WithdrawStatus != WithdrawStatusEnum.Refuse)
                {
                    //操作金额不能大于剩余金额
                    if (data.RemainAmount < (amount > 0 ? amount : amount * -1)) throw new CustomResponseException("金额不能大于剩余金额");
                }
            }
            //解冻金额不能大于冻结金额
            var blockedAmount = dto.BlockedAmount;
            if (dto.StatementType == StatementTypeEnum.Unfreeze)
            {
                if (data.BlockedAmount < (blockedAmount > 0 ? blockedAmount : blockedAmount * -1)) throw new CustomResponseException("解冻金额不能大于冻结金额");
            }
            var checkSignData = data;
            var sql = "UPDATE dbo.Wallet SET ";
            var param = new DynamicParameters();
            if (dto.Io == StatementIoEnum.Out) { amount = amount * -1; }
            //累计金额只记录收入金额
            if (dto.Io == StatementIoEnum.In)
            {
                sql += "TotalAmount = @TotalAmount,";
                param.Add("TotalAmount", data.TotalAmount + amount);
                checkSignData.TotalAmount = data.TotalAmount + amount;

                sql += "VirtualTotalAmount = @VirtualTotalAmount,";
                param.Add("VirtualTotalAmount", data.VirtualTotalAmount + dto.VirtualAmount);
                checkSignData.VirtualTotalAmount = data.VirtualTotalAmount + dto.VirtualAmount;
            }
            sql += "RemainAmount = @RemainAmount,";
            param.Add("RemainAmount", data.RemainAmount + amount);
            checkSignData.RemainAmount = data.RemainAmount + amount;

            if (dto.StatementType == StatementTypeEnum.Unfreeze || dto.StatementType == StatementTypeEnum.Close) { blockedAmount = blockedAmount * -1; }
            sql += "BlockedAmount = @BlockedAmount,";
            param.Add("BlockedAmount", data.BlockedAmount + blockedAmount);
            checkSignData.BlockedAmount = data.BlockedAmount + blockedAmount;

            sql += "VirtualRemainAmount = @VirtualRemainAmount,";
            param.Add("VirtualRemainAmount", data.VirtualRemainAmount + dto.VirtualAmount);
            checkSignData.VirtualRemainAmount = data.VirtualRemainAmount + dto.VirtualAmount;
            var updateTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            checkSignData.UpdateTime = updateTime;
            checkSignData.UserId = dto.UserId;
            sql += "CheckSign = @CheckSign,";
            //记录CheckSign MD5 前的json值
            var checkSign = CheckSign(checkSignData);
            param.Add("CheckSign", checkSign);
            sql += "UpdateTime = @UpdateTime WHERE UserId = @UserId";
            param.Add("UpdateTime", updateTime);
            param.Add("UserId", dto.UserId);
            var sqlSingle = new SqlSingle();
            sqlSingle.Sql = sql;
            sqlSingle.SqlParam = param;
            return sqlSingle;
        }


        public static SqlSingle UpdateWalletSql(Domain.Entities.Wallet data, OperateWalletDto dto, decimal amount, StatementIoEnum statementIo, decimal blockedAmount)
        {
            //CheckSign  验证不通过，不进行操作
            if (data.CheckSign != CheckSign(data)) throw new CustomResponseException("CheckSign验证不通过");
            var checkSignData = data;
            var sql = "UPDATE dbo.Wallet SET ";
            var param = new DynamicParameters();
            //累计金额只记录收入金额
            if (statementIo == StatementIoEnum.In)
            {
                sql += "TotalAmount += @TotalAmount,";
                param.Add("TotalAmount", amount);
                checkSignData.TotalAmount = data.TotalAmount + amount;

                sql += "VirtualTotalAmount += @VirtualTotalAmount,";
                param.Add("VirtualTotalAmount", dto.VirtualAmount);
                checkSignData.VirtualTotalAmount = data.VirtualTotalAmount + dto.VirtualAmount;
            }
            sql += "RemainAmount += @RemainAmount,";
            param.Add("RemainAmount", amount);
            checkSignData.RemainAmount = data.RemainAmount + amount;

            sql += "BlockedAmount += @BlockedAmount,";
            param.Add("BlockedAmount", blockedAmount);
            checkSignData.BlockedAmount = data.BlockedAmount + blockedAmount;

            sql += "VirtualRemainAmount += @VirtualRemainAmount,";
            param.Add("VirtualRemainAmount", dto.VirtualAmount);
            checkSignData.VirtualRemainAmount = data.VirtualRemainAmount + dto.VirtualAmount;
            var updateTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            checkSignData.UpdateTime = updateTime;
            checkSignData.UserId = dto.UserId;
            sql += "CheckSign = @CheckSign,";
            //记录CheckSign MD5 前的json值
            var checkSign = CheckSign(checkSignData);
            param.Add("CheckSign", checkSign);
            sql += "UpdateTime = @UpdateTime WHERE UserId = @UserId";
            param.Add("UpdateTime", updateTime);
            param.Add("UserId", dto.UserId);
            var sqlSingle = new SqlSingle();
            sqlSingle.Sql = sql;
            sqlSingle.SqlParam = param;
            return sqlSingle;
        }

        /// <summary>
        /// 预先冻结结算金额
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static SqlSingle BlockedAmountSql(decimal amount, Guid userId)
        {
            var param = new DynamicParameters();
            var sql = "UPDATE dbo.Wallet SET BlockedAmount = @BlockedAmount,";
            param.Add("BlockedAmount", amount);
            sql += "UpdateTime = @UpdateTime WHERE UserId = @UserId";
            param.Add("UpdateTime", Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            param.Add("UserId", userId);
            var sqlSingle = new SqlSingle();
            sqlSingle.Sql = sql;
            sqlSingle.SqlParam = param;
            return sqlSingle;
        }

        /// <summary>
        /// 查询数据生成用户信息认证密钥
        /// </summary>
        /// <param name="data"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string CheckSign(Domain.Entities.Wallet data)
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
            _logger.Info($"【查询数据生成密钥：{paramMd5} - {json}】");
            return paramMd5;
        }

        /// <summary>
        /// 传递参数生成用户信息认证密钥
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="updateTime"></param>
        /// <returns></returns>
        private static string CheckSignParams(OperateWalletDto dto, DateTime updateTime)
        {
            var paramJson = new
            {
                userId = dto.UserId,
                totalAmount = (int)(dto.Amount * 10000),
                blockedAmount = (int)(dto.BlockedAmount * 10000),
                remainAmount = (int)(dto.Amount * 10000),
                updateTime = updateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                virtualTotalAmount = (int)(dto.VirtualAmount * 10000),
                virtualRemainAmount = (int)(dto.VirtualAmount * 10000)
            };
            var json = JsonConvert.SerializeObject(paramJson).Trim();
            var paramMd5 = MD5.Compute(json).ToLowerInvariant();
            _logger.Info($"【查询数据生成密钥：{paramMd5} - {json}】");
            return paramMd5;
        }

    }
}
