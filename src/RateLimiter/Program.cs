using RateLimiterPractice;

// Maximum 2 requests in 2 seconds
IRateLimitingStrategy limiter =
    new SlidingWindowRateLimitingStrategy(2, 2000);

Console.WriteLine("Before waiting:");

Console.WriteLine(limiter.IsAllowed("user101")); // True
Console.WriteLine(limiter.IsAllowed("user101")); // True
Console.WriteLine(limiter.IsAllowed("user101")); // False

// Wait 2.2 seconds
Thread.Sleep(2200);

Console.WriteLine("After waiting:");

Console.WriteLine(limiter.IsAllowed("user101")); // True
Console.WriteLine(limiter.IsAllowed("user101")); // True
Console.WriteLine(limiter.IsAllowed("user101")); // False