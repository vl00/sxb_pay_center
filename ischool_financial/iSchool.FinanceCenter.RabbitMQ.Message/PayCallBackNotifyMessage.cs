using ProductManagement.Framework.RabbitMQ;
using ProductManagement.Framework.RabbitMQ.EventBus;
using System;

namespace iSchool.FinanceCenter.RabbitMQ.Message
{
    [MessageAlias("WeChatPayCallBack_QUEUE")]
    public class PayCallBackNotifyMessage : IMessage
    {
        public Guid OrderId { get; set; }
        public int PayStatus { get; set; }
        public DateTime AddTime { get; set; }
    }
}
