namespace BaseLibrary;

public class PoolTasks : IPoolTasks
{
    //Faz uma fila de tarefas e executa uma por uma
    //Se uma tarefa for adicionada enquanto outra estiver sendo executada, a nova tarefa será executada em seguida
    public void EnqueueTask(Task task)
    {
        tasks.Enqueue(task);
        if (processTask is null || (processTask.Status != TaskStatus.Running && processTask.Status != TaskStatus.Created) /*processTask.IsCompleted*/)
        {
            processTask = Task.Run(() =>
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
                    Task.WaitAll(task);
                }
            });
        }
    }



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
        processTask = Task.Run(() =>
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
                Task.WaitAll(task);
                //if (task.Status == TaskStatus.Running)
                //    try
                //    {
                //        task.Wait();
                //    }
                //    catch (Exception ex)
                //    {
                //        throw;
                //    }
            }
        });
        //processTask = new(() =>
        //{
        //    while (tasks.Count > 0)
        //    {
        //        Task task = tasks.Dequeue();
        //        if (task is null)
        //            continue;

        //        if (task.Status == TaskStatus.Created)
        //        {
        //            try
        //            {
        //                task.Start();
        //            }
        //            catch (Exception ex)
        //            {
        //                throw;
        //            }
        //        }
        //        if (task.Status == TaskStatus.Running)
        //            try
        //            {
        //                task.Wait();
        //            }
        //            catch (Exception ex)
        //            {
        //                throw;
        //            }
        //    }
        //});
        //if (processTask.Status == TaskStatus.Running)
        //    processTask.Start();
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
