using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw
{
    /// <summary>
    /// 审批数据返回类
    /// </summary>
    public class VerifyListResult
    {
        /// <summary>
        /// 审核编号
        /// </summary>
        public string No { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public Guid UserId { get; set; }

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
        /// 剩余金额
        /// </summary>
        public decimal RemainAmount { get; set; }

        /// <summary>
        /// 累计提现金额
        /// </summary>
        public decimal WithdrawCountAmount { get; set; }

        /// <summary>
        /// 提现方式
        /// </summary>
        public WithdrawWayEnum WithdrawType { get; set; }

        /// <summary>
        /// 提现方式描述
        /// </summary>
        public string WithdrawTypeDes => WithdrawType.GetDescription();

        /// <summary>
        /// 审批时间
        /// </summary>
        public Nullable<DateTime> ApprovalTime { get; set; }


        /// <summary>
        /// 审批状态
        /// </summary>
        private WithdrawStatusEnum ApprovalStatus { get; set; }

        /// <summary>
        /// 审批状态描述
        /// </summary>
        public string ApprovalDes => ApprovalStatus.GetDescription();

        /// <summary>
        /// 到账状态
        /// </summary>
        private CompanyPayStatusEnum ArrivalStatus { get; set; }

        private string PayContent { get; set; }

        /// <summary>
        /// 到账状态描述
        /// </summary>
        public string ArrivalDes => CompanyPayStatusEnum.Fail == ArrivalStatus? $"{ArrivalStatus.GetDescription()}-{PayContent}": ArrivalStatus.GetDescription();

        /// <summary>
        /// 到账时间
        /// </summary>
        public Nullable<DateTime> ArrivalTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}
