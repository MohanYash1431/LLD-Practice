public class AlwaysFailEmailNotificationChannel : INotificationChannel
{
    public Task SendAsync(Notification notification)
    {
        Console.WriteLine("[Email simulation] Attempting send...");

        throw new TimeoutException(
            "Simulated email provider timeout.");
    }
}