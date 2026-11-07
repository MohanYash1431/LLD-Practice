
namespace InterviewQuestions.TaskScheduler;

public class TaskExecutor
{
    private readonly TaskScheduler _scheduler;
    private readonly CancellationTokenSource _cts = new();
    private readonly Thread _dispatcher;

    private readonly object _workerLock = new();
    private int _activeWorkers = 0;

    public TaskExecutor(TaskScheduler scheduler)
    {
        _scheduler = scheduler;

        _dispatcher = new Thread(Run)
        {
            IsBackground = true
        };
    }

    // Start the dispatcher
    public void Start()
    {
        _dispatcher.Start();
    }

    // Continuously fetch due tasks
    private void Run()
    {
        while (!_cts.IsCancellationRequested)
        {
            var task = _scheduler.WaitForDueTask(_cts.Token);

            if (task == null)
                break;

            // Track workers for graceful shutdown
            lock (_workerLock)
            {
                _activeWorkers++;
            }

            // Submit task to ThreadPool
            bool queued = ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    ExecuteTask(task);
                }
                finally
                {
                    lock (_workerLock)
                    {
                        _activeWorkers--;
                        Monitor.PulseAll(_workerLock);
                    }
                }
            });

            if (!queued)
            {
                lock (_workerLock)
                {
                    _activeWorkers--;
                    Monitor.PulseAll(_workerLock);
                }

                throw new InvalidOperationException(
                    "Unable to queue task to ThreadPool.");
            }
        }
    }

    // Execute the actual job
    private void ExecuteTask(ScheduledTask task)
    {
        // Prevent execution if already cancelled
        if (!task.TryStart())
            return;

        try
        {
            // Schedule next recurring occurrence
            // Only original executions create recurrence
            if (task.IntervalMs > 0 &&
                task.RetryCount == 0 &&
                !_cts.IsCancellationRequested)
            {
                _scheduler.AddTask(new ScheduledTask
                {
                    Job = task.Job,
                    ExecuteAt = Now() + task.IntervalMs,
                    IntervalMs = task.IntervalMs,
                    Priority = task.Priority,
                    MaxRetries = task.MaxRetries,
                    RetryDelayMs = task.RetryDelayMs
                });
            }

            // Execute task
            task.Job.Execute();

            task.MarkCompleted();

            Console.WriteLine(
                $"Task {task.Id} completed successfully.");
        }
        catch (Exception ex)
        {
            task.MarkFailed();

            Console.WriteLine(
                $"Task {task.Id} failed: {ex.Message}");

            // Retry failed task
            if (task.RetryCount < task.MaxRetries &&
                !_cts.IsCancellationRequested)
            {
                _scheduler.AddTask(new ScheduledTask
                {
                    Job = task.Job,
                    ExecuteAt = Now() + task.RetryDelayMs,
                    RetryCount = task.RetryCount + 1,
                    MaxRetries = task.MaxRetries,
                    RetryDelayMs = task.RetryDelayMs,
                    Priority = task.Priority,
                    IntervalMs = 0
                });

                Console.WriteLine(
                    $"Retry {task.RetryCount + 1} scheduled.");
            }
        }
    }

    private static long Now()
    {
        return DateTimeOffset.UtcNow
            .ToUnixTimeMilliseconds();
    }

    // Gracefully stop the executor
    public void Stop()
    {
        // Stop dispatching new tasks
        _cts.Cancel();

        // Wait for dispatcher to exit
        _dispatcher.Join();

        // Wait for all submitted workers
        lock (_workerLock)
        {
            while (_activeWorkers > 0)
            {
                Monitor.Wait(_workerLock);
            }
        }

        Console.WriteLine(
            "Task Scheduler stopped gracefully.");
    }
}
