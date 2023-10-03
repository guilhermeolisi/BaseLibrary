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
            try
            {
                await processTask;
            }
            catch (Exception ex)
            {
                throw;
            }
        if (tasks.Count == 0)
            return;
        else if (processTask is not null)
        {

        }

        processTask = new(() =>
        {
            while (tasks.Count > 0)
            {
                Task task = tasks.Dequeue();
                if (task is null)
                    continue;

                if (task.Status == TaskStatus.Created)
                {
                    try
                    {
                        task.Start();
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
                if (task.Status == TaskStatus.Running)
                    try
                    {
                        task.Wait();
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
            }
        });

        processTask.Start();
        try
        {
            await processTask;
        }
        catch (Exception ex)
        {
            throw;
        }
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
