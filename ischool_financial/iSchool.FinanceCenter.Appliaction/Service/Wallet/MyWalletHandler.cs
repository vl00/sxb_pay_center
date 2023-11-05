using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Wallet;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Wallet;
using iSchool.Infrastructure;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Wallet
{
    /// <summary>
    /// 我的钱包控制器
    /// </summary>
    public class MyWalletHandler : IRequestHandler<MyWalletDto, MyWalletResult>
    {

        private readonly IRepository<MyWalletResult> _myWalletRep;

        /// <summary>
        /// 我的钱包控制器构造函数
        /// </summary>
        /// <param name="myWalletRep"></param>
        public MyWalletHandler(IRepository<MyWalletResult> myWalletRep)
        {
            _myWalletRep = myWalletRep;
        }

        /// <summary>
        ///  我的钱包
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<MyWalletResult> Handle(MyWalletDto dto, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var sql = $@"SELECT (TotalAmount+BlockedAmount) AS TotalIncomes,BlockedAmount AS WaitSettlementAmount,RemainAmount AS WithdrawalAmount FROM Wallet WHERE UserId = @UserId ";
            var result = _myWalletRep.Query(sql, new { UserId=dto.UserId }).FirstOrDefault();
            if (result == null)
            {
                var model = new OperateWalletDto
                {
                    Amount = 0,
                    BlockedAmount = 0,
                    UserId =dto.UserId,
                    VirtualAmount = 0,
                };
                //新增钱包
                var walletSql = WalletSql.AddWalletSql(model);
                if (null == walletSql) throw new CustomResponseException("添加用户钱包失败");
                await _myWalletRep.ExecuteAsync(walletSql.Sql, walletSql.SqlParam);
                result = _myWalletRep.Query(sql, new { UserId = dto.UserId }).FirstOrDefault();
            }
            var res = result;
            return res;
        }
    }
}
