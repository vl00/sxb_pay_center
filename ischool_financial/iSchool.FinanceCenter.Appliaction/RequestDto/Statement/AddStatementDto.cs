using iSchool.FinanceCenter.Domain.Enum;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.Statement
{
    /// <summary>
    /// 新增statement类
    /// </summary>
    public class AddStatementDto: IRequest<bool>
    {
        /// <summary>
        /// 用户id
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// 变动金额（正负数）
        /// </summary>
        [Required]
        public decimal Amount { get; set; }

        /// <summary>
        /// 流水类型（1充值、2支出、3 收入、4结算、5服务费）
        /// </summary>
        [Required]
        public StatementTypeEnum StatementType { get; set; }

        /// <summary>
        /// 1支出 2收入
        /// </summary>
        [Required]
        public StatementIoEnum Io { get; set; }

        /// <summary>
        /// 订单ID
        /// </summary>
        [Required]
        public Guid OrderId { get; set; }

        /// <summary>
        /// 商品类型 1上学问
        /// </summary>
        [Required]
        public OrderTypeEnum OrderType { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        public Guid OrderDetailId { get; set; }
        public DateTime? FixTime { get; set; } = null;
    }
}
