namespace BaseLibrary;

public interface IPoolTasks
{
    void EnqueueTask(Task task);
    Task AwaitATask(Task task);
    Task AwaitAllTasks();
    void WaitAllTasks();
}