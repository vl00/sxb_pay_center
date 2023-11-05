using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.ResponseDto.Withdraw
{
    /// <summary>
    /// 审核金额统计
    /// </summary>
    public class AmountStatisticsResult
    {
        /// <summary>
        /// 待审核总金额
        /// </summary>
        public decimal ApplyAmount { get; set; }

        /// <summary>
        /// 待审核总笔数
        /// </summary>
        public int ApplyCount { get; set; }

        /// <summary>
        /// 已审核总金额
        /// </summary>
        public decimal PassAmount { get; set; }
    }
}
