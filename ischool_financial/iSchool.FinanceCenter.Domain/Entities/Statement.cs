using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Domain.Entities
{
    /// <summary>
    /// 流水表实体类
    /// </summary>
    [Table("statement")]
    public partial class Statement
    {
        public Statement() { }
        public Statement(Guid id, Guid userId, string no, decimal amount, int statementType, int io, Guid orderId, int orderType, string remark)
        {
            Id = id;
            UserId = userId;
            No = no;
            Amount = amount;
            StatementType = statementType;
            Io = io;
            OrderId = orderId;
            OrderType = orderType;
            CreateTime = DateTime.Now;
            Remark = remark;
        }



        /// <summary>
        /// id
        /// </summary>
        [ExplicitKey]
        public Guid Id { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 流水号
        /// </summary>
        public string No { get; set; }

        /// <summary>
        /// 变动金额（正负数）
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 流水类型（1充值、2支出、3 收入、4结算、5服务费）
        /// </summary>
        public int StatementType { get; set; }

        /// <summary>
        /// 1支出 2收入
        /// </summary>
        public int Io { get; set; }

        /// <summary>
        /// 订单ID
        /// </summary>
        public Guid OrderId { get; set; }

        /// <summary>
        /// 商品类型 1上学问
        /// </summary>
        public int OrderType { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        public Guid OrderDetailId { get; set; }
    }
}
