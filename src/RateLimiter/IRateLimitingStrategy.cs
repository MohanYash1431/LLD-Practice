public interface IRateLimitingStrategy
{
    bool IsAllowed(string userId);
}