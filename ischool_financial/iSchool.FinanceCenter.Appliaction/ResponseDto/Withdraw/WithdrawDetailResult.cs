using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw
{
    /// <summary>
    /// 提现记录
    /// </summary>
    public class WithdrawDetailResult
    {
        private WithdrawWayEnum WithdrawWay { get; set; }
        private WithdrawStatusEnum WithdrawStatus { get; set; }
        /// <summary>
        /// 提现名称
        /// </summary>
        public string Name => NickName + BankCardNo;

        /// <summary>
        /// 申请时间
        /// </summary>
        public DateTime ApplyForTime { get; set; }

        /// <summary>
        /// 支付状态
        /// </summary>
        private CompanyPayStatusEnum PayStatus { get; set; }

        private DateTime PayTime { get; set; }

        /// <summary>
        /// 到账时间
        /// </summary>
        public string ArrivedTime => CompanyPayStatusEnum.Success == PayStatus ? PayTime.ToString() : "";

        /// <summary>
        /// 提现金额
        /// </summary>
        public decimal Amount { get; set; }

        private string NickName { get; set; }
        public string BankCardNo { get; set; }

        /// <summary>
        /// 提现方式
        /// </summary>
        public WithdrawWayEnum Type => WithdrawWay;

        /// <summary>
        /// 提现账号
        /// </summary>
        public string No { get; set; }

        /// <summary>
        /// 状态变化
        /// </summary>
        public List<WithdrawState> State { get; set; }

        public WithdrawStatusEnum Status => WithdrawStatus;

    }

    /// <summary>
    /// 结算状态
    /// </summary>
    public class WithdrawState
    {
        private WithdrawStatusEnum WithdrawStatus { get; set; }

        private CompanyPayStatusEnum PayStatus { get; set; }

        /// <summary>
        /// 状态1
        /// </summary>
        public bool State => WithdrawStatus == WithdrawStatusEnum.Apply ? true :
            WithdrawStatus == WithdrawStatusEnum.Pass ? true :
            WithdrawStatus == WithdrawStatusEnum.Refuse ? false :
            PayStatus == CompanyPayStatusEnum.Success ? true :false;

        /// <summary>
        /// 状态2
        /// </summary>
        public string Status => WithdrawStatus == WithdrawStatusEnum.Apply ? "提现":
            WithdrawStatus == WithdrawStatusEnum.Pass ? "审核" :
            WithdrawStatus == WithdrawStatusEnum.Refuse ? "审核" : "到账";

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 状态时间
        /// </summary>
        public DateTime Time { get; set; }
    }
}
