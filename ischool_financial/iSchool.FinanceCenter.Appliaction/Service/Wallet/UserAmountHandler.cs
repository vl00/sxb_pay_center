using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Wallet;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Wallet;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Wallet
{
    /// <summary>
    /// 
    /// </summary>
    public class UserAmountHandler : IRequestHandler<UserAmountDto, List<UserAmountResult>>
    {

        private readonly IRepository<UserAmountResult> _userAmountRep;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userAmountRep"></param>
        public UserAmountHandler(IRepository<UserAmountResult> userAmountRep)
        {
            _userAmountRep = userAmountRep;
        }

        /// <summary>
        ///  用户金额
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<UserAmountResult>> Handle(UserAmountDto dto, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var sql = $@"SELECT BlockedAmount, RemainAmount,UserId,
                        (SELECT SUM(WithdrawAmount) FROM Withdraw WHERE UserId = wt.UserId AND WithdrawStatus = 3) AS WithdrawAmount
                         FROM Wallet AS wt WHERE wt.UserId IN @UserIds ";
            var result = _userAmountRep.Query(sql, new { UserIds = dto.UserIds });
            return result?.ToList();
        }
    }
}
