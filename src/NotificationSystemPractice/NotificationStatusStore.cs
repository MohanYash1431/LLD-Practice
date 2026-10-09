using System.Collections.Concurrent;

public class NotificationStatusStore
{
    private readonly ConcurrentDictionary<
        (Guid NotificationId, NotificationType Channel),
        NotificationStatusEntry> _statuses = new();

    public void SetStatus(
        Guid notificationId,
        NotificationType channel,
        NotificationStatus status,
        string? error = null)
    {
        _statuses[(notificationId, channel)] =
            new NotificationStatusEntry(
                notificationId,
                channel,
                status,
                error);
    }

    public NotificationStatusEntry[] GetAll()
    {
        return _statuses.Values
            .OrderBy(entry => entry.NotificationId)
            .ThenBy(entry => entry.Channel)
            .ToArray();
    }
}