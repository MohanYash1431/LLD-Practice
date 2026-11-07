public interface IJob
{
    void Execute();
}

public class EmailJob : IJob
{
    public void Execute()
    {
        Console.WriteLine("Sending Email......");
    }
}

public class NotificationJob : IJob
{
    public void Execute()
    {
        Console.WriteLine("Sending Notification......");
    }
}