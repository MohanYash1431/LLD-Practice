
namespace InterviewQuestions.TaskScheduler;



public class TaskScheduler
{
    private readonly object _lock = new();

    private readonly Dictionary<string, ScheduledTask>
    _taskRegistry = new();

    private readonly PriorityQueue<ScheduledTask, ScheduledTask>
        _tasks = new(
            Comparer<ScheduledTask>.Create(
                (a, b) => a.CompareTo(b)
            )
        );

    // Add a new scheduled task
   
public void AddTask(ScheduledTask task)
{
    ArgumentNullException.ThrowIfNull(task);

    lock (_lock)
    {
        if (!_taskRegistry.TryAdd(task.Id, task))
            throw new InvalidOperationException(
                "Duplicate task ID.");

        _tasks.Enqueue(task, task);
        Monitor.PulseAll(_lock);
    }
}


    // View the next task without removing it
    public ScheduledTask? PeekNextTask()
    {
        lock (_lock)
        {
            return _tasks.TryPeek(out var task, out _)
                ? task
                : null;
        }
    }

    // Remove and return the next task
    public ScheduledTask? GetNextTask()
    {
        lock (_lock)
        {
            return _tasks.TryDequeue(out var task, out _)
                ? task
                : null;
        }
    }

    
public ScheduledTask? WaitForDueTask(CancellationToken token)
{
    using var registration = token.Register(() =>
    {
        lock (_lock)
        {
            Monitor.PulseAll(_lock);
        }
    });

    lock (_lock)
    {
        while (!token.IsCancellationRequested)
        {
            // Wait if queue is empty
            if (!_tasks.TryPeek(out var task, out _))
            {
                Monitor.Wait(_lock);
                continue;
            }

            if (task.Status == TaskStatus.Cancelled)
{
    _tasks.Dequeue();
    continue;
}

            long now = DateTimeOffset.UtcNow
                .ToUnixTimeMilliseconds();

            long remaining = task.ExecuteAt - now;

            // Execute only when due
            if (remaining <= 0)
                return _tasks.Dequeue();

            // Sleep until due, or until signalled
            Monitor.Wait(
                _lock,
                (int)Math.Min(remaining, int.MaxValue)
            );
        }

        return null;
    }
}


public bool CancelTask(string taskId)
{
    lock (_lock)
    {
        if (!_taskRegistry.TryGetValue(taskId, out var task))
            return false;

        bool cancelled = task.TryCancel();

        if (cancelled)
            Monitor.PulseAll(_lock);

        return cancelled;
    }
}

public TaskStatus? GetStatus(string taskId)
{
    lock (_lock)
    {
        return _taskRegistry.TryGetValue(taskId, out var task)
            ? task.Status
            : null;
    }
}


}
