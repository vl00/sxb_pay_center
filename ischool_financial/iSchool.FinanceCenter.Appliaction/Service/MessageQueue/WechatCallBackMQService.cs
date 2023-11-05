using iSchool.FinanceCenter.Messeage.EvenBus;
using iSchool.FinanceCenter.Messeage.QueueEntity;

namespace iSchool.FinanceCenter.Appliaction.Service.MessageQueue
{
    public class WechatCallBackMQService : IWechatCallBackMQService
    {
        private readonly IEventBus _eventBus;

        public WechatCallBackMQService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        //发布到RabbitMQ
        public void Notify(PayCallBackNotifyMessage message,string keyProfix="")
        {
            _eventBus.Publish(message, keyProfix);
        }


        //发布到RabbitMQ
        public void NotifyTest(WalletOpreateMessage message, string keyProfix = "")
        {
            _eventBus.Publish(message, keyProfix);
        }
    }
}
