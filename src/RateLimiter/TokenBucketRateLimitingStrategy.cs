namespace RateLimiterPractice;

public class TokenBucketRateLimitingStrategy
    : IRateLimitingStrategy
{
    private readonly Dictionary<string, TokenBucket> _buckets = new();
    private readonly object _lock = new();

    private readonly int _capacity;
    private readonly int _refillRate;

    public TokenBucketRateLimitingStrategy(int capacity, int refillRate)
    {
        _capacity = capacity;
        _refillRate = refillRate;
    }

    public bool IsAllowed(string userId)
    {
        TokenBucket bucket;

        lock (_lock)
        {
            if (!_buckets.TryGetValue(userId, out var existing))
            {
                existing = new TokenBucket(_capacity, _refillRate);
                _buckets[userId] = existing;
            }

            bucket = existing;
        }

        return bucket.IsAllowed();
    }
}