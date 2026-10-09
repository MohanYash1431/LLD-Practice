public abstract class NotificationDecorator : INotificationChannel
{
    protected readonly INotificationChannel Wrapped;

    protected NotificationDecorator(INotificationChannel wrapped)
    {
        Wrapped = wrapped
            ?? throw new ArgumentNullException(nameof(wrapped));
    }

    public abstract Task SendAsync(Notification notification);
}