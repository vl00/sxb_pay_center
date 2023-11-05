using Dapper;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw;
using iSchool.Infrastructure.Common;
using iSchool.Infrastructure.Dapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Appliaction.Service.Withdraw
{
    /// <summary>
    /// 提现记录控制器
    /// </summary>
    public class WithdrawListHandler : IRequestHandler<RecordsReqDto, PagedList<WithdrawRecordsResult>>
    {
        private readonly IRepository<WithdrawRecordsResult> repository;

        /// <summary>
        /// 提现记录控制器构造函数
        /// </summary>
        /// <param name="repository"></param>
        public WithdrawListHandler(IRepository<WithdrawRecordsResult> repository)
        {
            this.repository = repository;
        }


        /// <summary>
        ///  提现记录
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<PagedList<WithdrawRecordsResult>> Handle(RecordsReqDto dto, CancellationToken cancellationToken)
        {
            var sql = $@"SELECT NickName,BankCardNo,WithdrawWay,WithdrawAmount AS Amount,CreateTime AS ApplyForTime,WithdrawStatus,UpdateTime,PayStatus,PayTime,WithdrawNo AS No FROM Withdraw 
                        WHERE UserId = @UserId  ";
            var sqlCount = @"SELECT COUNT(1) FROM Withdraw WHERE UserId = @UserId  ";
            var sqlWhere = "";
            var param = new DynamicParameters();
            param.Add("UserId", dto.UserId);
            if (dto.SearchType != 0)
            {
                sqlWhere += "AND UpdateTime > @UpdateTime ";
                var date = DateTime.Now;
                if (dto.SearchType == 1) 
                {
                    param.Add("UpdateTime", date.ToString("yyyy-MM-dd")); 
                }
                else if (dto.SearchType == 2)
                {
                    param.Add("UpdateTime", date.AddDays(-7).ToString("yyyy-MM-dd"));
                }
                else if (dto.SearchType == 3)
                {
                    param.Add("UpdateTime", date.AddDays(-30).ToString("yyyy-MM-dd"));
                }
            }
            var pageSql = " ORDER BY UpdateTime DESC OFFSET @PageIndex ROWS FETCH NEXT @PageSize ROWS ONLY ";
            param.Add("@PageIndex", dto.PageSize * (dto.PageIndex - 1));
            param.Add("@PageSize", dto.PageSize);
            var totalSize = await repository.QueryCount(sqlCount + sqlWhere, param);
            var res = repository.Query(sql + sqlWhere + pageSql, param).ToList();
            var result = res.ToPagedList(dto.PageSize,dto.PageIndex, totalSize);
            return result;
        }
    }
}
