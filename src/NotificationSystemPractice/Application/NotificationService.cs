public class NotificationService
{
    private readonly UserPreferenceService _preferenceService;
    private readonly NotificationDispatcherService _dispatcher;

    public NotificationService(
        UserPreferenceService preferenceService,
        NotificationDispatcherService dispatcher)
    {
        _preferenceService = preferenceService;
        _dispatcher = dispatcher;
    }

    public async Task SubmitAsync(Notification notification)
    {
        NotificationType[] enabledChannels =
            _preferenceService.GetPreferences(notification.UserId);

        if (enabledChannels.Length == 0)
        {
            Console.WriteLine(
                $"No channels enabled for user '{notification.UserId}'.");

            return;
        }

        Console.WriteLine(
            $"User '{notification.UserId}' preferences: " +
            string.Join(", ", enabledChannels));

        await _dispatcher.SubmitAsync(notification, enabledChannels);

        Console.WriteLine("Submission accepted.");
    }
}