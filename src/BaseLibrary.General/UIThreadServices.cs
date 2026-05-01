using System.Runtime.ExceptionServices;

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
            Exception? caughtException = null;
            var waitHandle = new ManualResetEvent(false);

            uiContext.Post(_ =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
                finally
                {
                    waitHandle.Set();
                }
            }, null);

            if (!waitHandle.WaitOne(5000))
                throw new TimeoutException("RunOnUIThread timed out waiting for the UI thread.");

            if (caughtException is not null)
                ExceptionDispatchInfo.Capture(caughtException).Throw();
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
            T result = default!;
            Exception? caughtException = null;
            var waitHandle = new ManualResetEvent(false);

            uiContext.Post(_ =>
            {
                try
                {
                    result = func();
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                }
                finally
                {
                    waitHandle.Set();
                }
            }, null);

            if (!waitHandle.WaitOne(5000))
                throw new TimeoutException("RunOnUIThread<T> timed out waiting for the UI thread.");

            if (caughtException is not null)
                ExceptionDispatchInfo.Capture(caughtException).Throw();

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
