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
    public class WithdrawRecordsResult
    {
        private string NickName { get; set; }
        private string BankCardNo { get; set; }

        private WithdrawWayEnum WithdrawWay { get; set; }

        private string BankitterNo => BankCardNo?.Length > 4 ? BankCardNo?.Substring(BankCardNo.Length - 4, 4) : BankCardNo;
        /// <summary>
        /// 提现名称
        /// </summary>
        public string Name => WithdrawWay == WithdrawWayEnum.WeChat ? $"提现到微信账号-{NickName}{BankitterNo}" : $"提现到{ WithdrawWay.GetDescription()}-{NickName}{BankitterNo}";

        /// <summary>
        /// 申请时间
        /// </summary>
        public DateTime ApplyForTime { get; set; }

        /// <summary>
        /// 提现编号
        /// </summary>
        public string No { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        private WithdrawStatusEnum WithdrawStatus { get; set; }

        /// <summary>
        /// 状态描述
        /// </summary>
        private string StatusDes => WithdrawStatus.GetDescription();

        private CompanyPayStatusEnum PayStatus { get; set; }

        private DateTime PayTime { get; set; }

        /// <summary>
        /// 到账时间
        /// </summary>
        public string ArrivedTime
        {
            get
            {
                switch (WithdrawStatus)
                {
                    case WithdrawStatusEnum.Apply:
                        return "审核中";
                    case WithdrawStatusEnum.Refuse:
                        return "不通过";
                    case WithdrawStatusEnum.Pass:
                        return $"已到账({PayTime})";
                    case WithdrawStatusEnum.SyncThirdParty:
                        return "审核中";
                    default:
                        return "";
                }


            }
           
        }
    }
}
