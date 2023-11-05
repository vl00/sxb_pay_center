using iSchool.Domain.Modles;
using iSchool.FinanceCenter.Domain.Enum;
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace iSchool.FinanceCenter.Appliaction.RequestDto.Wallet
{
    /// <summary>
    /// 钱包参数类
    /// </summary>
    public class OperateWalletDto : IRequest<SqlBase>
    {
        /// <summary>
        /// 用户id
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// 虚拟赠送变动金额（正数）
        /// </summary>
        public decimal VirtualAmount { get; set; }

        /// <summary>
        /// 变动金额（正数）
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 冻结变动金额（正数）
        /// </summary>
        public decimal BlockedAmount { get; set; }

        /// <summary>
        /// 流水类型
        /// </summary>
        [Required]
        public StatementTypeEnum StatementType { get; set; }

        /// <summary>
        /// 1支出   2收入
        /// </summary>
        [Required]
        public StatementIoEnum Io { get; set; }

        /// <summary>
        /// 订单ID
        /// </summary>
        [Required]
        public Guid OrderId { get; set; }

        /// <summary>
        /// 商品类型 1上学问，2 完成提现(结算)，3 机构
        /// </summary>
        [Required]
        public OrderTypeEnum OrderType { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 公司修改
        /// </summary>
        public bool CompanyOperate { get; set; } = false;

        public WithdrawStatusEnum WithdrawStatus { get; set; } = WithdrawStatusEnum.Apply;
        public Guid  OrderDetailId { get; set; }
        public DateTime? FixTime { get; set; } = null;

    }
}
