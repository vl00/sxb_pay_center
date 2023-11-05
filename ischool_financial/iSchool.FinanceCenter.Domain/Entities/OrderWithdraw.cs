using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Entities
{
    /// <summary>
    /// 订单结算表
    /// </summary>
    [Table("OrderWithdraw")]
    public partial class OrderWithdraw
    {
        /// <summary>
        /// id
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 订单结算号
        /// </summary>
        public string No { get; set; }

        /// <summary>
        /// 订单id
        /// </summary>
        public Guid OrderId { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        public Guid OrderType { get; set; }

        /// <summary>
        /// 订单出账金额
        /// </summary>
        public decimal OutAmount { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
