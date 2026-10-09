using System.Collections.Concurrent;

public class UserPreferenceService
{
    private readonly ConcurrentDictionary<string, NotificationType[]>
        _preferences = new();

    public void SetPreferences(
        string userId,
        params NotificationType[] enabledChannels)
    {
        _preferences[userId] = enabledChannels.Distinct().ToArray();
    }

    public NotificationType[] GetPreferences(string userId)
    {
        if (_preferences.TryGetValue(userId, out var channels))
        {
            return channels.ToArray();
        }

        return Array.Empty<NotificationType>();
    }
}