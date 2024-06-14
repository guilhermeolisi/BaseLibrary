namespace BaseLibrary;

public class UIThreadServices : IUIThreadServices
{
    private Thread mainThread = Thread.CurrentThread;
    SynchronizationContext? UIContext = SynchronizationContext.Current;

    public bool IsOnUIThread => Thread.CurrentThread == mainThread;

    public void RunOnUIThread(Action action)
    {
        if (IsOnUIThread)
        {
            action();
        }
        else
        {
            UIContext?.Post(_ => action(), null);
        }
    }
    public async Task RunOnUIThreadAsync(Action action)
    {
        if (IsOnUIThread)
        {
            Task temp = new Task(action);
            temp.Start();
            await temp;
        }
        else
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            UIContext?.Post(_ =>
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
}
