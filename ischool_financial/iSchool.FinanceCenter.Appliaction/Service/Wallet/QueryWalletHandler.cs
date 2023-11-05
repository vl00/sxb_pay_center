using Dapper;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Statement;
using iSchool.FinanceCenter.Appliaction.RequestDto.Wallet;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Wallet
{
    /// <summary>
    /// 查询钱包控制器
    /// </summary>
    public class QueryWalletHandler : IRequestHandler<QueryWalletDto, List<Domain.Entities.Wallet>>
    {
        private readonly IRepository<Domain.Entities.Wallet> repository;

        /// <summary>
        /// 查询钱包控制器构造函数
        /// </summary>
        /// <param name="repository"></param>
        public QueryWalletHandler(IRepository<Domain.Entities.Wallet> repository)
        {
            this.repository = repository;
        }


        /// <summary>
        ///  查询钱包
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<Domain.Entities.Wallet>> Handle(QueryWalletDto dto, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var sql = @"SELECT UserId, TotalAmount, BlockedAmount, RemainAmount, UpdateTime, VirtualTotalAmount, VirtualRemainAmount, CheckSign FROM dbo.wallet WHERE 1=1 ";
            var sqlWhere = "";
            var param = new DynamicParameters();
            if (dto.UserId != Guid.Empty)
            {
                sqlWhere += "AND UserId = @UserId ";
                param.Add("UserId",dto.UserId);
            }
            if (null != dto.StartTime && null != dto.EndTime)
            {
                sqlWhere += "UpdateTime BETWEEN @StartTime AND @EndTime ";
                param.Add("StartTime", dto.StartTime);
                param.Add("EndTime", dto.EndTime);
            }
            var res = repository.Query(sql + sqlWhere, param);
            return res.ToList();
        }
    }
}
