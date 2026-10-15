
using Splitwise.Models;

namespace Splitwise.Strategies;

public class PercentageSplit : ISplitStrategy
{
    private readonly Dictionary<string, decimal> _percentages;

    public PercentageSplit(Dictionary<string, decimal> percentages)
    {
        _percentages = new Dictionary<string, decimal>(percentages);
    }

    public List<Split> GenerateSplits(
        string expenseId,
        User paidBy,
        long amountPaise,
        IReadOnlyList<User> participants)
    {
        if (amountPaise <= 0 || participants.Count == 0)
            throw new ArgumentException("Invalid expense.");

        if (_percentages.Values.Any(p => p < 0 || p > 100))
            throw new ArgumentException("Invalid percentage.");

        decimal total = participants.Sum(
            u => u.Id is not null && _percentages.TryGetValue(u.Id, out decimal p) ? p : 0m);

        if (total != 100m)
            throw new ArgumentException("Percentages must total 100.");

        List<Split> splits = new();

        var users = participants
            .Where(u => u.Id is not null && _percentages.TryGetValue(u.Id, out decimal p) && p > 0)
            .ToList();

        long allocated = 0;

        for (int i = 0; i < users.Count; i++)
        {
            User user = users[i];
            decimal percentage = _percentages[user.Id!];

            long userShare = i == users.Count - 1
                ? amountPaise - allocated
                : (long)decimal.Floor(amountPaise * percentage / 100m);

            allocated += userShare;

            if (user.Id == paidBy.Id)
                continue;

            splits.Add(new Split(
                paidBy, user, userShare, expenseId));
        }

        return splits;
    }
}
