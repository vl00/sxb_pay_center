using Dapper;
using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Appliaction.Http;
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
using static iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw.VerifyRecordListResult;

namespace iSchool.FinanceCenter.Appliaction.Service.Withdraw
{
    /// <summary>
    /// 审批记录控制器
    /// </summary>
    public class VerifyRecordListHandler : IRequestHandler<QueryVerifyRecordReqDto, PagedList<ApprovalRecord>>
    {
        private readonly IRepository<ApprovalRecord> _recordRep;
        private readonly IInsideHttpRepository _insideHttpRepository;

        /// <summary>
        /// 审批记录控制器构造函数
        /// </summary>
        /// <param name="insideHttpRepository"></param>
        /// <param name="recordRep"></param>
        public VerifyRecordListHandler(IInsideHttpRepository insideHttpRepository, IRepository<ApprovalRecord> recordRep)
        {
            _recordRep = recordRep;
            _insideHttpRepository = insideHttpRepository;
        }


        /// <summary>
        ///  审批记录
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<PagedList<ApprovalRecord>> Handle(QueryVerifyRecordReqDto dto, CancellationToken cancellationToken)
        {
            var userIds = new List<Guid>();
            if (!string.IsNullOrWhiteSpace(dto?.IdNamePhone))
            {
                userIds = await _insideHttpRepository.GetUserIds(dto?.IdNamePhone);
                if (userIds.Count() == 0) { return new List<ApprovalRecord>().ToPagedList(0, 0, 0); }
            }
            var sqlCount = $@"SELECT COUNT(1)
                        FROM Withdraw AS wd WHERE 1=1 ";
            var sql = $@"SELECT UserId, CreateTime AS ApplyTime, WithdrawAmount AS ApplyAmount, VerifyTime AS ApprovalTime, VerifyUserId, PaymentNo AS No FROM Withdraw WHERE 1 = 1 ";
            var whereSql = "";
            var param = new DynamicParameters();
            if (null != dto?.ApprovalStatus)
            {
                whereSql += "AND WithdrawStatus = @ApprovalStatus ";
                param.Add("ApprovalStatus", dto.ApprovalStatus);
            }
            if (null != dto?.ArrivalStatus)
            {
                whereSql += "AND PayStatus = @ArrivalStatus ";
                param.Add("ArrivalStatus", dto.ArrivalStatus);
            }
            if (userIds.Count() != 0)
            {
                whereSql += "AND UserId IN @UserId ";
                param.Add("@UserId", userIds);
            }
            if (null != dto?.ApprovalStartTime && null != dto?.ApprovalEndTime)
            {
                whereSql += "AND UpdateTime BETWEEN @ApprovalStartTime AND @ApprovalEndTime ";
                param.Add("@ApprovalStartTime", dto.ApprovalStartTime);
                param.Add("@ApprovalEndTime", dto.ApprovalEndTime);
            }
            if (null != dto?.ApplyStartTime && null != dto?.ApplyEndTime)
            {
                whereSql += "AND CreateTime BETWEEN @ApplyStartTime AND @ApplyEndTime ";
                param.Add("@ApplyStartTime", dto.ApplyStartTime);
                param.Add("@ApplyEndTime", dto.ApplyEndTime);
            }
            var pageSql = " ORDER BY UpdateTime DESC OFFSET @PageIndex ROWS FETCH NEXT @PageSize ROWS ONLY ";
            if (dto.PageSize == -1) { pageSql = " ORDER BY UpdateTime DESC "; }
            param.Add("@PageIndex", dto.PageSize * (dto.PageIndex - 1));
            param.Add("@PageSize", dto.PageSize);
            var totalSize = await _recordRep.QueryCount(sqlCount + whereSql, param);
            var res = _recordRep.Query(sql + whereSql + pageSql, param).ToList();
            var result = new List<ApprovalRecord>();
            if (res?.Count() == 0) {return result.ToPagedList(dto.PageSize, dto.PageIndex, totalSize); }
            var ids = new List<Guid>();
            foreach (var item in res)
            {
                ids.Add(item.UserId);
                
            }
            var verifyIds = new List<Guid>();
            foreach (var item in res)
            {
                verifyIds.Add(item.VerifyUserId);
            }
            var verifyNames = AdminInfoUtil.GetNames(verifyIds);
            var userInfos = await _insideHttpRepository.GetUsers(ids);
            foreach (var item in res)
            {
                var r = item;
                foreach (var user in userInfos.Where(user => user.Id == item.UserId))
                {
                    r.UserName = user.UserName;
                    r.Phone = user.UserPhone;
                }
                foreach (var user in verifyNames.Where(user => user.Key == item.VerifyUserId))
                {
                    r.VerifyName = user.Value;
                }
                result.Add(r);
            }
            var data = result.ToPagedList(dto.PageSize, dto.PageIndex, totalSize);
            return data;
        }
    }
}
