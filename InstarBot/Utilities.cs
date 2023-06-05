using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace PaxAndromeda.Instar;

public static class Utilities
{
    public static T CastTo<T>(object o) => (T)o;

    public static dynamic CastToReflected(object o, Type type)
    {
        var methodInfo = typeof(Utilities).GetMethod(nameof(CastTo), BindingFlags.Static | BindingFlags.Public);
        var genericArguments = new[] { type };
        var genericMethodInfo = methodInfo?.MakeGenericMethod(genericArguments);
        return genericMethodInfo?.Invoke(null, new[] { o });
    }

    public static dynamic CastToReflectedArray(object[] o, Type type)
    {
        // dirty reflection hacks
        Type arrayType = type.GetElementType();

        dynamic output = Activator.CreateInstance(type, args: new object[] { o.Length });
        if (output == null)
            return default;

        int i = 0;
        foreach (object r in
            from obj in o
            let methodInfo = typeof(Utilities).GetMethod(nameof(CastTo), BindingFlags.Static | BindingFlags.Public)
            let genericArguments = new[] { arrayType }
            let genericMethodInfo = methodInfo?.MakeGenericMethod(genericArguments)
            select genericMethodInfo?.Invoke(null, new[] { obj }))
        {
            output[i++] = (dynamic)r;
        }

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
    /// Execute's an async Task&lt;T&gt; method which has a void return value synchronously
    /// </summary>
    /// <param name="task">Task&lt;T&gt; method to execute</param>
    public static void RunSync(Func<Task> task)
    {
        var oldContext = SynchronizationContext.Current;
        var synch = new ExclusiveSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(synch);

        async void SendOrPostCallback(object _)
        {
            try
            {
                await task();
            }
            catch (Exception e)
            {
                synch.InnerException = e;
                throw;
            }
            finally
            {
                synch.EndMessageLoop();
            }
        }

        synch.Post(SendOrPostCallback, null);
        synch.BeginMessageLoop();

        SynchronizationContext.SetSynchronizationContext(oldContext);
    }

    /// <summary>
    /// Execute's an async Task&lt;T&gt; method which has a T return type synchronously
    /// </summary>
    /// <typeparam name="T">Return Type</typeparam>
    /// <param name="task">Task&lt;T&gt; method to execute</param>
    /// <returns></returns>
    public static T RunSync<T>(Func<Task<T>> task)
    {
        var oldContext = SynchronizationContext.Current;
        var synch = new ExclusiveSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(synch);
        T ret = default(T);

        async void SendOrPostCallback(object _)
        {
            try
            {
                ret = await task();
            }
            catch (Exception e)
            {
                synch.InnerException = e;
                throw;
            }
            finally
            {
                synch.EndMessageLoop();
            }
        }

        synch.Post(SendOrPostCallback, null);
        synch.BeginMessageLoop();
        SynchronizationContext.SetSynchronizationContext(oldContext);
        return ret;
    }

    public static List<T>? GetAttributesOfType<T>(this Enum enumVal) where T : Attribute
    {
        var type = enumVal.GetType();
        var memsInfo = type.GetMember(enumVal.ToString());
        if (memsInfo.Length == 0)
            return null;

        var attributes = memsInfo[0].GetCustomAttributes(typeof(T), false);
        return (attributes.Length > 0) ? attributes.OfType<T>().ToList() : null;
    }

    private class ExclusiveSynchronizationContext : SynchronizationContext
    {
        private bool done;
        public Exception InnerException { get; set; }
        private readonly AutoResetEvent workItemsWaiting = new(false);
        private readonly Queue<Tuple<SendOrPostCallback, object>> items =
            new();

        public override void Send(SendOrPostCallback d, object state)
        {
            throw new NotSupportedException("We cannot send to our same thread");
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            lock (items)
            {
                items.Enqueue(Tuple.Create(d, state));
            }

            workItemsWaiting.Set();
        }

        public void EndMessageLoop()
        {
            Post(_ => done = true, null);
        }

        public void BeginMessageLoop()
        {
            while (!done)
            {
                Tuple<SendOrPostCallback, object> task = null;
                lock (items)
                {
                    if (items.Count > 0)
                    {
                        task = items.Dequeue();
                    }
                }

                if (task != null)
                {
                    task.Item1(task.Item2);
                    if (InnerException != null) // the method threw an exeption
                    {
                        throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                    }
                }
                else
                {
                    workItemsWaiting.WaitOne();
                }
            }
        }

        public override SynchronizationContext CreateCopy() => this;
    }
}