using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BaseLibrary;

public class PoolTasks : IPoolTasks
{
    //public Task processTaskImport { get; private set; }
    Task processTask;

    Queue<Task> tasks = new();
    public async Task StackTask(Task task)
    {
        tasks.Enqueue(task);

        if (processTask is not null && !processTask.IsCompleted)
            await processTask;
        if (tasks.Count == 0)
            return;

        processTask = new(() =>
        {
            while (tasks.Count > 0)
            {
                Task task = tasks.Dequeue();
                if (task is null)
                    continue;
                if (task.Status == TaskStatus.Created)
                {
                    task.Start();
                }
                if (task.Status == TaskStatus.Running)
                    task.Wait();
            }
        });
        processTask.Start();
        await processTask;
    }
    public async Task AwaitATask(Task task)
    {
        if (task is null)
            return;
        while (task.Status == TaskStatus.Created)
        {
            await Task.Delay(20);
        }
        if (!task.IsCompleted)
            await task;
    }
    public async Task AwaitAllTasks()
    {
        await AwaitATask(processTask);
    }
    public void WaitAllTasks()
    {
        if (processTask is null)
            return;
        if (!processTask.IsCanceled)
            processTask.Wait();
    }
}
