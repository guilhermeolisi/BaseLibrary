using System.Collections.Concurrent;

namespace BaseLibrary;

public class PoolTasks : IPoolTasks
{
    //Faz uma fila de tarefas e executa uma por uma
    //Se uma tarefa for adicionada enquanto outra estiver sendo executada, a nova tarefa será executada em seguida

    //public void EnqueueTask(Task task)
    //{
    //    tasks.Enqueue(task);
    //    if (processTask is null || (processTask.Status != TaskStatus.Running && processTask.Status != TaskStatus.Created) /*processTask.IsCompleted*/)
    //    {
    //        processTask = Task.Run(() =>
    //        {
    //            while (tasks.Count > 0)
    //            {
    //                Task? task;// = tasks.Dequeue();
    //                if (!tasks.TryDequeue(out task))
    //                    continue;
    //                if (task is null)
    //                    continue;

    //                if (task.Status == TaskStatus.Created)
    //                {
    //                    try
    //                    {
    //                        task.Start();
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        throw;
    //                    }
    //                }
    //                try
    //                {
    //                    Task.WaitAll(task);
    //                }
    //                catch (Exception ex)
    //                {
    //                    throw;
    //                }
    //            }
    //        });
    //    }


    //}
    public async void EnqueueTask(Task task)
    {

        tasks.Enqueue(task);
        lock (lookRunning)
        {
            if (isRunning)
                return;
            isRunning = true;
        }
        do
        {
            if (processTask is null || (processTask.Status != TaskStatus.Running && processTask.Status != TaskStatus.Created))
            {
                if (tasks.Count == 0)
                    return;
                if (!tasks.TryDequeue(out processTask))
                    continue;
                if (processTask is null)
                    continue;
                if (processTask.Status == TaskStatus.Created)
                {
                    try
                    {
                        processTask.Start();
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
                try
                {
                    await processTask;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        } while (tasks.Count > 0);

        lock (lookRunning)
        {
            isRunning = false;
            processTask = null;
        }
    }
    bool isRunning = false;
    object lookRunning = new();

    Task? processTask;

    ConcurrentQueue<Task> tasks = new();
    public async Task AwaitATask(Task task)
    {
        if (task is null)
            return;
        while (task.Status == TaskStatus.Created)
        {
            await Task.Delay(20);
        }
        if (!task.IsCompleted)
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                throw;
            }
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
