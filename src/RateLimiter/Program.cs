using RateLimiterPractice;

IRateLimitingStrategy limiter =
    new TokenBucketRateLimitingStrategy(2, 1);

// Test 1: Exhaust tokens
Console.WriteLine(limiter.IsAllowed("user101"));
Console.WriteLine(limiter.IsAllowed("user101"));
Console.WriteLine(limiter.IsAllowed("user101"));

// Test 2: Different user
Console.WriteLine(limiter.IsAllowed("user102"));

// Test 3: Refill after waiting
Thread.Sleep(1200);

Console.WriteLine(limiter.IsAllowed("user101"));
Console.WriteLine(limiter.IsAllowed("user101"));