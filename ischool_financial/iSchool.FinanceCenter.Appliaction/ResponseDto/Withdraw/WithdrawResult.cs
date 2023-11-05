using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw
{
    /// <summary>
    /// 提现返回类
    /// </summary>
    public class WithdrawResult
    {
       
        /// <summary>
        /// 提现编号
        /// </summary>
        public string No { get; set; }
        public string ErrorMsg { get; set; }
       /// <summary>
       /// 0 正常 -1需要认证 -2 超过次数 -3超过金额,-4系统错误
       /// </summary>
        public int status { get; set; }

    }
    public class PreCheckWithdrawResult
    {
        /// <summary>
        /// 是否高登签约
        /// </summary>
        public bool IsSign { get; set; }
        /// <summary>
        /// 不限制的金额
        /// </summary>
        public int NoLimitAmount { get; set; }
        /// <summary>
        /// 提现编号
        /// </summary>
        public string No { get; set; }
        public string ErrorMsg { get; set; }
        /// <summary>
        /// 0 正常 -1需要认证 -2 超过次数 -3超过金额,-4系统错误
        /// </summary>
        public int status { get; set; }

    }
}
