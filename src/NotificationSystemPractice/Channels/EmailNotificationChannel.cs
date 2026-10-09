public class EmailNotificationChannel : INotificationChannel
{
    public Task SendAsync(Notification notification)
    {
        string? email = notification.Recipient.Email;

        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentException("Recipient email is required.");
        }

        Console.WriteLine($"Email sent to {email} with message: {notification.Message}");

        return Task.CompletedTask;
    }
}

