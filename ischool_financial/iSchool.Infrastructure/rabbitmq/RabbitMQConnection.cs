using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IRabbitMQChannel = RabbitMQ.Client.IModel;

namespace iSchool.BgServices
{
    public interface IRabbitMQConnection
    {
        ConnectionFactory ConnFactory { get; }
        /// <summary>
        /// heartbeat seconds
        /// </summary>
        ushort Heartbeat { get; }
        bool IsAutoReconnect { get; }
        bool IsOpened { get; }
        void Open();
        void Close();
        IRabbitMQChannel CreateChannel();
    }

    public static class RabbitMQ_Extension
    {
        public static T TryOpen<T>(this T connection) where T : IRabbitMQConnection
        {
            if (!connection.IsOpened) connection.Open();
            return connection;
        }

        public static IRabbitMQChannel OpenChannel(this IRabbitMQConnection connection)
        {
            if (!connection.IsOpened) connection.Open();
            return connection.CreateChannel();
        }
    }

    public class RabbitMQConnection : IRabbitMQConnection
    {
        readonly ILogger _log;
        IConnection _connection;
        readonly object sync_root = new object();

        public RabbitMQConnection(ConnectionFactory connectionFactory, ILoggerFactory loggerFactory = null)
        {
            _log = loggerFactory?.CreateLogger(GetType());
            ConnFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        // see https://www.rabbitmq.com/dotnet-api-guide.html#recovery
        public bool IsAutoReconnect => ConnFactory.AutomaticRecoveryEnabled && ConnFactory.TopologyRecoveryEnabled;
        public bool IsOpened => _connection != null && _connection.IsOpen;
        public ushort Heartbeat => ConnFactory.RequestedHeartbeat;
        public ConnectionFactory ConnFactory { get; }

        public IRabbitMQChannel CreateChannel()
        {
            if (!IsOpened) throw new InvalidOperationException("connection is closed");
            return _connection.CreateModel();
        }

        public void Close()
        {
            if (_connection == null) return;
            lock (sync_root)
            {
                if (_connection == null) return;
                CloseCore();
            }
        }

        public void Open()
        {
            if (!IsOpened)
            {
                lock (sync_root)
                {
                    if (_connection != null)
                    {
                        if (_connection.IsOpen) return;
                        CloseCore();
                    }
                    for (int i = 0, c = 2; i < c; i++)
                    {
                        try
                        {
                            _connection = ConnFactory.CreateConnection();
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (i + 1 == c) throw ex;
                            else Task.Delay(500).Wait();
                        }
                    }
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;
                }
            }
        }

        void CloseCore()
        {
            try
            {
                _connection.ConnectionShutdown -= OnConnectionShutdown;
                _connection.CallbackException -= OnCallbackException;
                _connection.ConnectionBlocked -= OnConnectionBlocked;

                _connection.Close();
            }
            catch (AlreadyClosedException)
            {
                // ignore
            }
            catch (IOException)
            {
                // ignore
            }
            finally
            {
                try { _connection.Dispose(); } catch { }
                _connection = null;
            }
        }

        void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            _log?.LogDebug("rabbitmq conn blocked: {0}", e.Reason);
        }

        void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            _log?.LogDebug(e.Exception, "rabbitmq conn callback error");
        }

        void OnConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _log?.LogDebug("rabbitmq conn shutdown, code={0}, reply={1}", e.ReplyCode, e.ReplyText);
        }
    }

    public class RabbitMQConnectionForPublish : RabbitMQConnection, IDisposable
    {
        public RabbitMQConnectionForPublish(ConnectionFactory connectionFactory, ILoggerFactory loggerFactory = null)
            : base(connectionFactory, loggerFactory)
        { }

        public void Dispose()
        {
            this.Close();
        }
    }
}
