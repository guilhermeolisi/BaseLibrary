namespace BaseLibrary
{
    public interface IPoolTasks
    {
        void EnqueueTask(Task task);
        Task AwaitATask(Task task);
        Task StackTask(Task task);
        Task AwaitAllTasks();
        void WaitAllTasks();
        //void Enqueue(Task task);
    }
}