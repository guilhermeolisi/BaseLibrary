

namespace BaseLibrary;

public interface IUIThreadServices
{
    bool IsOnUIThread { get; }

    void RunOnUIThread(Action action);
    T RunOnUIThread<T>(Func<T> func);
    Task RunOnUIThreadAsync(Action action);
    Task<T> RunOnUIThreadAsync<T>(Func<T> func);
}