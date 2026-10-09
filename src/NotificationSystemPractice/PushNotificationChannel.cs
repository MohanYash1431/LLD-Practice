public class PushNotificationChannel : INotificationChannel
{
    public Task SendAsync(Notification notification)
    {
        string? deviceToken = notification.Recipient.DeviceToken;

        if (string.IsNullOrEmpty(deviceToken))
        {
            throw new ArgumentException("Recipient device token is required.");
        }

        Console.WriteLine($"Push notification sent to {deviceToken} with message: {notification.Message}");

        return Task.CompletedTask;
    }
}