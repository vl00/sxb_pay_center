using Dapper;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw;
using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure;
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
    /// 提现详情控制器
    /// </summary>
    public class WithdrawDetailHandler : IRequestHandler<DetailReqDto, WithdrawDetailResult>
    {
        private readonly IRepository<WithdrawDetailResult> _detailRep;
        private readonly IRepository<WithdrawState> _stateRep;
        /// <summary>
        /// 提现详情控制器构造函数
        /// </summary>
        /// <param name="detailRep"></param>
        /// <param name="stateRep"></param>
        public WithdrawDetailHandler(IRepository<WithdrawDetailResult> detailRep, IRepository<WithdrawState> stateRep)
        {
            _detailRep = detailRep;
            _stateRep = stateRep;
        }


        /// <summary>
        ///  提现详情
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<WithdrawDetailResult> Handle(DetailReqDto dto, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var sql = $@"SELECT WithdrawStatus,BankCardNo,WithdrawWay,WithdrawAmount AS Amount, CreateTime AS ApplyForTime,WithdrawNo AS No,PayStatus,PayTime,NickName 
                        FROM Withdraw WHERE UserId = @UserId AND WithdrawNo = @WithdrawNo ";
            var res = _detailRep.Query(sql, new { UserId = dto.UserId, WithdrawNo = dto.No }).ToList()?.FirstOrDefault();
            res.BankCardNo = StringHelper.BankCardHide(res.BankCardNo);
            if (null == res) throw new CustomResponseException("查无提现详情");
            sql = $@"SELECT WithdrawStatus, CreateTime AS Time,Remark,PayStatus FROM WithdrawProcess WHERE WithdrawNo = @WithdrawNo  ORDER BY CreateTime  ";
            var list = _stateRep.Query(sql, new { WithdrawNo = dto.No});
            res.State = list.ToList();
          
            return res;
        }
    }
}
