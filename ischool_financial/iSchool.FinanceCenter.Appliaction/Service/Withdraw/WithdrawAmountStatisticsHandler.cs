using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Withdraw
{
    public class WithdrawAmountStatisticsHandler: IRequestHandler<AmountStatisticsDto, AmountStatisticsResult> 
    {
        private readonly IRepository<AmountStatisticsResult> _statisticsRep;

        public WithdrawAmountStatisticsHandler(IRepository<AmountStatisticsResult> statisticsRep)
        {
            _statisticsRep = statisticsRep;
        }

        public async Task<AmountStatisticsResult> Handle(AmountStatisticsDto dto, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var sql = @"SELECT a.ApplyAmount,b.PassAmount,c.ApplyCount FROM
                        (SELECT SUM(WithdrawAmount) AS ApplyAmount FROM Withdraw WHERE WithdrawStatus = 1) AS a,
                        (SELECT SUM(WithdrawAmount) AS PassAmount FROM Withdraw WHERE WithdrawStatus = 3 AND PayStatus = 1) AS b,
                        (SELECT COUNT(Id) AS ApplyCount FROM Withdraw WHERE WithdrawStatus = 1) AS c";
            var res = _statisticsRep.Query(sql,new { })?.ToList()?.FirstOrDefault();
            return res;
        }
    }
}
