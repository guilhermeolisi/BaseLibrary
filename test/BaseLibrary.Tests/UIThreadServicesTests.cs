using FluentAssertions;

namespace BaseLibrary.Tests;

public class UIThreadServicesTests
{
    // ─── helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// A SynchronizationContext that executes Post callbacks immediately on the
    /// calling thread, making it easy to exercise the "not on UI thread" path
    /// without spinning up a real UI dispatcher.
    /// </summary>
    private sealed class ImmediateSynchronizationContext : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object? state) => d(state);
        public override void Send(SendOrPostCallback d, object? state) => d(state);
    }

    /// <summary>
    /// Sets up a UIThreadServices with an ImmediateSynchronizationContext and
    /// then replaces the current context with a different one so that
    /// IsOnUIThread returns false (simulating a background thread).
    /// Returns the service and captures cleanup via an IDisposable Action.
    /// </summary>
    private static (UIThreadServices service, Action cleanup) CreateServiceNotOnUIThread()
    {
        var ctx = new ImmediateSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(ctx);
        var service = new UIThreadServices();

        // Switch away from the UI context so IsOnUIThread == false
        SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

        return (service, () => SynchronizationContext.SetSynchronizationContext(ctx));
    }

    // ─── void RunOnUIThread ──────────────────────────────────────────────────────

    [Fact]
    public void RunOnUIThread_WhenNotOnUIThread_ExecutesAction()
    {
        var (service, cleanup) = CreateServiceNotOnUIThread();
        try
        {
            var executed = false;
            service.RunOnUIThread(() => executed = true);
            executed.Should().BeTrue();
        }
        finally { cleanup(); }
    }

    [Fact]
    public void RunOnUIThread_WhenNotOnUIThread_PropagatesException()
    {
        var (service, cleanup) = CreateServiceNotOnUIThread();
        try
        {
            var act = () => service.RunOnUIThread(
                () => throw new InvalidOperationException("test exception"));

            act.Should().Throw<InvalidOperationException>()
               .WithMessage("test exception");
        }
        finally { cleanup(); }
    }

    [Fact]
    public void RunOnUIThread_WhenOnUIThread_ExecutesActionDirectly()
    {
        var ctx = new ImmediateSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(ctx);
        try
        {
            var service = new UIThreadServices();
            var executed = false;
            service.RunOnUIThread(() => executed = true);
            executed.Should().BeTrue();
        }
        finally { SynchronizationContext.SetSynchronizationContext(null); }
    }

    [Fact]
    public void RunOnUIThread_WhenOnUIThread_PropagatesException()
    {
        var ctx = new ImmediateSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(ctx);
        try
        {
            var service = new UIThreadServices();
            var act = () => service.RunOnUIThread(
                () => throw new InvalidOperationException("ui thread exception"));

            act.Should().Throw<InvalidOperationException>()
               .WithMessage("ui thread exception");
        }
        finally { SynchronizationContext.SetSynchronizationContext(null); }
    }

    // ─── RunOnUIThread<T> ────────────────────────────────────────────────────────

    [Fact]
    public void RunOnUIThreadT_WhenNotOnUIThread_ReturnsValue()
    {
        var (service, cleanup) = CreateServiceNotOnUIThread();
        try
        {
            var result = service.RunOnUIThread(() => 42);
            result.Should().Be(42);
        }
        finally { cleanup(); }
    }

    [Fact]
    public void RunOnUIThreadT_WhenNotOnUIThread_PropagatesException()
    {
        var (service, cleanup) = CreateServiceNotOnUIThread();
        try
        {
            Func<int> throwing = () => throw new InvalidOperationException("func exception");
            var act = () => service.RunOnUIThread(throwing);

            act.Should().Throw<InvalidOperationException>()
               .WithMessage("func exception");
        }
        finally { cleanup(); }
    }

    [Fact]
    public void RunOnUIThreadT_WhenOnUIThread_ReturnsValue()
    {
        var ctx = new ImmediateSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(ctx);
        try
        {
            var service = new UIThreadServices();
            var result = service.RunOnUIThread(() => 99);
            result.Should().Be(99);
        }
        finally { SynchronizationContext.SetSynchronizationContext(null); }
    }

    [Fact]
    public void RunOnUIThreadT_WhenOnUIThread_PropagatesException()
    {
        var ctx = new ImmediateSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(ctx);
        try
        {
            var service = new UIThreadServices();
            Func<int> throwing = () => throw new InvalidOperationException("ui func exception");
            var act = () => service.RunOnUIThread(throwing);

            act.Should().Throw<InvalidOperationException>()
               .WithMessage("ui func exception");
        }
        finally { SynchronizationContext.SetSynchronizationContext(null); }
    }

    // ─── constructor guard ───────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithoutSynchronizationContext_ThrowsInvalidOperationException()
    {
        SynchronizationContext.SetSynchronizationContext(null);
        var act = () => new UIThreadServices();
        act.Should().Throw<InvalidOperationException>();
    }
}
