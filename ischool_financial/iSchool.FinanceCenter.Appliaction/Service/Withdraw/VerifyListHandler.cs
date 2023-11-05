using Dapper;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.Http;
using iSchool.FinanceCenter.Appliaction.HttpDto;
using iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw;
using iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw;
using iSchool.Infrastructure;
using iSchool.Infrastructure.Common;
using iSchool.Infrastructure.Dapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProductManagement.Tool.HttpRequest;
using System.Net.Http;
using iSchool.FinanceCenter.Domain.Enum;

namespace iSchool.FinanceCenter.Appliaction.Service.Withdraw
{
    /// <summary>
    /// 提现审批控制器
    /// </summary>
    public class VerifyListHandler : IRequestHandler<QueryVerifyReqDto, PagedList<VerifyListResult>>
    {
        private readonly IRepository<VerifyListResult> repository;
        private readonly IInsideHttpRepository _insideHttpRepository;

        /// <summary>
        /// 提现审批控制器构造函数
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="insideHttpRepository"></param>
        public VerifyListHandler(IRepository<VerifyListResult> repository, IInsideHttpRepository insideHttpRepository)
        {
            this.repository = repository;
            _insideHttpRepository = insideHttpRepository;
        }


        /// <summary>
        ///  提现审批
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<PagedList<VerifyListResult>> Handle(QueryVerifyReqDto dto, CancellationToken cancellationToken)
        {
            var users = new List<UserInfo>();
            if (!string.IsNullOrWhiteSpace(dto?.Phone))
            {
                users = await _insideHttpRepository.GetUsersByPhone(dto.Phone);
                if (users.Count() == 0) { return new List<VerifyListResult>().ToPagedList(0, 0, 0); }
            }
            var sqlCount = $@"SELECT COUNT(1)
                        FROM Withdraw AS wd WHERE 1=1 ";
            var sql = $@"SELECT wd.WithdrawNo AS No,wd.UserId, wd.CreateTime AS ApplyTime, wd.WithdrawAmount AS ApplyAmount,wt.RemainAmount AS RemainAmount,
                        (SELECT SUM(WithdrawAmount) FROM Withdraw WHERE UserId = wd.UserId AND WithdrawStatus = 1) AS WithdrawCountAmount
                        ,wd.WithdrawWay AS WithdrawType,wd.UpdateTime AS ApprovalTime,wd.PayTime AS ArrivalTime,wd.WithdrawStatus AS ApprovalStatus ,wd.PayStatus AS ArrivalStatus, wd.RefuseContent AS Remark,wd.PayContent
                        FROM Withdraw AS wd
                        LEFT JOIN Wallet AS wt ON wt.UserId = wd.UserId WHERE 1=1 ";
            var whereSql = $" AND wd.WithdrawStatus<>{(int)WithdrawStatusEnum.SyncThirdParty}";
            var param = new DynamicParameters();
            if (null != dto?.ApprovalStatus) 
            {
                whereSql += "AND wd.WithdrawStatus = @ApprovalStatus ";
                param.Add("ApprovalStatus", dto.ApprovalStatus);
            }
            if (null != dto?.ArrivalStatus)
            {
                whereSql += "AND wd.PayStatus = @ArrivalStatus ";
                param.Add("ArrivalStatus", dto.ArrivalStatus);
            }
            if (users.Count() != 0)
            {
                whereSql += "AND wd.UserId IN @UserId ";
                var userIdList = new List<Guid>();
                foreach (var item in users)
                {
                    userIdList.Add(item.Id);
                }
                param.Add("@UserId", userIdList);
            }
            if (null != dto?.ArrivalStartTime && null != dto?.ArrivalEndTime)
            {
                whereSql += "AND wd.PayTime BETWEEN @ArrivalStartTime AND @ArrivalEndTime ";
                param.Add("@ArrivalStartTime", dto.ArrivalStartTime);
                param.Add("@ArrivalEndTime", dto.ArrivalEndTime);
            }
            if (null != dto?.ApprovalStartTime && null != dto?.ApprovalEndTime)
            {
                whereSql += "AND wd.UpdateTime BETWEEN @ApprovalStartTime AND @ApprovalEndTime ";
                param.Add("@ApprovalStartTime", dto.ApprovalStartTime);
                param.Add("@ApprovalEndTime", dto.ApprovalEndTime);
            }
            if (null != dto?.ApplyStartTime && null != dto?.ApplyEndTime)
            {
                whereSql += "AND wd.CreateTime BETWEEN @ApplyStartTime AND @ApplyEndTime ";
                param.Add("@ApplyStartTime", dto.ApplyStartTime);
                param.Add("@ApplyEndTime", dto.ApplyEndTime);
            }
            if ((int)dto.WithdrawWay != 0)
            {
                whereSql += "AND wd.WithdrawWay = @WithdrawWay ";
                param.Add("@WithdrawWay", dto.WithdrawWay);
            }
            var pageSql = " ORDER BY wd.UpdateTime DESC OFFSET @PageIndex ROWS FETCH NEXT @PageSize ROWS ONLY ";
            if (dto.PageSize == -1) { pageSql = " ORDER BY wd.UpdateTime DESC "; }
            param.Add("@PageIndex", dto.PageSize * (dto.PageIndex - 1));
            param.Add("@PageSize", dto.PageSize);
            var totalSize = await repository.QueryCount(sqlCount + whereSql, param);
            var res = repository.Query(sql + whereSql + pageSql, param).ToList();
            var result = new List<VerifyListResult>();
            if (res?.Count() == 0) { return result.ToPagedList(dto.PageSize, dto.PageIndex, totalSize); }
            var ids = (from item in res select item.UserId).ToList();
            var userInfos = await _insideHttpRepository.GetUsers(ids);
            
            foreach (var item in res)
            {
                var r = item;
                foreach (var user in userInfos.Where(user => user.Id == item.UserId))
                {
                    r.UserName = user.UserName;
                    r.Phone = user.UserPhone;
                }
                result.Add(r);
            }
            var data = result.ToPagedList(dto.PageSize, dto.PageIndex, totalSize);
            return data;
        }
    }

}
