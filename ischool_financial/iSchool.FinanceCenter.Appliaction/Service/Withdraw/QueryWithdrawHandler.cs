using Dapper;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw;
using iSchool.Infrastructure.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Withdraw
{
    /// <summary>
    /// 查询结算控制器
    /// </summary>
    public class QueryWithdrawHandler : IRequestHandler<QueryWithdrawDto, List<Domain.Entities.Withdraw>>
    {
        private readonly IRepository<Domain.Entities.Withdraw> repository;

        /// <summary>
        /// 查询结算控制器构造函数
        /// </summary>
        /// <param name="repository"></param>
        public QueryWithdrawHandler(IRepository<Domain.Entities.Withdraw> repository)
        {
            this.repository = repository;
        }


        /// <summary>
        ///  查询结算
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<Domain.Entities.Withdraw>> Handle(QueryWithdrawDto dto, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var sql = @"SELECT ID, UserId, OpenId, WithdrawWay, WithdrawStatus, PayStatus, WithdrawNo, WithdrawAmount, RefuseContent, NickName, BankCardNo, VerifyUserId, VerifyTime, CreateTime, UpdateTime, PayTime FROM dbo.Withdraw WHERE 1=1 ";
            var param = new DynamicParameters();
            var sqlWhere = "";
            if (dto.UserId !=  Guid.Empty)
            {
                sqlWhere += "AND UserId = @UserId ";
                param.Add("UserId",dto.UserId);
            }
            if (null != dto.WithdrawWay)
            {
                sqlWhere += "AND WithdrawWay = @WithdrawWay ";
                param.Add("WithdrawWay", dto.WithdrawWay);
            }
            if (!string.IsNullOrEmpty(dto.WithdrawNo))
            {
                sqlWhere += "AND WithdrawNo = @WithdrawNo ";
                param.Add("WithdrawNo", dto.WithdrawNo);
            }
            if (dto.VerifyUserId != Guid.Empty)
            {
                sqlWhere += "AND VerifyUserId = @VerifyUserId ";
                param.Add("VerifyUserId", dto.VerifyUserId);
            }
            return repository.Query(sql + sqlWhere, param).ToList();
        }
    }
}
