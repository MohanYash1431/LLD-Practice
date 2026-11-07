
namespace InterviewQuestions.TaskScheduler;

public class ScheduledTask : IComparable<ScheduledTask>
{
    private readonly object _statusLock = new();

    private TaskStatus _status = TaskStatus.Scheduled;

    public string Id { get; set; } = Guid.NewGuid().ToString();

    public IJob Job { get; set; } = default!;

    public long ExecuteAt { get; set; }

    public long IntervalMs { get; set; } = 0;

    public int RetryCount { get; set; } = 0;

    public int MaxRetries { get; set; } = 3;

    public long RetryDelayMs { get; set; } = 1000;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    // Thread-safe status property
    public TaskStatus Status
    {
        get
        {
            lock (_statusLock)
            {
                return _status;
            }
        }
    }

    // Scheduled -> Running
    public bool TryStart()
    {
        lock (_statusLock)
        {
            if (_status != TaskStatus.Scheduled)
                return false;

            _status = TaskStatus.Running;
            return true;
        }
    }

    // Scheduled -> Cancelled
    public bool TryCancel()
    {
        lock (_statusLock)
        {
            if (_status != TaskStatus.Scheduled)
                return false;

            _status = TaskStatus.Cancelled;
            return true;
        }
    }

    // Running -> Completed
    public void MarkCompleted()
    {
        lock (_statusLock)
        {
            if (_status == TaskStatus.Running)
                _status = TaskStatus.Completed;
        }
    }

    // Running -> Failed
    public void MarkFailed()
    {
        lock (_statusLock)
        {
            if (_status == TaskStatus.Running)
                _status = TaskStatus.Failed;
        }
    }

    // Priority Queue ordering
    public int CompareTo(ScheduledTask? other)
    {
        if (other == null)
            return 1;

        // Earlier execution time first
        int timeComparison = ExecuteAt.CompareTo(other.ExecuteAt);

        if (timeComparison != 0)
            return timeComparison;

        // Higher priority first for equal times
        return other.Priority.CompareTo(Priority);
    }
}
