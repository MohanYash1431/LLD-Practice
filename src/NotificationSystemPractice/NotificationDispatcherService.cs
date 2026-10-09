using System.Threading.Channels;

public class NotificationDispatcherService
{
    private readonly NotificationFactory _factory;
    private readonly Channel<NotificationWorkItem> _queue;
    private readonly NotificationStatusStore _statusStore;
    private readonly Task[] _workers;

    // One queued job contains the notification and its selected channels.
    private sealed record NotificationWorkItem(
        Notification Notification,
        NotificationType[] EnabledChannels
    );

 public NotificationDispatcherService(
    NotificationFactory factory,
    NotificationStatusStore statusStore,
    int workerCount = 5,
    int capacity = 100)
    {
        if (workerCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(workerCount));
        }

        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        _factory = factory;
        _statusStore = statusStore;

        _queue = Channel.CreateBounded<NotificationWorkItem>(
            new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            });

        _workers = new Task[workerCount];

        for (int i = 0; i < workerCount; i++)
        {
            int workerId = i + 1;

            _workers[i] = Task.Run(() => ProcessAsync(workerId));
        }
    }

  public async Task SubmitAsync(
    Notification notification,
    NotificationType[] enabledChannels)
{
    var workItem = new NotificationWorkItem(
        notification,
        enabledChannels.ToArray()
    );

    foreach (NotificationType type in workItem.EnabledChannels)
    {
        _statusStore.SetStatus(
            notification.Id,
            type,
            NotificationStatus.Pending);
    }

    try
    {
        await _queue.Writer.WriteAsync(workItem);
    }
    catch (Exception ex)
    {
        foreach (NotificationType type in workItem.EnabledChannels)
        {
            _statusStore.SetStatus(
                notification.Id,
                type,
                NotificationStatus.Failed,
                $"Submission failed: {ex.Message}");
        }

        throw;
    }
}

  private async Task ProcessAsync(int workerId)
{
    await foreach (var workItem in _queue.Reader.ReadAllAsync())
    {
        Notification notification = workItem.Notification;

        Console.WriteLine(
            $"Worker {workerId} processing notification " +
            $"{notification.Id}");

        foreach (NotificationType type in workItem.EnabledChannels)
        {
            _statusStore.SetStatus(
                notification.Id,
                type,
                NotificationStatus.Processing);

            try
            {
                INotificationChannel channel =
                    _factory.GetChannel(type);

                await channel.SendAsync(notification);

                _statusStore.SetStatus(
                    notification.Id,
                    type,
                    NotificationStatus.Succeeded);
            }
            catch (Exception ex)
            {
                _statusStore.SetStatus(
                    notification.Id,
                    type,
                    NotificationStatus.Failed,
                    ex.Message);

                Console.WriteLine(
                    $"Notification {notification.Id}: " +
                    $"{type} failed: {ex.Message}");
            }
        }
    }
}

    public async Task CompleteAsync()
    {
        // Stop accepting new work.
        _queue.Writer.TryComplete();

        // Wait until workers finish processing the queued work.
        await Task.WhenAll(_workers);
    }
}