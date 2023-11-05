using Dapper;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Statement;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Statement;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Statement
{
    /// <summary>
    /// 查询流水控制器
    /// </summary>
    public class QueryStatementHandler : IRequestHandler<QueryStatementDto, List<QueryStatementResult>>
    {

        private readonly IRepository<QueryStatementResult> statementRepository;

        /// <summary>
        /// 查询流水控制器构造函数
        /// </summary>
        /// <param name="statementRepository"></param>
        public QueryStatementHandler(IRepository<QueryStatementResult> statementRepository)
        {
            this.statementRepository = statementRepository;
        }


        /// <summary>
        /// 查询个人流水统计
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<QueryStatementResult>> Handle(QueryStatementDto dto, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var sql = @"SELECT UserId,SUM(Amount) AS SumAmount FROM [iSchoolFinance].[dbo].[Statement] WHERE 1 = 1 ";
            var dynamicParameters = new DynamicParameters();
            var sqlWhere = "";
            if (null != dto.StartTime && null != dto.EndTime)
            {
                sqlWhere += "AND CreateTime BETWEEN @StartTime AND @EndTime ";
                dynamicParameters.Add("StartTime",dto.StartTime);
                dynamicParameters.Add("EndTime", dto.EndTime);
            }
            if (dto.UserId != Guid.Empty)
            {
                sqlWhere += "AND UserId = @UserId ";
                dynamicParameters.Add("UserId", dto.UserId);
            }
            var data = statementRepository.Query(sql+ sqlWhere+ "GROUP BY UserId", dynamicParameters);
            return data.AsList();
        }
    }
}
