namespace BaseLibrary;

public class TaskQueueGPT : ITaskQueue
{
    private readonly object _lock = new();
    private Task _tail = Task.CompletedTask;

    // 1) Ação síncrona, sem retorno
    public Task Enqueue(Action work)
    {
        if (work is null) throw new ArgumentNullException(nameof(work));

        return Enqueue(async () =>
        {
            work();
            await Task.CompletedTask;
        });
    }

    // 2) Função assíncrona, sem retorno
    public Task Enqueue(Func<Task> work)
    {
        if (work is null) throw new ArgumentNullException(nameof(work));

        var tcs = new TaskCompletionSource<object?>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        lock (_lock)
        {
            _tail = _tail
                .ContinueWith(async _ =>
                {
                    try
                    {
                        await work().ConfigureAwait(false);
                        tcs.TrySetResult(null);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default)
                .Unwrap();
        }

        return tcs.Task;
    }

    // 3) Função assíncrona com retorno
    public Task<T> Enqueue<T>(Func<Task<T>> work)
    {
        if (work is null) throw new ArgumentNullException(nameof(work));

        var tcs = new TaskCompletionSource<T>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        lock (_lock)
        {
            _tail = _tail
                .ContinueWith(async _ =>
                {
                    try
                    {
                        var result = await work().ConfigureAwait(false);
                        tcs.TrySetResult(result);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default)
                .Unwrap();
        }

        return tcs.Task;
    }

    // 4) Função síncrona com retorno (o overload que você pediu)
    public Task<T> Enqueue<T>(Func<T> work)
    {
        if (work is null) throw new ArgumentNullException(nameof(work));

        // Aqui é o truque: convertemos para Func<Task<T>>
        return Enqueue<T>(() => Task.FromResult(work()));
    }

    // Aguarda tudo que foi enfileirado até o momento
    public Task WaitAllAsync()
    {
        lock (_lock)
        {
            return _tail;
        }
    }

    public void WaitAll()
    {
        WaitAllAsync().GetAwaiter().GetResult();
    }
}
