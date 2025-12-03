namespace BaseLibrary;

public class UIThreadServices : IUIThreadServices
{
    //private Thread mainThread = Thread.CurrentThread;
    private SynchronizationContext uiContext;

    //public bool IsOnUIThread => Thread.CurrentThread == mainThread;
    public bool IsOnUIThread => uiContext == SynchronizationContext.Current;

    public UIThreadServices()
    {
        uiContext = SynchronizationContext.Current ?? throw new InvalidOperationException("UI thread context is not available.");
    }

    public void RunOnUIThread(Action action)
    {
        if (IsOnUIThread)
        {
            action();
        }
        else
        {
            uiContext?.Post(_ => action(), null);
        }
    }
    public T RunOnUIThread<T>(Func<T> func)
    {
        if (IsOnUIThread)
        {
            return func();
        }
        else
        {
            T result = default;
            var waitHandle = new ManualResetEvent(false);

            uiContext.Post(_ =>
            {
                result = func();
                waitHandle.Set();
            }, null);

            waitHandle.WaitOne(5000);
            return result;
        }
    }
    public async Task RunOnUIThreadAsync(Action action)
    {
        if (IsOnUIThread)
        {
            action();
        }
        else
        {
            var tcs = new TaskCompletionSource<bool>();

            uiContext.Post(_ =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);

            await tcs.Task;
        }
    }

    public async Task<T> RunOnUIThreadAsync<T>(Func<T> func)
    {
        if (IsOnUIThread)
        {
            return func();
        }
        else
        {
            var tcs = new TaskCompletionSource<T>();

            uiContext.Post(_ =>
            {
                try
                {
                    T result = func();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);

            return await tcs.Task;
        }
    }
}
