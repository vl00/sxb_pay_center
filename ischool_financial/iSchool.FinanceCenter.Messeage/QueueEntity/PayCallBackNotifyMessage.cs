using iSchool.FinanceCenter.Messeage.EvenBus;
using iSchool.Infrastructure.SxbAttribute;
using System;

namespace iSchool.FinanceCenter.Messeage.QueueEntity
{
    [MessageAlias("WeChatPayCallBack_QUEUE")]
    public class PayCallBackNotifyMessage : IMessage
    {
        public string OrderNo { get; set; }
        public Guid OrderId { get; set; }
        public int PayStatus { get; set; }
        public DateTime AddTime { get; set; }
    }
}
