public enum NotificationStatus
{
    Pending,
    Processing,
    Succeeded,
    Failed
}

public sealed record NotificationStatusEntry(
    Guid NotificationId,
    NotificationType Channel,
    NotificationStatus Status,
    string? Error
);