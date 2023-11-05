using iSchool.FinanceCenter.Domain.Enum;
using iSchool.Infrastructure.Enums;
using System;

namespace iSchool.FinanceCenter.Appliaction.ResponseDto.Statement
{
    /// <summary>
    /// 流水详情
    /// </summary>
    public class StatementDetail
    {
        public OrderTypeEnum OrderType { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// 流水类型
        /// </summary>
        public StatementTypeEnum Type { get; set; }

        /// <summary>
        /// 类型描述
        /// </summary>
        public string TypeDes => Type == StatementTypeEnum.Close ? "失效" : Type.GetDescription();

        /// <summary>
        /// 金额
        /// </summary>
        public decimal Amount { get; set; }


        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo { get; set; }

        private string Remark { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name => (OrderType == OrderTypeEnum.OrgFx && Type == StatementTypeEnum.Close) ? $"佣金失效" : $"{Remark}";
        /// <summary>
        /// OrderId
        /// </summary>
        public Guid? OrderId { get; set; }
        /// <summary>
        /// OrderDetailId
        /// </summary>
        public Guid? OrderDetailId { get; set; }
    }
}
