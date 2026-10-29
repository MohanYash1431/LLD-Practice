public class TokenBucket
{
    public int Capacity { get; }
    public int Tokens { get; private set; }
    public int RefillRate { get; }
    public long LastRefillTime { get; private set; }
    private readonly object _lock = new object();

    public TokenBucket(int capacity, int refillRate)
    {
        Capacity = capacity;
        RefillRate = refillRate;
        Tokens = capacity;
        LastRefillTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
    
     private void Refill()
     {
         long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

         long elapsedSeconds = (now - LastRefillTime) / 1000;

         if (elapsedSeconds <= 0)
         return;

        long tokensToAdd = elapsedSeconds * RefillRate;

          Tokens = (int)Math.Min(Capacity, Tokens + tokensToAdd);

        LastRefillTime += elapsedSeconds * 1000;
    }

    //IsAllowed
    public bool IsAllowed()
    {
        lock (_lock)
        {
            Refill();
            if (Tokens > 0)
            {
                Tokens--;
                return true;
            }
            return false;
        }
    }

}