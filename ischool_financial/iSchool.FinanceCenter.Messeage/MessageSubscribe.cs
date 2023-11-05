using iSchool.FinanceCenter.Messeage.Config;
using iSchool.FinanceCenter.Messeage.EvenBus;
using iSchool.FinanceCenter.Messeage.Serialize;
using iSchool.Infrastructure;
using iSchool.Infrastructure.SxbAttribute;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Reflection;

namespace iSchool.FinanceCenter.Messeage
{
    public class MessageSubscribe : ISubscribe
    {
        private static readonly ConcurrentDictionary<string, MessageReceived> _messageReceiveds =
            new ConcurrentDictionary<string, MessageReceived>();

        private readonly IMessageSerialize _messageSerialize;

        private readonly MultiInstanceFactory _multiInstanceFactory;
        private readonly IRabbitMQPersistentConnection _persistentConnection;

        private readonly ILogger<MessageReceived> _logger;

        public MessageSubscribe(MultiInstanceFactory multiInstanceFactory,
            IRabbitMQPersistentConnection persistentConnection, ILoggerFactory loggerFactory,
            IMessageSerialize messageSerialize)
        {
            _multiInstanceFactory = multiInstanceFactory;
            _persistentConnection = persistentConnection;
            _messageSerialize = messageSerialize;
            _logger = loggerFactory.CreateLogger<MessageReceived>();
        }

        public void Subscribe<TMessage>() where TMessage : IMessage
        {
            var messageType = typeof(TMessage);
            var alias = messageType.GetCustomAttribute<MessageAliasAttribute>();
            var routeKey = messageType.GetCustomAttribute<RouteKeyAtttribute>();
            var exchange = messageType.GetCustomAttribute<MqExchangeAttribute>();
            var broker = Constants.BrokerName;
            if (!string.IsNullOrEmpty(exchange?.Exchange))
            {
                broker = exchange.Exchange;
            }
            var messageName = alias?.Alias ?? messageType.FullName;
            var queueName = messageName + _persistentConnection.ExtName();
      
            var routingKey = routeKey == null ? queueName : routeKey.Key + _persistentConnection.ExtName();
            _messageReceiveds.GetOrAdd(messageName, t =>
            {
                var handler = new MessageReceived<TMessage>(_messageSerialize, _multiInstanceFactory, _logger);

                if (!_persistentConnection.IsConnected)
                    _persistentConnection.TryConnect();

                var channel = _persistentConnection.CreateModel();

                channel.ExchangeDeclare(broker,
                    "direct",
                    true,
                    false,
                    null);

                channel.QueueDeclare(queueName,
                    true,
                    false,
                    false,
                    null);

                channel.QueueBind(queueName,
                    broker,
                    routingKey);

                channel.BasicQos(0, 1, false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += handler.Received;

                channel.BasicConsume(queueName,
                    false,
                    consumer);

                return handler;
            });
        }

        public void Unsubscribe<TMessage>() where TMessage : IMessage
        {
            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();
          
            var messageType = typeof(TMessage);
            var messageName = (messageType.FullName) + _persistentConnection.ExtName();
            var exchange = messageType.GetCustomAttribute<MqExchangeAttribute>();
            var broker = Constants.BrokerName;
            if (!string.IsNullOrEmpty(exchange?.Exchange))
            {
                broker = exchange.Exchange;
            }
            var routingKey = messageName + _persistentConnection.ExtName();
            if (_messageReceiveds.TryRemove(messageName, out var received))
                using (var channel = _persistentConnection.CreateModel())
                {
                    channel.QueueUnbind(messageName,
                        broker,
                        routingKey);
                }
        }
    }
}