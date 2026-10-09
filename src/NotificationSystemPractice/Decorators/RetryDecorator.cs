public class RetryDecorator : NotificationDecorator
{
    private readonly int _maxAttempts;
    private readonly int _delayMilliseconds;

    public RetryDecorator(
        INotificationChannel wrapped,
        int maxAttempts = 3,
        int delayMilliseconds = 500)
        : base(wrapped)
    {
        if (maxAttempts <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxAttempts));

        if (delayMilliseconds < 0)
            throw new ArgumentOutOfRangeException(nameof(delayMilliseconds));

        _maxAttempts = maxAttempts;
        _delayMilliseconds = delayMilliseconds;
    }

    public override async Task SendAsync(Notification notification)
    {
        for (int attempt = 1; attempt <= _maxAttempts; attempt++)
        {
            try
            {
                await Wrapped.SendAsync(notification);
                return;
            }
            catch (TimeoutException ex) when (attempt < _maxAttempts)
            {
                Console.WriteLine(
                    $"[Retry] Attempt {attempt} failed: {ex.Message}");

                await Task.Delay(_delayMilliseconds);
            }
        }
    }
}