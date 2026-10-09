using System.Diagnostics;

public class RateLimitDecorator : NotificationDecorator
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly TimeSpan _minimumInterval;

    private TimeSpan _nextAllowedAt = TimeSpan.Zero;

    public RateLimitDecorator(
        INotificationChannel wrapped,
        TimeSpan minimumInterval)
        : base(wrapped)
    {
        if (minimumInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minimumInterval));
        }

        _minimumInterval = minimumInterval;
    }

    public override async Task SendAsync(Notification notification)
    {
        await _gate.WaitAsync();

        try
        {
            TimeSpan remaining = _nextAllowedAt - _clock.Elapsed;

            while (remaining > TimeSpan.Zero)
            {
                await Task.Delay(remaining);

                remaining = _nextAllowedAt - _clock.Elapsed;
            }

            TimeSpan grantedAt = _clock.Elapsed;

            _nextAllowedAt = grantedAt + _minimumInterval;

            Console.WriteLine(
                $"[Rate limit] Permit granted at " +
                $"{grantedAt.TotalMilliseconds:F0} ms");
        }
        finally
        {
            _gate.Release();
        }

        await Wrapped.SendAsync(notification);
    }
}