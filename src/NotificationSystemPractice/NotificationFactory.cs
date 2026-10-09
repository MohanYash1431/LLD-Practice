public class NotificationFactory
{
    private readonly Dictionary<NotificationType, INotificationChannel> _registry;

    public NotificationFactory(Dictionary<NotificationType, INotificationChannel> registry)
    {
        _registry = new Dictionary<NotificationType, INotificationChannel>(registry);
    }

    public INotificationChannel GetChannel(NotificationType type)   
    {
        if(_registry.TryGetValue(type, out var channel))
        {
            return channel;
        }
        throw new ArgumentException($"Notification channel for type {type} is not registered.");        
        
    }
}