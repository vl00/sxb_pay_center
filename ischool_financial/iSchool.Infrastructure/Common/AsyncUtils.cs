using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Infrastructure
{
    public class AsyncUtils
    {
        private static IServiceScopeFactory ServiceScopeFactory;
        static int _inqNo;

        public static event Action<IServiceProvider, Exception> OnBackgroundError;

        public static void SetServiceScopeFactory(IServiceScopeFactory serviceScopeFactory)
        {
            if (ServiceScopeFactory != null) throw new InvalidOperationException("ServiceScopeFactory is set before");
            ServiceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Safely do async operation in asp.net core DI
        /// </summary>
        /// <param name="func"></param>
        /// <param name="state"></param>
        public static async void StartNew(Func<IServiceProvider, object, Task> func, object state = null)
        {
            if (ServiceScopeFactory == null) throw new InvalidOperationException($"must use to call {nameof(SetServiceScopeFactory)} first.");
            if (func == null) throw new ArgumentNullException(nameof(func));
            await Task.Delay(100).ConfigureAwait(false);
            using var scope = ServiceScopeFactory.CreateScope();
            try 
            { 
                await func(scope.ServiceProvider, state).ConfigureAwait(false); 
            }
            catch (Exception ex)
            {
                var ev = OnBackgroundError;
                if (ev == null) return;
                lock (ServiceScopeFactory)
                {
                    ev(scope.ServiceProvider, ex);
                }
            }
        }

        /// <summary>
        /// Safely do async operation with MediatR in asp.net core DI
        /// </summary>
        /// <param name="entity">IRequest or INotification</param>
        public static void StartNew(object entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            StartNew((sp, o) => on_mediator(sp.GetService<IMediator>(), o), entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool EnqSimpleQueue(object entity)
        {
            if (ServiceScopeFactory == null) throw new InvalidOperationException($"must use to call {nameof(SetServiceScopeFactory)} first.");
            //if (_inqNo > 20 || Volatile.Read(ref _inqNo) > 20)
            //{
            //    onenq_SimpleQueue_fail(entity);
            //    return false;
            //}
            //if (Interlocked.Increment(ref _inqNo) > 20)
            //{
            //    Interlocked.Decrement(ref _inqNo);
            //    onenq_SimpleQueue_fail(entity);
            //    return false;
            //}
            SimpleQueue.Default.Enqueue(o => ondeq_SimpleQueue(o).Wait(), entity);
            return true;
        }

        static async Task ondeq_SimpleQueue(object entity)
        {
            //Interlocked.Decrement(ref _inqNo);
            using var scope = ServiceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetService<IMediator>();
            try
            {
                await on_mediator(mediator, entity).ConfigureAwait(false);                
            }
            catch (Exception ex)
            {
                var ev = OnBackgroundError;
                if (ev == null) return;
                lock (ServiceScopeFactory)
                {
                    ev(scope.ServiceProvider, ex);
                }
            }
        }
        static void onenq_SimpleQueue_fail(object entity)
        { 
        }

        private static Task on_mediator(IMediator mediator, object entity)
        {
            return entity switch
            {
                IRequest req => mediator.Send(entity),
                INotification pub => mediator.Publish(entity),
                _ => throw new InvalidOperationException("entity can't be handled by MediatorR"),
            };
        }
    }
}
