public class SmsNotificationChannel : INotificationChannel
{
    public Task SendAsync(Notification notification)
    {
        string? phoneNumber = notification.Recipient.PhoneNumber;

        if (string.IsNullOrEmpty(phoneNumber))
        {
            throw new ArgumentException("Recipient phone number is required.");
        }

        Console.WriteLine($"SMS sent to {phoneNumber} with message: {notification.Message}");

        return Task.CompletedTask;
    }
}