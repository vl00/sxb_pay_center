using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Messeage.EvenBus
{
    public interface ISubscribe
    {
        /// <summary>
        /// 订阅一个事件
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        void Subscribe<TMessage>() where TMessage : IMessage;

        /// <summary>
        /// 取消订阅一个事件
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        void Unsubscribe<TMessage>() where TMessage : IMessage;
    }
}
