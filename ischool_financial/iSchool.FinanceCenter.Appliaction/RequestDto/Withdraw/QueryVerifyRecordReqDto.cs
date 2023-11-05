using iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw;
using iSchool.Infrastructure.Dapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.Withdraw
{
    public class QueryVerifyRecordReqDto : IRequest<PagedList<ApprovalRecord>>
    {
        /// <summary>
        /// 第几页.不传默认为1
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 页大小.不传默认为10
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 审批状态
        /// </summary>
        public int? ApprovalStatus { get; set; }

        /// <summary>
        /// 到账状态
        /// </summary>
        public int? ArrivalStatus { get; set; }

        /// <summary>
        /// ID/用户名/手机号
        /// </summary>
        public string IdNamePhone { get; set; }

        /// <summary>
        /// 审批开始时间
        /// </summary>
        public DateTime? ApprovalStartTime { get; set; }

        /// <summary>
        /// 审批结束时间
        /// </summary>
        public DateTime? ApprovalEndTime { get; set; }

        /// <summary>
        /// 申请开始时间
        /// </summary>
        public DateTime? ApplyStartTime { get; set; }

        /// <summary>
        /// 申请结束时间
        /// </summary>
        public DateTime? ApplyEndTime { get; set; }
    }
}
