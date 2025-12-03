namespace BaseLibrary;

public interface ITaskQueue
{
    Task Enqueue(Action work);
    Task Enqueue(Func<Task> work);
    Task<T> Enqueue<T>(Func<Task<T>> work);
    Task<T> Enqueue<T>(Func<T> work);
    Task WaitAllAsync();
    void WaitAll();
}