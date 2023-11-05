using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace iSchool.FinanceCenter.Messeage
{
    public interface IRabbitMQPersistentConnection : IDisposable
    {
        string ExtName();

        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}
