using System.Threading.Tasks;

namespace BaseLibrary
{
    public interface IPoolTasks
    {
        Task AwaitAllTasks();
        Task AwaitATask(Task task);
        Task StackTask(Task task);
        void WaitAllTasks();
    }
}