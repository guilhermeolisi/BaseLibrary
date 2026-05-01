namespace BaseLibrary;

public class TaskQueueGPT : ITaskQueue
{
    private readonly object _lock = new();
    private Task _tail = Task.CompletedTask;

    // 1) Ação síncrona, sem retorno
    public Task Enqueue(Action work)
    {
        if (work is null) throw new ArgumentNullException(nameof(work));

        // Use a synchronous-returning lambda to avoid async state-machine overhead
        // and to preserve the original exception stack trace.
        return Enqueue(() => { work(); return Task.CompletedTask; });
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
                    catch (OperationCanceledException ex)
                    {
                        // Propagate cancellation correctly so callers see IsCanceled == true.
                        tcs.TrySetCanceled(ex.CancellationToken);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                },
                CancellationToken.None,
                // DenyChildAttach prevents tasks created inside work() with
                // AttachedToParent from delaying completion of the chain node.
                TaskContinuationOptions.DenyChildAttach,
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
                    catch (OperationCanceledException ex)
                    {
                        // Propagate cancellation correctly so callers see IsCanceled == true.
                        tcs.TrySetCanceled(ex.CancellationToken);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                },
                CancellationToken.None,
                // DenyChildAttach prevents tasks created inside work() with
                // AttachedToParent from delaying completion of the chain node.
                TaskContinuationOptions.DenyChildAttach,
                TaskScheduler.Default)
                .Unwrap();
        }

        return tcs.Task;
    }

    // 4) Função síncrona com retorno
    public Task<T> Enqueue<T>(Func<T> work)
    {
        if (work is null) throw new ArgumentNullException(nameof(work));

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
        // Use Task.Run to avoid deadlocks when called from a thread bound to a
        // SynchronizationContext (e.g. UI thread) where continuations inside
        // the queue might be scheduled back onto the same context.
        Task.Run(WaitAllAsync).GetAwaiter().GetResult();
    }
}
