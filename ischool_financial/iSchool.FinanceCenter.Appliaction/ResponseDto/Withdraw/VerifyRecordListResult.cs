using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure.Dapper;
using iSchool.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw
{
    /// <summary>
    /// 审批数据返回类
    /// </summary>
    public class VerifyRecordListResult
    {
        /// <summary>
        /// 当前审批金额
        /// </summary>
        public decimal ApprovalAmount { get; set; }

        /// <summary>
        /// 已支付总额
        /// </summary>
        public decimal PayCountAmount { get; set; }

        /// <summary>
        /// 审批数据
        /// </summary>
        ///public PagedList<ApprovalRecord> ApprovalRecords { get; set; }
    }
    /// <summary>
    /// 审批数据返回List数据集
    /// </summary>
    public class ApprovalRecord
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 审批人Id
        /// </summary>
        public Guid VerifyUserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 申请时间
        /// </summary>
        public DateTime ApplyTime { get; set; }


        /// <summary>
        /// 申请金额
        /// </summary>
        public decimal ApplyAmount { get; set; }

        /// <summary>
        /// 审批时间
        /// </summary>
        public Nullable<DateTime> ApprovalTime { get; set; }


        /// <summary>
        /// 审批人
        /// </summary>
        public string VerifyName { get; set; }

        /// <summary>
        /// 支付订单号
        /// </summary>
        public string No { get; set; }
    }
}
