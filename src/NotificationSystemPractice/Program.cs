var recipient = new NotificationRecipient(
    Email: "user@example.com",
    PhoneNumber: "1234567890",
    DeviceToken: "device_token_123"
);

var notification = new Notification(
    Id: Guid.NewGuid(),
    UserId: "user123",
    Message: "This is a test notification.",
    Recipient: recipient
);

Console.WriteLine($"Notification created with ID: {notification.Id}");
Console.WriteLine($"Notification for user: {notification.UserId}");
Console.WriteLine($"Message: {notification.Message}");
Console.WriteLine($"Recipient Email: {notification.Recipient.Email}");
Console.WriteLine($"Recipient Phone Number: {notification.Recipient.PhoneNumber}");
Console.WriteLine($"Recipient Device Token: {notification.Recipient.DeviceToken}"); 

// Build the Email channel one layer at a time.
INotificationChannel emailChannel =
    new AlwaysFailEmailNotificationChannel();

emailChannel = new RateLimitDecorator(
    emailChannel,
    minimumInterval: TimeSpan.FromSeconds(1)
);

emailChannel = new RetryDecorator(
    emailChannel,
    maxAttempts: 3,
    delayMilliseconds: 500
);

emailChannel = new FormattingDecorator(
    emailChannel,
    message => $"[Inspection Service] {message}"
);

// Configure Push with its own formatting.
INotificationChannel pushChannel = new FormattingDecorator(
    new PushNotificationChannel(),
    message => $"[New assignment] {message}"
);

var registry = new Dictionary<NotificationType, INotificationChannel>
{
    { NotificationType.Email, emailChannel },
    { NotificationType.Sms, new SmsNotificationChannel() },
    { NotificationType.Push, pushChannel }
};


var factory = new NotificationFactory(registry);

var preferenceService = new UserPreferenceService();

// user123 enables Email and Push.
preferenceService.SetPreferences(
    "user123",
    NotificationType.Email,
    NotificationType.Push
);

var statusStore = new NotificationStatusStore();

var dispatcher = new NotificationDispatcherService(
    factory,
    statusStore,
    workerCount: 5,
    capacity: 100
);

var notificationService = new NotificationService(
    preferenceService,
    dispatcher
);

try
{
    for (int i = 1; i <= 3; i++)
    {
        var nextNotification = notification with
        {
            Id = Guid.NewGuid(),
            Message = $"Test notification {i}."
        };

        await notificationService.SubmitAsync(nextNotification);
    }

    Console.WriteLine("All three notifications submitted.");
}
finally
{
    await dispatcher.CompleteAsync();
}

var entries = statusStore.GetAll();

Console.WriteLine();
Console.WriteLine($"Final notification statuses ({entries.Length}):");

foreach (var entry in entries)
{
    Console.WriteLine(
        $"{entry.NotificationId} | " +
        $"{entry.Channel} | {entry.Status}");

    if (entry.Error is not null)
    {
        Console.WriteLine($"  Error: {entry.Error}");
    }
}

Console.WriteLine("All queued work finished.");

Console.WriteLine();
Console.WriteLine("Final notification statuses:");

foreach (var entry in statusStore.GetAll())
{
    Console.WriteLine(
        $"{entry.NotificationId} | " +
        $"{entry.Channel} | {entry.Status}");

    if (entry.Error is not null)
    {
        Console.WriteLine($"  Error: {entry.Error}");
    }
}



