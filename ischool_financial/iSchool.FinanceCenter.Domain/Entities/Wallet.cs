using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Entities
{
    /// <summary>
    /// 钱包表实体类
    /// </summary>
    [Table("Wallet")]
    public partial class Wallet
    {
        // <summary>
        /// 用户
        /// </summary>
        [ExplicitKey]
        public Guid UserId { get; set; }

        // <summary>
        /// 总金额
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 提现冻结金额
        /// </summary>
        public decimal BlockedAmount { get; set; }

        /// <summary>
        /// 剩余金额
        /// </summary>
        public decimal RemainAmount { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 虚拟赠送总金额
        /// </summary>
        public decimal VirtualTotalAmount { get; set; }

        /// <summary>
        /// 虚拟赠送剩余金额
        /// </summary>
        public decimal VirtualRemainAmount { get; set; }

        /// <summary>
        /// 检查字段
        /// </summary>
        public string CheckSign { get; set; }

    }
}
