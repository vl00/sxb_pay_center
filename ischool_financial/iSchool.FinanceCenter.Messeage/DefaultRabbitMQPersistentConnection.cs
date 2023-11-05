using iSchool.FinanceCenter.Messeage.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace iSchool.FinanceCenter.Messeage
{
    public class DefaultRabbitMQPersistentConnection : IRabbitMQPersistentConnection
    {

        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<DefaultRabbitMQPersistentConnection> _logger;

        private IConnection _connection;
        private bool _disposed;
        private readonly object _syncRoot = new object();

        private readonly IList<AmqpTcpEndpoint> _endpoints;

        private readonly string _extName;

        public DefaultRabbitMQPersistentConnection(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory, IOptions<RabbitMQOption> option)
        {
            _endpoints = option.Value.AmqpUris.Select(u => new AmqpTcpEndpoint(new Uri(u))).ToList();
            var f = (ConnectionFactory)connectionFactory;
            f.Uri = new Uri(option.Value.Uri);
            f.AutomaticRecoveryEnabled = false;
            _connectionFactory = f;
            _extName = option.Value.ExtName;
            _logger = loggerFactory.CreateLogger<DefaultRabbitMQPersistentConnection>();
        }
        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

        public string ExtName()
        {
            return _extName;
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("没有可用的 RabbitMQ 连接");
            }

            return _connection.CreateModel();
        }

        public bool TryConnect()
        {
            _logger.LogInformation("RabbitMQ Client is trying to connect");

            lock (_syncRoot)
            {

                var policy = Policy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        _logger.LogWarning(ex.ToString());
                    }
                );

                policy.Execute(() =>
                {
                    _connection = _connectionFactory
                          .CreateConnection(_endpoints);
                });

                if (IsConnected)
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;

                    _logger.LogInformation($"RabbitMQ persistent connection acquired a connection {_connection.Endpoint.HostName} and is subscribed to failure events");

                    return true;
                }
                _logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");

                return false;
            }
        }

        void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;

            _logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");

            TryConnect();
        }

        void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;

            _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");

            TryConnect();
        }

        void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed) return;

            _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");

            TryConnect();
        }

        public void Dispose()
        {
            if (_disposed || _connection==null) return;

            _disposed = true;

            try
            {
                _connection.Dispose();
            }
            catch (IOException ex)
            {
                _logger.LogCritical("mq:"+ex.ToString());
            }
        }


    }
}
