public class FailOnceEmailNotificationChannel : INotificationChannel
{
    private int _callCount;

    private readonly INotificationChannel _email =
        new EmailNotificationChannel();

    public Task SendAsync(Notification notification)
    {
        int currentCall = Interlocked.Increment(ref _callCount);

        if (currentCall == 1)
        {
            throw new TimeoutException(
                "Simulated email provider timeout.");
        }

        return _email.SendAsync(notification);
    }
}