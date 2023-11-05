using System;

namespace iSchool.FinanceCenter.Messeages
{
    [MessageAlias("WeChatPayCallBack_QUEUE")]
    public class PayCallBackNotifyMessage : IMessage
    {
        public Guid OrderId { get; set; }
        public int PayStatus { get; set; }
        public DateTime AddTime { get; set; }
    }
}
