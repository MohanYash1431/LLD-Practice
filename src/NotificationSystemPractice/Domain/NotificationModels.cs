public enum NotificationType
{
    Email,
    Sms,
    Push
}   

public sealed record NotificationRecipient(
    string? Email, 
    string? PhoneNumber,
    string? DeviceToken
);

public sealed record Notification(
    Guid Id,
    string UserId,
    string Message,
    NotificationRecipient Recipient
);

