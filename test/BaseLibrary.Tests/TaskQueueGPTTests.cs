using FluentAssertions;

namespace BaseLibrary.Tests;

public class TaskQueueGPTTests
{
    // ─── helpers ───────────────────────────────────────────────────────────────

    private static TaskQueueGPT CreateQueue() => new();

    // ─── sequential ordering ────────────────────────────────────────────────────

    [Fact]
    public async Task Enqueue_AsyncFunc_ExecutesInOrder()
    {
        var queue = CreateQueue();
        var results = new List<int>();

        for (int i = 0; i < 5; i++)
        {
            int captured = i;
            await queue.Enqueue(async () =>
            {
                await Task.Delay(10);
                results.Add(captured);
            });
        }

        results.Should().Equal(0, 1, 2, 3, 4);
    }

    [Fact]
    public async Task Enqueue_SyncAction_ExecutesInOrder()
    {
        var queue = CreateQueue();
        var results = new List<int>();

        for (int i = 0; i < 5; i++)
        {
            int captured = i;
            await queue.Enqueue(() => results.Add(captured));
        }

        results.Should().Equal(0, 1, 2, 3, 4);
    }

    [Fact]
    public async Task Enqueue_AsyncFuncWithReturn_ExecutesInOrder()
    {
        var queue = CreateQueue();
        var resultTasks = new List<Task<int>>();

        for (int i = 0; i < 5; i++)
        {
            int captured = i;
            resultTasks.Add(queue.Enqueue(async () =>
            {
                await Task.Delay(5);
                return captured;
            }));
        }

        var results = await Task.WhenAll(resultTasks);
        results.Should().Equal(0, 1, 2, 3, 4);
    }

    [Fact]
    public async Task Enqueue_SyncFuncWithReturn_ExecutesInOrder()
    {
        var queue = CreateQueue();
        var resultTasks = new List<Task<int>>();

        for (int i = 0; i < 5; i++)
        {
            int captured = i;
            resultTasks.Add(queue.Enqueue(() => captured));
        }

        var results = await Task.WhenAll(resultTasks);
        results.Should().Equal(0, 1, 2, 3, 4);
    }

    // ─── exception isolation ────────────────────────────────────────────────────

    [Fact]
    public async Task Enqueue_AsyncFunc_ExceptionDoesNotStopQueue()
    {
        var queue = CreateQueue();
        var executedAfter = false;

        var failingTask = queue.Enqueue(async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("intentional");
        });

        await queue.Enqueue(async () =>
        {
            await Task.Yield();
            executedAfter = true;
        });

