namespace RateLimiterPractice;

public class SlidingWindowRateLimitingStrategy
    : IRateLimitingStrategy
{
    private readonly Dictionary<string, SlidingWindow> _windows = new();
    private readonly object _lock = new();

    private readonly int _limit;
    private readonly long _windowLengthMs;

    public SlidingWindowRateLimitingStrategy(
        int limit, long windowLengthMs)
    {
        if (limit <= 0 || windowLengthMs <= 0)
            throw new ArgumentOutOfRangeException(nameof(limit));

        _limit = limit;
        _windowLengthMs = windowLengthMs;
    }

    public bool IsAllowed(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);

        SlidingWindow window;

        lock (_lock)
        {
            if (!_windows.TryGetValue(userId, out var existing))
            {
                existing = new SlidingWindow(_limit, checked((int)_windowLengthMs));
                _windows[userId] = existing;
            }

            window = existing;
        }

        return window.isAllowed(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }
}