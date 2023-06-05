using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace PaxAndromeda.Instar;

public static class Utilities
{
    public static T CastTo<T>(object o)
    {
        return (T)o;
    }

    public static dynamic CastToReflected(object o, Type type)
    {
        var methodInfo = typeof(Utilities).GetMethod(nameof(CastTo), BindingFlags.Static | BindingFlags.Public);
        var genericArguments = new[] { type };
        var genericMethodInfo = methodInfo?.MakeGenericMethod(genericArguments);
        return genericMethodInfo?.Invoke(null, new[] { o }) ?? null!;
    }

    public static dynamic CastToReflectedArray(object[] o, Type type)
    {
        // dirty reflection hacks
        var arrayType = type.GetElementType();

        dynamic output = Activator.CreateInstance(type, new object[] { o.Length })!;
        if (output == null)
            return default!;

        var i = 0;
        foreach (var r in
                 from obj in o
                 let methodInfo = typeof(Utilities).GetMethod(nameof(CastTo), BindingFlags.Static | BindingFlags.Public)
                 let genericArguments = new[] { arrayType }
                 let genericMethodInfo = methodInfo?.MakeGenericMethod(genericArguments)
                 select genericMethodInfo?.Invoke(null, new[] { obj }))
            output[i++] = (dynamic)r;

        return output;
    }

    [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
    public static async Task WaitUntil(Func<bool> predicate, CancellationToken token = default)
    {
        await Task.Run(async () =>
            {
                while (predicate())
                    await Task.Delay(1000, token)
                        .ContinueWith(_ => { })
                        .ConfigureAwait(false);
            }, token)
            .ContinueWith(_ => { })
            .ConfigureAwait(false);
    }

    public static bool IsDebug()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }

    /// <summary>
    ///     Executes an async Task&lt;T&gt; method which has a void return value synchronously
    /// </summary>
    /// <param name="task">Task&lt;T&gt; method to execute</param>
    public static void RunSync(Func<Task> task)
    {
        var oldContext = SynchronizationContext.Current;
        var sync = new ExclusiveSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(sync);

        async void SendOrPostCallback(object _)
        {
            try
            {
                await task();
            }
            catch (Exception e)
            {
                sync.InnerException = e;
                throw;
            }
            finally
            {
                sync.EndMessageLoop();
            }
        }

        sync.Post(SendOrPostCallback!, null!);
        sync.BeginMessageLoop();

        SynchronizationContext.SetSynchronizationContext(oldContext);
    }

    /// <summary>
    ///     Executes an async Task&lt;T&gt; method which has a T return type synchronously
    /// </summary>
    /// <typeparam name="T">Return Type</typeparam>
    /// <param name="task">Task&lt;T&gt; method to execute</param>
    /// <returns></returns>
    public static T RunSync<T>(Func<Task<T>> task)
    {
        var oldContext = SynchronizationContext.Current;
        var sync = new ExclusiveSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(sync);
        var ret = default(T);

        async void SendOrPostCallback(object _)
        {
            try
            {
                ret = await task();
            }
            catch (Exception e)
            {
                sync.InnerException = e;
                throw;
            }
            finally
            {
                sync.EndMessageLoop();
            }
        }

        sync.Post(SendOrPostCallback!, null!);
        sync.BeginMessageLoop();
        SynchronizationContext.SetSynchronizationContext(oldContext);
        return ret!;
    }

    public static List<T>? GetAttributesOfType<T>(this Enum enumVal) where T : Attribute
    {
        var type = enumVal.GetType();
        var membersInfo = type.GetMember(enumVal.ToString());
        if (membersInfo.Length == 0)
            return null;

        var attributes = membersInfo[0].GetCustomAttributes(typeof(T), false);
        return attributes.Length > 0 ? attributes.OfType<T>().ToList() : null;
    }

    private class ExclusiveSynchronizationContext : SynchronizationContext
    {
        private readonly Queue<Tuple<SendOrPostCallback, object>> _items =
            new();

        private readonly AutoResetEvent _workItemsWaiting = new(false);
        private bool _done;
        public Exception? InnerException { get; set; }

        public override void Send(SendOrPostCallback d, object? state)
        {
            throw new NotSupportedException("We cannot send to our same thread");
        }

        public override void Post(SendOrPostCallback d, object? state)
        {
            lock (_items)
            {
                _items.Enqueue(Tuple.Create(d, state)!);
            }

            _workItemsWaiting.Set();
        }

        public void EndMessageLoop()
        {
            Post(_ => _done = true, null);
        }

        public void BeginMessageLoop()
        {
            while (!_done)
            {
                Tuple<SendOrPostCallback, object> task = null!;
                lock (_items)
                {
                    if (_items.Count > 0) task = _items.Dequeue();
                }

                if (task != null)
                {
                    task.Item1(task.Item2);
                    if (InnerException != null) // the method threw an exception
                        throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                }
                else
                {
                    _workItemsWaiting.WaitOne();
                }
            }
        }

        public override SynchronizationContext CreateCopy()
        {
            return this;
        }
    }
}