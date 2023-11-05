using System;
using iSchool.FinanceCenter.Messeage.EvenBus;
using iSchool.Infrastructure.SxbAttribute;

namespace iSchool.FinanceCenter.Messeage.QueueEntity
{
    [MqExchange("ProductManagement.Event.Bus")]
    [MessageAlias("SyncWallet_QUEUE")]
    public class WalletOpreateMessage : IMessage
    {
        /// <summary>
        /// 用户id
        /// </summary>
      
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
       
        public int StatementType { get; set; }

        /// <summary>
        /// 1支出   2收入
        /// </summary>
       
        public int Io { get; set; }

        /// <summary>
        /// 订单ID
        /// </summary>
       
        public Guid OrderId { get; set; }

        /// <summary>
        /// 商品类型 1上学问，2 完成提现(结算)，3 机构
        /// </summary>
      
        public int OrderType { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}
