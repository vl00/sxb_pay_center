using iSchool.FinanceCenter.Messeage;
using iSchool.FinanceCenter.Messeage.Config;
using iSchool.FinanceCenter.Messeage.EvenBus;
using iSchool.FinanceCenter.Messeage.Serialize;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace iSchool.FinanceCenter.Api.Extension
{
    public static class SchoolRabbitMqSetup
    {
        public static IServiceCollection AddFinanceCenterRabbitMQ(this IServiceCollection collection,
            Action<RabbitMQOption> configureOptions,
            IMessageSerialize messageSerialize)
        {
            collection.Configure(configureOptions);
            collection.AddSingleton<MultiInstanceFactory>(p => p.GetRequiredServices);
            collection.AddSingleton(messageSerialize);
            collection.AddSingleton<IConnectionFactory, ConnectionFactory>();
            collection.AddSingleton<IRabbitMQPersistentConnection, DefaultRabbitMQPersistentConnection>();
            collection.AddSingleton<IEventBus, EventBusRabbitMQ>();
            collection.AddSingleton<ISubscribe, MessageSubscribe>();
            return collection;
        }

        private static readonly List<Assembly> Assembly = new List<Assembly>();


        private static IEnumerable<object> GetRequiredServices(this IServiceProvider provider, Type serviceType)
        {
            return (IEnumerable<object>) provider.GetRequiredService(
                typeof(IEnumerable<>).MakeGenericType(serviceType));
        }
        public static void ScanMessage(this IServiceCollection collection, params Assembly[] assemblys)
        {
            Assembly.AddRange(assemblys);

            //collection.Scan(scan => scan
            //    .FromAssemblies(assemblys)
            //    .AddClasses()
            //    .AsImplementedInterfaces());
        }
        public static void SubscribeRabbitMQ(this IApplicationBuilder app)
        {
            var subscribe = app.ApplicationServices.GetService<ISubscribe>();

            foreach (var assembly in Assembly)
            {
                var types = assembly.GetTypes().Where(type => type.GetInterfaces().Contains(typeof(IEventHandler)));

                foreach (var type in types)
                {
                    var genericType =
                        type.GetInterfaces().FirstOrDefault(t => t.IsGenericType)?.GetGenericArguments()[0];
                    if (genericType == null) continue;

                    var method = subscribe.GetType().GetMethod("Subscribe");
                    var generic = method.MakeGenericMethod(genericType);
                    generic.Invoke(subscribe, null);
                }
            }
        }
    }
}