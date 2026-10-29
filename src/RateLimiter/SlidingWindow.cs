public class SlidingWindow
{
    
    //Window length in milliseconds.    
    private readonly int WindowLengthMs;
    //Stores timestamps of accepted requests in FIFO order.
    private readonly Queue<long> _requests = new();

    //Maximum number of requests allowed within the window length.
    public int Limit {get;}

    private readonly object _lock = new();

    public SlidingWindow(int limit, int windowLengthMs)
    {
        WindowLengthMs  = windowLengthMs;
        Limit = limit;
    }

    public bool isAllowed(long timestamp)
    {
        lock (_lock)
        {
               long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

               long windowStart = now - WindowLengthMs;

             // Remove expired requests
             while (_requests.Count > 0 &&
               _requests.Peek() <= windowStart)
               {
                   _requests.Dequeue();
               }

             // Check limit
             if (_requests.Count >= Limit)
                return false;

            // Allow request
             _requests.Enqueue(now);

            return true;
        }
    }
}