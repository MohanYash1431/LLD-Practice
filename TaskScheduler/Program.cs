
using InterviewQuestions.TaskScheduler;
using Scheduler = InterviewQuestions.TaskScheduler.TaskScheduler;

var scheduler = new Scheduler();
var executor = new TaskExecutor(scheduler);

long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

// Test 1: Recurring email every 2 seconds
var emailTask = new ScheduledTask
{
    Job = new EmailJob(),
    ExecuteAt = now + 1000,
    IntervalMs = 2000,
    Priority = TaskPriority.High
};

// Test 2: Failed task with retry
var retryTask = new ScheduledTask
{
    Job = new FlakyJob(),
    ExecuteAt = now + 1500,
    MaxRetries = 2,
    RetryDelayMs = 1000
};

// Test 3: Cancelled task
var notificationTask = new ScheduledTask
{
    Job = new NotificationJob(),
    ExecuteAt = now + 4000
};

// Add tasks
scheduler.AddTask(emailTask);
scheduler.AddTask(retryTask);
scheduler.AddTask(notificationTask);

// Cancel notification
Console.WriteLine(
    $"Notification cancelled: {scheduler.CancelTask(notificationTask.Id)}");

// Start execution
executor.Start();

Console.WriteLine("Task Scheduler Started...");

// Keep application alive for 7 seconds
Thread.Sleep(7000);

// Stop gracefully
executor.Stop();

// Print final statuses
Console.WriteLine($"Email status: {scheduler.GetStatus(emailTask.Id)}");
Console.WriteLine($"Original retry task status: {scheduler.GetStatus(retryTask.Id)}");
Console.WriteLine($"Notification status: {scheduler.GetStatus(notificationTask.Id)}");

// Job which fails once
public class FlakyJob : IJob
{
    private int _attempts = 0;

    public void Execute()
    {
        int attempt = Interlocked.Increment(ref _attempts);

        Console.WriteLine($"Attempt: {attempt}");

        if (attempt == 1)
            throw new Exception("Simulated failure");

        Console.WriteLine("Retry successful!");
    }
}
