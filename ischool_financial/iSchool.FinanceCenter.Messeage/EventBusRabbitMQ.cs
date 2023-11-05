using iSchool.FinanceCenter.Messeage.Config;
using iSchool.FinanceCenter.Messeage.EvenBus;
using iSchool.FinanceCenter.Messeage.Serialize;
using iSchool.Infrastructure;
using iSchool.Infrastructure.SxbAttribute;
using Microsoft.Extensions.Logging;

using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Net.Sockets;
using System.Reflection;

namespace iSchool.FinanceCenter.Messeage
{
    public class EventBusRabbitMQ : IEventBus
    {
        private readonly ILogger<EventBusRabbitMQ> _logger;
        private readonly IMessageSerialize _messageSerialize;

        private readonly IRabbitMQPersistentConnection _persistentConnection;

        public EventBusRabbitMQ(IRabbitMQPersistentConnection persistentConnection,
            ILoggerFactory loggerFactory,
            IMessageSerialize messageSerialize)
        {
            _logger = loggerFactory.CreateLogger<EventBusRabbitMQ>();
            _persistentConnection = persistentConnection;
            _messageSerialize = messageSerialize;
        }

        public void Publish(IMessage message, string keyProfix = "")
        {
            if (!_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) => { _logger.LogWarning(ex.ToString()); });

            using (var channel = _persistentConnection.CreateModel())
            {
                var messageType = message.GetType();
                var alias = messageType.GetCustomAttribute<MessageAliasAttribute>();
                var messageName = keyProfix + (alias?.Alias ?? messageType.FullName);
                var queueName = messageName + _persistentConnection.ExtName();
          
                var routingKey = messageName + _persistentConnection.ExtName();
                channel.ExchangeDeclare(ConfigHelper.GetConfigString("RabbitMqExchange"),
                    "direct",
                    true,
                    false,
                    null);

                channel.QueueDeclare(queueName,
                    true,
                    false,
                    false,
                    null);

                channel.QueueBind(queueName, ConfigHelper.GetConfigString("RabbitMqExchange"), routingKey, null);
                var body = _messageSerialize.Serialize(message);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                policy.Execute(() =>
                {
                    channel.BasicPublish(ConfigHelper.GetConfigString("RabbitMqExchange"),
                        routingKey,
                        properties,
                        body);
                });
            }
        }
    }
}