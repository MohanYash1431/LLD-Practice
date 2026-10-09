public class FormattingDecorator : NotificationDecorator
{
    private readonly Func<string, string> _formatter;

    public FormattingDecorator(
        INotificationChannel wrapped,
        Func<string, string> formatter)
        : base(wrapped)
    {
        _formatter = formatter
            ?? throw new ArgumentNullException(nameof(formatter));
    }

    public override Task SendAsync(Notification notification)
    {
        var formattedNotification = notification with
        {
            Message = _formatter(notification.Message)
        };

        return Wrapped.SendAsync(formattedNotification);
    }
}