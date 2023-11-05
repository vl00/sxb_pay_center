using CSRedis;
using iSchool.Domain.Modles;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Statement;
using iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.Service.Statement;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Withdraw
{
    public class PayStatusSuccessHandler : IRequestHandler<PayStatusSuccessDto, bool>
    {
        private readonly IRepository<Domain.Entities.Withdraw> _repository;
        private readonly CSRedisClient _redis;
        public PayStatusSuccessHandler(IRepository<Domain.Entities.Withdraw> repository, CSRedisClient redis)
        {
            _repository = repository;
            _redis = redis;
        }
        /// <summary>
        /// 执行到账失败，重新支付用户账款到账后，
        /// 修改提现支付状态，并产生提现记录 
        /// </summary>
        /// <param name="payDto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> Handle(PayStatusSuccessDto payDto, CancellationToken cancellationToken)
        {
           //修改的结算订单是否存在
            var withdrawData = _repository.Query("SELECT ID, UserId, OpenId, WithdrawWay, WithdrawStatus, PayStatus, WithdrawNo, WithdrawAmount, RefuseContent, NickName, BankCardNo, VerifyUserId, VerifyTime, CreateTime, UpdateTime, PayTime FROM Withdraw WHERE WithdrawNo = @WithdrawNo  ", new { WithdrawNo = payDto.No })?.FirstOrDefault();
            if (null == withdrawData) throw new CustomResponseException("查无此结算订单");
            var withdrawNoLock = _redis.Lock(payDto.No, withdrawData.UserId, 60);
            if (!withdrawNoLock) throw new CustomResponseException("正在操作订单审核状态，请稍后");
            try
            {
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
                var sqlBase = new SqlBase();
                sqlBase.Sqls = new List<string>();
                sqlBase.SqlParams = new List<object>();
                var statementSql = AddStatementSql.AddStatement(statementDto);
                if (null == statementSql) throw new CustomResponseException("新增流水错误");
                sqlBase.Sqls.Add(statementSql.Sql);
                sqlBase.SqlParams.Add(statementSql.SqlParam);
                var dto = new UpdateWithdrawDto
                {
                    WithdrawNo = payDto.No,
                    VerifyUserId = withdrawData.VerifyUserId.Value,
                    RefuseContent = "",
                    WithdrawStatus=WithdrawStatusEnum.Pass,
                    PaymentNo=payDto.CompanyPayOrderNo
                };
                var withdrawSql = VerifyWithdrawHandler.UpdateWithdrawSql(dto, CompanyPayStatusEnum.Success);
                sqlBase.Sqls.Add(withdrawSql.Sql);
                sqlBase.SqlParams.Add(withdrawSql.SqlParam);
                var addWithdrawProcess = WithdrawProcessService.AddWithdrawProcess(withdrawData.WithdrawAmount, dto.VerifyUserId, dto.RefuseContent, dto.WithdrawNo, WithdrawStatusEnum.Pass, CompanyPayStatusEnum.Success);
                sqlBase.Sqls.Add(addWithdrawProcess.Sql);
                sqlBase.SqlParams.Add(addWithdrawProcess.SqlParam);
                var res = await _repository.Executes(sqlBase.Sqls, sqlBase.SqlParams);
                if (!res) { throw new CustomResponseException("到账状态修改失败"); }
                await _redis.DelAsync(payDto.No);
                return res;
            }
            catch (Exception ex)
            {
                await _redis.DelAsync(payDto.No);
                throw new CustomResponseException(ex.Message); ;
            }
        }

    }
}