        var act = async () => await failingTask;
        await act.Should().ThrowAsync<InvalidOperationException>();
        executedAfter.Should().BeTrue();
    }

    [Fact]
    public async Task Enqueue_SyncAction_ExceptionDoesNotStopQueue()
    {
        var queue = CreateQueue();
        var executedAfter = false;

        var failingTask = queue.Enqueue(() => throw new InvalidOperationException("intentional"));

        await queue.Enqueue(() => executedAfter = true);

        var act = async () => await failingTask;
        await act.Should().ThrowAsync<InvalidOperationException>();
        executedAfter.Should().BeTrue();
    }

    [Fact]
    public async Task Enqueue_SyncFuncWithReturn_ExceptionDoesNotStopQueue()
    {
        var queue = CreateQueue();
        var executedAfter = false;

        Func<int> throwingFunc = () => throw new InvalidOperationException("intentional");
        Task<int> failingTask = queue.Enqueue(throwingFunc);

        await queue.Enqueue(() => executedAfter = true);

        var act = async () => await failingTask;
        await act.Should().ThrowAsync<InvalidOperationException>();
        executedAfter.Should().BeTrue();
    }

    // ─── cancellation propagation ───────────────────────────────────────────────

    [Fact]
    public async Task Enqueue_AsyncFunc_CancellationIsPropagatedAsCanceled()
    {
        var queue = CreateQueue();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        Task cancelledTask = queue.Enqueue(async () =>
        {
            await Task.FromCanceled(cts.Token);
        });

        // Wait for the queue to process the item.
        await queue.WaitAllAsync();

        // The returned task must be Canceled, not Faulted.
        cancelledTask.IsCanceled.Should().BeTrue();
        cancelledTask.IsFaulted.Should().BeFalse();
    }

    [Fact]
    public async Task Enqueue_AsyncFuncWithReturn_CancellationIsPropagatedAsCanceled()
    {
        var queue = CreateQueue();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        Task<int> cancelledTask = queue.Enqueue(async () =>
        {
            await Task.FromCanceled<int>(cts.Token);
            return 0;
        });

        // Wait for the queue to process the item.
        await queue.WaitAllAsync();

        // The returned task must be Canceled, not Faulted.
        cancelledTask.IsCanceled.Should().BeTrue();
        cancelledTask.IsFaulted.Should().BeFalse();
    }

    // ─── WaitAllAsync / WaitAll ──────────────────────────────────────────────────

    [Fact]
    public async Task WaitAllAsync_CompletesAfterAllEnqueuedWork()
    {
        var queue = CreateQueue();
        var results = new List<int>();

        // Fire-and-forget enqueueing (don't await individual tasks).
        _ = queue.Enqueue(async () => { await Task.Delay(10); results.Add(1); });
        _ = queue.Enqueue(async () => { await Task.Delay(10); results.Add(2); });
        _ = queue.Enqueue(async () => { await Task.Delay(10); results.Add(3); });

        await queue.WaitAllAsync();

        results.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task WaitAllAsync_CompletesSuccessfullyEvenWhenItemsFail()
    {
        var queue = CreateQueue();

        _ = queue.Enqueue(async () =>
        {
            await Task.Yield();
            throw new InvalidOperationException("failure");
        });

        // WaitAllAsync should complete without throwing even though an item failed.
        var act = async () => await queue.WaitAllAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void WaitAll_BlocksUntilAllEnqueuedWork()
    {
        var queue = CreateQueue();
        var results = new List<int>();

        _ = queue.Enqueue(async () => { await Task.Delay(10); results.Add(1); });
        _ = queue.Enqueue(async () => { await Task.Delay(10); results.Add(2); });

        queue.WaitAll();

        results.Should().Equal(1, 2);
    }

    // ─── null argument guards ────────────────────────────────────────────────────

    [Fact]
    public void Enqueue_Action_NullThrowsArgumentNullException()
    {
        var queue = CreateQueue();
        Action? nullAction = null;
        // Record.Exception is used because Enqueue returns Task; the null-guard
        // throws synchronously before any Task is created.
        var ex = Record.Exception(() => { _ = queue.Enqueue(nullAction!); });
        Assert.IsType<ArgumentNullException>(ex);
    }

    [Fact]
    public void Enqueue_AsyncFunc_NullThrowsArgumentNullException()
    {
        var queue = CreateQueue();
        Func<Task>? nullFunc = null;
        var ex = Record.Exception(() => { _ = queue.Enqueue(nullFunc!); });
        Assert.IsType<ArgumentNullException>(ex);
    }

    [Fact]
    public void Enqueue_AsyncFuncWithReturn_NullThrowsArgumentNullException()
    {
        var queue = CreateQueue();
        Func<Task<int>>? nullFunc = null;
        var ex = Record.Exception(() => { _ = queue.Enqueue(nullFunc!); });
        Assert.IsType<ArgumentNullException>(ex);
    }

    [Fact]
    public void Enqueue_SyncFuncWithReturn_NullThrowsArgumentNullException()
    {
        var queue = CreateQueue();
        Func<int>? nullFunc = null;
        var ex = Record.Exception(() => { _ = queue.Enqueue(nullFunc!); });
        Assert.IsType<ArgumentNullException>(ex);
    }

    // ─── return values ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Enqueue_AsyncFuncWithReturn_ReturnsCorrectValue()
    {
        var queue = CreateQueue();
        var result = await queue.Enqueue(async () =>
        {
            await Task.Yield();
            return 42;
        });
        result.Should().Be(42);
    }

    [Fact]
    public async Task Enqueue_SyncFuncWithReturn_ReturnsCorrectValue()
    {
        var queue = CreateQueue();
        var result = await queue.Enqueue(() => 42);
        result.Should().Be(42);
    }
}

