using iSchool.Domain.Repository.Interfaces;
using iSchool.FinanceCenter.Messeage.QueueEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Appliaction.Service.MessageQueue
{
    public interface IWechatCallBackMQService: IDependency
    {
         void Notify(PayCallBackNotifyMessage message, string keyProfix = "");
        void NotifyTest(WalletOpreateMessage message, string keyProfix = "");
    }
}
