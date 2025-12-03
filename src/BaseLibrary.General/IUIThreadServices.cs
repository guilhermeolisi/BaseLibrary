

namespace BaseLibrary;

public interface IUIThreadServices
{
    bool IsOnUIThread { get; }

    /// <summary>
    /// Evitar usar o método síncrono para não bloquear a UI com sync on async deadlocks.
    /// </summary>
    /// <param name="action"></param>
    void RunOnUIThread(Action action);
    /// <summary>
    /// Evitar usar o método síncrono para não bloquear a UI com sync on async deadlocks.
    /// </summary>
    /// <param name="action"></param>
    T RunOnUIThread<T>(Func<T> func);
    Task RunOnUIThreadAsync(Action action);
    Task<T> RunOnUIThreadAsync<T>(Func<T> func);
}