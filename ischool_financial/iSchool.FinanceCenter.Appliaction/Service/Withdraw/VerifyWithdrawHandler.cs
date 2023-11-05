using CSRedis;
using Dapper;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.Http;
using iSchool.FinanceCenter.Appliaction.RequestDto.Statement;
using iSchool.FinanceCenter.Appliaction.RequestDto.Wallet;
using iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.ResponseDto.PayOrder;
using iSchool.FinanceCenter.Appliaction.Service.CompanyPay;
using iSchool.FinanceCenter.Appliaction.Service.Statement;
using iSchool.FinanceCenter.Appliaction.Service.Wallet;
using iSchool.FinanceCenter.Appliaction.Service.WechatTemplateMsg;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.FinanceCenter.Domain.Redis;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Common;
using iSchool.Infrastructure.Extensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Withdraw
{
    /// <summary>
    /// 结算申请状态变更控制器
    /// </summary>
    public class VerifyWithdrawHandler : IRequestHandler<UpdateWithdrawDto, bool>
    {
        private readonly IRepository<Domain.Entities.Withdraw> _repository;
        private readonly CSRedisClient _redis;
        private readonly IRepository<Domain.Entities.Wallet> _walletRep;
        private readonly IRepository<CheckOrderResult> _payOrderRepository;
        private readonly IMediator _mediator;
        private readonly IInsideHttpRepository _insideHttpRepo;

        /// <summary>
        /// 结算申请状态变更控制器构造函数
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="redis"></param>
        /// <param name="payOrderRepository"></param>
        /// <param name="walletRep"></param>
        /// <param name="mediator"></param>
        public VerifyWithdrawHandler(IInsideHttpRepository insideHttpRepo, IRepository<Domain.Entities.Withdraw> repository, CSRedisClient redis, IRepository<CheckOrderResult> payOrderRepository, IRepository<Domain.Entities.Wallet> walletRep, IMediator mediator)
        {
            _insideHttpRepo = insideHttpRepo;
            _repository = repository;
            _redis = redis;
            _payOrderRepository = payOrderRepository;
            _walletRep = walletRep;
            _mediator = mediator;
        }


        /// <summary>
        ///  结算申请状态变更
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> Handle(UpdateWithdrawDto dto, CancellationToken cancellationToken)
        {
            if (dto.WithdrawStatus == WithdrawStatusEnum.Apply) { throw new CustomResponseException("审核状态不能修改为待审核"); }
            if (dto.WithdrawStatus == WithdrawStatusEnum.SyncThirdParty) { throw new CustomResponseException("此审核是第三方审核。你没有权限"); }


            //防止重复调用
            var fightRepeatKey = $"VerifyWithdraw_{dto.WithdrawNo}";
            var going =  _redis.Get<int>(fightRepeatKey);
            if (0 == going)
            {
                 _redis.Set(fightRepeatKey, 1, TimeSpan.FromSeconds(8));
                //修改的结算订单是否存在
                var withdrawData = _repository.Query(@"SELECT ID, UserId, OpenId, WithdrawWay, WithdrawStatus, PayStatus, WithdrawNo, WithdrawAmount, RefuseContent, NickName, BankCardNo, VerifyUserId, VerifyTime, CreateTime, UpdateTime, PayTime,AppId FROM dbo.Withdraw WHERE WithdrawNo = @WithdrawNo  ", new { WithdrawNo = dto.WithdrawNo })?.FirstOrDefault();
                if (null == withdrawData) throw new CustomResponseException("查无此结算订单");
                //防并发，锁定订单，订单做唯一操作
                var withdrawNoKey = CacheKeys.WithdrawNo.FormatWith(dto.WithdrawNo);
                var withdrawNoLock = _redis.Lock(withdrawNoKey, withdrawData.UserId, 60);
                if (!withdrawNoLock) throw new CustomResponseException("正在操作订单审核状态，请稍后");

                var userIdKey = CacheKeys.WalletUserId.FormatWith(withdrawData.UserId.ToString());
                var userIdLock = _redis.Lock(userIdKey, withdrawData.UserId, 60);
                if (!userIdLock) throw new CustomResponseException("用户钱包锁定，请稍后");
                //只有是待审核状态订单才能操作
                if (withdrawData?.WithdrawStatus != (int)WithdrawStatusEnum.Apply) throw new CustomResponseException("提现订单已完成");
              
                try
                {
                    var sqlBase = new SqlBase();
                    sqlBase.Sqls = new List<string>();
                    sqlBase.SqlParams = new List<object>();

                    //结算成功，即刻修改钱包金额
                    var walletData = _walletRep.Query("SELECT UserId, TotalAmount, BlockedAmount, RemainAmount, UpdateTime, VirtualTotalAmount, VirtualRemainAmount, CheckSign FROM dbo.Wallet WHERE UserId = @UserId ",
                        new { UserId = withdrawData.UserId })?.FirstOrDefault();
                    if (null == walletData) throw new CustomResponseException("查找不到用户钱包");
                    //默认待支付
                    var payStatus = CompanyPayStatusEnum.Apply;
                    // 放入结算记录
                    var addWithdrawProcess = new SqlSingle();
                    //审核不通过产生流水，并把钱退回用户钱包
                    if (dto?.WithdrawStatus == WithdrawStatusEnum.Refuse)
                    {
                        var sqls = await Refuse(dto, withdrawData);
                        sqlBase.Sqls.AddRange(sqls.Sqls);
                        sqlBase.SqlParams.AddRange(sqls.SqlParams);
                        payStatus = CompanyPayStatusEnum.Apply;
                    }
                    //审核通过，只是更改提现的状态，并产生一次提现状态变化记录
                    if (dto?.WithdrawStatus == WithdrawStatusEnum.Pass)
                    {
                        addWithdrawProcess = WithdrawProcessService.AddWithdrawProcess(withdrawData.WithdrawAmount, dto.VerifyUserId, dto.RefuseContent, dto.WithdrawNo, dto.WithdrawStatus, CompanyPayStatusEnum.Payping);
                        if (null == addWithdrawProcess) throw new CustomResponseException("结算记录错误");
                        sqlBase.Sqls.Add(addWithdrawProcess.Sql);
                        sqlBase.SqlParams.Add(addWithdrawProcess.SqlParam);
                    }
                    //修改结算状态SQL
                    var updateWithdrawSql = UpdateWithdrawSql(dto, payStatus);
                    sqlBase.Sqls.Add(updateWithdrawSql.Sql);
                    sqlBase.SqlParams.Add(updateWithdrawSql.SqlParam);
                    var res = await _repository.Executes(sqlBase.Sqls, sqlBase.SqlParams);
                    if (!res) { throw new CustomResponseException("审核出错"); }
                    if (dto?.WithdrawStatus == WithdrawStatusEnum.Refuse)
                    {
                        var msgOpenId = withdrawData.OpenId;
                        //小程序兼容处理
                        if (!withdrawData.AppId.IsNullOrEmpty())
                        {
                            var listOpenIds = await _insideHttpRepo.GetUserOpenIds(new List<Guid>() { withdrawData.UserId });
                            var modelOpenId= listOpenIds.FirstOrDefault(x=>x.AppId== withdrawData.AppId);
                            if (null != modelOpenId)
                                msgOpenId = modelOpenId.OpenId;


                        }

                        #region 微信通知
                        await Task.Factory.StartNew(() =>
                        {
                            var msgReq = new WechatTemplateSendCommand()
                            {
                                OpenId = msgOpenId,
                                KeyWord1 = $"您申请提现（{withdrawData.WithdrawAmount.ToString("#0.00")}元）不通过,{dto.RefuseContent}",
                                KeyWord2 = DateTime.Now.ToDateTimeString(),
                                Remark = "点击更多查看详情",
                                MsyType = WechatMessageType.提现不通过通知,

                            };
                            _mediator.Send(msgReq);
                        });
                        #endregion

                    }

                    //订单完成操作，释放订单
                    await _redis.DelAsync(userIdKey);
                    //await _redis.DelAsync(orderIdKey);
                    await _redis.DelAsync(withdrawNoKey);
                    //执行到这里，判断若是拒绝通过，不再执行调用公司打款接口
                    if (dto?.WithdrawStatus == WithdrawStatusEnum.Refuse)
                    {
                        return true;
                    }
                    sqlBase = new SqlBase();
                    sqlBase.Sqls = new List<string>();
                    sqlBase.SqlParams = new List<object>();

                    //审核完成，调用公司打款接口，结算方式为微信提现，则需要调用微信打款功能
                    if (withdrawData.WithdrawWay == (int)WithdrawWayEnum.WeChat)
                    {
                        var wechatCompanyPay = new WechatCompanyPayCommand
                        {
                            Amount = withdrawData.WithdrawAmount,
                            OpenId = withdrawData.OpenId,
                            UserId = withdrawData.UserId,
                            WithDrawNo= dto.WithdrawNo,
                            AppId= withdrawData.AppId


                        };

                        var wechatPay = await _mediator.Send(wechatCompanyPay);
                        //返回失败
                        if (!wechatPay.OperateResult)
                        {
                            //支付状态修改为未到账
                            payStatus = CompanyPayStatusEnum.Fail;
                            //打款失败，状态充值为待审核
                            dto.WithdrawStatus = WithdrawStatusEnum.Apply;
                            dto.PayContent = wechatPay.AapplyDesc;
                            updateWithdrawSql = UpdateWithdrawSql(dto, payStatus);
                            sqlBase.Sqls.Add(updateWithdrawSql.Sql);
                            sqlBase.SqlParams.Add(updateWithdrawSql.SqlParam);
                            //记录订单变化过程
                            addWithdrawProcess = WithdrawProcessService.AddWithdrawProcess(withdrawData.WithdrawAmount, dto.VerifyUserId, dto.RefuseContent, dto.WithdrawNo, dto.WithdrawStatus, payStatus);
                            sqlBase.Sqls.Add(addWithdrawProcess.Sql);
                            sqlBase.SqlParams.Add(addWithdrawProcess.SqlParam);
                            res = await _repository.Executes(sqlBase.Sqls, sqlBase.SqlParams);
                            if (!res) { throw new CustomResponseException("支付状态修或审核记录出错"); }
                            throw new CustomResponseException($"公司打款失败：{wechatPay.AapplyDesc}");
                        }

                        //返回成功
                        //修改审核记录的支付单号
                        dto.PaymentNo = wechatPay.CompanyPayOrderId;
                        res = await AmountToAccount(withdrawData, dto);
                    }
                    else if (withdrawData.WithdrawWay == (int)WithdrawWayEnum.BankCard)
                    {
                        //线下打款，默认返回成功
                        res = await AmountToAccount(withdrawData, dto);
                        if (res)
                        {
                            #region 微信通知
                            var msgReq = new WechatTemplateSendCommand()
                            {
                                OpenId = withdrawData.OpenId,
                                KeyWord1 = $"您申请提现（{withdrawData.WithdrawAmount.ToString("#0.00")}元）已到银行卡",
                                KeyWord2 = DateTime.Now.ToDateTimeString(),
                                Remark = "点击更多查看详情",
                                MsyType = WechatMessageType.提现到账通知,

                            };
                            await _mediator.Send(msgReq);
                            #endregion
                        }
                    }
                    else
                    {
                        throw new CustomResponseException("没有此类操作");
                    }
                    return res;
                    #region 注释
                    //返回成功
                    ////提现已到账，产生流水和审核记录
                    //var statementDto = new AddStatementDto
                    //{
                    //    UserId = withdrawData.UserId,
                    //    Amount = withdrawData.WithdrawAmount,
                    //    StatementType = StatementTypeEnum.Settlement,
                    //    Io = StatementIoEnum.Out,
                    //    OrderId = withdrawData.ID,
                    //    OrderType = OrderTypeEnum.Withdraw,
                    //    Remark = "提现成功",
                    //};
                    //var statementSql = AddStatementSql.AddStatement(statementDto);
                    //if (null == statementSql) throw new CustomResponseException("新增流水错误");
                    //sqlBase.Sqls.Add(statementSql.Sql);
                    //sqlBase.SqlParams.Add(statementSql.SqlParam);
                    ////记录订单变化过程
                    //addWithdrawProcess = WithdrawProcessService.AddWithdrawProcess(withdrawData.WithdrawAmount, dto.VerifyUserId, dto.RefuseContent, dto.WithdrawNo, dto.WithdrawStatus, CompanyPayStatusEnum.Success);
                    //sqlBase.Sqls.Add(addWithdrawProcess.Sql);
                    //sqlBase.SqlParams.Add(addWithdrawProcess.SqlParam);
                    ////支付状态修改为已到账
                    //payStatus = CompanyPayStatusEnum.Success;
                    //updateWithdrawSql = UpdateWithdrawSql(dto, payStatus);
                    //sqlBase.Sqls.Add(updateWithdrawSql.Sql);
                    //sqlBase.SqlParams.Add(updateWithdrawSql.SqlParam);
                    ////提现到账失败，程序自动补全
                    //res = await _repository.Executes(sqlBase.Sqls, sqlBase.SqlParams);
                    //if (!res) { throw new CustomResponseException("产生流水和审核记录出错"); }

                    //return res;
                    #endregion
                }
                catch (Exception ex)
                {
                    //操作异常，释放订单
                    await _redis.DelAsync(userIdKey);
                    //await _redis.DelAsync(orderIdKey);
                    await _redis.DelAsync(withdrawNoKey);
                    throw new CustomResponseException(ex.Message);
                }
            }
            return false;


        }

        /// <summary>
        /// 打款成功到账后执行的代码
        /// </summary>
        /// <param name="withdrawData"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        private async Task<bool> AmountToAccount(Domain.Entities.Withdraw withdrawData, UpdateWithdrawDto dto)
        {
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            //提现已到账，产生流水和审核记录
            var statementDto = new AddStatementDto
            {
                UserId = withdrawData.UserId,
                Amount = withdrawData.WithdrawAmount,
                StatementType = StatementTypeEnum.Settlement,
                Io = StatementIoEnum.Out,
                OrderId = withdrawData.ID,
                OrderType = OrderTypeEnum.Withdraw,
                Remark = "提现成功",
            };
            var statementSql = AddStatementSql.AddStatement(statementDto);
            if (null == statementSql) throw new CustomResponseException("新增流水错误");
            sqlBase.Sqls.Add(statementSql.Sql);
            sqlBase.SqlParams.Add(statementSql.SqlParam);
            //记录订单变化过程
            var addWithdrawProcess = WithdrawProcessService.AddWithdrawProcess(withdrawData.WithdrawAmount, dto.VerifyUserId, dto.RefuseContent, dto.WithdrawNo, dto.WithdrawStatus, CompanyPayStatusEnum.Success);
            sqlBase.Sqls.Add(addWithdrawProcess.Sql);
            sqlBase.SqlParams.Add(addWithdrawProcess.SqlParam);
            //支付状态修改为已到账
            var payStatus = CompanyPayStatusEnum.Success;
            var updateWithdrawSql = UpdateWithdrawSql(dto, payStatus);
            sqlBase.Sqls.Add(updateWithdrawSql.Sql);
            sqlBase.SqlParams.Add(updateWithdrawSql.SqlParam);
          



            //提现到账失败，程序自动补全
            var res = await _repository.Executes(sqlBase.Sqls, sqlBase.SqlParams);
            if (!res) { throw new CustomResponseException("产生流水和审核记录出错"); }
            return res;
        }

        /// <summary>
        /// 审核不通过
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="withdrawData"></param>
        /// <returns></returns>
        private async Task<SqlBase> Refuse(UpdateWithdrawDto dto, Domain.Entities.Withdraw withdrawData)
        {
            var model = new OperateWalletDto
            {
                UserId = withdrawData.UserId,
                Amount = withdrawData.WithdrawAmount,
                StatementType = StatementTypeEnum.Settlement,
                Io = StatementIoEnum.Out,
                OrderId = withdrawData.ID,
                OrderType = OrderTypeEnum.Withdraw,
                Remark = string.IsNullOrWhiteSpace(dto.RefuseContent) ? "审核不通过" : dto.RefuseContent,
                WithdrawStatus = WithdrawStatusEnum.Refuse,
            };
            var resData = await _mediator.Send(model);
            if (null == resData) throw new CustomResponseException("变动钱包错误");
            var sqlBase = new SqlBase();
            sqlBase.Sqls = new List<string>();
            sqlBase.SqlParams = new List<object>();
            sqlBase.Sqls.AddRange(resData.Sqls);
            sqlBase.SqlParams.AddRange(resData.SqlParams);
            var addWithdrawProcess = WithdrawProcessService.AddWithdrawProcess(withdrawData.WithdrawAmount, dto.VerifyUserId, dto.RefuseContent, dto.WithdrawNo, dto.WithdrawStatus, CompanyPayStatusEnum.Apply);
            sqlBase.Sqls.Add(addWithdrawProcess.Sql);
            sqlBase.SqlParams.Add(addWithdrawProcess.SqlParam);
            return sqlBase;
        }

        /// <summary>
        /// 修改结算状态SQL
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="payStatus"></param>
        /// <returns></returns>
        public static SqlSingle UpdateWithdrawSql(UpdateWithdrawDto dto, CompanyPayStatusEnum payStatus)
        {
            var param = new DynamicParameters();
            var sql = @"UPDATE dbo.Withdraw SET ";

            sql += "WithdrawStatus = @WithdrawStatus, ";
            param.Add("WithdrawStatus", dto.WithdrawStatus);
            sql += "PayStatus = @PayStatus, ";
            param.Add("PayStatus", payStatus);
            if (payStatus == CompanyPayStatusEnum.Success)
            {
                sql += "PayTime = @PayTime, ";
                param.Add("PayTime", DateTime.Now);
            }

            if (!string.IsNullOrEmpty(dto.RefuseContent))
            {
                sql += "PayContent = @PayContent,";
                param.Add("PayContent", dto.PayContent);
            }
            if (!string.IsNullOrEmpty(dto.RefuseContent))
            {
                sql += "RefuseContent = @RefuseContent,";
                param.Add("RefuseContent", dto.RefuseContent);
            }
            if (!string.IsNullOrEmpty(dto.PaymentNo))
            {
                sql += "PaymentNo = @PaymentNo,";
                param.Add("PaymentNo", dto.PaymentNo);
            }
            sql += "VerifyUserId = @VerifyUserId,";
            param.Add("VerifyUserId", dto.VerifyUserId);
            sql += "VerifyTime = @VerifyTime,";
            param.Add("VerifyTime", DateTime.Now);
            sql += "UpdateTime = @UpdateTime WHERE WithdrawNo = @WithdrawNo";
            param.Add("UpdateTime", DateTime.Now);
            param.Add("WithdrawNo", dto.WithdrawNo);
            var sqlSingle = new SqlSingle();
            sqlSingle.Sql = sql;
            sqlSingle.SqlParam = param;
            return sqlSingle;
        }
    }
}
