using iSchool.FinanceCenter.Messeage.EvenBus;
using iSchool.FinanceCenter.Messeage.Serialize;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iSchool.FinanceCenter.Messeage
{
    internal abstract class MessageReceived
    {
        public abstract void Received(object sender, BasicDeliverEventArgs e);
    }

    internal class MessageReceived<TMessage> : MessageReceived where TMessage : IMessage
    {
        private readonly IMessageSerialize _messageSerialize;

        private readonly MultiInstanceFactory _multiInstanceFactory;

        private readonly ILogger<MessageReceived> _logger;

        public MessageReceived(IMessageSerialize messageSerialize,
            MultiInstanceFactory multiInstanceFactory, ILogger<MessageReceived> logger)
        {
            _messageSerialize = messageSerialize;
            _multiInstanceFactory = multiInstanceFactory;
            _logger = logger;
        }

        public override void Received(object sender, BasicDeliverEventArgs e)
        {
            if (!(sender is EventingBasicConsumer consumer)) return;

            try
            {
                _logger.LogInformation("消息内容：“" + Encoding.UTF8.GetString(e.Body) + "”。");

                var message = _messageSerialize.Deserialize<TMessage>(e.Body);

                var handler = new MessageHandlerImpl<TMessage>();

                var run = handler.Handle(message, _multiInstanceFactory).ConfigureAwait(false).GetAwaiter();

                run.GetResult();

                if (run.IsCompleted)
                    consumer.Model.BasicAck(e.DeliveryTag, false);
                else
                    consumer.Model.BasicNack(e.DeliveryTag, false, true);
            }
            catch (Exception ex)
            {
                var msg = Encoding.UTF8.GetString(e.Body);
                _logger.LogError(ex,"消费队列出错，消息内容：“" + msg +"”。");
                consumer.Model.BasicNack(e.DeliveryTag, false, false);
            }
        }
    }

    internal class MessageHandlerImpl<TMessage>
        where TMessage : IMessage
    {
        public Task Handle(TMessage message, MultiInstanceFactory multiInstanceFactory)
        {
            var handlers = GetHandlers(message, multiInstanceFactory);

            return Task.WhenAll(handlers);
        }

        private static IEnumerable<THandler> GetHandlers<THandler>(MultiInstanceFactory factory)
        {
            return factory(typeof(THandler)).Cast<THandler>();
        }

        private IEnumerable<Task> GetHandlers(TMessage message, MultiInstanceFactory factory)
        {
            var notificationHandlers = GetHandlers<IEventHandler<TMessage>>(factory)
                .Select(x => x.Handle(message));

            return notificationHandlers;
        }
    }
}