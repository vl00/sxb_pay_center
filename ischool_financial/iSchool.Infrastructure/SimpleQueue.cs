using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iSchool.Infrastructure
{
    // change from https://github.com/aspnet/AspNetCore/blob/master/src/Servers/Kestrel/Transport.Sockets/src/Internal/IOQueue.cs
    public partial class SimpleQueue : IThreadPoolWorkItem
    {
        readonly ConcurrentQueue<Work> _workItems = new ConcurrentQueue<Work>();
        int _doingWork;
        static SimpleQueue _Default;

        public static SimpleQueue Default => LazyInitializer.EnsureInitialized(ref _Default);

        public void Enqueue(Action action)
        {
            Enqueue((o) => ((Action)o).Invoke(), action);
        }

        public void Enqueue(Action<object> action, object state)
        {
            _workItems.Enqueue(new Work(action, state));

            if (Interlocked.CompareExchange(ref _doingWork, 1, 0) == 0)
            {
                ThreadPool.UnsafeQueueUserWorkItem(this, false);
            }
        }

        void IThreadPoolWorkItem.Execute()
        {
            while (true)
            {
                while (_workItems.TryDequeue(out Work item))
                {
                    item.Callback(item.State);
                }

                // All work done.

                // Set _doingWork (0 == false) prior to checking IsEmpty to catch any missed work in interim.
                // This doesn't need to be volatile due to the following barrier (i.e. it is volatile).
                _doingWork = 0;

                // Ensure _doingWork is written before IsEmpty is read.
                // As they are two different memory locations, we insert a barrier to guarantee ordering.
                Thread.MemoryBarrier();

                // Check if there is work to do
                if (_workItems.IsEmpty)
                {
                    // Nothing to do, exit.
                    break;
                }

                // Is work, can we set it as active again (via atomic Interlocked), prior to scheduling?
                if (Interlocked.Exchange(ref _doingWork, 1) == 1)
                {
                    // Execute has been rescheduled already, exit.
                    break;
                }

                // Is work, wasn't already scheduled so continue loop.
            }
        }

        private readonly struct Work
        {
            public readonly Action<object> Callback;
            public readonly object State;

            public Work(Action<object> callback, object state)
            {
                Callback = callback;
                State = state;
            }
        }
    }

    public static class SimpleQueue_Extension
    {
        public static IServiceScopeFactory ServiceScopeFactory;

        public static void EnqueueThenRunOnChildScope(this SimpleQueue queue, IServiceProvider services, Action<IServiceProvider, object> action, object state = null)
            => EnqueueThenRunOnChildScope(queue, action, state);
        public static void EnqueueThenRunOnChildScope(this SimpleQueue queue, IServiceProvider services, Func<IServiceProvider, object, Task> func, object state = null)
            => EnqueueThenRunOnChildScope(queue, func, state);

        public static void EnqueueThenRunOnChildScope(this SimpleQueue queue, Action<IServiceProvider, object> action, object state = null)
        {
            if (queue == null || action == null) return;

            var sp = ServiceScopeFactory.CreateScope();
            queue.Enqueue(on_action, Tuple.Create(sp, action, state));

            static void on_action(object o)
            {
                var (_sp, _action, _state) = o as Tuple<IServiceScope, Action<IServiceProvider, object>, object>;
                try { _action(_sp.ServiceProvider, _state); }
                catch { }
                finally { _sp.Dispose(); }
            }
        }

        public static void EnqueueThenRunOnChildScope(this SimpleQueue queue, Func<IServiceProvider, object, Task> func, object state = null)
        {
            if (queue == null || func == null) return;

            var sp = ServiceScopeFactory.CreateScope();
            queue.Enqueue(on_action, Tuple.Create(sp, func, state));

            static void on_action(object o)
            {
                var (_sp, _func, _state) = o as Tuple<IServiceScope, Func<IServiceProvider, object, Task>, object>;
                try { _func(_sp.ServiceProvider, _state).GetAwaiter().GetResult(); }
                catch { }
                finally { _sp.Dispose(); }
            }
        }

        public static Task EnqueueToAwaitCompleted(this SimpleQueue queue, Func<object, Task> func, object state = null)
        {
            if (queue == null || func == null) return Task.CompletedTask;

            var tcs = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            queue.Enqueue(on_action, Tuple.Create(tcs, func, state));
            return tcs.Task;

            static void on_action(object o)
            {
                on_task(o as Tuple<TaskCompletionSource<object>, Func<object, Task>, object>).GetAwaiter().GetResult();
            }

            static async Task on_task(Tuple<TaskCompletionSource<object>, Func<object, Task>, object> _tuple)
            {
                var (_tcs, _action, _state) = _tuple;
                try
                {
                    await _action(_state).ConfigureAwait(false);
                    _tcs.TrySetResult(null);
                }
                catch (OperationCanceledException ex)
                {
                    _tcs.TrySetCanceled(ex.CancellationToken);
                }
                catch (Exception ex)
                {
                    _tcs.TrySetException(ex);
                }
            }
        }
    }
}
